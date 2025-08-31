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
        Deleted = 9,
        Exported = 10,
        Viewed = 11,
        Imported = 12
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
        SecurityAlert = 15,
        SystemMaintenance = 16,
        SystemUpdate = 17,
        ReportSubmission = 18,
        ReportApproval = 19,
        ReportRejection = 20,
        Reminder = 21,
        Alert = 22,
        ReviewRequired = 23
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

    // Phase 9: Compliance and Security Enums
    public enum ComplianceReportType
    {
        GDPR = 1,
        POPIA = 2,
        SOX = 3,
        ISO27001 = 4,
        PCI_DSS = 5,
        HIPAA = 6,
        CustomCompliance = 7,
        SecurityAudit = 8,
        DataRetention = 9,
        PrivacyImpact = 10,
        FullAudit = 11,
        Security = 12,
        AccessControl = 13,
        Performance = 14,
        Regulatory = 15
    }

    public enum ComplianceStandard
    {
        GDPR = 1,
        POPIA = 2,
        SOX = 3,
        ISO27001 = 4,
        PCI_DSS = 5,
        HIPAA = 6,
        NIST = 7,
        Custom = 8,
        SOC2 = 9,
        Internal = 10
    }

    public enum ComplianceStatus
    {
        Compliant = 1,
        NonCompliant = 2,
        PartiallyCompliant = 3,
        UnderReview = 4,
        NotApplicable = 5,
        RequiresAction = 6
    }

    public enum RiskLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum SecurityEventType
    {
        LoginFailure = 1,
        UnauthorizedAccess = 2,
        DataBreach = 3,
        PasswordReset = 4,
        AccountLockout = 5,
        PrivilegeEscalation = 6,
        DataExfiltration = 7,
        MaliciousActivity = 8,
        SystemAnomaly = 9,
        ComplianceViolation = 10
    }

    public enum RegulatoryFormat
    {
        PDF = 1,
        Excel = 2,
        CSV = 3,
        JSON = 4,
        XML = 5,
        Custom = 6,
        AuditXML = 7,
        ComplianceCSV = 8,
        RegulatoryJSON = 9,
        SOXFormat = 10,
        GDPRFormat = 11
    }

    // Webhook System Enums
    public enum WebhookEventType
    {
        ReportCreated = 1,
        ReportUpdated = 2,
        ReportSubmitted = 3,
        ReportApproved = 4,
        ReportRejected = 5,
        UserCreated = 6,
        UserUpdated = 7,
        NotificationSent = 8,
        ComplianceAlert = 9,
        SecurityEvent = 10,
        SystemHealthCheck = 11,
        DataExport = 12,
        Custom = 99
    }

    public enum WebhookStatus
    {
        Active = 1,
        Inactive = 2,
        Failed = 3,
        Suspended = 4
    }

    public enum WebhookDeliveryStatus
    {
        Pending = 1,
        Delivered = 2,
        Failed = 3,
        Retrying = 4,
        Cancelled = 5,
        MaxRetriesReached = 6
    }
}
