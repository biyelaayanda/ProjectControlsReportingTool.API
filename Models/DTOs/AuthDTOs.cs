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
}
