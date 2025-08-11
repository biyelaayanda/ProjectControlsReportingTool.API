-- =============================================
-- Project Controls Reporting Tool Database
-- Phase 2: User Management Stored Procedures
-- =============================================

USE ProjectControlsReportingToolDB;
GO

-- =============================================
-- SP_CreateUser: Create a new user with password hashing
-- =============================================
CREATE OR ALTER PROCEDURE SP_CreateUser
    @Id UNIQUEIDENTIFIER,
    @Email NVARCHAR(100),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PasswordHash NVARCHAR(MAX),
    @PasswordSalt NVARCHAR(MAX),
    @Role INT,
    @Department INT,
    @PhoneNumber NVARCHAR(50) = NULL,
    @JobTitle NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if email already exists
        IF EXISTS(SELECT 1 FROM Users WHERE Email = @Email)
        BEGIN
            RAISERROR('Email already exists', 16, 1);
            RETURN;
        END
        
        -- Insert new user
        INSERT INTO Users (
            Id, Email, FirstName, LastName, PasswordHash, PasswordSalt,
            Role, Department, PhoneNumber, JobTitle, IsActive, CreatedDate
        )
        VALUES (
            @Id, @Email, @FirstName, @LastName, @PasswordHash, @PasswordSalt,
            @Role, @Department, @PhoneNumber, @JobTitle, 1, GETUTCDATE()
        );
        
        -- Log the action
        INSERT INTO AuditLogs (Id, Action, UserId, Details, Timestamp)
        VALUES (NEWID(), 1, @Id, 'User created: ' + @Email, GETUTCDATE());
        
        SELECT * FROM Users WHERE Id = @Id;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- =============================================
-- SP_AuthenticateUser: Authenticate user login
-- =============================================
CREATE OR ALTER PROCEDURE SP_AuthenticateUser
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId UNIQUEIDENTIFIER;
    DECLARE @StoredHash NVARCHAR(MAX);
    DECLARE @IsActive BIT;
    
    -- Get user details
    SELECT @UserId = Id, @StoredHash = PasswordHash, @IsActive = IsActive
    FROM Users 
    WHERE Email = @Email;
    
    -- Check if user exists and is active
    IF @UserId IS NULL OR @IsActive = 0
    BEGIN
        SELECT NULL AS Id, NULL AS Email; -- Return empty result
        RETURN;
    END
    
    -- Verify password (simplified - in real implementation, compare hashed passwords)
    IF @StoredHash = @PasswordHash
    BEGIN
        -- Update last login
        UPDATE Users SET LastLoginDate = GETUTCDATE() WHERE Id = @UserId;
        
        -- Log the action
        INSERT INTO AuditLogs (Id, Action, UserId, Details, Timestamp)
        VALUES (NEWID(), 2, @UserId, 'User authenticated: ' + @Email, GETUTCDATE());
        
        -- Return user details
        SELECT * FROM Users WHERE Id = @UserId;
    END
    ELSE
    BEGIN
        SELECT NULL AS Id, NULL AS Email; -- Return empty result for invalid password
    END
END
GO

-- =============================================
-- SP_GetUsersByDepartment: Get users by department
-- =============================================
CREATE OR ALTER PROCEDURE SP_GetUsersByDepartment
    @Department INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * 
    FROM Users 
    WHERE Department = @Department AND IsActive = 1
    ORDER BY FirstName, LastName;
END
GO

-- =============================================
-- SP_UpdateUserRole: Update user role
-- =============================================
CREATE OR ALTER PROCEDURE SP_UpdateUserRole
    @UserId UNIQUEIDENTIFIER,
    @NewRole INT,
    @UpdatedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @OldRole INT;
        DECLARE @Email NVARCHAR(100);
        
        -- Get current role
        SELECT @OldRole = Role, @Email = Email 
        FROM Users 
        WHERE Id = @UserId;
        
        IF @UserId IS NULL
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END
        
        -- Update role
        UPDATE Users 
        SET Role = @NewRole 
        WHERE Id = @UserId;
        
        -- Log the action
        INSERT INTO AuditLogs (Id, Action, UserId, Details, Timestamp)
        VALUES (NEWID(), 2, @UpdatedBy, 
                'User role updated for ' + @Email + ' from ' + CAST(@OldRole AS NVARCHAR) + ' to ' + CAST(@NewRole AS NVARCHAR), 
                GETUTCDATE());
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        SELECT 0 AS Success;
        THROW;
    END CATCH
END
GO

-- =============================================
-- SP_DeactivateUser: Deactivate a user account
-- =============================================
CREATE OR ALTER PROCEDURE SP_DeactivateUser
    @UserId UNIQUEIDENTIFIER,
    @DeactivatedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @Email NVARCHAR(100);
        
        -- Get user email
        SELECT @Email = Email 
        FROM Users 
        WHERE Id = @UserId;
        
        IF @UserId IS NULL
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END
        
        -- Deactivate user
        UPDATE Users 
        SET IsActive = 0 
        WHERE Id = @UserId;
        
        -- Log the action
        INSERT INTO AuditLogs (Id, Action, UserId, Details, Timestamp)
        VALUES (NEWID(), 2, @DeactivatedBy, 'User deactivated: ' + @Email, GETUTCDATE());
        
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        SELECT 0 AS Success;
        THROW;
    END CATCH
END
GO

-- =============================================
-- SP_GetUserById: Get user by ID
-- =============================================
CREATE OR ALTER PROCEDURE SP_GetUserById
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * 
    FROM Users 
    WHERE Id = @UserId;
END
GO

-- =============================================
-- SP_UpdateUserProfile: Update user profile information
-- =============================================
CREATE OR ALTER PROCEDURE SP_UpdateUserProfile
    @UserId UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(50) = NULL,
    @JobTitle NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @Email NVARCHAR(100);
        
        -- Get user email
        SELECT @Email = Email 
        FROM Users 
        WHERE Id = @UserId;
        
        IF @UserId IS NULL
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END
        
        -- Update profile
        UPDATE Users 
        SET FirstName = @FirstName,
            LastName = @LastName,
            PhoneNumber = @PhoneNumber,
            JobTitle = @JobTitle
        WHERE Id = @UserId;
        
        -- Log the action
        INSERT INTO AuditLogs (Id, Action, UserId, Details, Timestamp)
        VALUES (NEWID(), 2, @UserId, 'User profile updated: ' + @Email, GETUTCDATE());
        
        SELECT * FROM Users WHERE Id = @UserId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- =============================================
-- Grant permissions (if needed)
-- =============================================
PRINT 'Phase 2 User Management Stored Procedures created successfully!';
