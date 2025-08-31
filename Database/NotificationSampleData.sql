-- =============================================
-- Notification System Sample Data and Templates
-- Description: Insert default notification templates and sample data
-- Version: 1.0
-- Date: 2025-01-01
-- =============================================

USE [ProjectControlsReportingToolDB]
GO

-- =============================================
-- Insert Default System User for Notifications
-- =============================================
DECLARE @SystemUserId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

-- Check if system user exists, if not create it
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
END

-- =============================================
-- Insert Default Notification Templates
-- =============================================

-- Clear existing templates first (for re-run safety)
DELETE FROM NotificationTemplates WHERE IsSystem = 1;

-- Template 1: Report Submitted
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES (
    NEWID(), 'ReportSubmitted', 'Report Submitted: {{ReportNumber}}',
    '<h2>Report Submitted</h2><p>Report <strong>{{ReportTitle}}</strong> ({{ReportNumber}}) has been submitted by {{SubmitterName}} for your review.</p><p><a href="{{ActionUrl}}" style="background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">View Report</a></p>',
    'Report Submitted: {{ReportTitle}} ({{ReportNumber}}) has been submitted by {{SubmitterName}} for your review. View at: {{ActionUrl}}',
    1, 'Reports', 'Template for report submission notifications',
    1, 1, '["ReportTitle", "ReportNumber", "SubmitterName", "ActionUrl"]',
    @SystemUserId, GETUTCDATE(), 1
);

-- Template 2: Approval Required
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES (
    NEWID(), 'ApprovalRequired', 'Approval Required: {{ReportNumber}}',
    '<h2 style="color: #dc3545;">Approval Required</h2><p>Report <strong>{{ReportTitle}}</strong> ({{ReportNumber}}) requires your approval.</p><p><strong>Priority:</strong> {{Priority}}</p><p><strong>Due Date:</strong> {{DueDate}}</p><p><a href="{{ActionUrl}}" style="background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Review & Approve</a></p>',
    'APPROVAL REQUIRED: Report {{ReportTitle}} ({{ReportNumber}}) requires your approval. Priority: {{Priority}}. Due: {{DueDate}}. Review at: {{ActionUrl}}',
    2, 'Reports', 'Template for approval requirement notifications',
    1, 1, '["ReportTitle", "ReportNumber", "Priority", "DueDate", "ActionUrl"]',
    @SystemUserId, GETUTCDATE(), 1
);

-- Template 3: Report Approved
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES (
    NEWID(), 'ReportApproved', 'Report Approved: {{ReportNumber}}',
    '<h2 style="color: #28a745;">Report Approved</h2><p>Great news! Report <strong>{{ReportTitle}}</strong> ({{ReportNumber}}) has been approved by {{ApproverName}}.</p><p><strong>Approved Date:</strong> {{ApprovalDate}}</p><p><a href="{{ActionUrl}}" style="background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">View Report</a></p>',
    'Report Approved: {{ReportTitle}} ({{ReportNumber}}) has been approved by {{ApproverName}} on {{ApprovalDate}}. View at: {{ActionUrl}}',
    3, 'Reports', 'Template for report approval notifications',
    1, 1, '["ReportTitle", "ReportNumber", "ApproverName", "ApprovalDate", "ActionUrl"]',
    @SystemUserId, GETUTCDATE(), 1
);

-- Template 4: Report Rejected
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES (
    NEWID(), 'ReportRejected', 'Report Rejected: {{ReportNumber}}',
    '<h2 style="color: #dc3545;">Report Rejected</h2><p>Report <strong>{{ReportTitle}}</strong> ({{ReportNumber}}) has been rejected by {{RejectorName}}.</p><p><strong>Reason:</strong> {{RejectionReason}}</p><p><strong>Rejected Date:</strong> {{RejectionDate}}</p><p><a href="{{ActionUrl}}" style="background-color: #ffc107; color: black; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Review & Revise</a></p>',
    'Report Rejected: {{ReportTitle}} ({{ReportNumber}}) has been rejected by {{RejectorName}} on {{RejectionDate}}. Reason: {{RejectionReason}}. Review at: {{ActionUrl}}',
    4, 'Reports', 'Template for report rejection notifications',
    1, 1, '["ReportTitle", "ReportNumber", "RejectorName", "RejectionReason", "RejectionDate", "ActionUrl"]',
    @SystemUserId, GETUTCDATE(), 1
);

