using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Service interface for compliance reporting and audit analysis
    /// Provides regulatory compliance support and audit trail analysis
    /// </summary>
    public interface IComplianceService
    {
        #region Compliance Reports

        /// <summary>
        /// Generates comprehensive compliance report for regulatory authorities
        /// </summary>
        /// <param name="startDate">Report period start date</param>
        /// <param name="endDate">Report period end date</param>
        /// <param name="reportType">Type of compliance report</param>
        /// <param name="requestedBy">User requesting the report</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>Compliance report data</returns>
        Task<ComplianceReportDto> GenerateComplianceReportAsync(
            DateTime startDate, 
            DateTime endDate, 
            ComplianceReportType reportType, 
            Guid requestedBy, 
            UserRole userRole);

        /// <summary>
        /// Gets audit trail for specific entity or action
        /// </summary>
        /// <param name="entityId">Entity ID (optional)</param>
        /// <param name="auditAction">Audit action (optional)</param>
        /// <param name="userId">User ID (optional)</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <param name="requestedBy">User requesting audit trail</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>Audit trail entries</returns>
        Task<IEnumerable<AuditTrailDto>> GetAuditTrailAsync(
            Guid? entityId = null,
            AuditAction? auditAction = null,
            Guid? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Guid requestedBy = default,
            UserRole userRole = UserRole.GeneralStaff);

        /// <summary>
        /// Analyzes system compliance with regulatory requirements
        /// </summary>
        /// <param name="complianceStandard">Compliance standard to check</param>
        /// <param name="requestedBy">User requesting analysis</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>Compliance analysis results</returns>
        Task<ComplianceAnalysisDto> AnalyzeComplianceAsync(
            ComplianceStandard complianceStandard,
            Guid requestedBy,
            UserRole userRole);

        #endregion

        #region Data Retention

        /// <summary>
        /// Gets data retention policy compliance status
        /// </summary>
        /// <param name="requestedBy">User requesting status</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>Data retention compliance status</returns>
        Task<DataRetentionStatusDto> GetDataRetentionStatusAsync(Guid requestedBy, UserRole userRole);

        /// <summary>
        /// Archives old data according to retention policies
        /// </summary>
        /// <param name="archiveDate">Archive cutoff date</param>
        /// <param name="dryRun">Whether to perform a dry run (no actual archiving)</param>
        /// <param name="requestedBy">User requesting archival</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>Archival results</returns>
        Task<ArchivalResultDto> ArchiveDataAsync(
            DateTime archiveDate, 
            bool dryRun, 
            Guid requestedBy, 
            UserRole userRole);

        #endregion

        #region Security Compliance

        /// <summary>
        /// Generates security compliance report
        /// </summary>
        /// <param name="startDate">Report period start</param>
        /// <param name="endDate">Report period end</param>
        /// <param name="requestedBy">User requesting report</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>Security compliance report</returns>
        Task<SecurityComplianceReportDto> GenerateSecurityComplianceReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid requestedBy,
            UserRole userRole);

        /// <summary>
        /// Checks for potential compliance violations
        /// </summary>
        /// <param name="requestedBy">User requesting check</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>List of potential violations</returns>
        Task<IEnumerable<ComplianceViolationDto>> CheckComplianceViolationsAsync(
            Guid requestedBy,
            UserRole userRole);

        #endregion

        #region Regulatory Reporting

        /// <summary>
        /// Exports compliance data in regulatory format
        /// </summary>
        /// <param name="regulatoryFormat">Format for export</param>
        /// <param name="startDate">Export period start</param>
        /// <param name="endDate">Export period end</param>
        /// <param name="requestedBy">User requesting export</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>Export file information</returns>
        Task<ExportFileDto> ExportRegulatoryDataAsync(
            RegulatoryFormat regulatoryFormat,
            DateTime startDate,
            DateTime endDate,
            Guid requestedBy,
            UserRole userRole);

        /// <summary>
        /// Schedules automated compliance report generation
        /// </summary>
        /// <param name="scheduleDto">Schedule configuration</param>
        /// <param name="requestedBy">User creating schedule</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <returns>Scheduled report information</returns>
        Task<ScheduledReportDto> ScheduleComplianceReportAsync(
            ComplianceReportScheduleDto scheduleDto,
            Guid requestedBy,
            UserRole userRole);

        #endregion
    }
}
