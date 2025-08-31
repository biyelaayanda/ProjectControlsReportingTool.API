using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Controller for email template management operations
    /// Provides RESTful endpoints for creating, managing, and testing email templates
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailTemplatesController : ControllerBase
    {
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<EmailTemplatesController> _logger;

        public EmailTemplatesController(
            IEmailTemplateService emailTemplateService,
            ILogger<EmailTemplatesController> logger)
        {
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        #region CRUD Operations

        /// <summary>
        /// Get all email templates with optional filtering
        /// </summary>
        /// <param name="searchDto">Optional search criteria</param>
        /// <returns>List of email templates</returns>
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<List<EmailTemplateDto>>> GetAllTemplates([FromQuery] EmailTemplateSearchDto? searchDto = null)
        {
            try
            {
                var templates = await _emailTemplateService.GetAllAsync(searchDto);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email templates");
                return StatusCode(500, "An error occurred while retrieving email templates");
            }
        }

        /// <summary>
        /// Get email template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Email template or 404 if not found</returns>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<EmailTemplateDto>> GetTemplateById(Guid id)
        {
            try
            {
                var template = await _emailTemplateService.GetByIdAsync(id);
                
                if (template == null)
                {
                    return NotFound($"Email template with ID {id} not found");
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email template with ID: {TemplateId}", id);
                return StatusCode(500, "An error occurred while retrieving the email template");
            }
        }

        /// <summary>
        /// Get email template by name
        /// </summary>
        /// <param name="name">Template name</param>
        /// <returns>Email template or 404 if not found</returns>
        [HttpGet("by-name/{name}")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<EmailTemplateDto>> GetTemplateByName(string name)
        {
            try
            {
                var template = await _emailTemplateService.GetByNameAsync(name);
                
                if (template == null)
                {
                    return NotFound($"Email template with name '{name}' not found");
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email template with name: {TemplateName}", name);
                return StatusCode(500, "An error occurred while retrieving the email template");
            }
        }

        /// <summary>
        /// Create a new email template
        /// </summary>
        /// <param name="createDto">Template creation data</param>
        /// <returns>Created email template</returns>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<ActionResult<EmailTemplateDto>> CreateTemplate([FromBody] CreateEmailTemplateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                var template = await _emailTemplateService.CreateAsync(createDto, userId);
                
                return CreatedAtAction(nameof(GetTemplateById), new { id = template.Id }, template);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email template: {TemplateName}", createDto.Name);
                return StatusCode(500, "An error occurred while creating the email template");
            }
        }

        /// <summary>
        /// Update an existing email template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="updateDto">Template update data</param>
        /// <returns>Updated email template or 404 if not found</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<ActionResult<EmailTemplateDto>> UpdateTemplate(Guid id, [FromBody] UpdateEmailTemplateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                var template = await _emailTemplateService.UpdateAsync(id, updateDto, userId);
                
                if (template == null)
                {
                    return NotFound($"Email template with ID {id} not found");
                }

                return Ok(template);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email template with ID: {TemplateId}", id);
                return StatusCode(500, "An error occurred while updating the email template");
            }
        }

        /// <summary>
        /// Delete an email template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>No content if deleted successfully, 404 if not found</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _emailTemplateService.DeleteAsync(id, userId);
                
                if (!result)
                {
                    return NotFound($"Email template with ID {id} not found");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting email template with ID: {TemplateId}", id);
                return StatusCode(500, "An error occurred while deleting the email template");
            }
        }

        /// <summary>
        /// Duplicate an existing email template
        /// </summary>
        /// <param name="id">Source template ID</param>
        /// <param name="duplicateDto">Duplication data</param>
        /// <returns>Duplicated email template or 404 if source not found</returns>
        [HttpPost("{id:guid}/duplicate")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<ActionResult<EmailTemplateDto>> DuplicateTemplate(Guid id, [FromBody] DuplicateEmailTemplateDto duplicateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                var template = await _emailTemplateService.DuplicateAsync(id, duplicateDto, userId);
                
                if (template == null)
                {
                    return NotFound($"Source email template with ID {id} not found");
                }

                return CreatedAtAction(nameof(GetTemplateById), new { id = template.Id }, template);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating email template with ID: {TemplateId}", id);
                return StatusCode(500, "An error occurred while duplicating the email template");
            }
        }

        #endregion

        #region Template Rendering and Preview

        /// <summary>
        /// Render an email template with provided data
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="data">Data to merge with template</param>
        /// <returns>Rendered template or 404 if template not found</returns>
        [HttpPost("{id:guid}/render")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<RenderedEmailTemplateDto>> RenderTemplate(Guid id, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                var rendered = await _emailTemplateService.RenderTemplateAsync(id, data);
                
                if (rendered == null)
                {
                    return NotFound($"Email template with ID {id} not found");
                }

                return Ok(rendered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering email template with ID: {TemplateId}", id);
                return StatusCode(500, "An error occurred while rendering the email template");
            }
        }

        /// <summary>
        /// Preview an email template with sample data
        /// </summary>
        /// <param name="previewDto">Preview data</param>
        /// <returns>Preview result</returns>
        [HttpPost("preview")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<RenderedEmailTemplateDto>> PreviewTemplate([FromBody] EmailTemplatePreviewDto previewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var preview = await _emailTemplateService.PreviewTemplateAsync(previewDto);
                return Ok(preview);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing email template with ID: {TemplateId}", previewDto.TemplateId);
                return StatusCode(500, "An error occurred while previewing the email template");
            }
        }

        /// <summary>
        /// Validate template syntax and variables
        /// </summary>
        /// <param name="validationRequest">Validation request data</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<EmailTemplateValidationDto>> ValidateTemplate([FromBody] dynamic validationRequest)
        {
            try
            {
                string htmlContent = validationRequest?.htmlContent ?? "";
                string subject = validationRequest?.subject ?? "";

                var validation = await _emailTemplateService.ValidateTemplateAsync(htmlContent, subject);
                return Ok(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email template");
                return StatusCode(500, "An error occurred while validating the email template");
            }
        }

        #endregion

        #region Template Management

        /// <summary>
        /// Get templates by type
        /// </summary>
        /// <param name="templateType">Template type</param>
        /// <param name="includeInactive">Include inactive templates</param>
        /// <returns>List of templates of the specified type</returns>
        [HttpGet("by-type/{templateType}")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<List<EmailTemplateDto>>> GetTemplatesByType(string templateType, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var templates = await _emailTemplateService.GetByTypeAsync(templateType, includeInactive);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email templates by type: {TemplateType}", templateType);
                return StatusCode(500, "An error occurred while retrieving email templates by type");
            }
        }

        /// <summary>
        /// Get templates by category
        /// </summary>
        /// <param name="category">Template category</param>
        /// <param name="includeInactive">Include inactive templates</param>
        /// <returns>List of templates in the specified category</returns>
        [HttpGet("by-category/{category}")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<List<EmailTemplateDto>>> GetTemplatesByCategory(string category, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var templates = await _emailTemplateService.GetByCategoryAsync(category, includeInactive);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email templates by category: {Category}", category);
                return StatusCode(500, "An error occurred while retrieving email templates by category");
            }
        }

        /// <summary>
        /// Set template as default for its type
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>No content if set successfully, 404 if not found</returns>
        [HttpPost("{id:guid}/set-default")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<IActionResult> SetAsDefault(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _emailTemplateService.SetAsDefaultAsync(id, userId);
                
                if (!result)
                {
                    return NotFound($"Email template with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting email template as default. ID: {TemplateId}", id);
                return StatusCode(500, "An error occurred while setting the template as default");
            }
        }

        /// <summary>
        /// Toggle template active status
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Updated template or 404 if not found</returns>
        [HttpPost("{id:guid}/toggle-status")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<ActionResult<EmailTemplateDto>> ToggleActiveStatus(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var template = await _emailTemplateService.ToggleActiveStatusAsync(id, userId);
                
                if (template == null)
                {
                    return NotFound($"Email template with ID {id} not found");
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling email template active status. ID: {TemplateId}", id);
                return StatusCode(500, "An error occurred while toggling the template status");
            }
        }

        /// <summary>
        /// Get template usage statistics
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Template statistics or 404 if not found</returns>
        [HttpGet("{id:guid}/stats")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<EmailTemplateStatsDto>> GetTemplateStats(Guid id)
        {
            try
            {
                var stats = await _emailTemplateService.GetTemplateStatsAsync(id);
                
                if (stats == null)
                {
                    return NotFound($"Email template with ID {id} not found");
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email template statistics. ID: {TemplateId}", id);
                return StatusCode(500, "An error occurred while retrieving template statistics");
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Perform bulk operations on multiple templates
        /// </summary>
        /// <param name="operationDto">Bulk operation data</param>
        /// <returns>Number of templates affected</returns>
        [HttpPost("bulk-operation")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<ActionResult<int>> BulkOperation([FromBody] BulkEmailTemplateOperationDto operationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                var affectedCount = await _emailTemplateService.BulkOperationAsync(operationDto, userId);
                
                return Ok(affectedCount);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operation: {Operation}", operationDto.Operation);
                return StatusCode(500, "An error occurred while performing the bulk operation");
            }
        }

        /// <summary>
        /// Export templates
        /// </summary>
        /// <param name="templateIds">Template IDs to export (empty for all)</param>
        /// <returns>Export data</returns>
        [HttpPost("export")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<ActionResult<EmailTemplateExportDto>> ExportTemplates([FromBody] List<Guid>? templateIds = null)
        {
            try
            {
                var export = await _emailTemplateService.ExportTemplatesAsync(templateIds);
                return Ok(export);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting email templates");
                return StatusCode(500, "An error occurred while exporting email templates");
            }
        }

        /// <summary>
        /// Import templates
        /// </summary>
        /// <param name="importDto">Import data</param>
        /// <returns>Import result with success/failure details</returns>
        [HttpPost("import")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<ActionResult<EmailTemplateImportDto>> ImportTemplates([FromBody] EmailTemplateImportDto importDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                var result = await _emailTemplateService.ImportTemplatesAsync(importDto, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing email templates");
                return StatusCode(500, "An error occurred while importing email templates");
            }
        }

        #endregion

        #region System Template Management

        /// <summary>
        /// Create default system templates
        /// </summary>
        /// <returns>Number of templates created</returns>
        [HttpPost("system/create-defaults")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<ActionResult<int>> CreateDefaultSystemTemplates()
        {
            try
            {
                var userId = GetCurrentUserId();
                var createdCount = await _emailTemplateService.CreateDefaultSystemTemplatesAsync(userId);
                
                return Ok(createdCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default system templates");
                return StatusCode(500, "An error occurred while creating default system templates");
            }
        }

        /// <summary>
        /// Get all system templates
        /// </summary>
        /// <returns>List of system templates</returns>
        [HttpGet("system")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        public async Task<ActionResult<List<EmailTemplateDto>>> GetSystemTemplates()
        {
            try
            {
                var templates = await _emailTemplateService.GetSystemTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system templates");
                return StatusCode(500, "An error occurred while retrieving system templates");
            }
        }

        /// <summary>
        /// Reset system template to default
        /// </summary>
        /// <param name="templateName">System template name</param>
        /// <returns>No content if reset successfully, 404 if not found</returns>
        [HttpPost("system/{templateName}/reset")]
        [Authorize(Roles = nameof(UserRole.GM))]
        public async Task<IActionResult> ResetSystemTemplate(string templateName)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _emailTemplateService.ResetSystemTemplateAsync(templateName, userId);
                
                if (!result)
                {
                    return NotFound($"System template with name '{templateName}' not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting system template: {TemplateName}", templateName);
                return StatusCode(500, "An error occurred while resetting the system template");
            }
        }

        #endregion

        #region Private Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            return userId;
        }

        #endregion
    }
}
