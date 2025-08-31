using System.ComponentModel.DataAnnotations;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    /// <summary>
    /// DTO for user notification preference display
    /// </summary>
    public class UserNotificationPreferenceDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public bool EmailEnabled { get; set; }
        public bool RealTimeEnabled { get; set; }
        public bool PushEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public string Schedule { get; set; } = string.Empty;
        public string MinimumPriority { get; set; } = string.Empty;
        public string? QuietHoursStart { get; set; }
        public string? QuietHoursEnd { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating user notification preferences
    /// </summary>
    public class CreateUserNotificationPreferenceDto
    {
        [Required]
        [StringLength(100)]
        public string NotificationType { get; set; } = string.Empty;

        public bool EmailEnabled { get; set; } = true;
        public bool RealTimeEnabled { get; set; } = true;
        public bool PushEnabled { get; set; } = false;
        public bool SmsEnabled { get; set; } = false;

        [StringLength(50)]
        public string Schedule { get; set; } = "immediate";

        [Required]
        [StringLength(20)]
        public string MinimumPriority { get; set; } = "Low";

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string? QuietHoursStart { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string? QuietHoursEnd { get; set; }

        [StringLength(50)]
        public string TimeZone { get; set; } = "UTC";
    }

    /// <summary>
    /// DTO for updating user notification preferences
    /// </summary>
    public class UpdateUserNotificationPreferenceDto
    {
        public bool? EmailEnabled { get; set; }
        public bool? RealTimeEnabled { get; set; }
        public bool? PushEnabled { get; set; }
        public bool? SmsEnabled { get; set; }

        [StringLength(50)]
        public string? Schedule { get; set; }

        [StringLength(20)]
        public string? MinimumPriority { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string? QuietHoursStart { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string? QuietHoursEnd { get; set; }

        [StringLength(50)]
        public string? TimeZone { get; set; }
    }

    /// <summary>
    /// DTO for bulk notification preference updates
    /// </summary>
    public class BulkNotificationPreferenceUpdateDto
    {
        [Required]
        public List<string> NotificationTypes { get; set; } = new();

        public bool? EmailEnabled { get; set; }
        public bool? RealTimeEnabled { get; set; }
        public bool? PushEnabled { get; set; }
        public bool? SmsEnabled { get; set; }
        public string? Schedule { get; set; }
        public string? MinimumPriority { get; set; }
        
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string? QuietHoursStart { get; set; }
        
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string? QuietHoursEnd { get; set; }
        
        [StringLength(50)]
        public string? TimeZone { get; set; }
    }

    /// <summary>
    /// DTO for notification preference statistics
    /// </summary>
    public class NotificationPreferenceStatsDto
    {
        public int TotalPreferences { get; set; }
        public int EmailEnabledCount { get; set; }
        public int RealTimeEnabledCount { get; set; }
        public int PushEnabledCount { get; set; }
        public int SmsEnabledCount { get; set; }
        public bool HasQuietHours { get; set; }
        public string MostCommonPriority { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, int> PreferencesByType { get; set; } = new();
        public Dictionary<string, int> PreferencesBySchedule { get; set; } = new();
        public Dictionary<string, int> PreferencesByPriority { get; set; } = new();
    }

    /// <summary>
    /// DTO for notification preference templates (admin-defined defaults)
    /// </summary>
    public class NotificationPreferenceTemplateDto
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public bool EmailEnabled { get; set; }
        public bool RealTimeEnabled { get; set; }
        public bool PushEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public string Schedule { get; set; } = string.Empty;
        public string MinimumPriority { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for notification delivery summary
    /// </summary>
    public class NotificationDeliverySummaryDto
    {
        public Guid UserId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool ShouldSendEmail { get; set; }
        public bool ShouldSendRealTime { get; set; }
        public bool ShouldSendPush { get; set; }
        public bool ShouldSendSms { get; set; }
        public bool IsInQuietHours { get; set; }
        public DateTime CheckedAt { get; set; }
    }
}
