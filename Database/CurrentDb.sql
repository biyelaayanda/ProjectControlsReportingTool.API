USE [master]
GO
/****** Object:  Database [ProjectControlsReportingToolDB]    Script Date: 2025/08/31 03:41:16 ******/
CREATE DATABASE [ProjectControlsReportingToolDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ProjectControlsReportingToolDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\ProjectControlsReportingToolDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ProjectControlsReportingToolDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\ProjectControlsReportingToolDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ProjectControlsReportingToolDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET RECOVERY FULL 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET  MULTI_USER 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'ProjectControlsReportingToolDB', N'ON'
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [ProjectControlsReportingToolDB]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AuditLogs]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLogs](
	[Id] [uniqueidentifier] NOT NULL,
	[Action] [int] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ReportId] [uniqueidentifier] NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Details] [nvarchar](1000) NULL,
	[IpAddress] [nvarchar](100) NULL,
	[UserAgent] [nvarchar](500) NULL,
 CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportAttachments]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportAttachments](
	[Id] [uniqueidentifier] NOT NULL,
	[ReportId] [uniqueidentifier] NOT NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[OriginalFileName] [nvarchar](255) NOT NULL,
	[FilePath] [nvarchar](500) NOT NULL,
	[ContentType] [nvarchar](100) NULL,
	[FileSize] [bigint] NOT NULL,
	[UploadedDate] [datetime2](7) NOT NULL,
	[UploadedBy] [uniqueidentifier] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Description] [nvarchar](500) NULL,
	[ApprovalStage] [int] NOT NULL,
	[UploadedByName] [nvarchar](200) NULL,
	[UploadedByRole] [int] NOT NULL,
 CONSTRAINT [PK_ReportAttachments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reports]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reports](
	[Id] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Status] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[LastModifiedDate] [datetime2](7) NOT NULL,
	[SubmittedDate] [datetime2](7) NULL,
	[ManagerApprovedDate] [datetime2](7) NULL,
	[GMApprovedDate] [datetime2](7) NULL,
	[CompletedDate] [datetime2](7) NULL,
	[RejectionReason] [nvarchar](1000) NULL,
	[RejectedBy] [uniqueidentifier] NULL,
	[RejectedDate] [datetime2](7) NULL,
	[ReportNumber] [nvarchar](50) NULL,
	[Department] [int] NOT NULL,
	[DueDate] [datetime2](7) NULL,
	[Priority] [nvarchar](20) NOT NULL,
	[Type] [nvarchar](100) NULL,
 CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportSignatures]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportSignatures](
	[Id] [uniqueidentifier] NOT NULL,
	[ReportId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[SignatureType] [int] NOT NULL,
	[SignedDate] [datetime2](7) NOT NULL,
	[Comments] [nvarchar](1000) NULL,
	[SignatureFilePath] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_ReportSignatures] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [uniqueidentifier] NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[PasswordSalt] [nvarchar](max) NOT NULL,
	[Role] [int] NOT NULL,
	[Department] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[LastLoginDate] [datetime2](7) NOT NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[JobTitle] [nvarchar](100) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_AuditLogs_ReportId]    Script Date: 2025/08/31 03:41:16 ******/
