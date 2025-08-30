-- Migration Script: Update Executive references to GM
-- Date: $(date)
-- Description: This script updates all Executive references to GM (General Manager) in the database

-- =============================================================================
-- BACKUP SCRIPT FIRST - ALWAYS RUN THIS BEFORE APPLYING CHANGES
-- =============================================================================
-- BACKUP DATABASE [ProjectControlsReportingTool] TO DISK = 'C:\Backups\ProjectControlsReportingTool_BeforeGM_Migration.bak'

BEGIN TRANSACTION GMUpdate;

-- =============================================================================
-- 1. UPDATE COLUMN NAME: ExecutiveApprovedDate to GMApprovedDate
-- =============================================================================
PRINT 'Step 1: Updating column name ExecutiveApprovedDate to GMApprovedDate...'

-- Check if column exists before renaming
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reports]') AND name = 'ExecutiveApprovedDate')
BEGIN
    EXEC sp_rename 'Reports.ExecutiveApprovedDate', 'GMApprovedDate', 'COLUMN';
    PRINT 'Column ExecutiveApprovedDate renamed to GMApprovedDate successfully.'
END
ELSE
BEGIN
    PRINT 'Column ExecutiveApprovedDate not found. Skipping rename.'
END

-- =============================================================================
-- 2. UPDATE ENUM VALUES IN DATABASE
-- =============================================================================
PRINT 'Step 2: Updating enum values in Reports table...'

-- Update ReportStatus enum values: ExecutiveReview (5) remains the same but represents GMReview
-- Update ReportStatus enum values: ExecutiveRejected (9) remains the same but represents GMRejected
-- The enum values remain the same, only the meaning changes

-- Update any status descriptions or comments that might reference Executive
UPDATE Reports 
SET RejectionReason = REPLACE(RejectionReason, 'Executive', 'GM')
WHERE RejectionReason LIKE '%Executive%';

PRINT 'Updated rejection reasons containing Executive references.'

-- =============================================================================
-- 3. UPDATE USER ROLES - Executive (3) remains the same but represents GM
-- =============================================================================
PRINT 'Step 3: Updating user role references...'

-- The UserRole enum value (3) remains the same, only the meaning changes from Executive to GM
-- Update any user names or titles that contain "Executive"
UPDATE Users 
SET Name = REPLACE(Name, 'Executive', 'GM')
WHERE Name LIKE '%Executive%' AND Role = 3;

UPDATE Users 
SET Name = REPLACE(Name, 'BA Executive', 'BA GM')
WHERE Name LIKE '%BA Executive%' AND Role = 3;

PRINT 'Updated user names containing Executive references.'

-- =============================================================================
-- 4. UPDATE STORED PROCEDURES
-- =============================================================================
PRINT 'Step 4: Updating stored procedures...'

-- Update GetPendingApprovalsForExecutive to GetPendingApprovalsForGM
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetPendingApprovalsForExecutive')
BEGIN
    DROP PROCEDURE [dbo].[GetPendingApprovalsForExecutive];
    PRINT 'Dropped old GetPendingApprovalsForExecutive procedure.'
END

-- Create new GetPendingApprovalsForGM procedure
CREATE PROCEDURE [dbo].[GetPendingApprovalsForGM]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.Id,
        r.Title,
        r.Content,
        r.Description,
        r.Type,
        r.Priority,
        r.DueDate,
        r.Status,
        r.CreatedBy,
        r.CreatedDate,
        r.LastModifiedDate,
        r.SubmittedDate,
        r.ManagerApprovedDate,
        r.GMApprovedDate,
        r.CompletedDate,
        r.RejectionReason,
        r.RejectedBy,
        r.RejectedDate,
        r.ReportNumber,
        r.Department,
        u.Name as CreatorName,
        u.Email as CreatorEmail,
        u.Role as CreatorRole
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.Status = 5 -- GMReview (formerly ExecutiveReview)
       OR (r.Status = 4 AND u.Role = 2) -- ManagerApproved reports from LineManagers
    ORDER BY r.CreatedDate DESC;
END;

PRINT 'Created new GetPendingApprovalsForGM procedure.'

-- Update UpdateReportStatus procedure to use new column name
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'UpdateReportStatus')
BEGIN
    DROP PROCEDURE [dbo].[UpdateReportStatus];
END

