using ProjectControlsReportingTool.API.Business.Models;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Interface for Slack integration service operations
    /// </summary>
    public interface ISlackIntegrationService
    {
        #region Message Operations

        /// <summary>
        /// Send a message to Slack using webhook
        /// </summary>
        Task<ServiceResultDto<SendSlackMessageResponseDto>> SendSlackMessageAsync(SendSlackMessageDto request);

        /// <summary>
        /// Send bulk messages to multiple Slack channels
        /// </summary>
        Task<ServiceResultDto<BulkSlackMessageResponseDto>> SendBulkSlackMessagesAsync(BulkSlackMessageDto request);

        /// <summary>
        /// Send notification message based on templates
        /// </summary>
        Task<ServiceResultDto<SendSlackMessageResponseDto>> SendSlackNotificationAsync(SlackNotificationRequestDto request);

        /// <summary>
        /// Update an existing Slack message
        /// </summary>
        Task<ServiceResultDto<SendSlackMessageResponseDto>> UpdateSlackMessageAsync(UpdateSlackMessageDto request);

        /// <summary>
        /// Delete a Slack message
        /// </summary>
        Task<ServiceResultDto<bool>> DeleteSlackMessageAsync(string messageId, string channel, string webhookId);

        #endregion

        #region Webhook Management

        /// <summary>
        /// Create a new Slack webhook configuration
        /// </summary>
        Task<ServiceResultDto<SlackWebhookConfigDto>> CreateSlackWebhookAsync(CreateSlackWebhookDto request);

        /// <summary>
        /// Update an existing Slack webhook configuration
        /// </summary>
        Task<ServiceResultDto<SlackWebhookConfigDto>> UpdateSlackWebhookAsync(int webhookId, UpdateSlackWebhookDto request);

        /// <summary>
        /// Delete a Slack webhook configuration
        /// </summary>
        Task<ServiceResultDto<bool>> DeleteSlackWebhookAsync(int webhookId);

        /// <summary>
        /// Get all Slack webhook configurations
        /// </summary>
        Task<ServiceResultDto<List<SlackWebhookConfigDto>>> GetSlackWebhooksAsync();

        /// <summary>
        /// Get Slack webhook configuration by ID
        /// </summary>
        Task<ServiceResultDto<SlackWebhookConfigDto>> GetSlackWebhookByIdAsync(int webhookId);

        /// <summary>
        /// Test a Slack webhook connection
        /// </summary>
        Task<ServiceResultDto<SlackTestResponseDto>> TestSlackWebhookAsync(int webhookId);

        /// <summary>
        /// Test a Slack webhook URL directly
        /// </summary>
        Task<ServiceResultDto<SlackTestResponseDto>> TestSlackWebhookUrlAsync(string webhookUrl);

        #endregion

        #region Template Management

        /// <summary>
        /// Create a new Slack notification template
        /// </summary>
        Task<ServiceResultDto<SlackNotificationTemplateDto>> CreateSlackTemplateAsync(CreateSlackTemplateDto request);

        /// <summary>
        /// Update an existing Slack notification template
        /// </summary>
        Task<ServiceResultDto<SlackNotificationTemplateDto>> UpdateSlackTemplateAsync(int templateId, UpdateSlackTemplateDto request);

        /// <summary>
        /// Delete a Slack notification template
        /// </summary>
        Task<ServiceResultDto<bool>> DeleteSlackTemplateAsync(int templateId);

        /// <summary>
        /// Get all Slack notification templates
        /// </summary>
        Task<ServiceResultDto<List<SlackNotificationTemplateDto>>> GetSlackTemplatesAsync();

        /// <summary>
        /// Get Slack notification template by ID
        /// </summary>
        Task<ServiceResultDto<SlackNotificationTemplateDto>> GetSlackTemplateByIdAsync(int templateId);

        /// <summary>
        /// Get Slack notification templates by type
        /// </summary>
        Task<ServiceResultDto<List<SlackNotificationTemplateDto>>> GetSlackTemplatesByTypeAsync(string templateType);

        #endregion

        #region Analytics and Statistics

        /// <summary>
        /// Get Slack integration statistics
        /// </summary>
        Task<ServiceResultDto<SlackIntegrationStatsDto>> GetSlackStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get delivery failure logs
        /// </summary>
        Task<ServiceResultDto<List<SlackDeliveryFailureDto>>> GetSlackDeliveryFailuresAsync(int? days = 30);

        /// <summary>
        /// Get message delivery statistics by channel
        /// </summary>
        Task<ServiceResultDto<List<SlackChannelStatsDto>>> GetSlackChannelStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Retry failed Slack message deliveries
        /// </summary>
        Task<ServiceResultDto<int>> RetryFailedSlackDeliveriesAsync(List<int> failureIds = null);

        #endregion

        #region Channel Management

        /// <summary>
        /// Get available Slack channels for a webhook
        /// </summary>
        Task<ServiceResultDto<List<SlackChannelDto>>> GetSlackChannelsAsync(int webhookId);

        /// <summary>
        /// Validate if a channel exists and is accessible
        /// </summary>
        Task<ServiceResultDto<bool>> ValidateSlackChannelAsync(string channel, int webhookId);

        #endregion

        #region Configuration and Health

        /// <summary>
        /// Get Slack integration configuration
        /// </summary>
        Task<ServiceResultDto<SlackConfigurationDto>> GetSlackConfigurationAsync();

        /// <summary>
        /// Update Slack integration configuration
        /// </summary>
        Task<ServiceResultDto<SlackConfigurationDto>> UpdateSlackConfigurationAsync(UpdateSlackConfigurationDto request);

        /// <summary>
        /// Check health of all Slack webhooks
        /// </summary>
        Task<ServiceResultDto<List<SlackWebhookHealthDto>>> CheckSlackWebhookHealthAsync();

        #endregion
    }
}
