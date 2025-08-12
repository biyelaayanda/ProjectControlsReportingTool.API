-- =============================================
-- Phase 4: Database Integration & Data Persistence
-- Project Controls Reporting Tool
-- Created: August 12, 2025
-- =============================================

USE ProjectControlsReportingToolDB;
GO

-- =============================================
-- Section 1: Database Validation
-- =============================================
PRINT 'Phase 4: Starting Database Setup...';

-- Check if database exists and is accessible
IF DB_NAME() = 'ProjectControlsReportingToolDB'
    PRINT '✓ Database connection successful';
ELSE
    PRINT '✗ Database connection failed';

-- =============================================
-- Section 2: Create Stored Procedures
-- =============================================

-- SP: CreateReport
CREATE OR ALTER PROCEDURE [dbo].[CreateReport]
    @Id UNIQUEIDENTIFIER,
    @Title NVARCHAR(200),
    @Content NVARCHAR(MAX),
    @Description NVARCHAR(500) = NULL,
    @CreatedBy UNIQUEIDENTIFIER,
    @Department INT,
    @ReportNumber NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        -- Generate report number if not provided
        IF @ReportNumber IS NULL
        BEGIN
            DECLARE @DeptCode NVARCHAR(3);
            DECLARE @NextNumber INT;
            DECLARE @Year NVARCHAR(4) = CAST(YEAR(GETUTCDATE()) AS NVARCHAR(4));
            
            -- Get department code
            SET @DeptCode = CASE @Department
                WHEN 1 THEN 'PS'   -- Project Support
                WHEN 2 THEN 'DM'   -- Doc Management
                WHEN 3 THEN 'QS'   -- Quantity Surveying
                WHEN 4 THEN 'CM'   -- Contracts Management
                WHEN 5 THEN 'BA'   -- Business Assurance
                ELSE 'GN'          -- General
            END;
            
            -- Get next number for department and year
            SELECT @NextNumber = ISNULL(MAX(CAST(RIGHT(ReportNumber, 4) AS INT)), 0) + 1
            FROM Reports 
            WHERE ReportNumber LIKE @DeptCode + '-' + @Year + '-%'
            AND LEN(ReportNumber) = 11; -- Format: XX-YYYY-0000
            
            SET @ReportNumber = @DeptCode + '-' + @Year + '-' + RIGHT('0000' + CAST(@NextNumber AS NVARCHAR(4)), 4);
        END
        
        -- Insert the report
        INSERT INTO Reports (
            Id, Title, Content, Description, CreatedBy, Department, Status,
            CreatedDate, LastModifiedDate, ReportNumber
        )
        VALUES (
            @Id, @Title, @Content, @Description, @CreatedBy, @Department, 1, -- Draft status
            GETUTCDATE(), GETUTCDATE(), @ReportNumber
        );
        
        -- Return the created report
        SELECT * FROM Reports WHERE Id = @Id;
        
        COMMIT TRANSACTION
        PRINT '✓ Report created successfully: ' + @ReportNumber;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW;
    END CATCH
END
GO

-- SP: GetReportsByUser
CREATE OR ALTER PROCEDURE [dbo].[GetReportsByUser]
    @UserId UNIQUEIDENTIFIER,
    @Status INT = NULL,
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    SELECT 
        r.*,
        u.FirstName + ' ' + u.LastName AS CreatorName
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.CreatedBy = @UserId
        AND (@Status IS NULL OR r.Status = @Status)
    ORDER BY r.LastModifiedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP: GetReportsByDepartment
CREATE OR ALTER PROCEDURE [dbo].[GetReportsByDepartment]
    @Department INT,
    @Status INT = NULL,
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    SELECT 
        r.*,
        u.FirstName + ' ' + u.LastName AS CreatorName
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.Department = @Department
        AND (@Status IS NULL OR r.Status = @Status)
    ORDER BY r.LastModifiedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP: GetPendingApprovalsForManager
CREATE OR ALTER PROCEDURE [dbo].[GetPendingApprovalsForManager]
    @Department INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.*,
        u.FirstName + ' ' + u.LastName AS CreatorName
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.Department = @Department
        AND r.Status IN (2, 3) -- Submitted, ManagerReview
    ORDER BY r.SubmittedDate ASC;
