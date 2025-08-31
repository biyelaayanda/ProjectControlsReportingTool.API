using ProjectControlsReportingTool.API.Models.DTOs;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Interface for email notification services
    /// Handles sending email notifications for workflow events, system alerts, and reminders
    /// </summary>
    public interface IEmailNotificationService
    {
        /// <summary>
        /// Send email for workflow events (report submission, approval, rejection)
        /// </summary>
        /// <param name="dto">Workflow email details</param>
        /// <returns>True if email was sent successfully</returns>
        Task<bool> SendWorkflowEmailAsync(WorkflowEmailDto dto);

        /// <summary>
        /// Send system notification email
        /// </summary>
        /// <param name="dto">System email details</param>
        /// <returns>True if email was sent successfully</returns>
        Task<bool> SendSystemNotificationAsync(SystemEmailDto dto);

        /// <summary>
        /// Send bulk email notifications
        /// </summary>
        /// <param name="dto">Bulk email details</param>
        /// <returns>Result of bulk email operation</returns>
        Task<BulkEmailResultDto> SendBulkEmailAsync(BulkEmailDto dto);

        /// <summary>
        /// Send reminder email for due reports
        /// </summary>
        /// <param name="dto">Reminder email details</param>
        /// <returns>True if email was sent successfully</returns>
        Task<bool> SendReminderEmailAsync(ReminderEmailDto dto);

        /// <summary>
        /// Test email configuration
        /// </summary>
        /// <param name="testEmail">Email address to send test email to</param>
        /// <returns>True if test email was sent successfully</returns>
        Task<bool> TestEmailConfigurationAsync(string testEmail);
    }
}
