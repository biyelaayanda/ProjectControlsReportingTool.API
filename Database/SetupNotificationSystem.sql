-- =============================================
-- Complete Notification System Setup Script
-- Description: Runs all notification setup scripts in correct order
-- Version: 1.0
-- Date: 2025-01-01
-- Usage: Execute this script to set up the complete notification system
-- =============================================

USE [ProjectControlsReportingToolDB]
GO

PRINT 'Starting Notification System Setup...'
PRINT '======================================'

-- =============================================
-- STEP 1: Create Notification Tables
-- =============================================
PRINT 'Step 1: Creating notification tables...'

-- Drop existing tables if they exist (in reverse dependency order)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationHistory]') AND type in (N'U'))
    DROP TABLE [dbo].[NotificationHistory]

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationPreferences]') AND type in (N'U'))
    DROP TABLE [dbo].[NotificationPreferences]

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationSubscriptions]') AND type in (N'U'))
    DROP TABLE [dbo].[NotificationSubscriptions]

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailQueue]') AND type in (N'U'))
    DROP TABLE [dbo].[EmailQueue]

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
    DROP TABLE [dbo].[Notifications]

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationTemplates]') AND type in (N'U'))
    DROP TABLE [dbo].[NotificationTemplates]

-- NotificationTemplates table
CREATE TABLE [dbo].[NotificationTemplates](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[Name] [nvarchar](100) NOT NULL,
	[Subject] [nvarchar](200) NOT NULL,
	[HtmlTemplate] [nvarchar](max) NOT NULL,
	[TextTemplate] [nvarchar](max) NOT NULL,
	[Type] [int] NOT NULL,
	[Category] [nvarchar](100) NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL DEFAULT 1,
	[IsSystem] [bit] NOT NULL DEFAULT 0,
	[Variables] [nvarchar](max) NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[ModifiedBy] [uniqueidentifier] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[Version] [int] NOT NULL DEFAULT 1,
	
	CONSTRAINT [FK_NotificationTemplates_Users_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_NotificationTemplates_Users_ModifiedBy] FOREIGN KEY([ModifiedBy]) REFERENCES [dbo].[Users] ([Id])
)

-- Notifications table
CREATE TABLE [dbo].[Notifications](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[Title] [nvarchar](200) NOT NULL,
	[Message] [nvarchar](2000) NOT NULL,
	[Type] [int] NOT NULL,
	[Priority] [int] NOT NULL,
	[Status] [int] NOT NULL DEFAULT 1,
	[RecipientId] [uniqueidentifier] NOT NULL,
	[SenderId] [uniqueidentifier] NULL,
	[RelatedReportId] [uniqueidentifier] NULL,
	[Category] [nvarchar](100) NULL,
	[ActionUrl] [nvarchar](500) NULL,
	[ActionText] [nvarchar](100) NULL,
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[ScheduledDate] [datetime2](7) NULL,
	[SentDate] [datetime2](7) NULL,
	[ReadDate] [datetime2](7) NULL,
	[ExpiryDate] [datetime2](7) NULL,
	[ErrorMessage] [nvarchar](500) NULL,
	[RetryCount] [int] NOT NULL DEFAULT 0,
	[MaxRetries] [int] NOT NULL DEFAULT 3,
	[AdditionalData] [nvarchar](max) NULL,
	[IsRead] [bit] NOT NULL DEFAULT 0,
	[IsEmailSent] [bit] NOT NULL DEFAULT 0,
	[IsPushSent] [bit] NOT NULL DEFAULT 0,
	[IsDeleted] [bit] NOT NULL DEFAULT 0,
	
	CONSTRAINT [FK_Notifications_Users_RecipientId] FOREIGN KEY([RecipientId]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_Notifications_Users_SenderId] FOREIGN KEY([SenderId]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_Notifications_Reports_RelatedReportId] FOREIGN KEY([RelatedReportId]) REFERENCES [dbo].[Reports] ([Id])
)

