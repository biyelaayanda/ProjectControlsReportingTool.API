using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Type { get; set; }

        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        public DateTime? DueDate { get; set; }

        [Required]
        public ReportStatus Status { get; set; } = ReportStatus.Draft;

        [Required]
        public Guid CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        public DateTime? SubmittedDate { get; set; }

        public DateTime? ManagerApprovedDate { get; set; }

        public DateTime? ExecutiveApprovedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [StringLength(1000)]
        public string? RejectionReason { get; set; }

        public Guid? RejectedBy { get; set; }

        public DateTime? RejectedDate { get; set; }

        [StringLength(50)]
        public string? ReportNumber { get; set; }

        public Department Department { get; set; }

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; } = null!;

        [ForeignKey("RejectedBy")]
        public virtual User? RejectedByUser { get; set; }

        public virtual ICollection<ReportSignature> Signatures { get; set; } = new List<ReportSignature>();
        public virtual ICollection<ReportAttachment> Attachments { get; set; } = new List<ReportAttachment>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Computed properties
        [NotMapped]
        public string StatusName => Status.ToString();

        [NotMapped]
        public string DepartmentName => Department.ToString();

        [NotMapped]
        public bool CanBeEdited => Status == ReportStatus.Draft;

        [NotMapped]
        public bool CanBeSubmitted => Status == ReportStatus.Draft;

        [NotMapped]
        public bool IsInProgress => Status != ReportStatus.Completed && Status != ReportStatus.Rejected;
    }
}
