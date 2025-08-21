-- =============================================
-- Script: Delete All Reports
-- Description: Safely delete all reports and related data from the database
-- WARNING: This will permanently delete ALL report data!
-- =============================================

USE [ProjectControlsReportingDB]
GO

BEGIN TRANSACTION DeleteAllReports

BEGIN TRY
    PRINT 'Starting deletion of all reports and related data...'
    
    -- Get count of reports before deletion for confirmation
    DECLARE @ReportCount INT
    SELECT @ReportCount = COUNT(*) FROM Reports
    PRINT 'Total reports to delete: ' + CAST(@ReportCount AS VARCHAR(10))
    
    -- Step 1: Delete Report Attachments (if table exists)
    IF OBJECT_ID('ReportAttachments', 'U') IS NOT NULL
    BEGIN
        DELETE FROM ReportAttachments
        PRINT 'Deleted all report attachments'
    END
    
    -- Step 2: Delete Report Signatures
    IF OBJECT_ID('ReportSignatures', 'U') IS NOT NULL
    BEGIN
        DELETE FROM ReportSignatures
        PRINT 'Deleted all report signatures'
    END
    
    -- Step 3: Delete Audit Logs related to reports
    IF OBJECT_ID('AuditLogs', 'U') IS NOT NULL
    BEGIN
        DELETE FROM AuditLogs WHERE ReportId IS NOT NULL
        PRINT 'Deleted all report-related audit logs'
    END
    
    -- Step 4: Delete all Reports (main table)
    DELETE FROM Reports
    PRINT 'Deleted all reports from Reports table'
    
    -- Step 5: Reset identity column if it exists
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Reports') AND is_identity = 1)
    BEGIN
        DBCC CHECKIDENT ('Reports', RESEED, 0)
        PRINT 'Reset Reports identity column'
    END
    
    -- Get final count to confirm deletion
    SELECT @ReportCount = COUNT(*) FROM Reports
    PRINT 'Remaining reports after deletion: ' + CAST(@ReportCount AS VARCHAR(10))
    
    -- Commit the transaction
    COMMIT TRANSACTION DeleteAllReports
    PRINT 'SUCCESS: All reports have been deleted successfully!'
    
END TRY
BEGIN CATCH
    -- Rollback the transaction in case of error
    ROLLBACK TRANSACTION DeleteAllReports
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
    DECLARE @ErrorState INT = ERROR_STATE()
    
    PRINT 'ERROR: Failed to delete reports'
    PRINT 'Error Message: ' + @ErrorMessage
    
    -- Re-raise the error
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
END CATCH

GO

-- Optional: Show final status
PRINT '============================================='
PRINT 'FINAL STATUS:'
PRINT '============================================='

SELECT 
    'Reports' AS TableName,
    COUNT(*) AS RemainingRecords
FROM Reports

UNION ALL

SELECT 
    'ReportSignatures' AS TableName,
    COUNT(*) AS RemainingRecords
FROM ReportSignatures

UNION ALL

SELECT 
    'AuditLogs (Report-related)' AS TableName,
    COUNT(*) AS RemainingRecords
FROM AuditLogs 
WHERE ReportId IS NOT NULL

UNION ALL

SELECT 
    'ReportAttachments' AS TableName,
    CASE 
        WHEN OBJECT_ID('ReportAttachments', 'U') IS NOT NULL 
        THEN (SELECT COUNT(*) FROM ReportAttachments)
        ELSE 0 
    END AS RemainingRecords

PRINT '============================================='
PRINT 'Deletion script completed.'
PRINT '============================================='
