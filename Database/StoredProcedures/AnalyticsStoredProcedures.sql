-- =============================================
-- Rand Water Project Controls Reporting Tool
-- Analytics & Statistics Stored Procedures (Phase 7)
-- Created: August 31, 2025
-- Description: Advanced analytics, statistics, and performance optimization
-- =============================================

USE ProjectControlsReportingToolDB;
GO

-- =============================================
-- SP: GetReportStatistics
-- Description: Get comprehensive report statistics
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetReportStatistics]
    @UserId UNIQUEIDENTIFIER,
    @UserRole INT,
    @UserDepartment INT = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided (last 30 days)
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();
    
    -- Total report counts
    DECLARE @TotalReports INT, @DraftReports INT, @SubmittedReports INT, @InReviewReports INT;
    DECLARE @ApprovedReports INT, @CompletedReports INT, @RejectedReports INT;
    DECLARE @MyReports INT, @PendingMyApproval INT;
    
    -- Calculate totals based on user role and permissions
    IF @UserRole = 3 -- GM (can see all reports)
    BEGIN
        SELECT 
            @TotalReports = COUNT(*),
            @DraftReports = SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END),
            @SubmittedReports = SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END),
            @InReviewReports = SUM(CASE WHEN Status IN (3, 5) THEN 1 ELSE 0 END),
            @ApprovedReports = SUM(CASE WHEN Status IN (4, 6) THEN 1 ELSE 0 END),
            @CompletedReports = SUM(CASE WHEN Status = 6 THEN 1 ELSE 0 END),
            @RejectedReports = SUM(CASE WHEN Status IN (7, 8, 9) THEN 1 ELSE 0 END)
        FROM Reports
        WHERE CreatedDate BETWEEN @StartDate AND @EndDate;
        
        SELECT @PendingMyApproval = COUNT(*)
        FROM Reports 
        WHERE Status IN (4, 5) AND CreatedDate BETWEEN @StartDate AND @EndDate;
    END
    ELSE IF @UserRole = 2 -- LineManager (department reports)
    BEGIN
        SELECT 
            @TotalReports = COUNT(*),
            @DraftReports = SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END),
            @SubmittedReports = SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END),
            @InReviewReports = SUM(CASE WHEN Status IN (3, 5) THEN 1 ELSE 0 END),
            @ApprovedReports = SUM(CASE WHEN Status IN (4, 6) THEN 1 ELSE 0 END),
            @CompletedReports = SUM(CASE WHEN Status = 6 THEN 1 ELSE 0 END),
            @RejectedReports = SUM(CASE WHEN Status IN (7, 8, 9) THEN 1 ELSE 0 END)
        FROM Reports
        WHERE Department = @UserDepartment 
        AND CreatedDate BETWEEN @StartDate AND @EndDate;
        
        SELECT @PendingMyApproval = COUNT(*)
        FROM Reports 
        WHERE Status = 3 AND Department = @UserDepartment 
        AND CreatedDate BETWEEN @StartDate AND @EndDate;
    END
    ELSE -- GeneralStaff (own reports only)
    BEGIN
        SELECT 
            @TotalReports = COUNT(*),
            @DraftReports = SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END),
            @SubmittedReports = SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END),
            @InReviewReports = SUM(CASE WHEN Status IN (3, 5) THEN 1 ELSE 0 END),
            @ApprovedReports = SUM(CASE WHEN Status IN (4, 6) THEN 1 ELSE 0 END),
            @CompletedReports = SUM(CASE WHEN Status = 6 THEN 1 ELSE 0 END),
            @RejectedReports = SUM(CASE WHEN Status IN (7, 8, 9) THEN 1 ELSE 0 END)
        FROM Reports
        WHERE CreatedBy = @UserId 
        AND CreatedDate BETWEEN @StartDate AND @EndDate;
        
        SET @PendingMyApproval = 0; -- General staff don't approve reports
    END
    
    -- Get my reports count
    SELECT @MyReports = COUNT(*)
    FROM Reports 
    WHERE CreatedBy = @UserId 
    AND CreatedDate BETWEEN @StartDate AND @EndDate;
    
    -- Return statistics
    SELECT 
        @TotalReports AS TotalReports,
        @DraftReports AS DraftReports,
        @SubmittedReports AS SubmittedReports,
        @InReviewReports AS InReviewReports,
        @ApprovedReports AS ApprovedReports,
        @CompletedReports AS CompletedReports,
        @RejectedReports AS RejectedReports,
        @MyReports AS MyReports,
        @PendingMyApproval AS PendingMyApproval,
        @StartDate AS PeriodStart,
        @EndDate AS PeriodEnd;
