using System.ComponentModel.DataAnnotations;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    #region Compliance Report DTOs

    /// <summary>
    /// DTO for compliance report data
    /// </summary>
    public class ComplianceReportDto
    {
        public Guid Id { get; set; }
        public Guid ReportId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ComplianceReportType ReportType { get; set; }
        public string ReportTypeName { get; set; } = string.Empty;
        public ComplianceStandard Standard { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        public DateTime CoveragePeriodStart { get; set; }
        public DateTime CoveragePeriodEnd { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public string GeneratedByRole { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<ComplianceViolationDto> Violations { get; set; } = new();
        public List<ComplianceMetricDto> Metrics { get; set; } = new();
        public List<AuditSummaryDto> AuditSummaries { get; set; } = new();
        public List<SecurityMetricsDto> SecurityMetrics { get; set; } = new();
        public List<DataCategoryRetentionDto> DataRetention { get; set; } = new();
        public string? ExecutiveSummary { get; set; }
        public string? Recommendations { get; set; }
        public bool IsExported { get; set; }
        public DateTime? ExportedDate { get; set; }
        public RegulatoryFormat? ExportFormat { get; set; }

        [Range(0, 100)]
        public decimal ComplianceScore { get; set; }
        
        public string ComplianceLevel
        {
            get
            {
                return ComplianceScore >= 90 ? "Excellent" : 
                       ComplianceScore >= 80 ? "Good" : 
                       ComplianceScore >= 70 ? "Acceptable" : 
                       ComplianceScore >= 60 ? "Needs Improvement" : "Critical";
            }
        }
    }

    #endregion

    #region Audit Trail DTOs

    /// <summary>
    /// DTO for audit trail entries
    /// </summary>
    public class AuditTrailDto
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SessionId { get; set; }
        public string? AdditionalData { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public string RiskLevelName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for audit trail search filters
    /// </summary>
    public class AuditTrailFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public AuditAction? Action { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public bool? Success { get; set; }
        public RiskLevel? RiskLevel { get; set; }
        public string? IpAddress { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SortBy { get; set; } = "Timestamp";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// DTO for audit trail summary statistics
    /// </summary>
    public class AuditSummaryDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalEvents { get; set; }
        public int SuccessfulEvents { get; set; }
        public int FailedEvents { get; set; }
        public int UniqueUsers { get; set; }
        public int HighRiskEvents { get; set; }
        public int MediumRiskEvents { get; set; }
        public int LowRiskEvents { get; set; }
        public Dictionary<string, int> ActionBreakdown { get; set; } = new();
        public Dictionary<string, int> EntityTypeBreakdown { get; set; } = new();
        public List<TopUserActivity> TopUsers { get; set; } = new();
        public List<SecurityEventDto> SecurityEvents { get; set; } = new();
    }

    /// <summary>
    /// DTO for top user activity
    /// </summary>
    public class TopUserActivity
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime LastActivity { get; set; }
        public RiskLevel MaxRiskLevel { get; set; }
    }

    #endregion

    #region Security & Compliance DTOs

    /// <summary>
    /// DTO for security events and violations
    /// </summary>
    public class SecurityEventDto
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public SecurityEventType EventType { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RiskLevel Severity { get; set; }
        public string SeverityName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool Resolved { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedBy { get; set; }
        public string? Resolution { get; set; }
        public string? AdditionalData { get; set; }
    }

    /// <summary>
    /// DTO for compliance violations
    /// </summary>
    public class ComplianceViolationDto
    {
        public Guid Id { get; set; }
        public DateTime DetectedDate { get; set; }
        public ComplianceStandard Standard { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public string RuleCode { get; set; } = string.Empty;
        public string RuleDescription { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty;
        public RiskLevel Severity { get; set; }
        public string SeverityName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? RecommendedAction { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedBy { get; set; }
        public string? Resolution { get; set; }
    }

    /// <summary>
    /// DTO for compliance metrics and KPIs
    /// </summary>
    public class ComplianceMetricDto
    {
        public string MetricName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? Target { get; set; }
        public decimal? Threshold { get; set; }
        public ComplianceStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime MeasuredDate { get; set; }
        public string? Description { get; set; }
        public List<MetricTrendDto> Trend { get; set; } = new();
    }

    /// <summary>
    /// DTO for metric trend data
    /// </summary>
    public class MetricTrendDto
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string? Note { get; set; }
    }

    /// <summary>
    /// DTO for security metrics
    /// </summary>
    public class SecurityMetricsDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalSecurityEvents { get; set; }
        public int CriticalEvents { get; set; }
        public int HighRiskEvents { get; set; }
        public int MediumRiskEvents { get; set; }
        public int LowRiskEvents { get; set; }
        public int ResolvedEvents { get; set; }
        public int UnresolvedEvents { get; set; }
        public decimal AverageResolutionTimeHours { get; set; }
        public int UniqueThreats { get; set; }
        public int BlockedAttempts { get; set; }
        public int SuccessfulBreaches { get; set; }
        public List<ThreatSourceDto> ThreatSources { get; set; } = new();
    }

    /// <summary>
    /// DTO for threat source analysis
    /// </summary>
    public class ThreatSourceDto
    {
        public string Source { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public RiskLevel MaxSeverity { get; set; }
        public string Country { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
    }

    #endregion

    #region Data Retention DTOs

    /// <summary>
    /// DTO for data retention policies
    /// </summary>
    public class DataRetentionPolicyDto
    {
        public Guid Id { get; set; }
        public string DataCategory { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public int RetentionDays { get; set; }
        public bool AutoDelete { get; set; }
        public string? LegalBasis { get; set; }
        public ComplianceStandard? ApplicableStandard { get; set; }
        public string? StandardName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModified { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for data category retention status
    /// </summary>
    public class DataCategoryRetentionDto
    {
        public string Category { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int RecordsNearExpiry { get; set; }
        public int ExpiredRecords { get; set; }
        public int RetentionDays { get; set; }
        public DateTime? OldestRecord { get; set; }
        public DateTime? NextExpiry { get; set; }
        public long StorageSizeBytes { get; set; }
        public ComplianceStatus ComplianceStatus { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    #endregion

    #region Analysis & Reporting DTOs

    /// <summary>
    /// DTO for compliance analysis results
    /// </summary>
    public class ComplianceAnalysisDto
    {
        public Guid AnalysisId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public ComplianceStandard Standard { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public decimal OverallScore { get; set; }
        public ComplianceStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public List<ComplianceMetricDto> Metrics { get; set; } = new();
        public List<ComplianceViolationDto> Violations { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// DTO for data retention status
    /// </summary>
    public class DataRetentionStatusDto
    {
        public string DataCategory { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int RetentionDays { get; set; }
        public int RecordsNearExpiry { get; set; }
        public int ExpiredRecords { get; set; }
        public DateTime? OldestRecord { get; set; }
        public DateTime? NextExpiry { get; set; }
        public long StorageSizeBytes { get; set; }
        public string StorageSize { get; set; } = string.Empty;
        public ComplianceStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public List<ExpiringRecordDto> ExpiringRecords { get; set; } = new();
    }

    /// <summary>
    /// DTO for expiring records
    /// </summary>
    public class ExpiringRecordDto
    {
        public string RecordId { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for archival operation results
    /// </summary>
    public class ArchivalResultDto
    {
        public Guid OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string DataCategory { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int ArchivedRecords { get; set; }
        public int FailedRecords { get; set; }
        public long BytesArchived { get; set; }
        public string ArchiveSize { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public string ArchivePath { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    /// <summary>
    /// DTO for security compliance reports
    /// </summary>
    public class SecurityComplianceReportDto
    {
        public Guid ReportId { get; set; }
        public DateTime GeneratedDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public SecurityMetricsDto SecurityMetrics { get; set; } = new();
        public List<SecurityEventDto> CriticalEvents { get; set; } = new();
        public List<ThreatSourceDto> ThreatAnalysis { get; set; } = new();
        public List<ComplianceViolationDto> SecurityViolations { get; set; } = new();
        public Dictionary<string, int> EventTypeSummary { get; set; } = new();
        public string RiskAssessment { get; set; } = string.Empty;
        public List<string> SecurityRecommendations { get; set; } = new();
    }

    /// <summary>
    /// DTO for scheduled reports
    /// </summary>
    public class ScheduledReportDto
    {
        public Guid ScheduleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ComplianceReportType ReportType { get; set; }
        public ComplianceStandard Standard { get; set; }
        public string CronExpression { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new();
        public RegulatoryFormat OutputFormat { get; set; }
        public ComplianceReportScheduleDto Schedule { get; set; } = new();
    }

    /// <summary>
    /// DTO for compliance report schedule configuration
    /// </summary>
    public class ComplianceReportScheduleDto
    {
        public string Frequency { get; set; } = string.Empty; // Daily, Weekly, Monthly, Quarterly
        public int Interval { get; set; } = 1;
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
        public int DayOfMonth { get; set; }
        public TimeSpan PreferredTime { get; set; }
        public string TimeZone { get; set; } = "UTC";
        public bool EnableNotifications { get; set; }
        public bool AutoExport { get; set; }
    }

    #endregion

    #region Request/Response DTOs

    /// <summary>
    /// DTO for compliance report generation request
    /// </summary>
    public class ComplianceReportRequestDto
    {
        [Required]
        public ComplianceReportType ReportType { get; set; }
        
        [Required]
        public ComplianceStandard Standard { get; set; }
        
        [Required]
        public DateTime PeriodStart { get; set; }
        
        [Required]
        public DateTime PeriodEnd { get; set; }
        
        public string? Title { get; set; }
        public bool IncludeAuditTrail { get; set; } = true;
        public bool IncludeSecurityMetrics { get; set; } = true;
        public bool IncludeDataRetention { get; set; } = true;
        public bool IncludeViolations { get; set; } = true;
        public RegulatoryFormat? ExportFormat { get; set; }
        public List<string>? SpecificDataCategories { get; set; }
        public RiskLevel? MinimumSeverity { get; set; }
    }

    /// <summary>
    /// DTO for export file information
    /// </summary>
    public class ExportFileDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO for compliance status information
    /// </summary>
    public class ComplianceStatusDto
    {
        public ComplianceStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public decimal CompliancePercentage { get; set; }
        public DateTime LastAssessed { get; set; }
        public List<string> IssuesFound { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// DTO for compliance control information
    /// </summary>
    public class ComplianceControlDto
    {
        public string ControlId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ComplianceStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public RiskLevel RiskLevel { get; set; }
        public DateTime LastTested { get; set; }
        public string? TestResults { get; set; }
        public bool IsCompliant { get; set; }
    }

    /// <summary>
    /// DTO for compliance gap analysis
    /// </summary>
    public class ComplianceGapDto
    {
        public string GapId { get; set; } = string.Empty;
        public string AreaName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RiskLevel Impact { get; set; }
        public string ImpactDescription { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
        public DateTime IdentifiedDate { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    #endregion
}
