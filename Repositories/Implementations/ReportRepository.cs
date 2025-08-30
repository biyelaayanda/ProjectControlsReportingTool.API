using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Base;
using ProjectControlsReportingTool.API.Repositories.Interfaces;

namespace ProjectControlsReportingTool.API.Repositories.Implementations
{
    // Helper class for stored procedure result
    public class CanAccessResult
    {
        public int CanAccess { get; set; }
    }

    public class ReportRepository : BaseRepository<Report>, IReportRepository
    {
        public ReportRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Report> CreateReportAsync(Report report)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC CreateReport @Id, @Title, @Content, @Description, @Type, @Priority, @DueDate, @CreatedBy, @Department, @ReportNumber",
                    new SqlParameter("@Id", report.Id),
                    new SqlParameter("@Title", report.Title),
                    new SqlParameter("@Content", report.Content),
                    new SqlParameter("@Description", (object?)report.Description ?? DBNull.Value),
                    new SqlParameter("@Type", (object?)report.Type ?? DBNull.Value),
                    new SqlParameter("@Priority", report.Priority),
                    new SqlParameter("@DueDate", (object?)report.DueDate ?? DBNull.Value),
                    new SqlParameter("@CreatedBy", report.CreatedBy),
                    new SqlParameter("@Department", (int)report.Department),
                    new SqlParameter("@ReportNumber", (object?)report.ReportNumber ?? DBNull.Value)
                );

                // Return the created report by querying it back
                return await _context.Reports
                    .FirstAsync(r => r.Id == report.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating report: {ex.Message}", ex);
            }
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

                case UserRole.GM:
                    return await _context.Reports
                        .FromSqlRaw("EXEC GetPendingApprovalsForGM")
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
                case UserRole.GM:
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
            return await _context.Reports
                .Include(r => r.Creator)
                .Include(r => r.RejectedByUser)
                .Include(r => r.Signatures.Where(s => s.IsActive))
                .Include(r => r.Attachments.Where(a => a.IsActive))
                .FirstOrDefaultAsync(r => r.Id == reportId);
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
            if (report == null) 
            {
                return false;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) 
            {
                return false;
            }

            // Use enhanced access logic
            var canAccess = userRole switch
            {
                UserRole.GM => true,
                UserRole.LineManager => await CanLineManagerAccessAsync(userId, reportId, user.Department),
                UserRole.GeneralStaff => report.CreatedBy == userId,
                _ => false
            };
            
            return canAccess;