END
GO

-- =============================================
-- SP: GetTrendAnalysis
-- Description: Get report creation and completion trends
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetTrendAnalysis]
    @Period NVARCHAR(10) = 'daily',  -- daily, weekly, monthly, yearly
    @PeriodCount INT = 30,
    @Department INT = NULL,
    @UserRole INT,
    @UserDepartment INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StartDate DATETIME2;
    DECLARE @DateFormat NVARCHAR(20);
    DECLARE @DatePart NVARCHAR(10);
    
    -- Determine date range and formatting based on period
    IF @Period = 'daily'
    BEGIN
        SET @StartDate = DATEADD(DAY, -@PeriodCount, GETUTCDATE());
        SET @DateFormat = 'yyyy-MM-dd';
        SET @DatePart = 'day';
    END
    ELSE IF @Period = 'weekly'
    BEGIN
        SET @StartDate = DATEADD(WEEK, -@PeriodCount, GETUTCDATE());
        SET @DateFormat = 'yyyy-MM-dd';
        SET @DatePart = 'week';
    END
    ELSE IF @Period = 'monthly'
    BEGIN
        SET @StartDate = DATEADD(MONTH, -@PeriodCount, GETUTCDATE());
        SET @DateFormat = 'yyyy-MM';
        SET @DatePart = 'month';
    END
    ELSE IF @Period = 'yearly'
    BEGIN
        SET @StartDate = DATEADD(YEAR, -@PeriodCount, GETUTCDATE());
        SET @DateFormat = 'yyyy';
        SET @DatePart = 'year';
    END
    
    -- Build base query with role-based filtering
    DECLARE @SqlQuery NVARCHAR(MAX);
    SET @SqlQuery = N'
    WITH TrendData AS (
        SELECT 
            FORMAT(CreatedDate, ''' + @DateFormat + ''') AS Period,
            COUNT(*) AS CreatedCount,
            SUM(CASE WHEN Status = 6 THEN 1 ELSE 0 END) AS CompletedCount,
            SUM(CASE WHEN Status IN (7, 8, 9) THEN 1 ELSE 0 END) AS RejectedCount,
            AVG(CASE 
                WHEN CompletedDate IS NOT NULL AND SubmittedDate IS NOT NULL 
                THEN DATEDIFF(HOUR, SubmittedDate, CompletedDate) 
                ELSE NULL 
            END) AS AvgCompletionTimeHours
        FROM Reports 
        WHERE CreatedDate >= @StartDate';
    
    -- Add role-based filtering
    IF @UserRole = 2 AND @UserDepartment IS NOT NULL -- LineManager
        SET @SqlQuery = @SqlQuery + N' AND Department = @UserDepartment';
    ELSE IF @Department IS NOT NULL
        SET @SqlQuery = @SqlQuery + N' AND Department = @Department';
    
    SET @SqlQuery = @SqlQuery + N'
        GROUP BY FORMAT(CreatedDate, ''' + @DateFormat + ''')
    )
    SELECT 
        Period,
        CreatedCount,
        CompletedCount,
        RejectedCount,
        CAST(AvgCompletionTimeHours AS DECIMAL(10,2)) AS AvgCompletionTimeHours,
        CASE 
            WHEN CreatedCount > 0 
            THEN CAST((CompletedCount * 100.0 / CreatedCount) AS DECIMAL(5,2))
            ELSE 0 
        END AS CompletionRate
    FROM TrendData
    ORDER BY Period;';
    
    -- Execute dynamic query
    EXEC sp_executesql @SqlQuery, 
        N'@StartDate DATETIME2, @UserDepartment INT, @Department INT', 
        @StartDate, @UserDepartment, @Department;