END
GO

-- SP: GetPendingApprovalsForExecutive
CREATE OR ALTER PROCEDURE [dbo].[GetPendingApprovalsForExecutive]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.*,
        u.FirstName + ' ' + u.LastName AS CreatorName
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.Status IN (4, 5) -- ManagerApproved, ExecutiveReview
    ORDER BY r.ManagerApprovedDate ASC;
END
GO

-- SP: GetReportDetails
CREATE OR ALTER PROCEDURE [dbo].[GetReportDetails]
    @ReportId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.*,
        creator.FirstName + ' ' + creator.LastName AS CreatorName,
        rejected.FirstName + ' ' + rejected.LastName AS RejectedByName
    FROM Reports r
    INNER JOIN Users creator ON r.CreatedBy = creator.Id
    LEFT JOIN Users rejected ON r.RejectedBy = rejected.Id
    WHERE r.Id = @ReportId;
END
GO

-- SP: ApproveReport
CREATE OR ALTER PROCEDURE [dbo].[ApproveReport]
    @ReportId UNIQUEIDENTIFIER,
    @ApprovedBy UNIQUEIDENTIFIER,
    @Comments NVARCHAR(1000) = NULL,
    @SignatureType INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @CurrentStatus INT;
        DECLARE @NewStatus INT;
        DECLARE @ApproverRole INT;
        
        -- Get current status and approver role
        SELECT @CurrentStatus = r.Status, @ApproverRole = u.Role
        FROM Reports r, Users u
        WHERE r.Id = @ReportId AND u.Id = @ApprovedBy;
        
        -- Determine new status
        IF @ApproverRole = 2 AND @CurrentStatus IN (2, 3) -- Line Manager
            SET @NewStatus = 4; -- ManagerApproved
        ELSE IF @ApproverRole = 3 AND @CurrentStatus IN (4, 5) -- Executive
            SET @NewStatus = 6; -- Completed
        ELSE
        BEGIN
            ROLLBACK TRANSACTION
            RAISERROR('Invalid approval workflow', 16, 1);
            RETURN;
        END
        
        -- Update report status
        UPDATE Reports 
        SET Status = @NewStatus,
            LastModifiedDate = GETUTCDATE(),
            ManagerApprovedDate = CASE WHEN @NewStatus = 4 THEN GETUTCDATE() ELSE ManagerApprovedDate END,
            ExecutiveApprovedDate = CASE WHEN @NewStatus = 6 THEN GETUTCDATE() ELSE ExecutiveApprovedDate END,
            CompletedDate = CASE WHEN @NewStatus = 6 THEN GETUTCDATE() ELSE CompletedDate END
        WHERE Id = @ReportId;
        
        -- Add signature
        INSERT INTO ReportSignatures (
            Id, ReportId, UserId, SignatureType, SignedDate, Comments, IsActive
        )
        VALUES (
            NEWID(), @ReportId, @ApprovedBy, @SignatureType, GETUTCDATE(), @Comments, 1
        );
        
        COMMIT TRANSACTION
        PRINT '✓ Report approved successfully';
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW;
    END CATCH
END
GO

-- SP: RejectReport
CREATE OR ALTER PROCEDURE [dbo].[RejectReport]
    @ReportId UNIQUEIDENTIFIER,
    @RejectedBy UNIQUEIDENTIFIER,
    @Reason NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        UPDATE Reports 
        SET Status = 7, -- Rejected
            RejectedBy = @RejectedBy,
            RejectionReason = @Reason,
            RejectedDate = GETUTCDATE(),
            LastModifiedDate = GETUTCDATE()
        WHERE Id = @ReportId;
        
        COMMIT TRANSACTION
        PRINT '✓ Report rejected successfully';
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW;
    END CATCH
END
GO

