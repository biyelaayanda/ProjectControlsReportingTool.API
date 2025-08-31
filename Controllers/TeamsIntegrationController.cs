using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Business.Models;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Controller for Microsoft Teams integration management
    /// Provides comprehensive Teams messaging and webhook capabilities
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TeamsIntegrationController : ControllerBase
    {
        private readonly ITeamsIntegrationService _teamsService;
        private readonly ILogger<TeamsIntegrationController> _logger;

        public TeamsIntegrationController(
            ITeamsIntegrationService teamsService,
            ILogger<TeamsIntegrationController> logger)
        {
            _teamsService = teamsService;
            _logger = logger;
        }

        #region Message Management

        /// <summary>
        /// Sends a message to Microsoft Teams through webhook
        /// </summary>
        /// <param name="messageDto">Teams message details</param>
        /// <returns>Teams message response</returns>
        [HttpPost("send-message")]
        [ProducesResponseType(typeof(TeamsMessageResponseDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsMessageResponseDto>> SendMessage([FromBody] SendTeamsMessageDto messageDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.SendMessageAsync(messageDto, userId);

                _logger.LogInformation("User {UserId} sent Teams message to {WebhookUrl} - Success: {Success}", 
                    userId, messageDto.WebhookUrl, result.Success);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid Teams message request from user {UserId}: {Message}", 
                    GetCurrentUserId(), ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Teams message for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to send Teams message"));
            }
        }

        /// <summary>
        /// Sends bulk messages to multiple Teams webhooks
        /// </summary>
        /// <param name="bulkMessageDto">Bulk message details</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("send-bulk-message")]
        [ProducesResponseType(typeof(BulkTeamsMessageResultDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<BulkTeamsMessageResultDto>> SendBulkMessage([FromBody] BulkTeamsMessageDto bulkMessageDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.SendBulkMessageAsync(bulkMessageDto, userId);

                _logger.LogInformation("User {UserId} sent bulk Teams message to {WebhookCount} webhooks - Success Rate: {SuccessRate}%", 
                    userId, bulkMessageDto.WebhookUrls.Count, result.SuccessRate);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid bulk Teams message request from user {UserId}: {Message}", 
                    GetCurrentUserId(), ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk Teams message for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to send bulk Teams message"));
            }
        }

        /// <summary>
        /// Sends notification through Teams integration
        /// </summary>
        /// <param name="notificationDto">Notification details</param>
        /// <returns>Teams message response</returns>
        [HttpPost("send-notification")]
        [ProducesResponseType(typeof(TeamsMessageResponseDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsMessageResponseDto>> SendNotification([FromBody] TeamsNotificationDto notificationDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.SendNotificationAsync(
                    notificationDto.NotificationType,
                    notificationDto.Title,
                    notificationDto.Message,
                    userId,
                    notificationDto.ReportId,
                    notificationDto.Metadata);

                _logger.LogInformation("User {UserId} sent Teams notification {NotificationType} - Success: {Success}", 
                    userId, notificationDto.NotificationType, result.Success);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid Teams notification request from user {UserId}: {Message}", 
                    GetCurrentUserId(), ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Teams notification for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to send Teams notification"));
            }
        }

        /// <summary>
        /// Gets Teams message history
        /// </summary>
        /// <param name="userId">User ID filter (admin only for other users)</param>
        /// <param name="webhookUrl">Webhook URL filter</param>
        /// <param name="notificationType">Notification type filter</param>
        /// <param name="startDate">Start date filter</param>
        /// <param name="endDate">End date filter</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated Teams message history</returns>
        [HttpGet("message-history")]
        [ProducesResponseType(typeof(PagedResultDto<TeamsMessageResponseDto>), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<PagedResultDto<TeamsMessageResponseDto>>> GetMessageHistory(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? webhookUrl = null,
            [FromQuery] NotificationType? notificationType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Only admins can view other users' message history
                Guid? filterUserId = (userId.HasValue && userRole == UserRole.GM) ? userId : currentUserId;

                var result = await _teamsService.GetMessageHistoryAsync(
                    filterUserId, webhookUrl, notificationType, startDate, endDate, pageNumber, pageSize);

                _logger.LogDebug("Retrieved {Count} Teams messages for user {UserId}", 
                    result.Items.Count, filterUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams message history for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retrieve Teams message history"));
            }
        }

        #endregion

        #region Webhook Configuration

        /// <summary>
        /// Creates a new Teams webhook configuration
        /// </summary>
        /// <param name="configDto">Webhook configuration details</param>
        /// <returns>Created webhook configuration</returns>
        [HttpPost("webhook-configs")]
        [ProducesResponseType(typeof(TeamsWebhookConfigDto), 201)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsWebhookConfigDto>> CreateWebhookConfig([FromBody] TeamsWebhookConfigDto configDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.CreateWebhookConfigAsync(configDto, userId);

                _logger.LogInformation("User {UserId} created Teams webhook configuration {ConfigId} for {WebhookUrl}", 
                    userId, result.Name, configDto.WebhookUrl);

                return CreatedAtAction(nameof(GetWebhookConfigById), new { configId = result.Name }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid Teams webhook configuration request from user {UserId}: {Message}", 
                    GetCurrentUserId(), ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Teams webhook configuration for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to create Teams webhook configuration"));
            }
        }

        /// <summary>
        /// Updates an existing Teams webhook configuration
        /// </summary>
        /// <param name="configId">Configuration ID</param>
        /// <param name="configDto">Updated configuration details</param>
        /// <returns>Updated webhook configuration</returns>
        [HttpPut("webhook-configs/{configId:guid}")]
        [ProducesResponseType(typeof(TeamsWebhookConfigDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsWebhookConfigDto>> UpdateWebhookConfig(
            Guid configId, 
            [FromBody] TeamsWebhookConfigDto configDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.UpdateWebhookConfigAsync(configId, configDto, userId);

                _logger.LogInformation("User {UserId} updated Teams webhook configuration {ConfigId}", 
                    userId, configId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Teams webhook configuration {ConfigId} not found or invalid request from user {UserId}: {Message}", 
                    configId, GetCurrentUserId(), ex.Message);
                return NotFound(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Teams webhook configuration {ConfigId} for user {UserId}", 
                    configId, GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to update Teams webhook configuration"));
            }
        }

        /// <summary>
        /// Deletes a Teams webhook configuration
        /// </summary>
        /// <param name="configId">Configuration ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("webhook-configs/{configId:guid}")]
        [ProducesResponseType(typeof(ServiceResultDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<ServiceResultDto>> DeleteWebhookConfig(Guid configId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.DeleteWebhookConfigAsync(configId, userId);

                if (result.Success)
                {
                    _logger.LogInformation("User {UserId} deleted Teams webhook configuration {ConfigId}", 
                        userId, configId);
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Teams webhook configuration {ConfigId} for user {UserId}", 
                    configId, GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to delete Teams webhook configuration"));
            }
        }

        /// <summary>
        /// Gets Teams webhook configurations for the current user
        /// </summary>
        /// <param name="includeAll">Include configurations from all users (Admin only)</param>
        /// <returns>List of webhook configurations</returns>
        [HttpGet("webhook-configs")]
        [ProducesResponseType(typeof(List<TeamsWebhookConfigDto>), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<List<TeamsWebhookConfigDto>>> GetWebhookConfigs([FromQuery] bool includeAll = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Only admins can see all webhook configurations
                Guid? filterUserId = (includeAll && userRole == UserRole.GM) ? null : userId;

                var result = await _teamsService.GetWebhookConfigsAsync(filterUserId);

                _logger.LogDebug("Retrieved {Count} Teams webhook configurations for user {UserId}", 
                    result.Count, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams webhook configurations for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retrieve Teams webhook configurations"));
            }
        }

        /// <summary>
        /// Gets a specific Teams webhook configuration
        /// </summary>
        /// <param name="configId">Configuration ID</param>
        /// <returns>Webhook configuration</returns>
        [HttpGet("webhook-configs/{configId:guid}")]
        [ProducesResponseType(typeof(TeamsWebhookConfigDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsWebhookConfigDto>> GetWebhookConfigById(Guid configId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.GetWebhookConfigByIdAsync(configId, userId);

                if (result == null)
                {
                    return NotFound(ServiceResultDto.ErrorResult("Teams webhook configuration not found"));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams webhook configuration {ConfigId}", configId);
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retrieve Teams webhook configuration"));
            }
        }

        #endregion

        #region Template Management

        /// <summary>
        /// Creates a new Teams notification template
        /// </summary>
        /// <param name="templateDto">Template details</param>
        /// <returns>Created template</returns>
        [HttpPost("templates")]
        [ProducesResponseType(typeof(TeamsNotificationTemplateDto), 201)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsNotificationTemplateDto>> CreateTemplate([FromBody] CreateTeamsTemplateDto templateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.CreateTemplateAsync(templateDto, userId);

                _logger.LogInformation("User {UserId} created Teams template {TemplateId} for {NotificationType}", 
                    userId, result.Id, templateDto.NotificationType);

                return CreatedAtAction(nameof(GetTemplateById), new { templateId = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid Teams template request from user {UserId}: {Message}", 
                    GetCurrentUserId(), ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Teams template for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to create Teams template"));
            }
        }

        /// <summary>
        /// Updates an existing Teams notification template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="templateDto">Updated template details</param>
        /// <returns>Updated template</returns>
        [HttpPut("templates/{templateId:guid}")]
        [ProducesResponseType(typeof(TeamsNotificationTemplateDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsNotificationTemplateDto>> UpdateTemplate(
            Guid templateId, 
            [FromBody] CreateTeamsTemplateDto templateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.UpdateTemplateAsync(templateId, templateDto, userId);

                _logger.LogInformation("User {UserId} updated Teams template {TemplateId}", 
                    userId, templateId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Teams template {TemplateId} not found or invalid request from user {UserId}: {Message}", 
                    templateId, GetCurrentUserId(), ex.Message);
                return NotFound(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Teams template {TemplateId} for user {UserId}", 
                    templateId, GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to update Teams template"));
            }
        }

        /// <summary>
        /// Deletes a Teams notification template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("templates/{templateId:guid}")]
        [ProducesResponseType(typeof(ServiceResultDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<ServiceResultDto>> DeleteTemplate(Guid templateId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.DeleteTemplateAsync(templateId, userId);

                if (result.Success)
                {
                    _logger.LogInformation("User {UserId} deleted Teams template {TemplateId}", 
                        userId, templateId);
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Teams template {TemplateId} for user {UserId}", 
                    templateId, GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to delete Teams template"));
            }
        }

        /// <summary>
        /// Gets Teams notification templates
        /// </summary>
        /// <param name="userId">User ID filter (admin only for other users)</param>
        /// <param name="notificationType">Notification type filter</param>
        /// <param name="isActive">Active status filter</param>
        /// <returns>List of notification templates</returns>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(List<TeamsNotificationTemplateDto>), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<List<TeamsNotificationTemplateDto>>> GetTemplates(
            [FromQuery] Guid? userId = null,
            [FromQuery] NotificationType? notificationType = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Only admins can view other users' templates
                Guid? filterUserId = (userId.HasValue && userRole == UserRole.GM) ? userId : currentUserId;

                var result = await _teamsService.GetTemplatesAsync(filterUserId, notificationType, isActive);

                _logger.LogDebug("Retrieved {Count} Teams templates for user {UserId}", 
                    result.Count, filterUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams templates for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retrieve Teams templates"));
            }
        }

        /// <summary>
        /// Gets a specific Teams notification template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Notification template</returns>
        [HttpGet("templates/{templateId:guid}")]
        [ProducesResponseType(typeof(TeamsNotificationTemplateDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsNotificationTemplateDto>> GetTemplateById(Guid templateId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.GetTemplateByIdAsync(templateId, userId);

                if (result == null)
                {
                    return NotFound(ServiceResultDto.ErrorResult("Teams template not found"));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams template {TemplateId}", templateId);
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retrieve Teams template"));
            }
        }

        /// <summary>
        /// Renders a Teams template with provided data
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="data">Data for template substitution</param>
        /// <returns>Rendered Teams message</returns>
        [HttpPost("templates/{templateId:guid}/render")]
        [ProducesResponseType(typeof(SendTeamsMessageDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 404)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<SendTeamsMessageDto>> RenderTemplate(
            Guid templateId, 
            [FromBody] Dictionary<string, object> data)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.RenderTemplateAsync(templateId, data, userId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Teams template {TemplateId} not found for user {UserId}: {Message}", 
                    templateId, GetCurrentUserId(), ex.Message);
                return NotFound(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering Teams template {TemplateId}", templateId);
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to render Teams template"));
            }
        }

        #endregion

        #region Testing and Validation

        /// <summary>
        /// Tests a Teams webhook endpoint
        /// </summary>
        /// <param name="testDto">Test configuration</param>
        /// <returns>Test result</returns>
        [HttpPost("test-webhook")]
        [ProducesResponseType(typeof(TeamsTestResultDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsTestResultDto>> TestWebhook([FromBody] TeamsIntegrationTestDto testDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.TestWebhookAsync(testDto, userId);

                _logger.LogInformation("User {UserId} tested Teams webhook {WebhookUrl} - Success: {Success}", 
                    userId, testDto.WebhookUrl, result.Success);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid Teams webhook test request from user {UserId}: {Message}", 
                    GetCurrentUserId(), ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Teams webhook for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to test Teams webhook"));
            }
        }

        /// <summary>
        /// Validates a Teams webhook URL
        /// </summary>
        /// <param name="webhookUrl">Webhook URL to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate-webhook")]
        [ProducesResponseType(typeof(ServiceResultDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<ServiceResultDto>> ValidateWebhook([FromBody] ValidateWebhookDto validateDto)
        {
            try
            {
                var result = await _teamsService.ValidateWebhookUrlAsync(validateDto.WebhookUrl);

                _logger.LogDebug("User {UserId} validated Teams webhook {WebhookUrl} - Valid: {Valid}", 
                    GetCurrentUserId(), validateDto.WebhookUrl, result.Success);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Teams webhook URL for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to validate Teams webhook URL"));
            }
        }

        /// <summary>
        /// Tests all configured Teams webhooks for the current user
        /// </summary>
        /// <returns>List of test results</returns>
        [HttpPost("test-all-webhooks")]
        [ProducesResponseType(typeof(List<TeamsTestResultDto>), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<List<TeamsTestResultDto>>> TestAllWebhooks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var results = await _teamsService.TestAllWebhooksAsync(userId);

                _logger.LogInformation("User {UserId} tested {Count} Teams webhooks", 
                    userId, results.Count);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing all Teams webhooks for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to test Teams webhooks"));
            }
        }

        #endregion

        #region Analytics and Statistics

        /// <summary>
        /// Gets Teams integration statistics
        /// </summary>
        /// <param name="userId">User ID filter (admin only for other users)</param>
        /// <param name="webhookUrl">Webhook URL filter</param>
        /// <param name="startDate">Start date filter</param>
        /// <param name="endDate">End date filter</param>
        /// <returns>Teams integration statistics</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(TeamsIntegrationStatsDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<TeamsIntegrationStatsDto>> GetIntegrationStatistics(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? webhookUrl = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Only admins can view other users' statistics
                Guid? filterUserId = (userId.HasValue && userRole == UserRole.GM) ? userId : currentUserId;

                var result = await _teamsService.GetIntegrationStatsAsync(filterUserId, webhookUrl, startDate, endDate);

                _logger.LogDebug("Retrieved Teams integration statistics for user {UserId}", filterUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams integration statistics for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retrieve Teams integration statistics"));
            }
        }

        /// <summary>
        /// Gets Teams delivery failure information
        /// </summary>
        /// <param name="userId">User ID filter (admin only for other users)</param>
        /// <param name="webhookUrl">Webhook URL filter</param>
        /// <param name="isResolved">Resolution status filter</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated delivery failures</returns>
        [HttpGet("delivery-failures")]
        [ProducesResponseType(typeof(PagedResultDto<Data.Entities.TeamsDeliveryFailure>), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<PagedResultDto<Data.Entities.TeamsDeliveryFailure>>> GetDeliveryFailures(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? webhookUrl = null,
            [FromQuery] bool? isResolved = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Only admins can view other users' delivery failures
                Guid? filterUserId = (userId.HasValue && userRole == UserRole.GM) ? userId : currentUserId;

                var result = await _teamsService.GetDeliveryFailuresAsync(
                    filterUserId, webhookUrl, isResolved, pageNumber, pageSize);

                _logger.LogDebug("Retrieved {Count} Teams delivery failures for user {UserId}", 
                    result.Items.Count, filterUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Teams delivery failures for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retrieve Teams delivery failures"));
            }
        }

        /// <summary>
        /// Retries failed Teams message deliveries
        /// </summary>
        /// <param name="retryDto">Retry configuration</param>
        /// <returns>Retry operation result</returns>
        [HttpPost("retry-failed-deliveries")]
        [ProducesResponseType(typeof(ServiceResultDto), 200)]
        [ProducesResponseType(typeof(ServiceResultDto), 400)]
        [ProducesResponseType(typeof(ServiceResultDto), 500)]
        public async Task<ActionResult<ServiceResultDto>> RetryFailedDeliveries([FromBody] RetryFailedDeliveriesDto retryDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamsService.RetryFailedDeliveriesAsync(retryDto.FailureIds, userId);

                _logger.LogInformation("User {UserId} retried {Count} failed Teams deliveries - Result: {Success}", 
                    userId, retryDto.FailureIds.Count, result.Success);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid retry request from user {UserId}: {Message}", 
                    GetCurrentUserId(), ex.Message);
                return BadRequest(ServiceResultDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed Teams deliveries for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ServiceResultDto.ErrorResult("Failed to retry failed deliveries"));
            }
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

        #endregion
    }

    #region Support DTOs

    /// <summary>
    /// DTO for Teams notification sending
    /// </summary>
    public class TeamsNotificationDto
    {
        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public Guid? ReportId { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// DTO for webhook URL validation
    /// </summary>
    public class ValidateWebhookDto
    {
        [Required]
        [Url]
        public string WebhookUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for retrying failed deliveries
    /// </summary>
    public class RetryFailedDeliveriesDto
    {
        [Required]
        public List<Guid> FailureIds { get; set; } = new();
    }

    #endregion
}
