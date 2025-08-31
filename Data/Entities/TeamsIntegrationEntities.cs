using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectControlsReportingTool.API.Data.Entities
{
    /// <summary>
    /// Entity representing Microsoft Teams webhook configuration
    /// </summary>
    [Table("TeamsWebhookConfigs")]
    public class TeamsWebhookConfig : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string WebhookUrl { get; set; } = string.Empty;

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

        [StringLength(50)]
        public string DefaultFormat { get; set; } = "MessageCard"; // TeamsMessageFormat as string

        [StringLength(20)]
        public string? DefaultThemeColor { get; set; }

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
        public virtual ICollection<TeamsMessage> Messages { get; set; } = new List<TeamsMessage>();
        public virtual ICollection<TeamsNotificationTemplate> Templates { get; set; } = new List<TeamsNotificationTemplate>();
    }

    /// <summary>
    /// Entity representing a Teams message sent through the system
    /// </summary>
    [Table("TeamsMessages")]
    public class TeamsMessage : BaseEntity
    {
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(50)]
        public string MessageType { get; set; } = "Information"; // TeamsMessageType as string

        [StringLength(20)]
        public string? ThemeColor { get; set; }

        /// <summary>
        /// JSON serialized card actions
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ActionsJson { get; set; }

        /// <summary>
        /// JSON serialized facts/metadata
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? FactsJson { get; set; }

        public bool UseAdaptiveCard { get; set; } = false;

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
        public virtual TeamsWebhookConfig? WebhookConfig { get; set; }
    }

    /// <summary>
    /// Entity representing Teams notification templates
    /// </summary>
    [Table("TeamsNotificationTemplates")]
    public class TeamsNotificationTemplate : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        [StringLength(500)]
        public string TitleTemplate { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string MessageTemplate { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ThemeColor { get; set; }

        /// <summary>
        /// JSON serialized default actions
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? DefaultActionsJson { get; set; }

        public bool UseAdaptiveCard { get; set; } = false;

        /// <summary>
        /// JSON serialized default facts
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? DefaultFactsJson { get; set; }

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
        public virtual TeamsWebhookConfig? WebhookConfig { get; set; }
    }

    /// <summary>
    /// Entity for tracking Teams integration statistics
    /// </summary>
    [Table("TeamsIntegrationStats")]
    public class TeamsIntegrationStat : BaseEntity
    {
        public DateTime StatDate { get; set; }

        [StringLength(1000)]
        public string WebhookUrl { get; set; } = string.Empty;

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
        public virtual TeamsWebhookConfig? WebhookConfig { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }

    /// <summary>
    /// Entity for Teams delivery failures and retry tracking
    /// </summary>
    [Table("TeamsDeliveryFailures")]
    public class TeamsDeliveryFailure : BaseEntity
    {
        [Required]
        [StringLength(1000)]
        public string WebhookUrl { get; set; } = string.Empty;

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
        public Guid? TeamsMessageId { get; set; }
        public Guid? UserId { get; set; }

        // Navigation properties
        [ForeignKey("WebhookConfigId")]
        public virtual TeamsWebhookConfig? WebhookConfig { get; set; }

        [ForeignKey("TeamsMessageId")]
        public virtual TeamsMessage? TeamsMessage { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
