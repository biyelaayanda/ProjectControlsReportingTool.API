using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;
        private readonly ILogger<ExportController> _logger;

        public ExportController(IExportService exportService, ILogger<ExportController> logger)
        {
            _exportService = exportService;
            _logger = logger;
        }

        /// <summary>
        /// Export reports to specified format (PDF, Excel, Word, CSV)
        /// </summary>
        [HttpPost("reports")]
        public async Task<IActionResult> ExportReports([FromBody] ExportRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var userDepartment = GetCurrentUserDepartment();

                request.Type = ExportType.Reports; // Ensure correct type
                var result = await _exportService.ExportReportsAsync(request, userId, userRole, userDepartment);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting reports");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Export statistics to specified format (PDF, Excel)
        /// </summary>
        [HttpPost("statistics")]
        public async Task<IActionResult> ExportStatistics([FromBody] ExportRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var userDepartment = GetCurrentUserDepartment();

                request.Type = ExportType.Statistics; // Ensure correct type
                var result = await _exportService.ExportStatisticsAsync(request, userId, userRole, userDepartment);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Bulk export multiple items
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkExport([FromBody] BulkExportRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var userDepartment = GetCurrentUserDepartment();

                var result = await _exportService.BulkExportAsync(request, userId, userRole, userDepartment);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk export");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Download exported file
        /// </summary>
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                // Validate user is authenticated (userId check)
                _ = GetCurrentUserId(); // Ensure user is authenticated
                
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
                var filePath = Path.Combine(webRootPath, fileName);

                // Add file extension if missing
                if (!Path.HasExtension(filePath))
                {
                    // Try different extensions
                    var extensions = new[] { ".pdf", ".xlsx", ".docx", ".csv" };
                    foreach (var ext in extensions)
                    {
                        var testPath = filePath + ext;
                        if (System.IO.File.Exists(testPath))
                        {
                            filePath = testPath;
                            fileName += ext;
                            break;
                        }
                    }
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Export file not found");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileExtension = Path.GetExtension(filePath).ToLower();
                
                var contentType = fileExtension switch
                {
                    ".pdf" => "application/pdf",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".csv" => "text/csv",
                    _ => "application/octet-stream"
                };

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileName}", fileName);
                return StatusCode(500, "Error downloading file");
            }
        }

        /// <summary>
        /// Get export templates
        /// </summary>
        [HttpGet("templates")]
        public async Task<IActionResult> GetExportTemplates()
        {
            try
            {
                var templates = await _exportService.GetExportTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export templates");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get specific export template
        /// </summary>
        [HttpGet("templates/{templateName}")]
        public async Task<IActionResult> GetExportTemplate(string templateName)
        {
            try
            {
                var template = await _exportService.GetExportTemplateAsync(templateName);
                if (template == null)
                {
                    return NotFound("Template not found");
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export template {TemplateName}", templateName);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create new export template
        /// </summary>
        [HttpPost("templates")]
        [Authorize(Roles = "LineManager,GM")]
        public async Task<IActionResult> CreateExportTemplate([FromBody] ExportTemplateDto template)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _exportService.CreateExportTemplateAsync(template, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating export template");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete export template
        /// </summary>
        [HttpDelete("templates/{templateName}")]
        [Authorize(Roles = "LineManager,GM")]
        public async Task<IActionResult> DeleteExportTemplate(string templateName)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _exportService.DeleteExportTemplateAsync(templateName, userId);
                
                if (result)
                {
                    return Ok(new { message = "Template deleted successfully" });
                }

                return BadRequest("Failed to delete template");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting export template {TemplateName}", templateName);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get export history for current user
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetExportHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetCurrentUserId();
                var history = await _exportService.GetExportHistoryAsync(userId, startDate, endDate);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export history");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get specific export result
        /// </summary>
        [HttpGet("result/{exportId}")]
        public async Task<IActionResult> GetExportResult(Guid exportId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _exportService.GetExportResultAsync(exportId, userId);
                
                if (result == null)
                {
                    return NotFound("Export result not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export result {ExportId}", exportId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete export file
        /// </summary>
        [HttpDelete("{exportId}")]
        public async Task<IActionResult> DeleteExport(Guid exportId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _exportService.DeleteExportAsync(exportId, userId);
                
                if (result)
                {
                    return Ok(new { message = "Export deleted successfully" });
                }

                return BadRequest("Failed to delete export");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting export {ExportId}", exportId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get supported export formats
        /// </summary>
        [HttpGet("formats")]
        public IActionResult GetSupportedFormats()
        {
            var formats = new[]
            {
                new { Value = (int)ExportFormat.PDF, Name = "PDF", Description = "Portable Document Format", Extensions = new[] { ".pdf" } },
                new { Value = (int)ExportFormat.Excel, Name = "Excel", Description = "Microsoft Excel Spreadsheet", Extensions = new[] { ".xlsx" } },
                new { Value = (int)ExportFormat.Word, Name = "Word", Description = "Microsoft Word Document", Extensions = new[] { ".docx" } },
                new { Value = (int)ExportFormat.CSV, Name = "CSV", Description = "Comma-Separated Values", Extensions = new[] { ".csv" } }
            };

            return Ok(formats);
        }

        /// <summary>
        /// Get supported export types
        /// </summary>
        [HttpGet("types")]
        public IActionResult GetSupportedTypes()
        {
            var types = new[]
            {
                new { Value = (int)ExportType.Reports, Name = "Reports", Description = "Export report data" },
                new { Value = (int)ExportType.Statistics, Name = "Statistics", Description = "Export statistical data" },
                new { Value = (int)ExportType.Users, Name = "Users", Description = "Export user data" },
                new { Value = (int)ExportType.AuditLogs, Name = "Audit Logs", Description = "Export audit trail data" }
            };

            return Ok(types);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found"));
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.Parse<UserRole>(roleClaim ?? throw new UnauthorizedAccessException("User role not found"));
        }

        private Department GetCurrentUserDepartment()
        {
            var deptClaim = User.FindFirst("Department")?.Value;
            return Enum.Parse<Department>(deptClaim ?? throw new UnauthorizedAccessException("User department not found"));
        }
    }
}
