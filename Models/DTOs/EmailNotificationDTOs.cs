using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    /// <summary>
    /// DTO for workflow-related email notifications
    /// </summary>
    public class WorkflowEmailDto
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string ReportTitle { get; set; } = string.Empty;
        public Guid ReportId { get; set; }
        public string SubmitterName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string? Comments { get; set; }
        public string? ActionUrl { get; set; }
        public WorkflowType WorkflowType { get; set; }
        public WorkflowStatus WorkflowStatus { get; set; }
    }

    /// <summary>
    /// DTO for system notification emails
    /// </summary>
    public class SystemEmailDto
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public NotificationType NotificationType { get; set; }
    }

    /// <summary>
    /// DTO for reminder emails
    /// </summary>
    public class ReminderEmailDto
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string ReportTitle { get; set; } = string.Empty;
        public Guid ReportId { get; set; }
        public DateTime DueDate { get; set; }
        public int? DaysOverdue { get; set; }
        public string? ActionUrl { get; set; }
        public ReminderType ReminderType { get; set; }
    }

    /// <summary>
    /// DTO for bulk email operations
    /// </summary>
    public class BulkEmailDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<EmailRecipientDto> Recipients { get; set; } = new();
    }

    /// <summary>
    /// Email recipient information
    /// </summary>
    public class EmailRecipientDto
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
    }

    /// <summary>
    /// Result of bulk email operation
    /// </summary>
    public class BulkEmailResultDto
    {
        public int TotalEmails { get; set; }
        public int SuccessfulEmails { get; set; }
        public int FailedEmails { get; set; }
        public List<EmailResultDto> Results { get; set; } = new();
    }

    /// <summary>
    /// Individual email result
    /// </summary>
    public class EmailResultDto
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Email configuration test result
    /// </summary>
    public class EmailTestResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TestTime { get; set; }
        public string TestEmail { get; set; } = string.Empty;
    }
}

namespace ProjectControlsReportingTool.API.Models.Enums
{
    /// <summary>
    /// Types of workflow operations
    /// </summary>
    public enum WorkflowType
    {
        ReportSubmission = 1,
        ReportReview = 2,
        ReportApproval = 3,
        UserManagement = 4,
        SystemOperation = 5
    }

    /// <summary>
    /// Workflow status types
    /// </summary>
    public enum WorkflowStatus
    {
        Submitted = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        Cancelled = 5
    }

    /// <summary>
    /// Types of reminder notifications
    /// </summary>
    public enum ReminderType
    {
        ReportDue = 1,
        ReportOverdue = 2,
        ReviewPending = 3,
        ReviewOverdue = 4,
        SystemMaintenance = 5
    }

    /// <summary>
    /// Email delivery status
    /// </summary>
    public enum EmailStatus
    {
        Pending = 1,
        Sent = 2,
        Failed = 3,
        Bounced = 4,
        Delivered = 5
    }
}