            /* Temporarily disabled stored procedure approach
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
                    .SqlQueryRaw<CanAccessResult>("EXEC CanUserAccessReport @ReportId, @UserId, @UserRole, @UserDepartment", parameters)
                    .ToListAsync();

                var canAccess = result.FirstOrDefault()?.CanAccess == 1;
                return canAccess;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                // Fallback to basic access logic - Line Managers can view all reports from their department
                var fallbackResult = userRole switch
                {
                    UserRole.GM => true,
                    UserRole.LineManager => report.Department == user.Department, // Allow all statuses for Line Managers
                    UserRole.GeneralStaff => report.CreatedBy == userId,
                    _ => false
                };
                
                return fallbackResult;
            }
            */
        }

        private async Task<bool> CanLineManagerAccessAsync(Guid userId, Guid reportId, Department userDepartment)
        {
            var report = await _dbSet.FindAsync(reportId);
            if (report == null) return false;

            // Line managers can access:
            // 1. Their own reports (reports they created)
            if (report.CreatedBy == userId) 
            {
                return true;
            }

            // 2. Reports from their department
            if (report.Department == userDepartment)
            {
                return true;
            }

            // 3. Reports they have previously reviewed (signature-based)
            var hasSignature = await _context.ReportSignatures
                .AnyAsync(s => s.ReportId == reportId && s.UserId == userId && s.IsActive);
            if (hasSignature)
            {
                return true;
            }

            return false;
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
                        report.GMApprovedDate = DateTime.UtcNow;
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
                    case UserRole.GM:
                        // GMs can see all reports
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

            var parameters = new[]
            {
                new SqlParameter("@ReportId", reportId),
                new SqlParameter("@UserId", approvedBy),
                new SqlParameter("@UserRole", (int)user.Role),
                new SqlParameter("@Comments", (object)comments ?? DBNull.Value)
            };

            var results = await _context.Database
                .SqlQueryRaw<int>("EXEC ApproveReport @ReportId, @UserId, @UserRole, @Comments", parameters)
                .ToListAsync();

            return results.Any() && results[0] > 0;
        }

        public async Task<bool> RejectReportAsync(Guid reportId, Guid rejectedBy, string reason)
        {
            try
            {
                // Get the user to determine their role
                var user = await _context.Users.FindAsync(rejectedBy);
                if (user == null) return false;

                var parameters = new[]
                {
                    new SqlParameter("@ReportId", reportId),
                    new SqlParameter("@UserId", rejectedBy),
                    new SqlParameter("@UserRole", (int)user.Role),
                    new SqlParameter("@Comments", (object)reason ?? DBNull.Value)
                };

                // Use SqlQueryRaw to capture the result set from the stored procedure
                var results = await _context.Database
                    .SqlQueryRaw<int>("EXEC RejectReport @ReportId, @UserId, @UserRole, @Comments", parameters)
                    .ToListAsync();

                var success = results.Any() && results[0] > 0;
                return success;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Implement missing interface methods
        public async Task<IEnumerable<Report>> GetReportsByUserAsync(Guid userId)
        {
            return await GetByCreatorAsync(userId);
        }

        public async Task<IEnumerable<Report>> GetReportsByDepartmentAsync(Department department)
        {
            return await GetByDepartmentAsync(department);
        }

        public async Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status)
        {
            return await GetByStatusAsync(status);
        }

        public async Task<IEnumerable<Report>> GetPendingApprovalsForManagerAsync(Department department)
        {
            var parameters = new[]
            {
                new SqlParameter("@Department", (int)department)
            };
            return await _context.Reports
                .FromSqlRaw("EXEC GetPendingApprovalsForManager @Department", parameters)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetPendingApprovalsForGMAsync()
        {
            return await _context.Reports
                .FromSqlRaw("EXEC GetPendingApprovalsForGM")
                .ToListAsync();
        }

        // Legacy method for backwards compatibility - will be removed later
        public async Task<IEnumerable<Report>> GetPendingApprovalsForExecutiveAsync()
        {
            // Redirect to GM method for backwards compatibility
            return await GetPendingApprovalsForGMAsync();
        }

        public async Task<bool> UpdateReportStatusAsync(Guid reportId, ReportStatus status, Guid updatedBy)
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
                        report.GMApprovedDate = DateTime.UtcNow;
                        report.CompletedDate = DateTime.UtcNow;
                        break;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Report?> GetReportWithDetailsAsync(Guid reportId)
        {
            return await GetWithDetailsAsync(reportId);
        }

        public async Task<bool> CanUserAccessReportAsync(Guid reportId, Guid userId, UserRole userRole, Department userDepartment)
        {
            return await CanUserAccessReportAsync(userId, reportId, userRole);
        }

        public async Task<IEnumerable<Report>> SearchReportsAsync(string searchTerm, Department? department, ReportStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            var parameters = new[]
            {
                new SqlParameter("@SearchTerm", (object)searchTerm ?? DBNull.Value),
                new SqlParameter("@Department", department.HasValue ? (int)department.Value : DBNull.Value),
                new SqlParameter("@Status", status.HasValue ? (int)status.Value : DBNull.Value),
                new SqlParameter("@FromDate", (object)fromDate ?? DBNull.Value),
                new SqlParameter("@ToDate", (object)toDate ?? DBNull.Value),
                new SqlParameter("@Page", 1),
                new SqlParameter("@PageSize", 100)
            };

            var reportIds = await _context.Reports
                .FromSqlRaw("EXEC SearchReports @SearchTerm, @Department, @Status, @FromDate, @ToDate, @Page, @PageSize", parameters)
                .Select(r => r.Id)
                .ToListAsync();

            // Load the full report objects with navigation properties
            return await _dbSet
                .Where(r => reportIds.Contains(r.Id))
                .Include(r => r.Creator)
                .Include(r => r.Signatures.Where(s => s.IsActive))
                .ToListAsync();
        }

        // Override GetAllAsync to include Creator and Signatures navigation properties
        public override async Task<IEnumerable<Report>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Creator)
                .Include(r => r.Signatures.Where(s => s.IsActive))
                .ToListAsync();
        }

        // Override GetByIdAsync to include Creator navigation property
        public override async Task<Report?> GetByIdAsync(Guid id)
        {
            return await GetWithDetailsAsync(id);
        }
    }
}