-- NotificationPreferences table
CREATE TABLE [dbo].[NotificationPreferences](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[UserId] [uniqueidentifier] NOT NULL,
	[NotificationType] [int] NOT NULL,
	[EmailEnabled] [bit] NOT NULL DEFAULT 1,
	[PushEnabled] [bit] NOT NULL DEFAULT 1,
	[InAppEnabled] [bit] NOT NULL DEFAULT 1,
	[SmsEnabled] [bit] NOT NULL DEFAULT 0,
	[PreferredTime] [nvarchar](20) NULL,
	[FrequencyMinutes] [int] NULL,
	[IsActive] [bit] NOT NULL DEFAULT 1,
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[ModifiedDate] [datetime2](7) NULL,
	
	CONSTRAINT [FK_NotificationPreferences_Users_UserId] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [UK_NotificationPreferences_UserId_Type] UNIQUE ([UserId], [NotificationType])
)

-- NotificationHistory table
CREATE TABLE [dbo].[NotificationHistory](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[NotificationId] [uniqueidentifier] NOT NULL,
	[Status] [int] NOT NULL,
	[StatusMessage] [nvarchar](500) NULL,
	[Channel] [nvarchar](100) NULL,
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[AdditionalDetails] [nvarchar](2000) NULL,
	[IsError] [bit] NOT NULL DEFAULT 0,
	
	CONSTRAINT [FK_NotificationHistory_Notifications_NotificationId] FOREIGN KEY([NotificationId]) REFERENCES [dbo].[Notifications] ([Id]) ON DELETE CASCADE
)

-- EmailQueue table
CREATE TABLE [dbo].[EmailQueue](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[NotificationId] [uniqueidentifier] NULL,
	[ToEmail] [nvarchar](500) NOT NULL,
	[ToName] [nvarchar](500) NULL,
	[CcEmails] [nvarchar](500) NULL,
	[BccEmails] [nvarchar](500) NULL,
	[Subject] [nvarchar](200) NOT NULL,
	[HtmlBody] [nvarchar](max) NOT NULL,
	[TextBody] [nvarchar](max) NULL,
	[Status] [int] NOT NULL DEFAULT 1,
	[Priority] [int] NOT NULL DEFAULT 2,
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[ScheduledDate] [datetime2](7) NULL,
	[SentDate] [datetime2](7) NULL,
	[RetryCount] [int] NOT NULL DEFAULT 0,
	[MaxRetries] [int] NOT NULL DEFAULT 3,
	[ErrorMessage] [nvarchar](1000) NULL,
	[MessageId] [nvarchar](500) NULL,
	[Attachments] [nvarchar](max) NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT 0,
	
	CONSTRAINT [FK_EmailQueue_Notifications_NotificationId] FOREIGN KEY([NotificationId]) REFERENCES [dbo].[Notifications] ([Id])
)

-- NotificationSubscriptions table
CREATE TABLE [dbo].[NotificationSubscriptions](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[Name] [nvarchar](100) NOT NULL,
	[WebhookUrl] [nvarchar](500) NOT NULL,
	[SubscribedTypesJson] [nvarchar](max) NOT NULL,
	[SecretKey] [nvarchar](200) NULL,
	[AuthToken] [nvarchar](100) NULL,
	[IsActive] [bit] NOT NULL DEFAULT 1,
	[TimeoutSeconds] [int] NOT NULL DEFAULT 30,
	[MaxRetries] [int] NOT NULL DEFAULT 3,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[LastTriggeredDate] [datetime2](7) NULL,
	[SuccessCount] [int] NOT NULL DEFAULT 0,
	[FailureCount] [int] NOT NULL DEFAULT 0,
	
	CONSTRAINT [FK_NotificationSubscriptions_Users_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[Users] ([Id])
)

-- Create indexes
CREATE NONCLUSTERED INDEX [IX_Notifications_RecipientId] ON [dbo].[Notifications]([RecipientId])
CREATE NONCLUSTERED INDEX [IX_Notifications_Status] ON [dbo].[Notifications]([Status])
CREATE NONCLUSTERED INDEX [IX_Notifications_CreatedDate] ON [dbo].[Notifications]([CreatedDate] DESC)
CREATE NONCLUSTERED INDEX [IX_Notifications_IsRead] ON [dbo].[Notifications]([IsRead])
CREATE NONCLUSTERED INDEX [IX_NotificationPreferences_UserId] ON [dbo].[NotificationPreferences]([UserId])

PRINT 'Step 1 completed: Notification tables created successfully!'

-- =============================================
-- STEP 2: Create System User for Notifications
-- =============================================
PRINT 'Step 2: Creating system user...'

DECLARE @SystemUserId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @SystemUserId)
BEGIN
    INSERT INTO Users (
        Id, Email, FirstName, LastName, PasswordHash, PasswordSalt,
        Role, Department, IsActive, CreatedDate, LastLoginDate
    )
    VALUES (
        @SystemUserId, 'system@projectcontrols.com', 'System', 'Notifications',
        'SYSTEM_HASH', 'SYSTEM_SALT', 3, 1, 1, GETUTCDATE(), GETUTCDATE()
    );
    PRINT 'System user created successfully!'
