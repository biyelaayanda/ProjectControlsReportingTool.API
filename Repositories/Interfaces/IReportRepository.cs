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
        Task<IEnumerable<Report>> GetPendingApprovalsForGMAsync();
        Task<bool> UpdateReportStatusAsync(Guid reportId, ReportStatus status, Guid updatedBy);
        Task<bool> ApproveReportAsync(Guid reportId, Guid approvedBy, string? comments);
        Task<bool> RejectReportAsync(Guid reportId, Guid rejectedBy, string reason);
        Task<Report?> GetReportWithDetailsAsync(Guid reportId);
        Task<bool> CanUserAccessReportAsync(Guid reportId, Guid userId, UserRole userRole, Department userDepartment);
        Task<IEnumerable<Report>> SearchReportsAsync(string searchTerm, Department? department, ReportStatus? status, DateTime? fromDate, DateTime? toDate);
        
        // Additional methods that are implemented
        Task<IEnumerable<Report>> GetByCreatorAsync(Guid creatorId);
        Task<IEnumerable<Report>> GetByDepartmentAsync(Department department);
        Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status);
        Task<IEnumerable<Report>> GetPendingForUserAsync(Guid userId, UserRole userRole);
        Task<IEnumerable<Report>> GetCompletedByUserAsync(Guid userId, UserRole userRole);
        Task<Report?> GetWithDetailsAsync(Guid reportId);
        Task<string> GenerateReportNumberAsync(Department department);
        Task<bool> CanUserAccessReportAsync(Guid userId, Guid reportId, UserRole userRole);
        Task UpdateStatusAsync(Guid reportId, ReportStatus status);
        Task<IEnumerable<Report>> SearchReportsAsync(string searchTerm, Guid? userId = null, UserRole? userRole = null);
        Task<(IEnumerable<Report> Reports, int TotalCount)> GetPagedReportsAsync(int page, int pageSize, Guid? userId = null, UserRole? userRole = null, ReportStatus? status = null, Department? department = null);
    }
}
