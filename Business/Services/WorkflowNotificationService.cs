using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Phase 7: Workflow Notification Service
    /// Handles automatic email notifications for workflow events
    /// </summary>
    public class WorkflowNotificationService : IWorkflowNotificationService
    {
        private readonly IEmailNotificationService _emailService;
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkflowNotificationService> _logger;

        public WorkflowNotificationService(
            IEmailNotificationService emailService,
            INotificationService notificationService,
            ApplicationDbContext context,
            ILogger<WorkflowNotificationService> logger)
        {
            _emailService = emailService;
            _notificationService = notificationService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Handle report submission workflow
        /// </summary>
        public async Task<bool> HandleReportSubmissionAsync(Guid reportId, Guid submitterId)
        {
            try
            {
                var report = await GetReportWithUserAsync(reportId);
                if (report == null) 
                {
                    return false;
                }

                // Create in-app notification
                await CreateReportSubmissionNotificationAsync(report, submitterId);

                // Send email notification to reviewers
                await SendReportSubmissionEmailsAsync(report);

                _logger.LogInformation("Report submission workflow completed for report {ReportId}", reportId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling report submission workflow for report {ReportId}", reportId);
                return false;
            }
        }

        /// <summary>
        /// Handle report approval workflow
        /// </summary>
        public async Task<bool> HandleReportApprovalAsync(Guid reportId, Guid approverId, string? comments = null)
        {
            try
            {
                var report = await GetReportWithUserAsync(reportId);
                if (report == null) 
                {
                    return false;
                }

                // Create in-app notification
                await CreateReportApprovalNotificationAsync(report, approverId, comments);

                // Send email notification
                await SendReportApprovalEmailAsync(report, comments);

                _logger.LogInformation("Report approval workflow completed for report {ReportId}", reportId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling report approval workflow for report {ReportId}", reportId);
                return false;
            }
        }

        /// <summary>
        /// Handle report rejection workflow
        /// </summary>
        public async Task<bool> HandleReportRejectionAsync(Guid reportId, Guid rejectorId, string? comments = null)
        {
            try
            {
                var report = await GetReportWithUserAsync(reportId);
                if (report == null) 
                {
                    return false;
                }

                // Create in-app notification
                await CreateReportRejectionNotificationAsync(report, rejectorId, comments);

                // Send email notification
                await SendReportRejectionEmailAsync(report, comments);

                _logger.LogInformation("Report rejection workflow completed for report {ReportId}", reportId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling report rejection workflow for report {ReportId}", reportId);
                return false;
            }
        }

        /// <summary>
        /// Send reminder notifications for due reports
        /// </summary>
        public async Task<int> SendDueReportRemindersAsync()
        {
            try
            {
                var remindersSent = 0;
                var reminderThreshold = DateTime.UtcNow.AddDays(3); // 3 days before due

                var dueReports = await _context.Reports
                    .Include(r => r.Creator)
                    .Where(r => r.Status == ReportStatus.Draft &&
                               r.DueDate.HasValue &&
                               r.DueDate.Value <= reminderThreshold &&
                               r.DueDate.Value > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var report in dueReports)
                {
                    // Create in-app notification
                    await CreateDueReminderNotificationAsync(report);

                    // Send email reminder
                    await SendDueReminderEmailAsync(report);

                    remindersSent++;
                }

                _logger.LogInformation("Sent {Count} due report reminders", remindersSent);
                return remindersSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending due report reminders");
                return 0;
            }
        }

        /// <summary>
        /// Send overdue report notifications
        /// </summary>
        public async Task<int> SendOverdueReportNotificationsAsync()
        {
            try
            {
                var notificationsSent = 0;

                var overdueReports = await _context.Reports
                    .Include(r => r.Creator)
                    .Where(r => r.Status == ReportStatus.Draft &&
                               r.DueDate.HasValue &&
                               r.DueDate.Value < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var report in overdueReports)
                {
                    var daysOverdue = (DateTime.UtcNow - report.DueDate!.Value).Days;

                    // Create in-app notification
                    await CreateOverdueNotificationAsync(report, daysOverdue);

                    // Send email notification
                    await SendOverdueReminderEmailAsync(report, daysOverdue);

                    notificationsSent++;
                }

                _logger.LogInformation("Sent {Count} overdue report notifications", notificationsSent);
                return notificationsSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending overdue report notifications");
                return 0;
            }
        }

        /// <summary>
        /// Send review pending reminders
        /// </summary>
        public async Task<int> SendReviewPendingRemindersAsync()
        {
            try
            {
                var remindersSent = 0;

                var pendingReports = await _context.Reports
                    .Include(r => r.Creator)
                    .Where(r => r.Status == ReportStatus.Submitted)
                    .ToListAsync();

                foreach (var report in pendingReports)
                {
                    // Get potential reviewers (line managers and GMs)
                    var reviewers = await GetPotentialReviewersAsync(report.Creator.Department);

                    foreach (var reviewer in reviewers)
                    {
                        // Create in-app notification
                        await CreateReviewPendingNotificationAsync(report, reviewer.Id);

                        // Send email reminder
                        await SendReviewPendingEmailAsync(report, reviewer);

                        remindersSent++;
                    }
                }

                _logger.LogInformation("Sent {Count} review pending reminders", remindersSent);
                return remindersSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending review pending reminders");
                return 0;
            }
        }

        #region Private Helper Methods

        private async Task<dynamic?> GetReportWithUserAsync(Guid reportId)
        {
            return await _context.Reports
                .Include(r => r.Creator)
                .Where(r => r.Id == reportId)
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.DueDate,
                    r.Status,
                    User = new
                    {
                        r.Creator.Id,
                        r.Creator.FirstName,
                        r.Creator.LastName,
                        r.Creator.Email,
                        r.Creator.Department
                    }
                })
                .FirstOrDefaultAsync();
        }

        private async Task<List<ReviewerInfo>> GetPotentialReviewersAsync(Department department)
        {
            return await _context.Users
                .Where(u => u.IsActive && 
                           (u.Role == UserRole.LineManager || u.Role == UserRole.GM) &&
                           u.Department == department)
                .Select(u => new ReviewerInfo
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role,
                    Department = u.Department
                })
                .ToListAsync();
        }
    
        private class ReviewerInfo
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public UserRole Role { get; set; }
            public Department Department { get; set; }
        }

        private async Task CreateReportSubmissionNotificationAsync(dynamic report, Guid submitterId)
        {
            var createDto = new CreateNotificationDto
            {
                Title = "Report Submitted for Review",
                Message = $"Report '{report.Title}' has been submitted and is awaiting review.",
                Type = NotificationType.ReportSubmission,
                Priority = NotificationPriority.Normal,
                RecipientId = report.User.Id,
                Category = "Report Workflow"
            };

            await _notificationService.CreateNotificationAsync(createDto, submitterId);
        }

        private async Task CreateReportApprovalNotificationAsync(dynamic report, Guid approverId, string? comments)
        {
            var createDto = new CreateNotificationDto
            {
                Title = "Report Approved",
                Message = $"Your report '{report.Title}' has been approved." + 
                         (string.IsNullOrEmpty(comments) ? "" : $" Comments: {comments}"),
                Type = NotificationType.ReportApproval,
                Priority = NotificationPriority.High,
                RecipientId = report.User.Id,
                Category = "Report Workflow"
            };

            await _notificationService.CreateNotificationAsync(createDto, approverId);
        }

        private async Task CreateReportRejectionNotificationAsync(dynamic report, Guid rejectorId, string? comments)
        {
            var createDto = new CreateNotificationDto
            {
                Title = "Report Requires Revision",
                Message = $"Your report '{report.Title}' requires revision." + 
                         (string.IsNullOrEmpty(comments) ? "" : $" Feedback: {comments}"),
                Type = NotificationType.ReportRejection,
                Priority = NotificationPriority.High,
                RecipientId = report.User.Id,
                Category = "Report Workflow"
            };

            await _notificationService.CreateNotificationAsync(createDto, rejectorId);
        }

        private async Task CreateDueReminderNotificationAsync(dynamic report)
        {
            var daysUntilDue = report.DueDate.HasValue ? 
                (report.DueDate.Value - DateTime.UtcNow).Days : 0;

            var createDto = new CreateNotificationDto
            {
                Title = "Report Due Soon",
                Message = $"Your report '{report.Title}' is due in {daysUntilDue} days. Please complete and submit it soon.",
                Type = NotificationType.Reminder,
                Priority = NotificationPriority.Normal,
                RecipientId = report.User.Id,
                Category = "Due Date Reminder"
            };

            await _notificationService.CreateNotificationAsync(createDto, report.User.Id);
        }

        private async Task CreateOverdueNotificationAsync(dynamic report, int daysOverdue)
        {
            var createDto = new CreateNotificationDto
            {
                Title = "URGENT: Report Overdue",
                Message = $"Your report '{report.Title}' is {daysOverdue} days overdue. Please submit immediately.",
                Type = NotificationType.Alert,
                Priority = NotificationPriority.Critical,
                RecipientId = report.User.Id,
                Category = "Overdue Alert"
            };

            await _notificationService.CreateNotificationAsync(createDto, report.User.Id);
        }

        private async Task CreateReviewPendingNotificationAsync(dynamic report, Guid reviewerId)
        {
            var createDto = new CreateNotificationDto
            {
                Title = "Report Pending Review",
                Message = $"Report '{report.Title}' is pending your review.",
                Type = NotificationType.ReviewRequired,
                Priority = NotificationPriority.Normal,
                RecipientId = reviewerId,
                Category = "Review Required"
            };

            await _notificationService.CreateNotificationAsync(createDto, report.User.Id);
        }

        private async Task SendReportSubmissionEmailsAsync(dynamic report)
        {
            var reviewers = await GetPotentialReviewersAsync(report.User.Department);

            foreach (var reviewer in reviewers)
            {
                var emailDto = new WorkflowEmailDto
                {
                    RecipientEmail = reviewer.Email,
                    RecipientName = $"{reviewer.FirstName} {reviewer.LastName}",
                    ReportTitle = report.Title,
                    ReportId = report.Id,
                    SubmitterName = $"{report.User.FirstName} {report.User.LastName}",
                    DueDate = report.DueDate,
                    ActionUrl = $"/reports/{report.Id}",
                    WorkflowType = WorkflowType.ReportSubmission,
                    WorkflowStatus = WorkflowStatus.Submitted
                };

                await _emailService.SendWorkflowEmailAsync(emailDto);
            }
        }

        private async Task SendReportApprovalEmailAsync(dynamic report, string? comments)
        {
            var emailDto = new WorkflowEmailDto
            {
                RecipientEmail = report.User.Email,
                RecipientName = $"{report.User.FirstName} {report.User.LastName}",
                ReportTitle = report.Title,
                ReportId = report.Id,
                SubmitterName = $"{report.User.FirstName} {report.User.LastName}",
                DueDate = report.DueDate,
                Comments = comments,
                ActionUrl = $"/reports/{report.Id}",
                WorkflowType = WorkflowType.ReportApproval,
                WorkflowStatus = WorkflowStatus.Approved
            };

            await _emailService.SendWorkflowEmailAsync(emailDto);
        }

        private async Task SendReportRejectionEmailAsync(dynamic report, string? comments)
        {
            var emailDto = new WorkflowEmailDto
            {
                RecipientEmail = report.User.Email,
                RecipientName = $"{report.User.FirstName} {report.User.LastName}",
                ReportTitle = report.Title,
                ReportId = report.Id,
                SubmitterName = $"{report.User.FirstName} {report.User.LastName}",
                DueDate = report.DueDate,
                Comments = comments,
                ActionUrl = $"/reports/{report.Id}",
                WorkflowType = WorkflowType.ReportReview,
                WorkflowStatus = WorkflowStatus.Rejected
            };

            await _emailService.SendWorkflowEmailAsync(emailDto);
        }

        private async Task SendDueReminderEmailAsync(dynamic report)
        {
            var reminderDto = new ReminderEmailDto
            {
                RecipientEmail = report.User.Email,
                RecipientName = $"{report.User.FirstName} {report.User.LastName}",
                ReportTitle = report.Title,
                ReportId = report.Id,
                DueDate = report.DueDate ?? DateTime.UtcNow,
                ActionUrl = $"/reports/{report.Id}",
                ReminderType = ReminderType.ReportDue
            };

            await _emailService.SendReminderEmailAsync(reminderDto);
        }

        private async Task SendOverdueReminderEmailAsync(dynamic report, int daysOverdue)
        {
            var reminderDto = new ReminderEmailDto
            {
                RecipientEmail = report.User.Email,
                RecipientName = $"{report.User.FirstName} {report.User.LastName}",
                ReportTitle = report.Title,
                ReportId = report.Id,
                DueDate = report.DueDate ?? DateTime.UtcNow,
                DaysOverdue = daysOverdue,
                ActionUrl = $"/reports/{report.Id}",
                ReminderType = ReminderType.ReportOverdue
            };

            await _emailService.SendReminderEmailAsync(reminderDto);
        }

        private async Task SendReviewPendingEmailAsync(dynamic report, ReviewerInfo reviewer)
        {
            var reminderDto = new ReminderEmailDto
            {
                RecipientEmail = reviewer.Email,
                RecipientName = $"{reviewer.FirstName} {reviewer.LastName}",
                ReportTitle = report.Title,
                ReportId = report.Id,
                DueDate = report.DueDate ?? DateTime.UtcNow,
                ActionUrl = $"/reports/{report.Id}",
                ReminderType = ReminderType.ReviewPending
            };

            await _emailService.SendReminderEmailAsync(reminderDto);
        }

        #endregion
    }
}
