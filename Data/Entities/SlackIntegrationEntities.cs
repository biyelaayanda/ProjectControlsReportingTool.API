using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectControlsReportingTool.API.Data.Entities
{
    /// <summary>
    /// Entity representing Slack webhook configuration
    /// </summary>
    [Table("SlackWebhookConfigs")]
    public class SlackWebhookConfig : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DefaultChannel { get; set; }

        [StringLength(100)]
        public string? DefaultUsername { get; set; }

        [StringLength(500)]
        public string? DefaultIconEmoji { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// JSON serialized list of enabled notification types
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string EnabledNotificationsJson { get; set; } = "[]";

        /// <summary>
        /// Computed property for enabled notifications
        /// </summary>
        [NotMapped]
        public List<NotificationType> EnabledNotifications
        {
            get => string.IsNullOrEmpty(EnabledNotificationsJson) 
                ? new List<NotificationType>()
                : System.Text.Json.JsonSerializer.Deserialize<List<NotificationType>>(EnabledNotificationsJson) ?? new List<NotificationType>();
            set => EnabledNotificationsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        public bool IsActive { get; set; } = true;

        public bool UseAttachments { get; set; } = true;

        public bool UseBlocks { get; set; } = false;

        /// <summary>
        /// JSON serialized custom settings
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? CustomSettingsJson { get; set; }

        /// <summary>
        /// Computed property for custom settings
        /// </summary>
        [NotMapped]
        public Dictionary<string, object>? CustomSettings
        {
            get => string.IsNullOrEmpty(CustomSettingsJson) 
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(CustomSettingsJson);
            set => CustomSettingsJson = value != null 
                ? System.Text.Json.JsonSerializer.Serialize(value) 
                : null;
        }

        // Foreign key to User
        public Guid UserId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<SlackMessage> Messages { get; set; } = new List<SlackMessage>();
        public virtual ICollection<SlackNotificationTemplate> Templates { get; set; } = new List<SlackNotificationTemplate>();
    }

    /// <summary>
    /// Entity representing a Slack message sent through the system
    /// </summary>
    [Table("SlackMessages")]
    public class SlackMessage : BaseEntity
    {
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Text { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Channel { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [StringLength(500)]
        public string? IconEmoji { get; set; }

        [StringLength(1000)]
        public string? IconUrl { get; set; }

        /// <summary>
        /// JSON serialized attachments
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? AttachmentsJson { get; set; }

        /// <summary>
        /// JSON serialized blocks (Block Kit)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? BlocksJson { get; set; }

        [StringLength(100)]
        public string? ThreadTs { get; set; }

        public bool UnfurlLinks { get; set; } = true;

        public bool UnfurlMedia { get; set; } = true;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Sent, Failed

        public int? StatusCode { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? ErrorMessage { get; set; }

        public DateTime? SentAt { get; set; }

        public TimeSpan? ResponseTime { get; set; }

        [StringLength(100)]
        public string? MessageId { get; set; }

        public NotificationType NotificationType { get; set; }

        // Foreign keys
        public Guid? UserId { get; set; }
        public Guid? ReportId { get; set; }
        public Guid? WebhookConfigId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("ReportId")]
        public virtual Report? Report { get; set; }

        [ForeignKey("WebhookConfigId")]
        public virtual SlackWebhookConfig? WebhookConfig { get; set; }
    }

    /// <summary>
    /// Entity representing Slack notification templates
    /// </summary>
    [Table("SlackNotificationTemplates")]
    public class SlackNotificationTemplate : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string TextTemplate { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Channel { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [StringLength(500)]
        public string? IconEmoji { get; set; }

        /// <summary>
        /// JSON serialized default attachments
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? DefaultAttachmentsJson { get; set; }

        /// <summary>
        /// JSON serialized default blocks
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? DefaultBlocksJson { get; set; }

        public bool UseAttachments { get; set; } = true;

        public bool UseBlocks { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        public string? Description { get; set; }

        // Foreign keys
        public Guid UserId { get; set; }
        public Guid? WebhookConfigId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("WebhookConfigId")]
        public virtual SlackWebhookConfig? WebhookConfig { get; set; }
    }

    /// <summary>
    /// Entity for tracking Slack integration statistics
    /// </summary>
    [Table("SlackIntegrationStats")]
    public class SlackIntegrationStat : BaseEntity
    {
        public DateTime StatDate { get; set; }

        [StringLength(1000)]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Channel { get; set; }

        public NotificationType NotificationType { get; set; }

        public int TotalMessages { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }

        public TimeSpan AverageResponseTime { get; set; }
        public TimeSpan MaxResponseTime { get; set; }
        public TimeSpan MinResponseTime { get; set; }

        public DateTime LastMessageSent { get; set; }

        /// <summary>
        /// JSON serialized error summary
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ErrorSummaryJson { get; set; }

        // Foreign key
        public Guid? WebhookConfigId { get; set; }
        public Guid? UserId { get; set; }

        // Navigation properties
        [ForeignKey("WebhookConfigId")]
        public virtual SlackWebhookConfig? WebhookConfig { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }

    /// <summary>
    /// Entity for Slack delivery failures and retry tracking
    /// </summary>
    [Table("SlackDeliveryFailures")]
    public class SlackDeliveryFailure : BaseEntity
    {
        [Required]
        [StringLength(1000)]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Channel { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string ErrorMessage { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public NotificationType NotificationType { get; set; }

        public DateTime FailedAt { get; set; }

        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Failed"; // Failed, Retrying, Resolved, Abandoned

        /// <summary>
        /// JSON serialized original message payload
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? OriginalPayloadJson { get; set; }

        // Foreign keys
        public Guid? WebhookConfigId { get; set; }
        public Guid? SlackMessageId { get; set; }
        public Guid? UserId { get; set; }

        // Navigation properties
        [ForeignKey("WebhookConfigId")]
        public virtual SlackWebhookConfig? WebhookConfig { get; set; }

        [ForeignKey("SlackMessageId")]
        public virtual SlackMessage? SlackMessage { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
