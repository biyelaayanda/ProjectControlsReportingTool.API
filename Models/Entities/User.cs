using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public Department Department { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginDate { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        // SuperAdmin tracking fields
        public bool RequirePasswordChange { get; set; } = false;
        public Guid? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Guid? ModifiedBy { get; set; }

        // Navigation properties
        public virtual ICollection<Report> CreatedReports { get; set; } = new List<Report>();
        public virtual ICollection<ReportSignature> Signatures { get; set; } = new List<ReportSignature>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        
        // SuperAdmin navigation properties
        public virtual ICollection<UserManagementAudit> CreatedAudits { get; set; } = new List<UserManagementAudit>();
        public virtual ICollection<UserManagementAudit> TargetedAudits { get; set; } = new List<UserManagementAudit>();
        public virtual ICollection<SuperAdminSession> SuperAdminSessions { get; set; } = new List<SuperAdminSession>();
        
        // Foreign key navigation for SuperAdmin tracking
        public virtual User? CreatedByUser { get; set; }
        public virtual User? ModifiedByUser { get; set; }

        // Computed properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string DepartmentName => Department.ToString();

        [NotMapped]
        public string RoleName => Role.ToString();
    }
}
