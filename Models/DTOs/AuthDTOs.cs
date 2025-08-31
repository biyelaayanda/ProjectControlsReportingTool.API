using System.ComponentModel.DataAnnotations;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public Department Department { get; set; }

        public string? PhoneNumber { get; set; }

        public string? JobTitle { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public Department Department { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? JobTitle { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [Required]
        [MinLength(2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? JobTitle { get; set; }
    }

    // Bulk Operations DTOs
    public class BulkUserOperationDto
    {
        [Required]
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }

    public class BulkRoleAssignmentDto : BulkUserOperationDto
    {
        [Required]
        public UserRole Role { get; set; }
    }

    public class BulkDepartmentChangeDto : BulkUserOperationDto
    {
        [Required]
        public Department Department { get; set; }
    }

    public class BulkActivationDto : BulkUserOperationDto
    {
        [Required]
        public bool IsActive { get; set; }
    }

    public class UserImportDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public Department Department { get; set; }

        public string? PhoneNumber { get; set; }

        public string? JobTitle { get; set; }

        // Default password will be generated if not provided
        public string? Password { get; set; }
    }

    public class BulkUserImportDto
    {
        [Required]
        public List<UserImportDto> Users { get; set; } = new List<UserImportDto>();

        // Send email to users with their temporary passwords
        public bool SendWelcomeEmails { get; set; } = false;
    }

    public class BulkOperationResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessfulOperations { get; set; }
        public int FailedOperations { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public List<UserDto> ProcessedUsers { get; set; } = new List<UserDto>();
        public bool IsSuccess => FailedOperations == 0;
    }

    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public UserRole? Role { get; set; }
        public Department? Department { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminUserUpdateDto
    {
        [Required]
        [MinLength(2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public Department Department { get; set; }

        public string? PhoneNumber { get; set; }

        public string? JobTitle { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
