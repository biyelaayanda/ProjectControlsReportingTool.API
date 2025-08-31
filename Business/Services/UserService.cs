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

        public async Task<BulkOperationResultDto> BulkAssignRoleAsync(BulkRoleAssignmentDto dto)
        {
            var result = new BulkOperationResultDto
            {
                TotalRequested = dto.UserIds.Count
            };

            foreach (var userId in dto.UserIds)
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user == null)
                    {
                        result.FailedOperations++;
                        result.ErrorMessages.Add($"User with ID {userId} not found");
                        continue;
                    }

                    user.Role = dto.Role;
                    await _userRepository.UpdateAsync(user);

                    await _auditLogRepository.LogActionAsync(
                        AuditAction.Updated,
                        user.Id,
                        null,
                        $"Role changed to {dto.Role}"
                    );

                    result.SuccessfulOperations++;
                    result.ProcessedUsers.Add(MapUserToDto(user));
                }
                catch (Exception ex)
                {
                    result.FailedOperations++;
                    result.ErrorMessages.Add($"Failed to update user {userId}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<BulkOperationResultDto> BulkChangeDepartmentAsync(BulkDepartmentChangeDto dto)
        {
            var result = new BulkOperationResultDto
            {
                TotalRequested = dto.UserIds.Count
            };

            foreach (var userId in dto.UserIds)
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user == null)
                    {
                        result.FailedOperations++;
                        result.ErrorMessages.Add($"User with ID {userId} not found");
                        continue;
                    }

                    user.Department = dto.Department;
                    await _userRepository.UpdateAsync(user);

                    await _auditLogRepository.LogActionAsync(
                        AuditAction.Updated,
                        user.Id,
                        null,
                        $"Department changed to {dto.Department}"
                    );

                    result.SuccessfulOperations++;
                    result.ProcessedUsers.Add(MapUserToDto(user));
                }
                catch (Exception ex)
                {
                    result.FailedOperations++;
                    result.ErrorMessages.Add($"Failed to update user {userId}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<BulkOperationResultDto> BulkActivateDeactivateAsync(BulkActivationDto dto)
        {
            var result = new BulkOperationResultDto
            {
                TotalRequested = dto.UserIds.Count
            };

            foreach (var userId in dto.UserIds)
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user == null)
                    {
                        result.FailedOperations++;
                        result.ErrorMessages.Add($"User with ID {userId} not found");
                        continue;
                    }

                    user.IsActive = dto.IsActive;
                    await _userRepository.UpdateAsync(user);

                    await _auditLogRepository.LogActionAsync(
                        AuditAction.Updated,
                        user.Id,
                        null,
                        $"User {(dto.IsActive ? "activated" : "deactivated")}"
                    );

                    result.SuccessfulOperations++;
                    result.ProcessedUsers.Add(MapUserToDto(user));
                }
                catch (Exception ex)
                {
                    result.FailedOperations++;
                    result.ErrorMessages.Add($"Failed to update user {userId}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<BulkOperationResultDto> BulkImportUsersAsync(BulkUserImportDto dto)
        {
            var result = new BulkOperationResultDto
            {
                TotalRequested = dto.Users.Count
            };

            foreach (var userImport in dto.Users)
            {
                try
                {
                    // Check if email already exists
                    if (await _userRepository.EmailExistsAsync(userImport.Email))
                    {
                        result.FailedOperations++;
                        result.ErrorMessages.Add($"Email {userImport.Email} already exists");
                        continue;
                    }

                    // Generate password if not provided
                    var password = string.IsNullOrEmpty(userImport.Password) 
                        ? GenerateTemporaryPassword() 
                        : userImport.Password;

                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = userImport.Email,
                        FirstName = userImport.FirstName,
                        LastName = userImport.LastName,
                        Role = userImport.Role,
                        Department = userImport.Department,
                        PhoneNumber = userImport.PhoneNumber,
                        JobTitle = userImport.JobTitle,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    var createdUser = await _userRepository.CreateUserAsync(user, password);

                    await _auditLogRepository.LogActionAsync(
                        AuditAction.Created,
                        createdUser.Id,
                        null,
                        "User imported via bulk operation"
                    );

                    result.SuccessfulOperations++;
                    result.ProcessedUsers.Add(MapUserToDto(createdUser));
                }
                catch (Exception ex)
                {
                    result.FailedOperations++;
                    result.ErrorMessages.Add($"Failed to import user {userImport.Email}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<PagedResultDto<UserDto>> GetFilteredUsersAsync(UserFilterDto filter)
        {
            var users = await _userRepository.GetAllAsync();
            var query = users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(u => 
                    u.FirstName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (u.JobTitle != null && u.JobTitle.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (filter.Role.HasValue)
            {
                query = query.Where(u => u.Role == filter.Role.Value);
            }

            if (filter.Department.HasValue)
            {
                query = query.Where(u => u.Department == filter.Department.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(u => u.CreatedDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(u => u.CreatedDate <= filter.ToDate.Value);
            }

            var totalCount = query.Count();
            var pagedUsers = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(MapUserToDto)
                .ToList();

            return new PagedResultDto<UserDto>
            {
                Items = pagedUsers,
                TotalCount = totalCount,
                Page = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasNext = filter.PageNumber < (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasPrevious = filter.PageNumber > 1
            };
        }

        public async Task<AuthResponseDto> AdminUpdateUserAsync(Guid id, AdminUserUpdateDto dto)
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
                        ExpiresAt = DateTime.UtcNow,
                        ErrorMessage = "User not found"
                    };
                }

                // Update user properties (admin can change role and department)
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Role = dto.Role;
                user.Department = dto.Department;
                user.PhoneNumber = dto.PhoneNumber;
                user.JobTitle = dto.JobTitle;
                user.IsActive = dto.IsActive;

                await _userRepository.UpdateAsync(user);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated,
                    user.Id,
                    null,
                    "User updated by administrator"
                );

                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = MapUserToDto(user),
                    ExpiresAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = null!,
                    ExpiresAt = DateTime.UtcNow,
                    ErrorMessage = $"Update failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDto> ResetUserPasswordAsync(Guid id, string newPassword)
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
                        ExpiresAt = DateTime.UtcNow,
                        ErrorMessage = "User not found"
                    };
                }

                await _userRepository.UpdatePasswordAsync(user, newPassword);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated,
                    user.Id,
                    null,
                    "Password reset by administrator"
                );

                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = MapUserToDto(user),
                    ExpiresAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Token = string.Empty,
                    User = null!,
                    ExpiresAt = DateTime.UtcNow,
                    ErrorMessage = $"Password reset failed: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteUserPermanentlyAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Deleted,
                    user.Id,
                    null,
                    "User permanently deleted by administrator"
                );

                await _userRepository.DeleteAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Private Helper Methods

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

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
