using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Service for managing user notification preferences
    /// Handles user-specific notification settings and delivery logic
    /// </summary>
    public class UserNotificationPreferenceService : IUserNotificationPreferenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserNotificationPreferenceService> _logger;

        // Default notification types and their configurations
        private static readonly Dictionary<string, NotificationTypeInfoDto> DefaultNotificationTypes = new()
        {
            ["ReportGenerated"] = new()
            {
                Type = "ReportGenerated",
                DisplayName = "Report Generated",
                Description = "Notification when a report generation is completed",
                Category = "Reports",
                DefaultEmailEnabled = true,
                DefaultRealTimeEnabled = true,
                DefaultPushEnabled = false,
                DefaultPriority = "Medium"
            },
            ["ReportFailed"] = new()
            {
                Type = "ReportFailed",
                DisplayName = "Report Generation Failed",
                Description = "Notification when a report generation fails",
                Category = "Reports",
                DefaultEmailEnabled = true,
                DefaultRealTimeEnabled = true,
                DefaultPushEnabled = true,
                DefaultPriority = "High"
            },
            ["SystemMaintenance"] = new()
            {
                Type = "SystemMaintenance",
                DisplayName = "System Maintenance",
                Description = "Notification about scheduled system maintenance",
                Category = "System",
                DefaultEmailEnabled = true,
                DefaultRealTimeEnabled = false,
                DefaultPushEnabled = false,
                DefaultPriority = "Low"
            },
            ["SecurityAlert"] = new()
            {
                Type = "SecurityAlert",
                DisplayName = "Security Alert",
                Description = "Critical security notifications",
                Category = "Security",
                DefaultEmailEnabled = true,
                DefaultRealTimeEnabled = true,
                DefaultPushEnabled = true,
                DefaultPriority = "Critical"
            },
            ["UserAccountUpdate"] = new()
            {
                Type = "UserAccountUpdate",
                DisplayName = "Account Updates",
                Description = "Notifications about account changes",
                Category = "Account",
                DefaultEmailEnabled = true,
                DefaultRealTimeEnabled = true,
                DefaultPushEnabled = false,
                DefaultPriority = "Medium"
            },
            ["DataExport"] = new()
            {
                Type = "DataExport",
                DisplayName = "Data Export",
                Description = "Notification when data export is completed",
                Category = "Data",
                DefaultEmailEnabled = true,
                DefaultRealTimeEnabled = true,
                DefaultPushEnabled = false,
                DefaultPriority = "Medium"
            }
        };

        public UserNotificationPreferenceService(
            ApplicationDbContext context,
            ILogger<UserNotificationPreferenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Preference Management

        public async Task<List<UserNotificationPreferenceDto>> GetUserPreferencesAsync(Guid userId)
        {
            try
            {
                var preferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.NotificationType)
                    .Select(p => new UserNotificationPreferenceDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        NotificationType = p.NotificationType,
                        EmailEnabled = p.EmailEnabled,
                        RealTimeEnabled = p.RealTimeEnabled,
                        PushEnabled = p.PushEnabled,
                        SmsEnabled = p.SmsEnabled,
                        Schedule = p.Schedule,
                        MinimumPriority = p.MinimumPriority,
                        QuietHoursStart = p.QuietHoursStart,
                        QuietHoursEnd = p.QuietHoursEnd,
                        TimeZone = p.TimeZone,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} notification preferences for user {UserId}", 
                    preferences.Count, userId);

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preferences for user {UserId}", userId);
                throw new InvalidOperationException($"Failed to retrieve notification preferences for user {userId}", ex);
            }
        }

        public async Task<UserNotificationPreferenceDto?> GetUserPreferenceAsync(Guid userId, string notificationType)
        {
            try
            {
                var preference = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId && p.NotificationType == notificationType)
                    .Select(p => new UserNotificationPreferenceDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        NotificationType = p.NotificationType,
                        EmailEnabled = p.EmailEnabled,
                        RealTimeEnabled = p.RealTimeEnabled,
                        PushEnabled = p.PushEnabled,
                        SmsEnabled = p.SmsEnabled,
                        Schedule = p.Schedule,
                        MinimumPriority = p.MinimumPriority,
                        QuietHoursStart = p.QuietHoursStart,
                        QuietHoursEnd = p.QuietHoursEnd,
                        TimeZone = p.TimeZone,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                return preference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preference for user {UserId}, type {Type}", 
                    userId, notificationType);
                throw;
            }
        }

        public async Task<UserNotificationPreferenceDto> SetUserPreferenceAsync(Guid userId, CreateUserNotificationPreferenceDto createDto)
        {
            try
            {
                // Check if preference already exists
                var existingPreference = await _context.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == createDto.NotificationType);

                UserNotificationPreference preference;

                if (existingPreference != null)
                {
                    // Update existing preference
                    existingPreference.EmailEnabled = createDto.EmailEnabled;
                    existingPreference.RealTimeEnabled = createDto.RealTimeEnabled;
                    existingPreference.PushEnabled = createDto.PushEnabled;
                    existingPreference.SmsEnabled = createDto.SmsEnabled;
                    existingPreference.Schedule = createDto.Schedule;
                    existingPreference.MinimumPriority = createDto.MinimumPriority;
                    existingPreference.QuietHoursStart = createDto.QuietHoursStart;
                    existingPreference.QuietHoursEnd = createDto.QuietHoursEnd;
                    existingPreference.TimeZone = createDto.TimeZone;
                    existingPreference.UpdatedAt = DateTime.UtcNow;

                    preference = existingPreference;
                }
                else
                {
                    // Create new preference
                    preference = new UserNotificationPreference
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        NotificationType = createDto.NotificationType,
                        EmailEnabled = createDto.EmailEnabled,
                        RealTimeEnabled = createDto.RealTimeEnabled,
                        PushEnabled = createDto.PushEnabled,
                        SmsEnabled = createDto.SmsEnabled,
                        Schedule = createDto.Schedule,
                        MinimumPriority = createDto.MinimumPriority,
                        QuietHoursStart = createDto.QuietHoursStart,
                        QuietHoursEnd = createDto.QuietHoursEnd,
                        TimeZone = createDto.TimeZone,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserNotificationPreferences.Add(preference);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Set notification preference for user {UserId}, type {Type}", 
                    userId, createDto.NotificationType);

                return new UserNotificationPreferenceDto
                {
                    Id = preference.Id,
                    UserId = preference.UserId,
                    NotificationType = preference.NotificationType,
                    EmailEnabled = preference.EmailEnabled,
                    RealTimeEnabled = preference.RealTimeEnabled,
                    PushEnabled = preference.PushEnabled,
                    SmsEnabled = preference.SmsEnabled,
                    Schedule = preference.Schedule,
                    MinimumPriority = preference.MinimumPriority,
                    QuietHoursStart = preference.QuietHoursStart,
                    QuietHoursEnd = preference.QuietHoursEnd,
                    TimeZone = preference.TimeZone,
                    CreatedAt = preference.CreatedAt,
                    UpdatedAt = preference.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting notification preference for user {UserId}, type {Type}", 
                    userId, createDto.NotificationType);
                throw new InvalidOperationException($"Failed to set notification preference", ex);
            }
        }

        public async Task<UserNotificationPreferenceDto> UpdateUserPreferenceAsync(Guid userId, string notificationType, UpdateUserNotificationPreferenceDto updateDto)
        {
            try
            {
                var preference = await _context.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);

                if (preference == null)
                {
                    throw new InvalidOperationException($"Notification preference not found for user {userId} and type {notificationType}");
                }

                // Update only provided fields
                if (updateDto.EmailEnabled.HasValue)
                    preference.EmailEnabled = updateDto.EmailEnabled.Value;
                
                if (updateDto.RealTimeEnabled.HasValue)
                    preference.RealTimeEnabled = updateDto.RealTimeEnabled.Value;
                
                if (updateDto.PushEnabled.HasValue)
                    preference.PushEnabled = updateDto.PushEnabled.Value;
                
                if (updateDto.SmsEnabled.HasValue)
                    preference.SmsEnabled = updateDto.SmsEnabled.Value;
                
                if (updateDto.Schedule != null)
                    preference.Schedule = updateDto.Schedule;
                
                if (updateDto.MinimumPriority != null)
                    preference.MinimumPriority = updateDto.MinimumPriority;
                
                if (updateDto.QuietHoursStart != null)
                    preference.QuietHoursStart = updateDto.QuietHoursStart;
                
                if (updateDto.QuietHoursEnd != null)
                    preference.QuietHoursEnd = updateDto.QuietHoursEnd;
                
                if (updateDto.TimeZone != null)
                    preference.TimeZone = updateDto.TimeZone;

                preference.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated notification preference for user {UserId}, type {Type}", 
                    userId, notificationType);

                return new UserNotificationPreferenceDto
                {
                    Id = preference.Id,
                    UserId = preference.UserId,
                    NotificationType = preference.NotificationType,
                    EmailEnabled = preference.EmailEnabled,
                    RealTimeEnabled = preference.RealTimeEnabled,
                    PushEnabled = preference.PushEnabled,
                    SmsEnabled = preference.SmsEnabled,
                    Schedule = preference.Schedule,
                    MinimumPriority = preference.MinimumPriority,
                    QuietHoursStart = preference.QuietHoursStart,
                    QuietHoursEnd = preference.QuietHoursEnd,
                    TimeZone = preference.TimeZone,
                    CreatedAt = preference.CreatedAt,
                    UpdatedAt = preference.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preference for user {UserId}, type {Type}", 
                    userId, notificationType);
                throw;
            }
        }

        public async Task<bool> DeleteUserPreferenceAsync(Guid userId, string notificationType)
        {
            try
            {
                var preference = await _context.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);

                if (preference == null)
                {
                    return false;
                }

                _context.UserNotificationPreferences.Remove(preference);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted notification preference for user {UserId}, type {Type}", 
                    userId, notificationType);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification preference for user {UserId}, type {Type}", 
                    userId, notificationType);
                throw new InvalidOperationException($"Failed to delete notification preference", ex);
            }
        }

        public async Task<int> BulkUpdatePreferencesAsync(Guid userId, BulkNotificationPreferenceUpdateDto bulkUpdateDto)
        {
            try
            {
                var preferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                int updatedCount = 0;

                foreach (var preference in preferences)
                {
                    bool hasUpdates = false;

                    // Apply bulk updates
                    if (bulkUpdateDto.EmailEnabled.HasValue)
                    {
                        preference.EmailEnabled = bulkUpdateDto.EmailEnabled.Value;
                        hasUpdates = true;
                    }

                    if (bulkUpdateDto.RealTimeEnabled.HasValue)
                    {
                        preference.RealTimeEnabled = bulkUpdateDto.RealTimeEnabled.Value;
                        hasUpdates = true;
                    }

                    if (bulkUpdateDto.PushEnabled.HasValue)
                    {
                        preference.PushEnabled = bulkUpdateDto.PushEnabled.Value;
                        hasUpdates = true;
                    }

                    if (bulkUpdateDto.SmsEnabled.HasValue)
                    {
                        preference.SmsEnabled = bulkUpdateDto.SmsEnabled.Value;
                        hasUpdates = true;
                    }

                    if (bulkUpdateDto.MinimumPriority != null)
                    {
                        preference.MinimumPriority = bulkUpdateDto.MinimumPriority;
                        hasUpdates = true;
                    }

                    if (bulkUpdateDto.QuietHoursStart != null)
                    {
                        preference.QuietHoursStart = bulkUpdateDto.QuietHoursStart;
                        hasUpdates = true;
                    }

                    if (bulkUpdateDto.QuietHoursEnd != null)
                    {
                        preference.QuietHoursEnd = bulkUpdateDto.QuietHoursEnd;
                        hasUpdates = true;
                    }

                    if (bulkUpdateDto.TimeZone != null)
                    {
                        preference.TimeZone = bulkUpdateDto.TimeZone;
                        hasUpdates = true;
                    }

                    if (hasUpdates)
                    {
                        preference.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk updated {Count} notification preferences for user {UserId}", 
                    updatedCount, userId);

                return updatedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating notification preferences for user {UserId}", userId);
                throw new InvalidOperationException($"Failed to bulk update notification preferences", ex);
            }
        }

        #endregion

        #region Default Preferences

        public async Task<int> InitializeDefaultPreferencesAsync(Guid userId)
        {
            try
            {
                var existingPreferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .Select(p => p.NotificationType)
                    .ToListAsync();

                var newPreferences = new List<UserNotificationPreference>();

                foreach (var notificationTypeInfo in DefaultNotificationTypes.Values)
                {
                    if (!existingPreferences.Contains(notificationTypeInfo.Type))
                    {
                        newPreferences.Add(new UserNotificationPreference
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            NotificationType = notificationTypeInfo.Type,
                            EmailEnabled = notificationTypeInfo.DefaultEmailEnabled,
                            RealTimeEnabled = notificationTypeInfo.DefaultRealTimeEnabled,
                            PushEnabled = notificationTypeInfo.DefaultPushEnabled,
                            SmsEnabled = false, // SMS disabled by default
                            Schedule = "Always",
                            MinimumPriority = notificationTypeInfo.DefaultPriority,
                            QuietHoursStart = null,
                            QuietHoursEnd = null,
                            TimeZone = "UTC",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                if (newPreferences.Any())
                {
                    _context.UserNotificationPreferences.AddRange(newPreferences);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Initialized {Count} default notification preferences for user {UserId}", 
                    newPreferences.Count, userId);

                return newPreferences.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default notification preferences for user {UserId}", userId);
                throw new InvalidOperationException($"Failed to initialize default notification preferences", ex);
            }
        }

        public async Task<int> ResetToDefaultsAsync(Guid userId)
        {
            try
            {
                // Remove existing preferences
                var existingPreferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                _context.UserNotificationPreferences.RemoveRange(existingPreferences);

                // Create default preferences
                var defaultPreferences = DefaultNotificationTypes.Values.Select(notificationTypeInfo =>
                    new UserNotificationPreference
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        NotificationType = notificationTypeInfo.Type,
                        EmailEnabled = notificationTypeInfo.DefaultEmailEnabled,
                        RealTimeEnabled = notificationTypeInfo.DefaultRealTimeEnabled,
                        PushEnabled = notificationTypeInfo.DefaultPushEnabled,
                        SmsEnabled = false, // SMS disabled by default
                        Schedule = "Always",
                        MinimumPriority = notificationTypeInfo.DefaultPriority,
                        QuietHoursStart = null,
                        QuietHoursEnd = null,
                        TimeZone = "UTC",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();

                _context.UserNotificationPreferences.AddRange(defaultPreferences);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reset {Count} notification preferences to defaults for user {UserId}", 
                    defaultPreferences.Count, userId);

                return defaultPreferences.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting notification preferences to defaults for user {UserId}", userId);
                throw new InvalidOperationException($"Failed to reset notification preferences to defaults", ex);
            }
        }

        public async Task<List<NotificationTypeInfoDto>> GetAvailableNotificationTypesAsync()
        {
            return await Task.FromResult(DefaultNotificationTypes.Values.ToList());
        }

        #endregion

        #region Delivery Logic

        public async Task<bool> ShouldSendEmailAsync(Guid userId, string notificationType, string priority)
        {
            try
            {
                var preference = await GetUserPreferenceAsync(userId, notificationType);
                
                if (preference == null)
                {
                    // Use default if no preference exists
                    var defaultInfo = DefaultNotificationTypes.GetValueOrDefault(notificationType);
                    if (defaultInfo == null) return false;
                    
                    return defaultInfo.DefaultEmailEnabled && 
                           ShouldSendBasedOnPriority(priority, defaultInfo.DefaultPriority);
                }

                return preference.EmailEnabled && 
                       ShouldSendBasedOnPriority(priority, preference.MinimumPriority) &&
                       !await IsInQuietHoursAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email delivery for user {UserId}, type {Type}", 
                    userId, notificationType);
                return false; // Fail safely - don't send if unsure
            }
        }

        public async Task<bool> ShouldSendRealTimeAsync(Guid userId, string notificationType, string priority)
        {
            try
            {
                var preference = await GetUserPreferenceAsync(userId, notificationType);
                
                if (preference == null)
                {
                    var defaultInfo = DefaultNotificationTypes.GetValueOrDefault(notificationType);
                    if (defaultInfo == null) return false;
                    
                    return defaultInfo.DefaultRealTimeEnabled && 
                           ShouldSendBasedOnPriority(priority, defaultInfo.DefaultPriority);
                }

                return preference.RealTimeEnabled && 
                       ShouldSendBasedOnPriority(priority, preference.MinimumPriority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking real-time delivery for user {UserId}, type {Type}", 
                    userId, notificationType);
                return false;
            }
        }

        public async Task<bool> ShouldSendPushAsync(Guid userId, string notificationType, string priority)
        {
            try
            {
                var preference = await GetUserPreferenceAsync(userId, notificationType);
                
                if (preference == null)
                {
                    var defaultInfo = DefaultNotificationTypes.GetValueOrDefault(notificationType);
                    if (defaultInfo == null) return false;
                    
                    return defaultInfo.DefaultPushEnabled && 
                           ShouldSendBasedOnPriority(priority, defaultInfo.DefaultPriority);
                }

                return preference.PushEnabled && 
                       ShouldSendBasedOnPriority(priority, preference.MinimumPriority) &&
                       !await IsInQuietHoursAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking push delivery for user {UserId}, type {Type}", 
                    userId, notificationType);
                return false;
            }
        }

        public async Task<NotificationDeliverySummaryDto> GetDeliverySummaryAsync(Guid userId, string notificationType, string priority)
        {
            try
            {
                var isInQuietHours = await IsInQuietHoursAsync(userId);
                
                return new NotificationDeliverySummaryDto
                {
                    UserId = userId,
                    NotificationType = notificationType,
                    Priority = priority,
                    ShouldSendEmail = await ShouldSendEmailAsync(userId, notificationType, priority),
                    ShouldSendRealTime = await ShouldSendRealTimeAsync(userId, notificationType, priority),
                    ShouldSendPush = await ShouldSendPushAsync(userId, notificationType, priority),
                    ShouldSendSms = false, // SMS implementation pending
                    IsInQuietHours = isInQuietHours,
                    CheckedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery summary for user {UserId}, type {Type}", 
                    userId, notificationType);
                throw;
            }
        }

        #endregion

        #region Quiet Hours

        public async Task<bool> IsInQuietHoursAsync(Guid userId, DateTime? currentTime = null)
        {
            try
            {
                var currentUtc = currentTime ?? DateTime.UtcNow;
                
                // Get any preference with quiet hours set (they should all be the same)
                var preference = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId && p.QuietHoursStart != null && p.QuietHoursEnd != null)
                    .FirstOrDefaultAsync();

                if (preference == null || 
                    string.IsNullOrEmpty(preference.QuietHoursStart) || 
                    string.IsNullOrEmpty(preference.QuietHoursEnd))
                {
                    return false; // No quiet hours configured
                }

                // Convert current time to user's timezone
                var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(preference.TimeZone ?? "UTC");
                var userTime = TimeZoneInfo.ConvertTimeFromUtc(currentUtc, userTimeZone);

                // Parse quiet hours
                if (!TimeOnly.TryParse(preference.QuietHoursStart, out var startTime) ||
                    !TimeOnly.TryParse(preference.QuietHoursEnd, out var endTime))
                {
                    return false; // Invalid time format
                }

                var currentTimeOnly = TimeOnly.FromDateTime(userTime);

                // Handle quiet hours that span midnight
                if (startTime <= endTime)
                {
                    return currentTimeOnly >= startTime && currentTimeOnly <= endTime;
                }
                else
                {
                    return currentTimeOnly >= startTime || currentTimeOnly <= endTime;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quiet hours for user {UserId}", userId);
                return false; // Fail safely - assume not in quiet hours
            }
        }

        public async Task<int> UpdateQuietHoursAsync(Guid userId, string? startTime, string? endTime, string timeZone)
        {
            try
            {
                var preferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                foreach (var preference in preferences)
                {
                    preference.QuietHoursStart = startTime;
                    preference.QuietHoursEnd = endTime;
                    preference.TimeZone = timeZone;
                    preference.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated quiet hours for {Count} preferences for user {UserId}", 
                    preferences.Count, userId);

                return preferences.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiet hours for user {UserId}", userId);
                throw new InvalidOperationException($"Failed to update quiet hours", ex);
            }
        }

        #endregion

        #region Statistics & Analytics

        public async Task<NotificationPreferenceStatsDto> GetUserPreferenceStatsAsync(Guid userId)
        {
            try
            {
                var preferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                return new NotificationPreferenceStatsDto
                {
                    TotalPreferences = preferences.Count,
                    EmailEnabledCount = preferences.Count(p => p.EmailEnabled),
                    RealTimeEnabledCount = preferences.Count(p => p.RealTimeEnabled),
                    PushEnabledCount = preferences.Count(p => p.PushEnabled),
                    SmsEnabledCount = preferences.Count(p => p.SmsEnabled),
                    HasQuietHours = preferences.Any(p => !string.IsNullOrEmpty(p.QuietHoursStart)),
                    MostCommonPriority = preferences
                        .GroupBy(p => p.MinimumPriority)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "Medium",
                    LastUpdated = preferences.Max(p => p.UpdatedAt)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preference stats for user {UserId}", userId);
                throw;
            }
        }

        public async Task<NotificationPreferenceStatsDto> GetSystemPreferenceStatsAsync()
        {
            try
            {
                var allPreferences = await _context.UserNotificationPreferences.ToListAsync();

                return new NotificationPreferenceStatsDto
                {
                    TotalPreferences = allPreferences.Count,
                    EmailEnabledCount = allPreferences.Count(p => p.EmailEnabled),
                    RealTimeEnabledCount = allPreferences.Count(p => p.RealTimeEnabled),
                    PushEnabledCount = allPreferences.Count(p => p.PushEnabled),
                    SmsEnabledCount = allPreferences.Count(p => p.SmsEnabled),
                    HasQuietHours = allPreferences.Any(p => !string.IsNullOrEmpty(p.QuietHoursStart)),
                    MostCommonPriority = allPreferences
                        .GroupBy(p => p.MinimumPriority)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "Medium",
                    LastUpdated = allPreferences.Any() ? allPreferences.Max(p => p.UpdatedAt) : DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system preference stats");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private static bool ShouldSendBasedOnPriority(string messagePriority, string minimumPriority)
        {
            var priorityLevels = new Dictionary<string, int>
            {
                ["Low"] = 1,
                ["Medium"] = 2,
                ["High"] = 3,
                ["Critical"] = 4
            };

            var messageLevel = priorityLevels.GetValueOrDefault(messagePriority, 2);
            var minimumLevel = priorityLevels.GetValueOrDefault(minimumPriority, 2);

            return messageLevel >= minimumLevel;
        }

        #endregion
    }
}
