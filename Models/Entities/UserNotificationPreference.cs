using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    /// <summary>
    /// User notification preferences entity for managing user-specific notification settings
    /// Allows users to customize how they receive different types of notifications
    /// </summary>
    [Table("UserNotificationPreferences")]
    public class UserNotificationPreference
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Notification type (e.g., "ReportStatusUpdate", "WorkflowDeadline", "SystemBroadcast")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string NotificationType { get; set; } = string.Empty;

        /// <summary>
        /// Enable/disable email notifications for this type
        /// </summary>
        public bool EmailEnabled { get; set; } = true;

        /// <summary>
        /// Enable/disable real-time (WebSocket) notifications for this type
        /// </summary>
        public bool RealTimeEnabled { get; set; } = true;

        /// <summary>
        /// Enable/disable push notifications for this type
        /// </summary>
        public bool PushEnabled { get; set; } = false;

        /// <summary>
        /// Enable/disable SMS notifications for this type (future feature)
        /// </summary>
        public bool SmsEnabled { get; set; } = false;

        /// <summary>
        /// Custom notification schedule (e.g., "immediate", "daily_digest", "weekly_summary")
        /// </summary>
        [StringLength(50)]
        public string Schedule { get; set; } = "immediate";

        /// <summary>
        /// Priority threshold - only notify for notifications above this priority
        /// Values: "Low", "Normal", "High", "Critical"
        /// </summary>
        [StringLength(20)]
        public string MinimumPriority { get; set; } = "Low";

        /// <summary>
        /// Quiet hours start time (24-hour format, e.g., "22:00")
        /// </summary>
        [StringLength(5)]
        public string? QuietHoursStart { get; set; }

        /// <summary>
        /// Quiet hours end time (24-hour format, e.g., "08:00")
        /// </summary>
        [StringLength(5)]
        public string? QuietHoursEnd { get; set; }

        /// <summary>
        /// Time zone for quiet hours (e.g., "UTC+02:00", "Africa/Johannesburg")
        /// </summary>
        [StringLength(50)]
        public string TimeZone { get; set; } = "UTC";

        /// <summary>
        /// When this preference was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this preference was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property to User
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
