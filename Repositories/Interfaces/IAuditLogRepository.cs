using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Repositories.Interfaces
{
    public interface IAuditLogRepository : IBaseRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetByReportAsync(Guid reportId);
        Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
        Task LogActionAsync(AuditAction action, Guid userId, Guid? reportId = null, string? details = null, string? ipAddress = null, string? userAgent = null);
    }
}
