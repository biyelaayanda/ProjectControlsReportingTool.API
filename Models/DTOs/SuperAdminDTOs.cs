using System.ComponentModel.DataAnnotations;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    // SuperAdmin User Management DTOs
    public class SuperAdminCreateUserDto
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
        public UserRole Role { get; set; }

        [Required]
        public Department Department { get; set; }

        public string? PhoneNumber { get; set; }

        public string? JobTitle { get; set; }

        [MinLength(6)]
        public string? TemporaryPassword { get; set; } // If not provided, will be generated

        public bool RequirePasswordChange { get; set; } = true;

        public bool SendWelcomeEmail { get; set; } = true;

        [StringLength(500)]
        public string? CreationReason { get; set; }
    }

    public class SuperAdminUpdateUserDto
    {
        [MinLength(2)]
        public string? FirstName { get; set; }

        [MinLength(2)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public UserRole? Role { get; set; }

        public Department? Department { get; set; }

        public string? PhoneNumber { get; set; }

        public string? JobTitle { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(500)]
        public string? UpdateReason { get; set; }
    }

    public class SuperAdminUserDetailDto : UserDto
    {
        public Guid? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Guid? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public int TotalReports { get; set; }
        public DateTime? LastActivity { get; set; }
        public bool RequirePasswordChange { get; set; }
        public int LoginCount { get; set; }
        public double TotalSessionHours { get; set; }
    }

    public class SuperAdminUserFiltersDto
    {
        public string? Search { get; set; }
        public UserRole? Role { get; set; }
        public Department? Department { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? LastLoginAfter { get; set; }
        public DateTime? LastLoginBefore { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedDate";
        public string? SortDirection { get; set; } = "desc";
    }

    public class SuperAdminBulkCreateUsersDto
    {
        [Required]
        public List<SuperAdminCreateUserDto> Users { get; set; } = new List<SuperAdminCreateUserDto>();

        public bool SendWelcomeEmails { get; set; } = true;

        [StringLength(500)]
        public string? BulkCreationReason { get; set; }
    }

    public class SuperAdminBulkUpdateUsersDto
    {
        [Required]
        public List<SuperAdminBulkUserUpdateItemDto> Updates { get; set; } = new List<SuperAdminBulkUserUpdateItemDto>();

        [StringLength(500)]
        public string? BulkUpdateReason { get; set; }
    }

    public class SuperAdminBulkUserUpdateItemDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public SuperAdminUpdateUserDto Data { get; set; } = new SuperAdminUpdateUserDto();
    }

    public class SuperAdminBulkDeleteUsersDto
    {
        [Required]
        public List<Guid> UserIds { get; set; } = new List<Guid>();

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool PermanentDelete { get; set; } = false; // If false, soft delete
    }

    public class SuperAdminBulkOperationResultDto
    {
        public int TotalProcessed { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public List<SuperAdminBulkOperationErrorDto> Errors { get; set; } = new List<SuperAdminBulkOperationErrorDto>();
        public List<SuperAdminUserDetailDto> ProcessedUsers { get; set; } = new List<SuperAdminUserDetailDto>();
        public bool IsSuccessful => Failed == 0;
        public double SuccessRate => TotalProcessed > 0 ? (double)Successful / TotalProcessed * 100 : 0;
    }

    public class SuperAdminBulkOperationErrorDto
    {
        public int RowNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    public class SuperAdminPasswordResetResultDto
    {
        public string TemporaryPassword { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool EmailSent { get; set; }
        public string? EmailError { get; set; }
    }

    public class SuperAdminImportResultDto
    {
        public int TotalRows { get; set; }
        public int ProcessedRows { get; set; }
        public int SuccessfulRows { get; set; }
        public int FailedRows { get; set; }
        public List<SuperAdminImportErrorDto> Errors { get; set; } = new List<SuperAdminImportErrorDto>();
        public List<SuperAdminUserDetailDto>? Preview { get; set; }
        public bool IsSuccessful => FailedRows == 0;
        public double SuccessRate => ProcessedRows > 0 ? (double)SuccessfulRows / ProcessedRows * 100 : 0;
    }

    public class SuperAdminImportErrorDto
    {
        public int RowNumber { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    // Audit Report DTOs - Using existing DTOs where possible
    public class SuperAdminDepartmentReportParamsDto
    {
        [Required]
        public Department DepartmentId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        public bool IncludeMetrics { get; set; } = true;
        public bool IncludeUsers { get; set; } = true;
        public bool IncludeTrends { get; set; } = true;
    }

    public class SuperAdminUserReportParamsDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        public bool IncludeActivity { get; set; } = true;
        public bool IncludePerformance { get; set; } = true;
        public bool IncludeReports { get; set; } = true;
    }

    public class SuperAdminDepartmentAuditReportDto
    {
        public SuperAdminDepartmentInfoDto Department { get; set; } = new SuperAdminDepartmentInfoDto();
        public SuperAdminDateRangeDto DateRange { get; set; } = new SuperAdminDateRangeDto();
        public SuperAdminDepartmentMetricsDto Metrics { get; set; } = new SuperAdminDepartmentMetricsDto();
        public List<SuperAdminDepartmentUserSummaryDto> Users { get; set; } = new List<SuperAdminDepartmentUserSummaryDto>();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; } = string.Empty;
    }

    public class SuperAdminDepartmentInfoDto
    {
        public Department Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int ActiveUserCount { get; set; }
    }

    public class SuperAdminDateRangeDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays => (ToDate - FromDate).Days + 1;
    }

    public class SuperAdminDepartmentMetricsDto
    {
        public int ReportsCreated { get; set; }
        public int ReportsSubmitted { get; set; }
        public int ReportsApproved { get; set; }
        public int ReportsRejected { get; set; }
        public double AverageApprovalTimeHours { get; set; }
        public double RejectionRatePercentage { get; set; }
        public double ProductivityScore { get; set; }
        public double QualityScore { get; set; }
    }

    public class SuperAdminDepartmentUserSummaryDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ReportsCreated { get; set; }
        public double AverageQualityScore { get; set; }
        public DateTime? LastActivity { get; set; }
        public double ProductivityScore { get; set; }
        public bool IsActive { get; set; }
    }

    public class SuperAdminUserAuditReportDto
    {
        public SuperAdminUserInfoDto User { get; set; } = new SuperAdminUserInfoDto();
        public SuperAdminDateRangeDto DateRange { get; set; } = new SuperAdminDateRangeDto();
        public SuperAdminUserActivityDto Activity { get; set; } = new SuperAdminUserActivityDto();
        public List<SuperAdminUserReportSummaryDto> Reports { get; set; } = new List<SuperAdminUserReportSummaryDto>();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; } = string.Empty;
    }

    public class SuperAdminUserInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SuperAdminUserActivityDto
    {
        public int LoginCount { get; set; }
        public double SessionHours { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<SuperAdminFeatureUsageDto> FeatureUsage { get; set; } = new List<SuperAdminFeatureUsageDto>();
        public int DaysActive { get; set; }
        public double AverageSessionDuration { get; set; }
    }

    public class SuperAdminUserReportSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double QualityScore { get; set; }
        public int RevisionCount { get; set; }
    }

    public class SuperAdminFeatureUsageDto
    {
        public string Feature { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public double UsagePercentage { get; set; }
    }

    // System Health and Administration DTOs
    public class SuperAdminSystemHealthReportDto
    {
        public string SystemStatus { get; set; } = string.Empty; // Healthy, Warning, Critical
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public double SystemUptime { get; set; }
        public double AverageResponseTime { get; set; }
        public double ErrorRate { get; set; }
        public List<SuperAdminFeatureHealthDto> Features { get; set; } = new List<SuperAdminFeatureHealthDto>();
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;
        public SuperAdminSystemResourcesDto Resources { get; set; } = new SuperAdminSystemResourcesDto();
    }

    public class SuperAdminFeatureHealthDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Healthy, Warning, Critical
        public double Uptime { get; set; }
        public DateTime? LastError { get; set; }
        public string? LastErrorMessage { get; set; }
        public double ResponseTime { get; set; }
    }

    public class SuperAdminSystemResourcesDto
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public double DatabaseConnections { get; set; }
        public long DatabaseSize { get; set; }
    }

    public class SuperAdminAuditTrailFiltersDto
    {
        public Guid? UserId { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SortBy { get; set; } = "Timestamp";
        public string? SortDirection { get; set; } = "desc";
    }

    public class SuperAdminAuditEntryDto
    {
        public Guid Id { get; set; }
        public Guid AdminUserId { get; set; }
        public string AdminUserName { get; set; } = string.Empty;
        public Guid? TargetUserId { get; set; }
        public string? TargetUserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Changes { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IPAddress { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public Dictionary<string, object>? ChangesSummary { get; set; }
    }
}
