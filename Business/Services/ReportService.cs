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
                    var uploaderName = $"{user.FirstName} {user.LastName}";
                    await ProcessFileAttachmentsAsync(createdReport.Id, dto.Attachments, userId, ApprovalStage.Initial, user.Role, uploaderName);
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
                    
                    // Apply filters only when not using search (search stored procedure handles these)
                    if (filter.Status.HasValue)
                        reports = reports.Where(r => r.Status == filter.Status.Value);

                    if (filter.Department.HasValue && userRole == UserRole.GM)
                        reports = reports.Where(r => r.Department == filter.Department.Value);

                    if (filter.FromDate.HasValue)
                        reports = reports.Where(r => r.CreatedDate >= filter.FromDate.Value);

                    if (filter.ToDate.HasValue)
                        reports = reports.Where(r => r.CreatedDate <= filter.ToDate.Value);
                }

                // Apply role-based filtering for workflow visibility
                reports = ApplyRoleBasedFiltering(reports, userId, userRole, userDepartment);

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
                if (user == null) 
                {
                    return null;
                }
                var canAccess = await _reportRepository.CanUserAccessReportAsync(reportId, userId, userRole, user.Department);
                if (!canAccess) 
                {
                    return null;
                }

                var report = await _reportRepository.GetReportWithDetailsAsync(reportId);
                if (report == null) 
                {
                    return null;
                }
                var result = _mapper.Map<ReportDetailDto>(report);
                return result;
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

                if (userRole != UserRole.LineManager && userRole != UserRole.GM)
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

                    // Update to ManagerApproved and forward to GM
                    var success = await _reportRepository.UpdateReportStatusAsync(reportId, ReportStatus.ManagerApproved, userId);
                    if (success)
                    {
                        await _auditLogRepository.LogActionAsync(AuditAction.Approved, userId, reportId, 
                            $"Report approved by Line Manager: {dto.Comments}");
                        return ServiceResultDto.SuccessResult("Report approved and forwarded to GM");
                    }
                }
                else if (userRole == UserRole.GM)
                {
                    // GM can approve:
                    // 1. Manager-approved reports (from staff → manager → GM workflow)
                    // 2. Line Manager submitted reports (direct manager → GM workflow)
                    if (report.Status != ReportStatus.ManagerApproved && report.Status != ReportStatus.Submitted)
                        return ServiceResultDto.ErrorResult("Report must be either approved by Line Manager or submitted by Line Manager");

                    // Check if this is a Line Manager submitted report (Submitted status + creator is LineManager)
                    if (report.Status == ReportStatus.Submitted && report.Creator.Role != UserRole.LineManager)
                        return ServiceResultDto.ErrorResult("Only Line Manager submitted reports can be approved directly by GM");

                    // Final approval - mark as completed
                    var success = await _reportRepository.UpdateReportStatusAsync(reportId, ReportStatus.Completed, userId);
                    if (success)
                    {
                        await _auditLogRepository.LogActionAsync(AuditAction.Approved, userId, reportId, 
                            $"Report given final approval by GM: {dto.Comments}");
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
                {
                    return ServiceResultDto.ErrorResult("User not found");
                }

                if (userRole != UserRole.LineManager && userRole != UserRole.GM)
                {
                    return ServiceResultDto.ErrorResult("Insufficient permissions");
                }

                // Get the report to check current status
                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                {
                    return ServiceResultDto.ErrorResult("Report not found");
                }

                // Check if report can be rejected based on current status and user role
                if (userRole == UserRole.LineManager && report.Status != ReportStatus.Submitted)
                {
                    return ServiceResultDto.ErrorResult("Report must be in Submitted status for Line Manager to reject");
                }

                if (userRole == UserRole.GM && report.Status != ReportStatus.ManagerApproved && report.Status != ReportStatus.Submitted && report.Status != ReportStatus.GMReview)
                {
                    return ServiceResultDto.ErrorResult("Report must be Manager Approved, GM Review, or Submitted (for manager-created reports) for GM to reject");
                }

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

                // Only allow deletion if user is the creator and report is in draft/rejected status, or user is GM
                if (userRole != UserRole.GM && (report.CreatedBy != userId || 
                    (report.Status != ReportStatus.Draft && 
                     report.Status != ReportStatus.Rejected && 
                     report.Status != ReportStatus.ManagerRejected && 
                     report.Status != ReportStatus.GMRejected)))
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
                    UserRole.GM => await _reportRepository.GetPendingApprovalsForGMAsync(),
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

        public async Task<IEnumerable<ReportSummaryDto>> GetGMReportsAsync()
        {
            try
            {
                var reports = await _reportRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<ReportSummaryDto>>(reports.OrderByDescending(r => r.LastModifiedDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GM reports");
                throw;
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
                _logger.LogError(ex, "Error getting GM reports");
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
                            r.Status == ReportStatus.GMReview ||
                            r.Status == ReportStatus.Completed
                        )) ||
                        // Include reports that this manager has previously signed/reviewed
                        r.Signatures.Any(s => s.UserId == userId && s.SignatureType == SignatureType.ManagerSignature)
                    );

                case UserRole.GM:
                    // GMs can see ALL reports from ALL departments that need GM attention:
                    // 1. Their own reports
                    // 2. Reports approved by line managers (ManagerApproved status)
                    // 3. Reports submitted by line managers (Submitted status + creator is LineManager)
                    // 4. Reports in GM review
                    // 5. Completed reports for oversight
                    return reports.Where(r =>
                        r.CreatedBy == userId ||
                        r.Status == ReportStatus.ManagerApproved ||
                        (r.Status == ReportStatus.Submitted && r.Creator.Role == UserRole.LineManager) ||
                        r.Status == ReportStatus.GMReview ||
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
                // Staff submit to line managers, Line managers submit to GMs
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
                    report.Status != ReportStatus.GMRejected)
                    return ServiceResultDto.ErrorResult("Only draft or rejected reports can be submitted");

                // Determine target status based on submitter role
                var targetStatus = userRole == UserRole.GeneralStaff 
                    ? ReportStatus.Submitted        // Staff → Line Manager review
                    : ReportStatus.Submitted;       // Line Manager → GM review (changed from ManagerApproved)

                var success = await _reportRepository.UpdateReportStatusAsync(reportId, targetStatus, userId);
                if (success)
                {
                    var reviewerType = userRole == UserRole.GeneralStaff ? "Line Manager" : "GM";
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
                // 2. Reports in their department (if they're line manager or GM)
                // 3. All reports (if they're GM)
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                bool hasAccess = report.CreatedBy == userId || // Creator
                                user.Role == UserRole.GM || // GM can see all
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

        private async Task ProcessFileAttachmentsAsync(Guid reportId, List<IFormFile> attachments, Guid userId, ApprovalStage approvalStage = ApprovalStage.Initial, UserRole userRole = UserRole.GeneralStaff, string? uploaderName = null)
        {
            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "reports", reportId.ToString());
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Get user name if not provided
                if (string.IsNullOrEmpty(uploaderName))
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    uploaderName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User";
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
                            UploadedByName = uploaderName,
                            UploadedByRole = userRole,
                            ApprovalStage = approvalStage,
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

        public async Task<ServiceResultDto> UploadApprovalDocumentsAsync(Guid reportId, IFormFileCollection files, Guid userId, UserRole userRole, string? description = null)
        {
            try
            {
                // Get the report to check its status and user permissions
                var report = await _context.Reports
                    .Include(r => r.Creator)
                    .FirstOrDefaultAsync(r => r.Id == reportId);

                if (report == null)
                {
                    return ServiceResultDto.ErrorResult("Report not found");
                }

                // Determine the approval stage based on user role and report status
                ApprovalStage approvalStage;
                bool canUpload = false;

                switch (userRole)
                {
                    case UserRole.LineManager:
                        // Line managers can upload when report is submitted for their review
                        // AND when the report is from their department
                        var currentUser = await _userRepository.GetByIdAsync(userId);
                        _logger.LogInformation($"Manager upload check: UserId={userId}, UserDept={currentUser?.Department}, ReportDept={report.Department}, ReportStatus={report.Status}");
                        
                        if (currentUser == null)
                        {
                            return ServiceResultDto.ErrorResult("User not found");
                        }
                        
                        if (report.Status != ReportStatus.Submitted)
                        {
                            return ServiceResultDto.ErrorResult($"Cannot upload documents. Report status must be Submitted, but is: {report.Status}");
                        }
                        
                        if (report.Department != currentUser.Department)
                        {
                            return ServiceResultDto.ErrorResult($"Cannot upload documents. Report is from {report.Department} department, but you are from {currentUser.Department} department");
                        }
                        
                        canUpload = true;
                        approvalStage = ApprovalStage.ManagerReview;
                        break;
                    case UserRole.GM:
                        // GMs can upload when report is manager-approved or submitted by line managers
                        _logger.LogInformation($"GM upload check: ReportStatus={report.Status}, CreatorRole={report.Creator?.Role}");
                        
                        if (report.Status == ReportStatus.ManagerApproved)
                        {
                            canUpload = true;
                        }
                        else if (report.Status == ReportStatus.Submitted && report.Creator?.Role == UserRole.LineManager)
                        {
                            canUpload = true;
                        }
                        else
                        {
                            return ServiceResultDto.ErrorResult($"Cannot upload documents. Report status: {report.Status}, Creator role: {report.Creator?.Role}");
                        }
                        
                        approvalStage = ApprovalStage.GMReview;
                        break;
                    default:
                        return ServiceResultDto.ErrorResult("Only managers and GMs can upload approval documents");
                }

                if (!canUpload)
                {
                    return ServiceResultDto.ErrorResult($"Cannot upload documents. Report status: {report.Status}");
                }

                // Convert IFormFileCollection to List<IFormFile>
                var fileList = files.ToList();

                // Get user name for tracking
                var user = await _userRepository.GetByIdAsync(userId);
                var uploaderName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User";

                // Process the file attachments with approval stage
                await ProcessFileAttachmentsAsync(reportId, fileList, userId, approvalStage, userRole, uploaderName);

                // Log the action
                await _auditLogRepository.LogActionAsync(
                    AuditAction.Uploaded, 
                    userId, 
                    reportId, 
                    $"Uploaded {files.Count} document(s) during {approvalStage} stage"
                );

                return ServiceResultDto.SuccessResult($"Successfully uploaded {files.Count} document(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading approval documents for report {ReportId}", reportId);
                return ServiceResultDto.ErrorResult("An error occurred while uploading documents");
            }
        }

        public async Task<IEnumerable<ReportAttachmentDto>> GetReportAttachmentsByStageAsync(Guid reportId, ApprovalStage? stage, Guid userId, UserRole userRole)
        {
            try
            {
                // Check permissions to view this report
                var report = await _context.Reports
                    .Include(r => r.Creator)
                    .FirstOrDefaultAsync(r => r.Id == reportId);

                if (report == null)
                {
                    return Enumerable.Empty<ReportAttachmentDto>();
                }

                // Check access permissions (same logic as GetAttachmentAsync)
                bool hasAccess = false;

                switch (userRole)
                {
                    case UserRole.GeneralStaff:
                        hasAccess = report.CreatedBy == userId;
                        break;
                    case UserRole.LineManager:
                        hasAccess = report.CreatedBy == userId || report.Creator.Department == GetUserDepartment(userId);
                        break;
                    case UserRole.GM:
                        hasAccess = true; // GMs can see all reports
                        break;
                }

                if (!hasAccess)
                {
                    return Enumerable.Empty<ReportAttachmentDto>();
                }

                // Get attachments filtered by stage
                var query = _context.ReportAttachments
                    .Include(a => a.UploadedByUser)
                    .Where(a => a.ReportId == reportId && a.IsActive);

                if (stage.HasValue)
                {
                    query = query.Where(a => a.ApprovalStage == stage.Value);
                }

                var attachments = await query
                    .OrderBy(a => a.ApprovalStage)
                    .ThenBy(a => a.UploadedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ReportAttachmentDto>>(attachments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments by stage for report {ReportId}", reportId);
                return Enumerable.Empty<ReportAttachmentDto>();
            }
        }

        private Department GetUserDepartment(Guid userId)
        {
            // This is a placeholder - you might want to implement this properly
            // For now, returning a default value
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user?.Department ?? Department.ProjectSupport;
        }

        #region Statistics and Analytics Methods (Phase 7.1)

        public async Task<ReportStatisticsDto> GetReportStatisticsAsync(StatisticsFilterDto filter, Guid userId, UserRole userRole, Department userDepartment)
        {
            try
            {
                var statistics = new ReportStatisticsDto();

                // Get overall stats
                statistics.OverallStats = await GetOverallStatsAsync(filter.StartDate, filter.EndDate);

                // Get department stats (filtered by user role)
                var departmentStats = await GetDepartmentStatsAsync(filter.StartDate, filter.EndDate);
                if (userRole == UserRole.GeneralStaff)
                {
                    statistics.DepartmentStats = departmentStats.Where(d => d.Department == userDepartment);
                }
                else
                {
                    statistics.DepartmentStats = departmentStats;
                }

                // Get status stats
                statistics.StatusStats = await GetStatusStatsAsync(filter.StartDate, filter.EndDate, userRole, userDepartment);

                // Get priority stats
                statistics.PriorityStats = await GetPriorityStatsAsync(filter.StartDate, filter.EndDate, userRole, userDepartment);

                // Get performance metrics if requested
                if (filter.IncludePerformanceMetrics)
                {
                    statistics.PerformanceMetrics = await GetPerformanceMetricsAsync(filter.StartDate, filter.EndDate);
                }

                // Get trend analysis if requested
                if (filter.IncludeTrendAnalysis)
                {
                    var trendPeriod = filter.TrendPeriod ?? "monthly";
                    var department = userRole == UserRole.GeneralStaff ? userDepartment : filter.Department;
                    statistics.TrendAnalysis = await GetTrendAnalysisAsync(trendPeriod, 12, department);
                }

                // Get user-specific stats if requested
                if (filter.IncludeUserStats)
                {
                    var targetUserId = filter.UserId ?? userId;
                    statistics.UserSpecificStats = await GetUserStatsAsync(targetUserId, filter.StartDate, filter.EndDate);
                }

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report statistics for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OverallStatsDto> GetOverallStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Reports.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.CreatedDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(r => r.CreatedDate <= endDate.Value);

                var now = DateTime.UtcNow;
                var currentMonthStart = new DateTime(now.Year, now.Month, 1);
                var lastMonthStart = currentMonthStart.AddMonths(-1);

                var stats = new OverallStatsDto
                {
                    TotalReports = await query.CountAsync(),
                    TotalDrafts = await query.CountAsync(r => r.Status == ReportStatus.Draft),
                    TotalSubmitted = await query.CountAsync(r => r.Status == ReportStatus.Submitted),
                    TotalUnderReview = await query.CountAsync(r => r.Status == ReportStatus.ManagerReview || r.Status == ReportStatus.GMReview),
                    TotalApproved = await query.CountAsync(r => r.Status == ReportStatus.Completed),
                    TotalRejected = await query.CountAsync(r => r.Status == ReportStatus.ManagerRejected || r.Status == ReportStatus.GMRejected),
                    TotalOverdue = await query.CountAsync(r => r.DueDate < now && r.Status != ReportStatus.Completed),
                    TotalThisMonth = await query.CountAsync(r => r.CreatedDate >= currentMonthStart),
                    TotalLastMonth = await query.CountAsync(r => r.CreatedDate >= lastMonthStart && r.CreatedDate < currentMonthStart),
                    LastUpdated = DateTime.UtcNow
                };

                // Calculate month-over-month growth
                if (stats.TotalLastMonth > 0)
                {
                    stats.MonthOverMonthGrowth = ((double)(stats.TotalThisMonth - stats.TotalLastMonth) / stats.TotalLastMonth) * 100;
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overall stats");
                throw;
            }
        }

        public async Task<IEnumerable<DepartmentStatsDto>> GetDepartmentStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Reports.Include(r => r.Creator).AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.CreatedDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(r => r.CreatedDate <= endDate.Value);

                var departmentStats = await query
                    .GroupBy(r => r.Creator.Department)
                    .Select(g => new DepartmentStatsDto
                    {
                        Department = g.Key,
                        DepartmentName = g.Key.ToString(),
                        TotalReports = g.Count(),
                        PendingReports = g.Count(r => r.Status == ReportStatus.Submitted || r.Status == ReportStatus.ManagerReview || r.Status == ReportStatus.GMReview),
                        ApprovedReports = g.Count(r => r.Status == ReportStatus.Completed),
                        RejectedReports = g.Count(r => r.Status == ReportStatus.ManagerRejected || r.Status == ReportStatus.GMRejected),
                        OverdueReports = g.Count(r => r.DueDate < DateTime.UtcNow && r.Status != ReportStatus.Completed),
                        ApprovalRate = g.Count() > 0 ? (double)g.Count(r => r.Status == ReportStatus.Completed) / g.Count() * 100 : 0
                    })
                    .ToListAsync();

                // Calculate additional metrics
                foreach (var stat in departmentStats)
                {
                    // Calculate average completion time for approved reports
                    var completedReports = await query
                        .Where(r => r.Creator.Department == stat.Department && r.Status == ReportStatus.Completed && r.CompletedDate.HasValue)
                        .Select(r => new { r.CreatedDate, ApprovedDate = r.CompletedDate })
                        .ToListAsync();

                    if (completedReports.Any())
                    {
                        stat.AverageCompletionTime = completedReports
                            .Average(r => (r.ApprovedDate!.Value - r.CreatedDate).TotalDays);
                    }

                    // Get active users count
                    stat.ActiveUsers = await _context.Users
                        .CountAsync(u => u.Department == stat.Department && u.IsActive);
                }

                return departmentStats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department stats");
                throw;
            }
        }

        public async Task<IEnumerable<TrendDataDto>> GetTrendAnalysisAsync(string period = "monthly", int periodCount = 12, Department? department = null)
        {
            try
            {
                var trendData = new List<TrendDataDto>();
                var now = DateTime.UtcNow;

                for (int i = periodCount - 1; i >= 0; i--)
                {
                    DateTime startDate, endDate;
                    
                    switch (period.ToLower())
                    {
                        case "daily":
                            startDate = now.Date.AddDays(-i);
                            endDate = startDate.AddDays(1);
                            break;
                        case "weekly":
                            startDate = now.Date.AddDays(-7 * i);
                            endDate = startDate.AddDays(7);
                            break;
                        case "yearly":
                            startDate = new DateTime(now.Year - i, 1, 1);
                            endDate = startDate.AddYears(1);
                            break;
                        default: // monthly
                            startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                            endDate = startDate.AddMonths(1);
                            break;
                    }

                    var query = _context.Reports.Include(r => r.Creator).AsQueryable()
                        .Where(r => r.CreatedDate >= startDate && r.CreatedDate < endDate);

                    if (department.HasValue)
                        query = query.Where(r => r.Creator.Department == department.Value);

                    var periodData = new TrendDataDto
                    {
                        Date = startDate,
                        Period = period,
                        Department = department,
                        ReportsCreated = await query.CountAsync(),
                        ReportsApproved = await query.CountAsync(r => r.Status == ReportStatus.Completed),
                        ReportsRejected = await query.CountAsync(r => r.Status == ReportStatus.ManagerRejected || r.Status == ReportStatus.GMRejected),
                        ReportsSubmitted = await query.CountAsync(r => r.Status == ReportStatus.Submitted || r.Status == ReportStatus.ManagerReview || r.Status == ReportStatus.GMReview),
                        ActiveUsers = await _context.Users.CountAsync(u => u.IsActive && (!department.HasValue || u.Department == department.Value))
                    };

                    // Calculate average processing time for completed reports in this period
                    var completedReports = await query
                        .Where(r => r.Status == ReportStatus.Completed && r.CompletedDate.HasValue)
                        .Select(r => new { r.CreatedDate, ApprovedDate = r.CompletedDate })
                        .ToListAsync();

                    if (completedReports.Any())
                    {
                        periodData.AverageProcessingTime = completedReports
                            .Average(r => (r.ApprovedDate!.Value - r.CreatedDate).TotalDays);
                    }

                    trendData.Add(periodData);
                }

                return trendData.OrderBy(t => t.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trend analysis");
                throw;
            }
        }

        public async Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var now = DateTime.UtcNow;
                var measurementStart = startDate ?? now.AddDays(-30);
                var measurementEnd = endDate ?? now;

                var query = _context.Reports.AsQueryable()
                    .Where(r => r.CreatedDate >= measurementStart && r.CreatedDate <= measurementEnd);

                var metrics = new PerformanceMetricsDto
                {
                    MeasurementPeriodStart = measurementStart,
                    MeasurementPeriodEnd = measurementEnd,
                    SystemUptime = 99.5, // This would come from monitoring system
                    AverageResponseTime = 150, // This would come from monitoring system
                    ErrorRate = 2, // errors per 1000 requests - from monitoring system
                    UserSatisfactionScore = 4.2 // This would come from user feedback system
                };

                // Calculate average report creation time (time spent in draft status)
                var draftReports = await query
                    .Where(r => r.Status != ReportStatus.Draft && r.SubmittedDate.HasValue)
                    .Select(r => new { r.CreatedDate, r.SubmittedDate })
                    .ToListAsync();

                if (draftReports.Any())
                {
                    metrics.AverageReportCreationTime = draftReports
                        .Average(r => (r.SubmittedDate!.Value - r.CreatedDate).TotalMinutes);
                }

                // Calculate average approval time
                var approvedReports = await query
                    .Where(r => r.Status == ReportStatus.Completed && r.CompletedDate.HasValue && r.SubmittedDate.HasValue)
                    .Select(r => new { r.SubmittedDate, ApprovedDate = r.CompletedDate })
                    .ToListAsync();

                if (approvedReports.Any())
                {
                    metrics.AverageApprovalTime = approvedReports
                        .Average(r => (r.ApprovedDate!.Value - r.SubmittedDate!.Value).TotalDays);
                }

                // Calculate average review cycle time (submission to final decision)
                var processedReports = await query
                    .Where(r => (r.Status == ReportStatus.Completed || r.Status == ReportStatus.ManagerRejected || r.Status == ReportStatus.GMRejected) 
                                && r.SubmittedDate.HasValue)
                    .Select(r => new { 
                        r.SubmittedDate, 
                        FinalDate = r.CompletedDate ?? r.RejectedDate 
                    })
                    .Where(r => r.FinalDate.HasValue)
                    .ToListAsync();

                if (processedReports.Any())
                {
                    metrics.AverageReviewCycleTime = processedReports
                        .Average(r => (r.FinalDate!.Value - r.SubmittedDate!.Value).TotalDays);
                }

                // These would typically come from application monitoring tools
                metrics.TotalApiCalls = 15000; // Mock data - would be from monitoring
                
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                throw;
            }
        }

        public async Task<UserStatsDto> GetUserStatsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("User not found", nameof(userId));

                var query = _context.Reports.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.CreatedDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(r => r.CreatedDate <= endDate.Value);

                var userReports = query.Where(r => r.CreatedBy == userId);
                var now = DateTime.UtcNow;

                var stats = new UserStatsDto
                {
                    UserId = userId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    MyReportsCount = await userReports.CountAsync(),
                    MyDraftsCount = await userReports.CountAsync(r => r.Status == ReportStatus.Draft),
                    MyPendingApprovals = await userReports.CountAsync(r => r.Status == ReportStatus.Submitted || r.Status == ReportStatus.ManagerReview || r.Status == ReportStatus.GMReview),
                    MyApprovedReports = await userReports.CountAsync(r => r.Status == ReportStatus.Completed),
                    MyRejectedReports = await userReports.CountAsync(r => r.Status == ReportStatus.ManagerRejected || r.Status == ReportStatus.GMRejected),
                    MyOverdueReports = await userReports.CountAsync(r => r.DueDate < now && r.Status != ReportStatus.Completed),
                    LastLoginDate = user.LastLoginDate,
                };

                // Calculate last report created date
                var lastReport = await userReports.OrderByDescending(r => r.CreatedDate).FirstOrDefaultAsync();
                stats.LastReportCreated = lastReport?.CreatedDate ?? DateTime.MinValue;

                // Calculate approval rate
                var totalNonDraftReports = await userReports.CountAsync(r => r.Status != ReportStatus.Draft);
                if (totalNonDraftReports > 0)
                {
                    stats.MyApprovalRate = (double)stats.MyApprovedReports / totalNonDraftReports * 100;
                }

                // Calculate average completion time
                var completedReports = await userReports
                    .Where(r => r.Status == ReportStatus.Completed && r.CompletedDate.HasValue)
                    .Select(r => new { r.CreatedDate, ApprovedDate = r.CompletedDate })
                    .ToListAsync();

                if (completedReports.Any())
                {
                    stats.MyAverageCompletionTime = completedReports
                        .Average(r => (r.ApprovedDate!.Value - r.CreatedDate).TotalDays);
                }

                // For managers and GMs, get additional stats
                if (user.Role == UserRole.LineManager || user.Role == UserRole.GM)
                {
                    var reviewQuery = _context.Reports.AsQueryable();
                    
                    if (startDate.HasValue)
                        reviewQuery = reviewQuery.Where(r => r.CreatedDate >= startDate.Value);
                    if (endDate.HasValue)
                        reviewQuery = reviewQuery.Where(r => r.CreatedDate <= endDate.Value);

                    // Reports reviewed by this user (based on approval/rejection)
                    // Reports reviewed by this user (based on completion/rejection tracking)
                    // Note: This is simplified since we don't have ApprovedBy tracking
                    stats.ReportsReviewedByMe = await reviewQuery
                        .CountAsync(r => (r.Status == ReportStatus.Completed || r.Status == ReportStatus.ManagerRejected || r.Status == ReportStatus.GMRejected) 
                                        && r.RejectedBy == userId);

                    // For line managers, get team reports
                    if (user.Role == UserRole.LineManager)
                    {
                        stats.MyTeamReportsCount = await query
                            .Include(r => r.Creator)
                            .CountAsync(r => r.Creator.Department == user.Department && r.CreatedBy != userId);
                    }
                    else if (user.Role == UserRole.GM)
                    {
                        // GMs can see all reports as "team reports"
                        stats.MyTeamReportsCount = await query.CountAsync(r => r.CreatedBy != userId);
                    }

                    // Calculate average review time
                    var reviewedReports = await reviewQuery
                        .Where(r => r.RejectedBy == userId && r.SubmittedDate.HasValue)
                        .Select(r => new { 
                            r.SubmittedDate, 
                            ReviewDate = r.CompletedDate ?? r.RejectedDate 
                        })
                        .Where(r => r.ReviewDate.HasValue)
                        .ToListAsync();

                    if (reviewedReports.Any())
                    {
                        stats.MyAverageReviewTime = reviewedReports
                            .Average(r => (r.ReviewDate!.Value - r.SubmittedDate!.Value).TotalDays);
                    }
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user stats for user {UserId}", userId);
                throw;
            }
        }

        public Task<SystemPerformanceDto> GetSystemPerformanceAsync()
        {
            try
            {
                // This would typically integrate with monitoring tools like Application Insights, New Relic, etc.
                // For now, providing mock data that would be realistic
                
                var performance = new SystemPerformanceDto
                {
                    Timestamp = DateTime.UtcNow,
                    CpuUsage = 25.5, // percentage
                    MemoryUsage = 68.2, // percentage
                    DatabaseResponseTime = 45.3, // milliseconds
                    ActiveConnections = 12,
                    TotalRequests = 2547,
                    ErrorCount = 3,
                    ThroughputPerMinute = 85.2,
                    EndpointMetrics = new List<EndpointMetricDto>
                    {
                        new EndpointMetricDto
                        {
                            Endpoint = "/api/reports",
                            Method = "GET",
                            RequestCount = 1250,
                            AverageResponseTime = 180.5,
                            ErrorCount = 1,
                            ErrorRate = 0.08,
                            LastAccessed = DateTime.UtcNow.AddMinutes(-2)
                        },
                        new EndpointMetricDto
                        {
                            Endpoint = "/api/reports",
                            Method = "POST",
                            RequestCount = 345,
                            AverageResponseTime = 420.3,
                            ErrorCount = 2,
                            ErrorRate = 0.58,
                            LastAccessed = DateTime.UtcNow.AddMinutes(-1)
                        },
                        new EndpointMetricDto
                        {
                            Endpoint = "/api/auth/login",
                            Method = "POST",
                            RequestCount = 89,
                            AverageResponseTime = 95.2,
                            ErrorCount = 0,
                            ErrorRate = 0,
                            LastAccessed = DateTime.UtcNow.AddMinutes(-5)
                        }
                    }
                };

                return Task.FromResult(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system performance");
                throw;
            }
        }

        public Task<IEnumerable<EndpointMetricDto>> GetEndpointMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // This would typically come from application monitoring/logging
                // For now, providing mock data that would be realistic
                
                var metrics = new List<EndpointMetricDto>
                {
                    new EndpointMetricDto
                    {
                        Endpoint = "/api/reports",
                        Method = "GET",
                        RequestCount = 5420,
                        AverageResponseTime = 165.8,
                        ErrorCount = 12,
                        ErrorRate = 0.22,
                        LastAccessed = DateTime.UtcNow.AddMinutes(-1)
                    },
                    new EndpointMetricDto
                    {
                        Endpoint = "/api/reports",
                        Method = "POST",
                        RequestCount = 1230,
                        AverageResponseTime = 385.6,
                        ErrorCount = 8,
                        ErrorRate = 0.65,
                        LastAccessed = DateTime.UtcNow.AddMinutes(-3)
                    },
                    new EndpointMetricDto
                    {
                        Endpoint = "/api/reports/stats",
                        Method = "GET",
                        RequestCount = 245,
                        AverageResponseTime = 95.2,
                        ErrorCount = 1,
                        ErrorRate = 0.41,
                        LastAccessed = DateTime.UtcNow.AddMinutes(-10)
                    },
                    new EndpointMetricDto
                    {
                        Endpoint = "/api/auth/login",
                        Method = "POST",
                        RequestCount = 456,
                        AverageResponseTime = 120.5,
                        ErrorCount = 3,
                        ErrorRate = 0.66,
                        LastAccessed = DateTime.UtcNow.AddMinutes(-5)
                    },
                    new EndpointMetricDto
                    {
                        Endpoint = "/api/users",
                        Method = "GET",
                        RequestCount = 890,
                        AverageResponseTime = 78.3,
                        ErrorCount = 2,
                        ErrorRate = 0.22,
                        LastAccessed = DateTime.UtcNow.AddMinutes(-8)
                    }
                };

                return Task.FromResult(metrics.OrderByDescending(m => m.RequestCount).AsEnumerable());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting endpoint metrics");
                throw;
            }
        }

        private async Task<IEnumerable<StatusStatsDto>> GetStatusStatsAsync(DateTime? startDate, DateTime? endDate, UserRole userRole, Department userDepartment)
        {
            try
            {
                var query = _context.Reports.Include(r => r.Creator).AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.CreatedDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(r => r.CreatedDate <= endDate.Value);

                // Filter by user role and department
                if (userRole == UserRole.GeneralStaff)
                {
                    query = query.Where(r => r.Creator.Department == userDepartment);
                }

                var totalReports = await query.CountAsync();
                if (totalReports == 0)
                    return new List<StatusStatsDto>();

                var statusStats = await query
                    .GroupBy(r => r.Status)
                    .Select(g => new StatusStatsDto
                    {
                        Status = g.Key,
                        StatusName = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = (double)g.Count() / totalReports * 100
                    })
                    .ToListAsync();

                // Calculate average time in status (mock calculation for now)
                foreach (var stat in statusStats)
                {
                    switch (stat.Status)
                    {
                        case ReportStatus.Draft:
                            stat.AverageTimeInStatus = 2.5; // days
                            stat.TrendDirection = 0; // stable
                            break;
                        case ReportStatus.Submitted:
                            stat.AverageTimeInStatus = 1.2;
                            stat.TrendDirection = -1; // decreasing (good)
                            break;
                        case ReportStatus.ManagerReview:
                            stat.AverageTimeInStatus = 3.8;
                            stat.TrendDirection = 1; // increasing (attention needed)
                            break;
                        case ReportStatus.GMReview:
                            stat.AverageTimeInStatus = 2.5;
                            stat.TrendDirection = 1; // increasing (attention needed)
                            break;
                        case ReportStatus.Completed:
                            stat.AverageTimeInStatus = 0; // final status
                            stat.TrendDirection = 1; // increasing (good)
                            break;
                        default:
                            stat.AverageTimeInStatus = 0;
                            stat.TrendDirection = 0;
                            break;
                    }
                }

                return statusStats.OrderBy(s => s.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status stats");
                throw;
            }
        }

        private async Task<IEnumerable<PriorityStatsDto>> GetPriorityStatsAsync(DateTime? startDate, DateTime? endDate, UserRole userRole, Department userDepartment)
        {
            try
            {
                var query = _context.Reports.Include(r => r.Creator).AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.CreatedDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(r => r.CreatedDate <= endDate.Value);

                // Filter by user role and department
                if (userRole == UserRole.GeneralStaff)
                {
                    query = query.Where(r => r.Creator.Department == userDepartment);
                }

                var totalReports = await query.CountAsync();
                if (totalReports == 0)
                    return new List<PriorityStatsDto>();

                var now = DateTime.UtcNow;
                var priorityStats = await query
                    .GroupBy(r => r.Priority)
                    .Select(g => new PriorityStatsDto
                    {
                        Priority = g.Key,
                        Count = g.Count(),
                        Percentage = (double)g.Count() / totalReports * 100,
                        OverdueCount = g.Count(r => r.DueDate < now && r.Status != ReportStatus.Completed)
                    })
                    .ToListAsync();

                // Calculate average completion time for each priority
                foreach (var stat in priorityStats)
                {
                    var completedReports = await query
                        .Where(r => r.Priority == stat.Priority 
                                   && r.Status == ReportStatus.Completed 
                                   && r.CompletedDate.HasValue)
                        .Select(r => new { r.CreatedDate, ApprovedDate = r.CompletedDate })
                        .ToListAsync();

                    if (completedReports.Any())
                    {
                        stat.AverageCompletionTime = completedReports
                            .Average(r => (r.ApprovedDate!.Value - r.CreatedDate).TotalDays);
                    }
                }

                return priorityStats.OrderBy(p => p.Priority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting priority stats");
                throw;
            }
        }

        #endregion
    }
}
