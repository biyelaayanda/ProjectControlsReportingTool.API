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

    // Export DTOs for Phase 7.2
    public class ExportRequestDto
    {
        [Required]
        public ExportFormat Format { get; set; }
        
        public ExportType Type { get; set; } = ExportType.Reports;
        
        public ReportFilterDto? ReportFilter { get; set; }
        
        public StatisticsFilterDto? StatisticsFilter { get; set; }
        
        public IEnumerable<Guid>? SpecificReportIds { get; set; }
        
        [StringLength(200)]
        public string? FileName { get; set; }
        
        public ExportTemplateDto? Template { get; set; }
        
        public bool IncludeAttachments { get; set; } = false;
        
        public bool IncludeSignatures { get; set; } = false;
        
        public bool IncludeAuditTrail { get; set; } = false;
        
        [StringLength(500)]
        public string? CustomHeader { get; set; }
        
        [StringLength(500)]
        public string? CustomFooter { get; set; }
    }

    public class ExportTemplateDto
    {
        public string? LogoUrl { get; set; }
        public string? CompanyName { get; set; } = "Project Controls Reporting Tool";
        public string? ReportTitle { get; set; }
        public bool IncludeCoverPage { get; set; } = true;
        public bool IncludeTableOfContents { get; set; } = false;
        public bool IncludePageNumbers { get; set; } = true;
        public string? PageOrientation { get; set; } = "Portrait"; // Portrait or Landscape
        public string? PageSize { get; set; } = "A4"; // A4, Letter, etc.
        public Dictionary<string, string>? CustomStyles { get; set; }
    }

    public class ExportResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        public ExportFormat Format { get; set; }
        public ExportType Type { get; set; }
        public int RecordCount { get; set; }
        public string? DownloadUrl { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public Guid ExportId { get; set; } = Guid.NewGuid();
    }

    public class BulkExportRequestDto
    {
        [Required]
        public IEnumerable<ExportRequestDto> ExportRequests { get; set; } = new List<ExportRequestDto>();
        
        [StringLength(200)]
        public string? ZipFileName { get; set; }
        
        public bool CreateZipFile { get; set; } = true;
    }

    public class BulkExportResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public IEnumerable<ExportResultDto> Results { get; set; } = new List<ExportResultDto>();
        public string? ZipFileName { get; set; }
        public string? ZipDownloadUrl { get; set; }
        public long TotalSizeBytes { get; set; }
        public int TotalFiles { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        public TimeSpan TotalProcessingTime { get; set; }
    }

    public class ExportHistoryDto
    {
        public Guid ExportId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public ExportFormat Format { get; set; }
        public ExportType Type { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public ExportStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public long? FileSizeBytes { get; set; }
        public int RecordCount { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public TimeSpan? ProcessingTime { get; set; }
        public string? DownloadUrl { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    // Export enums
    public enum ExportFormat
    {
        PDF = 1,
        Excel = 2,
        Word = 3,
        CSV = 4,
        JSON = 5,
        XML = 6
    }

    public enum ExportType
    {
        Reports = 1,
        Statistics = 2,
        Users = 3,
        AuditLogs = 4,
        Templates = 5,
        Custom = 6
    }

    public enum ExportStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Expired = 5
    }

    //Advanced Analytics DTOs
    public class TimeSeriesAnalysisDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Period { get; set; } = string.Empty; // "daily", "weekly", "monthly", "quarterly", "yearly"
        public List<TimeSeriesDataPointDto> DataPoints { get; set; } = new();
        public TimeSeriesMetricsDto Metrics { get; set; } = new();
        public List<TimeSeriesTrendDto> Trends { get; set; } = new();
        public ComparisonDataDto? ComparisonData { get; set; }
    }

    public class TimeSeriesDataPointDto
    {
        public DateTime Timestamp { get; set; }
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public int Count { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class TimeSeriesMetricsDto
    {
        public double Average { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double StandardDeviation { get; set; }
        public double GrowthRate { get; set; }
        public string TrendDirection { get; set; } = string.Empty; // "Increasing", "Decreasing", "Stable"
        public double Volatility { get; set; }
    }

    public class TimeSeriesTrendDto
    {
        public string TrendType { get; set; } = string.Empty; // "Linear", "Exponential", "Seasonal"
        public double Coefficient { get; set; }
        public double RSquared { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? ProjectedDate { get; set; }
        public double? ProjectedValue { get; set; }
    }

    public class ComparisonDataDto
    {
        public DateTime PreviousPeriodStart { get; set; }
        public DateTime PreviousPeriodEnd { get; set; }
        public double PreviousPeriodValue { get; set; }
        public double PercentageChange { get; set; }
        public string ChangeDirection { get; set; } = string.Empty; // "Improved", "Declined", "NoChange"
        public bool IsSignificant { get; set; }
    }

    public class PerformanceDashboardDto
    {
        public OverallPerformanceDto Overall { get; set; } = new();
        public List<DepartmentPerformanceDto> Departments { get; set; } = new();
        public List<UserPerformanceDto> TopPerformers { get; set; } = new();
        public List<AlertDto> Alerts { get; set; } = new();
        public List<KpiDto> KeyPerformanceIndicators { get; set; } = new();
        public WorkflowEfficiencyDto WorkflowEfficiency { get; set; } = new();
        public QualityMetricsDto QualityMetrics { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class OverallPerformanceDto
    {
        public int TotalReports { get; set; }
        public int CompletedReports { get; set; }
        public int PendingReports { get; set; }
        public double CompletionRate { get; set; }
        public double AverageProcessingTime { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public int ActiveUsers { get; set; }
        public double SystemUptime { get; set; }
    }

    public class DepartmentPerformanceDto
    {
        public Department Department { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int ReportCount { get; set; }
        public double CompletionRate { get; set; }
        public double AverageProcessingTime { get; set; }
        public double QualityScore { get; set; }
        public int ActiveUsers { get; set; }
        public string PerformanceGrade { get; set; } = string.Empty; // "A", "B", "C", "D", "F"
        public List<string> Strengths { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
    }

    public class UserPerformanceDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Department Department { get; set; }
        public int ReportsCreated { get; set; }
        public int ReportsCompleted { get; set; }
        public double AverageQualityScore { get; set; }
        public double AverageCompletionTime { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public int Rank { get; set; }
        public string PerformanceLevel { get; set; } = string.Empty; // "Excellent", "Good", "Average", "NeedsImprovement"
    }

    public class AlertDto
    {
        public string AlertType { get; set; } = string.Empty; // "Warning", "Critical", "Information"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "High", "Medium", "Low"
        public DateTime CreatedAt { get; set; }
        public string? ActionRequired { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }

    public class KpiDto
    {
        public string Name { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double TargetValue { get; set; }
        public double PreviousValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "OnTrack", "AtRisk", "BehindTarget"
        public double PercentageOfTarget { get; set; }
        public double ChangeFromPrevious { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
    }

    public class WorkflowEfficiencyDto
    {
        public double AverageDraftTime { get; set; }
        public double AverageReviewTime { get; set; }
        public double AverageApprovalTime { get; set; }
        public double TotalCycleTime { get; set; }
        public double BottleneckScore { get; set; }
        public string MajorBottleneck { get; set; } = string.Empty;
        public List<WorkflowStageDto> StageMetrics { get; set; } = new();
    }

    public class WorkflowStageDto
    {
        public string StageName { get; set; } = string.Empty;
        public double AverageTime { get; set; }
        public int ReportsInStage { get; set; }
        public double EfficiencyScore { get; set; }
        public List<string> ImprovementSuggestions { get; set; } = new();
    }

    public class QualityMetricsDto
    {
        public double OverallQualityScore { get; set; }
        public double RejectionRate { get; set; }
        public double ResubmissionRate { get; set; }
        public double FirstPassSuccessRate { get; set; }
        public List<QualityIssueDto> CommonIssues { get; set; } = new();
        public List<QualityTrendDto> QualityTrends { get; set; } = new();
    }

    public class QualityIssueDto
    {
        public string IssueType { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public double ImpactScore { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
    }

    public class QualityTrendDto
    {
        public DateTime Period { get; set; }
        public double QualityScore { get; set; }
        public double RejectionRate { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
    }

    public class ComparativeAnalysisDto
    {
        public string AnalysisType { get; set; } = string.Empty; // "Department", "User", "TimePeriod"
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<ComparisonEntityDto> Entities { get; set; } = new();
        public ComparisonSummaryDto Summary { get; set; } = new();
        public List<InsightDto> Insights { get; set; } = new();
        public List<RecommendationDto> Recommendations { get; set; } = new();
    }

    public class ComparisonEntityDto
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Dictionary<string, double> Metrics { get; set; } = new();
        public int Rank { get; set; }
        public string PerformanceCategory { get; set; } = string.Empty;
        public double OverallScore { get; set; }
    }

    public class ComparisonSummaryDto
    {
        public ComparisonEntityDto TopPerformer { get; set; } = new();
        public ComparisonEntityDto LowestPerformer { get; set; } = new();
        public double AveragePerformance { get; set; }
        public double PerformanceRange { get; set; }
        public string OverallTrend { get; set; } = string.Empty;
        public int TotalEntitiesCompared { get; set; }
    }

    public class InsightDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Performance", "Quality", "Efficiency", "Trend"
        public string Severity { get; set; } = string.Empty; // "High", "Medium", "Low"
        public double Confidence { get; set; }
        public Dictionary<string, object> SupportingData { get; set; } = new();
    }

    public class RecommendationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // "High", "Medium", "Low"
        public string Impact { get; set; } = string.Empty; // "High", "Medium", "Low"
        public string Effort { get; set; } = string.Empty; // "High", "Medium", "Low"
        public List<string> Actions { get; set; } = new();
        public string? ExpectedOutcome { get; set; }
    }

    public class PredictiveAnalyticsDto
    {
        public string ModelType { get; set; } = string.Empty; // "ReportVolume", "CompletionTime", "QualityScore"
        public DateTime PredictionDate { get; set; }
        public string PredictionPeriod { get; set; } = string.Empty; // "NextWeek", "NextMonth", "NextQuarter"
        public List<PredictionDto> Predictions { get; set; } = new();
        public ModelMetricsDto ModelMetrics { get; set; } = new();
        public List<ScenarioDto> Scenarios { get; set; } = new();
        public List<RiskFactorDto> RiskFactors { get; set; } = new();
    }

    public class PredictionDto
    {
        public DateTime Date { get; set; }
        public double PredictedValue { get; set; }
        public double ConfidenceInterval { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public string Category { get; set; } = string.Empty;
        public Dictionary<string, object> Factors { get; set; } = new();
    }

    public class ModelMetricsDto
    {
        public double Accuracy { get; set; }
        public double MeanAbsoluteError { get; set; }
        public double RSquared { get; set; }
        public DateTime LastTrainedDate { get; set; }
        public int TrainingDataPoints { get; set; }
        public string ModelVersion { get; set; } = string.Empty;
    }

    public class ScenarioDto
    {
        public string ScenarioName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Probability { get; set; }
        public List<PredictionDto> Predictions { get; set; } = new();
        public string Impact { get; set; } = string.Empty;
    }

    public class RiskFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double RiskLevel { get; set; }
        public string Impact { get; set; } = string.Empty;
        public string Likelihood { get; set; } = string.Empty;
        public List<string> MitigationStrategies { get; set; } = new();
    }

    public class CustomReportGeneratorDto
    {
        public Guid ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty; // "Analytics", "Performance", "Comparison", "Prediction"
        public List<ReportParameterDto> Parameters { get; set; } = new();
        public List<ReportSectionDto> Sections { get; set; } = new();
        public ReportFormattingDto Formatting { get; set; } = new();
        public ReportScheduleDto? Schedule { get; set; }
        public List<string> Recipients { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class ReportParameterDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public string ParameterType { get; set; } = string.Empty; // "Date", "Department", "User", "Number", "Boolean"
        public object DefaultValue { get; set; } = new();
        public bool IsRequired { get; set; }
        public string? ValidationRule { get; set; }
        public List<object>? PossibleValues { get; set; }
    }

    public class ReportSectionDto
    {
        public string SectionName { get; set; } = string.Empty;
        public string SectionType { get; set; } = string.Empty; // "Chart", "Table", "Summary", "Text"
        public string DataSource { get; set; } = string.Empty;
        public Dictionary<string, object> Configuration { get; set; } = new();
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public class ReportFormattingDto
    {
        public string Theme { get; set; } = string.Empty;
        public string ColorScheme { get; set; } = string.Empty;
        public string FontFamily { get; set; } = string.Empty;
        public bool IncludeHeader { get; set; } = true;
        public bool IncludeFooter { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public string PageOrientation { get; set; } = "Portrait";
        public Dictionary<string, object> CustomStyles { get; set; } = new();
    }

    public class ReportScheduleDto
    {
        public string Frequency { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly", "Quarterly"
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TimeOfDay { get; set; } = string.Empty;
        public List<int>? DaysOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AdvancedAnalyticsFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Department>? Departments { get; set; }
        public List<Guid>? UserIds { get; set; }
        public List<ReportStatus>? Statuses { get; set; }
        public string? AnalysisType { get; set; }
        public string? GroupBy { get; set; }
        public string? TimeGranularity { get; set; } // "daily", "weekly", "monthly"
        public bool IncludePredictions { get; set; } = false;
        public bool IncludeComparisons { get; set; } = false;
        public int? TopN { get; set; }
        public Dictionary<string, object>? CustomFilters { get; set; }
    }
}