END
GO

-- =============================================
-- SP: GetPerformanceMetrics
-- Description: Get system and workflow performance metrics
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetPerformanceMetrics]
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @UserRole INT,
    @UserDepartment INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided (last 30 days)
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();
    
    -- Apply role-based filtering
    DECLARE @DepartmentFilter NVARCHAR(100) = '';
    IF @UserRole = 2 AND @UserDepartment IS NOT NULL
        SET @DepartmentFilter = ' AND r.Department = ' + CAST(@UserDepartment AS NVARCHAR(10));
    
    -- Average creation to submission time
    DECLARE @AvgCreationToSubmission DECIMAL(10,2);
    DECLARE @SqlQuery NVARCHAR(MAX);
    SET @SqlQuery = N'
    SELECT @AvgCreationToSubmission = AVG(CAST(DATEDIFF(HOUR, r.CreatedDate, r.SubmittedDate) AS DECIMAL(10,2)))
    FROM Reports r
    WHERE r.SubmittedDate IS NOT NULL 
    AND r.CreatedDate BETWEEN @StartDate AND @EndDate' + @DepartmentFilter;
    
    EXEC sp_executesql @SqlQuery, 
        N'@StartDate DATETIME2, @EndDate DATETIME2, @AvgCreationToSubmission DECIMAL(10,2) OUTPUT', 
        @StartDate, @EndDate, @AvgCreationToSubmission OUTPUT;
    
    -- Average approval time (submission to completion)
    DECLARE @AvgApprovalTime DECIMAL(10,2);
    SET @SqlQuery = N'
    SELECT @AvgApprovalTime = AVG(CAST(DATEDIFF(HOUR, r.SubmittedDate, r.CompletedDate) AS DECIMAL(10,2)))
    FROM Reports r
    WHERE r.CompletedDate IS NOT NULL 
    AND r.SubmittedDate IS NOT NULL
    AND r.CreatedDate BETWEEN @StartDate AND @EndDate' + @DepartmentFilter;
    
    EXEC sp_executesql @SqlQuery, 
        N'@StartDate DATETIME2, @EndDate DATETIME2, @AvgApprovalTime DECIMAL(10,2) OUTPUT', 
        @StartDate, @EndDate, @AvgApprovalTime OUTPUT;
    
    -- Department performance (if user can see multiple departments)
    IF @UserRole = 3 OR @UserDepartment IS NULL
    BEGIN
        SELECT 
            r.Department,
            COUNT(*) AS TotalReports,
            SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) AS CompletedReports,
            SUM(CASE WHEN r.Status IN (7, 8, 9) THEN 1 ELSE 0 END) AS RejectedReports,
            CASE 
                WHEN COUNT(*) > 0 
                THEN CAST((SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) AS DECIMAL(5,2))
                ELSE 0 
            END AS CompletionRate,
            AVG(CASE 
                WHEN r.CompletedDate IS NOT NULL AND r.SubmittedDate IS NOT NULL 
                THEN CAST(DATEDIFF(HOUR, r.SubmittedDate, r.CompletedDate) AS DECIMAL(10,2))
                ELSE NULL 
            END) AS AvgApprovalTimeHours
        FROM Reports r
        WHERE r.CreatedDate BETWEEN @StartDate AND @EndDate
        GROUP BY r.Department
        ORDER BY CompletionRate DESC;
    END
    
    -- Overall metrics
    SELECT 
        ISNULL(@AvgCreationToSubmission, 0) AS AvgCreationToSubmissionHours,
        ISNULL(@AvgApprovalTime, 0) AS AvgApprovalTimeHours,
        @StartDate AS PeriodStart,
        @EndDate AS PeriodEnd;
END
GO

