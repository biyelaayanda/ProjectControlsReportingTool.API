using System.ComponentModel.DataAnnotations;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Models
{
    #region Microsoft Teams Integration DTOs

    /// <summary>
    /// DTO for sending messages to Microsoft Teams
    /// </summary>
    public class SendTeamsMessageDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Title { get; set; }

        public TeamsMessageType MessageType { get; set; } = TeamsMessageType.Information;

        public List<TeamsCardAction>? Actions { get; set; }

        public Dictionary<string, object>? Facts { get; set; }

        [StringLength(1000)]
        public string? ThemeColor { get; set; }

        public bool UseAdaptiveCard { get; set; } = false;
    }

    /// <summary>
    /// DTO for Teams message response
    /// </summary>
    public class TeamsMessageResponseDto
    {
        public bool Success { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for Teams card actions
    /// </summary>
    public class TeamsCardAction
    {
        [Required]
        public string Type { get; set; } = "OpenUri";

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Target { get; set; } = string.Empty;

        public Dictionary<string, object>? Properties { get; set; }
    }

    /// <summary>
    /// DTO for Teams webhook configuration
    /// </summary>
    public class TeamsWebhookConfigDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Url]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public List<NotificationType> EnabledNotifications { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public TeamsMessageFormat DefaultFormat { get; set; } = TeamsMessageFormat.MessageCard;

        [StringLength(20)]
        public string? DefaultThemeColor { get; set; }

        public Dictionary<string, object>? CustomSettings { get; set; }
    }

    /// <summary>
    /// DTO for Teams integration test
    /// </summary>
    public class TeamsIntegrationTestDto
    {
        [Required]
        [Url]
        public string WebhookUrl { get; set; } = string.Empty;

        public string TestMessage { get; set; } = "Test message from Project Controls Reporting Tool";

        public TeamsMessageType MessageType { get; set; } = TeamsMessageType.Information;
    }

    /// <summary>
    /// DTO for Teams integration test result
    /// </summary>
    public class TeamsTestResultDto
    {
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public DateTime TestedAt { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for Teams notification template
    /// </summary>
    public class TeamsNotificationTemplateDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        public string TitleTemplate { get; set; } = string.Empty;

        [Required]
        public string MessageTemplate { get; set; } = string.Empty;

        public string? ThemeColor { get; set; }

        public List<TeamsCardAction>? DefaultActions { get; set; }

        public bool UseAdaptiveCard { get; set; } = false;

        public Dictionary<string, object>? DefaultFacts { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating Teams notification template
    /// </summary>
    public class CreateTeamsTemplateDto
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
        [StringLength(2000)]
        public string MessageTemplate { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ThemeColor { get; set; }

        public List<TeamsCardAction>? DefaultActions { get; set; }

        public bool UseAdaptiveCard { get; set; } = false;

        public Dictionary<string, object>? DefaultFacts { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for Teams integration statistics
    /// </summary>
    public class TeamsIntegrationStatsDto
    {
        public int TotalMessagesSent { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public DateTime LastMessageSent { get; set; }
        public Dictionary<NotificationType, int> MessagesByType { get; set; } = new();
        public Dictionary<string, int> MessagesByWebhook { get; set; } = new();
        public List<TeamsMessageDeliveryFailure> RecentFailures { get; set; } = new();
    }

    /// <summary>
    /// DTO for Teams delivery failure information (to avoid namespace conflicts)
    /// </summary>
    public class TeamsMessageDeliveryFailure
    {
        public DateTime FailedAt { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public NotificationType NotificationType { get; set; }
    }

    /// <summary>
    /// DTO for bulk Teams message sending
    /// </summary>
    public class BulkTeamsMessageDto
    {
        [Required]
        public List<string> WebhookUrls { get; set; } = new();

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Title { get; set; }

        public TeamsMessageType MessageType { get; set; } = TeamsMessageType.Information;

        public List<TeamsCardAction>? Actions { get; set; }

        public Dictionary<string, object>? Facts { get; set; }

        public bool ContinueOnError { get; set; } = true;

        public int MaxConcurrency { get; set; } = 5;
    }

    /// <summary>
    /// DTO for bulk Teams message result
    /// </summary>
    public class BulkTeamsMessageResultDto
    {
        public int TotalWebhooks { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }
        public List<TeamsMessageResponseDto> Results { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    #endregion

    #region Teams Message Enums

    /// <summary>
    /// Teams message types for different visual styling
    /// </summary>
    public enum TeamsMessageType
    {
        Information,
        Success,
        Warning,
        Error,
        Alert
    }

    /// <summary>
    /// Teams message format options
    /// </summary>
    public enum TeamsMessageFormat
    {
        SimpleText,
        MessageCard,
        AdaptiveCard
    }

    #endregion

    #region Teams Message Payloads

    /// <summary>
    /// Teams message card payload structure
    /// </summary>
    public class TeamsMessageCardPayload
    {
        public string Type { get; set; } = "MessageCard";
        public string Context { get; set; } = "https://schema.org/extensions";
        public string? Summary { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public string? ThemeColor { get; set; }
        public List<object>? Sections { get; set; }
        public List<object>? PotentialAction { get; set; }
    }

    /// <summary>
    /// Teams adaptive card payload structure
    /// </summary>
    public class TeamsAdaptiveCardPayload
    {
        public string Type { get; set; } = "message";
        public List<object> Attachments { get; set; } = new();
    }

    /// <summary>
    /// Teams simple text payload structure
    /// </summary>
    public class TeamsSimpleTextPayload
    {
        public string Text { get; set; } = string.Empty;
    }

    #endregion
}
