-- =====================================================
-- Executive to GM Migration Script - CORRECTED VERSION
-- This script updates all Executive references to GM
-- =====================================================

-- Start transaction for safety
BEGIN TRANSACTION GMUpdate;

PRINT 'Starting Executive to GM migration...';

-- =====================================================
-- 1. UPDATE TABLE COLUMNS
-- =====================================================

PRINT 'Step 1: Updating table columns...';

-- Check if ExecutiveApprovedDate column exists and rename it
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Reports' AND COLUMN_NAME = 'ExecutiveApprovedDate')
BEGIN
    EXEC sp_rename 'Reports.ExecutiveApprovedDate', 'GMApprovedDate', 'COLUMN';
    PRINT 'Renamed ExecutiveApprovedDate to GMApprovedDate';
END
ELSE
BEGIN
    PRINT 'ExecutiveApprovedDate column not found - skipping rename';
END

-- =====================================================
-- 2. UPDATE STORED PROCEDURES
-- =====================================================

PRINT 'Step 2: Updating stored procedures...';

-- Drop existing Executive procedure if it exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetPendingApprovalsForExecutive')
BEGIN
    DROP PROCEDURE GetPendingApprovalsForExecutive;
    PRINT 'Dropped GetPendingApprovalsForExecutive procedure';
END

GO

-- Create new GM procedure
CREATE PROCEDURE GetPendingApprovalsForGM
    @UserId UNIQUEIDENTIFIER,
    @UserRole INT,
    @UserDepartment INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        r.Id,
        r.Title,
        r.Type,
        r.Description,
        r.Status,
        r.Priority,
        r.DueDate,
        r.Department,
        r.CreatedDate,
        r.SubmittedDate,
        r.ManagerApprovedDate,
        r.GMApprovedDate,
        r.CompletedDate,
        r.CreatorId,
        u.FirstName + ' ' + u.LastName AS CreatorName,
        u.Email AS CreatorEmail,
        r.CreatedDate,
        r.LastModifiedDate
    FROM Reports r
    INNER JOIN Users u ON r.CreatorId = u.Id
    WHERE 
        (@UserRole = 3 AND r.Status IN (4, 5)) -- GM can see ManagerApproved(4) and GMReview(5)
        OR 
        (@UserRole = 2 AND r.Department = @UserDepartment AND r.Status = 3) -- LineManager sees ManagerReview(3) from their dept
    ORDER BY r.CreatedDate DESC;
END

GO

-- Update ApproveReport procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ApproveReport')
BEGIN
    DROP PROCEDURE ApproveReport;
END

GO

CREATE PROCEDURE ApproveReport
    @ReportId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UserRole INT,
    @Comments NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentStatus INT;
    DECLARE @NewStatus INT;
    DECLARE @ApprovalDate DATETIME2 = GETUTCDATE();
    
    -- Get current status
    SELECT @CurrentStatus = Status FROM Reports WHERE Id = @ReportId;
    
    -- Determine new status based on user role and current status
    IF @UserRole = 2 -- LineManager
    BEGIN
        IF @CurrentStatus = 3 -- ManagerReview
        BEGIN
            SET @NewStatus = 4; -- ManagerApproved
            UPDATE Reports 
            SET Status = @NewStatus, 
                ManagerApprovedDate = @ApprovalDate,
                LastModifiedDate = @ApprovalDate,
                LastModifiedBy = @UserId
            WHERE Id = @ReportId;
        END
    END
    ELSE IF @UserRole = 3 -- GM (was Executive)
    BEGIN
        IF @CurrentStatus IN (4, 5) -- ManagerApproved or GMReview
        BEGIN
            SET @NewStatus = 6; -- Completed
            UPDATE Reports 
            SET Status = @NewStatus, 
                GMApprovedDate = @ApprovalDate,
                CompletedDate = @ApprovalDate,
                LastModifiedDate = @ApprovalDate,
                LastModifiedBy = @UserId
            WHERE Id = @ReportId;
        END
    END
    
    -- Insert audit log
    INSERT INTO AuditLogs (Id, ReportId, UserId, Action, Details, Timestamp)
    VALUES (NEWID(), @ReportId, @UserId, 4, 
            'Report approved by ' + CASE @UserRole WHEN 2 THEN 'Line Manager' WHEN 3 THEN 'GM' END + 
            CASE WHEN @Comments IS NOT NULL THEN '. Comments: ' + @Comments ELSE '' END, 
            @ApprovalDate);
            
    SELECT @NewStatus AS NewStatus;
END

GO

-- Update RejectReport procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'RejectReport')
BEGIN
    DROP PROCEDURE RejectReport;
END

GO

