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
        Task<IEnumerable<ReportSummaryDto>> GetExecutiveReportsAsync();
        Task<ReportAttachment?> GetAttachmentAsync(Guid reportId, Guid attachmentId, Guid userId);
        Task<ServiceResultDto> UploadApprovalDocumentsAsync(Guid reportId, IFormFileCollection files, Guid userId, UserRole userRole, string? description = null);
        Task<IEnumerable<ReportAttachmentDto>> GetReportAttachmentsByStageAsync(Guid reportId, ApprovalStage? stage, Guid userId, UserRole userRole);
    }
}
