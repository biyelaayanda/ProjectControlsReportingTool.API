using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Business.AppSettings;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using System.Text.Json;
using WebPush;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Service for managing push notifications and subscriptions
    /// </summary>
    public class PushNotificationService : IPushNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly WebPushClient _webPushClient;
        private readonly PushNotificationOptions _options;

        public PushNotificationService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<PushNotificationService> logger,
            IOptions<PushNotificationOptions> options)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _options = options.Value;

            _webPushClient = new WebPushClient();
            _webPushClient.SetVapidDetails(
                _options.Subject,
                _options.PublicKey,
                _options.PrivateKey);
        }

        #region Subscription Management

        public async Task<PushNotificationSubscriptionDto> CreateSubscriptionAsync(Guid userId, CreatePushNotificationSubscriptionDto dto)
        {
            try
            {
                // Check if subscription already exists for this endpoint
                var existingSubscription = await _context.PushNotificationSubscriptions
                    .FirstOrDefaultAsync(s => s.Endpoint == dto.Endpoint);

                if (existingSubscription != null)
                {
                    // Update existing subscription
                    existingSubscription.UserId = userId;
                    existingSubscription.P256dhKey = dto.P256dhKey;
                    existingSubscription.AuthToken = dto.AuthToken;
                    existingSubscription.DeviceType = dto.DeviceType;
                    existingSubscription.DeviceName = dto.DeviceName;
                    existingSubscription.UserAgent = dto.UserAgent;
                    existingSubscription.ExpiresAt = dto.ExpiresAt;
                    existingSubscription.BrowserInfo = dto.BrowserInfo;
                    existingSubscription.OperatingSystem = dto.OperatingSystem;
                    existingSubscription.IsActive = true;
                    existingSubscription.HasPermission = true;
                    existingSubscription.EnabledForReports = dto.EnabledForReports;
                    existingSubscription.EnabledForApprovals = dto.EnabledForApprovals;
                    existingSubscription.EnabledForDeadlines = dto.EnabledForDeadlines;
                    existingSubscription.EnabledForAnnouncements = dto.EnabledForAnnouncements;
                    existingSubscription.EnabledForMentions = dto.EnabledForMentions;
                    existingSubscription.EnabledForReminders = dto.EnabledForReminders;
                    existingSubscription.MinimumPriority = dto.MinimumPriority;
                    existingSubscription.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return _mapper.Map<PushNotificationSubscriptionDto>(existingSubscription);
                }

                // Create new subscription
                var subscription = new PushNotificationSubscription
                {
                    UserId = userId,
                    Endpoint = dto.Endpoint,
                    P256dhKey = dto.P256dhKey,
                    AuthToken = dto.AuthToken,
                    DeviceType = dto.DeviceType,
                    DeviceName = dto.DeviceName,
                    UserAgent = dto.UserAgent,
                    ExpiresAt = dto.ExpiresAt,
                    BrowserInfo = dto.BrowserInfo,
                    OperatingSystem = dto.OperatingSystem,
                    IsActive = true,
                    HasPermission = true,
                    EnabledForReports = dto.EnabledForReports,
                    EnabledForApprovals = dto.EnabledForApprovals,
                    EnabledForDeadlines = dto.EnabledForDeadlines,
                    EnabledForAnnouncements = dto.EnabledForAnnouncements,
                    EnabledForMentions = dto.EnabledForMentions,
                    EnabledForReminders = dto.EnabledForReminders,
                    MinimumPriority = dto.MinimumPriority,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PushNotificationSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created push notification subscription for user {UserId} with endpoint {Endpoint}",
                    userId, dto.Endpoint);

                return _mapper.Map<PushNotificationSubscriptionDto>(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating push notification subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task<PushNotificationSubscriptionDto> UpdateSubscriptionAsync(Guid subscriptionId, UpdatePushNotificationSubscriptionDto dto)
        {
            try
            {
                var subscription = await _context.PushNotificationSubscriptions
                    .FindAsync(subscriptionId);

                if (subscription == null)
                {
                    throw new ArgumentException($"Subscription with ID {subscriptionId} not found");
                }

                // Update only provided fields
                if (dto.DeviceName != null)
                    subscription.DeviceName = dto.DeviceName;

                if (dto.IsActive.HasValue)
                    subscription.IsActive = dto.IsActive.Value;

                if (dto.EnabledForReports.HasValue)
                    subscription.EnabledForReports = dto.EnabledForReports.Value;

                if (dto.EnabledForApprovals.HasValue)
                    subscription.EnabledForApprovals = dto.EnabledForApprovals.Value;

                if (dto.EnabledForDeadlines.HasValue)
                    subscription.EnabledForDeadlines = dto.EnabledForDeadlines.Value;

                if (dto.EnabledForAnnouncements.HasValue)
                    subscription.EnabledForAnnouncements = dto.EnabledForAnnouncements.Value;

                if (dto.EnabledForMentions.HasValue)
                    subscription.EnabledForMentions = dto.EnabledForMentions.Value;

                if (dto.EnabledForReminders.HasValue)
                    subscription.EnabledForReminders = dto.EnabledForReminders.Value;

                if (!string.IsNullOrEmpty(dto.MinimumPriority))
                    subscription.MinimumPriority = dto.MinimumPriority;

                subscription.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated push notification subscription {SubscriptionId}", subscriptionId);

                return _mapper.Map<PushNotificationSubscriptionDto>(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating push notification subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<bool> DeleteSubscriptionAsync(Guid subscriptionId)
        {
            try
            {
                var subscription = await _context.PushNotificationSubscriptions
                    .FindAsync(subscriptionId);

                if (subscription == null)
                    return false;

                _context.PushNotificationSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted push notification subscription {SubscriptionId}", subscriptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting push notification subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<List<PushNotificationSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId)
        {
            try
            {
                var subscriptions = await _context.PushNotificationSubscriptions
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<PushNotificationSubscriptionDto>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<PushNotificationSubscriptionDto?> GetSubscriptionAsync(Guid subscriptionId)
        {
            try
            {
                var subscription = await _context.PushNotificationSubscriptions
                    .FindAsync(subscriptionId);

                return subscription != null ? _mapper.Map<PushNotificationSubscriptionDto>(subscription) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        #endregion

        #region Push Notification Sending

        public async Task<PushNotificationDeliveryDto> SendNotificationAsync(SendPushNotificationDto dto)
        {
            var startTime = DateTime.UtcNow;
            var result = new PushNotificationDeliveryDto
            {
                NotificationId = Guid.NewGuid(),
                Title = dto.Title,
                Body = dto.Body,
                SentAt = startTime
            };

            try
            {
                var subscriptions = await GetTargetSubscriptionsAsync(dto);
                result.TotalTargeted = subscriptions.Count;

                if (!subscriptions.Any())
                {
                    result.Errors.Add("No active subscriptions found for the specified criteria");
                    return result;
                }

                var payload = CreateNotificationPayload(dto);
                var deliveryTasks = subscriptions.Select(sub => SendToSubscriptionAsync(sub, payload)).ToArray();
                var deliveryResults = await Task.WhenAll(deliveryTasks);

                result.SuccessfulDeliveries = deliveryResults.Count(r => r.Success);
                result.FailedDeliveries = deliveryResults.Count(r => !r.Success);
                result.Errors.AddRange(deliveryResults.Where(r => !r.Success).Select(r => r.Error).Where(e => !string.IsNullOrEmpty(e)));
                result.SuccessRate = result.TotalTargeted > 0 ? (double)result.SuccessfulDeliveries / result.TotalTargeted * 100 : 0;
                result.DeliveryTime = DateTime.UtcNow - startTime;

                // Update subscription statistics
                await UpdateSubscriptionStatsAsync(deliveryResults);

                _logger.LogInformation("Sent push notification to {TotalTargeted} subscriptions. Success: {SuccessfulDeliveries}, Failed: {FailedDeliveries}",
                    result.TotalTargeted, result.SuccessfulDeliveries, result.FailedDeliveries);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification");
                result.Errors.Add($"Internal error: {ex.Message}");
                result.DeliveryTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        public async Task<PushNotificationDeliveryDto> SendTestNotificationAsync(TestPushNotificationDto dto)
        {
            var sendDto = new SendPushNotificationDto
            {
                Title = dto.Title,
                Body = dto.Body,
                Icon = dto.Icon,
                Url = dto.Url,
                Tag = "test",
                Priority = "High",
                NotificationType = "Test",
                Data = new Dictionary<string, object> { { "test", true } }
            };

            if (dto.TargetUserId.HasValue)
            {
                sendDto.UserIds = new List<Guid> { dto.TargetUserId.Value };
            }

            return await SendNotificationAsync(sendDto);
        }

        #endregion

        #region Subscription Search and Statistics

        public async Task<(List<PushNotificationSubscriptionDto> Subscriptions, int TotalCount)> SearchSubscriptionsAsync(PushNotificationSubscriptionSearchDto searchDto)
        {
            try
            {
                var query = _context.PushNotificationSubscriptions.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    query = query.Where(s => s.DeviceName!.Contains(searchDto.SearchTerm) ||
                                           s.BrowserInfo!.Contains(searchDto.SearchTerm) ||
                                           s.OperatingSystem!.Contains(searchDto.SearchTerm));
                }

                if (searchDto.UserId.HasValue)
                    query = query.Where(s => s.UserId == searchDto.UserId.Value);

                if (!string.IsNullOrEmpty(searchDto.DeviceType))
                    query = query.Where(s => s.DeviceType == searchDto.DeviceType);

                if (searchDto.IsActive.HasValue)
                    query = query.Where(s => s.IsActive == searchDto.IsActive.Value);

                if (searchDto.HasPermission.HasValue)
                    query = query.Where(s => s.HasPermission == searchDto.HasPermission.Value);

                if (searchDto.CreatedFrom.HasValue)
                    query = query.Where(s => s.CreatedAt >= searchDto.CreatedFrom.Value);

                if (searchDto.CreatedTo.HasValue)
                    query = query.Where(s => s.CreatedAt <= searchDto.CreatedTo.Value);

                if (searchDto.LastUsedFrom.HasValue)
                    query = query.Where(s => s.LastUsed >= searchDto.LastUsedFrom.Value);

                if (searchDto.LastUsedTo.HasValue)
                    query = query.Where(s => s.LastUsed <= searchDto.LastUsedTo.Value);

                if (!string.IsNullOrEmpty(searchDto.MinimumPriority))
                    query = query.Where(s => s.MinimumPriority == searchDto.MinimumPriority);

                var totalCount = await query.CountAsync();

                // Apply sorting
                query = searchDto.SortBy.ToLower() switch
                {
                    "devicetype" => searchDto.SortDirection.ToLower() == "asc" 
                        ? query.OrderBy(s => s.DeviceType) 
                        : query.OrderByDescending(s => s.DeviceType),
                    "devicename" => searchDto.SortDirection.ToLower() == "asc" 
                        ? query.OrderBy(s => s.DeviceName) 
                        : query.OrderByDescending(s => s.DeviceName),
                    "isactive" => searchDto.SortDirection.ToLower() == "asc" 
                        ? query.OrderBy(s => s.IsActive) 
                        : query.OrderByDescending(s => s.IsActive),
                    "lastused" => searchDto.SortDirection.ToLower() == "asc" 
                        ? query.OrderBy(s => s.LastUsed) 
                        : query.OrderByDescending(s => s.LastUsed),
                    _ => searchDto.SortDirection.ToLower() == "asc" 
                        ? query.OrderBy(s => s.CreatedAt) 
                        : query.OrderByDescending(s => s.CreatedAt)
                };

                // Apply pagination
                var subscriptions = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return (_mapper.Map<List<PushNotificationSubscriptionDto>>(subscriptions), totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching push notification subscriptions");
                throw;
            }
        }

        public async Task<PushNotificationSubscriptionStatsDto> GetSubscriptionStatsAsync()
        {
            try
            {
                var stats = new PushNotificationSubscriptionStatsDto();

                var subscriptions = await _context.PushNotificationSubscriptions.ToListAsync();

                stats.TotalSubscriptions = subscriptions.Count;
                stats.ActiveSubscriptions = subscriptions.Count(s => s.IsActive);
                stats.InactiveSubscriptions = subscriptions.Count(s => !s.IsActive);
                stats.WebSubscriptions = subscriptions.Count(s => s.DeviceType == "Web");
                stats.MobileSubscriptions = subscriptions.Count(s => s.DeviceType != "Web");

                stats.SubscriptionsByDevice = subscriptions
                    .GroupBy(s => s.DeviceType)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.SubscriptionsByBrowser = subscriptions
                    .Where(s => !string.IsNullOrEmpty(s.BrowserInfo))
                    .GroupBy(s => s.BrowserInfo!)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.TotalNotificationsSent = subscriptions.Sum(s => s.SuccessfulNotifications + s.FailedNotifications);
                stats.SuccessfulNotifications = subscriptions.Sum(s => s.SuccessfulNotifications);
                stats.FailedNotifications = subscriptions.Sum(s => s.FailedNotifications);
                stats.SuccessRate = stats.TotalNotificationsSent > 0 
                    ? (double)stats.SuccessfulNotifications / stats.TotalNotificationsSent * 100 
                    : 0;

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting push notification subscription statistics");
                throw;
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<BulkOperationResult> ProcessBulkOperationAsync(BulkPushNotificationOperationDto dto)
        {
            var result = new BulkOperationResult
            {
                TotalItems = dto.SubscriptionIds.Count,
                OperationType = dto.Operation
            };

            try
            {
                var subscriptions = await _context.PushNotificationSubscriptions
                    .Where(s => dto.SubscriptionIds.Contains(s.Id))
                    .ToListAsync();

                result.FoundItems = subscriptions.Count;

                switch (dto.Operation.ToLower())
                {
                    case "activate":
                        await ActivateSubscriptionsAsync(subscriptions);
                        result.SuccessfulItems = subscriptions.Count;
                        break;

                    case "deactivate":
                        await DeactivateSubscriptionsAsync(subscriptions);
                        result.SuccessfulItems = subscriptions.Count;
                        break;

                    case "delete":
                        await DeleteSubscriptionsAsync(subscriptions);
                        result.SuccessfulItems = subscriptions.Count;
                        break;

                    case "update":
                        if (dto.UpdateData != null)
                        {
                            result.SuccessfulItems = await UpdateSubscriptionsAsync(subscriptions, dto.UpdateData);
                        }
                        break;

                    case "test":
                        if (dto.TestNotification != null)
                        {
                            var testResult = await SendTestToSubscriptionsAsync(subscriptions, dto.TestNotification);
                            result.SuccessfulItems = testResult.SuccessfulDeliveries;
                            result.FailedItems = testResult.FailedDeliveries;
                        }
                        break;

                    default:
                        result.Errors.Add($"Unknown operation: {dto.Operation}");
                        break;
                }

                if (dto.Operation.ToLower() != "test")
                {
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Processed bulk operation {Operation} on {SuccessfulItems}/{TotalItems} subscriptions",
                    dto.Operation, result.SuccessfulItems, result.TotalItems);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk operation {Operation}", dto.Operation);
                result.Errors.Add($"Internal error: {ex.Message}");
                return result;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<List<PushNotificationSubscription>> GetTargetSubscriptionsAsync(SendPushNotificationDto dto)
        {
            var query = _context.PushNotificationSubscriptions
                .Where(s => s.IsActive && s.HasPermission);

            // Filter by user IDs if specified
            if (dto.UserIds?.Any() == true)
            {
                query = query.Where(s => dto.UserIds.Contains(s.UserId));
            }

            // Filter by device types if specified
            if (dto.DeviceTypes?.Any() == true)
            {
                query = query.Where(s => dto.DeviceTypes.Contains(s.DeviceType));
            }

            // Filter by minimum priority
            if (!string.IsNullOrEmpty(dto.MinimumPriority))
            {
                var allowedPriorities = GetAllowedPriorities(dto.MinimumPriority);
                query = query.Where(s => allowedPriorities.Contains(s.MinimumPriority));
            }

            // Filter by notification type preferences
            query = dto.NotificationType.ToLower() switch
            {
                "reports" => query.Where(s => s.EnabledForReports),
                "approvals" => query.Where(s => s.EnabledForApprovals),
                "deadlines" => query.Where(s => s.EnabledForDeadlines),
                "announcements" => query.Where(s => s.EnabledForAnnouncements),
                "mentions" => query.Where(s => s.EnabledForMentions),
                "reminders" => query.Where(s => s.EnabledForReminders),
                _ => query
            };

            // Filter only active devices if specified
            if (dto.OnlyActiveDevices == true)
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                query = query.Where(s => s.LastUsed == null || s.LastUsed >= thirtyDaysAgo);
            }

            return await query.ToListAsync();
        }

        private string CreateNotificationPayload(SendPushNotificationDto dto)
        {
            var payload = new
            {
                title = dto.Title,
                body = dto.Body,
                icon = dto.Icon ?? "/assets/logo.png",
                image = dto.Image,
                badge = dto.Badge ?? "/assets/badge.png",
                url = dto.Url,
                tag = dto.Tag,
                requireInteraction = dto.RequireInteraction,
                silent = dto.Silent,
                data = new Dictionary<string, object>(dto.Data)
                {
                    ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ["priority"] = dto.Priority,
                    ["type"] = dto.NotificationType
                },
                actions = dto.Actions.Select(action => new { action, title = action }).ToList()
            };

            return JsonSerializer.Serialize(payload);
        }

        private async Task<DeliveryResult> SendToSubscriptionAsync(PushNotificationSubscription subscription, string payload)
        {
            try
            {
                var pushSubscription = new PushSubscription(
                    subscription.Endpoint,
                    subscription.P256dhKey,
                    subscription.AuthToken);

                await _webPushClient.SendNotificationAsync(pushSubscription, payload);

                // Update success statistics
                subscription.SuccessfulNotifications++;
                subscription.LastUsed = DateTime.UtcNow;
                subscription.LastError = null;

                return new DeliveryResult { Success = true, SubscriptionId = subscription.Id };
            }
            catch (WebPushException ex)
            {
                _logger.LogWarning("Failed to send push notification to subscription {SubscriptionId}: {Error}",
                    subscription.Id, ex.Message);

                // Update failure statistics
                subscription.FailedNotifications++;
                subscription.LastError = ex.Message;

                // Deactivate subscription if it's permanently invalid
                if (ex.StatusCode == System.Net.HttpStatusCode.Gone || 
                    ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    subscription.IsActive = false;
                    subscription.HasPermission = false;
                }

                return new DeliveryResult 
                { 
                    Success = false, 
                    SubscriptionId = subscription.Id, 
                    Error = ex.Message 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending push notification to subscription {SubscriptionId}",
                    subscription.Id);

                subscription.FailedNotifications++;
                subscription.LastError = ex.Message;

                return new DeliveryResult 
                { 
                    Success = false, 
                    SubscriptionId = subscription.Id, 
                    Error = ex.Message 
                };
            }
        }

        private async Task UpdateSubscriptionStatsAsync(DeliveryResult[] results)
        {
            var subscriptionIds = results.Select(r => r.SubscriptionId).ToList();
            var subscriptions = await _context.PushNotificationSubscriptions
                .Where(s => subscriptionIds.Contains(s.Id))
                .ToListAsync();

            foreach (var subscription in subscriptions)
            {
                subscription.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static List<string> GetAllowedPriorities(string minimumPriority)
        {
            return minimumPriority.ToLower() switch
            {
                "low" => new List<string> { "Low", "Normal", "High", "Critical" },
                "normal" => new List<string> { "Normal", "High", "Critical" },
                "high" => new List<string> { "High", "Critical" },
                "critical" => new List<string> { "Critical" },
                _ => new List<string> { "Low", "Normal", "High", "Critical" }
            };
        }

        private async Task ActivateSubscriptionsAsync(List<PushNotificationSubscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                subscription.IsActive = true;
                subscription.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task DeactivateSubscriptionsAsync(List<PushNotificationSubscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                subscription.IsActive = false;
                subscription.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task DeleteSubscriptionsAsync(List<PushNotificationSubscription> subscriptions)
        {
            _context.PushNotificationSubscriptions.RemoveRange(subscriptions);
        }

        private async Task<int> UpdateSubscriptionsAsync(List<PushNotificationSubscription> subscriptions, UpdatePushNotificationSubscriptionDto updateData)
        {
            var updated = 0;
            foreach (var subscription in subscriptions)
            {
                if (updateData.DeviceName != null)
                    subscription.DeviceName = updateData.DeviceName;

                if (updateData.IsActive.HasValue)
                    subscription.IsActive = updateData.IsActive.Value;

                if (updateData.EnabledForReports.HasValue)
                    subscription.EnabledForReports = updateData.EnabledForReports.Value;

                if (updateData.EnabledForApprovals.HasValue)
                    subscription.EnabledForApprovals = updateData.EnabledForApprovals.Value;

                if (updateData.EnabledForDeadlines.HasValue)
                    subscription.EnabledForDeadlines = updateData.EnabledForDeadlines.Value;

                if (updateData.EnabledForAnnouncements.HasValue)
                    subscription.EnabledForAnnouncements = updateData.EnabledForAnnouncements.Value;

                if (updateData.EnabledForMentions.HasValue)
                    subscription.EnabledForMentions = updateData.EnabledForMentions.Value;

                if (updateData.EnabledForReminders.HasValue)
                    subscription.EnabledForReminders = updateData.EnabledForReminders.Value;

                if (!string.IsNullOrEmpty(updateData.MinimumPriority))
                    subscription.MinimumPriority = updateData.MinimumPriority;

                subscription.UpdatedAt = DateTime.UtcNow;
                updated++;
            }
            return updated;
        }

        private async Task<PushNotificationDeliveryDto> SendTestToSubscriptionsAsync(List<PushNotificationSubscription> subscriptions, TestPushNotificationDto testDto)
        {
            var sendDto = new SendPushNotificationDto
            {
                Title = testDto.Title,
                Body = testDto.Body,
                Icon = testDto.Icon,
                Url = testDto.Url,
                Tag = "bulk-test",
                Priority = "High",
                NotificationType = "Test",
                Data = new Dictionary<string, object> { { "test", true }, { "bulk", true } }
            };

            var payload = CreateNotificationPayload(sendDto);
            var deliveryTasks = subscriptions.Select(sub => SendToSubscriptionAsync(sub, payload)).ToArray();
            var deliveryResults = await Task.WhenAll(deliveryTasks);

            return new PushNotificationDeliveryDto
            {
                NotificationId = Guid.NewGuid(),
                Title = testDto.Title,
                Body = testDto.Body,
                TotalTargeted = subscriptions.Count,
                SuccessfulDeliveries = deliveryResults.Count(r => r.Success),
                FailedDeliveries = deliveryResults.Count(r => !r.Success),
                SuccessRate = subscriptions.Count > 0 ? (double)deliveryResults.Count(r => r.Success) / subscriptions.Count * 100 : 0,
                Errors = deliveryResults.Where(r => !r.Success).Select(r => r.Error).Where(e => !string.IsNullOrEmpty(e)).ToList(),
                SentAt = DateTime.UtcNow
            };
        }

        #endregion
    }

    #region Helper Classes

    public class DeliveryResult
    {
        public bool Success { get; set; }
        public Guid SubscriptionId { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class BulkOperationResult
    {
        public string OperationType { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int FoundItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public List<string> Errors { get; set; } = new();
        public double SuccessRate => TotalItems > 0 ? (double)SuccessfulItems / TotalItems * 100 : 0;
    }

    #endregion
}
