using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Repositories.Interfaces
{
    public interface IReportRepository : IBaseRepository<Report>
    {
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

    public interface IReportSignatureRepository : IBaseRepository<ReportSignature>
    {
        Task<IEnumerable<ReportSignature>> GetByReportIdAsync(Guid reportId);
        Task<ReportSignature?> GetByReportAndUserAsync(Guid reportId, Guid userId);
        Task<bool> HasUserSignedAsync(Guid reportId, Guid userId, SignatureType signatureType);
        Task<ReportSignature> AddSignatureAsync(Guid reportId, Guid userId, SignatureType signatureType, string? comments = null);
    }

    public interface IReportAttachmentRepository : IBaseRepository<ReportAttachment>
    {
        Task<IEnumerable<ReportAttachment>> GetByReportIdAsync(Guid reportId);
        Task<ReportAttachment?> GetByFileNameAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        Task<ReportAttachment> UploadFileAsync(Guid reportId, Guid uploadedBy, string fileName, string originalFileName, string filePath, long fileSize, string? contentType = null, string? description = null);
        Task DeleteFileAsync(Guid attachmentId);
    }

    public interface IAuditLogRepository : IBaseRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetByReportAsync(Guid reportId);
        Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
        Task LogActionAsync(AuditAction action, Guid userId, Guid? reportId = null, string? details = null, string? ipAddress = null, string? userAgent = null);
    }
}
