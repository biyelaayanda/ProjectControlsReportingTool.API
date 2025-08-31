using System.ComponentModel.DataAnnotations;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    /// <summary>
    /// DTO for SMS notification delivery results
    /// </summary>
    public class SmsDeliveryDto
    {
        public Guid MessageId { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsDelivered { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
        public decimal Cost { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? ExternalMessageId { get; set; }
    }

    /// <summary>
    /// DTO for sending SMS messages
    /// </summary>
    public class SendSmsDto
    {
        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(160, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        [StringLength(50)]
        public string MessageType { get; set; } = "Notification";

        public bool IsUrgent { get; set; } = false;

        public DateTime? ScheduledAt { get; set; }

        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// DTO for bulk SMS operations
    /// </summary>
    public class BulkSmsDto
    {
        [Required]
        public List<string> PhoneNumbers { get; set; } = new();

        [Required]
        [StringLength(160, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        [StringLength(50)]
        public string MessageType { get; set; } = "Broadcast";

        public bool IsUrgent { get; set; } = false;

        public DateTime? ScheduledAt { get; set; }
    }

    /// <summary>
    /// DTO for bulk SMS delivery results
    /// </summary>
    public class BulkSmsDeliveryDto
    {
        public Guid BatchId { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalRecipients { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double SuccessRate { get; set; }
        public decimal TotalCost { get; set; }
        public List<SmsDeliveryDto> Deliveries { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public TimeSpan ProcessingTime { get; set; }
    }

    /// <summary>
    /// DTO for SMS template management
    /// </summary>
    public class SmsTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSystemTemplate { get; set; }
        public List<string> Variables { get; set; } = new();
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating SMS templates
    /// </summary>
    public class CreateSmsTemplateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(160)]
        public string Template { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating SMS templates
    /// </summary>
    public class UpdateSmsTemplateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(160)]
        public string? Template { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for SMS delivery statistics
    /// </summary>
    public class SmsStatisticsDto
    {
        public int TotalMessagesSent { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double DeliveryRate { get; set; }
        public decimal TotalCost { get; set; }
        public int MessagesToday { get; set; }
        public int MessagesThisWeek { get; set; }
        public int MessagesThisMonth { get; set; }
        public Dictionary<string, int> MessagesByType { get; set; } = new();
        public Dictionary<string, int> MessagesByProvider { get; set; } = new();
        public Dictionary<string, double> DeliveryRateByProvider { get; set; } = new();
        public decimal AverageCostPerMessage { get; set; }
        public DateTime LastMessageSent { get; set; }
        public DateTime StatsGeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for SMS message history and tracking
    /// </summary>
    public class SmsMessageDto
    {
        public Guid Id { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public bool IsUrgent { get; set; }
        public decimal Cost { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? ExternalMessageId { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public Guid? RelatedReportId { get; set; }
        public string? RelatedReportTitle { get; set; }
    }

    /// <summary>
    /// DTO for SMS message search and filtering
    /// </summary>
    public class SmsSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? MessageType { get; set; }
        public string? Provider { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? SentFrom { get; set; }
        public DateTime? SentTo { get; set; }
        public bool? IsUrgent { get; set; }
        public string SortBy { get; set; } = "SentAt";
        public string SortDirection { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for SMS provider configuration
    /// </summary>
    public class SmsProviderDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public int Priority { get; set; }
        public decimal CostPerMessage { get; set; }
        public string SupportedCountries { get; set; } = string.Empty;
        public int MaxMessageLength { get; set; }
        public bool SupportsUnicode { get; set; }
        public double DeliveryRate { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime LastStatusCheck { get; set; }
    }

    /// <summary>
    /// DTO for testing SMS functionality
    /// </summary>
    public class TestSmsDto
    {
        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(160)]
        public string Message { get; set; } = "Test message from Project Controls Reporting Tool SMS service.";

        public string? Provider { get; set; }
    }

    /// <summary>
    /// DTO for SMS configuration and settings
    /// </summary>
    public class SmsConfigurationDto
    {
        public bool SmsEnabled { get; set; }
        public string DefaultProvider { get; set; } = string.Empty;
        public int MaxDailyMessages { get; set; }
        public int MaxMessagesPerUser { get; set; }
        public decimal DailyBudget { get; set; }
        public bool RequireApprovalForBulk { get; set; }
        public int BulkMessageThreshold { get; set; }
        public List<string> RestrictedNumbers { get; set; } = new();
        public List<string> AllowedCountryCodes { get; set; } = new();
        public bool LogAllMessages { get; set; }
        public int RetentionDays { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
