using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Business.Models;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Data.Entities;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Service for Microsoft Teams integration providing comprehensive messaging and webhook management
    /// </summary>
    public class TeamsIntegrationService : ITeamsIntegrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TeamsIntegrationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // Teams integration configuration
        private readonly string _userAgent;
        private readonly int _maxRetries;
        private readonly TimeSpan _timeout;
        private readonly int _rateLimitPerMinute;

        public TeamsIntegrationService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<TeamsIntegrationService> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;

            // Configure HTTP client and settings
            _userAgent = "ProjectControlsReportingTool-Teams/1.0";
            _maxRetries = _configuration.GetValue<int>("TeamsIntegration:MaxRetries", 3);
            _timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("TeamsIntegration:TimeoutSeconds", 30));
            _rateLimitPerMinute = _configuration.GetValue<int>("TeamsIntegration:RateLimitPerMinute", 30);

            _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
            _httpClient.Timeout = _timeout;
        }

        #region Message Management

        public async Task<TeamsMessageResponseDto> SendMessageAsync(SendTeamsMessageDto messageDto, Guid userId)
        {
            try
            {
                _logger.LogInformation("Sending Teams message from user {UserId} to webhook {WebhookUrl}", 
                    userId, messageDto.WebhookUrl);

                // Validate webhook URL
                var validationResult = await ValidateWebhookUrlAsync(messageDto.WebhookUrl);
                if (!validationResult.Success)
                {
                    throw new ArgumentException(validationResult.ErrorMessage);
                }

                // Check rate limiting
                await CheckRateLimitAsync(userId, messageDto.WebhookUrl);

                // Create Teams message entity
                var teamsMessage = new TeamsMessage
                {
                    Id = Guid.NewGuid(),
                    Title = messageDto.Title ?? "Notification",
                    Message = messageDto.Message,
                    WebhookUrl = messageDto.WebhookUrl,
                    MessageType = messageDto.MessageType.ToString(),
                    ThemeColor = messageDto.ThemeColor ?? GetThemeColorForMessageType(messageDto.MessageType),
                    ActionsJson = messageDto.Actions != null ? JsonSerializer.Serialize(messageDto.Actions) : null,
                    FactsJson = messageDto.Facts != null ? JsonSerializer.Serialize(messageDto.Facts) : null,
                    UseAdaptiveCard = messageDto.UseAdaptiveCard,
                    Status = "Pending",
                    NotificationType = NotificationType.SystemAlert, // Default
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TeamsMessages.Add(teamsMessage);
                await _context.SaveChangesAsync();

                // Send the message
                var response = await SendTeamsMessageAsync(teamsMessage);

                // Update message status
                teamsMessage.Status = response.Success ? "Sent" : "Failed";
                teamsMessage.StatusCode = response.Success ? 200 : 400;
                teamsMessage.ErrorMessage = response.ErrorMessage;
                teamsMessage.SentAt = response.SentAt;
                teamsMessage.MessageId = response.MessageId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Teams message {MessageId} sent with status: {Status}", 
                    teamsMessage.Id, teamsMessage.Status);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Teams message from user {UserId}", userId);
                throw;
            }
        }

        public async Task<BulkTeamsMessageResultDto> SendBulkMessageAsync(BulkTeamsMessageDto bulkMessageDto, Guid userId)
        {
            try
            {
                _logger.LogInformation("Sending bulk Teams message from user {UserId} to {WebhookCount} webhooks", 
                    userId, bulkMessageDto.WebhookUrls.Count);

                var results = new List<TeamsMessageResponseDto>();
                var errors = new List<string>();
                var stopwatch = Stopwatch.StartNew();

                var semaphore = new SemaphoreSlim(bulkMessageDto.MaxConcurrency, bulkMessageDto.MaxConcurrency);
                var tasks = bulkMessageDto.WebhookUrls.Select(async webhookUrl =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var messageDto = new SendTeamsMessageDto
                        {
                            Message = bulkMessageDto.Message,
                            WebhookUrl = webhookUrl,
                            Title = bulkMessageDto.Title,
                            MessageType = bulkMessageDto.MessageType,
                            Actions = bulkMessageDto.Actions,
                            Facts = bulkMessageDto.Facts
                        };

                        var result = await SendMessageAsync(messageDto, userId);
                        lock (results)
                        {
                            results.Add(result);
                        }

                        return result;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Failed to send to {webhookUrl}: {ex.Message}";
                        lock (errors)
                        {
                            errors.Add(errorMessage);
                        }

                        if (!bulkMessageDto.ContinueOnError)
                        {
                            throw;
                        }

                        return new TeamsMessageResponseDto
                        {
                            Success = false,
                            ErrorMessage = ex.Message,
                            WebhookUrl = webhookUrl,
                            SentAt = DateTime.UtcNow
                        };
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
                stopwatch.Stop();

                var successfulDeliveries = results.Count(r => r.Success);
                var failedDeliveries = results.Count - successfulDeliveries;

                _logger.LogInformation("Bulk Teams message completed: {Successful}/{Total} successful deliveries", 
                    successfulDeliveries, bulkMessageDto.WebhookUrls.Count);

                return new BulkTeamsMessageResultDto
                {
                    TotalWebhooks = bulkMessageDto.WebhookUrls.Count,
                    SuccessfulDeliveries = successfulDeliveries,
                    FailedDeliveries = failedDeliveries,
                    SuccessRate = bulkMessageDto.WebhookUrls.Count > 0 
                        ? (double)successfulDeliveries / bulkMessageDto.WebhookUrls.Count * 100 
                        : 0,
                    TotalProcessingTime = stopwatch.Elapsed,
                    Results = results,
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk Teams message from user {UserId}", userId);
                throw;
            }
        }

        public async Task<TeamsMessageResponseDto> SendNotificationAsync(
            NotificationType notificationType,
            string title,
            string message,
            Guid userId,
            Guid? reportId = null,
            Dictionary<string, object>? metadata = null)
        {
            try
            {
                _logger.LogInformation("Sending Teams notification {NotificationType} to user {UserId}", 
                    notificationType, userId);

                // Get user's Teams webhook configurations
                var webhookConfigs = await _context.TeamsWebhookConfigs
                    .Where(w => w.UserId == userId && 
                               w.IsActive && 
                               w.EnabledNotificationsJson.Contains(notificationType.ToString()))
                    .ToListAsync();

                if (!webhookConfigs.Any())
                {
                    _logger.LogWarning("No active Teams webhook configurations found for user {UserId} and notification type {NotificationType}", 
                        userId, notificationType);
                    
                    return new TeamsMessageResponseDto
                    {
                        Success = false,
                        ErrorMessage = "No active Teams webhook configurations found for this notification type",
                        SentAt = DateTime.UtcNow
                    };
                }

                // Use the first webhook configuration
                var config = webhookConfigs.First();

                // Check for custom template
                var template = await _context.TeamsNotificationTemplates
                    .FirstOrDefaultAsync(t => t.NotificationType == notificationType && 
                                             t.UserId == userId && 
                                             t.IsActive);

                var messageDto = new SendTeamsMessageDto
                {
                    WebhookUrl = config.WebhookUrl,
                    MessageType = GetMessageTypeForNotification(notificationType)
                };

                if (template != null)
                {
                    // Render template
                    var templateData = metadata ?? new Dictionary<string, object>();
                    templateData["Title"] = title;
                    templateData["Message"] = message;
                    templateData["UserId"] = userId;
                    templateData["ReportId"] = reportId;
                    templateData["NotificationType"] = notificationType.ToString();

                    messageDto.Title = RenderTemplate(template.TitleTemplate, templateData);
                    messageDto.Message = RenderTemplate(template.MessageTemplate, templateData);
                    messageDto.ThemeColor = template.ThemeColor;
                    messageDto.UseAdaptiveCard = template.UseAdaptiveCard;

                    if (!string.IsNullOrEmpty(template.DefaultActionsJson))
                    {
                        messageDto.Actions = JsonSerializer.Deserialize<List<TeamsCardAction>>(template.DefaultActionsJson);
                    }

                    if (!string.IsNullOrEmpty(template.DefaultFactsJson))
                    {
                        messageDto.Facts = JsonSerializer.Deserialize<Dictionary<string, object>>(template.DefaultFactsJson);
                    }
                }
                else
                {
                    // Use default formatting
                    messageDto.Title = title;
                    messageDto.Message = message;
                    messageDto.Facts = metadata;
                }

                return await SendMessageAsync(messageDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Teams notification {NotificationType} to user {UserId}", 
                    notificationType, userId);
                throw;
            }
        }

        public async Task<PagedResultDto<TeamsMessageResponseDto>> GetMessageHistoryAsync(
            Guid? userId = null,
            string? webhookUrl = null,
            NotificationType? notificationType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.TeamsMessages.AsQueryable();

                // Apply filters
                if (userId.HasValue)
                {
                    query = query.Where(m => m.UserId == userId.Value);
                }

                if (!string.IsNullOrEmpty(webhookUrl))
                {
                    query = query.Where(m => m.WebhookUrl.Contains(webhookUrl));
                }

                if (notificationType.HasValue)
                {
                    query = query.Where(m => m.NotificationType == notificationType.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(m => m.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(m => m.CreatedAt <= endDate.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination and ordering
                var messages = await query
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Convert to DTOs
                var messageDtos = messages.Select(m => new TeamsMessageResponseDto
                {
                    Success = m.Status == "Sent",
                    MessageId = m.MessageId ?? m.Id.ToString(),
                    Status = m.Status,
                    ErrorMessage = m.ErrorMessage,
                    SentAt = m.SentAt ?? m.CreatedAt,
                    WebhookUrl = m.WebhookUrl
                }).ToList();

                return new PagedResultDto<TeamsMessageResponseDto>
                {
                    Items = messageDtos,
                    TotalCount = totalCount,
                    Page = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNext = pageNumber * pageSize < totalCount,
                    HasPrevious = pageNumber > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams message history");
                throw;
            }
        }

        #endregion

        #region Webhook Configuration

        public async Task<TeamsWebhookConfigDto> CreateWebhookConfigAsync(TeamsWebhookConfigDto configDto, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating Teams webhook configuration for user {UserId}", userId);

                // Validate webhook URL
                var validationResult = await ValidateWebhookUrlAsync(configDto.WebhookUrl);
                if (!validationResult.Success)
                {
                    throw new ArgumentException(validationResult.ErrorMessage);
                }

                var config = new TeamsWebhookConfig
                {
                    Id = Guid.NewGuid(),
                    Name = configDto.Name,
                    WebhookUrl = configDto.WebhookUrl,
                    Description = configDto.Description,
                    EnabledNotifications = configDto.EnabledNotifications,
                    IsActive = configDto.IsActive,
                    DefaultFormat = configDto.DefaultFormat.ToString(),
                    DefaultThemeColor = configDto.DefaultThemeColor,
                    CustomSettings = configDto.CustomSettings,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TeamsWebhookConfigs.Add(config);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Teams webhook configuration {ConfigId} created for user {UserId}", 
                    config.Id, userId);

                return _mapper.Map<TeamsWebhookConfigDto>(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Teams webhook configuration for user {UserId}", userId);
                throw;
            }
        }

        public async Task<TeamsWebhookConfigDto> UpdateWebhookConfigAsync(Guid configId, TeamsWebhookConfigDto configDto, Guid userId)
        {
            try
            {
                var config = await _context.TeamsWebhookConfigs
                    .FirstOrDefaultAsync(c => c.Id == configId && c.UserId == userId);

                if (config == null)
                {
                    throw new ArgumentException("Teams webhook configuration not found or access denied");
                }

                // Validate webhook URL if changed
                if (config.WebhookUrl != configDto.WebhookUrl)
                {
                    var validationResult = await ValidateWebhookUrlAsync(configDto.WebhookUrl);
                    if (!validationResult.Success)
                    {
                        throw new ArgumentException(validationResult.ErrorMessage);
                    }
                }

                config.Name = configDto.Name;
                config.WebhookUrl = configDto.WebhookUrl;
                config.Description = configDto.Description;
                config.EnabledNotifications = configDto.EnabledNotifications;
                config.IsActive = configDto.IsActive;
                config.DefaultFormat = configDto.DefaultFormat.ToString();
                config.DefaultThemeColor = configDto.DefaultThemeColor;
                config.CustomSettings = configDto.CustomSettings;
                config.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Teams webhook configuration {ConfigId} updated for user {UserId}", 
                    configId, userId);

                return _mapper.Map<TeamsWebhookConfigDto>(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Teams webhook configuration {ConfigId} for user {UserId}", 
                    configId, userId);
                throw;
            }
        }

        public async Task<ServiceResultDto> DeleteWebhookConfigAsync(Guid configId, Guid userId)
        {
            try
            {
                var config = await _context.TeamsWebhookConfigs
                    .FirstOrDefaultAsync(c => c.Id == configId && c.UserId == userId);

                if (config == null)
                {
                    return ServiceResultDto.ErrorResult("Teams webhook configuration not found or access denied");
                }

                _context.TeamsWebhookConfigs.Remove(config);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Teams webhook configuration {ConfigId} deleted for user {UserId}", 
                    configId, userId);

                return ServiceResultDto.SuccessResult("Teams webhook configuration deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Teams webhook configuration {ConfigId} for user {UserId}", 
                    configId, userId);
                return ServiceResultDto.ErrorResult("Failed to delete Teams webhook configuration");
            }
        }

        public async Task<List<TeamsWebhookConfigDto>> GetWebhookConfigsAsync(Guid? userId = null)
        {
            try
            {
                var query = _context.TeamsWebhookConfigs.AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Where(c => c.UserId == userId.Value);
                }

                var configs = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<TeamsWebhookConfigDto>>(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams webhook configurations");
                throw;
            }
        }

        public async Task<TeamsWebhookConfigDto?> GetWebhookConfigByIdAsync(Guid configId, Guid userId)
        {
            try
            {
                var config = await _context.TeamsWebhookConfigs
                    .FirstOrDefaultAsync(c => c.Id == configId && c.UserId == userId);

                return config != null ? _mapper.Map<TeamsWebhookConfigDto>(config) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams webhook configuration {ConfigId}", configId);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<TeamsMessageResponseDto> SendTeamsMessageAsync(TeamsMessage teamsMessage)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Build Teams message payload
                var payload = BuildTeamsPayload(teamsMessage);
                var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send the message
                var response = await _httpClient.PostAsync(teamsMessage.WebhookUrl, content);
                stopwatch.Stop();

                var success = response.IsSuccessStatusCode;
                var statusCode = (int)response.StatusCode;
                var responseContent = await response.Content.ReadAsStringAsync();

                if (success)
                {
                    return new TeamsMessageResponseDto
                    {
                        Success = true,
                        MessageId = teamsMessage.Id.ToString(),
                        Status = "Sent",
                        SentAt = DateTime.UtcNow,
                        WebhookUrl = teamsMessage.WebhookUrl
                    };
                }
                else
                {
                    var errorMessage = $"Teams webhook returned {statusCode}: {responseContent}";
                    
                    // Log delivery failure
                    await LogDeliveryFailureAsync(teamsMessage, errorMessage, statusCode);

                    return new TeamsMessageResponseDto
                    {
                        Success = false,
                        MessageId = teamsMessage.Id.ToString(),
                        Status = "Failed",
                        ErrorMessage = errorMessage,
                        SentAt = DateTime.UtcNow,
                        WebhookUrl = teamsMessage.WebhookUrl
                    };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var errorMessage = $"Exception sending Teams message: {ex.Message}";
                
                // Log delivery failure
                await LogDeliveryFailureAsync(teamsMessage, errorMessage, 0);

                return new TeamsMessageResponseDto
                {
                    Success = false,
                    MessageId = teamsMessage.Id.ToString(),
                    Status = "Failed",
                    ErrorMessage = errorMessage,
                    SentAt = DateTime.UtcNow,
                    WebhookUrl = teamsMessage.WebhookUrl
                };
            }
            finally
            {
                teamsMessage.ResponseTime = stopwatch.Elapsed;
            }
        }

        private object BuildTeamsPayload(TeamsMessage teamsMessage)
        {
            if (teamsMessage.UseAdaptiveCard)
            {
                return BuildAdaptiveCardPayload(teamsMessage);
            }
            else
            {
                return BuildMessageCardPayload(teamsMessage);
            }
        }

        private object BuildMessageCardPayload(TeamsMessage teamsMessage)
        {
            var payload = new
            {
                type = "MessageCard",
                context = "https://schema.org/extensions",
                summary = teamsMessage.Title,
                title = teamsMessage.Title,
                text = teamsMessage.Message,
                themeColor = teamsMessage.ThemeColor ?? GetThemeColorForMessageType(
                    Enum.Parse<TeamsMessageType>(teamsMessage.MessageType))
            };

            // Add sections and actions if available
            if (!string.IsNullOrEmpty(teamsMessage.ActionsJson) || !string.IsNullOrEmpty(teamsMessage.FactsJson))
            {
                var sections = new List<object>();
                
                if (!string.IsNullOrEmpty(teamsMessage.FactsJson))
                {
                    var facts = JsonSerializer.Deserialize<Dictionary<string, object>>(teamsMessage.FactsJson);
                    if (facts != null && facts.Any())
                    {
                        sections.Add(new
                        {
                            facts = facts.Select(f => new { name = f.Key, value = f.Value?.ToString() }).ToArray()
                        });
                    }
                }

                var result = new
                {
                    type = payload.type,
                    context = payload.context,
                    summary = payload.summary,
                    title = payload.title,
                    text = payload.text,
                    themeColor = payload.themeColor,
                    sections = sections.ToArray()
                };

                if (!string.IsNullOrEmpty(teamsMessage.ActionsJson))
                {
                    var actions = JsonSerializer.Deserialize<List<TeamsCardAction>>(teamsMessage.ActionsJson);
                    if (actions != null && actions.Any())
                    {
                        return new
                        {
                            type = result.type,
                            context = result.context,
                            summary = result.summary,
                            title = result.title,
                            text = result.text,
                            themeColor = result.themeColor,
                            sections = result.sections,
                            potentialAction = actions.Select(a => new
                            {
                                type = a.Type,
                                name = a.Name,
                                target = a.Target
                            }).ToArray()
                        };
                    }
                }

                return result;
            }

            return payload;
        }

        private object BuildAdaptiveCardPayload(TeamsMessage teamsMessage)
        {
            // Build adaptive card structure
            var cardBody = new List<object>
            {
                new
                {
                    type = "TextBlock",
                    text = teamsMessage.Title,
                    weight = "Bolder",
                    size = "Medium"
                },
                new
                {
                    type = "TextBlock",
                    text = teamsMessage.Message,
                    wrap = true
                }
            };

            // Add facts if available
            if (!string.IsNullOrEmpty(teamsMessage.FactsJson))
            {
                var facts = JsonSerializer.Deserialize<Dictionary<string, object>>(teamsMessage.FactsJson);
                if (facts != null && facts.Any())
                {
                    cardBody.Add(new
                    {
                        type = "FactSet",
                        facts = facts.Select(f => new { title = f.Key, value = f.Value?.ToString() }).ToArray()
                    });
                }
            }

            var card = new
            {
                type = "AdaptiveCard",
                version = "1.3",
                body = cardBody.ToArray()
            };

            // Add actions if available
            if (!string.IsNullOrEmpty(teamsMessage.ActionsJson))
            {
                var actions = JsonSerializer.Deserialize<List<TeamsCardAction>>(teamsMessage.ActionsJson);
                if (actions != null && actions.Any())
                {
                    var cardActions = actions.Select(a => new
                    {
                        type = "Action.OpenUrl",
                        title = a.Name,
                        url = a.Target
                    }).ToArray();

                    return new
                    {
                        type = "message",
                        attachments = new[]
                        {
                            new
                            {
                                contentType = "application/vnd.microsoft.card.adaptive",
                                content = new
                                {
                                    type = card.type,
                                    version = card.version,
                                    body = card.body,
                                    actions = cardActions
                                }
                            }
                        }
                    };
                }
            }

            return new
            {
                type = "message",
                attachments = new[]
                {
                    new
                    {
                        contentType = "application/vnd.microsoft.card.adaptive",
                        content = card
                    }
                }
            };
        }

        private async Task LogDeliveryFailureAsync(TeamsMessage teamsMessage, string errorMessage, int statusCode)
        {
            try
            {
                var failure = new TeamsDeliveryFailure
                {
                    Id = Guid.NewGuid(),
                    WebhookUrl = teamsMessage.WebhookUrl,
                    ErrorMessage = errorMessage,
                    StatusCode = statusCode,
                    NotificationType = teamsMessage.NotificationType,
                    FailedAt = DateTime.UtcNow,
                    Status = "Failed",
                    TeamsMessageId = teamsMessage.Id,
                    UserId = teamsMessage.UserId,
                    WebhookConfigId = teamsMessage.WebhookConfigId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TeamsDeliveryFailures.Add(failure);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging Teams delivery failure for message {MessageId}", teamsMessage.Id);
            }
        }

        private async Task CheckRateLimitAsync(Guid userId, string webhookUrl)
        {
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            
            var recentMessages = await _context.TeamsMessages
                .Where(m => m.UserId == userId && 
                           m.WebhookUrl == webhookUrl && 
                           m.CreatedAt >= oneMinuteAgo)
                .CountAsync();

            if (recentMessages >= _rateLimitPerMinute)
            {
                throw new InvalidOperationException(
                    $"Rate limit exceeded. Maximum {_rateLimitPerMinute} messages per minute per webhook.");
            }
        }

        private string GetThemeColorForMessageType(TeamsMessageType messageType)
        {
            return messageType switch
            {
                TeamsMessageType.Success => "00FF00",
                TeamsMessageType.Warning => "FFAA00",
                TeamsMessageType.Error => "FF0000",
                TeamsMessageType.Alert => "FF6600",
                TeamsMessageType.Information => "0078D4",
                _ => "0078D4"
            };
        }

        private TeamsMessageType GetMessageTypeForNotification(NotificationType notificationType)
        {
            return notificationType switch
            {
                NotificationType.ReportApproved => TeamsMessageType.Success,
                NotificationType.ReportRejected => TeamsMessageType.Error,
                NotificationType.EscalationNotice => TeamsMessageType.Alert,
                NotificationType.DueDateReminder => TeamsMessageType.Warning,
                NotificationType.SecurityAlert => TeamsMessageType.Error,
                NotificationType.SystemAlert => TeamsMessageType.Alert,
                _ => TeamsMessageType.Information
            };
        }

        private string RenderTemplate(string template, Dictionary<string, object> data)
        {
            var result = template;
            
            foreach (var kvp in data)
            {
                var placeholder = $"{{{kvp.Key}}}";
                result = result.Replace(placeholder, kvp.Value?.ToString() ?? string.Empty);
            }

            return result;
        }

        #endregion

        #region Testing and Validation

        public async Task<TeamsTestResultDto> TestWebhookAsync(TeamsIntegrationTestDto testDto, Guid userId)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Testing Teams webhook {WebhookUrl} for user {UserId}", 
                    testDto.WebhookUrl, userId);

                var messageDto = new SendTeamsMessageDto
                {
                    WebhookUrl = testDto.WebhookUrl,
                    Title = "Test Message",
                    Message = testDto.TestMessage,
                    MessageType = testDto.MessageType
                };

                var response = await SendMessageAsync(messageDto, userId);
                stopwatch.Stop();

                return new TeamsTestResultDto
                {
                    Success = response.Success,
                    Status = response.Status,
                    ErrorMessage = response.ErrorMessage,
                    ResponseTime = stopwatch.Elapsed,
                    StatusCode = response.Success ? 200 : 400,
                    TestedAt = DateTime.UtcNow,
                    WebhookUrl = testDto.WebhookUrl
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error testing Teams webhook {WebhookUrl}", testDto.WebhookUrl);
                
                return new TeamsTestResultDto
                {
                    Success = false,
                    Status = "Error",
                    ErrorMessage = ex.Message,
                    ResponseTime = stopwatch.Elapsed,
                    StatusCode = 0,
                    TestedAt = DateTime.UtcNow,
                    WebhookUrl = testDto.WebhookUrl
                };
            }
        }

        public async Task<ServiceResultDto> ValidateWebhookUrlAsync(string webhookUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    return ServiceResultDto.ErrorResult("Webhook URL cannot be empty");
                }

                if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri))
                {
                    return ServiceResultDto.ErrorResult("Invalid webhook URL format");
                }

                if (uri.Scheme != "https")
                {
                    return ServiceResultDto.ErrorResult("Webhook URL must use HTTPS");
                }

                if (!uri.Host.Contains("outlook.office.com") && !uri.Host.Contains("outlook.office365.com"))
                {
                    return ServiceResultDto.ErrorResult("URL does not appear to be a valid Microsoft Teams webhook");
                }

                return ServiceResultDto.SuccessResult("Webhook URL is valid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook URL {WebhookUrl}", webhookUrl);
                return ServiceResultDto.ErrorResult("Error validating webhook URL");
            }
        }

        public async Task<List<TeamsTestResultDto>> TestAllWebhooksAsync(Guid userId)
        {
            try
            {
                var configs = await GetWebhookConfigsAsync(userId);
                var results = new List<TeamsTestResultDto>();

                foreach (var config in configs.Where(c => c.IsActive))
                {
                    var testDto = new TeamsIntegrationTestDto
                    {
                        WebhookUrl = config.WebhookUrl,
                        TestMessage = $"Test message for configuration: {config.Name}"
                    };

                    var result = await TestWebhookAsync(testDto, userId);
                    results.Add(result);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing all Teams webhooks for user {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Analytics and Statistics

        public async Task<TeamsIntegrationStatsDto> GetIntegrationStatsAsync(
            Guid? userId = null,
            string? webhookUrl = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _context.TeamsMessages.AsQueryable();

                // Apply filters
                if (userId.HasValue)
                {
                    query = query.Where(m => m.UserId == userId.Value);
                }

                if (!string.IsNullOrEmpty(webhookUrl))
                {
                    query = query.Where(m => m.WebhookUrl.Contains(webhookUrl));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(m => m.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(m => m.CreatedAt <= endDate.Value);
                }

                var messages = await query.ToListAsync();

                var totalMessages = messages.Count;
                var successfulDeliveries = messages.Count(m => m.Status == "Sent");
                var failedDeliveries = totalMessages - successfulDeliveries;
                var successRate = totalMessages > 0 ? (double)successfulDeliveries / totalMessages * 100 : 0;

                var averageResponseTime = messages.Where(m => m.ResponseTime.HasValue)
                    .Select(m => m.ResponseTime!.Value)
                    .DefaultIfEmpty(TimeSpan.Zero)
                    .Average(ts => ts.TotalMilliseconds);

                var messagesByType = messages.GroupBy(m => m.NotificationType)
                    .ToDictionary(g => g.Key, g => g.Count());

                var messagesByWebhook = messages.GroupBy(m => m.WebhookUrl)
                    .ToDictionary(g => g.Key, g => g.Count());

                var recentFailures = await _context.TeamsDeliveryFailures
                    .Where(f => !userId.HasValue || f.UserId == userId.Value)
                    .Where(f => string.IsNullOrEmpty(webhookUrl) || f.WebhookUrl.Contains(webhookUrl))
                    .Where(f => f.FailedAt >= (startDate ?? DateTime.UtcNow.AddDays(-7)))
                    .OrderByDescending(f => f.FailedAt)
                    .Take(10)
                    .Select(f => new TeamsMessageDeliveryFailure
                    {
                        FailedAt = f.FailedAt,
                        WebhookUrl = f.WebhookUrl,
                        ErrorMessage = f.ErrorMessage,
                        StatusCode = f.StatusCode,
                        NotificationType = f.NotificationType
                    })
                    .ToListAsync();

                return new TeamsIntegrationStatsDto
                {
                    TotalMessagesSent = totalMessages,
                    SuccessfulDeliveries = successfulDeliveries,
                    FailedDeliveries = failedDeliveries,
                    SuccessRate = successRate,
                    AverageResponseTime = TimeSpan.FromMilliseconds(averageResponseTime),
                    LastMessageSent = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue,
                    MessagesByType = messagesByType,
                    MessagesByWebhook = messagesByWebhook,
                    RecentFailures = recentFailures
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams integration statistics");
                throw;
            }
        }

        public async Task<PagedResultDto<Data.Entities.TeamsDeliveryFailure>> GetDeliveryFailuresAsync(
            Guid? userId = null,
            string? webhookUrl = null,
            bool? isResolved = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.TeamsDeliveryFailures.AsQueryable();

                // Apply filters
                if (userId.HasValue)
                {
                    query = query.Where(f => f.UserId == userId.Value);
                }

                if (!string.IsNullOrEmpty(webhookUrl))
                {
                    query = query.Where(f => f.WebhookUrl.Contains(webhookUrl));
                }

                if (isResolved.HasValue)
                {
                    if (isResolved.Value)
                    {
                        query = query.Where(f => f.Status == "Resolved");
                    }
                    else
                    {
                        query = query.Where(f => f.Status != "Resolved");
                    }
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination and ordering
                var failures = await query
                    .OrderByDescending(f => f.FailedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResultDto<Data.Entities.TeamsDeliveryFailure>
                {
                    Items = failures,
                    TotalCount = totalCount,
                    Page = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNext = pageNumber * pageSize < totalCount,
                    HasPrevious = pageNumber > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams delivery failures");
                throw;
            }
        }

        public async Task<ServiceResultDto> RetryFailedDeliveriesAsync(List<Guid> failureIds, Guid userId)
        {
            try
            {
                _logger.LogInformation("Retrying {Count} failed Teams deliveries for user {UserId}", 
                    failureIds.Count, userId);

                var failures = await _context.TeamsDeliveryFailures
                    .Where(f => failureIds.Contains(f.Id) && f.UserId == userId)
                    .ToListAsync();

                var retriedCount = 0;
                var errors = new List<string>();

                foreach (var failure in failures)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(failure.OriginalPayloadJson))
                        {
                            // Attempt to recreate and resend the message
                            var content = new StringContent(failure.OriginalPayloadJson, Encoding.UTF8, "application/json");
                            var response = await _httpClient.PostAsync(failure.WebhookUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                failure.Status = "Resolved";
                                failure.ResolvedAt = DateTime.UtcNow;
                                retriedCount++;
                            }
                            else
                            {
                                failure.RetryCount++;
                                failure.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, failure.RetryCount));
                                errors.Add($"Retry failed for {failure.WebhookUrl}: {response.StatusCode}");
                            }
                        }
                        else
                        {
                            errors.Add($"No original payload available for failure {failure.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failure.RetryCount++;
                        failure.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, failure.RetryCount));
                        errors.Add($"Exception retrying {failure.WebhookUrl}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                var message = $"Retried {retriedCount} out of {failures.Count} failed deliveries";
                if (errors.Any())
                {
                    message += $". {errors.Count} errors occurred.";
                }

                return ServiceResultDto.SuccessResult(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed Teams deliveries for user {UserId}", userId);
                return ServiceResultDto.ErrorResult("Failed to retry deliveries");
            }
        }

        #endregion

        #region Template Management

        public async Task<TeamsNotificationTemplateDto> CreateTemplateAsync(CreateTeamsTemplateDto templateDto, Guid userId)
        {
            try
            {
                var template = new TeamsNotificationTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = templateDto.Name,
                    NotificationType = templateDto.NotificationType,
                    TitleTemplate = templateDto.TitleTemplate,
                    MessageTemplate = templateDto.MessageTemplate,
                    ThemeColor = templateDto.ThemeColor,
                    DefaultActionsJson = templateDto.DefaultActions != null 
                        ? JsonSerializer.Serialize(templateDto.DefaultActions) 
                        : null,
                    UseAdaptiveCard = templateDto.UseAdaptiveCard,
                    DefaultFactsJson = templateDto.DefaultFacts != null 
                        ? JsonSerializer.Serialize(templateDto.DefaultFacts) 
                        : null,
                    IsActive = templateDto.IsActive,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TeamsNotificationTemplates.Add(template);
                await _context.SaveChangesAsync();

                return _mapper.Map<TeamsNotificationTemplateDto>(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Teams template for user {UserId}", userId);
                throw;
            }
        }

        public async Task<TeamsNotificationTemplateDto> UpdateTemplateAsync(Guid templateId, CreateTeamsTemplateDto templateDto, Guid userId)
        {
            try
            {
                var template = await _context.TeamsNotificationTemplates
                    .FirstOrDefaultAsync(t => t.Id == templateId && t.UserId == userId);

                if (template == null)
                {
                    throw new ArgumentException("Teams template not found or access denied");
                }

                template.Name = templateDto.Name;
                template.NotificationType = templateDto.NotificationType;
                template.TitleTemplate = templateDto.TitleTemplate;
                template.MessageTemplate = templateDto.MessageTemplate;
                template.ThemeColor = templateDto.ThemeColor;
                template.DefaultActionsJson = templateDto.DefaultActions != null 
                    ? JsonSerializer.Serialize(templateDto.DefaultActions) 
                    : null;
                template.UseAdaptiveCard = templateDto.UseAdaptiveCard;
                template.DefaultFactsJson = templateDto.DefaultFacts != null 
                    ? JsonSerializer.Serialize(templateDto.DefaultFacts) 
                    : null;
                template.IsActive = templateDto.IsActive;
                template.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return _mapper.Map<TeamsNotificationTemplateDto>(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Teams template {TemplateId} for user {UserId}", 
                    templateId, userId);
                throw;
            }
        }

        public async Task<ServiceResultDto> DeleteTemplateAsync(Guid templateId, Guid userId)
        {
            try
            {
                var template = await _context.TeamsNotificationTemplates
                    .FirstOrDefaultAsync(t => t.Id == templateId && t.UserId == userId);

                if (template == null)
                {
                    return ServiceResultDto.ErrorResult("Teams template not found or access denied");
                }

                _context.TeamsNotificationTemplates.Remove(template);
                await _context.SaveChangesAsync();

                return ServiceResultDto.SuccessResult("Teams template deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Teams template {TemplateId} for user {UserId}", 
                    templateId, userId);
                return ServiceResultDto.ErrorResult("Failed to delete Teams template");
            }
        }

        public async Task<List<TeamsNotificationTemplateDto>> GetTemplatesAsync(
            Guid? userId = null,
            NotificationType? notificationType = null,
            bool? isActive = null)
        {
            try
            {
                var query = _context.TeamsNotificationTemplates.AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Where(t => t.UserId == userId.Value);
                }

                if (notificationType.HasValue)
                {
                    query = query.Where(t => t.NotificationType == notificationType.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(t => t.IsActive == isActive.Value);
                }

                var templates = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<TeamsNotificationTemplateDto>>(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams templates");
                throw;
            }
        }

        public async Task<TeamsNotificationTemplateDto?> GetTemplateByIdAsync(Guid templateId, Guid userId)
        {
            try
            {
                var template = await _context.TeamsNotificationTemplates
                    .FirstOrDefaultAsync(t => t.Id == templateId && t.UserId == userId);

                return template != null ? _mapper.Map<TeamsNotificationTemplateDto>(template) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<SendTeamsMessageDto> RenderTemplateAsync(Guid templateId, Dictionary<string, object> data, Guid userId)
        {
            try
            {
                var template = await GetTemplateByIdAsync(templateId, userId);
                if (template == null)
                {
                    throw new ArgumentException("Teams template not found or access denied");
                }

                var messageDto = new SendTeamsMessageDto
                {
                    Title = RenderTemplate(template.TitleTemplate, data),
                    Message = RenderTemplate(template.MessageTemplate, data),
                    ThemeColor = template.ThemeColor,
                    UseAdaptiveCard = template.UseAdaptiveCard,
                    WebhookUrl = "" // This should be set by the caller
                };

                if (template.DefaultActions != null)
                {
                    messageDto.Actions = template.DefaultActions;
                }

                if (template.DefaultFacts != null)
                {
                    messageDto.Facts = template.DefaultFacts;
                }

                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering Teams template {TemplateId}", templateId);
                throw;
            }
        }

        #endregion

        #region Background Services

        public async Task<ServiceResultDto> ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var pendingMessages = await _context.TeamsMessages
                    .Where(m => m.Status == "Pending")
                    .OrderBy(m => m.CreatedAt)
                    .Take(100)
                    .ToListAsync(cancellationToken);

                if (!pendingMessages.Any())
                {
                    return ServiceResultDto.SuccessResult("No pending Teams messages to process");
                }

                var processedCount = 0;
                var errorCount = 0;

                foreach (var message in pendingMessages)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await SendTeamsMessageAsync(message);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing pending Teams message {MessageId}", message.Id);
                        message.Status = "Failed";
                        message.ErrorMessage = ex.Message;
                        errorCount++;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResultDto.SuccessResult(
                    $"Processed {processedCount} Teams messages. {errorCount} errors occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending Teams messages");
                return ServiceResultDto.ErrorResult("Failed to process pending Teams messages");
            }
        }

        public async Task<ServiceResultDto> CleanupOldDataAsync(int retentionDays = 90, CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

                // Clean up old messages
                var oldMessages = await _context.TeamsMessages
                    .Where(m => m.CreatedAt < cutoffDate)
                    .ToListAsync(cancellationToken);

                _context.TeamsMessages.RemoveRange(oldMessages);

                // Clean up old delivery failures
                var oldFailures = await _context.TeamsDeliveryFailures
                    .Where(f => f.FailedAt < cutoffDate && f.Status == "Resolved")
                    .ToListAsync(cancellationToken);

                _context.TeamsDeliveryFailures.RemoveRange(oldFailures);

                // Clean up old statistics
                var oldStats = await _context.TeamsIntegrationStats
                    .Where(s => s.StatDate < cutoffDate)
                    .ToListAsync(cancellationToken);

                _context.TeamsIntegrationStats.RemoveRange(oldStats);

                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResultDto.SuccessResult(
                    $"Cleaned up {oldMessages.Count} messages, {oldFailures.Count} failures, and {oldStats.Count} statistics records");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old Teams data");
                return ServiceResultDto.ErrorResult("Failed to clean up old Teams data");
            }
        }

        public async Task<ServiceResultDto> UpdateStatisticsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                // Get all webhook configurations
                var webhookConfigs = await _context.TeamsWebhookConfigs
                    .Where(w => w.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var config in webhookConfigs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Calculate statistics for today
                    var messages = await _context.TeamsMessages
                        .Where(m => m.WebhookConfigId == config.Id && m.CreatedAt.Date == today)
                        .ToListAsync(cancellationToken);

                    if (messages.Any())
                    {
                        var totalMessages = messages.Count;
                        var successfulDeliveries = messages.Count(m => m.Status == "Sent");
                        var failedDeliveries = totalMessages - successfulDeliveries;
                        var avgResponseTime = messages.Where(m => m.ResponseTime.HasValue)
                            .Select(m => m.ResponseTime!.Value)
                            .DefaultIfEmpty(TimeSpan.Zero)
                            .Average(ts => ts.TotalMilliseconds);

                        // Check if statistics already exist for today
                        var existingStat = await _context.TeamsIntegrationStats
                            .FirstOrDefaultAsync(s => s.WebhookConfigId == config.Id && s.StatDate.Date == today, 
                                cancellationToken);

                        if (existingStat != null)
                        {
                            // Update existing statistics
                            existingStat.TotalMessages = totalMessages;
                            existingStat.SuccessfulDeliveries = successfulDeliveries;
                            existingStat.FailedDeliveries = failedDeliveries;
                            existingStat.AverageResponseTime = TimeSpan.FromMilliseconds(avgResponseTime);
                            existingStat.LastMessageSent = messages.Max(m => m.CreatedAt);
                            existingStat.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            // Create new statistics
                            var stat = new TeamsIntegrationStat
                            {
                                Id = Guid.NewGuid(),
                                StatDate = today,
                                WebhookUrl = config.WebhookUrl,
                                NotificationType = NotificationType.SystemAlert, // Default
                                TotalMessages = totalMessages,
                                SuccessfulDeliveries = successfulDeliveries,
                                FailedDeliveries = failedDeliveries,
                                AverageResponseTime = TimeSpan.FromMilliseconds(avgResponseTime),
                                MaxResponseTime = messages.Where(m => m.ResponseTime.HasValue)
                                    .Select(m => m.ResponseTime!.Value)
                                    .DefaultIfEmpty(TimeSpan.Zero)
                                    .Max(),
                                MinResponseTime = messages.Where(m => m.ResponseTime.HasValue)
                                    .Select(m => m.ResponseTime!.Value)
                                    .DefaultIfEmpty(TimeSpan.Zero)
                                    .Min(),
                                LastMessageSent = messages.Max(m => m.CreatedAt),
                                WebhookConfigId = config.Id,
                                UserId = config.UserId,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.TeamsIntegrationStats.Add(stat);
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResultDto.SuccessResult("Teams integration statistics updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Teams integration statistics");
                return ServiceResultDto.ErrorResult("Failed to update Teams integration statistics");
            }
        }

        #endregion
    }
}
