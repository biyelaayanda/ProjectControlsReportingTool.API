using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using AutoMapper;

namespace ProjectControlsReportingTool.API.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IReportRepository reportRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReportDto?> CreateReportAsync(CreateReportDto dto, Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return null;
                }

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Content = dto.Content,
                    Description = dto.Description,
                    Type = dto.Type,
                    Priority = dto.Priority,
                    DueDate = dto.DueDate,
                    CreatedBy = userId,
                    Department = dto.Department, // Use department from DTO instead of user department
                    Status = ReportStatus.Draft,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                // Generate report number
                report.ReportNumber = await _reportRepository.GenerateReportNumberAsync(user.Department);

                var createdReport = await _reportRepository.CreateReportAsync(report);

                // Log the action
                await _auditLogRepository.LogActionAsync(AuditAction.Created, userId, createdReport.Id, 
                    $"Report created: {createdReport.Title}");

                return _mapper.Map<ReportDto>(createdReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report for user {UserId}", userId);
                return null;
            }
        }

        public async Task<IEnumerable<ReportSummaryDto>> GetReportsAsync(ReportFilterDto filter, Guid userId, UserRole userRole, Department userDepartment)
        {
            try
            {
                IEnumerable<Report> reports;

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    reports = await _reportRepository.SearchReportsAsync(filter.SearchTerm, filter.Department, filter.Status, filter.FromDate, filter.ToDate);
                }
                else
                {
                    reports = await _reportRepository.GetAllAsync();
                }

                // Apply role-based filtering for workflow visibility
                reports = ApplyRoleBasedFiltering(reports, userId, userRole, userDepartment);

                // Apply additional filters
                if (filter.Status.HasValue)
                    reports = reports.Where(r => r.Status == filter.Status.Value);

                if (filter.Department.HasValue && userRole == UserRole.Executive)
                    reports = reports.Where(r => r.Department == filter.Department.Value);

                if (filter.FromDate.HasValue)
                    reports = reports.Where(r => r.CreatedDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    reports = reports.Where(r => r.CreatedDate <= filter.ToDate.Value);

                return _mapper.Map<IEnumerable<ReportSummaryDto>>(reports.OrderByDescending(r => r.LastModifiedDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports for user {UserId}", userId);
                return new List<ReportSummaryDto>();
            }
        }

        public async Task<ReportDetailDto?> GetReportByIdAsync(Guid reportId, Guid userId, UserRole userRole)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return null;

                var canAccess = await _reportRepository.CanUserAccessReportAsync(reportId, userId, userRole, user.Department);
                if (!canAccess) return null;

                var report = await _reportRepository.GetReportWithDetailsAsync(reportId);
                if (report == null) return null;

                return _mapper.Map<ReportDetailDto>(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report {ReportId} for user {UserId}", reportId, userId);
                return null;
            }
        }

        public async Task<ServiceResultDto> UpdateReportStatusAsync(Guid reportId, UpdateReportStatusDto dto, Guid userId, UserRole userRole)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResultDto.ErrorResult("User not found");

                var canAccess = await _reportRepository.CanUserAccessReportAsync(reportId, userId, userRole, user.Department);
                if (!canAccess)
                    return ServiceResultDto.ErrorResult("Access denied");

                var success = await _reportRepository.UpdateReportStatusAsync(reportId, dto.Status, userId);
                if (success)
                {
                    await _auditLogRepository.LogActionAsync(AuditAction.Updated, userId, reportId, 
                        $"Status changed to {dto.Status}");
                    return ServiceResultDto.SuccessResult();
                }

                return ServiceResultDto.ErrorResult("Failed to update status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report status for report {ReportId}", reportId);
                return ServiceResultDto.ErrorResult("An error occurred");
            }
        }

        public async Task<ServiceResultDto> ApproveReportAsync(Guid reportId, ApprovalDto dto, Guid userId, UserRole userRole)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResultDto.ErrorResult("User not found");

                if (userRole != UserRole.LineManager && userRole != UserRole.Executive)
                    return ServiceResultDto.ErrorResult("Insufficient permissions");

                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                    return ServiceResultDto.ErrorResult("Report not found");

                // Workflow validation
                if (userRole == UserRole.LineManager)
                {
                    // Line Manager can only approve submitted reports from their department
                    if (report.Status != ReportStatus.Submitted)
                        return ServiceResultDto.ErrorResult("Report is not in submitted status");
                    
                    if (report.Department != user.Department)
                        return ServiceResultDto.ErrorResult("You can only approve reports from your department");

                    // Update to ManagerApproved and forward to Executive
                    var success = await _reportRepository.UpdateReportStatusAsync(reportId, ReportStatus.ManagerApproved, userId);
                    if (success)
                    {
                        await _auditLogRepository.LogActionAsync(AuditAction.Approved, userId, reportId, 
                            $"Report approved by Line Manager: {dto.Comments}");
                        return ServiceResultDto.SuccessResult("Report approved and forwarded to Executive");
                    }
                }
                else if (userRole == UserRole.Executive)
                {
                    // Executive can only approve manager-approved reports
                    if (report.Status != ReportStatus.ManagerApproved)
                        return ServiceResultDto.ErrorResult("Report must be approved by Line Manager first");

                    // Final approval - mark as completed
                    var success = await _reportRepository.UpdateReportStatusAsync(reportId, ReportStatus.Completed, userId);
                    if (success)
                    {
                        await _auditLogRepository.LogActionAsync(AuditAction.Approved, userId, reportId, 
                            $"Report given final approval by Executive: {dto.Comments}");
                        return ServiceResultDto.SuccessResult("Report completed - final approval given");
                    }
                }

                return ServiceResultDto.ErrorResult("Failed to approve report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving report {ReportId}", reportId);
                return ServiceResultDto.ErrorResult("An error occurred");
            }
        }

        public async Task<ServiceResultDto> RejectReportAsync(Guid reportId, RejectionDto dto, Guid userId, UserRole userRole)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResultDto.ErrorResult("User not found");

                if (userRole != UserRole.LineManager && userRole != UserRole.Executive)
                    return ServiceResultDto.ErrorResult("Insufficient permissions");

                var success = await _reportRepository.RejectReportAsync(reportId, userId, dto.Reason);
                if (success)
                {
                    await _auditLogRepository.LogActionAsync(AuditAction.Rejected, userId, reportId, 
                        $"Report rejected by {userRole}: {dto.Reason}");
                    return ServiceResultDto.SuccessResult();
                }

                return ServiceResultDto.ErrorResult("Failed to reject report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting report {ReportId}", reportId);
                return ServiceResultDto.ErrorResult("An error occurred");
            }
        }

        public async Task<ServiceResultDto> DeleteReportAsync(Guid reportId, Guid userId, UserRole userRole)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResultDto.ErrorResult("User not found");

                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                    return ServiceResultDto.ErrorResult("Report not found");

                // Only allow deletion if user is the creator and report is in draft status, or user is executive
                if (userRole != UserRole.Executive && (report.CreatedBy != userId || report.Status != ReportStatus.Draft))
                    return ServiceResultDto.ErrorResult("Cannot delete this report");

                await _reportRepository.DeleteAsync(reportId);
                await _auditLogRepository.LogActionAsync(AuditAction.Deleted, userId, reportId, "Report deleted");

                return ServiceResultDto.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", reportId);
                return ServiceResultDto.ErrorResult("An error occurred");
            }
        }

        public async Task<IEnumerable<ReportSummaryDto>> GetPendingApprovalsAsync(Guid userId, UserRole userRole, Department userDepartment)
        {
            try
            {
                IEnumerable<Report> reports = userRole switch
                {
                    UserRole.LineManager => await _reportRepository.GetPendingApprovalsForManagerAsync(userDepartment),
                    UserRole.Executive => await _reportRepository.GetPendingApprovalsForExecutiveAsync(),
                    _ => new List<Report>()
                };

                return _mapper.Map<IEnumerable<ReportSummaryDto>>(reports.OrderByDescending(r => r.SubmittedDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals for user {UserId}", userId);
                return new List<ReportSummaryDto>();
            }
        }

        public async Task<IEnumerable<ReportSummaryDto>> GetUserReportsAsync(Guid userId)
        {
            try
            {
                var reports = await _reportRepository.GetReportsByUserAsync(userId);
                return _mapper.Map<IEnumerable<ReportSummaryDto>>(reports.OrderByDescending(r => r.LastModifiedDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports for user {UserId}", userId);
                return new List<ReportSummaryDto>();
            }
        }

        public async Task<IEnumerable<ReportSummaryDto>> GetTeamReportsAsync(Guid managerId, Department department)
        {
            try
            {
                var reports = await _reportRepository.GetReportsByDepartmentAsync(department);
                return _mapper.Map<IEnumerable<ReportSummaryDto>>(reports.OrderByDescending(r => r.LastModifiedDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team reports for manager {ManagerId}", managerId);
                return new List<ReportSummaryDto>();
            }
        }

        public async Task<IEnumerable<ReportSummaryDto>> GetExecutiveReportsAsync()
        {
            try
            {
                var reports = await _reportRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<ReportSummaryDto>>(reports.OrderByDescending(r => r.LastModifiedDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executive reports");
                return new List<ReportSummaryDto>();
            }
        }

        private IEnumerable<Report> ApplyRoleBasedFiltering(IEnumerable<Report> reports, Guid userId, UserRole userRole, Department userDepartment)
        {
            switch (userRole)
            {
                case UserRole.GeneralStaff:
                    // Staff can only see their own reports
                    return reports.Where(r => r.CreatedBy == userId);

                case UserRole.LineManager:
                    // Line managers can see:
                    // 1. Their own reports
                    // 2. Reports from their department that are submitted and need their approval
                    // 3. Reports they have already approved
                    return reports.Where(r =>
                        r.CreatedBy == userId ||
                        (r.Department == userDepartment && (
                            r.Status == ReportStatus.Submitted ||
                            r.Status == ReportStatus.ManagerReview ||
                            r.Status == ReportStatus.ManagerApproved ||
                            r.Status == ReportStatus.ExecutiveReview ||
                            r.Status == ReportStatus.Completed
                        ))
                    );

                case UserRole.Executive:
                    // Executives can see:
                    // 1. Their own reports
                    // 2. All reports that have been approved by line managers (need executive approval)
                    // 3. All completed reports for oversight
                    return reports.Where(r =>
                        r.CreatedBy == userId ||
                        r.Status == ReportStatus.ManagerApproved ||
                        r.Status == ReportStatus.ExecutiveReview ||
                        r.Status == ReportStatus.Completed
                    );

                default:
                    return new List<Report>();
            }
        }

        public async Task<ServiceResultDto> SubmitReportAsync(Guid reportId, SubmitReportDto dto, Guid userId, UserRole userRole)
        {
            try
            {
                // Only staff can submit reports
                if (userRole != UserRole.GeneralStaff)
                    return ServiceResultDto.ErrorResult("Only staff members can submit reports");

                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                    return ServiceResultDto.ErrorResult("Report not found");

                if (report.CreatedBy != userId)
                    return ServiceResultDto.ErrorResult("You can only submit your own reports");

                if (report.Status != ReportStatus.Draft)
                    return ServiceResultDto.ErrorResult("Only draft reports can be submitted");

                // Update report status to submitted and assign to line manager
                var success = await _reportRepository.UpdateReportStatusAsync(reportId, ReportStatus.Submitted, userId);
                if (success)
                {
                    await _auditLogRepository.LogActionAsync(AuditAction.Submitted, userId, reportId, 
                        $"Report submitted for manager review: {dto.Comments}");
                    return ServiceResultDto.SuccessResult("Report submitted to Line Manager for review");
                }

                return ServiceResultDto.ErrorResult("Failed to submit report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting report {ReportId}", reportId);
                return ServiceResultDto.ErrorResult("An error occurred");
            }
        }
    }
}