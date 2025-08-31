using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Business.Models;
using ProjectControlsReportingTool.API.Data.Entities;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ProjectControlsReportingTool.API.Business.AppSettings;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Service for managing Slack integration operations
    /// </summary>
    public class SlackIntegrationService : ISlackIntegrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SlackIntegrationService> _logger;
        private readonly SlackSettings _slackSettings;
        private readonly int _maxRetries;

        public SlackIntegrationService(
            ApplicationDbContext context,
            IMapper mapper,
            HttpClient httpClient,
            ILogger<SlackIntegrationService> logger,
            IOptions<SlackSettings> slackSettings)
        {
            _context = context;
            _mapper = mapper;
            _httpClient = httpClient;
            _logger = logger;
            _slackSettings = slackSettings.Value;
            _maxRetries = _slackSettings?.MaxRetries ?? 3;
        }

        #region Message Operations

        public async Task<ServiceResultDto<SendSlackMessageResponseDto>> SendSlackMessageAsync(SendSlackMessageDto request)
        {
            try
            {
                var webhook = await _context.SlackWebhookConfigs
                    .FirstOrDefaultAsync(w => w.Id == request.WebhookId);

                if (webhook == null)
                {
                    return new ServiceResultDto<SendSlackMessageResponseDto>
                    {
                        Success = false,
                        ErrorMessage = "Webhook configuration not found"
                    };
                }

                if (!webhook.IsActive)
                {
                    return new ServiceResultDto<SendSlackMessageResponseDto>
                    {
                        Success = false,
                        ErrorMessage = "Webhook is not active"
                    };
                }

                var payload = BuildSlackPayload(request);
                var result = await SendSlackWebhookAsync(webhook.WebhookUrl, payload);

                var slackMessage = new SlackMessage
                {
                    WebhookId = request.WebhookId,
                    Channel = request.Channel ?? webhook.DefaultChannel,
                    MessageContent = request.Text,
                    Attachments = request.Attachments != null ? JsonSerializer.Serialize(request.Attachments) : null,
                    Blocks = request.Blocks != null ? JsonSerializer.Serialize(request.Blocks) : null,
                    MessageId = result.MessageId,
                    Status = result.Success ? "Sent" : "Failed",
                    SentAt = DateTime.UtcNow,
                    ErrorMessage = result.ErrorMessage,
                    ThreadTimestamp = request.ThreadTimestamp,
                    Username = request.Username,
                    IconEmoji = request.IconEmoji,
                    IconUrl = request.IconUrl
                };

                _context.SlackMessages.Add(slackMessage);

                if (result.Success)
                {
                    await UpdateSlackStatisticsAsync(request.WebhookId, "sent");
                }
                else
                {
                    await LogSlackDeliveryFailureAsync(request.WebhookId, request.Channel, result.ErrorMessage, payload);
                    await UpdateSlackStatisticsAsync(request.WebhookId, "failed");
                }

                await _context.SaveChangesAsync();

                return new ServiceResultDto<SendSlackMessageResponseDto>
                {
                    Success = result.Success,
                    Data = new SendSlackMessageResponseDto
                    {
                        MessageId = result.MessageId,
                        Success = result.Success,
                        Channel = request.Channel ?? webhook.DefaultChannel,
                        Timestamp = slackMessage.SentAt
                    },
                    ErrorMessage = result.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Slack message: {Message}", ex.Message);
                return new ServiceResultDto<SendSlackMessageResponseDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while sending the Slack message"
                };
            }
        }

        public async Task<ServiceResultDto<BulkSlackMessageResponseDto>> SendBulkSlackMessagesAsync(BulkSlackMessageDto request)
        {
            try
            {
                var response = new BulkSlackMessageResponseDto
                {
                    Results = new List<SendSlackMessageResponseDto>(),
                    TotalMessages = request.Messages.Count
                };

                var webhook = await _context.SlackWebhookConfigs
                    .FirstOrDefaultAsync(w => w.Id == request.WebhookId);

                if (webhook == null)
                {
                    return new ServiceResultDto<BulkSlackMessageResponseDto>
                    {
                        Success = false,
                        ErrorMessage = "Webhook configuration not found"
                    };
                }

                foreach (var message in request.Messages)
                {
                    message.WebhookId = request.WebhookId;
                    var result = await SendSlackMessageAsync(message);
                    
                    if (result.Data != null)
                    {
                        response.Results.Add(result.Data);
                        if (result.Success)
                            response.SuccessfulMessages++;
                        else
                            response.FailedMessages++;
                    }

                    // Add delay between messages to respect rate limits
                    if (_slackSettings?.RateLimitDelayMs > 0)
                    {
                        await Task.Delay(_slackSettings.RateLimitDelayMs);
                    }
                }

                return new ServiceResultDto<BulkSlackMessageResponseDto>
                {
                    Success = true,
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk Slack messages: {Message}", ex.Message);
                return new ServiceResultDto<BulkSlackMessageResponseDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while sending bulk Slack messages"
                };
            }
        }

        public async Task<ServiceResultDto<SendSlackMessageResponseDto>> SendSlackNotificationAsync(SlackNotificationRequestDto request)
        {
            try
            {
                var template = await _context.SlackNotificationTemplates
                    .FirstOrDefaultAsync(t => t.Id == request.TemplateId);

                if (template == null)
                {
                    return new ServiceResultDto<SendSlackMessageResponseDto>
                    {
                        Success = false,
                        ErrorMessage = "Notification template not found"
                    };
                }

                var renderedText = RenderTemplate(template.MessageTemplate, request.Variables);
                var attachments = !string.IsNullOrEmpty(template.AttachmentsTemplate) 
                    ? JsonSerializer.Deserialize<List<SlackAttachment>>(RenderTemplate(template.AttachmentsTemplate, request.Variables))
                    : null;
                var blocks = !string.IsNullOrEmpty(template.BlocksTemplate)
                    ? JsonSerializer.Deserialize<List<SlackBlock>>(RenderTemplate(template.BlocksTemplate, request.Variables))
                    : null;

                var messageRequest = new SendSlackMessageDto
                {
                    WebhookId = request.WebhookId,
                    Channel = request.Channel,
                    Text = renderedText,
                    Attachments = attachments,
                    Blocks = blocks,
                    Username = template.Username,
                    IconEmoji = template.IconEmoji,
                    ThreadTimestamp = request.ThreadTimestamp
                };

                return await SendSlackMessageAsync(messageRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Slack notification: {Message}", ex.Message);
                return new ServiceResultDto<SendSlackMessageResponseDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while sending the Slack notification"
                };
            }
        }

        public async Task<ServiceResultDto<SendSlackMessageResponseDto>> UpdateSlackMessageAsync(UpdateSlackMessageDto request)
        {
            try
            {
                // Note: Slack webhook URLs don't support message updates
                // This would require Slack Web API with bot tokens
                return new ServiceResultDto<SendSlackMessageResponseDto>
                {
                    Success = false,
                    ErrorMessage = "Message updates are not supported with webhook URLs. Use Slack Web API with bot tokens."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Slack message: {Message}", ex.Message);
                return new ServiceResultDto<SendSlackMessageResponseDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating the Slack message"
                };
            }
        }

        public async Task<ServiceResultDto<bool>> DeleteSlackMessageAsync(string messageId, string channel, string webhookId)
        {
            try
            {
                // Note: Slack webhook URLs don't support message deletion
                // This would require Slack Web API with bot tokens
                return new ServiceResultDto<bool>
                {
                    Success = false,
                    ErrorMessage = "Message deletion is not supported with webhook URLs. Use Slack Web API with bot tokens."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Slack message: {Message}", ex.Message);
                return new ServiceResultDto<bool>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while deleting the Slack message"
                };
            }
        }

        #endregion

        #region Webhook Management

        public async Task<ServiceResultDto<SlackWebhookConfigDto>> CreateSlackWebhookAsync(CreateSlackWebhookDto request)
        {
            try
            {
                var webhook = new SlackWebhookConfig
                {
                    Name = request.Name,
                    WebhookUrl = request.WebhookUrl,
                    DefaultChannel = request.DefaultChannel,
                    IsActive = request.IsActive,
                    Description = request.Description,
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    AllowedChannels = request.AllowedChannels != null ? string.Join(",", request.AllowedChannels) : null
                };

                _context.SlackWebhookConfigs.Add(webhook);
                await _context.SaveChangesAsync();

                // Test the webhook
                var testResult = await TestSlackWebhookUrlAsync(webhook.WebhookUrl);
                if (!testResult.Success)
                {
                    webhook.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                return new ServiceResultDto<SlackWebhookConfigDto>
                {
                    Success = true,
                    Data = _mapper.Map<SlackWebhookConfigDto>(webhook)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Slack webhook: {Message}", ex.Message);
                return new ServiceResultDto<SlackWebhookConfigDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating the Slack webhook"
                };
            }
        }

        public async Task<ServiceResultDto<SlackWebhookConfigDto>> UpdateSlackWebhookAsync(int webhookId, UpdateSlackWebhookDto request)
        {
            try
            {
                var webhook = await _context.SlackWebhookConfigs.FindAsync(webhookId);
                if (webhook == null)
                {
                    return new ServiceResultDto<SlackWebhookConfigDto>
                    {
                        Success = false,
                        ErrorMessage = "Webhook not found"
                    };
                }

                webhook.Name = request.Name;
                webhook.WebhookUrl = request.WebhookUrl;
                webhook.DefaultChannel = request.DefaultChannel;
                webhook.IsActive = request.IsActive;
                webhook.Description = request.Description;
                webhook.ModifiedAt = DateTime.UtcNow;
                webhook.AllowedChannels = request.AllowedChannels != null ? string.Join(",", request.AllowedChannels) : null;

                await _context.SaveChangesAsync();

                return new ServiceResultDto<SlackWebhookConfigDto>
                {
                    Success = true,
                    Data = _mapper.Map<SlackWebhookConfigDto>(webhook)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Slack webhook: {Message}", ex.Message);
                return new ServiceResultDto<SlackWebhookConfigDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating the Slack webhook"
                };
            }
        }

        public async Task<ServiceResultDto<bool>> DeleteSlackWebhookAsync(int webhookId)
        {
            try
            {
                var webhook = await _context.SlackWebhookConfigs.FindAsync(webhookId);
                if (webhook == null)
                {
                    return new ServiceResultDto<bool>
                    {
                        Success = false,
                        ErrorMessage = "Webhook not found"
                    };
                }

                _context.SlackWebhookConfigs.Remove(webhook);
                await _context.SaveChangesAsync();

                return new ServiceResultDto<bool>
                {
                    Success = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Slack webhook: {Message}", ex.Message);
                return new ServiceResultDto<bool>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while deleting the Slack webhook"
                };
            }
        }

        public async Task<ServiceResultDto<List<SlackWebhookConfigDto>>> GetSlackWebhooksAsync()
        {
            try
            {
                var webhooks = await _context.SlackWebhookConfigs
                    .OrderBy(w => w.Name)
                    .ToListAsync();

                return new ServiceResultDto<List<SlackWebhookConfigDto>>
                {
                    Success = true,
                    Data = _mapper.Map<List<SlackWebhookConfigDto>>(webhooks)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack webhooks: {Message}", ex.Message);
                return new ServiceResultDto<List<SlackWebhookConfigDto>>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving Slack webhooks"
                };
            }
        }

        public async Task<ServiceResultDto<SlackWebhookConfigDto>> GetSlackWebhookByIdAsync(int webhookId)
        {
            try
            {
                var webhook = await _context.SlackWebhookConfigs.FindAsync(webhookId);
                if (webhook == null)
                {
                    return new ServiceResultDto<SlackWebhookConfigDto>
                    {
                        Success = false,
                        ErrorMessage = "Webhook not found"
                    };
                }

                return new ServiceResultDto<SlackWebhookConfigDto>
                {
                    Success = true,
                    Data = _mapper.Map<SlackWebhookConfigDto>(webhook)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack webhook: {Message}", ex.Message);
                return new ServiceResultDto<SlackWebhookConfigDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving the Slack webhook"
                };
            }
        }

        public async Task<ServiceResultDto<SlackTestResponseDto>> TestSlackWebhookAsync(int webhookId)
        {
            try
            {
                var webhook = await _context.SlackWebhookConfigs.FindAsync(webhookId);
                if (webhook == null)
                {
                    return new ServiceResultDto<SlackTestResponseDto>
                    {
                        Success = false,
                        ErrorMessage = "Webhook not found"
                    };
                }

                return await TestSlackWebhookUrlAsync(webhook.WebhookUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Slack webhook: {Message}", ex.Message);
                return new ServiceResultDto<SlackTestResponseDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while testing the Slack webhook"
                };
            }
        }

        public async Task<ServiceResultDto<SlackTestResponseDto>> TestSlackWebhookUrlAsync(string webhookUrl)
        {
            try
            {
                var testPayload = new
                {
                    text = "🔧 Test message from Project Controls Reporting Tool",
                    username = "ProjectControlsBot",
                    icon_emoji = ":gear:"
                };

                var result = await SendSlackWebhookAsync(webhookUrl, JsonSerializer.Serialize(testPayload));

                return new ServiceResultDto<SlackTestResponseDto>
                {
                    Success = true,
                    Data = new SlackTestResponseDto
                    {
                        Success = result.Success,
                        ResponseTime = result.ResponseTime,
                        Message = result.Success ? "Webhook test successful" : result.ErrorMessage,
                        TestedAt = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Slack webhook URL: {Message}", ex.Message);
                return new ServiceResultDto<SlackTestResponseDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while testing the Slack webhook URL"
                };
            }
        }

        #endregion

        #region Template Management

        public async Task<ServiceResultDto<SlackNotificationTemplateDto>> CreateSlackTemplateAsync(CreateSlackTemplateDto request)
        {
            try
            {
                var template = new SlackNotificationTemplate
                {
                    Name = request.Name,
                    Description = request.Description,
                    TemplateType = request.TemplateType,
                    MessageTemplate = request.MessageTemplate,
                    AttachmentsTemplate = request.AttachmentsTemplate,
                    BlocksTemplate = request.BlocksTemplate,
                    Username = request.Username,
                    IconEmoji = request.IconEmoji,
                    IsActive = request.IsActive,
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SlackNotificationTemplates.Add(template);
                await _context.SaveChangesAsync();

                return new ServiceResultDto<SlackNotificationTemplateDto>
                {
                    Success = true,
                    Data = _mapper.Map<SlackNotificationTemplateDto>(template)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Slack template: {Message}", ex.Message);
                return new ServiceResultDto<SlackNotificationTemplateDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating the Slack template"
                };
            }
        }

        public async Task<ServiceResultDto<SlackNotificationTemplateDto>> UpdateSlackTemplateAsync(int templateId, UpdateSlackTemplateDto request)
        {
            try
            {
                var template = await _context.SlackNotificationTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return new ServiceResultDto<SlackNotificationTemplateDto>
                    {
                        Success = false,
                        ErrorMessage = "Template not found"
                    };
                }

                template.Name = request.Name;
                template.Description = request.Description;
                template.TemplateType = request.TemplateType;
                template.MessageTemplate = request.MessageTemplate;
                template.AttachmentsTemplate = request.AttachmentsTemplate;
                template.BlocksTemplate = request.BlocksTemplate;
                template.Username = request.Username;
                template.IconEmoji = request.IconEmoji;
                template.IsActive = request.IsActive;
                template.ModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ServiceResultDto<SlackNotificationTemplateDto>
                {
                    Success = true,
                    Data = _mapper.Map<SlackNotificationTemplateDto>(template)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Slack template: {Message}", ex.Message);
                return new ServiceResultDto<SlackNotificationTemplateDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating the Slack template"
                };
            }
        }

        public async Task<ServiceResultDto<bool>> DeleteSlackTemplateAsync(int templateId)
        {
            try
            {
                var template = await _context.SlackNotificationTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return new ServiceResultDto<bool>
                    {
                        Success = false,
                        ErrorMessage = "Template not found"
                    };
                }

                _context.SlackNotificationTemplates.Remove(template);
                await _context.SaveChangesAsync();

                return new ServiceResultDto<bool>
                {
                    Success = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Slack template: {Message}", ex.Message);
                return new ServiceResultDto<bool>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while deleting the Slack template"
                };
            }
        }

        public async Task<ServiceResultDto<List<SlackNotificationTemplateDto>>> GetSlackTemplatesAsync()
        {
            try
            {
                var templates = await _context.SlackNotificationTemplates
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                return new ServiceResultDto<List<SlackNotificationTemplateDto>>
                {
                    Success = true,
                    Data = _mapper.Map<List<SlackNotificationTemplateDto>>(templates)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack templates: {Message}", ex.Message);
                return new ServiceResultDto<List<SlackNotificationTemplateDto>>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving Slack templates"
                };
            }
        }

        public async Task<ServiceResultDto<SlackNotificationTemplateDto>> GetSlackTemplateByIdAsync(int templateId)
        {
            try
            {
                var template = await _context.SlackNotificationTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return new ServiceResultDto<SlackNotificationTemplateDto>
                    {
                        Success = false,
                        ErrorMessage = "Template not found"
                    };
                }

                return new ServiceResultDto<SlackNotificationTemplateDto>
                {
                    Success = true,
                    Data = _mapper.Map<SlackNotificationTemplateDto>(template)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack template: {Message}", ex.Message);
                return new ServiceResultDto<SlackNotificationTemplateDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving the Slack template"
                };
            }
        }

        public async Task<ServiceResultDto<List<SlackNotificationTemplateDto>>> GetSlackTemplatesByTypeAsync(string templateType)
        {
            try
            {
                var templates = await _context.SlackNotificationTemplates
                    .Where(t => t.TemplateType == templateType)
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                return new ServiceResultDto<List<SlackNotificationTemplateDto>>
                {
                    Success = true,
                    Data = _mapper.Map<List<SlackNotificationTemplateDto>>(templates)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack templates by type: {Message}", ex.Message);
                return new ServiceResultDto<List<SlackNotificationTemplateDto>>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving Slack templates by type"
                };
            }
        }

        #endregion

        #region Analytics and Statistics

        public async Task<ServiceResultDto<SlackIntegrationStatsDto>> GetSlackStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var stats = await _context.SlackIntegrationStats
                    .Where(s => s.Date >= startDate && s.Date <= endDate)
                    .ToListAsync();

                var totalSent = stats.Sum(s => s.MessagesSent);
                var totalFailed = stats.Sum(s => s.MessagesFailed);
                var totalReceived = stats.Sum(s => s.MessagesReceived);

                return new ServiceResultDto<SlackIntegrationStatsDto>
                {
                    Success = true,
                    Data = new SlackIntegrationStatsDto
                    {
                        TotalMessagesSent = totalSent,
                        TotalMessagesFailed = totalFailed,
                        TotalMessagesReceived = totalReceived,
                        SuccessRate = totalSent + totalFailed > 0 ? (decimal)totalSent / (totalSent + totalFailed) * 100 : 0,
                        StartDate = startDate.Value,
                        EndDate = endDate.Value,
                        DailyStats = stats.Select(s => new SlackDailyStatDto
                        {
                            Date = s.Date,
                            MessagesSent = s.MessagesSent,
                            MessagesFailed = s.MessagesFailed,
                            MessagesReceived = s.MessagesReceived
                        }).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack statistics: {Message}", ex.Message);
                return new ServiceResultDto<SlackIntegrationStatsDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving Slack statistics"
                };
            }
        }

        public async Task<ServiceResultDto<List<SlackDeliveryFailureDto>>> GetSlackDeliveryFailuresAsync(int? days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-(days ?? 30));

                var failures = await _context.SlackDeliveryFailures
                    .Where(f => f.FailedAt >= cutoffDate)
                    .OrderByDescending(f => f.FailedAt)
                    .ToListAsync();

                return new ServiceResultDto<List<SlackDeliveryFailureDto>>
                {
                    Success = true,
                    Data = _mapper.Map<List<SlackDeliveryFailureDto>>(failures)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack delivery failures: {Message}", ex.Message);
                return new ServiceResultDto<List<SlackDeliveryFailureDto>>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving Slack delivery failures"
                };
            }
        }

        public async Task<ServiceResultDto<List<SlackChannelStatsDto>>> GetSlackChannelStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var channelStats = await _context.SlackMessages
                    .Where(m => m.SentAt >= startDate && m.SentAt <= endDate)
                    .GroupBy(m => m.Channel)
                    .Select(g => new SlackChannelStatsDto
                    {
                        Channel = g.Key,
                        MessageCount = g.Count(),
                        SuccessfulMessages = g.Count(m => m.Status == "Sent"),
                        FailedMessages = g.Count(m => m.Status == "Failed"),
                        LastMessageAt = g.Max(m => m.SentAt)
                    })
                    .ToListAsync();

                return new ServiceResultDto<List<SlackChannelStatsDto>>
                {
                    Success = true,
                    Data = channelStats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack channel statistics: {Message}", ex.Message);
                return new ServiceResultDto<List<SlackChannelStatsDto>>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving Slack channel statistics"
                };
            }
        }

        public async Task<ServiceResultDto<int>> RetryFailedSlackDeliveriesAsync(List<int> failureIds = null)
        {
            try
            {
                var query = _context.SlackDeliveryFailures.AsQueryable();
                
                if (failureIds != null && failureIds.Any())
                {
                    query = query.Where(f => failureIds.Contains(f.Id));
                }
                else
                {
                    // Retry failures from last 24 hours if no specific IDs provided
                    var cutoffDate = DateTime.UtcNow.AddHours(-24);
                    query = query.Where(f => f.FailedAt >= cutoffDate);
                }

                var failures = await query.ToListAsync();
                int retriedCount = 0;

                foreach (var failure in failures)
                {
                    try
                    {
                        var result = await SendSlackWebhookAsync(failure.WebhookUrl, failure.MessagePayload);
                        if (result.Success)
                        {
                            _context.SlackDeliveryFailures.Remove(failure);
                            retriedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to retry Slack delivery for failure ID {FailureId}", failure.Id);
                    }

                    // Add delay between retries
                    if (_slackSettings?.RateLimitDelayMs > 0)
                    {
                        await Task.Delay(_slackSettings.RateLimitDelayMs);
                    }
                }

                await _context.SaveChangesAsync();

                return new ServiceResultDto<int>
                {
                    Success = true,
                    Data = retriedCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed Slack deliveries: {Message}", ex.Message);
                return new ServiceResultDto<int>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrying failed Slack deliveries"
                };
            }
        }

        #endregion

        #region Channel Management

        public async Task<ServiceResultDto<List<SlackChannelDto>>> GetSlackChannelsAsync(int webhookId)
        {
            try
            {
                // Note: Webhook URLs don't provide channel discovery
                // This would require Slack Web API with proper scopes
                var webhook = await _context.SlackWebhookConfigs.FindAsync(webhookId);
                if (webhook == null)
                {
                    return new ServiceResultDto<List<SlackChannelDto>>
                    {
                        Success = false,
                        ErrorMessage = "Webhook not found"
                    };
                }

                var channels = new List<SlackChannelDto>();
                
                // Add default channel if configured
                if (!string.IsNullOrEmpty(webhook.DefaultChannel))
                {
                    channels.Add(new SlackChannelDto
                    {
                        Id = webhook.DefaultChannel,
                        Name = webhook.DefaultChannel,
                        IsPrivate = false,
                        IsMember = true
                    });
                }

                // Add allowed channels if configured
                if (!string.IsNullOrEmpty(webhook.AllowedChannels))
                {
                    var allowedChannels = webhook.AllowedChannels.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var channel in allowedChannels)
                    {
                        if (channels.All(c => c.Id != channel.Trim()))
                        {
                            channels.Add(new SlackChannelDto
                            {
                                Id = channel.Trim(),
                                Name = channel.Trim(),
                                IsPrivate = false,
                                IsMember = true
                            });
                        }
                    }
                }

                return new ServiceResultDto<List<SlackChannelDto>>
                {
                    Success = true,
                    Data = channels
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack channels: {Message}", ex.Message);
                return new ServiceResultDto<List<SlackChannelDto>>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving Slack channels"
                };
            }
        }

        public async Task<ServiceResultDto<bool>> ValidateSlackChannelAsync(string channel, int webhookId)
        {
            try
            {
                var webhook = await _context.SlackWebhookConfigs.FindAsync(webhookId);
                if (webhook == null)
                {
                    return new ServiceResultDto<bool>
                    {
                        Success = false,
                        ErrorMessage = "Webhook not found"
                    };
                }

                // Check if channel is in allowed channels list (if configured)
                if (!string.IsNullOrEmpty(webhook.AllowedChannels))
                {
                    var allowedChannels = webhook.AllowedChannels.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim()).ToList();
                    
                    if (!allowedChannels.Contains(channel) && channel != webhook.DefaultChannel)
                    {
                        return new ServiceResultDto<bool>
                        {
                            Success = false,
                            ErrorMessage = "Channel is not in the allowed channels list"
                        };
                    }
                }

                return new ServiceResultDto<bool>
                {
                    Success = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Slack channel: {Message}", ex.Message);
                return new ServiceResultDto<bool>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while validating the Slack channel"
                };
            }
        }

        #endregion

        #region Configuration and Health

        public async Task<ServiceResultDto<SlackConfigurationDto>> GetSlackConfigurationAsync()
        {
            try
            {
                return new ServiceResultDto<SlackConfigurationDto>
                {
                    Success = true,
                    Data = new SlackConfigurationDto
                    {
                        MaxRetries = _slackSettings?.MaxRetries ?? 3,
                        TimeoutSeconds = _slackSettings?.TimeoutSeconds ?? 30,
                        RateLimitDelayMs = _slackSettings?.RateLimitDelayMs ?? 1000,
                        MaxMessageLength = _slackSettings?.MaxMessageLength ?? 4000,
                        EnableDeliveryTracking = _slackSettings?.EnableDeliveryTracking ?? true
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Slack configuration: {Message}", ex.Message);
                return new ServiceResultDto<SlackConfigurationDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving Slack configuration"
                };
            }
        }

        public async Task<ServiceResultDto<SlackConfigurationDto>> UpdateSlackConfigurationAsync(UpdateSlackConfigurationDto request)
        {
            try
            {
                // Note: In a real implementation, you would update configuration in database or settings file
                // For now, returning the current configuration
                return await GetSlackConfigurationAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Slack configuration: {Message}", ex.Message);
                return new ServiceResultDto<SlackConfigurationDto>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating Slack configuration"
                };
            }
        }

        public async Task<ServiceResultDto<List<SlackWebhookHealthDto>>> CheckSlackWebhookHealthAsync()
        {
            try
            {
                var webhooks = await _context.SlackWebhookConfigs.Where(w => w.IsActive).ToListAsync();
                var healthResults = new List<SlackWebhookHealthDto>();

                foreach (var webhook in webhooks)
                {
                    var testResult = await TestSlackWebhookUrlAsync(webhook.WebhookUrl);
                    
                    healthResults.Add(new SlackWebhookHealthDto
                    {
                        WebhookId = webhook.Id,
                        WebhookName = webhook.Name,
                        IsHealthy = testResult.Data?.Success ?? false,
                        ResponseTime = testResult.Data?.ResponseTime ?? 0,
                        LastChecked = DateTime.UtcNow,
                        ErrorMessage = testResult.Data?.Success == true ? null : testResult.Data?.Message
                    });
                }

                return new ServiceResultDto<List<SlackWebhookHealthDto>>
                {
                    Success = true,
                    Data = healthResults
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Slack webhook health: {Message}", ex.Message);
                return new ServiceResultDto<List<SlackWebhookHealthDto>>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while checking Slack webhook health"
                };
            }
        }

        #endregion

        #region Private Helper Methods

        private string BuildSlackPayload(SendSlackMessageDto request)
        {
            var payload = new Dictionary<string, object>
            {
                ["text"] = request.Text
            };

            if (!string.IsNullOrEmpty(request.Channel))
                payload["channel"] = request.Channel;

            if (!string.IsNullOrEmpty(request.Username))
                payload["username"] = request.Username;

            if (!string.IsNullOrEmpty(request.IconEmoji))
                payload["icon_emoji"] = request.IconEmoji;

            if (!string.IsNullOrEmpty(request.IconUrl))
                payload["icon_url"] = request.IconUrl;

            if (!string.IsNullOrEmpty(request.ThreadTimestamp))
                payload["thread_ts"] = request.ThreadTimestamp;

            if (request.Attachments != null && request.Attachments.Any())
                payload["attachments"] = request.Attachments;

            if (request.Blocks != null && request.Blocks.Any())
                payload["blocks"] = request.Blocks;

            return JsonSerializer.Serialize(payload);
        }

        private async Task<(bool Success, string MessageId, string ErrorMessage, int ResponseTime)> SendSlackWebhookAsync(string webhookUrl, string payload)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync(webhookUrl, content);

                stopwatch.Stop();
                var responseTime = (int)stopwatch.ElapsedMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    return (true, Guid.NewGuid().ToString(), null, responseTime);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, null, $"HTTP {(int)response.StatusCode}: {errorContent}", responseTime);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return (false, null, ex.Message, (int)stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task UpdateSlackStatisticsAsync(int webhookId, string status)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var stats = await _context.SlackIntegrationStats
                    .FirstOrDefaultAsync(s => s.WebhookId == webhookId && s.Date == today);

                if (stats == null)
                {
                    stats = new SlackIntegrationStat
                    {
                        WebhookId = webhookId,
                        Date = today,
                        MessagesSent = 0,
                        MessagesFailed = 0,
                        MessagesReceived = 0
                    };
                    _context.SlackIntegrationStats.Add(stats);
                }

                switch (status.ToLower())
                {
                    case "sent":
                        stats.MessagesSent++;
                        break;
                    case "failed":
                        stats.MessagesFailed++;
                        break;
                    case "received":
                        stats.MessagesReceived++;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update Slack statistics for webhook {WebhookId}", webhookId);
            }
        }

        private async Task LogSlackDeliveryFailureAsync(int webhookId, string channel, string errorMessage, string payload)
        {
            try
            {
                var webhook = await _context.SlackWebhookConfigs.FindAsync(webhookId);
                if (webhook == null) return;

                var failure = new SlackDeliveryFailure
                {
                    WebhookId = webhookId,
                    WebhookUrl = webhook.WebhookUrl,
                    Channel = channel,
                    ErrorMessage = errorMessage,
                    MessagePayload = payload,
                    FailedAt = DateTime.UtcNow,
                    RetryCount = 0
                };

                _context.SlackDeliveryFailures.Add(failure);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log Slack delivery failure for webhook {WebhookId}", webhookId);
            }
        }

        private static string RenderTemplate(string template, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(template) || variables == null)
                return template;

            var result = template;
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }

            return result;
        }

        #endregion
    }
}
