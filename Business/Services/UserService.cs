using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectControlsReportingTool.API.Business.AppSettings;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectControlsReportingTool.API.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly JwtSettings _jwtSettings;

        public UserService(
            IUserRepository userRepository, 
            IAuditLogRepository auditLogRepository,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterUserAsync(RegisterDto dto)
        {
            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(dto.Email))
                {
                    return new AuthResponseDto
                    {
                        Token = string.Empty,
                        User = null!,
                        ExpiresAt = DateTime.UtcNow,
                        ErrorMessage = "Email address is already registered. Please use a different email or try logging in."
                    };
                }

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Role = dto.Role,
                    Department = dto.Department,
                    PhoneNumber = dto.PhoneNumber,
                    JobTitle = dto.JobTitle,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateUserAsync(user, dto.Password);

                // Log the registration
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Created, 
                    createdUser.Id, 
                    null, 
                    $"User registered: {createdUser.Email}"
                );

                // Generate JWT token
                var token = GenerateJwtToken(createdUser);
                var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

                return new AuthResponseDto
                {
                    Token = token,
                    User = MapUserToDto(createdUser),
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = null!,
                    ExpiresAt = DateTime.UtcNow,
                    ErrorMessage = $"Registration failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDto> AuthenticateUserAsync(LoginDto dto)
        {
            try
            {
                var user = await _userRepository.AuthenticateAsync(dto.Email, dto.Password);
                
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Token = string.Empty,
                        User = null!,
                        ExpiresAt = DateTime.UtcNow
                    };
                }

                // Update last login
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Log the login
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated, 
                    user.Id, 
                    null, 
                    "User logged in"
                );

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

                return new AuthResponseDto
                {
                    Token = token,
                    User = MapUserToDto(user),
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception)
            {
                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = null!,
                    ExpiresAt = DateTime.UtcNow
                };
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<AuthResponseDto> UpdateUserAsync(Guid id, UpdateProfileDto dto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Token = string.Empty,
                        User = null!,
                        ExpiresAt = DateTime.UtcNow
                    };
                }

                // Update user properties
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.PhoneNumber = dto.PhoneNumber;
                user.JobTitle = dto.JobTitle;

                await _userRepository.UpdateAsync(user);

                // Log the update
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated, 
                    user.Id, 
                    null, 
                    "User profile updated"
                );

                // Generate new JWT token with updated info
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

                return new AuthResponseDto
                {
                    Token = token,
                    User = MapUserToDto(user),
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception)
            {
                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = null!,
                    ExpiresAt = DateTime.UtcNow
                };
            }
        }

        public async Task<AuthResponseDto> DeactivateUserAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Token = string.Empty,
                        User = null!,
                        ExpiresAt = DateTime.UtcNow
                    };
                }

                user.IsActive = false;
                await _userRepository.UpdateAsync(user);

                // Log the deactivation
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated, 
                    user.Id, 
                    null, 
                    "User deactivated"
                );

                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = MapUserToDto(user),
                    ExpiresAt = DateTime.UtcNow
                };
            }
            catch (Exception)
            {
                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = null!,
                    ExpiresAt = DateTime.UtcNow
                };
            }
        }

        #region Private Helper Methods

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("Department", user.Department.ToString()),
                new("UserId", user.Id.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
                RoleName = user.RoleName,
                Department = user.Department,
                DepartmentName = user.DepartmentName,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                PhoneNumber = user.PhoneNumber,
                JobTitle = user.JobTitle
            };
        }

        #endregion
    }
}
