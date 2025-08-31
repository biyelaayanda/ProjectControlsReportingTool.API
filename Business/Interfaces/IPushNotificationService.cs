using ProjectControlsReportingTool.API.Models.DTOs;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Interface for push notification management services
    /// </summary>
    public interface IPushNotificationService
    {
        #region Subscription Management

        /// <summary>
        /// Creates a new push notification subscription for a user
        /// </summary>
        /// <param name="userId">The user ID to create the subscription for</param>
        /// <param name="dto">The subscription creation data</param>
        /// <returns>The created subscription DTO</returns>
        Task<PushNotificationSubscriptionDto> CreateSubscriptionAsync(Guid userId, CreatePushNotificationSubscriptionDto dto);

        /// <summary>
        /// Updates an existing push notification subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription ID to update</param>
        /// <param name="dto">The subscription update data</param>
        /// <returns>The updated subscription DTO</returns>
        Task<PushNotificationSubscriptionDto> UpdateSubscriptionAsync(Guid subscriptionId, UpdatePushNotificationSubscriptionDto dto);

        /// <summary>
        /// Deletes a push notification subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription ID to delete</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        Task<bool> DeleteSubscriptionAsync(Guid subscriptionId);

        /// <summary>
        /// Gets all push notification subscriptions for a user
        /// </summary>
        /// <param name="userId">The user ID to get subscriptions for</param>
        /// <returns>List of subscription DTOs</returns>
        Task<List<PushNotificationSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId);

        /// <summary>
        /// Gets a specific push notification subscription by ID
        /// </summary>
        /// <param name="subscriptionId">The subscription ID to retrieve</param>
        /// <returns>The subscription DTO or null if not found</returns>
        Task<PushNotificationSubscriptionDto?> GetSubscriptionAsync(Guid subscriptionId);

        #endregion

        #region Push Notification Sending

        /// <summary>
        /// Sends a push notification to targeted users
        /// </summary>
        /// <param name="dto">The notification data and targeting criteria</param>
        /// <returns>Delivery result with statistics</returns>
        Task<PushNotificationDeliveryDto> SendNotificationAsync(SendPushNotificationDto dto);

        /// <summary>
        /// Sends a test push notification
        /// </summary>
        /// <param name="dto">The test notification data</param>
        /// <returns>Delivery result with statistics</returns>
        Task<PushNotificationDeliveryDto> SendTestNotificationAsync(TestPushNotificationDto dto);

        #endregion

        #region Search and Statistics

        /// <summary>
        /// Searches for push notification subscriptions with filtering and pagination
        /// </summary>
        /// <param name="searchDto">Search criteria and pagination parameters</param>
        /// <returns>Tuple of subscription list and total count</returns>
        Task<(List<PushNotificationSubscriptionDto> Subscriptions, int TotalCount)> SearchSubscriptionsAsync(PushNotificationSubscriptionSearchDto searchDto);

        /// <summary>
        /// Gets push notification subscription statistics
        /// </summary>
        /// <returns>Statistics DTO with counts and success rates</returns>
        Task<PushNotificationSubscriptionStatsDto> GetSubscriptionStatsAsync();

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Processes bulk operations on multiple subscriptions
        /// </summary>
        /// <param name="dto">Bulk operation data including subscription IDs and operation type</param>
        /// <returns>Bulk operation result with success/failure counts</returns>
        Task<Business.Services.BulkOperationResult> ProcessBulkOperationAsync(BulkPushNotificationOperationDto dto);

        #endregion
    }
}