-- =============================================
-- SP: GetDepartmentStatistics
-- Description: Get statistics by department
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetDepartmentStatistics]
    @UserRole INT,
    @UserDepartment INT = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided (last 30 days)
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();
    
    -- Only GM can see all departments, LineManager sees their own
    IF @UserRole = 3 OR @UserDepartment IS NULL
    BEGIN
        -- All departments (GM view)
        SELECT 
            r.Department,
            COUNT(*) AS TotalReports,
            SUM(CASE WHEN r.Status = 1 THEN 1 ELSE 0 END) AS DraftReports,
            SUM(CASE WHEN r.Status = 2 THEN 1 ELSE 0 END) AS SubmittedReports,
            SUM(CASE WHEN r.Status IN (3, 5) THEN 1 ELSE 0 END) AS InReviewReports,
            SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) AS CompletedReports,
            SUM(CASE WHEN r.Status IN (7, 8, 9) THEN 1 ELSE 0 END) AS RejectedReports,
            CASE 
                WHEN COUNT(*) > 0 
                THEN CAST((SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) AS DECIMAL(5,2))
                ELSE 0 
            END AS CompletionRate,
            AVG(CASE 
                WHEN r.CompletedDate IS NOT NULL AND r.CreatedDate IS NOT NULL 
                THEN CAST(DATEDIFF(HOUR, r.CreatedDate, r.CompletedDate) AS DECIMAL(10,2))
                ELSE NULL 
            END) AS AvgTotalTimeHours
        FROM Reports r
        WHERE r.CreatedDate BETWEEN @StartDate AND @EndDate
        GROUP BY r.Department
        ORDER BY r.Department;
    END
    ELSE
    BEGIN
        -- Single department (LineManager view)
        SELECT 
            r.Department,
            COUNT(*) AS TotalReports,
            SUM(CASE WHEN r.Status = 1 THEN 1 ELSE 0 END) AS DraftReports,
            SUM(CASE WHEN r.Status = 2 THEN 1 ELSE 0 END) AS SubmittedReports,
            SUM(CASE WHEN r.Status IN (3, 5) THEN 1 ELSE 0 END) AS InReviewReports,
            SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) AS CompletedReports,
            SUM(CASE WHEN r.Status IN (7, 8, 9) THEN 1 ELSE 0 END) AS RejectedReports,
            CASE 
                WHEN COUNT(*) > 0 
                THEN CAST((SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) AS DECIMAL(5,2))
                ELSE 0 
            END AS CompletionRate,
            AVG(CASE 
                WHEN r.CompletedDate IS NOT NULL AND r.CreatedDate IS NOT NULL 
                THEN CAST(DATEDIFF(HOUR, r.CreatedDate, r.CompletedDate) AS DECIMAL(10,2))
                ELSE NULL 
            END) AS AvgTotalTimeHours
        FROM Reports r
        WHERE r.Department = @UserDepartment
        AND r.CreatedDate BETWEEN @StartDate AND @EndDate
        GROUP BY r.Department;
    END
END
GO

