using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Core notification service interface for managing notifications
    /// </summary>
    public interface INotificationService
    {
        #region Notification Management

        /// <summary>
        /// Creates a single notification
        /// </summary>
        /// <param name="createDto">Notification creation data</param>
        /// <param name="createdBy">User creating the notification</param>
        /// <returns>Created notification</returns>
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto, Guid createdBy);

        /// <summary>
        /// Creates multiple notifications in bulk
        /// </summary>
        /// <param name="bulkDto">Bulk notification data</param>
        /// <param name="createdBy">User creating the notifications</param>
        /// <returns>List of created notifications</returns>
        Task<List<NotificationDto>> CreateBulkNotificationAsync(BulkNotificationDto bulkDto, Guid createdBy);

        /// <summary>
        /// Gets notifications for a specific user with filtering and pagination
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Paginated notification list</returns>
        Task<PagedResultDto<NotificationDto>> GetUserNotificationsAsync(Guid userId, NotificationFilterDto filter);

        /// <summary>
        /// Gets a specific notification by ID
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <param name="userId">Requesting user ID</param>
        /// <param name="userRole">Requesting user role</param>
        /// <returns>Notification details</returns>
        Task<NotificationDto?> GetNotificationByIdAsync(Guid notificationId, Guid userId, UserRole userRole);

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> MarkAsReadAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// Marks multiple notifications as read
        /// </summary>
        /// <param name="notificationIds">List of notification IDs</param>
        /// <param name="userId">User ID</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> MarkMultipleAsReadAsync(List<Guid> notificationIds, Guid userId);

        /// <summary>
        /// Marks all notifications as read for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// Deletes a notification (soft delete)
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="userRole">User role</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> DeleteNotificationAsync(Guid notificationId, Guid userId, UserRole userRole);

        /// <summary>
        /// Gets unread notification count for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Unread count</returns>
        Task<int> GetUnreadCountAsync(Guid userId);

        #endregion

        #region Workflow Notifications

        /// <summary>
        /// Sends report submission notification
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="submittedBy">User who submitted</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> SendReportSubmissionNotificationAsync(Guid reportId, Guid submittedBy);

        /// <summary>
        /// Sends approval required notification
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="approverRole">Role that needs to approve</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> SendApprovalRequiredNotificationAsync(Guid reportId, UserRole approverRole);

        /// <summary>
        /// Sends approval confirmation notification
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="approvedBy">User who approved</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> SendApprovalConfirmationNotificationAsync(Guid reportId, Guid approvedBy);

        /// <summary>
        /// Sends rejection notification
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="rejectedBy">User who rejected</param>
        /// <param name="rejectionReason">Reason for rejection</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> SendRejectionNotificationAsync(Guid reportId, Guid rejectedBy, string rejectionReason);

        /// <summary>
        /// Sends due date reminder notifications
        /// </summary>
        /// <param name="daysBeforeDue">Days before due date to send reminder</param>
        /// <returns>Number of reminders sent</returns>
        Task<int> SendDueDateRemindersAsync(int daysBeforeDue = 1);

        /// <summary>
        /// Sends escalation notifications for overdue reports
        /// </summary>
        /// <returns>Number of escalations sent</returns>
        Task<int> SendEscalationNotificationsAsync();

        #endregion

        #region Statistics and Monitoring

        /// <summary>
        /// Gets notification statistics
        /// </summary>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <param name="userId">User ID for user-specific stats (optional)</param>
        /// <returns>Notification statistics</returns>
        Task<NotificationStatisticsDto> GetNotificationStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, Guid? userId = null);

        /// <summary>
        /// Gets all notifications for admin management
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Paginated notification list</returns>
        Task<PagedResultDto<NotificationDto>> GetAllNotificationsAsync(NotificationFilterDto filter);

        #endregion

        #region Background Processing

        /// <summary>
        /// Processes pending notifications
        /// </summary>
        /// <param name="batchSize">Number of notifications to process</param>
        /// <returns>Number of notifications processed</returns>
        Task<int> ProcessPendingNotificationsAsync(int batchSize = 100);

        /// <summary>
        /// Cleans up old notifications
        /// </summary>
        /// <param name="olderThanDays">Delete notifications older than specified days</param>
        /// <returns>Number of notifications cleaned up</returns>
        Task<int> CleanupOldNotificationsAsync(int olderThanDays = 90);

        /// <summary>
        /// Retries failed notifications
        /// </summary>
        /// <param name="maxRetries">Maximum retry attempts</param>
        /// <returns>Number of notifications retried</returns>
        Task<int> RetryFailedNotificationsAsync(int maxRetries = 3);

        #endregion
    }

    /// <summary>
    /// Email service interface for email-specific operations
    /// </summary>
    public interface IEmailService
    {
        #region Email Sending

        /// <summary>
        /// Sends a single email
        /// </summary>
        /// <param name="to">Recipient email</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">HTML body content</param>
        /// <param name="textBody">Plain text body content</param>
        /// <param name="options">Email options</param>
        /// <returns>Success result with message ID</returns>
        Task<EmailSendResult> SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null, EmailOptions? options = null);

        /// <summary>
        /// Sends email using template
        /// </summary>
        /// <param name="to">Recipient email</param>
        /// <param name="templateId">Template ID</param>
        /// <param name="variables">Template variables</param>
        /// <param name="options">Email options</param>
        /// <returns>Success result with message ID</returns>
        Task<EmailSendResult> SendTemplatedEmailAsync(string to, Guid templateId, Dictionary<string, object> variables, EmailOptions? options = null);

        /// <summary>
        /// Queues email for later delivery
        /// </summary>
        /// <param name="queueItem">Email queue item</param>
        /// <returns>Queue item ID</returns>
        Task<Guid> QueueEmailAsync(EmailQueueDto queueItem);

        #endregion

        #region Email Queue Management

        /// <summary>
        /// Processes email queue
        /// </summary>
        /// <param name="batchSize">Number of emails to process</param>
        /// <returns>Number of emails processed</returns>
        Task<int> ProcessEmailQueueAsync(int batchSize = 50);

        /// <summary>
        /// Gets email queue status
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Paginated email queue list</returns>
        Task<PagedResultDto<EmailQueueDto>> GetEmailQueueAsync(NotificationFilterDto filter);

        /// <summary>
        /// Retries failed emails
        /// </summary>
        /// <param name="maxRetries">Maximum retry attempts</param>
        /// <returns>Number of emails retried</returns>
        Task<int> RetryFailedEmailsAsync(int maxRetries = 3);

        /// <summary>
        /// Cancels pending emails
        /// </summary>
        /// <param name="emailIds">List of email IDs to cancel</param>
        /// <returns>Number of emails cancelled</returns>
        Task<int> CancelPendingEmailsAsync(List<Guid> emailIds);

        #endregion

        #region Template Processing

        /// <summary>
        /// Processes email template with variables
        /// </summary>
        /// <param name="template">Email template</param>
        /// <param name="variables">Template variables</param>
        /// <returns>Processed template content</returns>
        Task<TemplatePreviewDto> ProcessTemplateAsync(NotificationTemplateDto template, Dictionary<string, object> variables);

        /// <summary>
        /// Validates template syntax
        /// </summary>
        /// <param name="htmlTemplate">HTML template content</param>
        /// <param name="textTemplate">Text template content</param>
        /// <returns>Validation result</returns>
        Task<TemplateValidationResult> ValidateTemplateAsync(string htmlTemplate, string textTemplate);

        #endregion
    }

    /// <summary>
    /// Notification template service interface
    /// </summary>
    public interface INotificationTemplateService
    {
        #region Template Management

        /// <summary>
        /// Creates a new notification template
        /// </summary>
        /// <param name="createDto">Template creation data</param>
        /// <param name="createdBy">User creating the template</param>
        /// <returns>Created template</returns>
        Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateDto createDto, Guid createdBy);

        /// <summary>
        /// Updates an existing template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="updateDto">Template update data</param>
        /// <param name="modifiedBy">User modifying the template</param>
        /// <returns>Updated template</returns>
        Task<NotificationTemplateDto> UpdateTemplateAsync(Guid templateId, UpdateNotificationTemplateDto updateDto, Guid modifiedBy);

        /// <summary>
        /// Gets a template by ID
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Template details</returns>
        Task<NotificationTemplateDto?> GetTemplateByIdAsync(Guid templateId);

        /// <summary>
        /// Gets templates by type
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="includeInactive">Include inactive templates</param>
        /// <returns>List of templates</returns>
        Task<List<NotificationTemplateDto>> GetTemplatesByTypeAsync(NotificationType type, bool includeInactive = false);

        /// <summary>
        /// Gets all templates with filtering
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Paginated template list</returns>
        Task<PagedResultDto<NotificationTemplateDto>> GetAllTemplatesAsync(TemplateFilterDto filter);

        /// <summary>
        /// Deletes a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="deletedBy">User deleting the template</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> DeleteTemplateAsync(Guid templateId, Guid deletedBy);

        /// <summary>
        /// Duplicates a template
        /// </summary>
        /// <param name="templateId">Source template ID</param>
        /// <param name="newName">New template name</param>
        /// <param name="createdBy">User creating the duplicate</param>
        /// <returns>Duplicated template</returns>
        Task<NotificationTemplateDto> DuplicateTemplateAsync(Guid templateId, string newName, Guid createdBy);

        #endregion

        #region Template Operations

        /// <summary>
        /// Previews template with sample data
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="variables">Template variables</param>
        /// <returns>Preview content</returns>
        Task<TemplatePreviewDto> PreviewTemplateAsync(Guid templateId, Dictionary<string, object>? variables = null);

        /// <summary>
        /// Gets available variables for a template type
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <returns>Available variables</returns>
        Task<List<TemplateVariableDto>> GetAvailableVariablesAsync(NotificationType type);

        /// <summary>
        /// Validates template content
        /// </summary>
        /// <param name="htmlTemplate">HTML template</param>
        /// <param name="textTemplate">Text template</param>
        /// <param name="variables">Template variables</param>
        /// <returns>Validation result</returns>
        Task<TemplateValidationResult> ValidateTemplateContentAsync(string htmlTemplate, string textTemplate, List<TemplateVariableDto>? variables = null);

        #endregion

        #region System Templates

        /// <summary>
        /// Initializes default system templates
        /// </summary>
        /// <returns>Number of templates created</returns>
        Task<int> InitializeSystemTemplatesAsync();

        /// <summary>
        /// Updates system templates to latest version
        /// </summary>
        /// <returns>Number of templates updated</returns>
        Task<int> UpdateSystemTemplatesAsync();

        #endregion
    }

    /// <summary>
    /// Notification preference service interface
    /// </summary>
    public interface INotificationPreferenceService
    {
        #region Preference Management

        /// <summary>
        /// Gets user notification preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User preferences</returns>
        Task<List<NotificationPreferenceDto>> GetUserPreferencesAsync(Guid userId);

        /// <summary>
        /// Updates user notification preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferences">Updated preferences</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> UpdateUserPreferencesAsync(Guid userId, UpdateNotificationPreferencesDto preferences);

        /// <summary>
        /// Resets user preferences to default
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> ResetToDefaultPreferencesAsync(Guid userId);

        /// <summary>
        /// Gets default preferences for a user role
        /// </summary>
        /// <param name="userRole">User role</param>
        /// <returns>Default preferences</returns>
        Task<List<NotificationPreferenceDto>> GetDefaultPreferencesAsync(UserRole userRole);

        #endregion

        #region Preference Validation

        /// <summary>
        /// Checks if user should receive notification based on preferences
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Notification type</param>
        /// <param name="channel">Notification channel</param>
        /// <returns>True if user should receive notification</returns>
        Task<bool> ShouldReceiveNotificationAsync(Guid userId, NotificationType notificationType, NotificationChannel channel);

        /// <summary>
        /// Gets optimal delivery time for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notificationType">Notification type</param>
        /// <returns>Optimal delivery time</returns>
        Task<DateTime?> GetOptimalDeliveryTimeAsync(Guid userId, NotificationType notificationType);

        #endregion
    }

    /// <summary>
    /// Webhook service interface for external integrations
    /// </summary>
    public interface IWebhookService
    {
        #region Webhook Management

        /// <summary>
        /// Creates a new webhook subscription
        /// </summary>
        /// <param name="createDto">Webhook creation data</param>
        /// <param name="createdBy">User creating the webhook</param>
        /// <returns>Created webhook subscription</returns>
        Task<WebhookSubscriptionDto> CreateWebhookSubscriptionAsync(CreateWebhookSubscriptionDto createDto, Guid createdBy);

        /// <summary>
        /// Gets webhook subscriptions
        /// </summary>
        /// <param name="createdBy">Filter by creator (optional)</param>
        /// <returns>List of webhook subscriptions</returns>
        Task<List<WebhookSubscriptionDto>> GetWebhookSubscriptionsAsync(Guid? createdBy = null);

        /// <summary>
        /// Updates webhook subscription
        /// </summary>
        /// <param name="webhookId">Webhook ID</param>
        /// <param name="updateDto">Update data</param>
        /// <param name="modifiedBy">User modifying the webhook</param>
        /// <returns>Updated webhook subscription</returns>
        Task<WebhookSubscriptionDto> UpdateWebhookSubscriptionAsync(Guid webhookId, CreateWebhookSubscriptionDto updateDto, Guid modifiedBy);

        /// <summary>
        /// Deletes webhook subscription
        /// </summary>
        /// <param name="webhookId">Webhook ID</param>
        /// <param name="deletedBy">User deleting the webhook</param>
        /// <returns>Success result</returns>
        Task<ServiceResultDto> DeleteWebhookSubscriptionAsync(Guid webhookId, Guid deletedBy);

        #endregion

        #region Webhook Delivery

        /// <summary>
        /// Sends notification to webhook subscribers
        /// </summary>
        /// <param name="notification">Notification to send</param>
        /// <returns>Number of webhooks triggered</returns>
        Task<int> TriggerWebhooksAsync(NotificationDto notification);

        /// <summary>
        /// Tests webhook endpoint
        /// </summary>
        /// <param name="webhookUrl">Webhook URL</param>
        /// <param name="secretKey">Secret key for authentication</param>
        /// <returns>Test result</returns>
        Task<WebhookTestResult> TestWebhookAsync(string webhookUrl, string? secretKey = null);

        #endregion
    }

    #region Result Classes

    /// <summary>
    /// Email send result
    /// </summary>
    public class EmailSendResult
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentDate { get; set; }
    }

    /// <summary>
    /// Template validation result
    /// </summary>
    public class TemplateValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> MissingVariables { get; set; } = new();
        public List<string> UnusedVariables { get; set; } = new();
    }

    /// <summary>
    /// Webhook test result
    /// </summary>
    public class WebhookTestResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? Response { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ResponseTime { get; set; }
    }

    /// <summary>
    /// Template filter DTO
    /// </summary>
    public class TemplateFilterDto
    {
        public NotificationType? Type { get; set; }
        public string? Category { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSystem { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
    }

    #endregion
}
