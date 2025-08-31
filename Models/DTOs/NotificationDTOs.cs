using ProjectControlsReportingTool.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    #region Notification DTOs

    /// <summary>
    /// DTO for creating a new notification
    /// </summary>
    public class CreateNotificationDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        [Required]
        public Guid RecipientId { get; set; }

        public Guid? SenderId { get; set; }

        public Guid? RelatedReportId { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        [MaxLength(100)]
        public string? ActionText { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public Dictionary<string, object>? AdditionalData { get; set; }

        public NotificationChannelOptions? ChannelOptions { get; set; }
    }

    /// <summary>
    /// DTO for bulk notification creation
    /// </summary>
    public class BulkNotificationDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        [Required]
        public List<Guid> RecipientIds { get; set; } = new();

        public Guid? SenderId { get; set; }

        public List<Department>? TargetDepartments { get; set; }

        public List<UserRole>? TargetRoles { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        [MaxLength(100)]
        public string? ActionText { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public Dictionary<string, object>? AdditionalData { get; set; }

        public NotificationChannelOptions? ChannelOptions { get; set; }
    }

    /// <summary>
    /// DTO for notification response
    /// </summary>
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string TypeDisplayName { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; }
        public string PriorityDisplayName { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; }
        public string StatusDisplayName { get; set; } = string.Empty;
        public Guid RecipientId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public Guid? SenderId { get; set; }
        public string? SenderName { get; set; }
        public Guid? RelatedReportId { get; set; }
        public string? RelatedReportTitle { get; set; }
        public string? Category { get; set; }
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsRead { get; set; }
        public bool IsEmailSent { get; set; }
        public bool IsPushSent { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
        public List<NotificationHistoryDto> History { get; set; } = new();
    }

    /// <summary>
    /// DTO for notification history
    /// </summary>
    public class NotificationHistoryDto
    {
        public Guid Id { get; set; }
        public NotificationStatus Status { get; set; }
        public string StatusDisplayName { get; set; } = string.Empty;
        public string? StatusMessage { get; set; }
        public string? Channel { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? AdditionalDetails { get; set; }
        public bool IsError { get; set; }
    }

    /// <summary>
    /// DTO for notification filtering and pagination
    /// </summary>
    public class NotificationFilterDto
    {
        public List<NotificationType>? Types { get; set; }
        public List<NotificationStatus>? Statuses { get; set; }
        public List<NotificationPriority>? Priorities { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsRead { get; set; }
        public string? Category { get; set; }
        public Guid? SenderId { get; set; }
        public Guid? RelatedReportId { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// DTO for notification channel options
    /// </summary>
    public class NotificationChannelOptions
    {
        public bool SendEmail { get; set; } = true;
        public bool SendPush { get; set; } = true;
        public bool SendInApp { get; set; } = true;
        public bool SendSms { get; set; } = false;
        public bool SendWebhook { get; set; } = false;
        public EmailOptions? EmailOptions { get; set; }
        public PushOptions? PushOptions { get; set; }
        public NotificationSmsOptions? SmsOptions { get; set; }
    }

    /// <summary>
    /// DTO for email-specific options
    /// </summary>
    public class EmailOptions
    {
        public List<string>? CcEmails { get; set; }
        public List<string>? BccEmails { get; set; }
        public List<string>? CcAddresses { get; set; }
        public List<string>? BccAddresses { get; set; }
        public EmailPriority Priority { get; set; } = EmailPriority.Normal;
        public List<EmailAttachmentDto>? Attachments { get; set; }
        public Dictionary<string, string>? CustomHeaders { get; set; }
        public bool RequestDeliveryReceipt { get; set; } = false;
        public TimeSpan? DelayDelivery { get; set; }
        public int MaxRetries { get; set; } = 3;
    }

    /// <summary>
    /// DTO for push notification options
    /// </summary>
    public class PushOptions
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Icon { get; set; }
        public string? Sound { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }

    /// <summary>
    /// DTO for SMS options in notifications
    /// </summary>
    public class NotificationSmsOptions
    {
        public string? PhoneNumber { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// DTO for email attachments
    /// </summary>
    public class EmailAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public bool IsInline { get; set; } = false;
        public string? ContentId { get; set; }
    }

    #endregion

    #region Notification Template DTOs

    /// <summary>
    /// DTO for creating notification templates
    /// </summary>
    public class CreateNotificationTemplateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string HtmlTemplate { get; set; } = string.Empty;

        [Required]
        public string TextTemplate { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public List<TemplateVariableDto>? Variables { get; set; }
    }

    /// <summary>
    /// DTO for updating notification templates
    /// </summary>
    public class UpdateNotificationTemplateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string HtmlTemplate { get; set; } = string.Empty;

        [Required]
        public string TextTemplate { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public List<TemplateVariableDto>? Variables { get; set; }
    }

    /// <summary>
    /// DTO for notification template response
    /// </summary>
    public class NotificationTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlTemplate { get; set; } = string.Empty;
        public string TextTemplate { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string TypeDisplayName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystem { get; set; }
        public List<TemplateVariableDto> Variables { get; set; } = new();
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int Version { get; set; }
    }

    /// <summary>
    /// DTO for template variables
    /// </summary>
    public class TemplateVariableDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // string, number, date, boolean
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? Format { get; set; }
        public List<string>? AllowedValues { get; set; }
    }

    /// <summary>
    /// DTO for template preview
    /// </summary>
    public class TemplatePreviewDto
    {
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public Dictionary<string, object> Variables { get; set; } = new();
    }

    #endregion

    #region Notification Preference DTOs

    /// <summary>
    /// DTO for notification preferences
    /// </summary>
    public class NotificationPreferenceDto
    {
        public Guid Id { get; set; }
        public NotificationType NotificationType { get; set; }
        public string TypeDisplayName { get; set; } = string.Empty;
        public bool EmailEnabled { get; set; }
        public bool PushEnabled { get; set; }
        public bool InAppEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public string? PreferredTime { get; set; }
        public int? FrequencyMinutes { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for updating notification preferences
    /// </summary>
    public class UpdateNotificationPreferencesDto
    {
        public List<NotificationPreferenceUpdateDto> Preferences { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual preference update
    /// </summary>
    public class NotificationPreferenceUpdateDto
    {
        public NotificationType NotificationType { get; set; }
        public bool EmailEnabled { get; set; }
        public bool PushEnabled { get; set; }
        public bool InAppEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public string? PreferredTime { get; set; }
        public int? FrequencyMinutes { get; set; }
    }

    #endregion

    #region Email Queue DTOs

    /// <summary>
    /// DTO for email queue items
    /// </summary>
    public class EmailQueueDto
    {
        public Guid Id { get; set; }
        public Guid? NotificationId { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string? ToName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public EmailQueueStatus Status { get; set; }
        public string StatusDisplayName { get; set; } = string.Empty;
        public EmailPriority Priority { get; set; }
        public string PriorityDisplayName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; }
        public string? ErrorMessage { get; set; }
        public string? MessageId { get; set; }
    }

    #endregion

    #region Webhook Subscription DTOs

    /// <summary>
    /// DTO for creating webhook subscriptions
    /// </summary>
    public class CreateWebhookSubscriptionDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Url]
        [MaxLength(500)]
        public string WebhookUrl { get; set; } = string.Empty;

        [Required]
        public List<NotificationType> SubscribedTypes { get; set; } = new();

        [MaxLength(200)]
        public string? SecretKey { get; set; }

        public int TimeoutSeconds { get; set; } = 30;

        public int MaxRetries { get; set; } = 3;
    }

    /// <summary>
    /// DTO for webhook subscription response
    /// </summary>
    public class WebhookSubscriptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public List<NotificationType> SubscribedTypes { get; set; } = new();
        public List<string> SubscribedTypeNames { get; set; } = new();
        public bool IsActive { get; set; }
        public int TimeoutSeconds { get; set; }
        public int MaxRetries { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastTriggeredDate { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double SuccessRate { get; set; }
    }

    #endregion

    #region Notification Statistics DTOs

    /// <summary>
    /// DTO for notification statistics
    /// </summary>
    public class NotificationStatisticsDto
    {
        public int TotalNotifications { get; set; }
        public int SentNotifications { get; set; }
        public int PendingNotifications { get; set; }
        public int FailedNotifications { get; set; }
        public int ReadNotifications { get; set; }
        public double DeliveryRate { get; set; }
        public double ReadRate { get; set; }
        public Dictionary<NotificationType, int> NotificationsByType { get; set; } = new();
        public Dictionary<NotificationStatus, int> NotificationsByStatus { get; set; } = new();
        public Dictionary<string, int> NotificationsByDay { get; set; } = new();
        public List<TopRecipientDto> TopRecipients { get; set; } = new();
        public NotificationPerformanceDto Performance { get; set; } = new();
    }

    /// <summary>
    /// DTO for top notification recipients
    /// </summary>
    public class TopRecipientDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int NotificationCount { get; set; }
        public int ReadCount { get; set; }
        public double ReadRate { get; set; }
    }

    /// <summary>
    /// DTO for notification performance metrics
    /// </summary>
    public class NotificationPerformanceDto
    {
        public double AverageDeliveryTime { get; set; }
        public double AverageReadTime { get; set; }
        public int EmailsPerHour { get; set; }
        public int PushNotificationsPerHour { get; set; }
        public double SystemLoad { get; set; }
        public List<string> PerformanceIssues { get; set; } = new();
    }

    #endregion

    #region Manual Notification DTOs

    /// <summary>
    /// DTO for manual notification sending
    /// </summary>
    public class ManualNotificationDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public List<Guid>? RecipientIds { get; set; }

        public List<string>? RecipientEmails { get; set; }

        public List<Department>? TargetDepartments { get; set; }

        public List<UserRole>? TargetRoles { get; set; }

        public bool SendToAllUsers { get; set; } = false;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        [MaxLength(100)]
        public string? ActionText { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public NotificationChannelOptions? ChannelOptions { get; set; }

        public Guid? TemplateId { get; set; }

        public Dictionary<string, object>? TemplateVariables { get; set; }
    }

    #endregion
}
