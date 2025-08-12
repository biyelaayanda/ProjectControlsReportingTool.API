using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
using ProjectControlsReportingTool.API.Business.AppSettings;
using AutoMapper;

namespace ProjectControlsReportingTool.API.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

                // Find user by email
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", loginDto.Email);
                    return new AuthResponseDto { ErrorMessage = "Invalid email or password" };
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User account is disabled for email {Email}", loginDto.Email);
                    return new AuthResponseDto { ErrorMessage = "Account is disabled" };
                }

                // Verify password
                if (!VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    _logger.LogWarning("Login failed: Invalid password for email {Email}", loginDto.Email);
                    
                    // Log failed login attempt
                    await _auditLogRepository.LogActionAsync(
                        AuditAction.Created, // Using Created as closest match for login attempt
                        user.Id,
                        null,
                        "Failed login attempt",
                        null,
                        null);

                    return new AuthResponseDto { ErrorMessage = "Invalid email or password" };
                }

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

                // Log successful login
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Created, // Using Created as closest match for login
                    user.Id,
                    null,
                    "Successful login",
                    null,
                    null);

                _logger.LogInformation("Login successful for email: {Email}", loginDto.Email);

                return new AuthResponseDto
                {
                    Token = token,
                    User = _mapper.Map<UserDto>(user),
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", loginDto.Email);
                return new AuthResponseDto { ErrorMessage = "An error occurred during login" };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}", registerDto.Email);

                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Email already exists {Email}", registerDto.Email);
                    return new AuthResponseDto { ErrorMessage = "Email already exists" };
                }

                // Create password hash and salt
                CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = registerDto.Email.ToLowerInvariant(),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Role = registerDto.Role,
                    Department = registerDto.Department,
                    PhoneNumber = registerDto.PhoneNumber,
                    JobTitle = registerDto.JobTitle,
                    PasswordHash = Convert.ToBase64String(passwordHash),
                    PasswordSalt = Convert.ToBase64String(passwordSalt),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow
                };

                // Save user
                var createdUser = await _userRepository.AddAsync(user);

                // Log registration
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Created,
                    createdUser.Id,
                    null,
                    "User registered",
                    null,
                    null);

                // Generate JWT token
                var token = GenerateJwtToken(createdUser);
                var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

                _logger.LogInformation("Registration successful for email: {Email}", registerDto.Email);

                return new AuthResponseDto
                {
                    Token = token,
                    User = _mapper.Map<UserDto>(createdUser),
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
                return new AuthResponseDto { ErrorMessage = "An error occurred during registration" };
            }
        }

        public async Task<ServiceResultDto> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResultDto.ErrorResult("User not found");
                }

                // Verify current password
                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                {
                    _logger.LogWarning("Password change failed: Invalid current password for user {UserId}", userId);
                    return ServiceResultDto.ErrorResult("Current password is incorrect");
                }

                // Create new password hash
                CreatePasswordHash(changePasswordDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

                // Update user password
                user.PasswordHash = Convert.ToBase64String(passwordHash);
                user.PasswordSalt = Convert.ToBase64String(passwordSalt);

                await _userRepository.UpdateAsync(user);

                // Log password change
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated,
                    userId,
                    null,
                    "Password changed",
                    null,
                    null);

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return ServiceResultDto.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return ServiceResultDto.ErrorResult("An error occurred while changing password");
            }
        }

        public async Task<ServiceResultDto> UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResultDto.ErrorResult("User not found");
                }

                // Update user profile
                user.FirstName = updateProfileDto.FirstName;
                user.LastName = updateProfileDto.LastName;
                user.PhoneNumber = updateProfileDto.PhoneNumber;
                user.JobTitle = updateProfileDto.JobTitle;

                await _userRepository.UpdateAsync(user);

                // Log profile update
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated,
                    userId,
                    null,
                    "Profile updated",
                    null,
                    null);

                _logger.LogInformation("Profile updated successfully for user {UserId}", userId);
                return ServiceResultDto.SuccessResult(_mapper.Map<UserDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return ServiceResultDto.ErrorResult("An error occurred while updating profile");
            }
        }

        public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user == null ? null : _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user {UserId}", userId);
                return null;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("Department", user.Department.ToString()),
                new("DepartmentId", ((int)user.Department).ToString())
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

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            try
            {
                var passwordHash = Convert.FromBase64String(storedHash);
                var passwordSalt = Convert.FromBase64String(storedSalt);

                using var hmac = new HMACSHA512(passwordSalt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                return computedHash.SequenceEqual(passwordHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
