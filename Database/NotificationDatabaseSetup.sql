-- =============================================
-- Notification System Database Setup Script
-- Description: Creates notification tables and stored procedures
-- Version: 1.0
-- Date: 2025-01-01
-- =============================================

USE [ProjectControlsReportingToolDB]
GO

-- =============================================
-- CREATE NOTIFICATION TABLES
-- =============================================

-- Drop existing tables if they exist (in reverse dependency order)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationHistory]') AND type in (N'U'))
    DROP TABLE [dbo].[NotificationHistory]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationPreferences]') AND type in (N'U'))
    DROP TABLE [dbo].[NotificationPreferences]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationSubscriptions]') AND type in (N'U'))
    DROP TABLE [dbo].[NotificationSubscriptions]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailQueue]') AND type in (N'U'))
    DROP TABLE [dbo].[EmailQueue]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
    DROP TABLE [dbo].[Notifications]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationTemplates]') AND type in (N'U'))
    DROP TABLE [dbo].[NotificationTemplates]
GO

-- =============================================
-- Table: NotificationTemplates
-- =============================================
CREATE TABLE [dbo].[NotificationTemplates](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[Name] [nvarchar](100) NOT NULL,
	[Subject] [nvarchar](200) NOT NULL,
	[HtmlTemplate] [nvarchar](max) NOT NULL,
	[TextTemplate] [nvarchar](max) NOT NULL,
	[Type] [int] NOT NULL, -- NotificationType enum
	[Category] [nvarchar](100) NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL DEFAULT 1,
	[IsSystem] [bit] NOT NULL DEFAULT 0,
	[Variables] [nvarchar](max) NULL, -- JSON array of available variables
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[ModifiedBy] [uniqueidentifier] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[Version] [int] NOT NULL DEFAULT 1,
	
	CONSTRAINT [FK_NotificationTemplates_Users_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_NotificationTemplates_Users_ModifiedBy] FOREIGN KEY([ModifiedBy]) REFERENCES [dbo].[Users] ([Id])
)
GO