CREATE PROCEDURE RejectReport
    @ReportId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UserRole INT,
    @Comments NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentStatus INT;
    DECLARE @NewStatus INT;
    DECLARE @RejectionDate DATETIME2 = GETUTCDATE();
    
    -- Get current status
    SELECT @CurrentStatus = Status FROM Reports WHERE Id = @ReportId;
    
    -- Determine new status based on user role
    IF @UserRole = 2 -- LineManager
    BEGIN
        SET @NewStatus = 8; -- ManagerRejected
    END
    ELSE IF @UserRole = 3 -- GM (was Executive)
    BEGIN
        SET @NewStatus = 9; -- GMRejected (was ExecutiveRejected)
    END
    
    -- Update report status
    UPDATE Reports 
    SET Status = @NewStatus,
        LastModifiedDate = @RejectionDate,
        LastModifiedBy = @UserId
    WHERE Id = @ReportId;
    
    -- Insert audit log
    INSERT INTO AuditLogs (Id, ReportId, UserId, Action, Details, Timestamp)
    VALUES (NEWID(), @ReportId, @UserId, 5, 
            'Report rejected by ' + CASE @UserRole WHEN 2 THEN 'Line Manager' WHEN 3 THEN 'GM' END + 
            CASE WHEN @Comments IS NOT NULL THEN '. Reason: ' + @Comments ELSE '' END, 
            @RejectionDate);
            
    SELECT @NewStatus AS NewStatus;
END

GO

-- Update CanUserAccessReport procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'CanUserAccessReport')
BEGIN
    DROP PROCEDURE CanUserAccessReport;
END

GO

CREATE PROCEDURE CanUserAccessReport
    @ReportId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UserRole INT,
    @UserDepartment INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CanAccess BIT = 0;
    DECLARE @CanEdit BIT = 0;
    DECLARE @CanApprove BIT = 0;
    DECLARE @CanReject BIT = 0;
    DECLARE @CanUpload BIT = 0;
    
    DECLARE @ReportCreatorId UNIQUEIDENTIFIER;
    DECLARE @ReportDepartment INT;
    DECLARE @ReportStatus INT;
    
    -- Get report details
    SELECT @ReportCreatorId = CreatorId, @ReportDepartment = Department, @ReportStatus = Status
    FROM Reports WHERE Id = @ReportId;
    
    -- Check access permissions
    IF @UserRole = 1 -- GeneralStaff
    BEGIN
        IF @ReportCreatorId = @UserId
        BEGIN
            SET @CanAccess = 1;
            IF @ReportStatus = 1 -- Draft
                SET @CanEdit = 1;
        END
    END
    ELSE IF @UserRole = 2 -- LineManager
    BEGIN
        IF @ReportCreatorId = @UserId OR @ReportDepartment = @UserDepartment
        BEGIN
            SET @CanAccess = 1;
            IF @ReportStatus IN (1, 3) -- Draft or ManagerReview
            BEGIN
                SET @CanEdit = 1;
                IF @ReportStatus = 3 -- ManagerReview
                BEGIN
                    SET @CanApprove = 1;
                    SET @CanReject = 1;
                    SET @CanUpload = 1;
                END
            END
        END
    END
    ELSE IF @UserRole = 3 -- GM (was Executive)
    BEGIN
        SET @CanAccess = 1; -- GM can access all reports
        IF @ReportStatus IN (1, 4, 5) -- Draft, ManagerApproved, or GMReview
        BEGIN
            SET @CanEdit = 1;
            IF @ReportStatus IN (4, 5) -- ManagerApproved or GMReview
            BEGIN
                SET @CanApprove = 1;
                SET @CanReject = 1;
                SET @CanUpload = 1;
            END
        END
    END
    
    SELECT @CanAccess AS CanAccess, @CanEdit AS CanEdit, @CanApprove AS CanApprove, 
           @CanReject AS CanReject, @CanUpload AS CanUpload;
END

GO

-- =====================================================
-- 3. VERIFICATION QUERIES
-- =====================================================

PRINT 'Step 3: Verification...';

-- Verify column rename
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Reports' AND COLUMN_NAME = 'GMApprovedDate')
BEGIN
    PRINT 'SUCCESS: GMApprovedDate column exists';
END
ELSE
BEGIN
    PRINT 'WARNING: GMApprovedDate column not found';
END

-- Verify procedures
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetPendingApprovalsForGM')
BEGIN
    PRINT 'SUCCESS: GetPendingApprovalsForGM procedure created';
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ApproveReport')
BEGIN
    PRINT 'SUCCESS: ApproveReport procedure updated';
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'RejectReport')
BEGIN
    PRINT 'SUCCESS: RejectReport procedure updated';
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'CanUserAccessReport')
BEGIN
    PRINT 'SUCCESS: CanUserAccessReport procedure updated';
END

-- Commit the transaction
COMMIT TRANSACTION GMUpdate;

PRINT 'Executive to GM migration completed successfully!';
PRINT 'All procedures have been updated to use GM role (3) instead of Executive';
PRINT 'Column ExecutiveApprovedDate has been renamed to GMApprovedDate';
