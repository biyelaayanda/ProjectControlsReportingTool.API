using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Controller for webhook management and integration APIs
    /// Provides comprehensive webhook subscription and delivery capabilities
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WebhooksController : ControllerBase
    {
        private readonly IWebhookService _webhookService;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(IWebhookService webhookService, ILogger<WebhooksController> logger)
        {
            _webhookService = webhookService;
            _logger = logger;
        }

        #region Webhook Management

        /// <summary>
        /// Creates a new webhook subscription
        /// </summary>
        /// <param name="createDto">Webhook subscription details</param>
        /// <returns>Created webhook subscription</returns>
        [HttpPost]
        [ProducesResponseType(typeof(WebhookSubscriptionDto), 201)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<WebhookSubscriptionDto>> CreateWebhookSubscription([FromBody] CreateWebhookSubscriptionDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var webhook = await _webhookService.CreateWebhookSubscriptionAsync(createDto, userId);

                _logger.LogInformation("User {UserId} created webhook subscription {WebhookId} for {WebhookUrl}", 
                    userId, webhook.Id, webhook.WebhookUrl);

                return CreatedAtAction(nameof(GetWebhookSubscriptions), new { }, webhook);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid webhook creation request: {Message}", ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating webhook subscription");
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to create webhook subscription"));
            }
        }

        /// <summary>
        /// Gets webhook subscriptions for the current user
        /// </summary>
        /// <param name="includeAll">Include subscriptions from all users (Admin only)</param>
        /// <returns>List of webhook subscriptions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<WebhookSubscriptionDto>), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<List<WebhookSubscriptionDto>>> GetWebhookSubscriptions([FromQuery] bool includeAll = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Only admins can see all webhooks
                Guid? filterUserId = (includeAll && userRole == UserRole.GM) ? null : userId;

                var webhooks = await _webhookService.GetWebhookSubscriptionsAsync(filterUserId);

                _logger.LogDebug("Retrieved {Count} webhook subscriptions for user {UserId}", 
                    webhooks.Count, userId);

                return Ok(webhooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving webhook subscriptions");
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retrieve webhook subscriptions"));
            }
        }

        /// <summary>
        /// Updates an existing webhook subscription
        /// </summary>
        /// <param name="webhookId">Webhook subscription ID</param>
        /// <param name="updateDto">Updated webhook details</param>
        /// <returns>Updated webhook subscription</returns>
        [HttpPut("{webhookId:guid}")]
        [ProducesResponseType(typeof(WebhookSubscriptionDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 403)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<WebhookSubscriptionDto>> UpdateWebhookSubscription(
            Guid webhookId, 
            [FromBody] CreateWebhookSubscriptionDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var webhook = await _webhookService.UpdateWebhookSubscriptionAsync(webhookId, updateDto, userId);

                _logger.LogInformation("User {UserId} updated webhook subscription {WebhookId}", 
                    userId, webhookId);

                return Ok(webhook);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Webhook {WebhookId} not found or invalid update request: {Message}", 
                    webhookId, ex.Message);
                return NotFound(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("User {UserId} unauthorized to update webhook {WebhookId}: {Message}", 
                    GetCurrentUserId(), webhookId, ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating webhook subscription {WebhookId}", webhookId);
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to update webhook subscription"));
            }
        }

        /// <summary>
        /// Deletes a webhook subscription
        /// </summary>
        /// <param name="webhookId">Webhook subscription ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{webhookId:guid}")]
        [ProducesResponseType(typeof(ServiceResultDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 403)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<ServiceResultDto>> DeleteWebhookSubscription(Guid webhookId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _webhookService.DeleteWebhookSubscriptionAsync(webhookId, userId);

                if (result.Success)
                {
                    _logger.LogInformation("User {UserId} deleted webhook subscription {WebhookId}", 
                        userId, webhookId);
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("User {UserId} unauthorized to delete webhook {WebhookId}", 
                    GetCurrentUserId(), webhookId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting webhook subscription {WebhookId}", webhookId);
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to delete webhook subscription"));
            }
        }

        #endregion

        #region Webhook Testing

        /// <summary>
        /// Tests a webhook endpoint
        /// </summary>
        /// <param name="testDto">Webhook test details</param>
        /// <returns>Test result</returns>
        [HttpPost("test")]
        [ProducesResponseType(typeof(WebhookTestResult), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<WebhookTestResult>> TestWebhook([FromBody] WebhookTestDto testDto)
        {
            try
            {
                var result = await _webhookService.TestWebhookAsync(testDto.WebhookUrl, testDto.SecretKey);

                _logger.LogInformation("User {UserId} tested webhook {WebhookUrl} - Success: {Success}, Status: {StatusCode}", 
                    GetCurrentUserId(), testDto.WebhookUrl, result.Success, result.StatusCode);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid webhook test request: {Message}", ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing webhook {WebhookUrl}", testDto.WebhookUrl);
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to test webhook"));
            }
        }

        #endregion

        #region Integration Information

        /// <summary>
        /// Gets available notification types for webhook subscriptions
        /// </summary>
        /// <returns>List of notification types</returns>
        [HttpGet("notification-types")]
        [ProducesResponseType(typeof(List<NotificationTypeInfo>), 200)]
        public ActionResult<List<NotificationTypeInfo>> GetNotificationTypes()
        {
            var notificationTypes = Enum.GetValues<NotificationType>()
                .Select(type => new NotificationTypeInfo
                {
                    Value = type,
                    Name = type.ToString(),
                    Description = GetNotificationTypeDescription(type)
                })
                .OrderBy(info => info.Name)
                .ToList();

            return Ok(notificationTypes);
        }

        /// <summary>
        /// Gets webhook integration documentation and examples
        /// </summary>
        /// <returns>Integration documentation</returns>
        [HttpGet("documentation")]
        [ProducesResponseType(typeof(WebhookDocumentationDto), 200)]
        public ActionResult<WebhookDocumentationDto> GetWebhookDocumentation()
        {
            var documentation = new WebhookDocumentationDto
            {
                Overview = "The Project Controls Reporting Tool supports webhook integrations for real-time event notifications. " +
                          "Webhooks allow external systems to receive immediate notifications when specific events occur.",
                
                Authentication = new WebhookAuthenticationInfo
                {
                    Method = "HMAC-SHA256",
                    HeaderName = "X-Webhook-Signature",
                    Description = "Webhooks are signed using HMAC-SHA256 with your secret key. " +
                                 "The signature is sent in the X-Webhook-Signature header as 'sha256=<signature>'."
                },

                PayloadFormat = new WebhookPayloadFormat
                {
                    ContentType = "application/json",
                    ExamplePayload = GetExampleWebhookPayload(),
                    RequiredHeaders = new List<string>
                    {
                        "X-Webhook-Event",
                        "X-Webhook-Delivery",
                        "X-Webhook-Signature (if secret key configured)",
                        "User-Agent: ProjectControlsReportingTool-Webhook/1.0"
                    }
                },

                DeliveryPolicy = new WebhookDeliveryPolicy
                {
                    TimeoutSeconds = "5-300 seconds (configurable)",
                    MaxRetries = "0-10 retries (configurable)",
                    RetryPolicy = "Exponential backoff: 1s, 2s, 4s, 8s, etc.",
                    ExpectedResponseCodes = "2xx status codes indicate success"
                },

                SecurityConsiderations = new List<string>
                {
                    "Always verify the webhook signature using your secret key",
                    "Use HTTPS endpoints to ensure secure transmission",
                    "Validate the webhook payload structure and data",
                    "Implement idempotency to handle duplicate deliveries",
                    "Rate limit webhook endpoints to prevent abuse",
                    "Log webhook deliveries for monitoring and debugging"
                }
            };

            return Ok(documentation);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Gets the current user's ID from JWT claims
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Gets the current user's role from JWT claims
        /// </summary>
        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.GeneralStaff;
        }

        /// <summary>
        /// Gets description for notification type
        /// </summary>
        private static string GetNotificationTypeDescription(NotificationType type)
        {
            return type switch
            {
                NotificationType.ReportSubmitted => "Triggered when a report is submitted for review",
                NotificationType.ApprovalRequired => "Triggered when a report requires approval",
                NotificationType.ReportApproved => "Triggered when a report is approved",
                NotificationType.ReportRejected => "Triggered when a report is rejected",
                NotificationType.DueDateReminder => "Triggered for upcoming report due dates",
                NotificationType.EscalationNotice => "Triggered for overdue reports requiring escalation",
                NotificationType.SystemAlert => "Triggered for system-wide alerts and announcements",
                NotificationType.UserWelcome => "Triggered when new users are created",
                NotificationType.PasswordReset => "Triggered for password reset requests",
                NotificationType.AccountActivation => "Triggered for account activation events",
                NotificationType.ReportComment => "Triggered when comments are added to reports",
                NotificationType.StatusChange => "Triggered when report status changes",
                NotificationType.BulkUpdate => "Triggered for bulk operation completions",
                NotificationType.MaintenanceNotice => "Triggered for system maintenance notifications",
                NotificationType.SecurityAlert => "Triggered for security-related events",
                _ => "System notification"
            };
        }

        /// <summary>
        /// Gets example webhook payload
        /// </summary>
        private static object GetExampleWebhookPayload()
        {
            return new
            {
                id = "550e8400-e29b-41d4-a716-446655440000",
                type = "report_submitted",
                timestamp = "2024-01-15T10:30:00Z",
                data = new
                {
                    title = "Monthly Progress Report Submitted",
                    message = "Report 'Project Alpha Status - January 2024' has been submitted for review",
                    priority = "Normal",
                    category = "Workflow",
                    recipient_id = "123e4567-e89b-12d3-a456-426614174000",
                    sender_id = "789e0123-e45f-67g8-h901-234567890abc",
                    related_entity_id = "report_12345",
                    related_entity_type = "Report",
                    action_url = "/reports/12345",
                    metadata = new
                    {
                        report_type = "Monthly",
                        department = "ProjectSupport",
                        submitted_by = "John Doe"
                    }
                }
            };
        }

        #endregion
    }

    #region Support DTOs

    /// <summary>
    /// DTO for webhook testing
    /// </summary>
    public class WebhookTestDto
    {
        [Required]
        [Url]
        public string WebhookUrl { get; set; } = string.Empty;
        
        public string? SecretKey { get; set; }
    }

    /// <summary>
    /// DTO for notification type information
    /// </summary>
    public class NotificationTypeInfo
    {
        public NotificationType Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for webhook documentation
    /// </summary>
    public class WebhookDocumentationDto
    {
        public string Overview { get; set; } = string.Empty;
        public WebhookAuthenticationInfo Authentication { get; set; } = new();
        public WebhookPayloadFormat PayloadFormat { get; set; } = new();
        public WebhookDeliveryPolicy DeliveryPolicy { get; set; } = new();
        public List<string> SecurityConsiderations { get; set; } = new();
    }

    /// <summary>
    /// DTO for webhook authentication information
    /// </summary>
    public class WebhookAuthenticationInfo
    {
        public string Method { get; set; } = string.Empty;
        public string HeaderName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for webhook payload format
    /// </summary>
    public class WebhookPayloadFormat
    {
        public string ContentType { get; set; } = string.Empty;
        public object? ExamplePayload { get; set; }
        public List<string> RequiredHeaders { get; set; } = new();
    }

    /// <summary>
    /// DTO for webhook delivery policy
    /// </summary>
    public class WebhookDeliveryPolicy
    {
        public string TimeoutSeconds { get; set; } = string.Empty;
        public string MaxRetries { get; set; } = string.Empty;
        public string RetryPolicy { get; set; } = string.Empty;
        public string ExpectedResponseCodes { get; set; } = string.Empty;
    }

    #endregion
}
