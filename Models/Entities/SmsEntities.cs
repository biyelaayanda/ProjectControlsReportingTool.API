using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    /// <summary>
    /// Entity representing an SMS message in the system
    /// </summary>
    [Table("SmsMessages")]
    public class SmsMessage : BaseEntity
    {
        /// <summary>
        /// External message ID from SMS provider
        /// </summary>
        [StringLength(100)]
        public string? ExternalMessageId { get; set; }

        /// <summary>
        /// Recipient phone number in E.164 format
        /// </summary>
        [Required]
        [Phone]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string Recipient { get; set; } = string.Empty;

        /// <summary>
        /// SMS message content (max 160 characters for standard SMS)
        /// </summary>
        [Required]
        [StringLength(1600)] // Allow for concatenated SMS
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Message delivery status
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Message priority level
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Priority { get; set; } = "Normal";

        /// <summary>
        /// Type of SMS message
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string MessageType { get; set; } = "Notification";

        /// <summary>
        /// Whether this is an urgent message requiring immediate delivery
        /// </summary>
        public bool IsUrgent { get; set; } = false;

        /// <summary>
        /// Cost of sending this message
        /// </summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal Cost { get; set; } = 0;

        /// <summary>
        /// SMS provider used for delivery
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Error message if delivery failed
        /// </summary>
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// When the message was sent to the provider
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the message was delivered (if confirmed by provider)
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// When the message was read (if supported by provider)
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// User who initiated this SMS (optional)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Navigation property to User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        /// <summary>
        /// Related report ID (if this SMS is about a specific report)
        /// </summary>
        public Guid? RelatedReportId { get; set; }

        /// <summary>
        /// Navigation property to Report
        /// </summary>
        [ForeignKey(nameof(RelatedReportId))]
        public virtual Report? RelatedReport { get; set; }

        /// <summary>
        /// Batch ID for bulk SMS operations
        /// </summary>
        public Guid? BatchId { get; set; }

        /// <summary>
        /// Number of SMS segments used (for long messages)
        /// </summary>
        public int SegmentCount { get; set; } = 1;

        /// <summary>
        /// Character encoding used (GSM7, UCS2, etc.)
        /// </summary>
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string Encoding { get; set; } = "GSM7";

        /// <summary>
        /// Country code of recipient
        /// </summary>
        [StringLength(5)]
        [Column(TypeName = "varchar(5)")]
        public string? CountryCode { get; set; }

        /// <summary>
        /// Carrier/network of recipient (if available)
        /// </summary>
        [StringLength(100)]
        public string? Carrier { get; set; }

        /// <summary>
        /// Number of delivery attempts
        /// </summary>
        public int DeliveryAttempts { get; set; } = 0;

        /// <summary>
        /// Last delivery attempt timestamp
        /// </summary>
        public DateTime? LastDeliveryAttempt { get; set; }

        /// <summary>
        /// When to retry delivery (for failed messages)
        /// </summary>
        public DateTime? RetryAt { get; set; }

        /// <summary>
        /// Additional metadata as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Template used for this message (if any)
        /// </summary>
        public Guid? TemplateId { get; set; }

        /// <summary>
        /// Navigation property to SMS Template
        /// </summary>
        [ForeignKey(nameof(TemplateId))]
        public virtual SmsTemplate? Template { get; set; }

        /// <summary>
        /// Whether delivery receipt was requested
        /// </summary>
        public bool DeliveryReceiptRequested { get; set; } = true;

        /// <summary>
        /// Whether this message contains sensitive information
        /// </summary>
        public bool IsSensitive { get; set; } = false;

        /// <summary>
        /// When this message expires and should not be delivered
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Calculated method to determine if message is expired
        /// </summary>
        [NotMapped]
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

        /// <summary>
        /// Calculated method to determine if message is pending
        /// </summary>
        [NotMapped]
        public bool IsPending => Status == "Pending" || Status == "Queued";

        /// <summary>
        /// Calculated method to determine if message was delivered
        /// </summary>
        [NotMapped]
        public bool IsDelivered => Status == "Delivered" || Status == "Read";

        /// <summary>
        /// Calculated method to determine if message failed
        /// </summary>
        [NotMapped]
        public bool IsFailed => Status == "Failed" || Status == "Expired" || Status == "Rejected";

        /// <summary>
        /// Calculated property for delivery time (if delivered)
        /// </summary>
        [NotMapped]
        public TimeSpan? DeliveryTime => DeliveredAt.HasValue ? DeliveredAt.Value - SentAt : null;
    }

    /// <summary>
    /// Entity representing SMS templates for standardized messaging
    /// </summary>
    [Table("SmsTemplates")]
    public class SmsTemplate : BaseEntity
    {
        /// <summary>
        /// Template name/identifier
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Template category (Notifications, Alerts, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// SMS template content with variable placeholders
        /// </summary>
        [Required]
        [StringLength(1600)] // Allow for longer templates
        public string Template { get; set; } = string.Empty;

        /// <summary>
        /// Template description
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether this template is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this is a system template (read-only)
        /// </summary>
        public bool IsSystemTemplate { get; set; } = false;

        /// <summary>
        /// Variables available in this template (JSON array)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Variables { get; set; } = "[]";

        /// <summary>
        /// How many times this template has been used
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// Last time this template was used
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// User who created this template
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Navigation property to creator
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User CreatedByUser { get; set; } = null!;

        /// <summary>
        /// User who last updated this template
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Navigation property to updater
        /// </summary>
        [ForeignKey(nameof(UpdatedBy))]
        public virtual User? UpdatedByUser { get; set; }

        /// <summary>
        /// Default priority for messages using this template
        /// </summary>
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string DefaultPriority { get; set; } = "Normal";

        /// <summary>
        /// Default message type for this template
        /// </summary>
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string DefaultMessageType { get; set; } = "Notification";

        /// <summary>
        /// Whether messages from this template are urgent by default
        /// </summary>
        public bool DefaultIsUrgent { get; set; } = false;

        /// <summary>
        /// Maximum length when rendered (for validation)
        /// </summary>
        public int MaxRenderedLength { get; set; } = 160;

        /// <summary>
        /// Template tags for categorization and search
        /// </summary>
        [StringLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// Navigation property to SMS messages using this template
        /// </summary>
        public virtual ICollection<SmsMessage> SmsMessages { get; set; } = new List<SmsMessage>();

        /// <summary>
        /// Calculated property to get variables as a list
        /// </summary>
        [NotMapped]
        public List<string> VariablesList
        {
            get
            {
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Variables) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        /// <summary>
        /// Calculated property to check if template is editable
        /// </summary>
        [NotMapped]
        public bool IsEditable => !IsSystemTemplate && IsActive;
    }

    /// <summary>
    /// Entity for tracking SMS delivery statistics and metrics
    /// </summary>
    [Table("SmsStatistics")]
    public class SmsStatistic : BaseEntity
    {
        /// <summary>
        /// Date for these statistics
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateOnly Date { get; set; }

        /// <summary>
        /// SMS provider name
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Total messages sent
        /// </summary>
        public int MessagesSent { get; set; } = 0;

        /// <summary>
        /// Successfully delivered messages
        /// </summary>
        public int MessagesDelivered { get; set; } = 0;

        /// <summary>
        /// Failed delivery messages
        /// </summary>
        public int MessagesFailed { get; set; } = 0;

        /// <summary>
        /// Messages still pending
        /// </summary>
        public int MessagesPending { get; set; } = 0;

        /// <summary>
        /// Total cost for the day
        /// </summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal TotalCost { get; set; } = 0;

        /// <summary>
        /// Average delivery time in seconds
        /// </summary>
        public double AverageDeliveryTime { get; set; } = 0;

        /// <summary>
        /// Delivery rate percentage
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal DeliveryRate { get; set; } = 0;

        /// <summary>
        /// Number of unique recipients
        /// </summary>
        public int UniqueRecipients { get; set; } = 0;

        /// <summary>
        /// Messages by type (JSON)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string MessagesByType { get; set; } = "{}";

        /// <summary>
        /// Error breakdown (JSON)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string ErrorBreakdown { get; set; } = "{}";

        /// <summary>
        /// Country code statistics (JSON)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string CountryStats { get; set; } = "{}";
    }
}
