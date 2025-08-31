using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    public interface IExportService
    {
        // Core Export Methods
        Task<ExportResultDto> ExportReportsAsync(ExportRequestDto request, Guid userId, UserRole userRole, Department userDepartment);
        Task<ExportResultDto> ExportStatisticsAsync(ExportRequestDto request, Guid userId, UserRole userRole, Department userDepartment);
        Task<BulkExportResultDto> BulkExportAsync(BulkExportRequestDto request, Guid userId, UserRole userRole, Department userDepartment);

        // Format-Specific Export Methods
        Task<byte[]> ExportReportsToPdfAsync(IEnumerable<ReportDetailDto> reports, ExportTemplateDto? template = null);
        Task<byte[]> ExportReportsToExcelAsync(IEnumerable<ReportDetailDto> reports, ExportTemplateDto? template = null);
        Task<byte[]> ExportReportsToWordAsync(IEnumerable<ReportDetailDto> reports, ExportTemplateDto? template = null);
        Task<byte[]> ExportReportsToCsvAsync(IEnumerable<ReportDetailDto> reports);

        // Statistics Export Methods
        Task<byte[]> ExportStatisticsToPdfAsync(ReportStatisticsDto statistics, ExportTemplateDto? template = null);
        Task<byte[]> ExportStatisticsToExcelAsync(ReportStatisticsDto statistics, ExportTemplateDto? template = null);

        // Template Management
        Task<IEnumerable<ExportTemplateDto>> GetExportTemplatesAsync();
        Task<ExportTemplateDto?> GetExportTemplateAsync(string templateName);
        Task<ExportTemplateDto> CreateExportTemplateAsync(ExportTemplateDto template, Guid userId);
        Task<bool> DeleteExportTemplateAsync(string templateName, Guid userId);

        // Export History and Management
        Task<IEnumerable<ExportHistoryDto>> GetExportHistoryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ExportResultDto?> GetExportResultAsync(Guid exportId, Guid userId);
        Task<byte[]?> DownloadExportAsync(Guid exportId, Guid userId);
        Task<bool> DeleteExportAsync(Guid exportId, Guid userId);

        // Utility Methods
        Task<bool> ValidateExportRequestAsync(ExportRequestDto request, Guid userId, UserRole userRole);
        Task<string> GetContentTypeForFormat(ExportFormat format);
        Task<string> GetFileExtensionForFormat(ExportFormat format);
    }
}
