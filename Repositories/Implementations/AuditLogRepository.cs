using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Base;
using ProjectControlsReportingTool.API.Repositories.Interfaces;

namespace ProjectControlsReportingTool.API.Repositories.Implementations
{
    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .Include(a => a.Report)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByReportAsync(Guid reportId)
        {
            return await _dbSet
                .Where(a => a.ReportId == reportId)
                .Include(a => a.User)
                .Include(a => a.Report)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action)
        {
            return await _dbSet
                .Where(a => a.Action == action)
                .Include(a => a.User)
                .Include(a => a.Report)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100)
        {
            return await _dbSet
                .Include(a => a.User)
                .Include(a => a.Report)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task LogActionAsync(AuditAction action, Guid userId, Guid? reportId = null, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                UserId = userId,
                ReportId = reportId,
                Details = details,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            await AddAsync(auditLog);
        }
    }
}
