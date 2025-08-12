using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Interfaces;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Controller for system initialization
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InitController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<InitController> _logger;

        public InitController(
            IAuthService authService, 
            IUserRepository userRepository,
            ILogger<InitController> logger)
        {
            _authService = authService;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Create initial admin user if no users exist
        /// </summary>
        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
        {
            try
            {
                // Check if any users exist
                var existingUsers = await _userRepository.GetAllAsync();
                if (existingUsers.Any())
                {
                    return BadRequest(new { message = "Admin user already exists or users are present in the system" });
                }

                // Create admin user
                var adminDto = new RegisterDto
                {
                    Email = request.Email,
                    Password = request.Password,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = UserRole.Executive,
                    Department = Department.ProjectSupport,
                    JobTitle = "System Administrator"
                };

                var result = await _authService.RegisterAsync(adminDto);

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    return BadRequest(new { message = result.ErrorMessage, details = "Registration failed" });
                }

                _logger.LogInformation("Initial admin user created successfully: {Email}", request.Email);
                
                return Ok(new { 
                    message = "Admin user created successfully",
                    email = request.Email 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user");
                return StatusCode(500, new { 
                    message = "An error occurred while creating admin user", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Check system status
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var userCount = users.Count();

                return Ok(new 
                { 
                    hasUsers = userCount > 0,
                    userCount = userCount,
                    needsInitialization = userCount == 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking system status");
                return StatusCode(500, new { message = "An error occurred while checking system status" });
            }
        }
    }

    public class CreateAdminRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