-- =============================================
-- Table: Notifications
-- =============================================
CREATE TABLE [dbo].[Notifications](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[Title] [nvarchar](200) NOT NULL,
	[Message] [nvarchar](2000) NOT NULL,
	[Type] [int] NOT NULL, -- NotificationType enum
	[Priority] [int] NOT NULL, -- NotificationPriority enum
	[Status] [int] NOT NULL DEFAULT 1, -- NotificationStatus enum (Pending = 1)
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
GO

-- =============================================
-- Table: NotificationPreferences
-- =============================================
CREATE TABLE [dbo].[NotificationPreferences](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[UserId] [uniqueidentifier] NOT NULL,
	[NotificationType] [int] NOT NULL, -- NotificationType enum
	[EmailEnabled] [bit] NOT NULL DEFAULT 1,
	[PushEnabled] [bit] NOT NULL DEFAULT 1,
	[InAppEnabled] [bit] NOT NULL DEFAULT 1,
	[SmsEnabled] [bit] NOT NULL DEFAULT 0,
	[PreferredTime] [nvarchar](20) NULL, -- HH:mm format
	[FrequencyMinutes] [int] NULL, -- For digest notifications
	[IsActive] [bit] NOT NULL DEFAULT 1,
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[ModifiedDate] [datetime2](7) NULL,
	
	CONSTRAINT [FK_NotificationPreferences_Users_UserId] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [UK_NotificationPreferences_UserId_Type] UNIQUE ([UserId], [NotificationType])
)
GO

-- =============================================
-- Table: NotificationHistory
-- =============================================
CREATE TABLE [dbo].[NotificationHistory](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[NotificationId] [uniqueidentifier] NOT NULL,
	[Status] [int] NOT NULL, -- NotificationStatus enum
	[StatusMessage] [nvarchar](500) NULL,
	[Channel] [nvarchar](100) NULL, -- Email, Push, SMS, InApp
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[AdditionalDetails] [nvarchar](2000) NULL,
	[IsError] [bit] NOT NULL DEFAULT 0,
	
	CONSTRAINT [FK_NotificationHistory_Notifications_NotificationId] FOREIGN KEY([NotificationId]) REFERENCES [dbo].[Notifications] ([Id]) ON DELETE CASCADE
)
GO

-- =============================================
-- Table: EmailQueue
-- =============================================
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
	[Status] [int] NOT NULL DEFAULT 1, -- EmailQueueStatus enum (Pending = 1)
	[Priority] [int] NOT NULL DEFAULT 2, -- EmailPriority enum (Normal = 2)
	[CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[ScheduledDate] [datetime2](7) NULL,
	[SentDate] [datetime2](7) NULL,
	[RetryCount] [int] NOT NULL DEFAULT 0,
	[MaxRetries] [int] NOT NULL DEFAULT 3,
	[ErrorMessage] [nvarchar](1000) NULL,
	[MessageId] [nvarchar](500) NULL, -- SMTP message ID
	[Attachments] [nvarchar](max) NULL, -- JSON array of attachment paths
	[IsDeleted] [bit] NOT NULL DEFAULT 0,
	
	CONSTRAINT [FK_EmailQueue_Notifications_NotificationId] FOREIGN KEY([NotificationId]) REFERENCES [dbo].[Notifications] ([Id])
)
GO

-- =============================================
-- Table: NotificationSubscriptions
-- =============================================
CREATE TABLE [dbo].[NotificationSubscriptions](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[Name] [nvarchar](100) NOT NULL,
	[WebhookUrl] [nvarchar](500) NOT NULL,
	[SubscribedTypesJson] [nvarchar](max) NOT NULL, -- JSON array of NotificationType values
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
GO

-- =============================================
-- CREATE INDEXES FOR PERFORMANCE
-- =============================================

-- Notifications indexes
CREATE NONCLUSTERED INDEX [IX_Notifications_RecipientId] ON [dbo].[Notifications]([RecipientId])
CREATE NONCLUSTERED INDEX [IX_Notifications_SenderId] ON [dbo].[Notifications]([SenderId])
CREATE NONCLUSTERED INDEX [IX_Notifications_RelatedReportId] ON [dbo].[Notifications]([RelatedReportId])
CREATE NONCLUSTERED INDEX [IX_Notifications_Type] ON [dbo].[Notifications]([Type])
CREATE NONCLUSTERED INDEX [IX_Notifications_Status] ON [dbo].[Notifications]([Status])
CREATE NONCLUSTERED INDEX [IX_Notifications_CreatedDate] ON [dbo].[Notifications]([CreatedDate] DESC)
CREATE NONCLUSTERED INDEX [IX_Notifications_IsRead] ON [dbo].[Notifications]([IsRead])
CREATE NONCLUSTERED INDEX [IX_Notifications_RecipientId_Status] ON [dbo].[Notifications]([RecipientId], [Status])

-- NotificationPreferences indexes
CREATE NONCLUSTERED INDEX [IX_NotificationPreferences_UserId] ON [dbo].[NotificationPreferences]([UserId])
CREATE NONCLUSTERED INDEX [IX_NotificationPreferences_NotificationType] ON [dbo].[NotificationPreferences]([NotificationType])

-- NotificationHistory indexes
CREATE NONCLUSTERED INDEX [IX_NotificationHistory_NotificationId] ON [dbo].[NotificationHistory]([NotificationId])
CREATE NONCLUSTERED INDEX [IX_NotificationHistory_Status] ON [dbo].[NotificationHistory]([Status])
CREATE NONCLUSTERED INDEX [IX_NotificationHistory_CreatedDate] ON [dbo].[NotificationHistory]([CreatedDate] DESC)

-- EmailQueue indexes
CREATE NONCLUSTERED INDEX [IX_EmailQueue_NotificationId] ON [dbo].[EmailQueue]([NotificationId])
CREATE NONCLUSTERED INDEX [IX_EmailQueue_Status] ON [dbo].[EmailQueue]([Status])
CREATE NONCLUSTERED INDEX [IX_EmailQueue_CreatedDate] ON [dbo].[EmailQueue]([CreatedDate] DESC)
CREATE NONCLUSTERED INDEX [IX_EmailQueue_ScheduledDate] ON [dbo].[EmailQueue]([ScheduledDate])

-- NotificationTemplates indexes
CREATE NONCLUSTERED INDEX [IX_NotificationTemplates_Type] ON [dbo].[NotificationTemplates]([Type])
CREATE NONCLUSTERED INDEX [IX_NotificationTemplates_IsActive] ON [dbo].[NotificationTemplates]([IsActive])
CREATE NONCLUSTERED INDEX [IX_NotificationTemplates_CreatedBy] ON [dbo].[NotificationTemplates]([CreatedBy])

-- NotificationSubscriptions indexes
CREATE NONCLUSTERED INDEX [IX_NotificationSubscriptions_CreatedBy] ON [dbo].[NotificationSubscriptions]([CreatedBy])
CREATE NONCLUSTERED INDEX [IX_NotificationSubscriptions_IsActive] ON [dbo].[NotificationSubscriptions]([IsActive])

GO

PRINT 'Notification tables created successfully!'
