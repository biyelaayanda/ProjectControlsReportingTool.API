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

    // Report Template DTOs
    public class CreateReportTemplateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string ContentTemplate { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Type { get; set; }

        [StringLength(20)]
        public string DefaultPriority { get; set; } = "Medium";

        public Department? DefaultDepartment { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        public int SortOrder { get; set; } = 0;

        [StringLength(200)]
        public string? DefaultTitle { get; set; }

        public int? DefaultDueDays { get; set; }

        [StringLength(1000)]
        public string? Instructions { get; set; }
    }

    public class UpdateReportTemplateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string ContentTemplate { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Type { get; set; }

        [StringLength(20)]
        public string DefaultPriority { get; set; } = "Medium";

        public Department? DefaultDepartment { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Tags { get; set; }

        public int SortOrder { get; set; } = 0;

        [StringLength(200)]
        public string? DefaultTitle { get; set; }

        public int? DefaultDueDays { get; set; }

        [StringLength(1000)]
        public string? Instructions { get; set; }
    }

    public class ReportTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ContentTemplate { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string DefaultPriority { get; set; } = "Medium";
        public Department? DefaultDepartment { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSystemTemplate { get; set; }
        public bool IsEditable { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public string? LastModifiedByName { get; set; }
        public string? Tags { get; set; }
        public List<string> TagList { get; set; } = new List<string>();
        public int SortOrder { get; set; }
        public string? DefaultTitle { get; set; }
        public int? DefaultDueDays { get; set; }
        public string? Instructions { get; set; }
        public int UsageCount { get; set; }
    }

    public class ReportTemplateSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public Department? DefaultDepartment { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSystemTemplate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public List<string> TagList { get; set; } = new List<string>();
        public int UsageCount { get; set; }
    }

    public class CreateReportFromTemplateDto
    {
        [Required]
        public Guid TemplateId { get; set; }

        [StringLength(200)]
        public string? CustomTitle { get; set; }

        [StringLength(500)]
        public string? CustomDescription { get; set; }

        public DateTime? CustomDueDate { get; set; }

        public Department? CustomDepartment { get; set; }

        [StringLength(20)]
        public string? CustomPriority { get; set; }

        // Template variable replacements
        public Dictionary<string, string>? VariableReplacements { get; set; }
    }

    public class ReportTemplateFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Type { get; set; }
        public Department? Department { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSystemTemplate { get; set; }
        public string? Tag { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    // Statistics DTOs
    public class ReportStatisticsDto
    {
        public OverallStatsDto OverallStats { get; set; } = new();
        public IEnumerable<DepartmentStatsDto> DepartmentStats { get; set; } = new List<DepartmentStatsDto>();
        public IEnumerable<StatusStatsDto> StatusStats { get; set; } = new List<StatusStatsDto>();
        public IEnumerable<PriorityStatsDto> PriorityStats { get; set; } = new List<PriorityStatsDto>();
        public PerformanceMetricsDto PerformanceMetrics { get; set; } = new();
        public IEnumerable<TrendDataDto> TrendAnalysis { get; set; } = new List<TrendDataDto>();
        public UserStatsDto? UserSpecificStats { get; set; }
    }

    public class OverallStatsDto
    {
        public int TotalReports { get; set; }
        public int TotalDrafts { get; set; }
        public int TotalSubmitted { get; set; }
        public int TotalUnderReview { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int TotalOverdue { get; set; }
        public int TotalThisMonth { get; set; }
        public int TotalLastMonth { get; set; }
        public double MonthOverMonthGrowth { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class DepartmentStatsDto
    {
        public Department Department { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int ApprovedReports { get; set; }
        public int RejectedReports { get; set; }
        public int OverdueReports { get; set; }
        public double AverageCompletionTime { get; set; } // in days
        public double ApprovalRate { get; set; } // percentage
        public int ActiveUsers { get; set; }
    }

    public class StatusStatsDto
    {
        public ReportStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public double AverageTimeInStatus { get; set; } // in days
        public int TrendDirection { get; set; } // -1: down, 0: stable, 1: up
    }

    public class PriorityStatsDto
    {
        public string Priority { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public double AverageCompletionTime { get; set; }
        public int OverdueCount { get; set; }
    }

    public class PerformanceMetricsDto
    {
        public double AverageReportCreationTime { get; set; } // in minutes
        public double AverageApprovalTime { get; set; } // in days
        public double AverageReviewCycleTime { get; set; } // in days
        public double SystemUptime { get; set; } // percentage
        public int TotalApiCalls { get; set; }
        public double AverageResponseTime { get; set; } // in milliseconds
        public int ErrorRate { get; set; } // errors per 1000 requests
        public double UserSatisfactionScore { get; set; } // 1-5 rating
        public DateTime MeasurementPeriodStart { get; set; }
        public DateTime MeasurementPeriodEnd { get; set; }
    }

    public class TrendDataDto
    {
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty; // "daily", "weekly", "monthly"
        public int ReportsCreated { get; set; }
        public int ReportsApproved { get; set; }
        public int ReportsRejected { get; set; }
        public int ReportsSubmitted { get; set; }
        public double AverageProcessingTime { get; set; }
        public int ActiveUsers { get; set; }
        public Department? Department { get; set; }
        public string? Metric { get; set; } // for specific metric tracking
        public double Value { get; set; } // generic value for metric tracking
    }

    public class UserStatsDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int MyReportsCount { get; set; }
        public int MyDraftsCount { get; set; }
        public int MyPendingApprovals { get; set; }
        public int MyApprovedReports { get; set; }
        public int MyRejectedReports { get; set; }
        public int MyOverdueReports { get; set; }
        public double MyAverageCompletionTime { get; set; }
        public double MyApprovalRate { get; set; }
        public int ReportsReviewedByMe { get; set; } // for managers/GMs
        public double MyAverageReviewTime { get; set; } // for managers/GMs
        public int MyTeamReportsCount { get; set; } // for managers
        public DateTime LastLoginDate { get; set; }
        public DateTime LastReportCreated { get; set; }
    }

    public class StatisticsFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Department? Department { get; set; }
        public Guid? UserId { get; set; }
        public ReportStatus? Status { get; set; }
        public string? Priority { get; set; }
        public string? TrendPeriod { get; set; } = "monthly"; // daily, weekly, monthly, yearly
        public bool IncludePerformanceMetrics { get; set; } = true;
        public bool IncludeTrendAnalysis { get; set; } = true;
        public bool IncludeUserStats { get; set; } = true;
        public string? MetricType { get; set; } // for specific metric requests
    }

    public class SystemPerformanceDto
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DatabaseResponseTime { get; set; }
        public int ActiveConnections { get; set; }
        public long TotalRequests { get; set; }
        public int ErrorCount { get; set; }
        public double ThroughputPerMinute { get; set; }
        public IEnumerable<EndpointMetricDto> EndpointMetrics { get; set; } = new List<EndpointMetricDto>();
    }

    public class EndpointMetricDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public double ErrorRate { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}
