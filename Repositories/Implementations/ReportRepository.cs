using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Base;
using ProjectControlsReportingTool.API.Repositories.Interfaces;

namespace ProjectControlsReportingTool.API.Repositories.Implementations
{
    public class ReportRepository : BaseRepository<Report>, IReportRepository
    {
        public ReportRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Report> CreateReportAsync(Report report)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", report.Id),
                new SqlParameter("@Title", report.Title),
                new SqlParameter("@Content", report.Content),
                new SqlParameter("@Description", (object)report.Description ?? DBNull.Value),
                new SqlParameter("@CreatedBy", report.CreatedBy),
                new SqlParameter("@Department", (int)report.Department),
                new SqlParameter("@ReportNumber", (object)report.ReportNumber ?? DBNull.Value)
            };

            var reports = await _context.Reports
                .FromSqlRaw("EXEC CreateReport @Id, @Title, @Content, @Description, @CreatedBy, @Department, @ReportNumber", parameters)
                .ToListAsync();

            return reports.First();
        }

        // Implement IReportRepository interface methods
        public async Task<IEnumerable<Report>> GetByCreatorAsync(Guid creatorId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", creatorId),
                new SqlParameter("@Status", DBNull.Value),
                new SqlParameter("@Page", 1),
                new SqlParameter("@PageSize", 100)
            };

            return await _context.Reports
                .FromSqlRaw("EXEC GetReportsByUser @UserId, @Status, @Page, @PageSize", parameters)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetByDepartmentAsync(Department department)
        {
            var parameters = new[]
            {
                new SqlParameter("@Department", (int)department),
                new SqlParameter("@Status", DBNull.Value),
                new SqlParameter("@Page", 1),
                new SqlParameter("@PageSize", 100)
            };

            return await _context.Reports
                .FromSqlRaw("EXEC GetReportsByDepartment @Department, @Status, @Page, @PageSize", parameters)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status)
        {
            return await _dbSet
                .Where(r => r.Status == status)
                .Include(r => r.Creator)
                .OrderByDescending(r => r.LastModifiedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetPendingForUserAsync(Guid userId, UserRole userRole)
        {
            // Get user's department first
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<Report>();

            switch (userRole)
            {
                case UserRole.LineManager:
                    var parameters = new[]
                    {
                        new SqlParameter("@Department", (int)user.Department)
                    };
                    return await _context.Reports
                        .FromSqlRaw("EXEC GetPendingApprovalsForManager @Department", parameters)
                        .ToListAsync();

                case UserRole.Executive:
                    return await _context.Reports
                        .FromSqlRaw("EXEC GetPendingApprovalsForExecutive")
                        .ToListAsync();

                default:
                    return new List<Report>();
            }
        }

        public async Task<IEnumerable<Report>> GetCompletedByUserAsync(Guid userId, UserRole userRole)
        {
            var completedStatus = (int)ReportStatus.Completed;

            switch (userRole)
            {
                case UserRole.Executive:
                    return await _dbSet
                        .Where(r => r.Status == ReportStatus.Completed)
                        .Include(r => r.Creator)
                        .OrderByDescending(r => r.CompletedDate)
                        .ToListAsync();

                case UserRole.LineManager:
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null) return new List<Report>();

                    return await _dbSet
                        .Where(r => r.Status == ReportStatus.Completed && r.Department == user.Department)
                        .Include(r => r.Creator)
                        .OrderByDescending(r => r.CompletedDate)
                        .ToListAsync();

                case UserRole.GeneralStaff:
                default:
                    return await _dbSet
                        .Where(r => r.Status == ReportStatus.Completed && r.CreatedBy == userId)
                        .Include(r => r.Creator)
                        .OrderByDescending(r => r.CompletedDate)
                        .ToListAsync();
            }
        }

        public async Task<Report?> GetWithDetailsAsync(Guid reportId)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReportId", reportId)
            };

            var reports = await _context.Reports
                .FromSqlRaw("EXEC GetReportDetails @ReportId", parameters)
                .Include(r => r.Creator)
                .Include(r => r.RejectedByUser)
                .Include(r => r.Signatures.Where(s => s.IsActive))
                .Include(r => r.Attachments.Where(a => a.IsActive))
                .ToListAsync();

            return reports.FirstOrDefault();
        }

        public async Task<string> GenerateReportNumberAsync(Department department)
        {
            var deptCode = department switch
            {
                Department.ProjectSupport => "PS",
                Department.DocManagement => "DM",
                Department.QS => "QS",
                Department.ContractsManagement => "CM",
                Department.BusinessAssurance => "BA",
                _ => "GN"
            };

            var year = DateTime.UtcNow.Year;
            var nextNumber = await _dbSet
                .Where(r => r.ReportNumber != null && r.ReportNumber.StartsWith($"{deptCode}-{year}"))
                .CountAsync() + 1;

            return $"{deptCode}-{year}-{nextNumber:0000}";
        }

        public async Task<bool> CanUserAccessReportAsync(Guid userId, Guid reportId, UserRole userRole)
        {
            var report = await _dbSet.FindAsync(reportId);
            if (report == null) return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var parameters = new[]
            {
                new SqlParameter("@ReportId", reportId),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@UserRole", (int)userRole),
                new SqlParameter("@UserDepartment", (int)user.Department)
            };

            try
            {
                var result = await _context.Database
                    .SqlQuery<int>($"EXEC CanUserAccessReport {reportId}, {userId}, {(int)userRole}, {(int)user.Department}")
                    .ToListAsync();

                return result.FirstOrDefault() == 1;
            }
            catch
            {
                // Fallback to basic access logic
                return userRole switch
                {
                    UserRole.Executive => true,
                    UserRole.LineManager => report.Department == user.Department,
                    UserRole.GeneralStaff => report.CreatedBy == userId,
                    _ => false
                };
            }
        }

        public async Task UpdateStatusAsync(Guid reportId, ReportStatus status)
        {
            var report = await _dbSet.FindAsync(reportId);
            if (report != null)
            {
                report.Status = status;
                report.LastModifiedDate = DateTime.UtcNow;
                
                // Update specific date fields based on status
                switch (status)
                {
                    case ReportStatus.Submitted:
                        report.SubmittedDate = DateTime.UtcNow;
                        break;
                    case ReportStatus.ManagerApproved:
                        report.ManagerApprovedDate = DateTime.UtcNow;
                        break;
                    case ReportStatus.Completed:
                        report.ExecutiveApprovedDate = DateTime.UtcNow;
                        report.CompletedDate = DateTime.UtcNow;
                        break;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Report>> SearchReportsAsync(string searchTerm, Guid? userId = null, UserRole? userRole = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@SearchTerm", (object)searchTerm ?? DBNull.Value),
                new SqlParameter("@Department", DBNull.Value),
                new SqlParameter("@Status", DBNull.Value),
                new SqlParameter("@FromDate", DBNull.Value),
                new SqlParameter("@ToDate", DBNull.Value),
                new SqlParameter("@Page", 1),
                new SqlParameter("@PageSize", 100)
            };

            return await _context.Reports
                .FromSqlRaw("EXEC SearchReports @SearchTerm, @Department, @Status, @FromDate, @ToDate, @Page, @PageSize", parameters)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Report> Reports, int TotalCount)> GetPagedReportsAsync(int page, int pageSize, Guid? userId = null, UserRole? userRole = null, ReportStatus? status = null, Department? department = null)
        {
            var query = _dbSet.Include(r => r.Creator).AsQueryable();

            // Apply filters based on user role and parameters
            if (userId.HasValue && userRole.HasValue)
            {
                switch (userRole.Value)
                {
                    case UserRole.GeneralStaff:
                        query = query.Where(r => r.CreatedBy == userId.Value);
                        break;
                    case UserRole.LineManager:
                        var user = await _context.Users.FindAsync(userId.Value);
                        if (user != null)
                            query = query.Where(r => r.Department == user.Department);
                        break;
                    case UserRole.Executive:
                        // Executives can see all reports
                        break;
                }
            }

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (department.HasValue)
                query = query.Where(r => r.Department == department.Value);

            var totalCount = await query.CountAsync();
            var reports = await query
                .OrderByDescending(r => r.LastModifiedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reports, totalCount);
        }

        // Additional helper methods for the service layer
        public async Task<bool> ApproveReportAsync(Guid reportId, Guid approvedBy, string? comments)
        {
            var user = await _context.Users.FindAsync(approvedBy);
            if (user == null) return false;

            var signatureType = user.Role == UserRole.LineManager ? 1 : 2; // 1 = Manager, 2 = Executive

            var parameters = new[]
            {
                new SqlParameter("@ReportId", reportId),
                new SqlParameter("@ApprovedBy", approvedBy),
                new SqlParameter("@Comments", (object)comments ?? DBNull.Value),
                new SqlParameter("@SignatureType", signatureType)
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC ApproveReport @ReportId, @ApprovedBy, @Comments, @SignatureType", parameters);

            return result > 0;
        }

        public async Task<bool> RejectReportAsync(Guid reportId, Guid rejectedBy, string reason)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReportId", reportId),
                new SqlParameter("@RejectedBy", rejectedBy),
                new SqlParameter("@Reason", reason)
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC RejectReport @ReportId, @RejectedBy, @Reason", parameters);

            return result > 0;
        }
    }
}