-- Template 5: Due Date Reminder
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES (
    NEWID(), 'DueDateReminder', 'Due Date Reminder: {{ReportNumber}}',
    '<h2 style="color: #ffc107;">Due Date Reminder</h2><p>Report <strong>{{ReportTitle}}</strong> ({{ReportNumber}}) is approaching its due date.</p><p><strong>Due Date:</strong> {{DueDate}}</p><p><strong>Days Remaining:</strong> {{DaysRemaining}}</p><p><a href="{{ActionUrl}}" style="background-color: #ffc107; color: black; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Take Action</a></p>',
    'Due Date Reminder: Report {{ReportTitle}} ({{ReportNumber}}) is due on {{DueDate}}. {{DaysRemaining}} days remaining. Take action at: {{ActionUrl}}',
    5, 'Reports', 'Template for due date reminder notifications',
    1, 1, '["ReportTitle", "ReportNumber", "DueDate", "DaysRemaining", "ActionUrl"]',
    @SystemUserId, GETUTCDATE(), 1
);

-- Template 6: System Alert
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES (
    NEWID(), 'SystemAlert', 'System Alert: {{AlertTitle}}',
    '<h2 style="color: #dc3545;">System Alert</h2><p><strong>{{AlertTitle}}</strong></p><p>{{AlertMessage}}</p><p><strong>Alert Level:</strong> {{AlertLevel}}</p><p><strong>Time:</strong> {{AlertTime}}</p>',
    'System Alert: {{AlertTitle}} - {{AlertMessage}}. Alert Level: {{AlertLevel}}. Time: {{AlertTime}}',
    7, 'System', 'Template for system alert notifications',
    1, 1, '["AlertTitle", "AlertMessage", "AlertLevel", "AlertTime"]',
    @SystemUserId, GETUTCDATE(), 1
);

-- Template 7: User Welcome
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES (
    NEWID(), 'UserWelcome', 'Welcome to Project Controls Reporting Tool',
    '<h2 style="color: #007bff;">Welcome to Project Controls Reporting Tool</h2><p>Hello <strong>{{UserName}}</strong>,</p><p>Welcome to the Project Controls Reporting Tool! Your account has been successfully created.</p><p><strong>Role:</strong> {{UserRole}}</p><p><strong>Department:</strong> {{UserDepartment}}</p><p>You can now start creating and managing reports.</p><p><a href="{{LoginUrl}}" style="background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Login Now</a></p>',
    'Welcome to Project Controls Reporting Tool! Hello {{UserName}}, your account has been created. Role: {{UserRole}}, Department: {{UserDepartment}}. Login at: {{LoginUrl}}',
    8, 'Users', 'Template for welcome notifications to new users',
    1, 1, '["UserName", "UserRole", "UserDepartment", "LoginUrl"]',
    @SystemUserId, GETUTCDATE(), 1
);

-- Template 8: Status Change
INSERT INTO NotificationTemplates (
    Id, Name, Subject, HtmlTemplate, TextTemplate, Type, Category, Description,
    IsActive, IsSystem, Variables, CreatedBy, CreatedDate, Version
)
VALUES (
    NEWID(), 'StatusChange', 'Status Update: {{ReportNumber}}',
    '<h2>Report Status Updated</h2><p>Report <strong>{{ReportTitle}}</strong> ({{ReportNumber}}) status has been updated.</p><p><strong>Previous Status:</strong> {{PreviousStatus}}</p><p><strong>New Status:</strong> {{NewStatus}}</p><p><strong>Updated By:</strong> {{UpdatedBy}}</p><p><strong>Update Time:</strong> {{UpdateTime}}</p><p><a href="{{ActionUrl}}" style="background-color: #17a2b8; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">View Report</a></p>',
    'Status Update: Report {{ReportTitle}} ({{ReportNumber}}) status changed from {{PreviousStatus}} to {{NewStatus}} by {{UpdatedBy}} at {{UpdateTime}}. View at: {{ActionUrl}}',
    12, 'Reports', 'Template for report status change notifications',
    1, 1, '["ReportTitle", "ReportNumber", "PreviousStatus", "NewStatus", "UpdatedBy", "UpdateTime", "ActionUrl"]',
    @SystemUserId, GETUTCDATE(), 1
);

