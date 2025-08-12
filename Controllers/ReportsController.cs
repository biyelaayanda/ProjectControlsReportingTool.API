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
        public async Task<IActionResult> CreateReport(CreateReportDto dto)
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
            return Ok(reports);
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
            var deptClaim = User.FindFirst("department")?.Value;
            return Enum.Parse<Department>(deptClaim ?? throw new UnauthorizedAccessException("User department not found"));
        }
    }
}
