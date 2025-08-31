namespace ProjectControlsReportingTool.API.Models.Enums
{
    public enum UserRole
    {
        GeneralStaff = 1,
        LineManager = 2,
        GM = 3
    }

    public enum Department
    {
        ProjectSupport = 1,
        DocManagement = 2,
        QS = 3,
        ContractsManagement = 4,
        BusinessAssurance = 5
    }

    public enum ReportStatus
    {
        Draft = 1,
        Submitted = 2,
        ManagerReview = 3,
        ManagerApproved = 4,
        GMReview = 5,
        Completed = 6,
        Rejected = 7,          // Generic rejection (for backward compatibility)
        ManagerRejected = 8,   // Specifically rejected by Line Manager
        GMRejected = 9  // Specifically rejected by GM
    }

    public enum SignatureType
    {
        ManagerSignature = 1,
        GMSignature = 2
    }

    public enum AuditAction
    {
        Created = 1,
        Updated = 2,
        Submitted = 3,
        Approved = 4,
        Rejected = 5,
        Signed = 6,
        Downloaded = 7,
        Uploaded = 8,
        Deleted = 9
    }

    public enum ApprovalStage
    {
        Initial = 1,           // Uploaded by report creator during initial creation
        ManagerReview = 2,     // Uploaded by line manager during approval process
        GMReview = 3    // Uploaded by GM during approval process
    }

    // Phase 8: Notification System Enums
    public enum NotificationType
    {
        ReportSubmitted = 1,
        ApprovalRequired = 2,
        ReportApproved = 3,
        ReportRejected = 4,
        DueDateReminder = 5,
        EscalationNotice = 6,
        SystemAlert = 7,
        UserWelcome = 8,
        PasswordReset = 9,
        AccountActivation = 10,
        ReportComment = 11,
        StatusChange = 12,
        BulkUpdate = 13,
        MaintenanceNotice = 14,
        SecurityAlert = 15
    }

    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    public enum NotificationStatus
    {
        Pending = 1,
        Sent = 2,
        Delivered = 3,
        Read = 4,
        Failed = 5,
        Cancelled = 6,
        Expired = 7
    }

    public enum EmailQueueStatus
    {
        Pending = 1,
        Processing = 2,
        Sent = 3,
        Failed = 4,
        Cancelled = 5,
        Expired = 6
    }

    public enum EmailPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    public enum NotificationChannel
    {
        Email = 1,
        InApp = 2,
        Push = 3,
        SMS = 4,
        Webhook = 5
    }
}
