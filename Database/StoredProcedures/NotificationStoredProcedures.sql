-- =============================================
-- Notification System Stored Procedures
-- Description: Stored procedures for notification CRUD operations
-- Version: 1.0
-- Date: 2025-01-01
-- =============================================

USE [ProjectControlsReportingToolDB]
GO

-- =============================================
-- SP: GetNotifications
-- Description: Get notifications for a specific user with filtering and pagination
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetNotifications]
    @UserId UNIQUEIDENTIFIER,
    @Status INT = NULL, -- NotificationStatus filter (optional)
    @Type INT = NULL, -- NotificationType filter (optional)
    @IsRead BIT = NULL, -- Read status filter (optional)
    @Category NVARCHAR(100) = NULL, -- Category filter (optional)
    @Page INT = 1,
    @PageSize INT = 20,
    @OrderBy NVARCHAR(50) = 'CreatedDate', -- CreatedDate, Priority, Title
    @OrderDirection NVARCHAR(4) = 'DESC' -- ASC or DESC
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    -- Get notifications with filters
    SELECT 
        n.Id,
        n.Title,
        n.Message,
        n.Type,
        n.Priority,
        n.Status,
        n.Category,
        n.ActionUrl,
        n.ActionText,
        n.CreatedDate,
        n.ReadDate,
        n.IsRead,
        n.RelatedReportId,
        CASE WHEN n.SenderId IS NOT NULL THEN 
            u.FirstName + ' ' + u.LastName 
        ELSE 'System' END AS SenderName,
        CASE WHEN n.RelatedReportId IS NOT NULL THEN 
            r.Title 
        ELSE NULL END AS RelatedReportTitle,
        COUNT(*) OVER() AS TotalCount
    FROM Notifications n
    LEFT JOIN Users u ON n.SenderId = u.Id
    LEFT JOIN Reports r ON n.RelatedReportId = r.Id
    WHERE 
        n.RecipientId = @UserId
        AND n.IsDeleted = 0
        AND (@Status IS NULL OR n.Status = @Status)
        AND (@Type IS NULL OR n.Type = @Type)
        AND (@IsRead IS NULL OR n.IsRead = @IsRead)
        AND (@Category IS NULL OR n.Category = @Category)
    ORDER BY 
        CASE WHEN @OrderBy = 'CreatedDate' AND @OrderDirection = 'DESC' THEN n.CreatedDate END DESC,
        CASE WHEN @OrderBy = 'CreatedDate' AND @OrderDirection = 'ASC' THEN n.CreatedDate END ASC,
        CASE WHEN @OrderBy = 'Priority' AND @OrderDirection = 'DESC' THEN n.Priority END DESC,
        CASE WHEN @OrderBy = 'Priority' AND @OrderDirection = 'ASC' THEN n.Priority END ASC,
        CASE WHEN @OrderBy = 'Title' AND @OrderDirection = 'DESC' THEN n.Title END DESC,
        CASE WHEN @OrderBy = 'Title' AND @OrderDirection = 'ASC' THEN n.Title END ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- SP: GetNotificationById
-- Description: Get a specific notification by ID
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetNotificationById]
    @NotificationId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        n.Id,
        n.Title,
        n.Message,
        n.Type,
        n.Priority,
        n.Status,
        n.Category,
        n.ActionUrl,
        n.ActionText,
        n.CreatedDate,
        n.ReadDate,
        n.IsRead,
        n.RelatedReportId,
        n.AdditionalData,
        CASE WHEN n.SenderId IS NOT NULL THEN 
            u.FirstName + ' ' + u.LastName 
        ELSE 'System' END AS SenderName,
        CASE WHEN n.RelatedReportId IS NOT NULL THEN 
            r.Title 
        ELSE NULL END AS RelatedReportTitle
    FROM Notifications n
    LEFT JOIN Users u ON n.SenderId = u.Id
    LEFT JOIN Reports r ON n.RelatedReportId = r.Id
    WHERE 
        n.Id = @NotificationId
        AND n.RecipientId = @UserId
        AND n.IsDeleted = 0;
END
GO

