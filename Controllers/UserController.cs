using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateProfileDto dto)
        {
            var result = await _userService.UpdateUserAsync(id, dto);
            if (string.IsNullOrEmpty(result.Token))
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (string.IsNullOrEmpty(result.Token))
                return BadRequest(result);
            return Ok(result);
        }
    }
}
