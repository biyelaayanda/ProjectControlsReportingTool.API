# Notification System Database Setup

## Overview

This document provides comprehensive setup instructions for the Project Controls Reporting Tool notification system database infrastructure. The notification system supports real-time in-app notifications, email notifications, and comprehensive notification management.

## Database Components

### Tables Created

1. **Notifications** - Core notification storage
2. **NotificationTemplates** - Template system for standardized notifications
3. **NotificationPreferences** - User notification preferences
4. **NotificationHistory** - Audit trail for notification delivery
5. **EmailQueue** - Email delivery queue for reliable email sending
6. **NotificationSubscriptions** - External webhook subscriptions

### Stored Procedures Created

1. **GetNotifications** - Retrieve notifications with filtering and pagination
2. **GetNotificationById** - Get specific notification details
3. **CreateNotification** - Create new notifications
4. **MarkNotificationAsRead** - Mark notifications as read
5. **DeleteNotification** - Soft delete notifications
6. **GetNotificationStats** - Get notification statistics for users
7. **CreateBulkNotifications** - Create notifications for multiple users
8. **GetRecentNotifications** - Get recent notifications for dashboard
9. **CleanupExpiredNotifications** - Maintenance procedure for expired notifications
10. **CreateReportNotification** - Create report workflow notifications

## Setup Instructions

### Prerequisites

- SQL Server 2019 or later
- ProjectControlsReportingToolDB database must exist
- Users table must be populated

### Installation Steps

1. **Run Database Setup Script**
   ```sql
   -- Execute the main setup script
   sqlcmd -S [ServerName] -d ProjectControlsReportingToolDB -i "NotificationDatabaseSetup.sql"
   ```

2. **Run Stored Procedures Script**
   ```sql
   -- Execute the stored procedures script
   sqlcmd -S [ServerName] -d ProjectControlsReportingToolDB -i "StoredProcedures/NotificationStoredProcedures.sql"
   ```

3. **Run Sample Data Script**
   ```sql
   -- Execute the sample data script
   sqlcmd -S [ServerName] -d ProjectControlsReportingToolDB -i "NotificationSampleData.sql"
   ```

### Alternative: Entity Framework Migration

If using Entity Framework migrations:

```bash
# Navigate to API project directory
cd ProjectControlsReportingTool.API

# Create migration
dotnet ef migrations add AddNotificationSystem

# Update database
dotnet ef database update
```

## API Endpoints Supported

The database supports the following API endpoints:

- `GET /api/notifications` - Get user notifications with pagination
- `GET /api/notifications/{id}` - Get specific notification
- `PUT /api/notifications/{id}/read` - Mark notification as read
- `DELETE /api/notifications/{id}` - Delete notification
- `GET /api/notifications/stats` - Get notification statistics
- `POST /api/notifications` - Create notification
- `POST /api/notifications/broadcast` - Broadcast system notifications

## Notification Types

The system supports the following notification types:

1. **ReportSubmitted** (1) - Report submission notifications
2. **ApprovalRequired** (2) - Approval requirement notifications
3. **ReportApproved** (3) - Report approval notifications
4. **ReportRejected** (4) - Report rejection notifications
5. **DueDateReminder** (5) - Due date reminder notifications
6. **EscalationNotice** (6) - Escalation notifications
7. **SystemAlert** (7) - System alert notifications
8. **UserWelcome** (8) - User welcome notifications
9. **PasswordReset** (9) - Password reset notifications
10. **AccountActivation** (10) - Account activation notifications
11. **ReportComment** (11) - Report comment notifications
12. **StatusChange** (12) - Status change notifications
13. **BulkUpdate** (13) - Bulk update notifications
14. **MaintenanceNotice** (14) - Maintenance notice notifications
15. **SecurityAlert** (15) - Security alert notifications

## Notification Priorities

- **Low** (1) - Non-urgent informational notifications
- **Normal** (2) - Standard notifications
- **High** (3) - Important notifications requiring attention
- **Critical** (4) - Urgent notifications requiring immediate attention

## Usage Examples

### Creating a Report Notification

```sql
-- Create approval required notification
EXEC CreateReportNotification 
    @ReportId = 'REPORT-GUID-HERE',
    @NotificationType = 2, -- ApprovalRequired
    @RecipientId = 'USER-GUID-HERE',
    @SenderId = 'SENDER-GUID-HERE',
    @AdditionalMessage = 'Urgent approval needed by end of day';
```