-- =============================================
-- SP: CreateNotification
-- Description: Create a new notification
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[CreateNotification]
    @Id UNIQUEIDENTIFIER = NULL,
    @Title NVARCHAR(200),
    @Message NVARCHAR(2000),
    @Type INT, -- NotificationType
    @Priority INT, -- NotificationPriority
    @RecipientId UNIQUEIDENTIFIER,
    @SenderId UNIQUEIDENTIFIER = NULL,
    @RelatedReportId UNIQUEIDENTIFIER = NULL,
    @Category NVARCHAR(100) = NULL,
    @ActionUrl NVARCHAR(500) = NULL,
    @ActionText NVARCHAR(100) = NULL,
    @ScheduledDate DATETIME2 = NULL,
    @ExpiryDate DATETIME2 = NULL,
    @AdditionalData NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        -- Generate ID if not provided
        IF @Id IS NULL
            SET @Id = NEWID();
        
        INSERT INTO Notifications (
            Id, Title, Message, Type, Priority, RecipientId, SenderId, 
            RelatedReportId, Category, ActionUrl, ActionText, 
            ScheduledDate, ExpiryDate, AdditionalData, CreatedDate
        )
        VALUES (
            @Id, @Title, @Message, @Type, @Priority, @RecipientId, @SenderId,
            @RelatedReportId, @Category, @ActionUrl, @ActionText,
            @ScheduledDate, @ExpiryDate, @AdditionalData, GETUTCDATE()
        );
        
        -- Log creation in history
        INSERT INTO NotificationHistory (
            NotificationId, Status, StatusMessage, Channel, CreatedDate
        )
        VALUES (
            @Id, 1, 'Notification created', 'System', GETUTCDATE()
        );
        
        COMMIT TRANSACTION
        
        -- Return the created notification
        SELECT @Id AS Id;
        
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
-- SP: MarkNotificationAsRead
-- Description: Mark a notification as read
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[MarkNotificationAsRead]
    @NotificationId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @RecipientId UNIQUEIDENTIFIER;
        DECLARE @IsAlreadyRead BIT;
        
        -- Check if notification exists and belongs to user
        SELECT @RecipientId = RecipientId, @IsAlreadyRead = IsRead
        FROM Notifications 
        WHERE Id = @NotificationId AND IsDeleted = 0;
        
        IF @RecipientId IS NULL
        BEGIN
            RAISERROR('Notification not found', 16, 1);
            RETURN;
        END
        
        IF @RecipientId != @UserId
        BEGIN
            RAISERROR('Unauthorized access to notification', 16, 1);
            RETURN;
        END
        
        -- Update notification if not already read
        IF @IsAlreadyRead = 0
        BEGIN
            UPDATE Notifications 
            SET 
                IsRead = 1,
                ReadDate = GETUTCDATE(),
                Status = 4 -- Read status
            WHERE Id = @NotificationId;
            
            -- Log the read action
            INSERT INTO NotificationHistory (
                NotificationId, Status, StatusMessage, Channel, CreatedDate
            )
            VALUES (
                @NotificationId, 4, 'Notification marked as read', 'InApp', GETUTCDATE()
            );
        END
        
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
-- SP: DeleteNotification
-- Description: Soft delete a notification
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[DeleteNotification]
    @NotificationId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @RecipientId UNIQUEIDENTIFIER;
        
        -- Check if notification exists and belongs to user
        SELECT @RecipientId = RecipientId
        FROM Notifications 
        WHERE Id = @NotificationId AND IsDeleted = 0;
        
        IF @RecipientId IS NULL
        BEGIN
            RAISERROR('Notification not found', 16, 1);
            RETURN;
        END
        
        IF @RecipientId != @UserId
        BEGIN
            RAISERROR('Unauthorized access to notification', 16, 1);
            RETURN;
        END
        
        -- Soft delete the notification
        UPDATE Notifications 
        SET IsDeleted = 1
        WHERE Id = @NotificationId;
        
        -- Log the deletion
        INSERT INTO NotificationHistory (
            NotificationId, Status, StatusMessage, Channel, CreatedDate
        )
        VALUES (
            @NotificationId, 6, 'Notification deleted by user', 'InApp', GETUTCDATE()
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
-- SP: GetNotificationStats
-- Description: Get notification statistics for a user
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetNotificationStats]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) AS Total,
        SUM(CASE WHEN IsRead = 0 THEN 1 ELSE 0 END) AS Unread,
        SUM(CASE WHEN IsRead = 1 THEN 1 ELSE 0 END) AS [Read],
        SUM(CASE WHEN Priority = 4 THEN 1 ELSE 0 END) AS Critical, -- Critical priority
        SUM(CASE WHEN Priority = 3 THEN 1 ELSE 0 END) AS [High], -- High priority
        SUM(CASE WHEN CreatedDate >= DATEADD(day, -7, GETUTCDATE()) THEN 1 ELSE 0 END) AS LastWeek,
        SUM(CASE WHEN CreatedDate >= DATEADD(day, -1, GETUTCDATE()) THEN 1 ELSE 0 END) AS LastDay
    FROM Notifications
    WHERE RecipientId = @UserId AND IsDeleted = 0;
