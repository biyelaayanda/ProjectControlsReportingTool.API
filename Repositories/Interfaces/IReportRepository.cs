using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Repositories.Interfaces
{
    public interface IReportRepository : IBaseRepository<Report>
    {
        Task<Report> CreateReportAsync(Report report);
        Task<IEnumerable<Report>> GetReportsByUserAsync(Guid userId);
        Task<IEnumerable<Report>> GetReportsByDepartmentAsync(Department department);
        Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status);
        Task<IEnumerable<Report>> GetPendingApprovalsForManagerAsync(Department department);
        Task<IEnumerable<Report>> GetPendingApprovalsForExecutiveAsync();
        Task<bool> UpdateReportStatusAsync(Guid reportId, ReportStatus status, Guid updatedBy);
        Task<bool> ApproveReportAsync(Guid reportId, Guid approvedBy, string? comments);
        Task<bool> RejectReportAsync(Guid reportId, Guid rejectedBy, string reason);
        Task<Report?> GetReportWithDetailsAsync(Guid reportId);
        Task<bool> CanUserAccessReportAsync(Guid reportId, Guid userId, UserRole userRole, Department userDepartment);
        Task<IEnumerable<Report>> SearchReportsAsync(string searchTerm, Department? department, ReportStatus? status, DateTime? fromDate, DateTime? toDate);
    }
}
