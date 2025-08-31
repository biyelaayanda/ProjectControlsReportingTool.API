using ProjectControlsReportingTool.API.Models.DTOs;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Service interface for managing user notification preferences
    /// Handles user-specific notification settings and delivery preferences
    /// </summary>
    public interface IUserNotificationPreferenceService
    {
        #region Preference Management

        /// <summary>
        /// Get all notification preferences for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user notification preferences</returns>
        Task<List<UserNotificationPreferenceDto>> GetUserPreferencesAsync(Guid userId);

        /// <summary>
        /// Get a specific notification preference for a user and notification type
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Type of notification</param>
        /// <returns>Notification preference or null if not found</returns>
        Task<UserNotificationPreferenceDto?> GetUserPreferenceAsync(Guid userId, string notificationType);

        /// <summary>
        /// Create or update notification preference for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="createDto">Preference creation data</param>
        /// <returns>Created/updated preference</returns>
        Task<UserNotificationPreferenceDto> SetUserPreferenceAsync(Guid userId, CreateUserNotificationPreferenceDto createDto);

        /// <summary>
        /// Update an existing notification preference
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Type of notification</param>
        /// <param name="updateDto">Preference update data</param>
        /// <returns>Updated preference</returns>
        Task<UserNotificationPreferenceDto> UpdateUserPreferenceAsync(Guid userId, string notificationType, UpdateUserNotificationPreferenceDto updateDto);

        /// <summary>
        /// Delete a notification preference
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Type of notification</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteUserPreferenceAsync(Guid userId, string notificationType);

        /// <summary>
        /// Bulk update multiple notification preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="bulkUpdateDto">Bulk update data</param>
        /// <returns>Number of updated preferences</returns>
        Task<int> BulkUpdatePreferencesAsync(Guid userId, BulkNotificationPreferenceUpdateDto bulkUpdateDto);

        #endregion

        #region Default Preferences

        /// <summary>
        /// Initialize default notification preferences for a new user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of default preferences created</returns>
        Task<int> InitializeDefaultPreferencesAsync(Guid userId);

        /// <summary>
        /// Reset user preferences to system defaults
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of preferences reset</returns>
        Task<int> ResetToDefaultsAsync(Guid userId);

        /// <summary>
        /// Get available notification types for preference management
        /// </summary>
        /// <returns>List of notification types with descriptions</returns>
        Task<List<NotificationTypeInfoDto>> GetAvailableNotificationTypesAsync();

        #endregion

        #region Delivery Logic

        /// <summary>
        /// Check if a notification should be delivered via email based on user preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Type of notification</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>True if email should be sent</returns>
        Task<bool> ShouldSendEmailAsync(Guid userId, string notificationType, string priority);

        /// <summary>
        /// Check if a notification should be delivered via real-time WebSocket
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Type of notification</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>True if real-time notification should be sent</returns>
        Task<bool> ShouldSendRealTimeAsync(Guid userId, string notificationType, string priority);

        /// <summary>
        /// Check if a notification should be delivered via push notification
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Type of notification</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>True if push notification should be sent</returns>
        Task<bool> ShouldSendPushAsync(Guid userId, string notificationType, string priority);

        /// <summary>
        /// Get comprehensive delivery summary for a notification
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Type of notification</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>Delivery summary with all channels</returns>
        Task<NotificationDeliverySummaryDto> GetDeliverySummaryAsync(Guid userId, string notificationType, string priority);

        #endregion

        #region Quiet Hours

        /// <summary>
        /// Check if current time falls within user's quiet hours
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="currentTime">Current time (optional, defaults to UTC now)</param>
        /// <returns>True if in quiet hours</returns>
        Task<bool> IsInQuietHoursAsync(Guid userId, DateTime? currentTime = null);

        /// <summary>
        /// Update user's quiet hours settings for all preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="startTime">Quiet hours start time (HH:mm format)</param>
        /// <param name="endTime">Quiet hours end time (HH:mm format)</param>
        /// <param name="timeZone">User's time zone</param>
        /// <returns>Number of preferences updated</returns>
        Task<int> UpdateQuietHoursAsync(Guid userId, string? startTime, string? endTime, string timeZone);

        #endregion

        #region Statistics & Analytics

        /// <summary>
        /// Get notification preference statistics for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Preference statistics</returns>
        Task<NotificationPreferenceStatsDto> GetUserPreferenceStatsAsync(Guid userId);

        /// <summary>
        /// Get system-wide notification preference statistics (Admin only)
        /// </summary>
        /// <returns>System preference statistics</returns>
        Task<NotificationPreferenceStatsDto> GetSystemPreferenceStatsAsync();

        #endregion
    }

    /// <summary>
    /// DTO for notification type information
    /// </summary>
    public class NotificationTypeInfoDto
    {
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool DefaultEmailEnabled { get; set; }
        public bool DefaultRealTimeEnabled { get; set; }
        public bool DefaultPushEnabled { get; set; }
        public string DefaultPriority { get; set; } = string.Empty;
    }
}
