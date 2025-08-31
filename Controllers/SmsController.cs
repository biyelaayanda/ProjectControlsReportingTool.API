using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Controller for SMS operations and management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly ILogger<SmsController> _logger;

        public SmsController(ISmsService smsService, ILogger<SmsController> logger)
        {
            _smsService = smsService;
            _logger = logger;
        }

        /// <summary>
        /// Send a single SMS message
        /// </summary>
        /// <param name="sendSmsDto">SMS details</param>
        /// <returns>Delivery result</returns>
        [HttpPost("send")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<SmsDeliveryDto>> SendSms([FromBody] SendSmsDto sendSmsDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _smsService.SendSmsAsync(sendSmsDto, userId);

                if (!result.IsDelivered)
                {
                    return BadRequest(new { message = "Failed to send SMS", error = result.ErrorMessage });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Send SMS using a template
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number</param>
        /// <param name="templateId">Template ID</param>
        /// <param name="variables">Template variables</param>
        /// <returns>Delivery result</returns>
        [HttpPost("send/template")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<SmsDeliveryDto>> SendSmsFromTemplate(
            [FromQuery] string phoneNumber,
            [FromQuery] Guid templateId,
            [FromBody] Dictionary<string, object> variables)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _smsService.SendSmsFromTemplateAsync(phoneNumber, templateId, variables, userId);

                if (!result.IsDelivered)
                {
                    return BadRequest(new { message = "Failed to send SMS from template", error = result.ErrorMessage });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS from template");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Send bulk SMS messages
        /// </summary>
        /// <param name="bulkSmsDto">Bulk SMS details</param>
        /// <returns>Bulk delivery result</returns>
        [HttpPost("send/bulk")]
        [Authorize(Roles = "GM")]
        public async Task<ActionResult<BulkSmsDeliveryDto>> SendBulkSms([FromBody] BulkSmsDto bulkSmsDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _smsService.SendBulkSmsAsync(bulkSmsDto, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk SMS");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Send test SMS message
        /// </summary>
        /// <param name="testSmsDto">Test SMS details</param>
        /// <returns>Delivery result</returns>
        [HttpPost("test")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<SmsDeliveryDto>> SendTestSms([FromBody] TestSmsDto testSmsDto)
        {
            try
            {
                var result = await _smsService.SendTestSmsAsync(testSmsDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test SMS");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS delivery status
        /// </summary>
        /// <param name="messageId">Message ID</param>
        /// <returns>SMS message details</returns>
        [HttpGet("{messageId}/status")]
        public async Task<ActionResult<SmsMessageDto>> GetSmsStatus(Guid messageId)
        {
            try
            {
                var result = await _smsService.GetSmsStatusAsync(messageId);
                
                if (result == null)
                {
                    return NotFound(new { message = "SMS message not found" });
                }

                // Check if user can access this SMS message
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                if (userRole != UserRole.GM && result.UserId != currentUserId)
                {
                    return Forbid("You can only view your own SMS messages");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS status for message {MessageId}", messageId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS messages with filtering and pagination
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Paginated SMS messages</returns>
        [HttpPost("search")]
        public async Task<ActionResult<object>> GetSmsMessages([FromBody] SmsSearchDto searchDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Non-GM users can only see their own messages
                if (userRole != UserRole.GM)
                {
                    searchDto.UserId = currentUserId;
                }

                var (messages, totalCount) = await _smsService.GetSmsMessagesAsync(searchDto);

                var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

                return Ok(new
                {
                    messages,
                    pagination = new
                    {
                        currentPage = searchDto.Page,
                        pageSize = searchDto.PageSize,
                        totalCount,
                        totalPages,
                        hasNextPage = searchDto.Page < totalPages,
                        hasPreviousPage = searchDto.Page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS messages");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS statistics
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="provider">Optional provider filter</param>
        /// <returns>SMS statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<SmsStatisticsDto>> GetSmsStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? provider = null)
        {
            try
            {
                var result = await _smsService.GetSmsStatisticsAsync(fromDate, toDate, provider);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS statistics");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Retry failed SMS message
        /// </summary>
        /// <param name="messageId">Message ID to retry</param>
        /// <returns>Delivery result</returns>
        [HttpPost("{messageId}/retry")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<SmsDeliveryDto>> RetrySms(Guid messageId)
        {
            try
            {
                var result = await _smsService.RetrySmsAsync(messageId);

                if (!result.IsDelivered)
                {
                    return BadRequest(new { message = "Failed to retry SMS", error = result.ErrorMessage });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying SMS message {MessageId}", messageId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel pending SMS message
        /// </summary>
        /// <param name="messageId">Message ID to cancel</param>
        /// <returns>Success status</returns>
        [HttpPost("{messageId}/cancel")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult> CancelSms(Guid messageId)
        {
            try
            {
                var success = await _smsService.CancelSmsAsync(messageId);

                if (!success)
                {
                    return BadRequest(new { message = "Failed to cancel SMS message" });
                }

                return Ok(new { message = "SMS message cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling SMS message {MessageId}", messageId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS templates
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <param name="activeOnly">Only active templates</param>
        /// <returns>SMS templates</returns>
        [HttpGet("templates")]
        public async Task<ActionResult<List<SmsTemplateDto>>> GetSmsTemplates(
            [FromQuery] string? category = null,
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                var templates = await _smsService.GetSmsTemplatesAsync(category, activeOnly);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS templates");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Create SMS template
        /// </summary>
        /// <param name="createDto">Template details</param>
        /// <returns>Created template</returns>
        [HttpPost("templates")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<SmsTemplateDto>> CreateSmsTemplate([FromBody] CreateSmsTemplateDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var template = await _smsService.CreateSmsTemplateAsync(createDto, userId);
                return CreatedAtAction(nameof(GetSmsTemplate), new { templateId = template.Id }, template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SMS template");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific SMS template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>SMS template</returns>
        [HttpGet("templates/{templateId}")]
        public async Task<ActionResult<SmsTemplateDto>> GetSmsTemplate(Guid templateId)
        {
            try
            {
                var templates = await _smsService.GetSmsTemplatesAsync(activeOnly: false);
                var template = templates.FirstOrDefault(t => t.Id == templateId);

                if (template == null)
                {
                    return NotFound(new { message = "SMS template not found" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS template {TemplateId}", templateId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Update SMS template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="updateDto">Update details</param>
        /// <returns>Updated template</returns>
        [HttpPut("templates/{templateId}")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<SmsTemplateDto>> UpdateSmsTemplate(Guid templateId, [FromBody] UpdateSmsTemplateDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var template = await _smsService.UpdateSmsTemplateAsync(templateId, updateDto, userId);
                return Ok(template);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SMS template {TemplateId}", templateId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete SMS template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("templates/{templateId}")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult> DeleteSmsTemplate(Guid templateId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _smsService.DeleteSmsTemplateAsync(templateId, userId);

                if (!success)
                {
                    return BadRequest(new { message = "Failed to delete SMS template" });
                }

                return Ok(new { message = "SMS template deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SMS template {TemplateId}", templateId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Preview SMS template with variables
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="variables">Template variables</param>
        /// <returns>Rendered message</returns>
        [HttpPost("templates/{templateId}/preview")]
        public async Task<ActionResult<object>> PreviewSmsTemplate(Guid templateId, [FromBody] Dictionary<string, object> variables)
        {
            try
            {
                var renderedMessage = await _smsService.PreviewSmsTemplateAsync(templateId, variables);
                return Ok(new { renderedMessage, messageLength = renderedMessage.Length });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing SMS template {TemplateId}", templateId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available SMS providers and their status
        /// </summary>
        /// <returns>SMS providers</returns>
        [HttpGet("providers")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<List<SmsProviderDto>>> GetSmsProviders()
        {
            try
            {
                var providers = await _smsService.GetSmsProvidersAsync();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS providers");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Test SMS provider connectivity
        /// </summary>
        /// <param name="providerName">Provider to test</param>
        /// <returns>Test result</returns>
        [HttpPost("providers/{providerName}/test")]
        [Authorize(Roles = "GM")]
        public async Task<ActionResult<object>> TestSmsProvider(string providerName)
        {
            try
            {
                var isConnected = await _smsService.TestSmsProviderAsync(providerName);
                return Ok(new { provider = providerName, isConnected, testedAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SMS provider {Provider}", providerName);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS configuration
        /// </summary>
        /// <returns>SMS configuration</returns>
        [HttpGet("configuration")]
        [Authorize(Roles = "GM")]
        public async Task<ActionResult<SmsConfigurationDto>> GetSmsConfiguration()
        {
            try
            {
                var config = await _smsService.GetSmsConfigurationAsync();
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS configuration");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Update SMS configuration
        /// </summary>
        /// <param name="configDto">Configuration updates</param>
        /// <returns>Updated configuration</returns>
        [HttpPut("configuration")]
        [Authorize(Roles = "GM")]
        public async Task<ActionResult<SmsConfigurationDto>> UpdateSmsConfiguration([FromBody] SmsConfigurationDto configDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var config = await _smsService.UpdateSmsConfigurationAsync(configDto, userId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SMS configuration");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Send notification SMS for report status changes
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="status">New status</param>
        /// <param name="phoneNumbers">Phone numbers to notify</param>
        /// <returns>Bulk delivery result</returns>
        [HttpPost("notifications/report-status")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<BulkSmsDeliveryDto>> SendReportNotificationSms(
            [FromQuery] Guid reportId,
            [FromQuery] string status,
            [FromBody] List<string> phoneNumbers)
        {
            try
            {
                var result = await _smsService.SendReportNotificationSmsAsync(reportId, status, phoneNumbers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending report notification SMS");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Send deadline reminder SMS
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="daysUntilDeadline">Days until deadline</param>
        /// <param name="phoneNumbers">Phone numbers to notify</param>
        /// <returns>Bulk delivery result</returns>
        [HttpPost("notifications/deadline-reminder")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<BulkSmsDeliveryDto>> SendDeadlineReminderSms(
            [FromQuery] Guid reportId,
            [FromQuery] int daysUntilDeadline,
            [FromBody] List<string> phoneNumbers)
        {
            try
            {
                var result = await _smsService.SendDeadlineReminderSmsAsync(reportId, phoneNumbers, daysUntilDeadline);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending deadline reminder SMS");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Send approval request SMS
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="phoneNumbers">Approver phone numbers</param>
        /// <returns>Bulk delivery result</returns>
        [HttpPost("notifications/approval-request")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<ActionResult<BulkSmsDeliveryDto>> SendApprovalRequestSms(
            [FromQuery] Guid reportId,
            [FromBody] List<string> phoneNumbers)
        {
            try
            {
                var result = await _smsService.SendApprovalRequestSmsAsync(reportId, phoneNumbers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending approval request SMS");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Send emergency alert SMS
        /// </summary>
        /// <param name="message">Alert message</param>
        /// <param name="phoneNumbers">Phone numbers to alert</param>
        /// <returns>Bulk delivery result</returns>
        [HttpPost("notifications/emergency-alert")]
        [Authorize(Roles = "GM")]
        public async Task<ActionResult<BulkSmsDeliveryDto>> SendEmergencyAlertSms(
            [FromQuery] string message,
            [FromBody] List<string> phoneNumbers)
        {
            try
            {
                var result = await _smsService.SendEmergencyAlertSmsAsync(message, phoneNumbers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency alert SMS");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Validate phone number format
        /// </summary>
        /// <param name="phoneNumber">Phone number to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate-phone")]
        public async Task<ActionResult<object>> ValidatePhoneNumber([FromBody] string phoneNumber)
        {
            try
            {
                var isValid = await _smsService.ValidatePhoneNumberAsync(phoneNumber);
                string? formattedNumber = null;

                if (isValid)
                {
                    formattedNumber = await _smsService.FormatPhoneNumberAsync(phoneNumber);
                }

                return Ok(new { phoneNumber, isValid, formattedNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating phone number");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS usage for current user
        /// </summary>
        /// <returns>Usage statistics</returns>
        [HttpGet("usage")]
        public async Task<ActionResult<object>> GetUserSmsUsage()
        {
            try
            {
                var userId = GetCurrentUserId();
                var (dailyCount, monthlyCount, dailyCost, monthlyCost) = await _smsService.GetUserSmsUsageAsync(userId);

                return Ok(new
                {
                    userId,
                    dailyUsage = new { count = dailyCount, cost = dailyCost },
                    monthlyUsage = new { count = monthlyCount, cost = monthlyCost },
                    retrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user SMS usage");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS message history for a specific report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <returns>SMS messages related to the report</returns>
        [HttpGet("reports/{reportId}/history")]
        public async Task<ActionResult<List<SmsMessageDto>>> GetReportSmsHistory(Guid reportId)
        {
            try
            {
                var messages = await _smsService.GetReportSmsHistoryAsync(reportId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS history for report {ReportId}", reportId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS message history for current user
        /// </summary>
        /// <param name="limit">Maximum number of messages to return</param>
        /// <returns>User's SMS message history</returns>
        [HttpGet("users/current/history")]
        public async Task<ActionResult<List<SmsMessageDto>>> GetCurrentUserSmsHistory([FromQuery] int limit = 50)
        {
            try
            {
                var userId = GetCurrentUserId();
                var messages = await _smsService.GetUserSmsHistoryAsync(userId, limit);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS history for current user");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Process SMS delivery webhooks
        /// </summary>
        /// <param name="provider">SMS provider name</param>
        /// <param name="webhookData">Webhook payload</param>
        /// <returns>Processing result</returns>
        [HttpPost("webhooks/{provider}")]
        [AllowAnonymous]
        public async Task<ActionResult> ProcessSmsWebhook(string provider, [FromBody] Dictionary<string, object> webhookData)
        {
            try
            {
                var success = await _smsService.ProcessSmsWebhookAsync(provider, webhookData);

                if (!success)
                {
                    return BadRequest(new { message = "Failed to process webhook" });
                }

                return Ok(new { message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SMS webhook from {Provider}", provider);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.GeneralStaff;
        }

        #endregion
    }
}