### Getting User Notifications

```sql
-- Get recent unread notifications for user
EXEC GetNotifications 
    @UserId = 'USER-GUID-HERE',
    @IsRead = 0,
    @Page = 1,
    @PageSize = 10,
    @OrderBy = 'CreatedDate',
    @OrderDirection = 'DESC';
```

### Getting Notification Statistics

```sql
-- Get notification stats for dashboard
EXEC GetNotificationStats @UserId = 'USER-GUID-HERE';
```

### Bulk Notification Creation

```sql
-- Send system alert to all line managers
EXEC CreateBulkNotifications 
    @Title = 'System Maintenance Notice',
    @Message = 'Scheduled maintenance this weekend',
    @Type = 7, -- SystemAlert
    @Priority = 2, -- Normal
    @UserRole = 2, -- LineManager
    @Category = 'System';
```

## Frontend Integration

The frontend notification service is configured to call these API endpoints:

```typescript
// Example service usage in Angular
export class NotificationService {
  async getNotifications(page: number = 1, pageSize: number = 20) {
    return firstValueFrom(
      this.http.get<any>(`${this.apiUrl}/notifications`, {
        params: { page: page.toString(), pageSize: pageSize.toString() }
      })
    );
  }

  async markAsRead(notificationId: string) {
    return firstValueFrom(
      this.http.put<any>(`${this.apiUrl}/notifications/${notificationId}/read`, {})
    );
  }

  async deleteNotification(notificationId: string) {
    return firstValueFrom(
      this.http.delete<any>(`${this.apiUrl}/notifications/${notificationId}`)
    );
  }

  async getStats() {
    return firstValueFrom(
      this.http.get<any>(`${this.apiUrl}/notifications/stats`)
    );
  }
}
```

## Maintenance

### Regular Cleanup

Run the cleanup procedure regularly to manage old notifications:

```sql
-- Clean up expired and old read notifications
EXEC CleanupExpiredNotifications;
```

### Performance Monitoring

Monitor the following indexes for performance:

```sql
-- Check index usage
SELECT 
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE s.database_id = DB_ID('ProjectControlsReportingToolDB')
    AND OBJECT_NAME(s.object_id) LIKE '%Notification%'
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;
```

## Testing

After setup, verify the system is working:

1. **Check Tables**
   ```sql
   SELECT COUNT(*) FROM Notifications;
   SELECT COUNT(*) FROM NotificationTemplates;
   SELECT COUNT(*) FROM NotificationPreferences;
   ```

2. **Test Stored Procedures**
   ```sql
   -- Test getting notifications for a user
   DECLARE @TestUserId UNIQUEIDENTIFIER;
   SELECT TOP 1 @TestUserId = Id FROM Users WHERE IsActive = 1;
   EXEC GetNotificationStats @UserId = @TestUserId;
   ```

3. **Verify Frontend Integration**
   - Start the API and frontend applications
   - Navigate to `/notifications` page
   - Verify notifications load without browser freezing
   - Test mark as read functionality
   - Test delete functionality

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Verify SQL Server is running
   - Check connection string in appsettings.json
   - Ensure database exists

2. **Foreign Key Violations**
   - Verify Users table has data
   - Check that user IDs exist before creating notifications

3. **Performance Issues**
   - Review index usage
   - Consider partitioning for large notification volumes
   - Implement archiving strategy for old notifications

### Error Messages

- **"Notification not found"** - Check notification ID and user permissions
- **"Unauthorized access"** - Verify user owns the notification
- **"Recipient not found"** - Check that recipient user exists and is active

## Security Considerations

1. **Access Control** - Users can only access their own notifications
2. **Input Validation** - All parameters are validated in stored procedures
3. **SQL Injection Prevention** - Parameterized queries used throughout
4. **Audit Trail** - All notification actions logged in NotificationHistory

## Configuration

### Environment Variables

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProjectControlsReportingToolDB;Trusted_Connection=true;"
  },
  "NotificationSettings": {
    "CleanupOldNotificationsDays": 90,
    "MaxNotificationsPerUser": 1000,
    "DefaultPageSize": 20
  }
}
```

This completes the notification system database infrastructure setup. The system is now ready to support real-time notifications with the configured frontend Angular application.
