using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Business.Models;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Service for SMS operations with multiple provider support
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SmsService> _logger;
        private readonly SmsOptions _smsOptions;
        private readonly HttpClient _httpClient;

        // SMS provider configurations
        private readonly Dictionary<string, SmsProviderConfig> _providers;

        public SmsService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<SmsService> logger,
            IOptions<SmsOptions> smsOptions,
            HttpClient httpClient)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _smsOptions = smsOptions.Value;
            _httpClient = httpClient;

            // Initialize SMS providers
            _providers = InitializeSmsProviders();
        }

        /// <summary>
        /// Send a single SMS message
        /// </summary>
        public async Task<SmsDeliveryDto> SendSmsAsync(SendSmsDto sendSmsDto, Guid? userId = null)
        {
            try
            {
                _logger.LogInformation("Sending SMS to {PhoneNumber}", sendSmsDto.PhoneNumber);

                // Validate inputs
                if (!await ValidatePhoneNumberAsync(sendSmsDto.PhoneNumber))
                {
                    return CreateFailedDelivery(sendSmsDto.PhoneNumber, sendSmsDto.Message, "Invalid phone number format");
                }

                if (userId.HasValue && !await CanUserSendSmsAsync(userId.Value))
                {
                    return CreateFailedDelivery(sendSmsDto.PhoneNumber, sendSmsDto.Message, "User SMS limit exceeded");
                }

                // Format phone number
                var formattedNumber = await FormatPhoneNumberAsync(sendSmsDto.PhoneNumber);

                // Select SMS provider
                var provider = await SelectSmsProviderAsync(formattedNumber);
                if (provider == null)
                {
                    return CreateFailedDelivery(formattedNumber, sendSmsDto.Message, "No SMS provider available");
                }

                // Create SMS message entity
                var smsMessage = new SmsMessage
                {
                    Id = Guid.NewGuid(),
                    Recipient = formattedNumber,
                    Message = sendSmsDto.Message,
                    Priority = sendSmsDto.Priority,
                    MessageType = sendSmsDto.MessageType,
                    IsUrgent = sendSmsDto.IsUrgent,
                    Provider = provider.Name,
                    UserId = userId,
                    Status = "Pending",
                    SentAt = DateTime.UtcNow,
                    SegmentCount = CalculateSegmentCount(sendSmsDto.Message),
                    Encoding = DetectEncoding(sendSmsDto.Message),
                    CountryCode = ExtractCountryCode(formattedNumber),
                    DeliveryReceiptRequested = true,
                    ExpiresAt = sendSmsDto.ScheduledAt?.AddHours(24) ?? DateTime.UtcNow.AddHours(24),
                    Metadata = JsonSerializer.Serialize(sendSmsDto.Metadata)
                };

                // Save to database
                _context.SmsMessages.Add(smsMessage);
                await _context.SaveChangesAsync();

                // Send via provider
                var deliveryResult = await SendViaSmsProviderAsync(provider, smsMessage);

                // Update message with delivery result
                smsMessage.Status = deliveryResult.IsDelivered ? "Sent" : "Failed";
                smsMessage.ExternalMessageId = deliveryResult.ExternalMessageId;
                smsMessage.Cost = deliveryResult.Cost;
                smsMessage.ErrorMessage = deliveryResult.ErrorMessage;
                smsMessage.DeliveryAttempts = 1;
                smsMessage.LastDeliveryAttempt = DateTime.UtcNow;

                if (!deliveryResult.IsDelivered)
                {
                    smsMessage.RetryAt = DateTime.UtcNow.AddMinutes(5); // Retry in 5 minutes
                }

                await _context.SaveChangesAsync();

                // Update statistics
                await UpdateSmsStatisticsAsync(provider.Name, deliveryResult.IsDelivered, deliveryResult.Cost);

                _logger.LogInformation("SMS sent to {PhoneNumber} via {Provider}. Status: {Status}", 
                    formattedNumber, provider.Name, smsMessage.Status);

                return _mapper.Map<SmsDeliveryDto>(smsMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", sendSmsDto.PhoneNumber);
                return CreateFailedDelivery(sendSmsDto.PhoneNumber, sendSmsDto.Message, ex.Message);
            }
        }

        /// <summary>
        /// Send SMS using a template
        /// </summary>
        public async Task<SmsDeliveryDto> SendSmsFromTemplateAsync(string phoneNumber, Guid templateId, Dictionary<string, object> variables, Guid? userId = null)
        {
            try
            {
                // Get template
                var template = await _context.SmsTemplates
                    .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive);

                if (template == null)
                {
                    return CreateFailedDelivery(phoneNumber, "", "SMS template not found");
                }

                // Render template
                var renderedMessage = RenderSmsTemplate(template.Template, variables);

                if (renderedMessage.Length > template.MaxRenderedLength)
                {
                    return CreateFailedDelivery(phoneNumber, renderedMessage, "Rendered message exceeds maximum length");
                }

                // Create send DTO
                var sendSmsDto = new SendSmsDto
                {
                    PhoneNumber = phoneNumber,
                    Message = renderedMessage,
                    Priority = template.DefaultPriority,
                    MessageType = template.DefaultMessageType,
                    IsUrgent = template.DefaultIsUrgent
                };

                // Send SMS
                var result = await SendSmsAsync(sendSmsDto, userId);

                // Update template usage
                if (result.IsDelivered)
                {
                    template.UsageCount++;
                    template.LastUsedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Update message with template reference
                if (result.MessageId != Guid.Empty)
                {
                    var smsMessage = await _context.SmsMessages.FindAsync(result.MessageId);
                    if (smsMessage != null)
                    {
                        smsMessage.TemplateId = templateId;
                        await _context.SaveChangesAsync();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS from template {TemplateId} to {PhoneNumber}", templateId, phoneNumber);
                return CreateFailedDelivery(phoneNumber, "", ex.Message);
            }
        }

        /// <summary>
        /// Send bulk SMS messages
        /// </summary>
        public async Task<BulkSmsDeliveryDto> SendBulkSmsAsync(BulkSmsDto bulkSmsDto, Guid? userId = null)
        {
            var startTime = DateTime.UtcNow;
            var batchId = Guid.NewGuid();
            var deliveries = new List<SmsDeliveryDto>();
            var errors = new List<string>();

            try
            {
                _logger.LogInformation("Starting bulk SMS send to {Count} recipients", bulkSmsDto.PhoneNumbers.Count);

                // Validate bulk operation
                if (bulkSmsDto.PhoneNumbers.Count > _smsOptions.MaxBulkRecipients)
                {
                    errors.Add($"Bulk SMS limited to {_smsOptions.MaxBulkRecipients} recipients");
                    return CreateBulkFailedDelivery(batchId, bulkSmsDto.Message, bulkSmsDto.PhoneNumbers.Count, errors, startTime);
                }

                if (userId.HasValue && !await CanUserSendSmsAsync(userId.Value, bulkSmsDto.PhoneNumbers.Count))
                {
                    errors.Add("User SMS limit exceeded for bulk operation");
                    return CreateBulkFailedDelivery(batchId, bulkSmsDto.Message, bulkSmsDto.PhoneNumbers.Count, errors, startTime);
                }

                // Send to each recipient
                foreach (var phoneNumber in bulkSmsDto.PhoneNumbers.Distinct())
                {
                    try
                    {
                        var sendSmsDto = new SendSmsDto
                        {
                            PhoneNumber = phoneNumber,
                            Message = bulkSmsDto.Message,
                            Priority = bulkSmsDto.Priority,
                            MessageType = bulkSmsDto.MessageType,
                            IsUrgent = bulkSmsDto.IsUrgent,
                            ScheduledAt = bulkSmsDto.ScheduledAt
                        };

                        var delivery = await SendSmsAsync(sendSmsDto, userId);
                        deliveries.Add(delivery);

                        // Update batch ID
                        if (delivery.MessageId != Guid.Empty)
                        {
                            var smsMessage = await _context.SmsMessages.FindAsync(delivery.MessageId);
                            if (smsMessage != null)
                            {
                                smsMessage.BatchId = batchId;
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to send to {phoneNumber}: {ex.Message}");
                        _logger.LogError(ex, "Error sending bulk SMS to {PhoneNumber}", phoneNumber);
                    }
                }

                var processingTime = DateTime.UtcNow - startTime;
                var successfulDeliveries = deliveries.Count(d => d.IsDelivered);
                var totalCost = deliveries.Sum(d => d.Cost);

                _logger.LogInformation("Bulk SMS completed. {Successful}/{Total} successful", 
                    successfulDeliveries, bulkSmsDto.PhoneNumbers.Count);

                return new BulkSmsDeliveryDto
                {
                    BatchId = batchId,
                    Message = bulkSmsDto.Message,
                    TotalRecipients = bulkSmsDto.PhoneNumbers.Count,
                    SuccessfulDeliveries = successfulDeliveries,
                    FailedDeliveries = deliveries.Count - successfulDeliveries,
                    SuccessRate = bulkSmsDto.PhoneNumbers.Count > 0 ? (double)successfulDeliveries / bulkSmsDto.PhoneNumbers.Count * 100 : 0,
                    TotalCost = totalCost,
                    Deliveries = deliveries,
                    Errors = errors,
                    SentAt = startTime,
                    ProcessingTime = processingTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk SMS operation");
                errors.Add(ex.Message);
                return CreateBulkFailedDelivery(batchId, bulkSmsDto.Message, bulkSmsDto.PhoneNumbers.Count, errors, startTime);
            }
        }

        /// <summary>
        /// Send test SMS message
        /// </summary>
        public async Task<SmsDeliveryDto> SendTestSmsAsync(TestSmsDto testSmsDto)
        {
            var sendSmsDto = new SendSmsDto
            {
                PhoneNumber = testSmsDto.PhoneNumber,
                Message = testSmsDto.Message,
                Priority = "Low",
                MessageType = "Test",
                IsUrgent = false
            };

            return await SendSmsAsync(sendSmsDto);
        }

        /// <summary>
        /// Get SMS delivery status
        /// </summary>
        public async Task<SmsMessageDto?> GetSmsStatusAsync(Guid messageId)
        {
            var smsMessage = await _context.SmsMessages
                .Include(s => s.User)
                .Include(s => s.RelatedReport)
                .Include(s => s.Template)
                .FirstOrDefaultAsync(s => s.Id == messageId);

            return smsMessage != null ? _mapper.Map<SmsMessageDto>(smsMessage) : null;
        }

        /// <summary>
        /// Get SMS messages with filtering and pagination
        /// </summary>
        public async Task<(List<SmsMessageDto> Messages, int TotalCount)> GetSmsMessagesAsync(SmsSearchDto searchDto)
        {
            var query = _context.SmsMessages
                .Include(s => s.User)
                .Include(s => s.RelatedReport)
                .Include(s => s.Template)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                query = query.Where(s => s.Message.Contains(searchDto.SearchTerm) || 
                                        s.Recipient.Contains(searchDto.SearchTerm));
            }

            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                query = query.Where(s => s.Status == searchDto.Status);
            }

            if (!string.IsNullOrEmpty(searchDto.Priority))
            {
                query = query.Where(s => s.Priority == searchDto.Priority);
            }

            if (!string.IsNullOrEmpty(searchDto.MessageType))
            {
                query = query.Where(s => s.MessageType == searchDto.MessageType);
            }

            if (!string.IsNullOrEmpty(searchDto.Provider))
            {
                query = query.Where(s => s.Provider == searchDto.Provider);
            }

            if (searchDto.UserId.HasValue)
            {
                query = query.Where(s => s.UserId == searchDto.UserId);
            }

            if (searchDto.SentFrom.HasValue)
            {
                query = query.Where(s => s.SentAt >= searchDto.SentFrom);
            }

            if (searchDto.SentTo.HasValue)
            {
                query = query.Where(s => s.SentAt <= searchDto.SentTo);
            }

            if (searchDto.IsUrgent.HasValue)
            {
                query = query.Where(s => s.IsUrgent == searchDto.IsUrgent);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = searchDto.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(GetSortExpression(searchDto.SortBy))
                : query.OrderByDescending(GetSortExpression(searchDto.SortBy));

            // Apply pagination
            var messages = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            var messageDtos = _mapper.Map<List<SmsMessageDto>>(messages);

            return (messageDtos, totalCount);
        }

        /// <summary>
        /// Get SMS statistics
        /// </summary>
        public async Task<SmsStatisticsDto> GetSmsStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? provider = null)
        {
            fromDate ??= DateTime.UtcNow.AddDays(-30);
            toDate ??= DateTime.UtcNow;

            var query = _context.SmsMessages.AsQueryable();

            query = query.Where(s => s.SentAt >= fromDate && s.SentAt <= toDate);

            if (!string.IsNullOrEmpty(provider))
            {
                query = query.Where(s => s.Provider == provider);
            }

            var messages = await query.ToListAsync();

            var totalMessages = messages.Count;
            var successfulDeliveries = messages.Count(m => m.IsDelivered);
            var failedDeliveries = messages.Count(m => m.IsFailed);
            var deliveryRate = totalMessages > 0 ? (double)successfulDeliveries / totalMessages * 100 : 0;

            var today = DateTime.UtcNow.Date;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            return new SmsStatisticsDto
            {
                TotalMessagesSent = totalMessages,
                SuccessfulDeliveries = successfulDeliveries,
                FailedDeliveries = failedDeliveries,
                DeliveryRate = deliveryRate,
                TotalCost = messages.Sum(m => m.Cost),
                MessagesToday = messages.Count(m => m.SentAt.Date == today),
                MessagesThisWeek = messages.Count(m => m.SentAt.Date >= thisWeek),
                MessagesThisMonth = messages.Count(m => m.SentAt.Date >= thisMonth),
                MessagesByType = messages.GroupBy(m => m.MessageType).ToDictionary(g => g.Key, g => g.Count()),
                MessagesByProvider = messages.GroupBy(m => m.Provider).ToDictionary(g => g.Key, g => g.Count()),
                DeliveryRateByProvider = messages.GroupBy(m => m.Provider)
                    .ToDictionary(g => g.Key, g => g.Count() > 0 ? (double)g.Count(m => m.IsDelivered) / g.Count() * 100 : 0),
                AverageCostPerMessage = totalMessages > 0 ? messages.Sum(m => m.Cost) / totalMessages : 0,
                LastMessageSent = messages.Any() ? messages.Max(m => m.SentAt) : DateTime.MinValue,
                StatsGeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Retry failed SMS message
        /// </summary>
        public async Task<SmsDeliveryDto> RetrySmsAsync(Guid messageId)
        {
            var smsMessage = await _context.SmsMessages.FindAsync(messageId);
            if (smsMessage == null)
            {
                return CreateFailedDelivery("", "", "SMS message not found");
            }

            if (!smsMessage.IsFailed)
            {
                return CreateFailedDelivery(smsMessage.Recipient, smsMessage.Message, "SMS message is not in failed state");
            }

            if (smsMessage.IsExpired)
            {
                return CreateFailedDelivery(smsMessage.Recipient, smsMessage.Message, "SMS message has expired");
            }

            // Select provider and retry
            var provider = await SelectSmsProviderAsync(smsMessage.Recipient);
            if (provider == null)
            {
                return CreateFailedDelivery(smsMessage.Recipient, smsMessage.Message, "No SMS provider available");
            }

            var deliveryResult = await SendViaSmsProviderAsync(provider, smsMessage);

            // Update message
            smsMessage.Status = deliveryResult.IsDelivered ? "Sent" : "Failed";
            smsMessage.Provider = provider.Name;
            smsMessage.ExternalMessageId = deliveryResult.ExternalMessageId;
            smsMessage.Cost += deliveryResult.Cost;
            smsMessage.ErrorMessage = deliveryResult.ErrorMessage;
            smsMessage.DeliveryAttempts++;
            smsMessage.LastDeliveryAttempt = DateTime.UtcNow;

            if (!deliveryResult.IsDelivered && smsMessage.DeliveryAttempts < _smsOptions.MaxRetryAttempts)
            {
                smsMessage.RetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, smsMessage.DeliveryAttempts) * 5); // Exponential backoff
            }

            await _context.SaveChangesAsync();

            return _mapper.Map<SmsDeliveryDto>(smsMessage);
        }

        /// <summary>
        /// Cancel pending SMS message
        /// </summary>
        public async Task<bool> CancelSmsAsync(Guid messageId)
        {
            var smsMessage = await _context.SmsMessages.FindAsync(messageId);
            if (smsMessage == null || !smsMessage.IsPending)
            {
                return false;
            }

            smsMessage.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Get SMS templates
        /// </summary>
        public async Task<List<SmsTemplateDto>> GetSmsTemplatesAsync(string? category = null, bool activeOnly = true)
        {
            var query = _context.SmsTemplates
                .Include(t => t.CreatedByUser)
                .Include(t => t.UpdatedByUser)
                .AsQueryable();

            if (activeOnly)
            {
                query = query.Where(t => t.IsActive);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            var templates = await query.OrderBy(t => t.Category).ThenBy(t => t.Name).ToListAsync();
            return _mapper.Map<List<SmsTemplateDto>>(templates);
        }

        /// <summary>
        /// Create SMS template
        /// </summary>
        public async Task<SmsTemplateDto> CreateSmsTemplateAsync(CreateSmsTemplateDto createDto, Guid userId)
        {
            var template = new SmsTemplate
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Category = createDto.Category,
                Template = createDto.Template,
                Description = createDto.Description,
                IsActive = createDto.IsActive,
                Variables = JsonSerializer.Serialize(ExtractTemplateVariables(createDto.Template)),
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SmsTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Load with navigation properties
            template = await _context.SmsTemplates
                .Include(t => t.CreatedByUser)
                .FirstAsync(t => t.Id == template.Id);

            return _mapper.Map<SmsTemplateDto>(template);
        }

        /// <summary>
        /// Update SMS template
        /// </summary>
        public async Task<SmsTemplateDto> UpdateSmsTemplateAsync(Guid templateId, UpdateSmsTemplateDto updateDto, Guid userId)
        {
            var template = await _context.SmsTemplates.FindAsync(templateId);
            if (template == null)
            {
                throw new InvalidOperationException("SMS template not found");
            }

            if (template.IsSystemTemplate)
            {
                throw new InvalidOperationException("Cannot modify system templates");
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateDto.Name)) template.Name = updateDto.Name;
            if (!string.IsNullOrEmpty(updateDto.Category)) template.Category = updateDto.Category;
            if (!string.IsNullOrEmpty(updateDto.Template))
            {
                template.Template = updateDto.Template;
                template.Variables = JsonSerializer.Serialize(ExtractTemplateVariables(updateDto.Template));
            }
            if (!string.IsNullOrEmpty(updateDto.Description)) template.Description = updateDto.Description;
            if (updateDto.IsActive.HasValue) template.IsActive = updateDto.IsActive.Value;

            template.UpdatedBy = userId;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Load with navigation properties
            template = await _context.SmsTemplates
                .Include(t => t.CreatedByUser)
                .Include(t => t.UpdatedByUser)
                .FirstAsync(t => t.Id == template.Id);

            return _mapper.Map<SmsTemplateDto>(template);
        }

        /// <summary>
        /// Delete SMS template
        /// </summary>
        public async Task<bool> DeleteSmsTemplateAsync(Guid templateId, Guid userId)
        {
            var template = await _context.SmsTemplates.FindAsync(templateId);
            if (template == null || template.IsSystemTemplate)
            {
                return false;
            }

            template.IsActive = false;
            template.UpdatedBy = userId;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Preview SMS template with variables
        /// </summary>
        public async Task<string> PreviewSmsTemplateAsync(Guid templateId, Dictionary<string, object> variables)
        {
            var template = await _context.SmsTemplates.FindAsync(templateId);
            if (template == null)
            {
                throw new InvalidOperationException("SMS template not found");
            }

            return RenderSmsTemplate(template.Template, variables);
        }

        /// <summary>
        /// Get available SMS providers and their status
        /// </summary>
        public async Task<List<SmsProviderDto>> GetSmsProvidersAsync()
        {
            var providers = new List<SmsProviderDto>();

            foreach (var provider in _providers.Values)
            {
                var isActive = await TestSmsProviderAsync(provider.Name);

                providers.Add(new SmsProviderDto
                {
                    Name = provider.Name,
                    IsActive = isActive,
                    IsDefault = provider.Name == _smsOptions.DefaultProvider,
                    Priority = provider.Priority,
                    CostPerMessage = provider.CostPerMessage,
                    SupportedCountries = string.Join(", ", provider.SupportedCountries),
                    MaxMessageLength = provider.MaxMessageLength,
                    SupportsUnicode = provider.SupportsUnicode,
                    DeliveryRate = await GetProviderDeliveryRateAsync(provider.Name),
                    StatusDescription = isActive ? "Active" : "Inactive",
                    LastStatusCheck = DateTime.UtcNow
                });
            }

            return providers.OrderBy(p => p.Priority).ToList();
        }

        /// <summary>
        /// Test SMS provider connectivity
        /// </summary>
        public async Task<bool> TestSmsProviderAsync(string providerName)
        {
            try
            {
                if (!_providers.ContainsKey(providerName))
                {
                    return false;
                }

                var provider = _providers[providerName];

                // Test provider endpoint
                var response = await _httpClient.GetAsync(provider.StatusEndpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SMS provider {Provider}", providerName);
                return false;
            }
        }

        /// <summary>
        /// Get SMS configuration
        /// </summary>
        public async Task<SmsConfigurationDto> GetSmsConfigurationAsync()
        {
            return new SmsConfigurationDto
            {
                SmsEnabled = _smsOptions.Enabled,
                DefaultProvider = _smsOptions.DefaultProvider,
                MaxDailyMessages = _smsOptions.MaxDailyMessages,
                MaxMessagesPerUser = _smsOptions.MaxMessagesPerUser,
                DailyBudget = _smsOptions.DailyBudget,
                RequireApprovalForBulk = _smsOptions.RequireApprovalForBulk,
                BulkMessageThreshold = _smsOptions.BulkMessageThreshold,
                RestrictedNumbers = _smsOptions.RestrictedNumbers.ToList(),
                AllowedCountryCodes = _smsOptions.AllowedCountryCodes.ToList(),
                LogAllMessages = _smsOptions.LogAllMessages,
                RetentionDays = _smsOptions.RetentionDays,
                LastUpdated = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Update SMS configuration
        /// </summary>
        public async Task<SmsConfigurationDto> UpdateSmsConfigurationAsync(SmsConfigurationDto configDto, Guid userId)
        {
            // Note: In a real implementation, this would update configuration storage
            // For now, we'll just return the current config as this is typically stored in appsettings
            _logger.LogInformation("SMS configuration update requested by user {UserId}", userId);
            return await GetSmsConfigurationAsync();
        }

        /// <summary>
        /// Send notification SMS for report status changes
        /// </summary>
        public async Task<BulkSmsDeliveryDto> SendReportNotificationSmsAsync(Guid reportId, string status, List<string> recipientPhoneNumbers)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return CreateBulkFailedDelivery(Guid.NewGuid(), "", recipientPhoneNumbers.Count, 
                    new List<string> { "Report not found" }, DateTime.UtcNow);
            }

            var message = $"Report '{report.Title}' status changed to {status}. Check the system for details.";

            var bulkSmsDto = new BulkSmsDto
            {
                PhoneNumbers = recipientPhoneNumbers,
                Message = message,
                Priority = "High",
                MessageType = "ReportNotification",
                IsUrgent = status == "Rejected"
            };

            return await SendBulkSmsAsync(bulkSmsDto);
        }

        /// <summary>
        /// Send deadline reminder SMS
        /// </summary>
        public async Task<BulkSmsDeliveryDto> SendDeadlineReminderSmsAsync(Guid reportId, List<string> recipientPhoneNumbers, int daysUntilDeadline)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return CreateBulkFailedDelivery(Guid.NewGuid(), "", recipientPhoneNumbers.Count, 
                    new List<string> { "Report not found" }, DateTime.UtcNow);
            }

            var urgency = daysUntilDeadline <= 1 ? "URGENT" : daysUntilDeadline <= 3 ? "Important" : "";
            var message = $"{urgency} Reminder: Report '{report.Title}' due in {daysUntilDeadline} day(s). Due: {report.DueDate:yyyy-MM-dd}";

            var bulkSmsDto = new BulkSmsDto
            {
                PhoneNumbers = recipientPhoneNumbers,
                Message = message,
                Priority = daysUntilDeadline <= 1 ? "Critical" : "High",
                MessageType = "DeadlineReminder",
                IsUrgent = daysUntilDeadline <= 1
            };

            return await SendBulkSmsAsync(bulkSmsDto);
        }

        /// <summary>
        /// Send approval request SMS
        /// </summary>
        public async Task<BulkSmsDeliveryDto> SendApprovalRequestSmsAsync(Guid reportId, List<string> approverPhoneNumbers)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return CreateBulkFailedDelivery(Guid.NewGuid(), "", approverPhoneNumbers.Count, 
                    new List<string> { "Report not found" }, DateTime.UtcNow);
            }

            var message = $"Approval Required: Report '{report.Title}' is ready for your review. Please check the system.";

            var bulkSmsDto = new BulkSmsDto
            {
                PhoneNumbers = approverPhoneNumbers,
                Message = message,
                Priority = "High",
                MessageType = "ApprovalRequest",
                IsUrgent = false
            };

            return await SendBulkSmsAsync(bulkSmsDto);
        }

        /// <summary>
        /// Send emergency alert SMS
        /// </summary>
        public async Task<BulkSmsDeliveryDto> SendEmergencyAlertSmsAsync(string message, List<string> recipientPhoneNumbers)
        {
            var bulkSmsDto = new BulkSmsDto
            {
                PhoneNumbers = recipientPhoneNumbers,
                Message = $"EMERGENCY ALERT: {message}",
                Priority = "Critical",
                MessageType = "EmergencyAlert",
                IsUrgent = true
            };

            return await SendBulkSmsAsync(bulkSmsDto);
        }

        /// <summary>
        /// Validate phone number format
        /// </summary>
        public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Basic E.164 format validation
            var e164Pattern = @"^\+[1-9]\d{1,14}$";
            if (Regex.IsMatch(phoneNumber, e164Pattern))
                return true;

            // Local number validation (for South Africa)
            var localPattern = @"^0[0-9]{9}$";
            return Regex.IsMatch(phoneNumber, localPattern);
        }

        /// <summary>
        /// Format phone number to E.164 standard
        /// </summary>
        public async Task<string> FormatPhoneNumberAsync(string phoneNumber, string defaultCountryCode = "+27")
        {
            phoneNumber = Regex.Replace(phoneNumber, @"[^\d+]", ""); // Remove non-digits except +

            if (phoneNumber.StartsWith("+"))
                return phoneNumber;

            if (phoneNumber.StartsWith("0"))
                return defaultCountryCode + phoneNumber.Substring(1);

            return defaultCountryCode + phoneNumber;
        }

        /// <summary>
        /// Get SMS usage for user (daily/monthly limits)
        /// </summary>
        public async Task<(int DailyCount, int MonthlyCount, decimal DailyCost, decimal MonthlyCost)> GetUserSmsUsageAsync(Guid userId)
        {
            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var dailyMessages = await _context.SmsMessages
                .Where(s => s.UserId == userId && s.SentAt.Date == today)
                .ToListAsync();

            var monthlyMessages = await _context.SmsMessages
                .Where(s => s.UserId == userId && s.SentAt >= thisMonth)
                .ToListAsync();

            return (
                DailyCount: dailyMessages.Count,
                MonthlyCount: monthlyMessages.Count,
                DailyCost: dailyMessages.Sum(s => s.Cost),
                MonthlyCost: monthlyMessages.Sum(s => s.Cost)
            );
        }

        /// <summary>
        /// Check if user can send SMS (within limits)
        /// </summary>
        public async Task<bool> CanUserSendSmsAsync(Guid userId, int messageCount = 1)
        {
            if (!_smsOptions.Enabled)
                return false;

            var usage = await GetUserSmsUsageAsync(userId);

            // Check daily limits
            if (usage.DailyCount + messageCount > _smsOptions.MaxMessagesPerUser)
                return false;

            // Check system daily limits
            var todaySystemCount = await _context.SmsMessages
                .CountAsync(s => s.SentAt.Date == DateTime.UtcNow.Date);

            if (todaySystemCount + messageCount > _smsOptions.MaxDailyMessages)
                return false;

            return true;
        }

        /// <summary>
        /// Process SMS delivery receipts/webhooks
        /// </summary>
        public async Task<bool> ProcessSmsWebhookAsync(string providerName, Dictionary<string, object> webhookData)
        {
            try
            {
                _logger.LogInformation("Processing SMS webhook from {Provider}", providerName);

                // Extract message ID and status from webhook data
                if (!webhookData.ContainsKey("messageId") || !webhookData.ContainsKey("status"))
                {
                    _logger.LogWarning("Invalid webhook data from {Provider}", providerName);
                    return false;
                }

                var externalMessageId = webhookData["messageId"].ToString();
                var status = webhookData["status"].ToString();

                // Find the SMS message
                var smsMessage = await _context.SmsMessages
                    .FirstOrDefaultAsync(s => s.ExternalMessageId == externalMessageId);

                if (smsMessage == null)
                {
                    _logger.LogWarning("SMS message not found for external ID {ExternalId}", externalMessageId);
                    return false;
                }

                // Update status
                var oldStatus = smsMessage.Status;
                smsMessage.Status = MapProviderStatusToInternalStatus(status);

                if (smsMessage.Status == "Delivered" && smsMessage.DeliveredAt == null)
                {
                    smsMessage.DeliveredAt = DateTime.UtcNow;
                }

                // Update error message if failed
                if (webhookData.ContainsKey("errorMessage"))
                {
                    smsMessage.ErrorMessage = webhookData["errorMessage"].ToString();
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("SMS status updated from {OldStatus} to {NewStatus} for message {MessageId}", 
                    oldStatus, smsMessage.Status, smsMessage.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SMS webhook from {Provider}", providerName);
                return false;
            }
        }

        /// <summary>
        /// Clean up old SMS messages based on retention policy
        /// </summary>
        public async Task<int> CleanupOldSmsMessagesAsync(int retentionDays = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            var oldMessages = await _context.SmsMessages
                .Where(s => s.CreatedAt < cutoffDate)
                .ToListAsync();

            _context.SmsMessages.RemoveRange(oldMessages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} old SMS messages", oldMessages.Count);
            return oldMessages.Count;
        }

        /// <summary>
        /// Get SMS message history for a specific report
        /// </summary>
        public async Task<List<SmsMessageDto>> GetReportSmsHistoryAsync(Guid reportId)
        {
            var messages = await _context.SmsMessages
                .Include(s => s.User)
                .Include(s => s.Template)
                .Where(s => s.RelatedReportId == reportId)
                .OrderByDescending(s => s.SentAt)
                .ToListAsync();

            return _mapper.Map<List<SmsMessageDto>>(messages);
        }

        /// <summary>
        /// Get SMS message history for a specific user
        /// </summary>
        public async Task<List<SmsMessageDto>> GetUserSmsHistoryAsync(Guid userId, int limit = 50)
        {
            var messages = await _context.SmsMessages
                .Include(s => s.RelatedReport)
                .Include(s => s.Template)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SentAt)
                .Take(limit)
                .ToListAsync();

            return _mapper.Map<List<SmsMessageDto>>(messages);
        }

        #region Private Helper Methods

        private Dictionary<string, SmsProviderConfig> InitializeSmsProviders()
        {
            // In a real implementation, these would be configured via appsettings
            return new Dictionary<string, SmsProviderConfig>
            {
                ["Twilio"] = new SmsProviderConfig
                {
                    Name = "Twilio",
                    Priority = 1,
                    CostPerMessage = 0.05m,
                    MaxMessageLength = 1600,
                    SupportsUnicode = true,
                    SupportedCountries = new[] { "ZA", "US", "GB", "AU" },
                    StatusEndpoint = "https://status.twilio.com/api/v2/status.json"
                },
                ["BulkSMS"] = new SmsProviderConfig
                {
                    Name = "BulkSMS",
                    Priority = 2,
                    CostPerMessage = 0.04m,
                    MaxMessageLength = 160,
                    SupportsUnicode = false,
                    SupportedCountries = new[] { "ZA" },
                    StatusEndpoint = "https://www.bulksms.com/api/status"
                }
            };
        }

        private async Task<SmsProviderConfig?> SelectSmsProviderAsync(string phoneNumber)
        {
            var countryCode = ExtractCountryCode(phoneNumber);
            
            return _providers.Values
                .Where(p => p.SupportedCountries.Contains(countryCode) || p.SupportedCountries.Contains("ALL"))
                .OrderBy(p => p.Priority)
                .FirstOrDefault();
        }

        private async Task<SmsDeliveryResult> SendViaSmsProviderAsync(SmsProviderConfig provider, SmsMessage smsMessage)
        {
            try
            {
                // Simulate SMS sending (in real implementation, integrate with actual SMS providers)
                await Task.Delay(100); // Simulate network delay

                // Mock success/failure based on phone number for testing
                var isSuccess = !smsMessage.Recipient.EndsWith("0000"); // Mock failure for numbers ending in 0000

                return new SmsDeliveryResult
                {
                    IsDelivered = isSuccess,
                    ExternalMessageId = isSuccess ? Guid.NewGuid().ToString() : null,
                    Cost = provider.CostPerMessage * smsMessage.SegmentCount,
                    ErrorMessage = isSuccess ? null : "Mock SMS delivery failure"
                };
            }
            catch (Exception ex)
            {
                return new SmsDeliveryResult
                {
                    IsDelivered = false,
                    ErrorMessage = ex.Message,
                    Cost = 0
                };
            }
        }

        private async Task UpdateSmsStatisticsAsync(string provider, bool delivered, decimal cost)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var stats = await _context.SmsStatistics
                .FirstOrDefaultAsync(s => s.Date == today && s.Provider == provider);

            if (stats == null)
            {
                stats = new SmsStatistic
                {
                    Id = Guid.NewGuid(),
                    Date = today,
                    Provider = provider,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.SmsStatistics.Add(stats);
            }

            stats.MessagesSent++;
            if (delivered)
            {
                stats.MessagesDelivered++;
            }
            else
            {
                stats.MessagesFailed++;
            }
            
            stats.TotalCost += cost;
            stats.DeliveryRate = stats.MessagesSent > 0 ? (decimal)stats.MessagesDelivered / stats.MessagesSent * 100 : 0;
            stats.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private int CalculateSegmentCount(string message)
        {
            // Standard SMS is 160 characters for GSM7, 70 for Unicode
            var isUnicode = ContainsUnicodeCharacters(message);
            var maxLength = isUnicode ? 70 : 160;
            
            return (int)Math.Ceiling((double)message.Length / maxLength);
        }

        private string DetectEncoding(string message)
        {
            return ContainsUnicodeCharacters(message) ? "UCS2" : "GSM7";
        }

        private bool ContainsUnicodeCharacters(string message)
        {
            return message.Any(c => c > 127);
        }

        private string ExtractCountryCode(string phoneNumber)
        {
            if (phoneNumber.StartsWith("+27")) return "ZA";
            if (phoneNumber.StartsWith("+1")) return "US";
            if (phoneNumber.StartsWith("+44")) return "GB";
            if (phoneNumber.StartsWith("+61")) return "AU";
            return "ZA"; // Default to South Africa
        }

        private string RenderSmsTemplate(string template, Dictionary<string, object> variables)
        {
            var result = template;
            foreach (var variable in variables)
            {
                var placeholder = $"{{{variable.Key}}}";
                result = result.Replace(placeholder, variable.Value?.ToString() ?? "");
            }
            return result;
        }

        private List<string> ExtractTemplateVariables(string template)
        {
            var pattern = @"\{([^}]+)\}";
            var matches = Regex.Matches(template, pattern);
            return matches.Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
        }

        private string MapProviderStatusToInternalStatus(string providerStatus)
        {
            return providerStatus.ToLower() switch
            {
                "delivered" => "Delivered",
                "sent" => "Sent",
                "failed" => "Failed",
                "pending" => "Pending",
                "expired" => "Expired",
                "rejected" => "Rejected",
                _ => "Unknown"
            };
        }

        private async Task<double> GetProviderDeliveryRateAsync(string providerName)
        {
            var last30Days = DateTime.UtcNow.AddDays(-30);
            var messages = await _context.SmsMessages
                .Where(s => s.Provider == providerName && s.SentAt >= last30Days)
                .ToListAsync();

            if (!messages.Any())
                return 0;

            var deliveredCount = messages.Count(m => m.IsDelivered);
            return (double)deliveredCount / messages.Count * 100;
        }

        private System.Linq.Expressions.Expression<Func<SmsMessage, object>> GetSortExpression(string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "recipient" => s => s.Recipient,
                "status" => s => s.Status,
                "priority" => s => s.Priority,
                "provider" => s => s.Provider,
                "cost" => s => s.Cost,
                "createdat" => s => s.CreatedAt,
                _ => s => s.SentAt
            };
        }

        private SmsDeliveryDto CreateFailedDelivery(string recipient, string message, string errorMessage)
        {
            return new SmsDeliveryDto
            {
                MessageId = Guid.Empty,
                Recipient = recipient,
                Message = message,
                IsDelivered = false,
                Status = "Failed",
                ErrorMessage = errorMessage,
                SentAt = DateTime.UtcNow,
                Cost = 0,
                Provider = "None"
            };
        }

        private BulkSmsDeliveryDto CreateBulkFailedDelivery(Guid batchId, string message, int recipientCount, List<string> errors, DateTime startTime)
        {
            return new BulkSmsDeliveryDto
            {
                BatchId = batchId,
                Message = message,
                TotalRecipients = recipientCount,
                SuccessfulDeliveries = 0,
                FailedDeliveries = recipientCount,
                SuccessRate = 0,
                TotalCost = 0,
                Deliveries = new List<SmsDeliveryDto>(),
                Errors = errors,
                SentAt = startTime,
                ProcessingTime = DateTime.UtcNow - startTime
            };
        }

        #endregion

        #region Private Classes

        private class SmsProviderConfig
        {
            public string Name { get; set; } = string.Empty;
            public int Priority { get; set; }
            public decimal CostPerMessage { get; set; }
            public int MaxMessageLength { get; set; }
            public bool SupportsUnicode { get; set; }
            public string[] SupportedCountries { get; set; } = Array.Empty<string>();
            public string StatusEndpoint { get; set; } = string.Empty;
        }

        private class SmsDeliveryResult
        {
            public bool IsDelivered { get; set; }
            public string? ExternalMessageId { get; set; }
            public decimal Cost { get; set; }
            public string? ErrorMessage { get; set; }
        }

        #endregion
    }
}