-- SP: SearchReports
CREATE OR ALTER PROCEDURE [dbo].[SearchReports]
    @SearchTerm NVARCHAR(200) = NULL,
    @Department INT = NULL,
    @Status INT = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    SELECT 
        r.*,
        u.FirstName + ' ' + u.LastName AS CreatorName
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE 
        (@SearchTerm IS NULL OR 
         r.Title LIKE '%' + @SearchTerm + '%' OR 
         r.Content LIKE '%' + @SearchTerm + '%' OR
         r.ReportNumber LIKE '%' + @SearchTerm + '%')
        AND (@Department IS NULL OR r.Department = @Department)
        AND (@Status IS NULL OR r.Status = @Status)
        AND (@FromDate IS NULL OR r.CreatedDate >= @FromDate)
        AND (@ToDate IS NULL OR r.CreatedDate <= @ToDate)
    ORDER BY r.LastModifiedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP: CanUserAccessReport
CREATE OR ALTER PROCEDURE [dbo].[CanUserAccessReport]
    @ReportId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UserRole INT,
    @UserDepartment INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CanAccess BIT = 0;
    DECLARE @ReportCreator UNIQUEIDENTIFIER;
    DECLARE @ReportDepartment INT;
    
    -- Get report details
    SELECT @ReportCreator = CreatedBy, @ReportDepartment = Department
    FROM Reports
    WHERE Id = @ReportId;
    
    -- Check access permissions
    IF @UserRole = 3 -- Executive
        SET @CanAccess = 1;
    ELSE IF @UserRole = 2 AND @ReportDepartment = @UserDepartment -- Line Manager in same department
        SET @CanAccess = 1;
    ELSE IF @ReportCreator = @UserId -- Report creator
        SET @CanAccess = 1;
    
    SELECT @CanAccess AS CanAccess;
END
GO

-- =============================================
-- Section 3: Create Sample Data
-- =============================================

-- Insert sample users if they don't exist
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@projectcontrols.com')
BEGIN
    INSERT INTO Users (Id, FirstName, LastName, Email, Role, Department, IsActive, CreatedDate)
    VALUES 
    (NEWID(), 'System', 'Administrator', 'admin@projectcontrols.com', 3, 1, 1, GETUTCDATE()),
    (NEWID(), 'John', 'Manager', 'john.manager@projectcontrols.com', 2, 1, 1, GETUTCDATE()),
    (NEWID(), 'Jane', 'Staff', 'jane.staff@projectcontrols.com', 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'Mike', 'QS Manager', 'mike.qs@projectcontrols.com', 2, 3, 1, GETUTCDATE()),
    (NEWID(), 'Sarah', 'BA Executive', 'sarah.ba@projectcontrols.com', 3, 5, 1, GETUTCDATE());
    
    PRINT '✓ Sample users created';
END
ELSE
BEGIN
    PRINT '✓ Users already exist';
END

-- =============================================
-- Section 4: Validation & Testing
-- =============================================

-- Test stored procedures
PRINT 'Testing stored procedures...';

-- Count existing data
DECLARE @UserCount INT, @ReportCount INT;
SELECT @UserCount = COUNT(*) FROM Users;
SELECT @ReportCount = COUNT(*) FROM Reports;

PRINT '✓ Database contains ' + CAST(@UserCount AS NVARCHAR(10)) + ' users';
PRINT '✓ Database contains ' + CAST(@ReportCount AS NVARCHAR(10)) + ' reports';

-- Test report creation
DECLARE @TestReportId UNIQUEIDENTIFIER = NEWID();
DECLARE @TestUserId UNIQUEIDENTIFIER;

SELECT TOP 1 @TestUserId = Id FROM Users WHERE Role = 1; -- Get a staff user

IF @TestUserId IS NOT NULL
BEGIN
    EXEC CreateReport 
        @Id = @TestReportId,
        @Title = 'Phase 4 Test Report',
        @Content = 'This is a test report created during Phase 4 database setup.',
        @Description = 'Testing database integration',
        @CreatedBy = @TestUserId,
        @Department = 1;
    
    PRINT '✓ Test report creation successful';
END

PRINT 'Phase 4 Database Setup Complete!';
PRINT '================================';
PRINT 'Next Steps:';
PRINT '1. Test API endpoints with database';
PRINT '2. Verify frontend can communicate with API';
PRINT '3. Test report workflow (create, submit, approve)';
PRINT '4. Implement authentication (Phase 5)';
