-- =============================================
-- Rand Water Project Controls Reporting Tool
-- Report Management Stored Procedures
-- Created: August 12, 2025
-- Description: Complete report lifecycle management
-- =============================================

USE ProjectControlsReportingToolDB;
GO

-- =============================================
-- SP: CreateReport
-- Description: Creates a new report
-- =============================================
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
            
            -- Get department code
            SET @DeptCode = CASE @Department
                WHEN 1 THEN 'PS'   -- Project Support
                WHEN 2 THEN 'DM'   -- Doc Management
                WHEN 3 THEN 'QS'   -- Quantity Surveying
                WHEN 4 THEN 'CM'   -- Contracts Management
                WHEN 5 THEN 'BA'   -- Business Assurance
                ELSE 'GN'          -- General
            END;
            
            -- Get next number for department
            SELECT @NextNumber = ISNULL(MAX(CAST(RIGHT(ReportNumber, 4) AS INT)), 0) + 1
            FROM Reports 
            WHERE ReportNumber LIKE @DeptCode + '%'
            AND LEN(ReportNumber) = 11; -- Format: XX-YYYY-0000
            
            SET @ReportNumber = @DeptCode + '-' + CAST(YEAR(GETUTCDATE()) AS NVARCHAR(4)) + '-' + RIGHT('0000' + CAST(@NextNumber AS NVARCHAR(4)), 4);
        END
        
        INSERT INTO Reports (
            Id, Title, Content, Description, Status, CreatedBy, Department,
            CreatedDate, LastModifiedDate, ReportNumber
        )
        VALUES (
            @Id, @Title, @Content, @Description, 1, -- Draft status
            @CreatedBy, @Department, GETUTCDATE(), GETUTCDATE(), @ReportNumber
        );
        
        COMMIT TRANSACTION
        
        -- Return the created report
        SELECT 
            Id, Title, Content, Description, Status, CreatedBy, Department,
            CreatedDate, LastModifiedDate, ReportNumber
        FROM Reports 
        WHERE Id = @Id;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- SP: GetReportsByUser
-- Description: Gets reports created by a specific user
-- =============================================
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
        r.Id, r.Title, r.Description, r.Status, r.Department,
        r.CreatedDate, r.LastModifiedDate, r.ReportNumber,
        u.FirstName + ' ' + u.LastName AS CreatedByName,
        COUNT(*) OVER() AS TotalCount
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.CreatedBy = @UserId
    AND (@Status IS NULL OR r.Status = @Status)
    ORDER BY r.LastModifiedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- SP: GetReportsByDepartment
-- Description: Gets reports for a specific department (for managers)
-- =============================================
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
        r.Id, r.Title, r.Description, r.Status, r.Department,
        r.CreatedDate, r.LastModifiedDate, r.ReportNumber,
        u.FirstName + ' ' + u.LastName AS CreatedByName,
        COUNT(*) OVER() AS TotalCount
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.Department = @Department
    AND (@Status IS NULL OR r.Status = @Status)
    ORDER BY r.LastModifiedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- SP: GetPendingApprovalsForManager
-- Description: Gets reports pending manager approval
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetPendingApprovalsForManager]
    @Department INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.Id, r.Title, r.Description, r.Status, r.Department,
        r.CreatedDate, r.LastModifiedDate, r.SubmittedDate, r.ReportNumber,
        u.FirstName + ' ' + u.LastName AS CreatedByName
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.Department = @Department
    AND r.Status = 3 -- ManagerReview
    ORDER BY r.SubmittedDate ASC;
END
GO

-- =============================================
-- SP: GetPendingApprovalsForExecutive
-- Description: Gets reports pending executive approval
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetPendingApprovalsForExecutive]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.Id, r.Title, r.Description, r.Status, r.Department,
        r.CreatedDate, r.LastModifiedDate, r.ManagerApprovedDate, r.ReportNumber,
        u.FirstName + ' ' + u.LastName AS CreatedByName
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE r.Status = 5 -- ExecutiveReview
    ORDER BY r.ManagerApprovedDate ASC;
END
GO