END
GO

-- =============================================
-- SP: CreateBulkNotifications
-- Description: Create notifications for multiple users
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[CreateBulkNotifications]
    @Title NVARCHAR(200),
    @Message NVARCHAR(2000),
    @Type INT, -- NotificationType
    @Priority INT, -- NotificationPriority
    @SenderId UNIQUEIDENTIFIER = NULL,
    @Category NVARCHAR(100) = NULL,
    @ActionUrl NVARCHAR(500) = NULL,
    @ActionText NVARCHAR(100) = NULL,
    @ScheduledDate DATETIME2 = NULL,
    @ExpiryDate DATETIME2 = NULL,
    @AdditionalData NVARCHAR(MAX) = NULL,
    @UserRole INT = NULL, -- Send to users with specific role
    @Department INT = NULL, -- Send to users in specific department
    @ActiveUsersOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @CreatedCount INT = 0;
        
        -- Create notifications for matching users
        INSERT INTO Notifications (
            Id, Title, Message, Type, Priority, RecipientId, SenderId, 
            Category, ActionUrl, ActionText, ScheduledDate, ExpiryDate, 
            AdditionalData, CreatedDate
        )
        SELECT 
            NEWID(), @Title, @Message, @Type, @Priority, u.Id, @SenderId,
            @Category, @ActionUrl, @ActionText, @ScheduledDate, @ExpiryDate,
            @AdditionalData, GETUTCDATE()
        FROM Users u
        WHERE 
            (@ActiveUsersOnly = 0 OR u.IsActive = 1)
            AND (@UserRole IS NULL OR u.Role = @UserRole)
            AND (@Department IS NULL OR u.Department = @Department);
        
        SET @CreatedCount = @@ROWCOUNT;
        
        COMMIT TRANSACTION
        
        SELECT @CreatedCount AS CreatedCount;
        
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
-- SP: GetRecentNotifications
-- Description: Get recent notifications for dashboard
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetRecentNotifications]
    @UserId UNIQUEIDENTIFIER,
    @Hours INT = 24,
    @MaxCount INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxCount)
        n.Id,
        n.Title,
        n.Message,
        n.Type,
        n.Priority,
        n.CreatedDate,
        n.IsRead,
        n.ActionUrl,
        n.ActionText,
        CASE WHEN n.SenderId IS NOT NULL THEN 
            u.FirstName + ' ' + u.LastName 
        ELSE 'System' END AS SenderName
    FROM Notifications n
    LEFT JOIN Users u ON n.SenderId = u.Id
    WHERE 
        n.RecipientId = @UserId
        AND n.IsDeleted = 0
        AND n.CreatedDate >= DATEADD(hour, -@Hours, GETUTCDATE())
    ORDER BY n.CreatedDate DESC;
END
GO

