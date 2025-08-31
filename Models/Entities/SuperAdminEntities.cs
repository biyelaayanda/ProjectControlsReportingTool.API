using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    [Table("UserManagementAudit")]
    public class UserManagementAudit
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AdminUserId { get; set; }

        public Guid? TargetUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Created, Modified, Deleted, RoleChanged, Activated, Deactivated

        public string? Changes { get; set; } // JSON of changes

        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(45)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Navigation properties
        [ForeignKey("AdminUserId")]
        public virtual User AdminUser { get; set; } = null!;

        [ForeignKey("TargetUserId")]
        public virtual User? TargetUser { get; set; }
    }

    [Table("SuperAdminSessions")]
    public class SuperAdminSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime SessionStart { get; set; } = DateTime.UtcNow;

        public DateTime? SessionEnd { get; set; }

        public int ActionsPerformed { get; set; } = 0;

        [StringLength(45)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public TimeSpan? SessionDuration => SessionEnd.HasValue ? SessionEnd.Value - SessionStart : null;

        [NotMapped]
        public bool IsActive => !SessionEnd.HasValue;
    }

    [Table("AuditReportCache")]
    public class AuditReportCache
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string ReportType { get; set; } = string.Empty; // Department, User, System

        public string? Parameters { get; set; } // JSON parameters

        [Required]
        public Guid GeneratedBy { get; set; }

        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public string? ReportData { get; set; } // JSON report data

        // Navigation properties
        [ForeignKey("GeneratedBy")]
        public virtual User GeneratedByUser { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        [NotMapped]
        public TimeSpan TimeToExpiry => ExpiresAt - DateTime.UtcNow;
    }
}