CREATE NONCLUSTERED INDEX [IX_AuditLogs_ReportId] ON [dbo].[AuditLogs]
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuditLogs_UserId]    Script Date: 2025/08/31 03:41:16 ******/
CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId] ON [dbo].[AuditLogs]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReportAttachments_ReportId]    Script Date: 2025/08/31 03:41:16 ******/
CREATE NONCLUSTERED INDEX [IX_ReportAttachments_ReportId] ON [dbo].[ReportAttachments]
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReportAttachments_UploadedBy]    Script Date: 2025/08/31 03:41:16 ******/
CREATE NONCLUSTERED INDEX [IX_ReportAttachments_UploadedBy] ON [dbo].[ReportAttachments]
(
	[UploadedBy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reports_CreatedBy]    Script Date: 2025/08/31 03:41:16 ******/
CREATE NONCLUSTERED INDEX [IX_Reports_CreatedBy] ON [dbo].[Reports]
(
	[CreatedBy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reports_RejectedBy]    Script Date: 2025/08/31 03:41:16 ******/
CREATE NONCLUSTERED INDEX [IX_Reports_RejectedBy] ON [dbo].[Reports]
(
	[RejectedBy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReportSignatures_ReportId]    Script Date: 2025/08/31 03:41:16 ******/
CREATE NONCLUSTERED INDEX [IX_ReportSignatures_ReportId] ON [dbo].[ReportSignatures]
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReportSignatures_UserId]    Script Date: 2025/08/31 03:41:16 ******/
CREATE NONCLUSTERED INDEX [IX_ReportSignatures_UserId] ON [dbo].[ReportSignatures]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Email]    Script Date: 2025/08/31 03:41:16 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AuditLogs] ADD  DEFAULT (getutcdate()) FOR [Timestamp]
GO
ALTER TABLE [dbo].[ReportAttachments] ADD  DEFAULT (getutcdate()) FOR [UploadedDate]
GO
ALTER TABLE [dbo].[ReportAttachments] ADD  DEFAULT ((1)) FOR [ApprovalStage]
GO
ALTER TABLE [dbo].[ReportAttachments] ADD  DEFAULT ((1)) FOR [UploadedByRole]
GO
ALTER TABLE [dbo].[Reports] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Reports] ADD  DEFAULT (getutcdate()) FOR [LastModifiedDate]
GO
ALTER TABLE [dbo].[Reports] ADD  DEFAULT (N'') FOR [Priority]
GO
ALTER TABLE [dbo].[ReportSignatures] ADD  DEFAULT (getutcdate()) FOR [SignedDate]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[AuditLogs]  WITH CHECK ADD  CONSTRAINT [FK_AuditLogs_Reports_ReportId] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[AuditLogs] CHECK CONSTRAINT [FK_AuditLogs_Reports_ReportId]
GO
ALTER TABLE [dbo].[AuditLogs]  WITH CHECK ADD  CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[AuditLogs] CHECK CONSTRAINT [FK_AuditLogs_Users_UserId]
GO
ALTER TABLE [dbo].[ReportAttachments]  WITH CHECK ADD  CONSTRAINT [FK_ReportAttachments_Reports_ReportId] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportAttachments] CHECK CONSTRAINT [FK_ReportAttachments_Reports_ReportId]
GO
ALTER TABLE [dbo].[ReportAttachments]  WITH CHECK ADD  CONSTRAINT [FK_ReportAttachments_Users_UploadedBy] FOREIGN KEY([UploadedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[ReportAttachments] CHECK CONSTRAINT [FK_ReportAttachments_Users_UploadedBy]
GO
ALTER TABLE [dbo].[Reports]  WITH CHECK ADD  CONSTRAINT [FK_Reports_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT [FK_Reports_Users_CreatedBy]
GO
ALTER TABLE [dbo].[Reports]  WITH CHECK ADD  CONSTRAINT [FK_Reports_Users_RejectedBy] FOREIGN KEY([RejectedBy])
REFERENCES [dbo].[Users] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT [FK_Reports_Users_RejectedBy]
GO
ALTER TABLE [dbo].[ReportSignatures]  WITH CHECK ADD  CONSTRAINT [FK_ReportSignatures_Reports_ReportId] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportSignatures] CHECK CONSTRAINT [FK_ReportSignatures_Reports_ReportId]
GO
ALTER TABLE [dbo].[ReportSignatures]  WITH CHECK ADD  CONSTRAINT [FK_ReportSignatures_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[ReportSignatures] CHECK CONSTRAINT [FK_ReportSignatures_Users_UserId]
GO
/****** Object:  StoredProcedure [dbo].[ApproveReport]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ApproveReport]
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
                LastModifiedDate = @ApprovalDate
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
                LastModifiedDate = @ApprovalDate
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
/****** Object:  StoredProcedure [dbo].[AuthenticateUser]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: AuthenticateUser
-- Description: Authenticates user with password verification
-- =============================================
CREATE   PROCEDURE [dbo].[AuthenticateUser]
    @Email NVARCHAR(100),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- This will be enhanced to include password verification logic
    -- For now, returning user for password verification in C# code
    SELECT 
        Id, Email, FirstName, LastName, PasswordHash, PasswordSalt,
        Role, Department, IsActive, CreatedDate, LastLoginDate,
        PhoneNumber, JobTitle
    FROM Users 
    WHERE Email = @Email AND IsActive = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[AuthenticateUserByHash]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: AuthenticateUserByHash
-- Description: Authenticates user by password hash
-- =============================================
CREATE   PROCEDURE [dbo].[AuthenticateUserByHash]
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Email, FirstName, LastName, PasswordHash, PasswordSalt,
        Role, Department, IsActive, CreatedDate, LastLoginDate,
        PhoneNumber, JobTitle
    FROM Users 
    WHERE Email = @Email AND PasswordHash = @PasswordHash AND IsActive = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[CanUserAccessReport]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CanUserAccessReport]
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
    
    -- Get report details - handle different creator column names
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reports' AND COLUMN_NAME = 'CreatorId')
    BEGIN
        SELECT @ReportCreatorId = CreatedBy, @ReportDepartment = Department, @ReportStatus = Status
        FROM Reports WHERE Id = @ReportId;
    END
    ELSE IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reports' AND COLUMN_NAME = 'CreatedBy')
    BEGIN
        SELECT @ReportCreatorId = CreatedBy, @ReportDepartment = Department, @ReportStatus = Status
        FROM Reports WHERE Id = @ReportId;
    END
    ELSE
    BEGIN
        -- No creator column found, only check department and status
        SELECT @ReportCreatorId = NULL, @ReportDepartment = Department, @ReportStatus = Status
        FROM Reports WHERE Id = @ReportId;
    END
    
    -- Check access permissions
    IF @UserRole = 1 -- GeneralStaff
    BEGIN
        IF @ReportCreatorId = @UserId OR @ReportCreatorId IS NULL
        BEGIN
            SET @CanAccess = 1;
            IF @ReportStatus = 1 -- Draft
                SET @CanEdit = 1;
        END
    END
    ELSE IF @UserRole = 2 -- LineManager
    BEGIN
        IF @ReportCreatorId = @UserId OR @ReportDepartment = @UserDepartment OR @ReportCreatorId IS NULL
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
/****** Object:  StoredProcedure [dbo].[CheckEmailExists]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: CheckEmailExists
-- Description: Checks if an email already exists
-- =============================================
CREATE   PROCEDURE [dbo].[CheckEmailExists]
    @Email NVARCHAR(100),
    @Exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        SET @Exists = 1
    ELSE
        SET @Exists = 0
END
GO
/****** Object:  StoredProcedure [dbo].[CreateReport]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- SP: CreateReport
-- Description: Creates a new report
-- =============================================
CREATE   PROCEDURE [dbo].[CreateReport]
    @Id UNIQUEIDENTIFIER,
    @Title NVARCHAR(200),
    @Content NVARCHAR(MAX),
    @Description NVARCHAR(500) = NULL,
    @Type NVARCHAR(100) = NULL,
    @Priority NVARCHAR(20) = 'Medium',
    @DueDate DATETIME2 = NULL,
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
            Id, Title, Content, Description, Type, Priority, DueDate, Status, CreatedBy, Department,
            CreatedDate, LastModifiedDate, ReportNumber
        )
        VALUES (
            @Id, @Title, @Content, @Description, @Type, @Priority, @DueDate, 1, -- Draft status
            @CreatedBy, @Department, GETUTCDATE(), GETUTCDATE(), @ReportNumber
        );
        
        COMMIT TRANSACTION
        
        -- Return the created report
        SELECT 
            Id, Title, Content, Description, Type, Priority, DueDate, Status, CreatedBy, Department,
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
/****** Object:  StoredProcedure [dbo].[CreateUser]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: CreateUser
-- Description: Creates a new user account
-- =============================================
CREATE   PROCEDURE [dbo].[CreateUser]
    @Id UNIQUEIDENTIFIER,
    @Email NVARCHAR(100),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @PasswordSalt NVARCHAR(255),
    @Role INT,
    @Department INT,
    @IsActive BIT,
    @PhoneNumber NVARCHAR(50) = NULL,
    @JobTitle NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        INSERT INTO Users (
            Id, Email, FirstName, LastName, PasswordHash, PasswordSalt,
            Role, Department, IsActive, CreatedDate, LastLoginDate,
            PhoneNumber, JobTitle
        )
        VALUES (
            @Id, @Email, @FirstName, @LastName, @PasswordHash, @PasswordSalt,
            @Role, @Department, @IsActive, GETUTCDATE(), GETUTCDATE(),
            @PhoneNumber, @JobTitle
        );
        
        -- Log the user creation in audit log
        INSERT INTO AuditLogs (Id, UserId, Action, Details, Timestamp, IpAddress, UserAgent)
        VALUES (
            NEWID(), @Id, 1, -- UserCreated action
            CONCAT('User account created: ', @Email),
            GETUTCDATE(), 
            '127.0.0.1', -- Default IP for system actions
            'System'
        );
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[DeactivateUser]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: DeactivateUser
-- Description: Deactivates a user account
-- =============================================
CREATE   PROCEDURE [dbo].[DeactivateUser]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        UPDATE Users 
        SET IsActive = 0
        WHERE Id = @UserId;
        
        -- Log the deactivation
        INSERT INTO AuditLogs (Id, UserId, Action, Details, Timestamp, IpAddress, UserAgent)
        VALUES (
            NEWID(), @UserId, 4, -- UserDeactivated action
            'User account deactivated',
            GETUTCDATE(), 
            '127.0.0.1',
            'System'
        );
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetPendingApprovalsForGM]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create new GM procedure
CREATE PROCEDURE [dbo].[GetPendingApprovalsForGM]
    @UserId UNIQUEIDENTIFIER,
    @UserRole INT,
    @UserDepartment INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if CreatorId column exists, if not use CreatedBy or UserId
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reports' AND COLUMN_NAME = 'CreatorId')
    BEGIN
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
            r.CreatedBy,
            u.FirstName + ' ' + u.LastName AS CreatorName,
            u.Email AS CreatorEmail,
            r.LastModifiedDate
        FROM Reports r
        LEFT JOIN Users u ON r.CreatedBy = u.Id
        WHERE 
            (@UserRole = 3 AND r.Status IN (4, 5)) -- GM can see ManagerApproved(4) and GMReview(5)
            OR 
            (@UserRole = 2 AND r.Department = @UserDepartment AND r.Status = 3) -- LineManager sees ManagerReview(3) from their dept
        ORDER BY r.CreatedDate DESC;
    END
    ELSE IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reports' AND COLUMN_NAME = 'CreatorId')
    BEGIN
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
            r.CreatedBy AS CreatorId,
            u.FirstName + ' ' + u.LastName AS CreatorName,
            u.Email AS CreatorEmail,
            r.LastModifiedDate
        FROM Reports r
        LEFT JOIN Users u ON r.CreatedBy = u.Id
        WHERE 
            (@UserRole = 3 AND r.Status IN (4, 5)) -- GM can see ManagerApproved(4) and GMReview(5)
            OR 
            (@UserRole = 2 AND r.Department = @UserDepartment AND r.Status = 3) -- LineManager sees ManagerReview(3) from their dept
        ORDER BY r.CreatedDate DESC;
    END
    ELSE
    BEGIN
        -- Fallback: No creator column found, just return reports without creator info
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
            NULL AS CreatorId,
            'Unknown' AS CreatorName,
            'unknown@company.com' AS CreatorEmail,
            r.LastModifiedDate
        FROM Reports r
        WHERE 
            (@UserRole = 3 AND r.Status IN (4, 5)) -- GM can see ManagerApproved(4) and GMReview(5)
            OR 
            (@UserRole = 2 AND r.Department = @UserDepartment AND r.Status = 3) -- LineManager sees ManagerReview(3) from their dept
        ORDER BY r.CreatedDate DESC;
    END
END

GO
/****** Object:  StoredProcedure [dbo].[GetPendingApprovalsForManager]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- SP: GetPendingApprovalsForManager
-- Description: Gets reports pending manager approval
-- =============================================
CREATE   PROCEDURE [dbo].[GetPendingApprovalsForManager]
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
/****** Object:  StoredProcedure [dbo].[GetReportDetails]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- SP: GetReportDetails
-- Description: Gets detailed report information
-- =============================================
CREATE   PROCEDURE [dbo].[GetReportDetails]
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
/****** Object:  StoredProcedure [dbo].[GetReportsByDepartment]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- SP: GetReportsByDepartment
-- Description: Gets reports for a specific department (for managers)
-- =============================================
CREATE   PROCEDURE [dbo].[GetReportsByDepartment]
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
/****** Object:  StoredProcedure [dbo].[GetReportsByUser]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- SP: GetReportsByUser
-- Description: Gets reports created by a specific user
-- =============================================
CREATE   PROCEDURE [dbo].[GetReportsByUser]
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
/****** Object:  StoredProcedure [dbo].[GetUserByEmail]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: GetUserByEmail
-- Description: Retrieves a user by email address
-- =============================================
CREATE   PROCEDURE [dbo].[GetUserByEmail]
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Email, FirstName, LastName, PasswordHash, PasswordSalt,
        Role, Department, IsActive, CreatedDate, LastLoginDate,
        PhoneNumber, JobTitle
    FROM Users 
    WHERE Email = @Email AND IsActive = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[GetUsersByDepartment]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: GetUsersByDepartment
-- Description: Retrieves all active users by department
-- =============================================
CREATE   PROCEDURE [dbo].[GetUsersByDepartment]
    @Department INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Email, FirstName, LastName, PasswordHash, PasswordSalt,
        Role, Department, IsActive, CreatedDate, LastLoginDate,
        PhoneNumber, JobTitle
    FROM Users 
    WHERE Department = @Department AND IsActive = 1
    ORDER BY FirstName, LastName;
END
GO
/****** Object:  StoredProcedure [dbo].[GetUsersByRole]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: GetUsersByRole
-- Description: Retrieves all active users by role
-- =============================================
CREATE   PROCEDURE [dbo].[GetUsersByRole]
    @Role INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Email, FirstName, LastName, PasswordHash, PasswordSalt,
        Role, Department, IsActive, CreatedDate, LastLoginDate,
        PhoneNumber, JobTitle
    FROM Users 
    WHERE Role = @Role AND IsActive = 1
    ORDER BY FirstName, LastName;
END
GO
/****** Object:  StoredProcedure [dbo].[RejectReport]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RejectReport]
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
        LastModifiedDate = @RejectionDate
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
/****** Object:  StoredProcedure [dbo].[SearchReports]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- SP: SearchReports
-- Description: Advanced report search
-- =============================================
CREATE   PROCEDURE [dbo].[SearchReports]
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
/****** Object:  StoredProcedure [dbo].[UpdateLastLogin]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: UpdateLastLogin
-- Description: Updates user's last login timestamp
-- =============================================
CREATE   PROCEDURE [dbo].[UpdateLastLogin]
    @UserId UNIQUEIDENTIFIER,
    @LoginTime DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users 
    SET LastLoginDate = @LoginTime
    WHERE Id = @UserId;
    
    -- Log the login
    INSERT INTO AuditLogs (Id, UserId, Action, Details, Timestamp, IpAddress, UserAgent)
    VALUES (
        NEWID(), @UserId, 2, -- UserLogin action
        'User logged in',
        @LoginTime, 
        '127.0.0.1',
        'System'
    );
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateReportStatus]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- SP: UpdateReportStatus
-- Description: Updates report status and workflow
-- =============================================
CREATE   PROCEDURE [dbo].[UpdateReportStatus]
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
/****** Object:  StoredProcedure [dbo].[UpdateUserRole]    Script Date: 2025/08/31 03:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- =============================================
-- SP: UpdateUserRole
-- Description: Updates a user's role
-- =============================================
CREATE   PROCEDURE [dbo].[UpdateUserRole]
    @UserId UNIQUEIDENTIFIER,
    @NewRole INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        UPDATE Users 
        SET Role = @NewRole
        WHERE Id = @UserId;
        
        -- Log the role change
        INSERT INTO AuditLogs (Id, UserId, Action, Details, Timestamp, IpAddress, UserAgent)
        VALUES (
            NEWID(), @UserId, 3, -- UserRoleChanged action
            CONCAT('User role updated to: ', @NewRole),
            GETUTCDATE(), 
            '127.0.0.1',
            'System'
        );
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO
USE [master]
GO
ALTER DATABASE [ProjectControlsReportingToolDB] SET  READ_WRITE 
GO
