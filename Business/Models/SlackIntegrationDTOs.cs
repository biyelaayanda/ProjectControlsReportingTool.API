using System.ComponentModel.DataAnnotations;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Models
{
    #region Slack Integration DTOs

    /// <summary>
    /// DTO for sending messages to Slack
    /// </summary>
    public class SendSlackMessageDto
    {
        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Text { get; set; } = string.Empty;

        [Required]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Channel { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [StringLength(500)]
        public string? IconEmoji { get; set; }

        [StringLength(1000)]
        public string? IconUrl { get; set; }

        public List<SlackAttachment>? Attachments { get; set; }

        public List<SlackBlock>? Blocks { get; set; }

        [StringLength(100)]
        public string? ThreadTs { get; set; }

        public bool UnfurlLinks { get; set; } = true;
        public bool UnfurlMedia { get; set; } = true;
    }

    /// <summary>
    /// DTO for Slack message response
    /// </summary>
    public class SlackMessageResponseDto
    {
        public bool Success { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
        public string? Channel { get; set; }
        public string? ThreadTs { get; set; }
    }

    /// <summary>
    /// DTO for Slack attachment
    /// </summary>
    public class SlackAttachment
    {
        [StringLength(1000)]
        public string? Fallback { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        [StringLength(500)]
        public string? Pretext { get; set; }

        [StringLength(500)]
        public string? AuthorName { get; set; }

        [StringLength(1000)]
        public string? AuthorLink { get; set; }

        [StringLength(1000)]
        public string? AuthorIcon { get; set; }

        [StringLength(500)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? TitleLink { get; set; }

        [StringLength(4000)]
        public string? Text { get; set; }

        public List<SlackField>? Fields { get; set; }

        [StringLength(1000)]
        public string? ImageUrl { get; set; }

        [StringLength(1000)]
        public string? ThumbUrl { get; set; }

        [StringLength(500)]
        public string? Footer { get; set; }

        [StringLength(1000)]
        public string? FooterIcon { get; set; }

        public DateTime? Ts { get; set; }

        public List<SlackAction>? Actions { get; set; }
    }

    /// <summary>
    /// DTO for Slack field
    /// </summary>
    public class SlackField
    {
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Value { get; set; } = string.Empty;

        public bool Short { get; set; } = false;
    }

    /// <summary>
    /// DTO for Slack action
    /// </summary>
    public class SlackAction
    {
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = "button";

        [Required]
        [StringLength(500)]
        public string Text { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Url { get; set; }

        [StringLength(100)]
        public string? Style { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Value { get; set; }

        [StringLength(500)]
        public string? Confirm { get; set; }
    }

    /// <summary>
    /// DTO for Slack block (Block Kit)
    /// </summary>
    public class SlackBlock
    {
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(100)]
        public string? BlockId { get; set; }

        public object? Text { get; set; }

        public List<object>? Elements { get; set; }

        public object? Accessory { get; set; }

        public List<SlackField>? Fields { get; set; }
    }

    /// <summary>
    /// DTO for Slack webhook configuration
    /// </summary>
    public class SlackWebhookConfigDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Url]
        public string WebhookUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DefaultChannel { get; set; }

        [StringLength(100)]
        public string? DefaultUsername { get; set; }

        [StringLength(500)]
        public string? DefaultIconEmoji { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public List<NotificationType> EnabledNotifications { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public bool UseAttachments { get; set; } = true;

        public bool UseBlocks { get; set; } = false;

        public Dictionary<string, object>? CustomSettings { get; set; }
    }

    /// <summary>
    /// DTO for Slack integration test
    /// </summary>
    public class SlackIntegrationTestDto
    {
        [Required]
        [Url]
        public string WebhookUrl { get; set; } = string.Empty;

        public string TestMessage { get; set; } = "Test message from Project Controls Reporting Tool";

        [StringLength(100)]
        public string? Channel { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        public bool UseAttachments { get; set; } = true;
    }

    /// <summary>
    /// DTO for Slack integration test result
    /// </summary>
    public class SlackTestResultDto
    {
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public DateTime TestedAt { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
        public string? Channel { get; set; }
    }

    /// <summary>
    /// DTO for Slack notification template
    /// </summary>
    public class SlackNotificationTemplateDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        [StringLength(4000)]
        public string TextTemplate { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Channel { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [StringLength(500)]
        public string? IconEmoji { get; set; }

        public List<SlackAttachment>? DefaultAttachments { get; set; }

        public List<SlackBlock>? DefaultBlocks { get; set; }

        public bool UseAttachments { get; set; } = true;

        public bool UseBlocks { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating Slack notification template
    /// </summary>
    public class CreateSlackTemplateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        [StringLength(4000)]
        public string TextTemplate { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Channel { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [StringLength(500)]
        public string? IconEmoji { get; set; }

        public List<SlackAttachment>? DefaultAttachments { get; set; }

        public List<SlackBlock>? DefaultBlocks { get; set; }

        public bool UseAttachments { get; set; } = true;

        public bool UseBlocks { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for Slack integration statistics
    /// </summary>
    public class SlackIntegrationStatsDto
    {
        public int TotalMessagesSent { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public DateTime LastMessageSent { get; set; }
        public Dictionary<NotificationType, int> MessagesByType { get; set; } = new();
        public Dictionary<string, int> MessagesByChannel { get; set; } = new();
        public Dictionary<string, int> MessagesByWebhook { get; set; } = new();
        public List<SlackDeliveryFailureInfo> RecentFailures { get; set; } = new();
    }

    /// <summary>
    /// DTO for Slack delivery failure information
    /// </summary>
    public class SlackDeliveryFailureInfo
    {
        public DateTime FailedAt { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
        public string? Channel { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public NotificationType NotificationType { get; set; }
    }

    /// <summary>
    /// DTO for bulk Slack message sending
    /// </summary>
    public class BulkSlackMessageDto
    {
        [Required]
        public List<string> WebhookUrls { get; set; } = new();

        [Required]
        [StringLength(4000)]
        public string Text { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Channel { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [StringLength(500)]
        public string? IconEmoji { get; set; }

        public List<SlackAttachment>? Attachments { get; set; }

        public List<SlackBlock>? Blocks { get; set; }

        public bool ContinueOnError { get; set; } = true;

        public int MaxConcurrency { get; set; } = 5;
    }

    /// <summary>
    /// DTO for bulk Slack message result
    /// </summary>
    public class BulkSlackMessageResultDto
    {
        public int TotalWebhooks { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }
        public List<SlackMessageResponseDto> Results { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    #endregion

    #region Slack Message Format Enums

    /// <summary>
    /// Slack message attachment colors
    /// </summary>
    public enum SlackAttachmentColor
    {
        Good,
        Warning,
        Danger,
        Primary,
        Default
    }

    /// <summary>
    /// Slack button styles
    /// </summary>
    public enum SlackButtonStyle
    {
        Default,
        Primary,
        Danger
    }

    #endregion

    #region Slack Webhook Payloads

    /// <summary>
    /// Slack webhook payload structure
    /// </summary>
    public class SlackWebhookPayload
    {
        public string Text { get; set; } = string.Empty;
        public string? Channel { get; set; }
        public string? Username { get; set; }
        public string? IconEmoji { get; set; }
        public string? IconUrl { get; set; }
        public List<SlackAttachment>? Attachments { get; set; }
        public List<SlackBlock>? Blocks { get; set; }
        public string? ThreadTs { get; set; }
        public bool UnfurlLinks { get; set; } = true;
        public bool UnfurlMedia { get; set; } = true;
    }

    #endregion
}
