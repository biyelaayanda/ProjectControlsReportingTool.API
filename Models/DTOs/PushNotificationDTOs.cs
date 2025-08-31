using System.ComponentModel.DataAnnotations;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    /// <summary>
    /// DTO for displaying push notification subscription information
    /// </summary>
    public class PushNotificationSubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUsed { get; set; }
        public int SuccessfulNotifications { get; set; }
        public int FailedNotifications { get; set; }
        public string? LastError { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? BrowserInfo { get; set; }
        public string? OperatingSystem { get; set; }
        public bool HasPermission { get; set; }
        public bool EnabledForReports { get; set; }
        public bool EnabledForApprovals { get; set; }
        public bool EnabledForDeadlines { get; set; }
        public bool EnabledForAnnouncements { get; set; }
        public bool EnabledForMentions { get; set; }
        public bool EnabledForReminders { get; set; }
        public string MinimumPriority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new push notification subscription
    /// </summary>
    public class CreatePushNotificationSubscriptionDto
    {
        [Required]
        [StringLength(500)]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string P256dhKey { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string AuthToken { get; set; } = string.Empty;

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(50)]
        public string DeviceType { get; set; } = "Web";

        [StringLength(200)]
        public string? DeviceName { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [StringLength(200)]
        public string? BrowserInfo { get; set; }

        [StringLength(100)]
        public string? OperatingSystem { get; set; }

        public bool EnabledForReports { get; set; } = true;
        public bool EnabledForApprovals { get; set; } = true;
        public bool EnabledForDeadlines { get; set; } = true;
        public bool EnabledForAnnouncements { get; set; } = true;
        public bool EnabledForMentions { get; set; } = true;
        public bool EnabledForReminders { get; set; } = true;

        [StringLength(20)]
        public string MinimumPriority { get; set; } = "Normal";
    }

    /// <summary>
    /// DTO for updating push notification subscription preferences
    /// </summary>
    public class UpdatePushNotificationSubscriptionDto
    {
        [StringLength(200)]
        public string? DeviceName { get; set; }

        public bool? IsActive { get; set; }

        public bool? EnabledForReports { get; set; }
        public bool? EnabledForApprovals { get; set; }
        public bool? EnabledForDeadlines { get; set; }
        public bool? EnabledForAnnouncements { get; set; }
        public bool? EnabledForMentions { get; set; }
        public bool? EnabledForReminders { get; set; }

        [StringLength(20)]
        public string? MinimumPriority { get; set; }
    }

    /// <summary>
    /// DTO for push notification subscription statistics
    /// </summary>
    public class PushNotificationSubscriptionStatsDto
    {
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int InactiveSubscriptions { get; set; }
        public int WebSubscriptions { get; set; }
        public int MobileSubscriptions { get; set; }
        public Dictionary<string, int> SubscriptionsByDevice { get; set; } = new();
        public Dictionary<string, int> SubscriptionsByBrowser { get; set; } = new();
        public int TotalNotificationsSent { get; set; }
        public int SuccessfulNotifications { get; set; }
        public int FailedNotifications { get; set; }
        public double SuccessRate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for sending push notifications
    /// </summary>
    public class SendPushNotificationDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Body { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Icon { get; set; }

        [StringLength(500)]
        public string? Image { get; set; }

        [StringLength(200)]
        public string? Badge { get; set; }

        [StringLength(500)]
        public string? Url { get; set; }

        [StringLength(50)]
        public string? Tag { get; set; }

        public bool RequireInteraction { get; set; } = false;

        public bool Silent { get; set; } = false;

        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        [StringLength(50)]
        public string NotificationType { get; set; } = "General";

        public Dictionary<string, object> Data { get; set; } = new();

        public List<string> Actions { get; set; } = new();

        // Targeting options
        public List<Guid>? UserIds { get; set; }
        public List<string>? DeviceTypes { get; set; }
        public string? MinimumPriority { get; set; }
        public bool? OnlyActiveDevices { get; set; } = true;
    }

    /// <summary>
    /// DTO for push notification delivery result
    /// </summary>
    public class PushNotificationDeliveryDto
    {
        public Guid NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int TotalTargeted { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double SuccessRate { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public TimeSpan DeliveryTime { get; set; }
    }

    /// <summary>
    /// DTO for testing push notification functionality
    /// </summary>
    public class TestPushNotificationDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "Test Notification";

        [Required]
        [StringLength(500)]
        public string Body { get; set; } = "This is a test push notification from Project Controls Reporting Tool.";

        [StringLength(200)]
        public string? Icon { get; set; }

        [StringLength(500)]
        public string? Url { get; set; }

        public Guid? TargetUserId { get; set; }
        public string? TargetSubscriptionId { get; set; }
    }

    /// <summary>
    /// DTO for bulk push notification operations
    /// </summary>
    public class BulkPushNotificationOperationDto
    {
        [Required]
        public List<Guid> SubscriptionIds { get; set; } = new();

        [Required]
        [StringLength(50)]
        public string Operation { get; set; } = string.Empty; // activate, deactivate, delete, test

        public UpdatePushNotificationSubscriptionDto? UpdateData { get; set; }
        public TestPushNotificationDto? TestNotification { get; set; }
    }

    /// <summary>
    /// DTO for push notification subscription search and filtering
    /// </summary>
    public class PushNotificationSubscriptionSearchDto
    {
        public string? SearchTerm { get; set; }
        public Guid? UserId { get; set; }
        public string? DeviceType { get; set; }
        public bool? IsActive { get; set; }
        public bool? HasPermission { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? LastUsedFrom { get; set; }
        public DateTime? LastUsedTo { get; set; }
        public string? MinimumPriority { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
