using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    [Table("ReportSignatures")]
    public class ReportSignature
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ReportId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public SignatureType SignatureType { get; set; }

        [Required]
        public DateTime SignedDate { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? Comments { get; set; }

        [StringLength(500)]
        public string? SignatureFilePath { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ReportId")]
        public virtual Report Report { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public string SignatureTypeName => SignatureType.ToString();
    }

    [Table("ReportAttachments")]
    public class ReportAttachment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ReportId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        [Required]
        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid UploadedBy { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        [ForeignKey("ReportId")]
        public virtual Report Report { get; set; } = null!;

        [ForeignKey("UploadedBy")]
        public virtual User UploadedByUser { get; set; } = null!;
    }

    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public AuditAction Action { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public Guid? ReportId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? Details { get; set; }

        [StringLength(100)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ReportId")]
        public virtual Report? Report { get; set; }

        // Computed properties
        [NotMapped]
        public string ActionName => Action.ToString();
    }
}
