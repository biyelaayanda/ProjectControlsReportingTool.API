using System.ComponentModel.DataAnnotations;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    /// <summary>
    /// Result DTO for GetReportStatistics stored procedure
    /// Contains comprehensive report statistics with security validation
    /// </summary>
    public class ReportStatisticsResultDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Total reports must be non-negative")]
        public int TotalReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Draft reports must be non-negative")]
        public int DraftReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Submitted reports must be non-negative")]
        public int SubmittedReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "In-review reports must be non-negative")]
        public int InReviewReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Approved reports must be non-negative")]
        public int ApprovedReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Completed reports must be non-negative")]
        public int CompletedReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Rejected reports must be non-negative")]
        public int RejectedReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "User reports must be non-negative")]
        public int MyReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Pending approvals must be non-negative")]
        public int PendingMyApproval { get; set; }

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// Result DTO for GetTrendAnalysis stored procedure
    /// Contains time-series trend data with validation
    /// </summary>
    public class TrendAnalysisResultDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Period must be 50 characters or less")]
        public string Period { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Created count must be non-negative")]
        public int CreatedCount { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Completed count must be non-negative")]
        public int CompletedCount { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Rejected count must be non-negative")]
        public int RejectedCount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Average completion time must be non-negative")]
        public decimal? AvgCompletionTimeHours { get; set; }

        [Range(0, 100, ErrorMessage = "Completion rate must be between 0 and 100")]
        public decimal CompletionRate { get; set; }
    }

    /// <summary>
    /// Result DTO for GetPerformanceMetrics stored procedure
    /// Contains performance metrics with validation
    /// </summary>
    public class PerformanceMetricsResultDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Average creation to submission time must be non-negative")]
        public decimal? AvgCreationToSubmissionHours { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Average approval time must be non-negative")]
        public decimal? AvgApprovalTimeHours { get; set; }

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// Result DTO for GetDepartmentStatistics stored procedure
    /// Contains department-specific statistics with validation
    /// </summary>
    public class DepartmentStatisticsResultDto
    {
        [Required]
        [Range(1, 10, ErrorMessage = "Department ID must be between 1 and 10")]
        public int Department { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Total reports must be non-negative")]
        public int TotalReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Draft reports must be non-negative")]
        public int DraftReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Submitted reports must be non-negative")]
        public int SubmittedReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "In-review reports must be non-negative")]
        public int InReviewReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Completed reports must be non-negative")]
        public int CompletedReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Rejected reports must be non-negative")]
        public int RejectedReports { get; set; }

        [Range(0, 100, ErrorMessage = "Completion rate must be between 0 and 100")]
        public decimal CompletionRate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Average total time must be non-negative")]
        public decimal? AvgTotalTimeHours { get; set; }
    }

    /// <summary>
    /// Result DTO for GetUserStatistics stored procedure
    /// Contains user-specific statistics with validation
    /// </summary>
    public class UserStatisticsResultDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "User name must be 200 characters or less")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email must be 100 characters or less")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(1, 10, ErrorMessage = "Department ID must be between 1 and 10")]
        public int Department { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Role ID must be between 1 and 10")]
        public int Role { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Total reports must be non-negative")]
        public int TotalReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Draft reports must be non-negative")]
        public int DraftReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Submitted reports must be non-negative")]
        public int SubmittedReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "In-review reports must be non-negative")]
        public int InReviewReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Completed reports must be non-negative")]
        public int CompletedReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Rejected reports must be non-negative")]
        public int RejectedReports { get; set; }

        [Range(0, 100, ErrorMessage = "Completion rate must be between 0 and 100")]
        public decimal CompletionRate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Average completion time must be non-negative")]
        public decimal? AvgCompletionTimeHours { get; set; }

        public DateTime? FirstReportDate { get; set; }

        public DateTime? LastActivityDate { get; set; }

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// Result DTO for GetSystemPerformanceMetrics stored procedure
    /// Contains system-wide performance metrics (GM only)
    /// </summary>
    public class SystemPerformanceResultDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Total reports must be non-negative")]
        public int TotalReports { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Active users must be non-negative")]
        public int ActiveUsers { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Total attachments must be non-negative")]
        public int TotalAttachments { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Total audit entries must be non-negative")]
        public int TotalAuditEntries { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Reports last 30 days must be non-negative")]
        public int ReportsLast30Days { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Reports last 7 days must be non-negative")]
        public int ReportsLast7Days { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Reports last 24 hours must be non-negative")]
        public int ReportsLast24Hours { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Active users last 30 days must be non-negative")]
        public int ActiveUsersLast30Days { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Logins last 30 days must be non-negative")]
        public int LoginsLast30Days { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Average approval time must be non-negative")]
        public decimal? AvgApprovalTimeHours { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Errors last 30 days must be non-negative")]
        public int ErrorsLast30Days { get; set; }

        [Required]
        [Range(0, long.MaxValue, ErrorMessage = "Total storage bytes must be non-negative")]
        public long TotalStorageBytes { get; set; }

        [Required]
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Result DTO for GetAdvancedAnalyticsData stored procedure
    /// Contains flexible analytics data for advanced analysis
    /// </summary>
    public class AdvancedAnalyticsResultDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Period must be 50 characters or less")]
        public string Period { get; set; } = string.Empty;

        [Required]
        [Range(1, 10, ErrorMessage = "Department ID must be between 1 and 10")]
        public int Department { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Reports created must be non-negative")]
        public int ReportsCreated { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Reports completed must be non-negative")]
        public int ReportsCompleted { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Reports rejected must be non-negative")]
        public int ReportsRejected { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Average approval time must be non-negative")]
        public decimal? AvgApprovalTimeHours { get; set; }
    }
}