-- =============================================
-- Insert Default Notification Preferences for All Users
-- =============================================

-- Delete existing preferences to avoid duplicates
DELETE FROM NotificationPreferences;

-- Insert default preferences for all existing users
INSERT INTO NotificationPreferences (
    Id, UserId, NotificationType, EmailEnabled, PushEnabled, InAppEnabled, 
    SmsEnabled, IsActive, CreatedDate
)
SELECT 
    NEWID(), u.Id, nt.NotificationType, 
    CASE 
        WHEN nt.NotificationType IN (1, 2, 3, 4, 5, 6, 7) THEN 1 -- Enable email for important notifications
        ELSE 0 
    END AS EmailEnabled,
    1 AS PushEnabled, -- Enable push for all by default
    1 AS InAppEnabled, -- Enable in-app for all by default
    0 AS SmsEnabled, -- SMS disabled by default
    1 AS IsActive,
    GETUTCDATE() AS CreatedDate
FROM Users u
CROSS JOIN (
    VALUES (1), (2), (3), (4), (5), (6), (7), (8), (9), (10), (11), (12), (13), (14), (15)
) AS nt(NotificationType)
WHERE u.IsActive = 1;

-- =============================================
-- Create Sample Notifications for Testing (if users exist)
-- =============================================

-- Only create sample notifications if there are active users
IF EXISTS (SELECT 1 FROM Users WHERE IsActive = 1 AND Id != @SystemUserId)
BEGIN
    DECLARE @SampleUserId UNIQUEIDENTIFIER;
    SELECT TOP 1 @SampleUserId = Id FROM Users WHERE IsActive = 1 AND Id != @SystemUserId;
    
    -- Sample notification 1: System welcome
    INSERT INTO Notifications (
        Id, Title, Message, Type, Priority, RecipientId, SenderId,
        Category, ActionUrl, ActionText, CreatedDate
    )
    VALUES (
        NEWID(), 'Welcome to Project Controls Reporting Tool',
        'Welcome! Your account has been set up successfully. You can now start creating and managing reports.',
        8, 2, @SampleUserId, @SystemUserId, -- UserWelcome, Normal priority
        'System', '/dashboard', 'Go to Dashboard', GETUTCDATE()
    );
    
    -- Sample notification 2: System alert
    INSERT INTO Notifications (
        Id, Title, Message, Type, Priority, RecipientId, SenderId,
        Category, ActionUrl, ActionText, CreatedDate
    )
    VALUES (
        NEWID(), 'System Maintenance Scheduled',
        'Scheduled maintenance will occur this weekend from 2:00 AM to 4:00 AM. The system will be temporarily unavailable.',
        7, 2, @SampleUserId, @SystemUserId, -- SystemAlert, Normal priority
        'System', '/maintenance', 'View Details', GETUTCDATE()
    );
END

PRINT 'Notification templates and sample data inserted successfully!'

-- =============================================
-- Display Summary
-- =============================================
SELECT 'Notification Templates' AS Component, COUNT(*) AS Count FROM NotificationTemplates WHERE IsSystem = 1
UNION ALL
SELECT 'Notification Preferences' AS Component, COUNT(*) AS Count FROM NotificationPreferences
UNION ALL
SELECT 'Sample Notifications' AS Component, COUNT(*) AS Count FROM Notifications
UNION ALL
SELECT 'Active Users' AS Component, COUNT(*) AS Count FROM Users WHERE IsActive = 1;
