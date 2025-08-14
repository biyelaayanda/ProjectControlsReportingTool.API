using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ProjectControlsReportingTool.API.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;
        private readonly IWebHostEnvironment _environment;

        public ReportService(
            IReportRepository reportRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<ReportService> logger,
            IWebHostEnvironment environment)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _environment = environment;
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
                    Department = user.Department, // Use user's department instead of DTO department
                    Status = ReportStatus.Draft,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                // Generate report number
                report.ReportNumber = await _reportRepository.GenerateReportNumberAsync(user.Department);

                var createdReport = await _reportRepository.CreateReportAsync(report);

                // Handle file attachments if any
                if (dto.Attachments != null && dto.Attachments.Count > 0)
                {
                    await ProcessFileAttachmentsAsync(createdReport.Id, dto.Attachments, userId);
                }

                // Log the action
                await _auditLogRepository.LogActionAsync(AuditAction.Created, userId, createdReport.Id, 
                    $"Report created: {createdReport.Title}");

                // Return the report with attachments
                var reportWithAttachments = await GetReportWithAttachmentsAsync(createdReport.Id);
                return _mapper.Map<ReportDto>(reportWithAttachments);
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
                Console.WriteLine($"GetReportByIdAsync called: ReportId={reportId}, UserId={userId}, UserRole={userRole}");
                
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) 
                {
                    Console.WriteLine($"User {userId} not found");
                    return null;
                }
                Console.WriteLine($"User found: {user.FirstName} {user.LastName}, Department: {user.Department}");

                var canAccess = await _reportRepository.CanUserAccessReportAsync(reportId, userId, userRole, user.Department);
                Console.WriteLine($"Access check result: {canAccess}");
                if (!canAccess) 
                {
                    Console.WriteLine($"Access denied for user {userId} to report {reportId}");
                    return null;
                }

                var report = await _reportRepository.GetReportWithDetailsAsync(reportId);
                if (report == null) 
                {
                    Console.WriteLine($"Report {reportId} not found in database");
                    return null;
                }
                Console.WriteLine($"Report found: {report.Title}, Status: {report.Status}, Department: {report.Department}");

                var result = _mapper.Map<ReportDetailDto>(report);
                Console.WriteLine($"Successfully mapped report to DTO");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetReportByIdAsync: {ex.Message}");
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

                var report = await _reportRepository.GetWithDetailsAsync(reportId);
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
                    // Executive can approve:
                    // 1. Manager-approved reports (from staff → manager → executive workflow)
                    // 2. Line Manager submitted reports (direct manager → executive workflow)
                    if (report.Status != ReportStatus.ManagerApproved && report.Status != ReportStatus.Submitted)
                        return ServiceResultDto.ErrorResult("Report must be either approved by Line Manager or submitted by Line Manager");

                    // Check if this is a Line Manager submitted report (Submitted status + creator is LineManager)
                    if (report.Status == ReportStatus.Submitted && report.Creator.Role != UserRole.LineManager)
                        return ServiceResultDto.ErrorResult("Only Line Manager submitted reports can be approved directly by Executive");

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
                Console.WriteLine($"RejectReportAsync called: ReportId={reportId}, UserId={userId}, UserRole={userRole}, Reason={dto.Reason}");
                
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    Console.WriteLine($"User {userId} not found");
                    return ServiceResultDto.ErrorResult("User not found");
                }

                if (userRole != UserRole.LineManager && userRole != UserRole.Executive)
                {
                    Console.WriteLine($"Insufficient permissions for user {userId} with role {userRole}");
                    return ServiceResultDto.ErrorResult("Insufficient permissions");
                }

                // Get the report to check current status
                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                {
                    Console.WriteLine($"Report {reportId} not found");
                    return ServiceResultDto.ErrorResult("Report not found");
                }

                Console.WriteLine($"Report found: {report.Title}, Status: {report.Status}");

                // Check if report can be rejected based on current status and user role
                if (userRole == UserRole.LineManager && report.Status != ReportStatus.Submitted)
                {
                    Console.WriteLine($"Line Manager cannot reject report with status {report.Status}");
                    return ServiceResultDto.ErrorResult("Report must be in Submitted status for Line Manager to reject");
                }

                if (userRole == UserRole.Executive && report.Status != ReportStatus.ManagerApproved && report.Status != ReportStatus.Submitted && report.Status != ReportStatus.ExecutiveReview)
                {
                    Console.WriteLine($"Executive cannot reject report with status {report.Status}");
                    return ServiceResultDto.ErrorResult("Report must be Manager Approved, Executive Review, or Submitted (for manager-created reports) for Executive to reject");
                }

                var success = await _reportRepository.RejectReportAsync(reportId, userId, dto.Reason);
                Console.WriteLine($"Repository rejection result: {success}");
                
                if (success)
                {
                    await _auditLogRepository.LogActionAsync(AuditAction.Rejected, userId, reportId, 
                        $"Report rejected by {userRole}: {dto.Reason}");
                    Console.WriteLine($"Report {reportId} successfully rejected");
                    return ServiceResultDto.SuccessResult();
                }

                Console.WriteLine($"Failed to reject report {reportId}");
                return ServiceResultDto.ErrorResult("Failed to reject report");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RejectReportAsync: {ex.Message}");
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

                // Only allow deletion if user is the creator and report is in draft/rejected status, or user is executive
                if (userRole != UserRole.Executive && (report.CreatedBy != userId || 
                    (report.Status != ReportStatus.Draft && 
                     report.Status != ReportStatus.Rejected && 
                     report.Status != ReportStatus.ManagerRejected && 
                     report.Status != ReportStatus.ExecutiveRejected)))
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
                    // 3. Reports they have already approved/reviewed (regardless of current status)
                    return reports.Where(r =>
                        r.CreatedBy == userId ||
                        (r.Department == userDepartment && (
                            r.Status == ReportStatus.Submitted ||
                            r.Status == ReportStatus.ManagerReview ||
                            r.Status == ReportStatus.ManagerApproved ||
                            r.Status == ReportStatus.ExecutiveReview ||
                            r.Status == ReportStatus.Completed
                        )) ||
                        // Include reports that this manager has previously signed/reviewed
                        r.Signatures.Any(s => s.UserId == userId && s.SignatureType == SignatureType.ManagerSignature)
                    );

                case UserRole.Executive:
                    // Executives can see ALL reports from ALL departments that need executive attention:
                    // 1. Their own reports
                    // 2. Reports approved by line managers (ManagerApproved status)
                    // 3. Reports submitted by line managers (Submitted status + creator is LineManager)
                    // 4. Reports in executive review
                    // 5. Completed reports for oversight
                    return reports.Where(r =>
                        r.CreatedBy == userId ||
                        r.Status == ReportStatus.ManagerApproved ||
                        (r.Status == ReportStatus.Submitted && r.Creator.Role == UserRole.LineManager) ||
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
                // Only staff and line managers can submit reports
                // Staff submit to line managers, Line managers submit to executives
                if (userRole != UserRole.GeneralStaff && userRole != UserRole.LineManager)
                    return ServiceResultDto.ErrorResult("Only staff members and line managers can submit reports");

                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                    return ServiceResultDto.ErrorResult("Report not found");

                if (report.CreatedBy != userId)
                    return ServiceResultDto.ErrorResult("You can only submit your own reports");

                if (report.Status != ReportStatus.Draft && 
                    report.Status != ReportStatus.Rejected && 
                    report.Status != ReportStatus.ManagerRejected && 
                    report.Status != ReportStatus.ExecutiveRejected)
                    return ServiceResultDto.ErrorResult("Only draft or rejected reports can be submitted");

                // Determine target status based on submitter role
                var targetStatus = userRole == UserRole.GeneralStaff 
                    ? ReportStatus.Submitted        // Staff → Line Manager review
                    : ReportStatus.Submitted;       // Line Manager → Executive review (changed from ManagerApproved)

                var success = await _reportRepository.UpdateReportStatusAsync(reportId, targetStatus, userId);
                if (success)
                {
                    var reviewerType = userRole == UserRole.GeneralStaff ? "Line Manager" : "Executive";
                    await _auditLogRepository.LogActionAsync(AuditAction.Submitted, userId, reportId, 
                        $"Report submitted for {reviewerType} review: {dto.Comments}");
                    return ServiceResultDto.SuccessResult($"Report submitted to {reviewerType} for review");
                }

                return ServiceResultDto.ErrorResult("Failed to submit report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting report {ReportId}", reportId);
                return ServiceResultDto.ErrorResult("An error occurred");
            }
        }

        public async Task<ReportAttachment?> GetAttachmentAsync(Guid reportId, Guid attachmentId, Guid userId)
        {
            try
            {
                // First check if the user has access to the report
                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                    return null;

                // Check if user has permission to access this report
                // Users can access:
                // 1. Reports they created
                // 2. Reports in their department (if they're line manager or executive)
                // 3. All reports (if they're executive)
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                bool hasAccess = report.CreatedBy == userId || // Creator
                                user.Role == UserRole.Executive || // Executive can see all
                                (user.Role == UserRole.LineManager && report.Department == user.Department); // Line manager for same department

                if (!hasAccess)
                    throw new UnauthorizedAccessException("You don't have permission to access this report's attachments");

                // Get the specific attachment
                var attachment = await _context.ReportAttachments
                    .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ReportId == reportId && a.IsActive);

                return attachment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachment {AttachmentId} for report {ReportId}", attachmentId, reportId);
                throw;
            }
        }

        private async Task ProcessFileAttachmentsAsync(Guid reportId, List<IFormFile> attachments, Guid userId)
        {
            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "reports", reportId.ToString());
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                foreach (var attachment in attachments)
                {
                    if (attachment.Length > 0)
                    {
                        // Generate unique filename
                        var fileExtension = Path.GetExtension(attachment.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsPath, uniqueFileName);

                        // Save file to disk
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await attachment.CopyToAsync(stream);
                        }

                        // Save attachment info to database
                        var reportAttachment = new ReportAttachment
                        {
                            Id = Guid.NewGuid(),
                            ReportId = reportId,
                            FileName = uniqueFileName,
                            OriginalFileName = attachment.FileName,
                            FilePath = filePath,
                            ContentType = attachment.ContentType,
                            FileSize = attachment.Length,
                            UploadedBy = userId,
                            UploadedDate = DateTime.UtcNow,
                            IsActive = true
                        };

                        _context.ReportAttachments.Add(reportAttachment);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file attachments for report {ReportId}", reportId);
                throw;
            }
        }

        private async Task<Report?> GetReportWithAttachmentsAsync(Guid reportId)
        {
            return await _context.Reports
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }
    }
}