using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Data.Entities;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Text;
using System.Globalization;
using System.Security.Cryptography;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Business.Services
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SuperAdminService> _logger;

        public SuperAdminService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<SuperAdminService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        #region User Management

        public async Task<PagedResultDto<SuperAdminUserDetailDto>> GetUsersAsync(SuperAdminUserFiltersDto filters)
        {
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var searchTerm = filters.Search.ToLower();
                query = query.Where(u => 
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.JobTitle != null && u.JobTitle.ToLower().Contains(searchTerm));
            }

            if (filters.Role.HasValue)
            {
                query = query.Where(u => u.Role == filters.Role.Value);
            }

            if (filters.Department.HasValue)
            {
                query = query.Where(u => u.Department == filters.Department.Value);
            }

            if (filters.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filters.IsActive.Value);
            }

            if (filters.CreatedAfter.HasValue)
            {
                query = query.Where(u => u.CreatedDate >= filters.CreatedAfter.Value);
            }

            if (filters.CreatedBefore.HasValue)
            {
                query = query.Where(u => u.CreatedDate <= filters.CreatedBefore.Value);
            }

            if (filters.LastLoginAfter.HasValue)
            {
                query = query.Where(u => u.LastLoginDate >= filters.LastLoginAfter.Value);
            }

            if (filters.LastLoginBefore.HasValue)
            {
                query = query.Where(u => u.LastLoginDate <= filters.LastLoginBefore.Value);
            }

            // Apply sorting
            query = filters.SortBy?.ToLower() switch
            {
                "firstname" => filters.SortDirection == "asc" 
                    ? query.OrderBy(u => u.FirstName) 
                    : query.OrderByDescending(u => u.FirstName),
                "lastname" => filters.SortDirection == "asc" 
                    ? query.OrderBy(u => u.LastName) 
                    : query.OrderByDescending(u => u.LastName),
                "email" => filters.SortDirection == "asc" 
                    ? query.OrderBy(u => u.Email) 
                    : query.OrderByDescending(u => u.Email),
                "role" => filters.SortDirection == "asc" 
                    ? query.OrderBy(u => u.Role) 
                    : query.OrderByDescending(u => u.Role),
                "department" => filters.SortDirection == "asc" 
                    ? query.OrderBy(u => u.Department) 
                    : query.OrderByDescending(u => u.Department),
                "lastlogindate" => filters.SortDirection == "asc" 
                    ? query.OrderBy(u => u.LastLoginDate) 
                    : query.OrderByDescending(u => u.LastLoginDate),
                "isactive" => filters.SortDirection == "asc" 
                    ? query.OrderBy(u => u.IsActive) 
                    : query.OrderByDescending(u => u.IsActive),
                _ => filters.SortDirection == "asc" 
                    ? query.OrderBy(u => u.CreatedDate) 
                    : query.OrderByDescending(u => u.CreatedDate)
            };

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Select(u => new SuperAdminUserDetailDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Role = u.Role,
                    RoleName = u.Role.ToString(),
                    Department = u.Department,
                    DepartmentName = u.Department.ToString(),
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    LastLoginDate = u.LastLoginDate,
                    PhoneNumber = u.PhoneNumber,
                    JobTitle = u.JobTitle,
                    CreatedBy = u.CreatedBy,
                    ModifiedAt = u.ModifiedAt,
                    ModifiedBy = u.ModifiedBy,
                    TotalReports = u.CreatedReports.Count(),
                    RequirePasswordChange = u.RequirePasswordChange,
                    LoginCount = u.SuperAdminSessions.Count(),
                    TotalSessionHours = u.SuperAdminSessions.Sum(s => 
                        s.SessionEnd.HasValue ? 
                        (s.SessionEnd.Value - s.SessionStart).TotalHours : 
                        (DateTime.UtcNow - s.SessionStart).TotalHours)
                })
                .ToListAsync();

            return new PagedResultDto<SuperAdminUserDetailDto>
            {
                Items = users,
                TotalCount = totalCount,
                Page = filters.PageNumber,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize),
                HasNext = filters.PageNumber < (int)Math.Ceiling((double)totalCount / filters.PageSize),
                HasPrevious = filters.PageNumber > 1
            };
        }

        public async Task<SuperAdminUserDetailDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.CreatedReports)
                .Include(u => u.SuperAdminSessions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            // Get creator and modifier names
            var createdByName = user.CreatedBy.HasValue ? 
                await _context.Users
                    .Where(u => u.Id == user.CreatedBy.Value)
                    .Select(u => $"{u.FirstName} {u.LastName}")
                    .FirstOrDefaultAsync() : null;

            var modifiedByName = user.ModifiedBy.HasValue ? 
                await _context.Users
                    .Where(u => u.Id == user.ModifiedBy.Value)
                    .Select(u => $"{u.FirstName} {u.LastName}")
                    .FirstOrDefaultAsync() : null;

            // Get last activity from reports or sessions
            var lastActivity = await _context.Reports
                .Where(r => r.CreatedBy == userId)
                .OrderByDescending(r => r.LastModifiedDate)
                .Select(r => r.LastModifiedDate)
                .FirstOrDefaultAsync();

            if (lastActivity == default)
            {
                lastActivity = await _context.SuperAdminSessions
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SessionStart)
                    .Select(s => s.SessionStart)
                    .FirstOrDefaultAsync();
            }

            return new SuperAdminUserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role,
                RoleName = user.Role.ToString(),
                Department = user.Department,
                DepartmentName = user.Department.ToString(),
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                PhoneNumber = user.PhoneNumber,
                JobTitle = user.JobTitle,
                CreatedBy = user.CreatedBy,
                CreatedByName = createdByName,
                ModifiedAt = user.ModifiedAt,
                ModifiedBy = user.ModifiedBy,
                ModifiedByName = modifiedByName,
                TotalReports = user.CreatedReports?.Count ?? 0,
                LastActivity = lastActivity != default ? lastActivity : null,
                RequirePasswordChange = user.RequirePasswordChange,
                LoginCount = user.SuperAdminSessions?.Count ?? 0,
                TotalSessionHours = user.SuperAdminSessions?.Sum(s => 
                    s.SessionEnd.HasValue ? 
                    (s.SessionEnd.Value - s.SessionStart).TotalHours : 
                    (DateTime.UtcNow - s.SessionStart).TotalHours) ?? 0
            };
        }

        public async Task<SuperAdminUserDetailDto> CreateUserAsync(SuperAdminCreateUserDto createUserDto, Guid currentUserId)
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == createUserDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"A user with email '{createUserDto.Email}' already exists.");
            }

            // Prevent creating another SuperAdmin (business rule)
            if (createUserDto.Role == UserRole.SuperAdmin)
            {
                var existingSuperAdmin = await _context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin && u.IsActive);
                if (existingSuperAdmin)
                {
                    throw new InvalidOperationException("Only one SuperAdmin is allowed in the system.");
                }
            }

            // Generate password if not provided
            var password = !string.IsNullOrWhiteSpace(createUserDto.TemporaryPassword) 
                ? createUserDto.TemporaryPassword 
                : GenerateSecurePassword();

            var salt = GenerateSalt();
            var hashedPassword = HashPasswordWithSalt(password, salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                Role = createUserDto.Role,
                Department = createUserDto.Department,
                PhoneNumber = createUserDto.PhoneNumber,
                JobTitle = createUserDto.JobTitle,
                IsActive = true,
                RequirePasswordChange = createUserDto.RequirePasswordChange,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            _context.Users.Add(user);

            // Log the creation action
            var auditEntry = new UserManagementAudit
            {
                Id = Guid.NewGuid(),
                AdminUserId = currentUserId,
                TargetUserId = user.Id,
                Action = "CREATE_USER",
                Changes = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Email = createUserDto.Email,
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    Role = createUserDto.Role.ToString(),
                    Department = createUserDto.Department.ToString(),
                    PhoneNumber = createUserDto.PhoneNumber,
                    JobTitle = createUserDto.JobTitle
                }),
                Reason = createUserDto.CreationReason,
                Timestamp = DateTime.UtcNow,
                IPAddress = GetCurrentIPAddress()
            };

            _context.UserManagementAudits.Add(auditEntry);

            await _context.SaveChangesAsync();

            // Send welcome email if requested
            if (createUserDto.SendWelcomeEmail)
            {
                try
                {
                    await SendWelcomeEmail(user, password);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                    // Don't fail the entire operation if email fails
                }
            }

            return await GetUserByIdAsync(user.Id) ?? throw new InvalidOperationException("Failed to retrieve created user.");
        }

        public async Task<SuperAdminUserDetailDto?> UpdateUserAsync(Guid userId, SuperAdminUpdateUserDto updateUserDto, Guid currentUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            // Prevent modifying SuperAdmin role unless current user is SuperAdmin
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user.Role == UserRole.SuperAdmin && currentUser?.Role != UserRole.SuperAdmin)
            {
                throw new InvalidOperationException("Only SuperAdmin can modify SuperAdmin accounts.");
            }

            // Track changes for audit
            var changes = new Dictionary<string, object>();
            var originalValues = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName) && user.FirstName != updateUserDto.FirstName)
            {
                originalValues["FirstName"] = user.FirstName;
                changes["FirstName"] = updateUserDto.FirstName;
                user.FirstName = updateUserDto.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.LastName) && user.LastName != updateUserDto.LastName)
            {
                originalValues["LastName"] = user.LastName;
                changes["LastName"] = updateUserDto.LastName;
                user.LastName = updateUserDto.LastName;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.Email) && user.Email != updateUserDto.Email)
            {
                // Check if new email already exists
                var emailExists = await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != userId);
                if (emailExists)
                {
                    throw new InvalidOperationException($"A user with email '{updateUserDto.Email}' already exists.");
                }

                originalValues["Email"] = user.Email;
                changes["Email"] = updateUserDto.Email;
                user.Email = updateUserDto.Email;
            }

            if (updateUserDto.Role.HasValue && user.Role != updateUserDto.Role.Value)
            {
                originalValues["Role"] = user.Role.ToString();
                changes["Role"] = updateUserDto.Role.Value.ToString();
                user.Role = updateUserDto.Role.Value;
            }

            if (updateUserDto.Department.HasValue && user.Department != updateUserDto.Department.Value)
            {
                originalValues["Department"] = user.Department.ToString();
                changes["Department"] = updateUserDto.Department.Value.ToString();
                user.Department = updateUserDto.Department.Value;
            }

            if (updateUserDto.PhoneNumber != null && user.PhoneNumber != updateUserDto.PhoneNumber)
            {
                originalValues["PhoneNumber"] = user.PhoneNumber ?? "null";
                changes["PhoneNumber"] = updateUserDto.PhoneNumber;
                user.PhoneNumber = updateUserDto.PhoneNumber;
            }

            if (updateUserDto.JobTitle != null && user.JobTitle != updateUserDto.JobTitle)
            {
                originalValues["JobTitle"] = user.JobTitle ?? "null";
                changes["JobTitle"] = updateUserDto.JobTitle;
                user.JobTitle = updateUserDto.JobTitle;
            }

            if (updateUserDto.IsActive.HasValue && user.IsActive != updateUserDto.IsActive.Value)
            {
                originalValues["IsActive"] = user.IsActive;
                changes["IsActive"] = updateUserDto.IsActive.Value;
                user.IsActive = updateUserDto.IsActive.Value;
            }

            if (changes.Any())
            {
                user.ModifiedAt = DateTime.UtcNow;
                user.ModifiedBy = currentUserId;

                // Log the update action
                var auditEntry = new UserManagementAudit
                {
                    Id = Guid.NewGuid(),
                    AdminUserId = currentUserId,
                    TargetUserId = userId,
                    Action = "UPDATE_USER",
                    Changes = System.Text.Json.JsonSerializer.Serialize(new { Original = originalValues, Updated = changes }),
                    Reason = updateUserDto.UpdateReason,
                    Timestamp = DateTime.UtcNow,
                    IPAddress = GetCurrentIPAddress()
                };

                _context.UserManagementAudits.Add(auditEntry);
                await _context.SaveChangesAsync();
            }

            return await GetUserByIdAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(Guid userId, string reason, bool permanent, Guid currentUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return false;

            // Prevent deleting SuperAdmin
            if (user.Role == UserRole.SuperAdmin)
            {
                throw new InvalidOperationException("SuperAdmin account cannot be deleted.");
            }

            // Prevent self-deletion
            if (userId == currentUserId)
            {
                throw new InvalidOperationException("You cannot delete your own account.");
            }

            if (permanent)
            {
                // Hard delete - remove all related data
                var reports = await _context.Reports.Where(r => r.CreatedBy == userId).ToListAsync();
                var audits = await _context.UserManagementAudits.Where(a => a.TargetUserId == userId).ToListAsync();
                var sessions = await _context.SuperAdminSessions.Where(s => s.UserId == userId).ToListAsync();

                _context.Reports.RemoveRange(reports);
                _context.UserManagementAudits.RemoveRange(audits);
                _context.SuperAdminSessions.RemoveRange(sessions);
                _context.Users.Remove(user);
            }
            else
            {
                // Soft delete
                user.IsActive = false;
                user.ModifiedAt = DateTime.UtcNow;
                user.ModifiedBy = currentUserId;
            }

            // Log the deletion action
            var auditEntry = new UserManagementAudit
            {
                Id = Guid.NewGuid(),
                AdminUserId = currentUserId,
                TargetUserId = userId,
                Action = permanent ? "PERMANENT_DELETE_USER" : "SOFT_DELETE_USER",
                Changes = System.Text.Json.JsonSerializer.Serialize(new
                {
                    UserEmail = user.Email,
                    UserName = $"{user.FirstName} {user.LastName}",
                    Permanent = permanent
                }),
                Reason = reason,
                Timestamp = DateTime.UtcNow,
                IPAddress = GetCurrentIPAddress()
            };

            _context.UserManagementAudits.Add(auditEntry);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<SuperAdminPasswordResetResultDto?> ResetUserPasswordAsync(Guid userId, bool sendEmail, Guid currentUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            var newPassword = GenerateSecurePassword();
            var newSalt = GenerateSalt();
            user.PasswordHash = HashPasswordWithSalt(newPassword, newSalt);
            user.PasswordSalt = newSalt;
            user.RequirePasswordChange = true;
            user.ModifiedAt = DateTime.UtcNow;
            user.ModifiedBy = currentUserId;

            // Log the password reset action
            var auditEntry = new UserManagementAudit
            {
                Id = Guid.NewGuid(),
                AdminUserId = currentUserId,
                TargetUserId = userId,
                Action = "RESET_PASSWORD",
                Changes = System.Text.Json.JsonSerializer.Serialize(new
                {
                    UserEmail = user.Email,
                    RequirePasswordChange = true
                }),
                Reason = "Admin-initiated password reset",
                Timestamp = DateTime.UtcNow,
                IPAddress = GetCurrentIPAddress()
            };

            _context.UserManagementAudits.Add(auditEntry);
            await _context.SaveChangesAsync();

            var result = new SuperAdminPasswordResetResultDto
            {
                TemporaryPassword = newPassword,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Password valid for 7 days
                EmailSent = false
            };

            if (sendEmail)
            {
                try
                {
                    await SendPasswordResetEmail(user, newPassword);
                    result.EmailSent = true;
                }
                catch (Exception ex)
                {
                    result.EmailSent = false;
                    result.EmailError = ex.Message;
                    _logger.LogWarning(ex, "Failed to send password reset email to {Email}", user.Email);
                }
            }

            return result;
        }

        public async Task<SuperAdminUserDetailDto?> ToggleUserStatusAsync(Guid userId, string reason, Guid currentUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            // Prevent disabling SuperAdmin
            if (user.Role == UserRole.SuperAdmin && user.IsActive)
            {
                throw new InvalidOperationException("SuperAdmin account cannot be disabled.");
            }

            // Prevent self-disabling
            if (userId == currentUserId && user.IsActive)
            {
                throw new InvalidOperationException("You cannot disable your own account.");
            }

            var originalStatus = user.IsActive;
            user.IsActive = !user.IsActive;
            user.ModifiedAt = DateTime.UtcNow;
            user.ModifiedBy = currentUserId;

            // Log the status change action
            var auditEntry = new UserManagementAudit
            {
                Id = Guid.NewGuid(),
                AdminUserId = currentUserId,
                TargetUserId = userId,
                Action = user.IsActive ? "ENABLE_USER" : "DISABLE_USER",
                Changes = System.Text.Json.JsonSerializer.Serialize(new
                {
                    UserEmail = user.Email,
                    FromStatus = originalStatus ? "Active" : "Inactive",
                    ToStatus = user.IsActive ? "Active" : "Inactive"
                }),
                Reason = reason,
                Timestamp = DateTime.UtcNow,
                IPAddress = GetCurrentIPAddress()
            };

            _context.UserManagementAudits.Add(auditEntry);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }

        #endregion

        #region Bulk Operations

        public async Task<SuperAdminBulkOperationResultDto> BulkCreateUsersAsync(SuperAdminBulkCreateUsersDto bulkCreateDto, Guid currentUserId)
        {
            var result = new SuperAdminBulkOperationResultDto
            {
                TotalProcessed = bulkCreateDto.Users.Count
            };

            foreach (var (userDto, index) in bulkCreateDto.Users.Select((u, i) => (u, i)))
            {
                try
                {
                    var createdUser = await CreateUserAsync(userDto, currentUserId);
                    result.ProcessedUsers.Add(createdUser);
                    result.Successful++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add(new SuperAdminBulkOperationErrorDto
                    {
                        RowNumber = index + 1,
                        Email = userDto.Email,
                        Error = ex.Message,
                        Details = ex.InnerException?.Message
                    });
                    _logger.LogWarning(ex, "Failed to create user in bulk operation: {Email}", userDto.Email);
                }
            }

            return result;
        }

        public async Task<SuperAdminBulkOperationResultDto> BulkUpdateUsersAsync(SuperAdminBulkUpdateUsersDto bulkUpdateDto, Guid currentUserId)
        {
            var result = new SuperAdminBulkOperationResultDto
            {
                TotalProcessed = bulkUpdateDto.Updates.Count
            };

            foreach (var (updateItem, index) in bulkUpdateDto.Updates.Select((u, i) => (u, i)))
            {
                try
                {
                    var updatedUser = await UpdateUserAsync(updateItem.UserId, updateItem.Data, currentUserId);
                    if (updatedUser != null)
                    {
                        result.ProcessedUsers.Add(updatedUser);
                        result.Successful++;
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add(new SuperAdminBulkOperationErrorDto
                        {
                            RowNumber = index + 1,
                            Email = "Unknown",
                            Error = $"User with ID {updateItem.UserId} not found"
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add(new SuperAdminBulkOperationErrorDto
                    {
                        RowNumber = index + 1,
                        Email = "Unknown",
                        Error = ex.Message,
                        Details = ex.InnerException?.Message
                    });
                    _logger.LogWarning(ex, "Failed to update user in bulk operation: {UserId}", updateItem.UserId);
                }
            }

            return result;
        }

        public async Task<SuperAdminBulkOperationResultDto> BulkDeleteUsersAsync(SuperAdminBulkDeleteUsersDto bulkDeleteDto, Guid currentUserId)
        {
            var result = new SuperAdminBulkOperationResultDto
            {
                TotalProcessed = bulkDeleteDto.UserIds.Count
            };

            foreach (var (userId, index) in bulkDeleteDto.UserIds.Select((id, i) => (id, i)))
            {
                try
                {
                    var success = await DeleteUserAsync(userId, bulkDeleteDto.Reason, bulkDeleteDto.PermanentDelete, currentUserId);
                    if (success)
                    {
                        result.Successful++;
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add(new SuperAdminBulkOperationErrorDto
                        {
                            RowNumber = index + 1,
                            Email = "Unknown",
                            Error = $"User with ID {userId} not found"
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add(new SuperAdminBulkOperationErrorDto
                    {
                        RowNumber = index + 1,
                        Email = "Unknown",
                        Error = ex.Message,
                        Details = ex.InnerException?.Message
                    });
                    _logger.LogWarning(ex, "Failed to delete user in bulk operation: {UserId}", userId);
                }
            }

            return result;
        }

        #endregion

        #region Import/Export

        public async Task<SuperAdminImportResultDto> ImportUsersAsync(IFormFile file, bool preview, bool sendWelcomeEmails, Guid currentUserId)
        {
            var result = new SuperAdminImportResultDto();
            
            // Simple CSV parsing for now - will implement proper CSV handling later
            using var reader = new StringReader(Encoding.UTF8.GetString(await GetFileBytes(file)));
            var lines = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            if (lines.Count <= 1) // Header only or empty
            {
                result.TotalRows = 0;
                result.ProcessedRows = 0;
                return result;
            }

            result.TotalRows = lines.Count - 1; // Exclude header
            result.ProcessedRows = result.TotalRows;

            // For now, return a placeholder implementation
            result.SuccessfulRows = 0;
            result.FailedRows = result.TotalRows;
            result.Errors.Add(new SuperAdminImportErrorDto
            {
                RowNumber = 1,
                Field = "General",
                Value = "CSV Import",
                Error = "CSV import functionality will be implemented in Phase 2"
            });

            return result;
        }

        public async Task<byte[]> ExportUsersAsync(SuperAdminUserFiltersDto filters)
        {
            var users = await GetUsersAsync(new SuperAdminUserFiltersDto
            {
                Search = filters.Search,
                Role = filters.Role,
                Department = filters.Department,
                IsActive = filters.IsActive,
                CreatedAfter = filters.CreatedAfter,
                CreatedBefore = filters.CreatedBefore,
                LastLoginAfter = filters.LastLoginAfter,
                LastLoginBefore = filters.LastLoginBefore,
                PageNumber = 1,
                PageSize = int.MaxValue, // Get all records for export
                SortBy = filters.SortBy,
                SortDirection = filters.SortDirection
            });

            var csv = new StringBuilder();
            csv.AppendLine("Email,FirstName,LastName,Role,Department,PhoneNumber,JobTitle,IsActive,CreatedDate,LastLoginDate,TotalReports,LoginCount");

            foreach (var user in users.Items)
            {
                csv.AppendLine($"{user.Email},{user.FirstName},{user.LastName},{user.RoleName},{user.DepartmentName},{user.PhoneNumber},{user.JobTitle},{user.IsActive},{user.CreatedDate:yyyy-MM-dd HH:mm:ss},{user.LastLoginDate:yyyy-MM-dd HH:mm:ss},{user.TotalReports},{user.LoginCount}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        #endregion

        #region Audit Reports

        public async Task<SuperAdminDepartmentAuditReportDto> GenerateDepartmentReportAsync(SuperAdminDepartmentReportParamsDto parameters, Guid currentUserId)
        {
            // Implementation would generate comprehensive department audit report
            // This is a placeholder for the complex report generation logic
            throw new NotImplementedException("Department audit report generation will be implemented in Phase 2");
        }

        public async Task<SuperAdminUserAuditReportDto> GenerateUserReportAsync(SuperAdminUserReportParamsDto parameters, Guid currentUserId)
        {
            // Implementation would generate comprehensive user audit report
            // This is a placeholder for the complex report generation logic
            throw new NotImplementedException("User audit report generation will be implemented in Phase 2");
        }

        #endregion

        #region System Administration

        public async Task<SuperAdminSystemHealthReportDto> GetSystemHealthAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var recentLogins = await _context.Users.CountAsync(u => u.LastLoginDate >= DateTime.UtcNow.AddDays(-7));

            return new SuperAdminSystemHealthReportDto
            {
                SystemStatus = "Healthy", // This would be calculated based on various metrics
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                SystemUptime = 99.9, // This would be calculated from system metrics
                AverageResponseTime = 150, // This would be calculated from performance metrics
                ErrorRate = 0.1, // This would be calculated from error logs
                Resources = new SuperAdminSystemResourcesDto
                {
                    CpuUsage = 45.2,
                    MemoryUsage = 62.8,
                    DiskUsage = 34.1,
                    DatabaseConnections = 12,
                    DatabaseSize = 2147483648 // 2GB in bytes
                },
                Features = new List<SuperAdminFeatureHealthDto>
                {
                    new SuperAdminFeatureHealthDto
                    {
                        Name = "User Management",
                        Status = "Healthy",
                        Uptime = 99.9,
                        ResponseTime = 120
                    },
                    new SuperAdminFeatureHealthDto
                    {
                        Name = "Report Generation",
                        Status = "Healthy",
                        Uptime = 99.8,
                        ResponseTime = 250
                    },
                    new SuperAdminFeatureHealthDto
                    {
                        Name = "Notifications",
                        Status = "Healthy",
                        Uptime = 99.9,
                        ResponseTime = 95
                    }
                }
            };
        }

        public async Task<PagedResultDto<SuperAdminAuditEntryDto>> GetAuditTrailAsync(SuperAdminAuditTrailFiltersDto filters)
        {
            var query = _context.UserManagementAudits
                .Include(a => a.AdminUser)
                .Include(a => a.TargetUser)
                .AsQueryable();

            // Apply filters
            if (filters.UserId.HasValue)
            {
                query = query.Where(a => a.AdminUserId == filters.UserId.Value || a.TargetUserId == filters.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.Action))
            {
                query = query.Where(a => a.Action.Contains(filters.Action));
            }

            if (filters.FromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= filters.FromDate.Value);
            }

            if (filters.ToDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= filters.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(a => 
                    a.Action.ToLower().Contains(searchTerm) ||
                    a.Reason != null && a.Reason.ToLower().Contains(searchTerm) ||
                    a.AdminUser.Email.ToLower().Contains(searchTerm) ||
                    (a.TargetUser != null && a.TargetUser.Email.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            query = filters.SortBy?.ToLower() switch
            {
                "action" => filters.SortDirection == "asc" 
                    ? query.OrderBy(a => a.Action) 
                    : query.OrderByDescending(a => a.Action),
                "adminuser" => filters.SortDirection == "asc" 
                    ? query.OrderBy(a => a.AdminUser.Email) 
                    : query.OrderByDescending(a => a.AdminUser.Email),
                "targetuser" => filters.SortDirection == "asc" 
                    ? query.OrderBy(a => a.TargetUser!.Email) 
                    : query.OrderByDescending(a => a.TargetUser!.Email),
                _ => filters.SortDirection == "asc" 
                    ? query.OrderBy(a => a.Timestamp) 
                    : query.OrderByDescending(a => a.Timestamp)
            };

            var totalCount = await query.CountAsync();

            var auditEntries = await query
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Select(a => new SuperAdminAuditEntryDto
                {
                    Id = a.Id,
                    AdminUserId = a.AdminUserId,
                    AdminUserName = $"{a.AdminUser.FirstName} {a.AdminUser.LastName}",
                    TargetUserId = a.TargetUserId,
                    TargetUserName = a.TargetUser != null ? $"{a.TargetUser.FirstName} {a.TargetUser.LastName}" : null,
                    Action = a.Action,
                    Changes = a.Changes,
                    Reason = a.Reason,
                    Timestamp = a.Timestamp,
                    IPAddress = a.IPAddress,
                    ActionDescription = GetActionDescription(a.Action)
                })
                .ToListAsync();

            return new PagedResultDto<SuperAdminAuditEntryDto>
            {
                Items = auditEntries,
                TotalCount = totalCount,
                Page = filters.PageNumber,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize),
                HasNext = filters.PageNumber < (int)Math.Ceiling((double)totalCount / filters.PageSize),
                HasPrevious = filters.PageNumber > 1
            };
        }

        public async Task<object> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var recentLogins = await _context.Users.CountAsync(u => u.LastLoginDate >= DateTime.UtcNow.AddDays(-7));
            var pendingPasswordResets = await _context.Users.CountAsync(u => u.RequirePasswordChange);

            var usersByRole = await _context.Users
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var usersByDepartment = await _context.Users
                .GroupBy(u => u.Department)
                .Select(g => new { Department = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var recentAuditActions = await _context.UserManagementAudits
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .Include(a => a.AdminUser)
                .Include(a => a.TargetUser)
                .Select(a => new
                {
                    Action = a.Action,
                    AdminUser = $"{a.AdminUser.FirstName} {a.AdminUser.LastName}",
                    TargetUser = a.TargetUser != null ? $"{a.TargetUser.FirstName} {a.TargetUser.LastName}" : null,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            return new
            {
                UserStats = new
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    RecentLogins = recentLogins,
                    PendingPasswordResets = pendingPasswordResets
                },
                UsersByRole = usersByRole,
                UsersByDepartment = usersByDepartment,
                RecentActivity = recentAuditActions,
                SystemHealth = new
                {
                    Status = "Healthy",
                    Uptime = 99.9,
                    ResponseTime = 150,
                    ErrorRate = 0.1
                }
            };
        }

        #endregion

        #region Session Management

        public async Task LogAdminSessionAsync(Guid adminUserId, string action, string ipAddress)
        {
            var session = new SuperAdminSession
            {
                Id = Guid.NewGuid(),
                UserId = adminUserId,
                SessionStart = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = GetCurrentUserAgent()
            };

            _context.SuperAdminSessions.Add(session);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateAdminPermissionsAsync(Guid adminUserId, string operation)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminUserId);
            if (user == null || !user.IsActive || user.Role != UserRole.SuperAdmin)
            {
                return false;
            }

            // Log the permission check
            _logger.LogInformation("Permission validated for SuperAdmin {UserId} for operation: {Operation}", 
                adminUserId, operation);

            return true;
        }

        #endregion

        #region Helper Methods

        private string GenerateSecurePassword()
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            var random = new Random();
            var chars = new char[12];
            for (int i = 0; i < 12; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }
            return new string(chars);
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPasswordWithSalt(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashedBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private async Task<byte[]> GetFileBytes(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private async Task SendWelcomeEmail(User user, string temporaryPassword)
        {
            var emailDto = new SystemEmailDto
            {
                RecipientEmail = user.Email,
                RecipientName = $"{user.FirstName} {user.LastName}",
                Title = "Welcome to Project Controls Reporting Tool",
                Message = $@"
                    <h2>Welcome to Project Controls Reporting Tool</h2>
                    <p>Hello {user.FirstName} {user.LastName},</p>
                    <p>Your account has been created by an administrator. Here are your login details:</p>
                    <p><strong>Email:</strong> {user.Email}</p>
                    <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                    <p><strong>Note:</strong> You will be required to change your password upon first login.</p>
                    <p>Please log in at your earliest convenience to access the system.</p>
                    <p>Best regards,<br>Project Controls Team</p>
                ",
                NotificationType = NotificationType.UserWelcome
            };

            // Send using email service with proper parameters
            await _emailService.SendEmailAsync(
                emailDto.RecipientEmail,
                emailDto.RecipientName,
                emailDto.Title,
                emailDto.Message
            );
        }

        private async Task SendPasswordResetEmail(User user, string temporaryPassword)
        {
            var emailDto = new SystemEmailDto
            {
                RecipientEmail = user.Email,
                RecipientName = $"{user.FirstName} {user.LastName}",
                Title = "Password Reset - Project Controls Reporting Tool",
                Message = $@"
                    <h2>Password Reset</h2>
                    <p>Hello {user.FirstName} {user.LastName},</p>
                    <p>Your password has been reset by an administrator. Here is your temporary password:</p>
                    <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                    <p><strong>Note:</strong> You will be required to change this password upon your next login.</p>
                    <p>If you did not request this password reset, please contact your administrator immediately.</p>
                    <p>Best regards,<br>Project Controls Team</p>
                ",
                NotificationType = NotificationType.PasswordReset
            };

            // Send using email service with proper parameters
            await _emailService.SendEmailAsync(
                emailDto.RecipientEmail,
                emailDto.RecipientName,
                emailDto.Title,
                emailDto.Message
            );
        }

        private List<ImportValidationError> ValidateImportRecord(UserImportRecord record)
        {
            var errors = new List<ImportValidationError>();

            if (string.IsNullOrWhiteSpace(record.Email) || !IsValidEmail(record.Email))
            {
                errors.Add(new ImportValidationError("Email", record.Email, "Invalid email address"));
            }

            if (string.IsNullOrWhiteSpace(record.FirstName))
            {
                errors.Add(new ImportValidationError("FirstName", record.FirstName, "First name is required"));
            }

            if (string.IsNullOrWhiteSpace(record.LastName))
            {
                errors.Add(new ImportValidationError("LastName", record.LastName, "Last name is required"));
            }

            if (!Enum.IsDefined(typeof(UserRole), record.Role))
            {
                errors.Add(new ImportValidationError("Role", record.Role.ToString(), "Invalid role"));
            }

            if (!Enum.IsDefined(typeof(Department), record.Department))
            {
                errors.Add(new ImportValidationError("Department", record.Department.ToString(), "Invalid department"));
            }

            return errors;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GetActionDescription(string action)
        {
            return action switch
            {
                "CREATE_USER" => "Created new user account",
                "UPDATE_USER" => "Updated user information",
                "DELETE_USER" => "Deleted user account",
                "SOFT_DELETE_USER" => "Deactivated user account",
                "PERMANENT_DELETE_USER" => "Permanently deleted user account",
                "RESET_PASSWORD" => "Reset user password",
                "ENABLE_USER" => "Enabled user account",
                "DISABLE_USER" => "Disabled user account",
                _ => action
            };
        }

        private string GetCurrentIPAddress()
        {
            // This would typically be injected via IHttpContextAccessor
            return "127.0.0.1"; // Placeholder
        }

        private string GetCurrentUserAgent()
        {
            // This would typically be injected via IHttpContextAccessor
            return "SuperAdmin-Service"; // Placeholder
        }

        #endregion

        #region Helper Classes

        private class UserImportRecord
        {
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public UserRole Role { get; set; }
            public Department Department { get; set; }
            public string? PhoneNumber { get; set; }
            public string? JobTitle { get; set; }
        }

        private class ImportValidationError
        {
            public string Field { get; }
            public string Value { get; }
            public string Error { get; }

            public ImportValidationError(string field, string value, string error)
            {
                Field = field;
                Value = value ?? string.Empty;
                Error = error;
            }
        }

        #endregion
    }
}