-- =============================================
-- SP: GetUserStatistics
-- Description: Get individual user statistics
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetUserStatistics]
    @UserId UNIQUEIDENTIFIER,
    @TargetUserId UNIQUEIDENTIFIER = NULL, -- If provided, get stats for this user instead
    @UserRole INT,
    @UserDepartment INT = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided (last 30 days)
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();
    
    -- Determine which user's stats to show
    DECLARE @StatsUserId UNIQUEIDENTIFIER = ISNULL(@TargetUserId, @UserId);
    
    -- Check permissions - users can only see their own stats unless they're managers/GM
    IF @TargetUserId IS NOT NULL AND @TargetUserId != @UserId
    BEGIN
        IF @UserRole = 1 -- GeneralStaff cannot see other users' stats
        BEGIN
            RAISERROR('Insufficient permissions to view other user statistics', 16, 1);
            RETURN;
        END
        
        -- LineManager can only see stats for users in their department
        IF @UserRole = 2
        BEGIN
            DECLARE @TargetUserDepartment INT;
            SELECT @TargetUserDepartment = Department FROM Users WHERE Id = @TargetUserId;
            
            IF @TargetUserDepartment != @UserDepartment
            BEGIN
                RAISERROR('Insufficient permissions to view user statistics from other departments', 16, 1);
                RETURN;
            END
        END
    END
    
    -- Get user statistics
    SELECT 
        u.Id AS UserId,
        u.FirstName + ' ' + u.LastName AS UserName,
        u.Email,
        u.Department,
        u.Role,
        COUNT(r.Id) AS TotalReports,
        SUM(CASE WHEN r.Status = 1 THEN 1 ELSE 0 END) AS DraftReports,
        SUM(CASE WHEN r.Status = 2 THEN 1 ELSE 0 END) AS SubmittedReports,
        SUM(CASE WHEN r.Status IN (3, 5) THEN 1 ELSE 0 END) AS InReviewReports,
        SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) AS CompletedReports,
        SUM(CASE WHEN r.Status IN (7, 8, 9) THEN 1 ELSE 0 END) AS RejectedReports,
        CASE 
            WHEN COUNT(r.Id) > 0 
            THEN CAST((SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) * 100.0 / COUNT(r.Id)) AS DECIMAL(5,2))
            ELSE 0 
        END AS CompletionRate,
        AVG(CASE 
            WHEN r.CompletedDate IS NOT NULL AND r.CreatedDate IS NOT NULL 
            THEN CAST(DATEDIFF(HOUR, r.CreatedDate, r.CompletedDate) AS DECIMAL(10,2))
            ELSE NULL 
        END) AS AvgCompletionTimeHours,
        MIN(r.CreatedDate) AS FirstReportDate,
        MAX(r.LastModifiedDate) AS LastActivityDate,
        @StartDate AS PeriodStart,
        @EndDate AS PeriodEnd
    FROM Users u
    LEFT JOIN Reports r ON u.Id = r.CreatedBy 
        AND r.CreatedDate BETWEEN @StartDate AND @EndDate
    WHERE u.Id = @StatsUserId
    GROUP BY u.Id, u.FirstName, u.LastName, u.Email, u.Department, u.Role;
END
GO

-- =============================================
-- SP: GetSystemPerformanceMetrics
-- Description: Get overall system performance metrics (GM only)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetSystemPerformanceMetrics]
    @UserRole INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only GM can access system performance metrics
    IF @UserRole != 3
    BEGIN
        RAISERROR('Insufficient permissions to view system performance metrics', 16, 1);
        RETURN;
    END
    
    DECLARE @Now DATETIME2 = GETUTCDATE();
    DECLARE @Last30Days DATETIME2 = DATEADD(DAY, -30, @Now);
    DECLARE @Last7Days DATETIME2 = DATEADD(DAY, -7, @Now);
    DECLARE @Last24Hours DATETIME2 = DATEADD(HOUR, -24, @Now);
    
    -- Overall system metrics
    SELECT 
        -- Database size and record counts
        (SELECT COUNT(*) FROM Reports) AS TotalReports,
        (SELECT COUNT(*) FROM Users WHERE IsActive = 1) AS ActiveUsers,
        (SELECT COUNT(*) FROM ReportAttachments) AS TotalAttachments,
        (SELECT COUNT(*) FROM AuditLogs) AS TotalAuditEntries,
        
        -- Activity metrics (last 30 days)
        (SELECT COUNT(*) FROM Reports WHERE CreatedDate >= @Last30Days) AS ReportsLast30Days,
        (SELECT COUNT(*) FROM Reports WHERE CreatedDate >= @Last7Days) AS ReportsLast7Days,
        (SELECT COUNT(*) FROM Reports WHERE CreatedDate >= @Last24Hours) AS ReportsLast24Hours,
        
        -- User activity (last 30 days)
        (SELECT COUNT(DISTINCT CreatedBy) FROM Reports WHERE CreatedDate >= @Last30Days) AS ActiveUsersLast30Days,
        (SELECT COUNT(*) FROM Users WHERE LastLoginDate >= @Last30Days) AS LoginsLast30Days,
        
        -- Workflow efficiency
        (SELECT AVG(CAST(DATEDIFF(HOUR, SubmittedDate, CompletedDate) AS FLOAT))
         FROM Reports 
         WHERE CompletedDate IS NOT NULL AND SubmittedDate IS NOT NULL 
         AND CompletedDate >= @Last30Days) AS AvgApprovalTimeHours,
        
        -- Error rates
        (SELECT COUNT(*) FROM AuditLogs WHERE Timestamp >= @Last30Days AND Details LIKE '%error%') AS ErrorsLast30Days,
        
        -- Storage metrics
        (SELECT SUM(FileSize) FROM ReportAttachments WHERE IsActive = 1) AS TotalStorageBytes,
        
        -- Performance indicators
        @Now AS GeneratedAt;
    
    -- Department performance summary
    SELECT 
        r.Department,
        COUNT(*) AS TotalReports,
        AVG(CASE 
            WHEN r.CompletedDate IS NOT NULL AND r.SubmittedDate IS NOT NULL 
            THEN CAST(DATEDIFF(HOUR, r.SubmittedDate, r.CompletedDate) AS FLOAT)
            ELSE NULL 
        END) AS AvgApprovalTimeHours,
        CAST((SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) AS DECIMAL(5,2)) AS CompletionRate
    FROM Reports r
    WHERE r.CreatedDate >= @Last30Days
    GROUP BY r.Department
    ORDER BY CompletionRate DESC;
    
    -- Recent activity summary
    SELECT 
        FORMAT(CreatedDate, 'yyyy-MM-dd') AS Date,
        COUNT(*) AS ReportsCreated,
        SUM(CASE WHEN Status = 6 THEN 1 ELSE 0 END) AS ReportsCompleted
    FROM Reports
    WHERE CreatedDate >= @Last7Days
    GROUP BY FORMAT(CreatedDate, 'yyyy-MM-dd')
    ORDER BY Date DESC;