-- =============================================
-- SP: CleanupExpiredNotifications
-- Description: Clean up expired notifications (for scheduled maintenance)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[CleanupExpiredNotifications]
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @DeletedCount INT = 0;
        
        -- Soft delete expired notifications
        UPDATE Notifications 
        SET IsDeleted = 1
        WHERE 
            ExpiryDate IS NOT NULL 
            AND ExpiryDate < GETUTCDATE()
            AND IsDeleted = 0;
        
        SET @DeletedCount = @@ROWCOUNT;
        
        -- Also clean up old read notifications (older than 90 days)
        UPDATE Notifications 
        SET IsDeleted = 1
        WHERE 
            IsRead = 1
            AND ReadDate < DATEADD(day, -90, GETUTCDATE())
            AND IsDeleted = 0;
        
        SET @DeletedCount = @DeletedCount + @@ROWCOUNT;
        
        COMMIT TRANSACTION
        
        SELECT @DeletedCount AS DeletedCount;
        
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
-- SP: CreateReportNotification
-- Description: Create notification related to report workflow
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[CreateReportNotification]
    @ReportId UNIQUEIDENTIFIER,
    @NotificationType INT, -- NotificationType enum
    @RecipientId UNIQUEIDENTIFIER,
    @SenderId UNIQUEIDENTIFIER = NULL,
    @AdditionalMessage NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @ReportTitle NVARCHAR(200);
        DECLARE @ReportNumber NVARCHAR(50);
        DECLARE @ReportStatus INT;
        DECLARE @Title NVARCHAR(200);
        DECLARE @Message NVARCHAR(2000);
        DECLARE @Priority INT = 2; -- Normal priority
        DECLARE @ActionUrl NVARCHAR(500);
        
        -- Get report details
        SELECT @ReportTitle = Title, @ReportNumber = ReportNumber, @ReportStatus = Status
        FROM Reports WHERE Id = @ReportId;
        
        IF @ReportTitle IS NULL
        BEGIN
            RAISERROR('Report not found', 16, 1);
            RETURN;
        END
        
        -- Build notification content based on type
        SET @ActionUrl = '/reports/' + CAST(@ReportId AS NVARCHAR(36));
        
        -- Determine notification content based on type
        IF @NotificationType = 1 -- ReportSubmitted
        BEGIN
            SET @Title = 'Report Submitted: ' + @ReportNumber;
            SET @Message = 'Report "' + @ReportTitle + '" has been submitted for review.';
            IF @AdditionalMessage IS NOT NULL
                SET @Message = @Message + ' ' + @AdditionalMessage;
        END
        ELSE IF @NotificationType = 2 -- ApprovalRequired
        BEGIN
            SET @Title = 'Approval Required: ' + @ReportNumber;
            SET @Message = 'Report "' + @ReportTitle + '" requires your approval.';
            SET @Priority = 3; -- High priority
        END
        ELSE IF @NotificationType = 3 -- ReportApproved
        BEGIN
            SET @Title = 'Report Approved: ' + @ReportNumber;
            SET @Message = 'Report "' + @ReportTitle + '" has been approved.';
        END
        ELSE IF @NotificationType = 4 -- ReportRejected
        BEGIN
            SET @Title = 'Report Rejected: ' + @ReportNumber;
            SET @Message = 'Report "' + @ReportTitle + '" has been rejected.';
            SET @Priority = 3; -- High priority
            IF @AdditionalMessage IS NOT NULL
                SET @Message = @Message + ' Reason: ' + @AdditionalMessage;
        END
        ELSE IF @NotificationType = 12 -- StatusChange
        BEGIN
            SET @Title = 'Report Status Updated: ' + @ReportNumber;
            SET @Message = 'The status of report "' + @ReportTitle + '" has been updated.';
            IF @AdditionalMessage IS NOT NULL
                SET @Message = @Message + ' ' + @AdditionalMessage;
        END
        ELSE
        BEGIN
            SET @Title = 'Report Update: ' + @ReportNumber;
            SET @Message = 'Report "' + @ReportTitle + '" has been updated.';
            IF @AdditionalMessage IS NOT NULL
                SET @Message = @AdditionalMessage;
        END
        
        -- Create the notification
        INSERT INTO Notifications (
            Id, Title, Message, Type, Priority, RecipientId, SenderId,
            RelatedReportId, Category, ActionUrl, ActionText, CreatedDate
        )
        VALUES (
            NEWID(), @Title, @Message, @NotificationType, @Priority, @RecipientId, @SenderId,
            @ReportId, 'Reports', @ActionUrl, 'View Report', GETUTCDATE()
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

PRINT 'Notification stored procedures created successfully!'
