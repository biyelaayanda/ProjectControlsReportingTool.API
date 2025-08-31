using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    public interface IReportService
    {
        Task<ReportDto?> CreateReportAsync(CreateReportDto dto, Guid userId);
        Task<IEnumerable<ReportSummaryDto>> GetReportsAsync(ReportFilterDto filter, Guid userId, UserRole userRole, Department userDepartment);
        Task<ReportDetailDto?> GetReportByIdAsync(Guid reportId, Guid userId, UserRole userRole);
        Task<ServiceResultDto> UpdateReportStatusAsync(Guid reportId, UpdateReportStatusDto dto, Guid userId, UserRole userRole);
        Task<ServiceResultDto> ApproveReportAsync(Guid reportId, ApprovalDto dto, Guid userId, UserRole userRole);
        Task<ServiceResultDto> SubmitReportAsync(Guid reportId, SubmitReportDto dto, Guid userId, UserRole userRole);
        Task<ServiceResultDto> RejectReportAsync(Guid reportId, RejectionDto dto, Guid userId, UserRole userRole);
        Task<ServiceResultDto> DeleteReportAsync(Guid reportId, Guid userId, UserRole userRole);
        Task<IEnumerable<ReportSummaryDto>> GetPendingApprovalsAsync(Guid userId, UserRole userRole, Department userDepartment);
        Task<IEnumerable<ReportSummaryDto>> GetUserReportsAsync(Guid userId);
        Task<IEnumerable<ReportSummaryDto>> GetTeamReportsAsync(Guid managerId, Department department);
        Task<IEnumerable<ReportSummaryDto>> GetGMReportsAsync();
        Task<ReportAttachment?> GetAttachmentAsync(Guid reportId, Guid attachmentId, Guid userId);
        Task<ServiceResultDto> UploadApprovalDocumentsAsync(Guid reportId, IFormFileCollection files, Guid userId, UserRole userRole, string? description = null);
        Task<IEnumerable<ReportAttachmentDto>> GetReportAttachmentsByStageAsync(Guid reportId, ApprovalStage? stage, Guid userId, UserRole userRole);

        // Statistics and Analytics
        Task<ReportStatisticsDto> GetReportStatisticsAsync(StatisticsFilterDto filter, Guid userId, UserRole userRole, Department userDepartment);
        Task<OverallStatsDto> GetOverallStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<DepartmentStatsDto>> GetDepartmentStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<TrendDataDto>> GetTrendAnalysisAsync(string period = "monthly", int periodCount = 12, Department? department = null);
        Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<UserStatsDto> GetUserStatsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);

        // Phase 7.3 Advanced Analytics
        Task<TimeSeriesAnalysisDto> GetTimeSeriesAnalysisAsync(AdvancedAnalyticsFilterDto filter, Guid userId, UserRole userRole);
        Task<PerformanceDashboardDto> GetPerformanceDashboardAsync(AdvancedAnalyticsFilterDto filter, Guid userId, UserRole userRole);
        Task<ComparativeAnalysisDto> GetComparativeAnalysisAsync(AdvancedAnalyticsFilterDto filter, Guid userId, UserRole userRole);
        Task<PredictiveAnalyticsDto> GetPredictiveAnalyticsAsync(AdvancedAnalyticsFilterDto filter, Guid userId, UserRole userRole);
        Task<CustomReportGeneratorDto> GenerateCustomReportAsync(CustomReportGeneratorDto reportConfig, Guid userId, UserRole userRole);
        Task<IEnumerable<CustomReportGeneratorDto>> GetCustomReportTemplatesAsync(Guid userId, UserRole userRole);
        Task<ServiceResultDto> SaveCustomReportTemplateAsync(CustomReportGeneratorDto reportTemplate, Guid userId);
        Task<ServiceResultDto> DeleteCustomReportTemplateAsync(Guid templateId, Guid userId, UserRole userRole);
        Task<SystemPerformanceDto> GetSystemPerformanceAsync();
        Task<IEnumerable<EndpointMetricDto>> GetEndpointMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
