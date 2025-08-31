using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Business.Services;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Threading.Tasks;

namespace ProjectControlsReportingTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _userService.RegisterUserAsync(dto);
            if (string.IsNullOrEmpty(result.Token))
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _userService.AuthenticateUserAsync(dto);
            if (string.IsNullOrEmpty(result.Token))
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("search")]
        [Authorize(Roles = "GM,LineManager")]
        public async Task<IActionResult> GetFilteredUsers(UserFilterDto filter)
        {
            var result = await _userService.GetFilteredUsersAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, UpdateProfileDto dto)
        {
            var result = await _userService.UpdateUserAsync(id, dto);
            if (string.IsNullOrEmpty(result.Token))
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}/admin")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> AdminUpdate(Guid id, AdminUserUpdateDto dto)
        {
            var result = await _userService.AdminUpdateUserAsync(id, dto);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (string.IsNullOrEmpty(result.Token))
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> DeletePermanently(Guid id)
        {
            var result = await _userService.DeleteUserPermanentlyAsync(id);
            if (!result)
                return BadRequest("Failed to delete user");
            return Ok("User deleted successfully");
        }

        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody] string newPassword)
        {
            var result = await _userService.ResetUserPasswordAsync(id, newPassword);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
                return BadRequest(result);
            return Ok(result);
        }

        // Bulk Operations
        [HttpPost("bulk/assign-role")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> BulkAssignRole(BulkRoleAssignmentDto dto)
        {
            var result = await _userService.BulkAssignRoleAsync(dto);
            return Ok(result);
        }

        [HttpPost("bulk/change-department")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> BulkChangeDepartment(BulkDepartmentChangeDto dto)
        {
            var result = await _userService.BulkChangeDepartmentAsync(dto);
            return Ok(result);
        }

        [HttpPost("bulk/activate-deactivate")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> BulkActivateDeactivate(BulkActivationDto dto)
        {
            var result = await _userService.BulkActivateDeactivateAsync(dto);
            return Ok(result);
        }

        [HttpPost("bulk/import")]
        [Authorize(Roles = "GM")]
        public async Task<IActionResult> BulkImportUsers(BulkUserImportDto dto)
        {
            var result = await _userService.BulkImportUsersAsync(dto);
            return Ok(result);
        }
    }
}
