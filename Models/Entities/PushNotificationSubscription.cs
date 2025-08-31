using System.ComponentModel.DataAnnotations;
using ProjectControlsReportingTool.API.Models.Entities.Base;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    /// <summary>
    /// Entity representing a user's push notification subscription
    /// Stores browser/device registration information for Web Push API
    /// </summary>
    public class PushNotificationSubscription : BaseEntity
    {
        /// <summary>
        /// User who owns this subscription
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Unique endpoint URL for this subscription (from browser/device)
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// P256DH key for encryption (from Web Push API)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string P256dhKey { get; set; } = string.Empty;

        /// <summary>
        /// Auth token for authentication (from Web Push API)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string AuthToken { get; set; } = string.Empty;

        /// <summary>
        /// User agent string of the subscribing browser/device
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Device type (Web, Android, iOS, etc.)
        /// </summary>
        [StringLength(50)]
        public string DeviceType { get; set; } = "Web";

        /// <summary>
        /// Device name/identifier for user reference
        /// </summary>
        [StringLength(200)]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Whether this subscription is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Last time this subscription was successfully used
        /// </summary>
        public DateTime? LastUsed { get; set; }

        /// <summary>
        /// Number of successful notifications sent to this subscription
        /// </summary>
        public int SuccessfulNotifications { get; set; } = 0;

        /// <summary>
        /// Number of failed notifications for this subscription
        /// </summary>
        public int FailedNotifications { get; set; } = 0;

        /// <summary>
        /// Last error message if subscription failed
        /// </summary>
        [StringLength(1000)]
        public string? LastError { get; set; }

        /// <summary>
        /// When the subscription expires (if applicable)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// IP address when subscription was created
        /// </summary>
        [StringLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Browser/device information
        /// </summary>
        [StringLength(200)]
        public string? BrowserInfo { get; set; }

        /// <summary>
        /// Operating system information
        /// </summary>
        [StringLength(100)]
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// Whether user has granted notification permissions
        /// </summary>
        public bool HasPermission { get; set; } = true;

        /// <summary>
        /// User notification preferences for push notifications
        /// </summary>
        public bool EnabledForReports { get; set; } = true;
        public bool EnabledForApprovals { get; set; } = true;
        public bool EnabledForDeadlines { get; set; } = true;
        public bool EnabledForAnnouncements { get; set; } = true;
        public bool EnabledForMentions { get; set; } = true;
        public bool EnabledForReminders { get; set; } = true;

        /// <summary>
        /// Minimum priority level for push notifications
        /// </summary>
        [StringLength(20)]
        public string MinimumPriority { get; set; } = "Normal"; // Low, Normal, High, Critical

        /// <summary>
        /// Navigation property to User
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Validate subscription data
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Endpoint) &&
                   !string.IsNullOrEmpty(P256dhKey) &&
                   !string.IsNullOrEmpty(AuthToken) &&
                   IsActive &&
                   HasPermission;
        }

        /// <summary>
        /// Check if subscription should receive notification based on type and priority
        /// </summary>
        public bool ShouldReceiveNotification(string notificationType, string priority)
        {
            if (!IsValid()) return false;

            // Check priority threshold
            var priorityLevel = GetPriorityLevel(priority);
            var minimumLevel = GetPriorityLevel(MinimumPriority);
            if (priorityLevel < minimumLevel) return false;

            // Check notification type preferences
            return notificationType.ToLower() switch
            {
                "report" or "reportcreated" or "reportupdated" => EnabledForReports,
                "approval" or "approvalrequired" or "approved" or "rejected" => EnabledForApprovals,
                "deadline" or "deadlineapproaching" or "overdue" => EnabledForDeadlines,
                "announcement" or "broadcast" => EnabledForAnnouncements,
                "mention" or "mentioned" => EnabledForMentions,
                "reminder" => EnabledForReminders,
                _ => true // Default to enabled for unknown types
            };
        }

        private static int GetPriorityLevel(string priority)
        {
            return priority.ToLower() switch
            {
                "low" => 1,
                "normal" => 2,
                "high" => 3,
                "critical" => 4,
                _ => 2 // Default to Normal
            };
        }
    }
}
