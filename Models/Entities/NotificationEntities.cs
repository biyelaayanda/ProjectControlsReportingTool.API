using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    /// <summary>
    /// Represents a notification in the system
    /// </summary>
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationPriority Priority { get; set; }

        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        [Required]
        public Guid RecipientId { get; set; }

        [ForeignKey(nameof(RecipientId))]
        public virtual User Recipient { get; set; } = null!;

        public Guid? SenderId { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual User? Sender { get; set; }

        public Guid? RelatedReportId { get; set; }

        [ForeignKey(nameof(RelatedReportId))]
        public virtual Report? RelatedReport { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        [MaxLength(100)]
        public string? ActionText { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ScheduledDate { get; set; }

        public DateTime? SentDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; } = 0;

        public int MaxRetries { get; set; } = 3;

        [Column(TypeName = "nvarchar(max)")]
        public string? AdditionalData { get; set; }

        public bool IsRead { get; set; } = false;

        public bool IsEmailSent { get; set; } = false;

        public bool IsPushSent { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        // Navigation properties for audit trail
        public virtual ICollection<NotificationHistory> History { get; set; } = new List<NotificationHistory>();
    }

    /// <summary>
    /// Represents notification template for standardized messaging
    /// </summary>
    [Table("NotificationTemplates")]
    public class NotificationTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string HtmlTemplate { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string TextTemplate { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystem { get; set; } = false;

        [Column(TypeName = "nvarchar(max)")]
        public string? Variables { get; set; } // JSON array of available variables

        [Required]
        public Guid CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Guid? ModifiedBy { get; set; }

        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? Modifier { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int Version { get; set; } = 1;

        // Navigation properties
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    /// <summary>
    /// Represents user notification preferences
    /// </summary>
    [Table("NotificationPreferences")]
    public class NotificationPreference
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public NotificationType NotificationType { get; set; }

        public bool EmailEnabled { get; set; } = true;

        public bool PushEnabled { get; set; } = true;

        public bool InAppEnabled { get; set; } = true;

        public bool SmsEnabled { get; set; } = false;

        [MaxLength(20)]
        public string? PreferredTime { get; set; } // HH:mm format

        public int? FrequencyMinutes { get; set; } // For digest notifications

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// Represents notification history for audit trail
    /// </summary>
    [Table("NotificationHistory")]
    public class NotificationHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NotificationId { get; set; }

        [ForeignKey(nameof(NotificationId))]
        public virtual Notification Notification { get; set; } = null!;

        [Required]
        public NotificationStatus Status { get; set; }

        [MaxLength(500)]
        public string? StatusMessage { get; set; }

        [MaxLength(100)]
        public string? Channel { get; set; } // Email, Push, SMS, InApp

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(2000)]
        public string? AdditionalDetails { get; set; }

        public bool IsError { get; set; } = false;
    }

    /// <summary>
    /// Represents email queue for reliable delivery
    /// </summary>
    [Table("EmailQueue")]
    public class EmailQueue
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? NotificationId { get; set; }

        [ForeignKey(nameof(NotificationId))]
        public virtual Notification? Notification { get; set; }

        [Required]
        [MaxLength(500)]
        public string ToEmail { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ToName { get; set; }

        [MaxLength(500)]
        public string? CcEmails { get; set; }

        [MaxLength(500)]
        public string? BccEmails { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string HtmlBody { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string? TextBody { get; set; }

        public EmailQueueStatus Status { get; set; } = EmailQueueStatus.Pending;

        public EmailPriority Priority { get; set; } = EmailPriority.Normal;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ScheduledDate { get; set; }

        public DateTime? SentDate { get; set; }

        public int RetryCount { get; set; } = 0;

        public int MaxRetries { get; set; } = 3;

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        [MaxLength(500)]
        public string? MessageId { get; set; } // SMTP message ID

        [Column(TypeName = "nvarchar(max)")]
        public string? Attachments { get; set; } // JSON array of attachment paths

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// Represents notification subscription for external webhooks
    /// </summary>
    [Table("NotificationSubscriptions")]
    public class NotificationSubscription
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string WebhookUrl { get; set; } = string.Empty;

        [Required]
        public NotificationType[] SubscribedTypes { get; set; } = Array.Empty<NotificationType>();

        [MaxLength(200)]
        public string? SecretKey { get; set; }

        [MaxLength(100)]
        public string? AuthToken { get; set; }

        public bool IsActive { get; set; } = true;

        public int TimeoutSeconds { get; set; } = 30;

        public int MaxRetries { get; set; } = 3;

        [Required]
        public Guid CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastTriggeredDate { get; set; }

        public int SuccessCount { get; set; } = 0;

        public int FailureCount { get; set; } = 0;
    }
}