-- =============================================
-- SP: GetReportDetails
-- Description: Gets detailed report information
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetReportDetails]
    @ReportId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get report details
    SELECT 
        r.Id, r.Title, r.Content, r.Description, r.Status, r.Department,
        r.CreatedDate, r.LastModifiedDate, r.SubmittedDate,
        r.ManagerApprovedDate, r.ExecutiveApprovedDate, r.CompletedDate,
        r.RejectionReason, r.RejectedDate, r.ReportNumber,
        creator.FirstName + ' ' + creator.LastName AS CreatedByName,
        rejector.FirstName + ' ' + rejector.LastName AS RejectedByName
    FROM Reports r
    INNER JOIN Users creator ON r.CreatedBy = creator.Id
    LEFT JOIN Users rejector ON r.RejectedBy = rejector.Id
    WHERE r.Id = @ReportId;
    
    -- Get signatures
    SELECT 
        rs.Id, rs.ReportId, rs.UserId, rs.SignatureType,
        rs.SignedDate, rs.Comments, rs.IsActive,
        u.FirstName + ' ' + u.LastName AS UserName
    FROM ReportSignatures rs
    INNER JOIN Users u ON rs.UserId = u.Id
    WHERE rs.ReportId = @ReportId
    AND rs.IsActive = 1
    ORDER BY rs.SignedDate DESC;
    
    -- Get attachments
    SELECT 
        ra.Id, ra.ReportId, ra.FileName, ra.OriginalFileName,
        ra.ContentType, ra.FileSize, ra.UploadedDate, ra.Description,
        u.FirstName + ' ' + u.LastName AS UploadedByName
    FROM ReportAttachments ra
    INNER JOIN Users u ON ra.UploadedBy = u.Id
    WHERE ra.ReportId = @ReportId
    AND ra.IsActive = 1
    ORDER BY ra.UploadedDate DESC;
END
GO

