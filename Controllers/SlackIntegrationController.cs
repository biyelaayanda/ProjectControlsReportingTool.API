using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Business.Models;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Controller for managing Slack integration operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SlackIntegrationController : ControllerBase
    {
        private readonly ISlackIntegrationService _slackIntegrationService;
        private readonly ILogger<SlackIntegrationController> _logger;

        public SlackIntegrationController(
            ISlackIntegrationService slackIntegrationService,
            ILogger<SlackIntegrationController> logger)
        {
            _slackIntegrationService = slackIntegrationService;
            _logger = logger;
        }

        #region Message Operations

        /// <summary>
        /// Send a message to Slack using webhook
        /// </summary>
        [HttpPost("messages/send")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> SendSlackMessage([FromBody] SendSlackMessageDto request)
        {
            try
            {
                var result = await _slackIntegrationService.SendSlackMessageAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendSlackMessage endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Send bulk messages to multiple Slack channels
        /// </summary>
        [HttpPost("messages/bulk")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SendBulkSlackMessages([FromBody] BulkSlackMessageDto request)
        {
            try
            {
                var result = await _slackIntegrationService.SendBulkSlackMessagesAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendBulkSlackMessages endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Send notification message based on templates
        /// </summary>
        [HttpPost("notifications/send")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> SendSlackNotification([FromBody] SlackNotificationRequestDto request)
        {
            try
            {
                var result = await _slackIntegrationService.SendSlackNotificationAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendSlackNotification endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Update an existing Slack message
        /// </summary>
        [HttpPut("messages/{messageId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateSlackMessage(string messageId, [FromBody] UpdateSlackMessageDto request)
        {
            try
            {
                request.MessageId = messageId;
                var result = await _slackIntegrationService.UpdateSlackMessageAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateSlackMessage endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Delete a Slack message
        /// </summary>
        [HttpDelete("messages/{messageId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteSlackMessage(string messageId, [FromQuery] string channel, [FromQuery] string webhookId)
        {
            try
            {
                var result = await _slackIntegrationService.DeleteSlackMessageAsync(messageId, channel, webhookId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteSlackMessage endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        #endregion

        #region Webhook Management

        /// <summary>
        /// Create a new Slack webhook configuration
        /// </summary>
        [HttpPost("webhooks")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSlackWebhook([FromBody] CreateSlackWebhookDto request)
        {
            try
            {
                var result = await _slackIntegrationService.CreateSlackWebhookAsync(request);
                
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetSlackWebhookById), new { id = result.Data.Id }, result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateSlackWebhook endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Update an existing Slack webhook configuration
        /// </summary>
        [HttpPut("webhooks/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSlackWebhook(int id, [FromBody] UpdateSlackWebhookDto request)
        {
            try
            {
                var result = await _slackIntegrationService.UpdateSlackWebhookAsync(id, request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateSlackWebhook endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Delete a Slack webhook configuration
        /// </summary>
        [HttpDelete("webhooks/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSlackWebhook(int id)
        {
            try
            {
                var result = await _slackIntegrationService.DeleteSlackWebhookAsync(id);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteSlackWebhook endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Get all Slack webhook configurations
        /// </summary>
        [HttpGet("webhooks")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSlackWebhooks()
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackWebhooksAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackWebhooks endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Get Slack webhook configuration by ID
        /// </summary>
        [HttpGet("webhooks/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSlackWebhookById(int id)
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackWebhookByIdAsync(id);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackWebhookById endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Test a Slack webhook connection
        /// </summary>
        [HttpPost("webhooks/{id}/test")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> TestSlackWebhook(int id)
        {
            try
            {
                var result = await _slackIntegrationService.TestSlackWebhookAsync(id);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestSlackWebhook endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Test a Slack webhook URL directly
        /// </summary>
        [HttpPost("webhooks/test-url")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestSlackWebhookUrl([FromBody] TestSlackWebhookUrlDto request)
        {
            try
            {
                var result = await _slackIntegrationService.TestSlackWebhookUrlAsync(request.WebhookUrl);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestSlackWebhookUrl endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        #endregion

        #region Template Management

        /// <summary>
        /// Create a new Slack notification template
        /// </summary>
        [HttpPost("templates")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateSlackTemplate([FromBody] CreateSlackTemplateDto request)
        {
            try
            {
                var result = await _slackIntegrationService.CreateSlackTemplateAsync(request);
                
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetSlackTemplateById), new { id = result.Data.Id }, result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateSlackTemplate endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Update an existing Slack notification template
        /// </summary>
        [HttpPut("templates/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateSlackTemplate(int id, [FromBody] UpdateSlackTemplateDto request)
        {
            try
            {
                var result = await _slackIntegrationService.UpdateSlackTemplateAsync(id, request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateSlackTemplate endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Delete a Slack notification template
        /// </summary>
        [HttpDelete("templates/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSlackTemplate(int id)
        {
            try
            {
                var result = await _slackIntegrationService.DeleteSlackTemplateAsync(id);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteSlackTemplate endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Get all Slack notification templates
        /// </summary>
        [HttpGet("templates")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetSlackTemplates()
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackTemplatesAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackTemplates endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Get Slack notification template by ID
        /// </summary>
        [HttpGet("templates/{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetSlackTemplateById(int id)
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackTemplateByIdAsync(id);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackTemplateById endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Get Slack notification templates by type
        /// </summary>
        [HttpGet("templates/type/{templateType}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetSlackTemplatesByType(string templateType)
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackTemplatesByTypeAsync(templateType);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackTemplatesByType endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        #endregion

        #region Analytics and Statistics

        /// <summary>
        /// Get Slack integration statistics
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSlackStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackStatisticsAsync(startDate, endDate);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackStatistics endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Get delivery failure logs
        /// </summary>
        [HttpGet("delivery-failures")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSlackDeliveryFailures([FromQuery] int? days = 30)
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackDeliveryFailuresAsync(days);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackDeliveryFailures endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Get message delivery statistics by channel
        /// </summary>
        [HttpGet("channel-statistics")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSlackChannelStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackChannelStatisticsAsync(startDate, endDate);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackChannelStatistics endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Retry failed Slack message deliveries
        /// </summary>
        [HttpPost("retry-failures")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RetryFailedSlackDeliveries([FromBody] RetrySlackDeliveriesDto request)
        {
            try
            {
                var result = await _slackIntegrationService.RetryFailedSlackDeliveriesAsync(request?.FailureIds);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RetryFailedSlackDeliveries endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        #endregion

        #region Channel Management

        /// <summary>
        /// Get available Slack channels for a webhook
        /// </summary>
        [HttpGet("webhooks/{webhookId}/channels")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetSlackChannels(int webhookId)
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackChannelsAsync(webhookId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackChannels endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Validate if a channel exists and is accessible
        /// </summary>
        [HttpPost("channels/validate")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> ValidateSlackChannel([FromBody] ValidateSlackChannelDto request)
        {
            try
            {
                var result = await _slackIntegrationService.ValidateSlackChannelAsync(request.Channel, request.WebhookId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ValidateSlackChannel endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        #endregion

        #region Configuration and Health

        /// <summary>
        /// Get Slack integration configuration
        /// </summary>
        [HttpGet("configuration")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSlackConfiguration()
        {
            try
            {
                var result = await _slackIntegrationService.GetSlackConfigurationAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlackConfiguration endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Update Slack integration configuration
        /// </summary>
        [HttpPut("configuration")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSlackConfiguration([FromBody] UpdateSlackConfigurationDto request)
        {
            try
            {
                var result = await _slackIntegrationService.UpdateSlackConfigurationAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateSlackConfiguration endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        /// <summary>
        /// Check health of all Slack webhooks
        /// </summary>
        [HttpGet("health")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CheckSlackWebhookHealth()
        {
            try
            {
                var result = await _slackIntegrationService.CheckSlackWebhookHealthAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckSlackWebhookHealth endpoint");
                return StatusCode(500, new { Error = "An internal error occurred" });
            }
        }

        #endregion
    }
}