END
GO

-- =============================================
-- SP: GetAdvancedAnalyticsData
-- Description: Get data for advanced analytics (time series, comparative, predictive)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetAdvancedAnalyticsData]
    @AnalysisType NVARCHAR(50), -- 'timeseries', 'comparative', 'predictive'
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @Departments NVARCHAR(MAX) = NULL, -- Comma-separated list
    @Metrics NVARCHAR(MAX) = NULL, -- Comma-separated list
    @Granularity NVARCHAR(20) = 'daily', -- daily, weekly, monthly
    @UserRole INT,
    @UserDepartment INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Apply role-based filtering
    DECLARE @DepartmentFilter NVARCHAR(500) = '';
    IF @UserRole = 2 AND @UserDepartment IS NOT NULL
        SET @DepartmentFilter = ' AND r.Department = ' + CAST(@UserDepartment AS NVARCHAR(10));
    ELSE IF @Departments IS NOT NULL
        SET @DepartmentFilter = ' AND r.Department IN (' + @Departments + ')';
    
    -- Determine date format based on granularity
    DECLARE @DateFormat NVARCHAR(20);
    IF @Granularity = 'daily'
        SET @DateFormat = 'yyyy-MM-dd';
    ELSE IF @Granularity = 'weekly'
        SET @DateFormat = 'yyyy-MM-dd'; -- We'll group by week later
    ELSE IF @Granularity = 'monthly'
        SET @DateFormat = 'yyyy-MM';
    
    IF @AnalysisType = 'timeseries'
    BEGIN
        -- Time series analysis
        DECLARE @TimeSeriesQuery NVARCHAR(MAX);
        SET @TimeSeriesQuery = N'
        WITH TimeSeriesData AS (
            SELECT 
                FORMAT(r.CreatedDate, ''' + @DateFormat + ''') AS Period,
                r.Department,
                COUNT(*) AS ReportsCreated,
                SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) AS ReportsCompleted,
                SUM(CASE WHEN r.Status IN (7, 8, 9) THEN 1 ELSE 0 END) AS ReportsRejected,
                AVG(CASE 
                    WHEN r.CompletedDate IS NOT NULL AND r.SubmittedDate IS NOT NULL 
                    THEN CAST(DATEDIFF(HOUR, r.SubmittedDate, r.CompletedDate) AS FLOAT)
                    ELSE NULL 
                END) AS AvgApprovalTimeHours
            FROM Reports r
            WHERE r.CreatedDate BETWEEN @StartDate AND @EndDate' + @DepartmentFilter + '
            GROUP BY FORMAT(r.CreatedDate, ''' + @DateFormat + '''), r.Department
        )
        SELECT * FROM TimeSeriesData ORDER BY Period, Department;';
        
        EXEC sp_executesql @TimeSeriesQuery, 
            N'@StartDate DATETIME2, @EndDate DATETIME2', 
            @StartDate, @EndDate;
    END
    ELSE IF @AnalysisType = 'comparative'
    BEGIN
        -- Comparative analysis between departments
        SELECT 
            r.Department,
            COUNT(*) AS TotalReports,
            SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) AS CompletedReports,
            SUM(CASE WHEN r.Status IN (7, 8, 9) THEN 1 ELSE 0 END) AS RejectedReports,
            CAST((SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) AS DECIMAL(5,2)) AS CompletionRate,
            AVG(CASE 
                WHEN r.CompletedDate IS NOT NULL AND r.SubmittedDate IS NOT NULL 
                THEN CAST(DATEDIFF(HOUR, r.SubmittedDate, r.CompletedDate) AS FLOAT)
                ELSE NULL 
            END) AS AvgApprovalTimeHours,
            AVG(CASE 
                WHEN r.SubmittedDate IS NOT NULL 
                THEN CAST(DATEDIFF(HOUR, r.CreatedDate, r.SubmittedDate) AS FLOAT)
                ELSE NULL 
            END) AS AvgCreationTimeHours
        FROM Reports r
        WHERE r.CreatedDate BETWEEN @StartDate AND @EndDate
        GROUP BY r.Department
        ORDER BY CompletionRate DESC;
    END
    ELSE IF @AnalysisType = 'predictive'
    BEGIN
        -- Predictive analysis data (historical patterns for ML)
        WITH MonthlyTrends AS (
            SELECT 
                YEAR(r.CreatedDate) AS Year,
                MONTH(r.CreatedDate) AS Month,
                r.Department,
                COUNT(*) AS ReportsCreated,
                SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) AS ReportsCompleted,
                AVG(CASE 
                    WHEN r.CompletedDate IS NOT NULL AND r.CreatedDate IS NOT NULL 
                    THEN CAST(DATEDIFF(HOUR, r.CreatedDate, r.CompletedDate) AS FLOAT)
                    ELSE NULL 
                END) AS AvgCompletionTimeHours
            FROM Reports r
            WHERE r.CreatedDate >= DATEADD(YEAR, -2, @StartDate) -- Get 2 years of data for prediction
            GROUP BY YEAR(r.CreatedDate), MONTH(r.CreatedDate), r.Department
        )
        SELECT 
            Year,
            Month,
            Department,
            ReportsCreated,
            ReportsCompleted,
            AvgCompletionTimeHours,
            LAG(ReportsCreated, 1) OVER (PARTITION BY Department ORDER BY Year, Month) AS PreviousMonthCreated,
            LAG(ReportsCreated, 12) OVER (PARTITION BY Department ORDER BY Year, Month) AS SameMonthLastYear
        FROM MonthlyTrends
        ORDER BY Year DESC, Month DESC, Department;
    END
