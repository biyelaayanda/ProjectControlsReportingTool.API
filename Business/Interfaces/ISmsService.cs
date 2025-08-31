using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Interface for SMS service operations
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Send a single SMS message
        /// </summary>
        /// <param name="sendSmsDto">SMS details</param>
        /// <param name="userId">Optional user ID for tracking</param>
        /// <returns>Delivery result</returns>
        Task<SmsDeliveryDto> SendSmsAsync(SendSmsDto sendSmsDto, Guid? userId = null);

        /// <summary>
        /// Send SMS using a template
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number</param>
        /// <param name="templateId">Template ID</param>
        /// <param name="variables">Template variables</param>
        /// <param name="userId">Optional user ID for tracking</param>
        /// <returns>Delivery result</returns>
        Task<SmsDeliveryDto> SendSmsFromTemplateAsync(string phoneNumber, Guid templateId, Dictionary<string, object> variables, Guid? userId = null);

        /// <summary>
        /// Send bulk SMS messages
        /// </summary>
        /// <param name="bulkSmsDto">Bulk SMS details</param>
        /// <param name="userId">Optional user ID for tracking</param>
        /// <returns>Bulk delivery result</returns>
        Task<BulkSmsDeliveryDto> SendBulkSmsAsync(BulkSmsDto bulkSmsDto, Guid? userId = null);

        /// <summary>
        /// Send test SMS message
        /// </summary>
        /// <param name="testSmsDto">Test SMS details</param>
        /// <returns>Delivery result</returns>
        Task<SmsDeliveryDto> SendTestSmsAsync(TestSmsDto testSmsDto);

        /// <summary>
        /// Get SMS delivery status
        /// </summary>
        /// <param name="messageId">Message ID</param>
        /// <returns>SMS message details</returns>
        Task<SmsMessageDto?> GetSmsStatusAsync(Guid messageId);

        /// <summary>
        /// Get SMS messages with filtering and pagination
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Paginated SMS messages</returns>
        Task<(List<SmsMessageDto> Messages, int TotalCount)> GetSmsMessagesAsync(SmsSearchDto searchDto);

        /// <summary>
        /// Get SMS statistics
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="provider">Optional provider filter</param>
        /// <returns>SMS statistics</returns>
        Task<SmsStatisticsDto> GetSmsStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? provider = null);

        /// <summary>
        /// Retry failed SMS message
        /// </summary>
        /// <param name="messageId">Message ID to retry</param>
        /// <returns>Delivery result</returns>
        Task<SmsDeliveryDto> RetrySmsAsync(Guid messageId);

        /// <summary>
        /// Cancel pending SMS message
        /// </summary>
        /// <param name="messageId">Message ID to cancel</param>
        /// <returns>True if cancelled successfully</returns>
        Task<bool> CancelSmsAsync(Guid messageId);

        /// <summary>
        /// Get SMS templates
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <param name="activeOnly">Only active templates</param>
        /// <returns>SMS templates</returns>
        Task<List<SmsTemplateDto>> GetSmsTemplatesAsync(string? category = null, bool activeOnly = true);

        /// <summary>
        /// Create SMS template
        /// </summary>
        /// <param name="createDto">Template details</param>
        /// <param name="userId">User creating the template</param>
        /// <returns>Created template</returns>
        Task<SmsTemplateDto> CreateSmsTemplateAsync(CreateSmsTemplateDto createDto, Guid userId);

        /// <summary>
        /// Update SMS template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="updateDto">Update details</param>
        /// <param name="userId">User updating the template</param>
        /// <returns>Updated template</returns>
        Task<SmsTemplateDto> UpdateSmsTemplateAsync(Guid templateId, UpdateSmsTemplateDto updateDto, Guid userId);

        /// <summary>
        /// Delete SMS template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="userId">User deleting the template</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteSmsTemplateAsync(Guid templateId, Guid userId);

        /// <summary>
        /// Preview SMS template with variables
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="variables">Template variables</param>
        /// <returns>Rendered message</returns>
        Task<string> PreviewSmsTemplateAsync(Guid templateId, Dictionary<string, object> variables);

        /// <summary>
        /// Get available SMS providers and their status
        /// </summary>
        /// <returns>SMS providers</returns>
        Task<List<SmsProviderDto>> GetSmsProvidersAsync();

        /// <summary>
        /// Test SMS provider connectivity
        /// </summary>
        /// <param name="providerName">Provider to test</param>
        /// <returns>Test result</returns>
        Task<bool> TestSmsProviderAsync(string providerName);

        /// <summary>
        /// Get SMS configuration
        /// </summary>
        /// <returns>SMS configuration</returns>
        Task<SmsConfigurationDto> GetSmsConfigurationAsync();

        /// <summary>
        /// Update SMS configuration
        /// </summary>
        /// <param name="configDto">Configuration updates</param>
        /// <param name="userId">User updating configuration</param>
        /// <returns>Updated configuration</returns>
        Task<SmsConfigurationDto> UpdateSmsConfigurationAsync(SmsConfigurationDto configDto, Guid userId);

        /// <summary>
        /// Send notification SMS for report status changes
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="status">New status</param>
        /// <param name="recipientPhoneNumbers">Phone numbers to notify</param>
        /// <returns>Bulk delivery result</returns>
        Task<BulkSmsDeliveryDto> SendReportNotificationSmsAsync(Guid reportId, string status, List<string> recipientPhoneNumbers);

        /// <summary>
        /// Send deadline reminder SMS
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="recipientPhoneNumbers">Phone numbers to notify</param>
        /// <param name="daysUntilDeadline">Days until deadline</param>
        /// <returns>Bulk delivery result</returns>
        Task<BulkSmsDeliveryDto> SendDeadlineReminderSmsAsync(Guid reportId, List<string> recipientPhoneNumbers, int daysUntilDeadline);

        /// <summary>
        /// Send approval request SMS
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="approverPhoneNumbers">Approver phone numbers</param>
        /// <returns>Bulk delivery result</returns>
        Task<BulkSmsDeliveryDto> SendApprovalRequestSmsAsync(Guid reportId, List<string> approverPhoneNumbers);

        /// <summary>
        /// Send emergency alert SMS
        /// </summary>
        /// <param name="message">Alert message</param>
        /// <param name="recipientPhoneNumbers">Phone numbers to alert</param>
        /// <returns>Bulk delivery result</returns>
        Task<BulkSmsDeliveryDto> SendEmergencyAlertSmsAsync(string message, List<string> recipientPhoneNumbers);

        /// <summary>
        /// Validate phone number format
        /// </summary>
        /// <param name="phoneNumber">Phone number to validate</param>
        /// <returns>True if valid</returns>
        Task<bool> ValidatePhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Format phone number to E.164 standard
        /// </summary>
        /// <param name="phoneNumber">Phone number to format</param>
        /// <param name="defaultCountryCode">Default country code if not specified</param>
        /// <returns>Formatted phone number</returns>
        Task<string> FormatPhoneNumberAsync(string phoneNumber, string defaultCountryCode = "+27");

        /// <summary>
        /// Get SMS usage for user (daily/monthly limits)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Usage statistics</returns>
        Task<(int DailyCount, int MonthlyCount, decimal DailyCost, decimal MonthlyCost)> GetUserSmsUsageAsync(Guid userId);

        /// <summary>
        /// Check if user can send SMS (within limits)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="messageCount">Number of messages to send</param>
        /// <returns>True if allowed</returns>
        Task<bool> CanUserSendSmsAsync(Guid userId, int messageCount = 1);

        /// <summary>
        /// Process SMS delivery receipts/webhooks
        /// </summary>
        /// <param name="providerName">SMS provider name</param>
        /// <param name="webhookData">Webhook payload</param>
        /// <returns>True if processed successfully</returns>
        Task<bool> ProcessSmsWebhookAsync(string providerName, Dictionary<string, object> webhookData);

        /// <summary>
        /// Clean up old SMS messages based on retention policy
        /// </summary>
        /// <param name="retentionDays">Days to retain messages</param>
        /// <returns>Number of messages cleaned up</returns>
        Task<int> CleanupOldSmsMessagesAsync(int retentionDays = 90);

        /// <summary>
        /// Get SMS message history for a specific report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <returns>SMS messages related to the report</returns>
        Task<List<SmsMessageDto>> GetReportSmsHistoryAsync(Guid reportId);

        /// <summary>
        /// Get SMS message history for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Maximum number of messages to return</param>
        /// <returns>User's SMS message history</returns>
        Task<List<SmsMessageDto>> GetUserSmsHistoryAsync(Guid userId, int limit = 50);
    }
}
