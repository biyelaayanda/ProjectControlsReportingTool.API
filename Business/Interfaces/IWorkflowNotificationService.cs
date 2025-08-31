namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Interface for workflow notification services
    /// Handles automatic notifications for workflow events like report submission, approval, rejection
    /// </summary>
    public interface IWorkflowNotificationService
    {
        /// <summary>
        /// Handle report submission workflow notifications
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="submitterId">User ID who submitted the report</param>
        /// <returns>True if notifications were sent successfully</returns>
        Task<bool> HandleReportSubmissionAsync(Guid reportId, Guid submitterId);

        /// <summary>
        /// Handle report approval workflow notifications
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="approverId">User ID who approved the report</param>
        /// <param name="comments">Optional approval comments</param>
        /// <returns>True if notifications were sent successfully</returns>
        Task<bool> HandleReportApprovalAsync(Guid reportId, Guid approverId, string? comments = null);

        /// <summary>
        /// Handle report rejection workflow notifications
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="rejectorId">User ID who rejected the report</param>
        /// <param name="comments">Optional rejection comments</param>
        /// <returns>True if notifications were sent successfully</returns>
        Task<bool> HandleReportRejectionAsync(Guid reportId, Guid rejectorId, string? comments = null);

        /// <summary>
        /// Send reminder notifications for reports due soon
        /// </summary>
        /// <returns>Number of reminders sent</returns>
        Task<int> SendDueReportRemindersAsync();

        /// <summary>
        /// Send notifications for overdue reports
        /// </summary>
        /// <returns>Number of notifications sent</returns>
        Task<int> SendOverdueReportNotificationsAsync();

        /// <summary>
        /// Send reminders for reports pending review
        /// </summary>
        /// <returns>Number of reminders sent</returns>
        Task<int> SendReviewPendingRemindersAsync();
    }
}
