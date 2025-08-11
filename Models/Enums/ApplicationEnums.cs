namespace ProjectControlsReportingTool.API.Models.Enums
{
    public enum UserRole
    {
        GeneralStaff = 1,
        LineManager = 2,
        Executive = 3
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
        ExecutiveReview = 5,
        Completed = 6,
        Rejected = 7
    }

    public enum SignatureType
    {
        ManagerSignature = 1,
        ExecutiveSignature = 2
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
        Uploaded = 8
    }
}
