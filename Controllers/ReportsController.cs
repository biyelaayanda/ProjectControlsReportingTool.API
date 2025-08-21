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
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromForm] CreateReportDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _reportService.CreateReportAsync(dto, userId);
            
            if (result == null)
                return BadRequest("Failed to create report");
                
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetReports([FromQuery] ReportFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var userDepartment = GetCurrentUserDepartment();
            
            var reports = await _reportService.GetReportsAsync(filter, userId, userRole, userDepartment);
            
            // Wrap response to match frontend expectations
            var response = new { reports = reports };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(Guid id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var report = await _reportService.GetReportByIdAsync(id, userId, userRole);
            
            if (report == null)
                return NotFound();
                
            return Ok(report);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateReportStatus(Guid id, UpdateReportStatusDto dto)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var result = await _reportService.UpdateReportStatusAsync(id, dto, userId, userRole);
            
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
                
            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveReport(Guid id, [FromBody] ApprovalDto dto)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var result = await _reportService.ApproveReportAsync(id, dto, userId, userRole);
            
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
                
            return Ok(result);
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitReport(Guid id, [FromBody] SubmitReportDto dto)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var result = await _reportService.SubmitReportAsync(id, dto, userId, userRole);
            
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
                
            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectReport(Guid id, [FromBody] RejectionDto dto)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var result = await _reportService.RejectReportAsync(id, dto, userId, userRole);
            
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
                
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var result = await _reportService.DeleteReportAsync(id, userId, userRole);
            
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
                
            return Ok(result);
        }

        [HttpGet("pending-approvals")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var userDepartment = GetCurrentUserDepartment();
            
            var reports = await _reportService.GetPendingApprovalsAsync(userId, userRole, userDepartment);
            return Ok(reports);
        }

        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var userId = GetCurrentUserId();
            var reports = await _reportService.GetUserReportsAsync(userId);
            return Ok(reports);
        }

        [HttpGet("{reportId}/attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(Guid reportId, Guid attachmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var attachment = await _reportService.GetAttachmentAsync(reportId, attachmentId, userId);
                
                if (attachment == null)
                    return NotFound("Attachment not found");

                if (!System.IO.File.Exists(attachment.FilePath))
                    return NotFound("File not found on disk");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath);
                var contentType = attachment.ContentType ?? "application/octet-stream";
                
                return File(fileBytes, contentType, attachment.OriginalFileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You don't have permission to download this attachment");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error downloading file: {ex.Message}");
            }
        }

        [HttpGet("{reportId}/attachments/{attachmentId}/preview")]
        public async Task<IActionResult> PreviewAttachment(Guid reportId, Guid attachmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var attachment = await _reportService.GetAttachmentAsync(reportId, attachmentId, userId);
                
                if (attachment == null)
                    return NotFound("Attachment not found");

                if (!System.IO.File.Exists(attachment.FilePath))
                    return NotFound("File not found on disk");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath);
                var contentType = attachment.ContentType ?? "application/octet-stream";
                
                // Set headers for inline display (preview)
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{attachment.OriginalFileName}\"";
                
                return File(fileBytes, contentType);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You don't have permission to preview this attachment");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error previewing file: {ex.Message}");
            }
        }

        [HttpPost("{id}/approval-documents")]
        public async Task<IActionResult> UploadApprovalDocuments(Guid id, [FromForm] IFormFileCollection files, [FromForm] string? description = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files provided");
                }

                var result = await _reportService.UploadApprovalDocumentsAsync(id, files, userId, userRole, description);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading approval documents: {ex.Message}");
            }
        }

        [HttpGet("{id}/attachments")]
        public async Task<IActionResult> GetReportAttachments(Guid id, [FromQuery] ApprovalStage? stage = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var attachments = await _reportService.GetReportAttachmentsByStageAsync(id, stage, userId, userRole);

                // Group attachments by approval stage for better frontend handling
                var groupedAttachments = attachments
                    .GroupBy(a => a.ApprovalStage)
                    .ToDictionary(
                        g => g.Key.ToString(),
                        g => g.OrderBy(a => a.UploadedDate).ToList()
                    );

                return Ok(new 
                { 
                    reportId = id,
                    attachmentsByStage = groupedAttachments,
                    totalAttachments = attachments.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving attachments: {ex.Message}");
            }
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
