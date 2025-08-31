using Microsoft.AspNetCore.SignalR;
using ProjectControlsReportingTool.API.Hubs;
using ProjectControlsReportingTool.API.Business.Interfaces;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Real-time notification service that integrates with SignalR for instant updates
    /// Handles live notifications for reports, workflows, and system events
    /// </summary>
    public interface IRealTimeNotificationService
    {
        // User-specific notifications
        Task NotifyUserAsync(string userId, string type, object data);
        Task NotifyUserReportStatusAsync(string userId, int reportId, string status, string message);
        Task NotifyUserWorkflowUpdateAsync(string userId, int workflowId, string stage, string message);
        
        // Group notifications
        Task NotifyGroupAsync(string groupName, string type, object data);
        Task NotifyProjectMembersAsync(int projectId, string type, object data);
        Task NotifyDepartmentAsync(string department, string type, object data);
        Task NotifyRoleAsync(string role, string type, object data);
        
        // Broadcast notifications
        Task BroadcastSystemNotificationAsync(string type, object data);
        Task BroadcastMaintenanceNotificationAsync(string message, DateTime? scheduledFor = null);
        
        // Report-specific notifications
        Task NotifyReportSubmittedAsync(int reportId, string submitterName, string reportTitle);
        Task NotifyReportApprovedAsync(int reportId, string approverName, string reportTitle);
        Task NotifyReportRejectedAsync(int reportId, string reviewerName, string reportTitle, string reason);
        Task NotifyReportCommentAsync(int reportId, string commenterName, string comment);
        
        // Workflow notifications
        Task NotifyWorkflowStartedAsync(int workflowId, string workflowName, List<string> assigneeIds);
        Task NotifyWorkflowStageCompletedAsync(int workflowId, string stageName, string completedBy, string nextStage);
        Task NotifyWorkflowDeadlineApproachingAsync(int workflowId, string workflowName, DateTime deadline, List<string> assigneeIds);
        
        // System notifications
        Task NotifyNewUserRegisteredAsync(string adminRole, string newUserName, string newUserEmail);
        Task NotifySystemUpdateAsync(string version, string updateNotes, List<string> features);
        Task NotifyDataExportReadyAsync(string userId, string exportType, string downloadUrl);
        
        // Connection management
        Task<object> GetConnectionStatsAsync();
        Task<bool> IsUserOnlineAsync(string userId);
    }

    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> hubContext;
        private readonly ILogger<RealTimeNotificationService> logger;
        private readonly IEmailService emailService;

        public RealTimeNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<RealTimeNotificationService> logger,
            IEmailService emailService)
        {
            this.hubContext = hubContext;
            this.logger = logger;
            this.emailService = emailService;
        }

        #region User-Specific Notifications

        /// <summary>
        /// Send notification to a specific user
        /// </summary>
        public async Task NotifyUserAsync(string userId, string type, object data)
        {
            try
            {
                var notification = CreateNotification(type, data);
                await hubContext.Clients.User(userId).SendAsync("Notification", notification);
                
                logger.LogDebug("Real-time notification sent to user {UserId}: {Type}", userId, type);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Notify user of report status change with real-time and email notifications
        /// </summary>
        public async Task NotifyUserReportStatusAsync(string userId, int reportId, string status, string message)
        {
            var data = new
            {
                ReportId = reportId,
                Status = status,
                Message = message,
                ActionUrl = $"/reports/{reportId}",
                Priority = GetStatusPriority(status)
            };

            // Send real-time notification
            await NotifyUserAsync(userId, "ReportStatusUpdate", data);

            // Send email notification for important status changes
            if (ShouldSendEmailForStatus(status))
            {
                try
                {
                    var userEmail = await GetUserEmailAsync(userId);
                    var userName = await GetUserNameAsync(userId);
                    
                    var htmlBody = $@"
                        <html>
                        <body>
                            <h2>Report Status Update</h2>
                            <p>Dear {userName},</p>
                            <p>Your report #{reportId} has been updated with the following status: <strong>{status}</strong></p>
                            <p><strong>Message:</strong> {message}</p>
                            <p><a href='{GetBaseUrl()}/reports/{reportId}'>View Report</a></p>
                            <p>Best regards,<br/>Project Controls Reporting Tool</p>
                        </body>
                        </html>";

                    await emailService.SendEmailAsync(
                        to: userEmail,
                        subject: $"Report #{reportId} Status Update: {status}",
                        htmlBody: htmlBody
                    );
                    
                    logger.LogDebug("Email notification sent for report {ReportId} status change to user {UserId}", reportId, userId);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send email notification for report {ReportId} to user {UserId}", reportId, userId);
                }
            }
        }

        /// <summary>
        /// Notify user of workflow update
        /// </summary>
        public async Task NotifyUserWorkflowUpdateAsync(string userId, int workflowId, string stage, string message)
        {
            var data = new
            {
                WorkflowId = workflowId,
                Stage = stage,
                Message = message,
                ActionUrl = $"/workflows/{workflowId}",
                RequiresAction = IsActionRequiredForStage(stage)
            };

            await NotifyUserAsync(userId, "WorkflowUpdate", data);
        }

        #endregion

        #region Group Notifications

        /// <summary>
        /// Send notification to a specific group
        /// </summary>
        public async Task NotifyGroupAsync(string groupName, string type, object data)
        {
            try
            {
                var notification = CreateNotification(type, data);
                await hubContext.Clients.Group(groupName).SendAsync("Notification", notification);
                
                logger.LogDebug("Real-time notification sent to group {GroupName}: {Type}", groupName, type);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending notification to group {GroupName}", groupName);
            }
        }

        /// <summary>
        /// Notify all members of a project
        /// </summary>
        public async Task NotifyProjectMembersAsync(int projectId, string type, object data)
        {
            await NotifyGroupAsync($"Project_{projectId}", type, data);
        }

        /// <summary>
        /// Notify all members of a department
        /// </summary>
        public async Task NotifyDepartmentAsync(string department, string type, object data)
        {
            await NotifyGroupAsync($"Department_{department}", type, data);
        }

        /// <summary>
        /// Notify all users with a specific role
        /// </summary>
        public async Task NotifyRoleAsync(string role, string type, object data)
        {
            await NotifyGroupAsync($"Role_{role}", type, data);
        }

        #endregion

        #region Broadcast Notifications

        /// <summary>
        /// Broadcast system notification to all users
        /// </summary>
        public async Task BroadcastSystemNotificationAsync(string type, object data)
        {
            try
            {
                var notification = CreateNotification(type, data);
                await hubContext.Clients.All.SendAsync("SystemNotification", notification);
                
                logger.LogInformation("System notification broadcasted: {Type}", type);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error broadcasting system notification");
            }
        }

        /// <summary>
        /// Broadcast maintenance notification
        /// </summary>
        public async Task BroadcastMaintenanceNotificationAsync(string message, DateTime? scheduledFor = null)
        {
            var data = new
            {
                Message = message,
                ScheduledFor = scheduledFor,
                Type = "Maintenance",
                Severity = "Warning"
            };

            await BroadcastSystemNotificationAsync("MaintenanceNotice", data);
        }

        #endregion

        #region Report-Specific Notifications

        /// <summary>
        /// Notify when a report is submitted
        /// </summary>
        public async Task NotifyReportSubmittedAsync(int reportId, string submitterName, string reportTitle)
        {
            var data = new
            {
                ReportId = reportId,
                ReportTitle = reportTitle,
                SubmitterName = submitterName,
                SubmittedAt = DateTime.UtcNow,
                ActionUrl = $"/reports/{reportId}",
                Message = $"New report '{reportTitle}' submitted by {submitterName}"
            };

            // Notify reviewers and managers
            await NotifyRoleAsync("Reviewer", "ReportSubmitted", data);
            await NotifyRoleAsync("Manager", "ReportSubmitted", data);
            
            logger.LogInformation("Report submission notification sent for Report {ReportId}", reportId);
        }

        /// <summary>
        /// Notify when a report is approved
        /// </summary>
        public async Task NotifyReportApprovedAsync(int reportId, string approverName, string reportTitle)
        {
            var data = new
            {
                ReportId = reportId,
                ReportTitle = reportTitle,
                ApproverName = approverName,
                ApprovedAt = DateTime.UtcNow,
                ActionUrl = $"/reports/{reportId}",
                Message = $"Report '{reportTitle}' approved by {approverName}"
            };

            // Notify all project members
            await NotifyGroupAsync($"Report_{reportId}", "ReportApproved", data);
            
            logger.LogInformation("Report approval notification sent for Report {ReportId}", reportId);
        }

        /// <summary>
        /// Notify when a report is rejected
        /// </summary>
        public async Task NotifyReportRejectedAsync(int reportId, string reviewerName, string reportTitle, string reason)
        {
            var data = new
            {
                ReportId = reportId,
                ReportTitle = reportTitle,
                ReviewerName = reviewerName,
                Reason = reason,
                RejectedAt = DateTime.UtcNow,
                ActionUrl = $"/reports/{reportId}",
                Message = $"Report '{reportTitle}' requires revision",
                Priority = "High"
            };

            // Notify report submitter and stakeholders
            await NotifyGroupAsync($"Report_{reportId}", "ReportRejected", data);
            
            logger.LogInformation("Report rejection notification sent for Report {ReportId}", reportId);
        }

        /// <summary>
        /// Notify when a comment is added to a report
        /// </summary>
        public async Task NotifyReportCommentAsync(int reportId, string commenterName, string comment)
        {
            var data = new
            {
                ReportId = reportId,
                CommenterName = commenterName,
                Comment = comment.Length > 100 ? comment.Substring(0, 100) + "..." : comment,
                CommentedAt = DateTime.UtcNow,
                ActionUrl = $"/reports/{reportId}",
                Message = $"New comment on report by {commenterName}"
            };

            await NotifyGroupAsync($"Report_{reportId}", "ReportComment", data);
        }

        #endregion

        #region Workflow Notifications

        /// <summary>
        /// Notify when a workflow is started
        /// </summary>
        public async Task NotifyWorkflowStartedAsync(int workflowId, string workflowName, List<string> assigneeIds)
        {
            var data = new
            {
                WorkflowId = workflowId,
                WorkflowName = workflowName,
                StartedAt = DateTime.UtcNow,
                ActionUrl = $"/workflows/{workflowId}",
                Message = $"Workflow '{workflowName}' has been started",
                RequiresAction = true
            };

            // Notify each assignee individually
            foreach (var assigneeId in assigneeIds)
            {
                await NotifyUserAsync(assigneeId, "WorkflowStarted", data);
            }
        }

        /// <summary>
        /// Notify when a workflow stage is completed
        /// </summary>
        public async Task NotifyWorkflowStageCompletedAsync(int workflowId, string stageName, string completedBy, string nextStage)
        {
            var data = new
            {
                WorkflowId = workflowId,
                StageName = stageName,
                CompletedBy = completedBy,
                NextStage = nextStage,
                CompletedAt = DateTime.UtcNow,
                ActionUrl = $"/workflows/{workflowId}",
                Message = $"Stage '{stageName}' completed by {completedBy}. Next: {nextStage}"
            };

            await NotifyGroupAsync($"Workflow_{workflowId}", "WorkflowStageCompleted", data);
        }

        /// <summary>
        /// Notify when workflow deadline is approaching with real-time and email alerts
        /// </summary>
        public async Task NotifyWorkflowDeadlineApproachingAsync(int workflowId, string workflowName, DateTime deadline, List<string> assigneeIds)
        {
            var timeRemaining = deadline - DateTime.UtcNow;
            var data = new
            {
                WorkflowId = workflowId,
                WorkflowName = workflowName,
                Deadline = deadline,
                TimeRemaining = $"{timeRemaining.Days} days, {timeRemaining.Hours} hours",
                ActionUrl = $"/workflows/{workflowId}",
                Message = $"Deadline approaching for '{workflowName}'",
                Priority = timeRemaining.TotalHours < 24 ? "Critical" : "High"
            };

            foreach (var assigneeId in assigneeIds)
            {
                // Send real-time notification
                await NotifyUserAsync(assigneeId, "WorkflowDeadlineApproaching", data);

                // Send email alert for critical deadlines (less than 24 hours)
                if (timeRemaining.TotalHours < 24)
                {
                    try
                    {
                        var userEmail = await GetUserEmailAsync(assigneeId);
                        var userName = await GetUserNameAsync(assigneeId);
                        
                        var htmlBody = $@"
                            <html>
                            <body>
                                <h2 style='color: #d32f2f;'>🚨 Urgent: Workflow Deadline Approaching</h2>
                                <p>Dear {userName},</p>
                                <p><strong>CRITICAL ALERT:</strong> The workflow <strong>'{workflowName}'</strong> has less than 24 hours remaining before its deadline.</p>
                                <p><strong>Deadline:</strong> {deadline:yyyy-MM-dd HH:mm} UTC</p>
                                <p><strong>Time Remaining:</strong> {timeRemaining.Days} days, {timeRemaining.Hours} hours</p>
                                <p><strong>Action Required:</strong> Please complete your assigned tasks immediately.</p>
                                <p><a href='{GetBaseUrl()}/workflows/{workflowId}' style='background-color: #d32f2f; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>View Workflow</a></p>
                                <p>Best regards,<br/>Project Controls Reporting Tool</p>
                            </body>
                            </html>";

                        await emailService.SendEmailAsync(
                            to: userEmail,
                            subject: $"🚨 URGENT: Workflow '{workflowName}' Deadline in {timeRemaining.Hours} hours",
                            htmlBody: htmlBody
                        );
                        
                        logger.LogDebug("Critical deadline email sent for workflow {WorkflowId} to user {UserId}", workflowId, assigneeId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to send deadline email for workflow {WorkflowId} to user {UserId}", workflowId, assigneeId);
                    }
                }
            }
        }

        #endregion

        #region System Notifications

        /// <summary>
        /// Notify admins when a new user registers
        /// </summary>
        public async Task NotifyNewUserRegisteredAsync(string adminRole, string newUserName, string newUserEmail)
        {
            var data = new
            {
                NewUserName = newUserName,
                NewUserEmail = newUserEmail,
                RegisteredAt = DateTime.UtcNow,
                ActionUrl = "/admin/users",
                Message = $"New user registration: {newUserName}",
                RequiresAction = true
            };

            await NotifyRoleAsync(adminRole, "NewUserRegistered", data);
        }

        /// <summary>
        /// Notify about system updates
        /// </summary>
        public async Task NotifySystemUpdateAsync(string version, string updateNotes, List<string> features)
        {
            var data = new
            {
                Version = version,
                UpdateNotes = updateNotes,
                Features = features,
                UpdatedAt = DateTime.UtcNow,
                Message = $"System updated to version {version}"
            };

            await BroadcastSystemNotificationAsync("SystemUpdate", data);
        }

        /// <summary>
        /// Notify when data export is ready
        /// </summary>
        public async Task NotifyDataExportReadyAsync(string userId, string exportType, string downloadUrl)
        {
            var data = new
            {
                ExportType = exportType,
                DownloadUrl = downloadUrl,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                ActionUrl = downloadUrl,
                Message = $"Your {exportType} export is ready for download"
            };

            await NotifyUserAsync(userId, "DataExportReady", data);
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Get current connection statistics
        /// </summary>
        public async Task<object> GetConnectionStatsAsync()
        {
            return await Task.FromResult(NotificationHub.GetConnectionStats());
        }

        /// <summary>
        /// Check if a user is currently online
        /// </summary>
        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            // This is a simplified check - in production, you might want to implement
            // a more sophisticated presence system
            try
            {
                var stats = NotificationHub.GetConnectionStats();
                return await Task.FromResult(true); // Simplified implementation
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Create a standardized notification object
        /// </summary>
        private object CreateNotification(string type, object data)
        {
            return new
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Source = "ProjectControlsReportingTool"
            };
        }

        /// <summary>
        /// Get priority level based on status
        /// </summary>
        private static string GetStatusPriority(string status)
        {
            return status.ToLower() switch
            {
                "rejected" => "High",
                "approved" => "Medium",
                "submitted" => "Medium",
                "in_review" => "Low",
                _ => "Low"
            };
        }

        /// <summary>
        /// Determine if a workflow stage requires user action
        /// </summary>
        private static bool IsActionRequiredForStage(string stage)
        {
            var actionStages = new[] { "review", "approval", "submission", "validation" };
            return actionStages.Any(s => stage.ToLower().Contains(s));
        }

        /// <summary>
        /// Determine if email notification should be sent for this status
        /// </summary>
        private static bool ShouldSendEmailForStatus(string status)
        {
            var emailStatuses = new[] { "approved", "rejected", "submitted", "requires_changes" };
            return emailStatuses.Contains(status.ToLower());
        }

        /// <summary>
        /// Get base URL for email links (placeholder - should be configured)
        /// </summary>
        private static string GetBaseUrl()
        {
            // TODO: This should be configurable from appsettings
            return "https://localhost:4200"; // Default Angular dev server
        }

        /// <summary>
        /// Get user email by ID (placeholder - should integrate with user service)
        /// </summary>
        private async Task<string> GetUserEmailAsync(string userId)
        {
            // TODO: Integrate with actual user service/repository
            await Task.Delay(1); // Placeholder async operation
            return $"user{userId}@company.com"; // Placeholder
        }

        /// <summary>
        /// Get user name by ID (placeholder - should integrate with user service)
        /// </summary>
        private async Task<string> GetUserNameAsync(string userId)
        {
            // TODO: Integrate with actual user service/repository
            await Task.Delay(1); // Placeholder async operation
            return $"User {userId}"; // Placeholder
        }

        #endregion
    }
}
