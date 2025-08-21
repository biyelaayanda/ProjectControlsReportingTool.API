using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.DTOs
{
    public class ReportDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
        public ReportStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public UserRole CreatorRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ManagerApprovedDate { get; set; }
        public DateTime? ExecutiveApprovedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? RejectionReason { get; set; }
        public Guid? RejectedBy { get; set; }
        public string? RejectedByName { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? ReportNumber { get; set; }
        public Department Department { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool CanBeEdited { get; set; }
        public bool CanBeSubmitted { get; set; }
        public bool IsInProgress { get; set; }
    }

    public class ReportSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
        public ReportStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public UserRole CreatorRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? ReportNumber { get; set; }
        public Department Department { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class ReportDetailDto : ReportDto
    {
        public new string CreatedByName { get; set; } = string.Empty;
        public new string? RejectedBy { get; set; }
        public List<ReportSignatureDto> Signatures { get; set; } = new();
        public List<ReportAttachmentDto> Attachments { get; set; } = new();
    }

    public class CreateReportDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
        public Department Department { get; set; }
    }

    public class UpdateReportDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
    }

    public class ReportApprovalDto
    {
        public string? Comments { get; set; }
    }

    public class ReportRejectionDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ReportSignatureDto
    {
        public Guid Id { get; set; }
        public Guid ReportId { get; set; }
        public Guid SignedBy { get; set; }
        public string SignedByName { get; set; } = string.Empty;
        public SignatureType SignatureType { get; set; }
        public DateTime SignedDate { get; set; }
        public string? Comments { get; set; }
        public bool IsActive { get; set; }
    }

    public class ReportAttachmentDto
    {
        public Guid Id { get; set; }
        public Guid ReportId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public string? MimeType => ContentType;
        public string? Description { get; set; }
        public Guid UploadedBy { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public bool IsActive { get; set; }
        public ApprovalStage ApprovalStage { get; set; }
        public string ApprovalStageName { get; set; } = string.Empty;
        public UserRole UploadedByRole { get; set; }
        public string UploadedByRoleName { get; set; } = string.Empty;
    }
}