END
GO

-- =============================================
-- SP: GetExportAnalyticsData
-- Description: Get comprehensive data for exports
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetExportAnalyticsData]
    @ExportType NVARCHAR(50), -- 'reports', 'statistics', 'audit', 'users'
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @UserRole INT,
    @UserDepartment INT = NULL,
    @UserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(MONTH, -3, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();
    
    IF @ExportType = 'reports'
    BEGIN
        -- Export reports data with role-based filtering
        IF @UserRole = 3 -- GM can see all
        BEGIN
            SELECT 
                r.Id, r.Title, r.Description, r.Status, r.Department, r.Priority, r.Type,
                r.CreatedDate, r.LastModifiedDate, r.SubmittedDate, r.CompletedDate,
                r.ReportNumber, r.RejectionReason,
                u.FirstName + ' ' + u.LastName AS CreatedByName,
                u.Email AS CreatedByEmail,
                u.Department AS CreatorDepartment
            FROM Reports r
            INNER JOIN Users u ON r.CreatedBy = u.Id
            WHERE r.CreatedDate BETWEEN @StartDate AND @EndDate
            ORDER BY r.CreatedDate DESC;
        END
        ELSE IF @UserRole = 2 -- LineManager sees department reports
        BEGIN
            SELECT 
                r.Id, r.Title, r.Description, r.Status, r.Department, r.Priority, r.Type,
                r.CreatedDate, r.LastModifiedDate, r.SubmittedDate, r.CompletedDate,
                r.ReportNumber, r.RejectionReason,
                u.FirstName + ' ' + u.LastName AS CreatedByName,
                u.Email AS CreatedByEmail,
                u.Department AS CreatorDepartment
            FROM Reports r
            INNER JOIN Users u ON r.CreatedBy = u.Id
            WHERE r.Department = @UserDepartment
            AND r.CreatedDate BETWEEN @StartDate AND @EndDate
            ORDER BY r.CreatedDate DESC;
        END
        ELSE -- GeneralStaff sees own reports
        BEGIN
            SELECT 
                r.Id, r.Title, r.Description, r.Status, r.Department, r.Priority, r.Type,
                r.CreatedDate, r.LastModifiedDate, r.SubmittedDate, r.CompletedDate,
                r.ReportNumber, r.RejectionReason,
                u.FirstName + ' ' + u.LastName AS CreatedByName,
                u.Email AS CreatedByEmail,
                u.Department AS CreatorDepartment
            FROM Reports r
            INNER JOIN Users u ON r.CreatedBy = u.Id
            WHERE r.CreatedBy = @UserId
            AND r.CreatedDate BETWEEN @StartDate AND @EndDate
            ORDER BY r.CreatedDate DESC;
        END
    END
    ELSE IF @ExportType = 'statistics'
    BEGIN
        -- Export statistics summary
        EXEC GetReportStatistics @UserId, @UserRole, @UserDepartment, @StartDate, @EndDate;
        EXEC GetDepartmentStatistics @UserRole, @UserDepartment, @StartDate, @EndDate;
        EXEC GetPerformanceMetrics @StartDate, @EndDate, @UserRole, @UserDepartment;
    END
    ELSE IF @ExportType = 'audit' AND @UserRole IN (2, 3) -- Manager and GM only
    BEGIN
        -- Export audit logs
        SELECT 
            al.Id, al.Action, al.Timestamp, al.Details, al.IpAddress, al.UserAgent,
            u.FirstName + ' ' + u.LastName AS UserName,
            u.Email AS UserEmail,
            r.Title AS ReportTitle,
            r.ReportNumber
        FROM AuditLogs al
        INNER JOIN Users u ON al.UserId = u.Id
        LEFT JOIN Reports r ON al.ReportId = r.Id
        WHERE al.Timestamp BETWEEN @StartDate AND @EndDate
        ORDER BY al.Timestamp DESC;
    END
    ELSE IF @ExportType = 'users' AND @UserRole = 3 -- GM only
    BEGIN
        -- Export users data
        SELECT 
            u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.Department,
            u.IsActive, u.CreatedDate, u.LastLoginDate, u.PhoneNumber, u.JobTitle,
            COUNT(r.Id) AS TotalReports,
            SUM(CASE WHEN r.Status = 6 THEN 1 ELSE 0 END) AS CompletedReports
        FROM Users u
        LEFT JOIN Reports r ON u.Id = r.CreatedBy 
            AND r.CreatedDate BETWEEN @StartDate AND @EndDate
        GROUP BY u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.Department,
                 u.IsActive, u.CreatedDate, u.LastLoginDate, u.PhoneNumber, u.JobTitle
        ORDER BY u.LastLoginDate DESC;
    END
END
GO

PRINT 'Phase 7 Analytics stored procedures created successfully!';
PRINT 'Created procedures:';
PRINT '  - GetReportStatistics: Comprehensive report statistics with role-based filtering';
PRINT '  - GetTrendAnalysis: Time-based trend analysis for reports';
PRINT '  - GetPerformanceMetrics: System and workflow performance metrics';
PRINT '  - GetDepartmentStatistics: Department-specific performance statistics';
PRINT '  - GetUserStatistics: Individual user performance metrics';
PRINT '  - GetSystemPerformanceMetrics: Overall system performance (GM only)';
PRINT '  - GetAdvancedAnalyticsData: Data for time series, comparative, and predictive analytics';
PRINT '  - GetExportAnalyticsData: Comprehensive data export with role-based access control';