CREATE PROCEDURE [dbo].[UpdateReportStatus]
    @ReportId UNIQUEIDENTIFIER,
    @NewStatus INT,
    @UserId UNIQUEIDENTIFIER,
    @Comments NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentStatus INT;
    DECLARE @ApproverRole INT;
    
    -- Get current status and approver role
    SELECT @CurrentStatus = r.Status, @ApproverRole = u.Role
    FROM Reports r
    INNER JOIN Users u ON u.Id = @UserId
    WHERE r.Id = @ReportId;
    
    -- Validate status transition
    IF @ApproverRole = 2 AND @CurrentStatus = 2 -- LineManager approving Submitted
    BEGIN
        -- Line Manager can approve submitted reports
        IF @NewStatus = 4 -- ManagerApproved
        BEGIN
            SET @NewStatus = 5; -- GMReview (forward to GM)
        END
    END
    ELSE IF @ApproverRole = 3 AND @CurrentStatus IN (4, 5) -- GM approving ManagerApproved or GMReview
    BEGIN
        -- GM can give final approval
        IF @NewStatus = 6 -- Completed
        BEGIN
            -- Final approval
        END
    END
    
    -- Update report status
    UPDATE Reports 
    SET Status = @NewStatus,
        LastModifiedDate = GETUTCDATE(),
        ManagerApprovedDate = CASE WHEN @NewStatus = 4 THEN GETUTCDATE() ELSE ManagerApprovedDate END,
        GMApprovedDate = CASE WHEN @NewStatus = 6 THEN GETUTCDATE() ELSE GMApprovedDate END,
        CompletedDate = CASE WHEN @NewStatus = 6 THEN GETUTCDATE() ELSE CompletedDate END,
        RejectionReason = CASE WHEN @NewStatus IN (7, 8, 9) THEN @Comments ELSE RejectionReason END,
        RejectedBy = CASE WHEN @NewStatus IN (7, 8, 9) THEN @UserId ELSE RejectedBy END,
        RejectedDate = CASE WHEN @NewStatus IN (7, 8, 9) THEN GETUTCDATE() ELSE RejectedDate END
    WHERE Id = @ReportId;
    
    -- Log the action
    INSERT INTO AuditLogs (Id, ReportId, UserId, Action, Details, Timestamp)
    VALUES (NEWID(), @ReportId, @UserId, 
        CASE 
            WHEN @NewStatus = 6 THEN 4 -- Approved
            WHEN @NewStatus IN (7, 8, 9) THEN 5 -- Rejected
            ELSE 2 -- Updated
        END,
        COALESCE(@Comments, 'Status updated to ' + CAST(@NewStatus AS NVARCHAR(10))),
        GETUTCDATE());
END;

PRINT 'Updated UpdateReportStatus procedure with GM column reference.'

-- Update ValidateReportUpload procedure
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'ValidateReportUpload')
BEGIN
    DROP PROCEDURE [dbo].[ValidateReportUpload];
END

CREATE PROCEDURE [dbo].[ValidateReportUpload]
    @ReportId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @CanUpload BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ReportStatus INT;
    DECLARE @UserRole INT;
    DECLARE @UserDept INT;
    DECLARE @ReportDept INT;
    DECLARE @CreatorRole INT;
    
    SELECT 
        @ReportStatus = r.Status,
        @UserRole = u.Role,
        @UserDept = u.Department,
        @ReportDept = r.Department,
        @CreatorRole = creator.Role
    FROM Reports r
    INNER JOIN Users u ON u.Id = @UserId
    INNER JOIN Users creator ON creator.Id = r.CreatedBy
    WHERE r.Id = @ReportId;
    
    SET @CanUpload = 0;
    
    -- Business rules for document upload
    IF @UserRole = 3 -- GM
    BEGIN
        -- GMs can upload when report is manager-approved or submitted by line managers
        IF (@ReportStatus = 4) -- ManagerApproved
           OR (@ReportStatus = 2 AND @CreatorRole = 2) -- Submitted by LineManager
           OR (@ReportStatus = 5) -- GMReview
        BEGIN
            SET @CanUpload = 1;
        END
    END
    ELSE IF @UserRole = 2 -- LineManager
    BEGIN
        -- Line managers can upload for their department reports when submitted
        IF @ReportStatus = 2 AND @UserDept = @ReportDept
        BEGIN
            SET @CanUpload = 1;
        END
    END
END;

PRINT 'Updated ValidateReportUpload procedure with GM logic.'

-- =============================================================================
-- 5. UPDATE ANY OTHER REFERENCES
-- =============================================================================
PRINT 'Step 5: Updating any additional references...'

-- Update any stored comments or descriptions that might reference Executive
UPDATE AuditLogs 
SET Details = REPLACE(Details, 'Executive', 'GM')
WHERE Details LIKE '%Executive%';

UPDATE AuditLogs 
SET Details = REPLACE(Details, 'executive', 'GM')
WHERE Details LIKE '%executive%';

PRINT 'Updated audit log details containing Executive references.'

-- =============================================================================
-- 6. VERIFICATION QUERIES
-- =============================================================================
PRINT 'Step 6: Running verification queries...'

-- Verify column rename
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reports]') AND name = 'GMApprovedDate')
BEGIN
    PRINT '✓ GMApprovedDate column exists.'
END
ELSE
BEGIN
    PRINT '✗ GMApprovedDate column missing!'
END

-- Verify procedure creation
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetPendingApprovalsForGM')
BEGIN
    PRINT '✓ GetPendingApprovalsForGM procedure exists.'
END
ELSE
BEGIN
    PRINT '✗ GetPendingApprovalsForGM procedure missing!'
END

-- Count updated records
DECLARE @UpdatedUsers INT, @UpdatedLogs INT;
SELECT @UpdatedUsers = COUNT(*) FROM Users WHERE Name LIKE '%GM%' AND Role = 3;
SELECT @UpdatedLogs = COUNT(*) FROM AuditLogs WHERE Details LIKE '%GM%';

PRINT 'Updated ' + CAST(@UpdatedUsers AS NVARCHAR(10)) + ' user records.'
PRINT 'Updated ' + CAST(@UpdatedLogs AS NVARCHAR(10)) + ' audit log records.'

-- =============================================================================
-- COMMIT OR ROLLBACK
-- =============================================================================
PRINT 'Migration completed successfully!'
PRINT 'Review the changes above. If everything looks correct, run: COMMIT TRANSACTION GMUpdate;'
PRINT 'If you need to rollback, run: ROLLBACK TRANSACTION GMUpdate;'

-- Uncomment the next line to auto-commit (be careful!)
-- COMMIT TRANSACTION GMUpdate;

-- For now, we'll leave the transaction open for manual review
-- COMMIT TRANSACTION GMUpdate;
