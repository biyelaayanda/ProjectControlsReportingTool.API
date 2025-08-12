-- =============================================
-- Project Controls Reporting Tool - Stored Procedures
-- Author: Auto-generated for UserRepository
-- Create date: 2025/08/12
-- Description: Stored procedures for user management operations
-- =============================================

USE [ProjectControlsReportingToolDB]
GO

-- =============================================
-- SP: GetUserByEmail
-- Description: Retrieves a user by email address
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetUserByEmail]
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

-- =============================================
-- SP: AuthenticateUser
-- Description: Authenticates user with password verification
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[AuthenticateUser]
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

-- =============================================
-- SP: AuthenticateUserByHash
-- Description: Authenticates user by password hash
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[AuthenticateUserByHash]
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

-- =============================================
-- SP: GetUsersByDepartment
-- Description: Retrieves all active users by department
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetUsersByDepartment]
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

-- =============================================
-- SP: GetUsersByRole
-- Description: Retrieves all active users by role
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetUsersByRole]
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

-- =============================================
-- SP: CheckEmailExists
-- Description: Checks if an email already exists
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[CheckEmailExists]
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

-- =============================================
-- SP: CreateUser
-- Description: Creates a new user account
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[CreateUser]
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

-- =============================================
-- SP: UpdateUserRole
-- Description: Updates a user's role
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[UpdateUserRole]
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

-- =============================================
-- SP: DeactivateUser
-- Description: Deactivates a user account
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[DeactivateUser]
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

-- =============================================
-- SP: UpdateLastLogin
-- Description: Updates user's last login timestamp
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[UpdateLastLogin]
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

PRINT 'All stored procedures created successfully for ProjectControlsReportingTool UserRepository'