END
ELSE
BEGIN
    PRINT 'System user already exists!'
END

-- =============================================
-- STEP 3: Create Essential Stored Procedures
-- =============================================
PRINT 'Step 3: Creating stored procedures...'

-- GetNotifications procedure
EXEC('
CREATE OR ALTER PROCEDURE [dbo].[GetNotifications]
    @UserId UNIQUEIDENTIFIER,
    @Status INT = NULL,
    @Page INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    SELECT 
        n.Id,
        n.Title,
        n.Message,
        n.Type,
        n.Priority,
        n.Status,
        n.Category,
        n.CreatedDate,
        n.ReadDate,
        n.IsRead,
        n.ActionUrl,
        n.ActionText,
        COUNT(*) OVER() AS TotalCount
    FROM Notifications n
    WHERE 
        n.RecipientId = @UserId
        AND n.IsDeleted = 0
        AND (@Status IS NULL OR n.Status = @Status)
    ORDER BY n.CreatedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
')

-- MarkNotificationAsRead procedure
EXEC('
CREATE OR ALTER PROCEDURE [dbo].[MarkNotificationAsRead]
    @NotificationId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Notifications 
    SET 
        IsRead = 1,
        ReadDate = GETUTCDATE(),
        Status = 4
    WHERE Id = @NotificationId AND RecipientId = @UserId AND IsDeleted = 0;
    
    SELECT @@ROWCOUNT AS Success;
END
')

-- DeleteNotification procedure
EXEC('
CREATE OR ALTER PROCEDURE [dbo].[DeleteNotification]
    @NotificationId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Notifications 
    SET IsDeleted = 1
    WHERE Id = @NotificationId AND RecipientId = @UserId AND IsDeleted = 0;
    
    SELECT @@ROWCOUNT AS Success;
END
')

-- GetNotificationStats procedure
EXEC('
CREATE OR ALTER PROCEDURE [dbo].[GetNotificationStats]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) AS Total,
        SUM(CASE WHEN IsRead = 0 THEN 1 ELSE 0 END) AS Unread,
        SUM(CASE WHEN IsRead = 1 THEN 1 ELSE 0 END) AS [Read],
        SUM(CASE WHEN Priority = 4 THEN 1 ELSE 0 END) AS Critical,
        SUM(CASE WHEN Priority = 3 THEN 1 ELSE 0 END) AS [High]
    FROM Notifications
    WHERE RecipientId = @UserId AND IsDeleted = 0;
END
')

PRINT 'Step 3 completed: Stored procedures created successfully!'

-- =============================================
-- STEP 4: Insert Default Notification Templates
-- =============================================
PRINT 'Step 4: Creating notification templates...'

-- Clear existing system templates
DELETE FROM NotificationTemplates WHERE IsSystem = 1;

-- Insert essential templates
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES 
(NEWID(), 'ReportSubmitted', 'Report Submitted: {{ReportNumber}}',
 '<h2>Report Submitted</h2><p>Report <strong>{{ReportTitle}}</strong> has been submitted for review.</p>',
 'Report {{ReportTitle}} has been submitted for review.',
 1, 'Reports', 'Template for report submission notifications', 1, 1, 
 '["ReportTitle", "ReportNumber"]', @SystemUserId, GETUTCDATE(), 1),

(NEWID(), 'ApprovalRequired', 'Approval Required: {{ReportNumber}}',
 '<h2>Approval Required</h2><p>Report <strong>{{ReportTitle}}</strong> requires your approval.</p>',
 'Report {{ReportTitle}} requires your approval.',
 2, 'Reports', 'Template for approval requirement notifications', 1, 1,
 '["ReportTitle", "ReportNumber"]', @SystemUserId, GETUTCDATE(), 1),

(NEWID(), 'ReportApproved', 'Report Approved: {{ReportNumber}}',
 '<h2>Report Approved</h2><p>Report <strong>{{ReportTitle}}</strong> has been approved.</p>',
 'Report {{ReportTitle}} has been approved.',
 3, 'Reports', 'Template for report approval notifications', 1, 1,
 '["ReportTitle", "ReportNumber"]', @SystemUserId, GETUTCDATE(), 1),

(NEWID(), 'SystemAlert', 'System Alert: {{AlertTitle}}',
 '<h2>System Alert</h2><p>{{AlertMessage}}</p>',
 'System Alert: {{AlertMessage}}',
 7, 'System', 'Template for system alert notifications', 1, 1,
 '["AlertTitle", "AlertMessage"]', @SystemUserId, GETUTCDATE(), 1);

PRINT 'Step 4 completed: Notification templates created successfully!'

-- =============================================
-- STEP 5: Insert Default Notification Preferences
-- =============================================
PRINT 'Step 5: Creating default notification preferences...'

DELETE FROM NotificationPreferences;

INSERT INTO NotificationPreferences (
    Id, UserId, NotificationType, EmailEnabled, PushEnabled, InAppEnabled, 
    SmsEnabled, IsActive, CreatedDate
)
SELECT 
    NEWID(), u.Id, nt.NotificationType, 
    CASE WHEN nt.NotificationType IN (1, 2, 3, 4, 7) THEN 1 ELSE 0 END AS EmailEnabled,
    1 AS PushEnabled,
    1 AS InAppEnabled,
    0 AS SmsEnabled,
    1 AS IsActive,
    GETUTCDATE() AS CreatedDate
FROM Users u
CROSS JOIN (VALUES (1), (2), (3), (4), (5), (6), (7), (8), (12)) AS nt(NotificationType)
WHERE u.IsActive = 1;

PRINT 'Step 5 completed: Default notification preferences created!'

-- =============================================
-- STEP 6: Create Sample Notifications
-- =============================================
PRINT 'Step 6: Creating sample notifications...'

IF EXISTS (SELECT 1 FROM Users WHERE IsActive = 1 AND Id != @SystemUserId)
BEGIN
    DECLARE @SampleUserId UNIQUEIDENTIFIER;
    SELECT TOP 1 @SampleUserId = Id FROM Users WHERE IsActive = 1 AND Id != @SystemUserId;
    
    INSERT INTO Notifications (
        Id, Title, Message, Type, Priority, RecipientId, SenderId,
        Category, ActionUrl, ActionText, CreatedDate
    )
    VALUES 
    (NEWID(), 'Welcome to Project Controls Reporting Tool',
     'Welcome! Your account has been set up successfully. You can now start creating and managing reports.',
     8, 2, @SampleUserId, @SystemUserId, 'System', '/dashboard', 'Go to Dashboard', GETUTCDATE()),
     
    (NEWID(), 'Notification System Active',
     'The notification system has been successfully configured and is now active. You will receive real-time notifications for report updates.',
     7, 2, @SampleUserId, @SystemUserId, 'System', '/notifications', 'View Notifications', GETUTCDATE());
     
    PRINT 'Sample notifications created successfully!'
END

-- =============================================
-- FINAL SUMMARY
-- =============================================
PRINT ' '
PRINT 'Notification System Setup Complete!'
PRINT '===================================='
PRINT 'Summary of created components:'

SELECT 'Tables Created' AS Component, 6 AS Count
UNION ALL
SELECT 'Stored Procedures Created' AS Component, 4 AS Count
UNION ALL
SELECT 'Notification Templates' AS Component, COUNT(*) AS Count FROM NotificationTemplates WHERE IsSystem = 1
UNION ALL
SELECT 'Notification Preferences' AS Component, COUNT(*) AS Count FROM NotificationPreferences
UNION ALL
SELECT 'Sample Notifications' AS Component, COUNT(*) AS Count FROM Notifications
UNION ALL
SELECT 'Active Users Configured' AS Component, COUNT(DISTINCT UserId) AS Count FROM NotificationPreferences;

PRINT ' '
PRINT 'Next Steps:'
PRINT '1. Start the API application'
PRINT '2. Navigate to /notifications in the frontend'
PRINT '3. Verify notifications load without browser issues'
PRINT '4. Test mark as read and delete functionality'
PRINT ' '
PRINT 'The notification system is now ready for use!'
