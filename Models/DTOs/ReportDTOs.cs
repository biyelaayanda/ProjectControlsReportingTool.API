using System.ComponentModel.DataAnnotations;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    public class CreateReportDto
    {
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
        public Department Department { get; set; }
        
        // File attachments support
        public List<IFormFile>? Attachments { get; set; }
    }

    public class UpdateReportDto
    {
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
    }

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
        public string CreatorName { get; set; } = string.Empty;
        public UserRole CreatorRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ManagerApprovedDate { get; set; }
        public DateTime? GMApprovedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? RejectionReason { get; set; }
        public string? RejectedByName { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? ReportNumber { get; set; }
        public Department Department { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool CanBeEdited { get; set; }
        public bool CanBeSubmitted { get; set; }
        public bool IsInProgress { get; set; }
        public List<ReportSignatureDto> Signatures { get; set; } = new();
        public List<ReportAttachmentDto> Attachments { get; set; } = new();
    }

    public class ReportSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ReportStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public UserRole CreatorRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? ReportNumber { get; set; }
        public Department Department { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool CanBeEdited { get; set; }
        public int AttachmentCount { get; set; }
    }

    public class ReportSignatureDto
    {
        public Guid Id { get; set; }
        public Guid ReportId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public SignatureType SignatureType { get; set; }
        public string SignatureTypeName { get; set; } = string.Empty;
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
        public string? ContentType { get; set; }
        public string? MimeType => ContentType;
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid UploadedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public ApprovalStage ApprovalStage { get; set; }
        public string ApprovalStageName { get; set; } = string.Empty;
        public UserRole UploadedByRole { get; set; }
        public string UploadedByRoleName { get; set; } = string.Empty;
    }

    public class ApproveReportDto
    {
        [StringLength(1000)]
        public string? Comments { get; set; }
    }

    public class RejectReportDto
    {
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class SubmitReportDto
    {
        [StringLength(500)]
        public string? Comments { get; set; }
    }

    public class ReportFilterDto
    {
        public ReportStatus? Status { get; set; }
        public Department? Department { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true;
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    // Additional DTOs for controller methods
    public class UpdateReportStatusDto
    {
        [Required]
        public ReportStatus Status { get; set; }
        public string? Comments { get; set; }
    }

    public class ApprovalDto
    {
        public string? Comments { get; set; }
    }

    public class RejectionDto
    {
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReportDetailDto : ReportDto
    {
        // Inherits all properties from ReportDto
        // Additional detail properties are already included in ReportDto
    }

    public class ServiceResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public object? Data { get; set; }

        public static ServiceResultDto SuccessResult(object? data = null)
        {
            return new ServiceResultDto { Success = true, Data = data };
        }

        public static ServiceResultDto ErrorResult(string errorMessage)
        {
            return new ServiceResultDto { Success = false, ErrorMessage = errorMessage };
        }
    }
}
