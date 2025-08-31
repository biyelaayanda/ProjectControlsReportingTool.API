using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Repositories
{
    /// <summary>
    /// Repository interface for user notification preferences data access
    /// Provides efficient querying and caching for notification preferences
    /// </summary>
    public interface IUserNotificationPreferenceRepository
    {
        #region Basic CRUD Operations

        /// <summary>
        /// Get all notification preferences for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user notification preferences</returns>
        Task<List<UserNotificationPreference>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Get a specific notification preference by user and type
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Notification type</param>
        /// <returns>User notification preference or null</returns>
        Task<UserNotificationPreference?> GetByUserAndTypeAsync(Guid userId, string notificationType);

        /// <summary>
        /// Get notification preference by ID
        /// </summary>
        /// <param name="id">Preference ID</param>
        /// <returns>User notification preference or null</returns>
        Task<UserNotificationPreference?> GetByIdAsync(Guid id);

        /// <summary>
        /// Create a new notification preference
        /// </summary>
        /// <param name="preference">Notification preference to create</param>
        /// <returns>Created preference</returns>
        Task<UserNotificationPreference> CreateAsync(UserNotificationPreference preference);

        /// <summary>
        /// Update an existing notification preference
        /// </summary>
        /// <param name="preference">Notification preference to update</param>
        /// <returns>Updated preference</returns>
        Task<UserNotificationPreference> UpdateAsync(UserNotificationPreference preference);

        /// <summary>
        /// Delete a notification preference
        /// </summary>
        /// <param name="preference">Notification preference to delete</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteAsync(UserNotificationPreference preference);

        /// <summary>
        /// Delete notification preference by ID
        /// </summary>
        /// <param name="id">Preference ID</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteByIdAsync(Guid id);

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Create multiple notification preferences
        /// </summary>
        /// <param name="preferences">List of preferences to create</param>
        /// <returns>Number of created preferences</returns>
        Task<int> CreateBulkAsync(List<UserNotificationPreference> preferences);

        /// <summary>
        /// Update multiple notification preferences
        /// </summary>
        /// <param name="preferences">List of preferences to update</param>
        /// <returns>Number of updated preferences</returns>
        Task<int> UpdateBulkAsync(List<UserNotificationPreference> preferences);

        /// <summary>
        /// Delete all preferences for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of deleted preferences</returns>
        Task<int> DeleteByUserIdAsync(Guid userId);

        /// <summary>
        /// Delete multiple preferences by IDs
        /// </summary>
        /// <param name="ids">List of preference IDs</param>
        /// <returns>Number of deleted preferences</returns>
        Task<int> DeleteByIdsAsync(List<Guid> ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get users who have specific notification type enabled for a channel
        /// </summary>
        /// <param name="notificationType">Notification type</param>
        /// <param name="channel">Notification channel (Email, RealTime, Push, Sms)</param>
        /// <returns>List of user IDs</returns>
        Task<List<Guid>> GetUsersWithChannelEnabledAsync(string notificationType, string channel);

        /// <summary>
        /// Get users with minimum priority threshold for notification type
        /// </summary>
        /// <param name="notificationType">Notification type</param>
        /// <param name="priority">Message priority to check</param>
        /// <returns>List of user IDs</returns>
        Task<List<Guid>> GetUsersWithPriorityThresholdAsync(string notificationType, string priority);

        /// <summary>
        /// Get users currently in quiet hours
        /// </summary>
        /// <param name="currentTime">Current time to check (optional, defaults to UTC now)</param>
        /// <returns>List of user IDs in quiet hours</returns>
        Task<List<Guid>> GetUsersInQuietHoursAsync(DateTime? currentTime = null);

        /// <summary>
        /// Get users with specific schedule preference
        /// </summary>
        /// <param name="schedule">Schedule type (Always, BusinessHours, Custom)</param>
        /// <returns>List of user IDs</returns>
        Task<List<Guid>> GetUsersByScheduleAsync(string schedule);

        /// <summary>
        /// Check if user exists in preferences (has any preferences set)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user has preferences</returns>
        Task<bool> UserHasPreferencesAsync(Guid userId);

        /// <summary>
        /// Get notification types configured for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of notification types</returns>
        Task<List<string>> GetConfiguredNotificationTypesAsync(Guid userId);

        #endregion

        #region Statistics & Analytics

        /// <summary>
        /// Get total count of notification preferences
        /// </summary>
        /// <returns>Total preference count</returns>
        Task<int> GetTotalPreferencesCountAsync();

        /// <summary>
        /// Get count of preferences by notification type
        /// </summary>
        /// <returns>Dictionary of notification type to count</returns>
        Task<Dictionary<string, int>> GetPreferencesCountByTypeAsync();

        /// <summary>
        /// Get count of users with each channel enabled
        /// </summary>
        /// <returns>Dictionary of channel to enabled user count</returns>
        Task<Dictionary<string, int>> GetChannelEnabledCountsAsync();

        /// <summary>
        /// Get most common priority settings
        /// </summary>
        /// <returns>Dictionary of priority to user count</returns>
        Task<Dictionary<string, int>> GetPriorityDistributionAsync();

        /// <summary>
        /// Get users with quiet hours configured
        /// </summary>
        /// <returns>Count of users with quiet hours</returns>
        Task<int> GetUsersWithQuietHoursCountAsync();

        /// <summary>
        /// Get timezone distribution of users
        /// </summary>
        /// <returns>Dictionary of timezone to user count</returns>
        Task<Dictionary<string, int>> GetTimezoneDistributionAsync();

        #endregion

        #region Caching Operations

        /// <summary>
        /// Get cached user preferences (if caching is implemented)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Cached preferences or null if not cached</returns>
        Task<List<UserNotificationPreference>?> GetCachedUserPreferencesAsync(Guid userId);

        /// <summary>
        /// Cache user preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferences">Preferences to cache</param>
        /// <param name="expiration">Cache expiration time</param>
        /// <returns>Success status</returns>
        Task<bool> CacheUserPreferencesAsync(Guid userId, List<UserNotificationPreference> preferences, TimeSpan? expiration = null);

        /// <summary>
        /// Invalidate cached user preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success status</returns>
        Task<bool> InvalidateUserPreferencesCacheAsync(Guid userId);

        /// <summary>
        /// Clear all preference caches
        /// </summary>
        /// <returns>Success status</returns>
        Task<bool> ClearAllPreferenceCachesAsync();

        #endregion
    }
}
