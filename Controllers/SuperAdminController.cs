using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Services;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService _superAdminService;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(
            ISuperAdminService superAdminService,
            ILogger<SuperAdminController> logger)
        {
            _superAdminService = superAdminService;
            _logger = logger;
        }

        // Helper method to get current user ID
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // Helper method to get current user role
        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.GeneralStaff;
        }

        // Helper method to check SuperAdmin authorization
        private bool IsSuperAdmin()
        {
            return GetCurrentUserRole() == UserRole.SuperAdmin;
        }

        // User Management Endpoints

        /// <summary>
        /// Get all users with filtering and pagination
        /// </summary>
        [HttpPost("users/search")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(PagedResultDto<SuperAdminUserDetailDto>), 200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PagedResultDto<SuperAdminUserDetailDto>>> GetUsers(
            [FromBody] SuperAdminUserFiltersDto filters)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can access user management.");
                }

                var result = await _superAdminService.GetUsersAsync(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users with filters: {@Filters}", filters);
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }

        /// <summary>
        /// Get user by ID with detailed information
        /// </summary>
        [HttpGet("users/{userId:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminUserDetailDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminUserDetailDto>> GetUser(Guid userId)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can access user details.");
                }

                var user = await _superAdminService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost("users")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminUserDetailDto), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminUserDetailDto>> CreateUser(
            [FromBody] SuperAdminCreateUserDto createUserDto)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can create users.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var result = await _superAdminService.CreateUserAsync(createUserDto, currentUserId);

                return CreatedAtAction(
                    nameof(GetUser),
                    new { userId = result.Id },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User creation failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {@CreateUserDto}", createUserDto);
                return StatusCode(500, "An error occurred while creating the user.");
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("users/{userId:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminUserDetailDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminUserDetailDto>> UpdateUser(
            Guid userId,
            [FromBody] SuperAdminUpdateUserDto updateUserDto)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can update users.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var result = await _superAdminService.UpdateUserAsync(userId, updateUserDto, currentUserId);

                if (result == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User update failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}: {@UpdateUserDto}", userId, updateUserDto);
                return StatusCode(500, "An error occurred while updating the user.");
            }
        }

        /// <summary>
        /// Delete a user (soft delete by default)
        /// </summary>
        [HttpDelete("users/{userId:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteUser(
            Guid userId,
            [FromQuery] string reason,
            [FromQuery] bool permanent = false)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can delete users.");
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest("Deletion reason is required.");
                }

                var currentUserId = GetCurrentUserId();
                var success = await _superAdminService.DeleteUserAsync(userId, reason, permanent, currentUserId);

                if (!success)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User deletion failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}, Permanent: {Permanent}, Reason: {Reason}", 
                    userId, permanent, reason);
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        [HttpPost("users/{userId:guid}/reset-password")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminPasswordResetResultDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminPasswordResetResultDto>> ResetUserPassword(
            Guid userId,
            [FromQuery] bool sendEmail = true)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can reset user passwords.");
                }

                var currentUserId = GetCurrentUserId();
                var result = await _superAdminService.ResetUserPasswordAsync(userId, sendEmail, currentUserId);

                if (result == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                return StatusCode(500, "An error occurred while resetting the user's password.");
            }
        }

        /// <summary>
        /// Toggle user active status
        /// </summary>
        [HttpPatch("users/{userId:guid}/toggle-status")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminUserDetailDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminUserDetailDto>> ToggleUserStatus(
            Guid userId,
            [FromQuery] string reason)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can toggle user status.");
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest("Status change reason is required.");
                }

                var currentUserId = GetCurrentUserId();
                var result = await _superAdminService.ToggleUserStatusAsync(userId, reason, currentUserId);

                if (result == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User status toggle failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {UserId}, Reason: {Reason}", userId, reason);
                return StatusCode(500, "An error occurred while toggling the user's status.");
            }
        }

        // Bulk Operations

        /// <summary>
        /// Create multiple users in bulk
        /// </summary>
        [HttpPost("users/bulk-create")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminBulkOperationResultDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminBulkOperationResultDto>> BulkCreateUsers(
            [FromBody] SuperAdminBulkCreateUsersDto bulkCreateDto)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can bulk create users.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var result = await _superAdminService.BulkCreateUsersAsync(bulkCreateDto, currentUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk user creation: {@BulkCreateDto}", bulkCreateDto);
                return StatusCode(500, "An error occurred during bulk user creation.");
            }
        }

        /// <summary>
        /// Update multiple users in bulk
        /// </summary>
        [HttpPut("users/bulk-update")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminBulkOperationResultDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminBulkOperationResultDto>> BulkUpdateUsers(
            [FromBody] SuperAdminBulkUpdateUsersDto bulkUpdateDto)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can bulk update users.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var result = await _superAdminService.BulkUpdateUsersAsync(bulkUpdateDto, currentUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk user update: {@BulkUpdateDto}", bulkUpdateDto);
                return StatusCode(500, "An error occurred during bulk user update.");
            }
        }

        /// <summary>
        /// Delete multiple users in bulk
        /// </summary>
        [HttpDelete("users/bulk-delete")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminBulkOperationResultDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminBulkOperationResultDto>> BulkDeleteUsers(
            [FromBody] SuperAdminBulkDeleteUsersDto bulkDeleteDto)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can bulk delete users.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var result = await _superAdminService.BulkDeleteUsersAsync(bulkDeleteDto, currentUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk user deletion: {@BulkDeleteDto}", bulkDeleteDto);
                return StatusCode(500, "An error occurred during bulk user deletion.");
            }
        }

        // Import/Export Operations

        /// <summary>
        /// Import users from CSV file
        /// </summary>
        [HttpPost("users/import")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminImportResultDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminImportResultDto>> ImportUsers(
            IFormFile file,
            [FromQuery] bool preview = false,
            [FromQuery] bool sendWelcomeEmails = false)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can import users.");
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("Please provide a valid CSV file.");
                }

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only CSV files are supported.");
                }

                var currentUserId = GetCurrentUserId();
                var result = await _superAdminService.ImportUsersAsync(file, preview, sendWelcomeEmails, currentUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing users from file: {FileName}", file?.FileName);
                return StatusCode(500, "An error occurred during user import.");
            }
        }

        /// <summary>
        /// Export users to CSV file
        /// </summary>
        [HttpPost("users/export")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ExportUsers(
            [FromBody] SuperAdminUserFiltersDto filters)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can export user data.");
                }

                var csvData = await _superAdminService.ExportUsersAsync(filters);
                var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting users with filters: {@Filters}", filters);
                return StatusCode(500, "An error occurred during user export.");
            }
        }

        // System Administration

        /// <summary>
        /// Get system health report
        /// </summary>
        [HttpGet("system/health")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminSystemHealthReportDto), 200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SuperAdminSystemHealthReportDto>> GetSystemHealth()
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can access system health reports.");
                }

                var healthReport = await _superAdminService.GetSystemHealthAsync();
                return Ok(healthReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system health report");
                return StatusCode(500, "An error occurred while retrieving system health.");
            }
        }

        /// <summary>
        /// Get audit trail with filtering
        /// </summary>
        [HttpPost("audit/trail")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(PagedResultDto<SuperAdminAuditEntryDto>), 200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PagedResultDto<SuperAdminAuditEntryDto>>> GetAuditTrail(
            [FromBody] SuperAdminAuditTrailFiltersDto filters)
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can access audit trails.");
                }

                var auditTrail = await _superAdminService.GetAuditTrailAsync(filters);
                return Ok(auditTrail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit trail with filters: {@Filters}", filters);
                return StatusCode(500, "An error occurred while retrieving audit trail.");
            }
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard/stats")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            try
            {
                if (!IsSuperAdmin())
                {
                    return Forbid("Only Super Admins can access dashboard statistics.");
                }

                var stats = await _superAdminService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, "An error occurred while retrieving dashboard statistics.");
            }
        }
    }
}
