using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    [ApiController]
    [Route("api/report-templates")]
    [Authorize]
    public class ReportTemplateController : ControllerBase
    {
        private readonly IReportTemplateService _templateService;

        public ReportTemplateController(IReportTemplateService templateService)
        {
            _templateService = templateService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplates([FromQuery] ReportTemplateFilterDto filter)
        {
            var result = await _templateService.GetTemplatesAsync(filter);
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTemplates()
        {
            var templates = await _templateService.GetActiveTemplatesAsync();
            return Ok(templates);
        }

        [HttpGet("department/{department}")]
        public async Task<IActionResult> GetTemplatesByDepartment(Department department)
        {
            var templates = await _templateService.GetTemplatesByDepartmentAsync(department);
            return Ok(templates);
        }

        [HttpGet("my-templates")]
        public async Task<IActionResult> GetMyTemplates()
        {
            var userId = GetCurrentUserId();
            var templates = await _templateService.GetUserTemplatesAsync(userId);
            return Ok(templates);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplate(Guid id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();
            return Ok(template);
        }

        [HttpPost]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<IActionResult> CreateTemplate(CreateReportTemplateDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _templateService.CreateTemplateAsync(dto, userId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return CreatedAtAction(nameof(GetTemplate), new { id = ((ReportTemplateDto)result.Data!).Id }, result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<IActionResult> UpdateTemplate(Guid id, UpdateReportTemplateDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _templateService.UpdateTemplateAsync(id, dto, userId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _templateService.DeleteTemplateAsync(id, userId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> ActivateTemplate(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _templateService.ActivateTemplateAsync(id, userId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> DeactivateTemplate(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _templateService.DeactivateTemplateAsync(id, userId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("{id}/duplicate")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<IActionResult> DuplicateTemplate(Guid id, [FromBody] string newName)
        {
            var userId = GetCurrentUserId();
            var result = await _templateService.DuplicateTemplateAsync(id, newName, userId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return CreatedAtAction(nameof(GetTemplate), new { id = ((ReportTemplateDto)result.Data!).Id }, result.Data);
        }

        [HttpPost("create-report")]
        public async Task<IActionResult> CreateReportFromTemplate(CreateReportFromTemplateDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _templateService.CreateReportFromTemplateAsync(dto, userId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result.Data);
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> PreviewTemplate(Guid id, [FromQuery] Dictionary<string, string>? variables = null)
        {
            var result = await _templateService.PreviewTemplateAsync(id, variables);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result.Data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTemplates([FromQuery] string searchTerm)
        {
            var templates = await _templateService.SearchTemplatesAsync(searchTerm);
            return Ok(templates);
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetTemplateTypes()
        {
            var types = await _templateService.GetTemplateTypesAsync();
            return Ok(types);
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetTemplateTags()
        {
            var tags = await _templateService.GetTemplateTagsAsync();
            return Ok(tags);
        }

        [HttpGet("validate-name")]
        public async Task<IActionResult> ValidateTemplateName([FromQuery] string name, [FromQuery] Guid? excludeId = null)
        {
            var isValid = await _templateService.ValidateTemplateNameAsync(name, excludeId);
            return Ok(new { isValid, available = isValid });
        }
    }
}