-- =============================================
-- SP: UpdateReportStatus
-- Description: Updates report status and workflow
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[UpdateReportStatus]
    @ReportId UNIQUEIDENTIFIER,
    @NewStatus INT,
    @UpdatedBy UNIQUEIDENTIFIER,
    @Comments NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @CurrentStatus INT;
        DECLARE @Department INT;
        
        -- Get current status and department
        SELECT @CurrentStatus = Status, @Department = Department
        FROM Reports 
        WHERE Id = @ReportId;
        
        IF @CurrentStatus IS NULL
        BEGIN
            RAISERROR('Report not found', 16, 1);
            RETURN;
        END
        
        -- Update report based on new status
        UPDATE Reports 
        SET 
            Status = @NewStatus,
            LastModifiedDate = GETUTCDATE(),
            SubmittedDate = CASE WHEN @NewStatus = 2 THEN GETUTCDATE() ELSE SubmittedDate END,
            ManagerApprovedDate = CASE WHEN @NewStatus = 4 THEN GETUTCDATE() ELSE ManagerApprovedDate END,
            ExecutiveApprovedDate = CASE WHEN @NewStatus = 6 THEN GETUTCDATE() ELSE ExecutiveApprovedDate END,
            CompletedDate = CASE WHEN @NewStatus = 6 THEN GETUTCDATE() ELSE CompletedDate END
        WHERE Id = @ReportId;
        
        -- Log audit trail
        INSERT INTO AuditLogs (Id, UserId, ReportId, Action, Details, Timestamp)
        VALUES (
            NEWID(), @UpdatedBy, @ReportId, 2, -- Updated action
            CONCAT('Status changed from ', @CurrentStatus, ' to ', @NewStatus, 
                   CASE WHEN @Comments IS NOT NULL THEN '. Comments: ' + @Comments ELSE '' END),
            GETUTCDATE()
        );
        
        COMMIT TRANSACTION
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- SP: ApproveReport
-- Description: Approve a report (manager or executive)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ApproveReport]
    @ReportId UNIQUEIDENTIFIER,
    @ApprovedBy UNIQUEIDENTIFIER,
    @Comments NVARCHAR(1000) = NULL,
    @SignatureType INT -- 1 = Manager, 2 = Executive
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @CurrentStatus INT;
        DECLARE @NewStatus INT;
        
        -- Get current status
        SELECT @CurrentStatus = Status FROM Reports WHERE Id = @ReportId;
        
        -- Determine new status based on signature type
        IF @SignatureType = 1 -- Manager approval
        BEGIN
            IF @CurrentStatus != 3 -- ManagerReview
            BEGIN
                RAISERROR('Report is not in the correct status for manager approval', 16, 1);
                RETURN;
            END
            SET @NewStatus = 5; -- ExecutiveReview
        END
        ELSE IF @SignatureType = 2 -- Executive approval
        BEGIN
            IF @CurrentStatus != 5 -- ExecutiveReview
            BEGIN
                RAISERROR('Report is not in the correct status for executive approval', 16, 1);
                RETURN;
            END
            SET @NewStatus = 6; -- Completed
        END
        
        -- Update report status
        EXEC UpdateReportStatus @ReportId, @NewStatus, @ApprovedBy, @Comments;
        
        -- Add signature
        INSERT INTO ReportSignatures (
            Id, ReportId, UserId, SignatureType, SignedDate, Comments, IsActive
        )
        VALUES (
            NEWID(), @ReportId, @ApprovedBy, @SignatureType, GETUTCDATE(), @Comments, 1
        );
        
        COMMIT TRANSACTION
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- SP: RejectReport
-- Description: Reject a report
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[RejectReport]
    @ReportId UNIQUEIDENTIFIER,
    @RejectedBy UNIQUEIDENTIFIER,
    @Reason NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        -- Update report to rejected status
        UPDATE Reports 
        SET 
            Status = 7, -- Rejected
            RejectionReason = @Reason,
            RejectedBy = @RejectedBy,
            RejectedDate = GETUTCDATE(),
            LastModifiedDate = GETUTCDATE()
        WHERE Id = @ReportId;
        
        -- Log audit trail
        INSERT INTO AuditLogs (Id, UserId, ReportId, Action, Details, Timestamp)
        VALUES (
            NEWID(), @RejectedBy, @ReportId, 2, -- Updated action
            CONCAT('Report rejected. Reason: ', @Reason),
            GETUTCDATE()
        );
        
        COMMIT TRANSACTION
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- SP: SearchReports
-- Description: Advanced report search
-- =============================================
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
        r.Id, r.Title, r.Description, r.Status, r.Department,
        r.CreatedDate, r.LastModifiedDate, r.ReportNumber,
        u.FirstName + ' ' + u.LastName AS CreatedByName,
        COUNT(*) OVER() AS TotalCount
    FROM Reports r
    INNER JOIN Users u ON r.CreatedBy = u.Id
    WHERE 
        (@SearchTerm IS NULL OR r.Title LIKE '%' + @SearchTerm + '%' OR r.Content LIKE '%' + @SearchTerm + '%')
        AND (@Department IS NULL OR r.Department = @Department)
        AND (@Status IS NULL OR r.Status = @Status)
        AND (@FromDate IS NULL OR r.CreatedDate >= @FromDate)
        AND (@ToDate IS NULL OR r.CreatedDate <= @ToDate)
    ORDER BY r.LastModifiedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- SP: CanUserAccessReport
-- Description: Check if user can access a specific report
-- =============================================
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
    
    IF @ReportCreator IS NULL
    BEGIN
        SELECT 0 AS CanAccess;
        RETURN;
    END
    
    -- Check access rules
    IF @UserRole = 3 -- Executive
        SET @CanAccess = 1; -- Executives can see all reports
    ELSE IF @UserRole = 2 -- Line Manager
        SET @CanAccess = CASE WHEN @ReportDepartment = @UserDepartment THEN 1 ELSE 0 END;
    ELSE IF @UserRole = 1 -- General Staff
        SET @CanAccess = CASE WHEN @ReportCreator = @UserId THEN 1 ELSE 0 END;
    
    SELECT @CanAccess AS CanAccess;
END
GO

PRINT 'All report management stored procedures created successfully!';
