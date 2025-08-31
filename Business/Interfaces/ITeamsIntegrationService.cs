using ProjectControlsReportingTool.API.Business.Models;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Interface for Microsoft Teams integration service
    /// Provides comprehensive Teams messaging and webhook management
    /// </summary>
    public interface ITeamsIntegrationService
    {
        #region Message Management

        /// <summary>
        /// Sends a message to Microsoft Teams through webhook
        /// </summary>
        /// <param name="messageDto">Teams message details</param>
        /// <param name="userId">ID of the user sending the message</param>
        /// <returns>Teams message response</returns>
        Task<TeamsMessageResponseDto> SendMessageAsync(SendTeamsMessageDto messageDto, Guid userId);

        /// <summary>
        /// Sends bulk messages to multiple Teams webhooks
        /// </summary>
        /// <param name="bulkMessageDto">Bulk message details</param>
        /// <param name="userId">ID of the user sending the messages</param>
        /// <returns>Bulk operation result</returns>
        Task<BulkTeamsMessageResultDto> SendBulkMessageAsync(BulkTeamsMessageDto bulkMessageDto, Guid userId);

        /// <summary>
        /// Sends notification through Teams integration
        /// </summary>
        /// <param name="notificationType">Type of notification</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="userId">Target user ID</param>
        /// <param name="reportId">Related report ID (optional)</param>
        /// <param name="metadata">Additional metadata (optional)</param>
        /// <returns>Teams message response</returns>
        Task<TeamsMessageResponseDto> SendNotificationAsync(
            NotificationType notificationType,
            string title,
            string message,
            Guid userId,
            Guid? reportId = null,
            Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Gets Teams message history
        /// </summary>
        /// <param name="userId">User ID (null for all users, admin only)</param>
        /// <param name="webhookUrl">Filter by webhook URL (optional)</param>
        /// <param name="notificationType">Filter by notification type (optional)</param>
        /// <param name="startDate">Start date filter (optional)</param>
        /// <param name="endDate">End date filter (optional)</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>Paginated Teams message history</returns>
        Task<PagedResultDto<TeamsMessageResponseDto>> GetMessageHistoryAsync(
            Guid? userId = null,
            string? webhookUrl = null,
            NotificationType? notificationType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 20);

        #endregion

        #region Webhook Configuration

        /// <summary>
        /// Creates a new Teams webhook configuration
        /// </summary>
        /// <param name="configDto">Webhook configuration details</param>
        /// <param name="userId">ID of the user creating the configuration</param>
        /// <returns>Created webhook configuration</returns>
        Task<TeamsWebhookConfigDto> CreateWebhookConfigAsync(TeamsWebhookConfigDto configDto, Guid userId);

        /// <summary>
        /// Updates an existing Teams webhook configuration
        /// </summary>
        /// <param name="configId">Configuration ID</param>
        /// <param name="configDto">Updated configuration details</param>
        /// <param name="userId">ID of the user updating the configuration</param>
        /// <returns>Updated webhook configuration</returns>
        Task<TeamsWebhookConfigDto> UpdateWebhookConfigAsync(Guid configId, TeamsWebhookConfigDto configDto, Guid userId);

        /// <summary>
        /// Deletes a Teams webhook configuration
        /// </summary>
        /// <param name="configId">Configuration ID</param>
        /// <param name="userId">ID of the user deleting the configuration</param>
        /// <returns>Service operation result</returns>
        Task<ServiceResultDto> DeleteWebhookConfigAsync(Guid configId, Guid userId);

        /// <summary>
        /// Gets Teams webhook configurations for a user
        /// </summary>
        /// <param name="userId">User ID (null for all users, admin only)</param>
        /// <returns>List of webhook configurations</returns>
        Task<List<TeamsWebhookConfigDto>> GetWebhookConfigsAsync(Guid? userId = null);

        /// <summary>
        /// Gets a specific Teams webhook configuration
        /// </summary>
        /// <param name="configId">Configuration ID</param>
        /// <param name="userId">User ID for authorization</param>
        /// <returns>Webhook configuration</returns>
        Task<TeamsWebhookConfigDto?> GetWebhookConfigByIdAsync(Guid configId, Guid userId);

        #endregion

        #region Template Management

        /// <summary>
        /// Creates a new Teams notification template
        /// </summary>
        /// <param name="templateDto">Template details</param>
        /// <param name="userId">ID of the user creating the template</param>
        /// <returns>Created template</returns>
        Task<TeamsNotificationTemplateDto> CreateTemplateAsync(CreateTeamsTemplateDto templateDto, Guid userId);

        /// <summary>
        /// Updates an existing Teams notification template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="templateDto">Updated template details</param>
        /// <param name="userId">ID of the user updating the template</param>
        /// <returns>Updated template</returns>
        Task<TeamsNotificationTemplateDto> UpdateTemplateAsync(Guid templateId, CreateTeamsTemplateDto templateDto, Guid userId);

        /// <summary>
        /// Deletes a Teams notification template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="userId">ID of the user deleting the template</param>
        /// <returns>Service operation result</returns>
        Task<ServiceResultDto> DeleteTemplateAsync(Guid templateId, Guid userId);

        /// <summary>
        /// Gets Teams notification templates
        /// </summary>
        /// <param name="userId">User ID (null for all users, admin only)</param>
        /// <param name="notificationType">Filter by notification type (optional)</param>
        /// <param name="isActive">Filter by active status (optional)</param>
        /// <returns>List of notification templates</returns>
        Task<List<TeamsNotificationTemplateDto>> GetTemplatesAsync(
            Guid? userId = null,
            NotificationType? notificationType = null,
            bool? isActive = null);

        /// <summary>
        /// Gets a specific Teams notification template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="userId">User ID for authorization</param>
        /// <returns>Notification template</returns>
        Task<TeamsNotificationTemplateDto?> GetTemplateByIdAsync(Guid templateId, Guid userId);

        /// <summary>
        /// Renders a Teams template with provided data
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="data">Data for template substitution</param>
        /// <param name="userId">User ID for authorization</param>
        /// <returns>Rendered Teams message</returns>
        Task<SendTeamsMessageDto> RenderTemplateAsync(Guid templateId, Dictionary<string, object> data, Guid userId);

        #endregion

        #region Testing and Validation

        /// <summary>
        /// Tests a Teams webhook endpoint
        /// </summary>
        /// <param name="testDto">Test configuration</param>
        /// <param name="userId">ID of the user performing the test</param>
        /// <returns>Test result</returns>
        Task<TeamsTestResultDto> TestWebhookAsync(TeamsIntegrationTestDto testDto, Guid userId);

        /// <summary>
        /// Validates a Teams webhook URL
        /// </summary>
        /// <param name="webhookUrl">Webhook URL to validate</param>
        /// <returns>Validation result</returns>
        Task<ServiceResultDto> ValidateWebhookUrlAsync(string webhookUrl);

        /// <summary>
        /// Tests all configured Teams webhooks for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of test results</returns>
        Task<List<TeamsTestResultDto>> TestAllWebhooksAsync(Guid userId);

        #endregion

        #region Analytics and Statistics

        /// <summary>
        /// Gets Teams integration statistics
        /// </summary>
        /// <param name="userId">User ID (null for all users, admin only)</param>
        /// <param name="webhookUrl">Filter by webhook URL (optional)</param>
        /// <param name="startDate">Start date for statistics (optional)</param>
        /// <param name="endDate">End date for statistics (optional)</param>
        /// <returns>Teams integration statistics</returns>
        Task<TeamsIntegrationStatsDto> GetIntegrationStatsAsync(
            Guid? userId = null,
            string? webhookUrl = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Gets Teams delivery failure information
        /// </summary>
        /// <param name="userId">User ID (null for all users, admin only)</param>
        /// <param name="webhookUrl">Filter by webhook URL (optional)</param>
        /// <param name="isResolved">Filter by resolution status (optional)</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>Paginated delivery failures</returns>
        Task<PagedResultDto<Data.Entities.TeamsDeliveryFailure>> GetDeliveryFailuresAsync(
            Guid? userId = null,
            string? webhookUrl = null,
            bool? isResolved = null,
            int pageNumber = 1,
            int pageSize = 20);

        /// <summary>
        /// Retries failed Teams message deliveries
        /// </summary>
        /// <param name="failureIds">List of failure IDs to retry</param>
        /// <param name="userId">User ID for authorization</param>
        /// <returns>Retry operation result</returns>
        Task<ServiceResultDto> RetryFailedDeliveriesAsync(List<Guid> failureIds, Guid userId);

        #endregion

        #region Background Services

        /// <summary>
        /// Processes pending Teams messages (background service method)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Processing result</returns>
        Task<ServiceResultDto> ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleans up old Teams messages and statistics (background service method)
        /// </summary>
        /// <param name="retentionDays">Number of days to retain data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cleanup result</returns>
        Task<ServiceResultDto> CleanupOldDataAsync(int retentionDays = 90, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates Teams integration statistics (background service method)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Update result</returns>
        Task<ServiceResultDto> UpdateStatisticsAsync(CancellationToken cancellationToken = default);

        #endregion
    }
}
