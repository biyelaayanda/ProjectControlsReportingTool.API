using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;
using System.Text.Json;

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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateReportWithFiles([FromForm] CreateReportDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _reportService.CreateReportAsync(dto, userId);
            
            if (result == null)
                return BadRequest("Failed to create report");
                
            return Ok(result);
        }

        [HttpPost("json")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateReportJson([FromBody] CreateReportDto dto)
        {
            var userId = GetCurrentUserId();
            
            // Ensure no attachments for JSON requests
            dto.Attachments = null;
            
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

        // Statistics and Analytics Endpoints (Phase 7.1)
        [HttpGet("stats")]
        public async Task<IActionResult> GetReportStatistics([FromQuery] StatisticsFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var userDepartment = GetCurrentUserDepartment();

                var statistics = await _reportService.GetReportStatisticsAsync(filter, userId, userRole, userDepartment);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving statistics: {ex.Message}");
            }
        }

        [HttpGet("stats/overview")]
        public async Task<IActionResult> GetOverallStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var stats = await _reportService.GetOverallStatsAsync(startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving overall stats: {ex.Message}");
            }
        }

        [HttpGet("stats/departments")]
        public async Task<IActionResult> GetDepartmentStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                var userDepartment = GetCurrentUserDepartment();
                
                var stats = await _reportService.GetDepartmentStatsAsync(startDate, endDate);
                
                // Filter by user access level
                if (userRole == UserRole.GeneralStaff)
                {
                    stats = stats.Where(s => s.Department == userDepartment);
                }
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving department stats: {ex.Message}");
            }
        }

        [HttpGet("stats/trends")]
        public async Task<IActionResult> GetTrendAnalysis(
            [FromQuery] string period = "monthly", 
            [FromQuery] int periodCount = 12, 
            [FromQuery] Department? department = null)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                var userDepartment = GetCurrentUserDepartment();
                
                // Filter department access for general staff
                if (userRole == UserRole.GeneralStaff)
                {
                    department = userDepartment;
                }
                
                var trends = await _reportService.GetTrendAnalysisAsync(period, periodCount, department);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving trend analysis: {ex.Message}");
            }
        }

        [HttpGet("stats/performance")]
        [Authorize(Roles = "LineManager,GM")]
        public async Task<IActionResult> GetPerformanceMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var metrics = await _reportService.GetPerformanceMetricsAsync(startDate, endDate);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving performance metrics: {ex.Message}");
            }
        }

        [HttpGet("stats/user")]
        public async Task<IActionResult> GetUserStats([FromQuery] Guid? userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();
                
                // General staff can only see their own stats
                if (currentUserRole == UserRole.GeneralStaff && userId.HasValue && userId.Value != currentUserId)
                {
                    return Forbid("You can only view your own statistics");
                }
                
                var targetUserId = userId ?? currentUserId;
                var stats = await _reportService.GetUserStatsAsync(targetUserId, startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving user stats: {ex.Message}");
            }
        }

        [HttpGet("stats/system")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> GetSystemPerformance()
        {
            try
            {
                var performance = await _reportService.GetSystemPerformanceAsync();
                return Ok(performance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving system performance: {ex.Message}");
            }
        }

        [HttpGet("stats/endpoints")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> GetEndpointMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var metrics = await _reportService.GetEndpointMetricsAsync(startDate, endDate);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving endpoint metrics: {ex.Message}");
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
