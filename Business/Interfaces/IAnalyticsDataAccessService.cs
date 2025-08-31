using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Secure interface for analytics data access using stored procedures
    /// Implements security best practices and input validation
    /// </summary>
    public interface IAnalyticsDataAccessService
    {
        /// <summary>
        /// Securely execute GetReportStatistics stored procedure
        /// </summary>
        /// <param name="userId">Authenticated user ID</param>
        /// <param name="userRole">User role for authorization</param>
        /// <param name="userDepartment">User department for filtering</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>Report statistics with role-based filtering applied</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissions</exception>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
        Task<ReportStatisticsResultDto?> GetReportStatisticsAsync(
            Guid userId, 
            UserRole userRole, 
            Department? userDepartment, 
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Securely execute GetTrendAnalysis stored procedure
        /// </summary>
        /// <param name="period">Time period for trend analysis (daily, weekly, monthly, yearly)</param>
        /// <param name="periodCount">Number of periods to analyze</param>
        /// <param name="department">Optional department filter</param>
        /// <param name="userRole">User role for authorization</param>
        /// <param name="userDepartment">User department for security filtering</param>
        /// <returns>Trend analysis data with role-based filtering</returns>
        Task<IEnumerable<TrendAnalysisResultDto>> GetTrendAnalysisAsync(
            string period, 
            int periodCount, 
            Department? department, 
            UserRole userRole, 
            Department? userDepartment);

        /// <summary>
        /// Securely execute GetPerformanceMetrics stored procedure
        /// </summary>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="userRole">User role for authorization</param>
        /// <param name="userDepartment">User department for filtering</param>
        /// <returns>Performance metrics with appropriate access control</returns>
        Task<PerformanceMetricsResultDto?> GetPerformanceMetricsAsync(
            DateTime? startDate, 
            DateTime? endDate, 
            UserRole userRole, 
            Department? userDepartment);

        /// <summary>
        /// Securely execute GetDepartmentStatistics stored procedure
        /// </summary>
        /// <param name="userRole">User role for authorization</param>
        /// <param name="userDepartment">User department for filtering</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>Department statistics with role-based access control</returns>
        Task<IEnumerable<DepartmentStatisticsResultDto>> GetDepartmentStatisticsAsync(
            UserRole userRole, 
            Department? userDepartment, 
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Securely execute GetUserStatistics stored procedure
        /// </summary>
        /// <param name="targetUserId">User ID to get statistics for</param>
        /// <param name="requestingUserId">ID of user making the request</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <param name="userDepartment">Department of requesting user</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>User statistics with access control validation</returns>
        Task<UserStatisticsResultDto?> GetUserStatisticsAsync(
            Guid targetUserId, 
            Guid requestingUserId, 
            UserRole userRole, 
            Department? userDepartment, 
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Securely execute GetSystemPerformanceMetrics stored procedure (GM only)
        /// </summary>
        /// <param name="userRole">User role for authorization (must be GM)</param>
        /// <returns>System performance metrics</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not GM</exception>
        Task<SystemPerformanceResultDto?> GetSystemPerformanceMetricsAsync(UserRole userRole);

        /// <summary>
        /// Securely execute GetAdvancedAnalyticsData stored procedure
        /// </summary>
        /// <param name="analysisType">Type of analysis (timeseries, comparative, predictive)</param>
        /// <param name="startDate">Start date for analysis</param>
        /// <param name="endDate">End date for analysis</param>
        /// <param name="departments">Optional department filter</param>
        /// <param name="granularity">Data granularity (daily, weekly, monthly)</param>
        /// <param name="userRole">User role for authorization</param>
        /// <param name="userDepartment">User department for filtering</param>
        /// <returns>Advanced analytics data with security filtering</returns>
        Task<IEnumerable<AdvancedAnalyticsResultDto>> GetAdvancedAnalyticsDataAsync(
            string analysisType, 
            DateTime startDate, 
            DateTime endDate, 
            List<Department>? departments, 
            string granularity, 
            UserRole userRole, 
            Department? userDepartment);

        /// <summary>
        /// Securely execute GetExportAnalyticsData stored procedure
        /// </summary>
        /// <param name="exportType">Type of export (reports, statistics, audit, users)</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="userId">ID of requesting user</param>
        /// <param name="userRole">Role of requesting user</param>
        /// <param name="userDepartment">Department of requesting user</param>
        /// <returns>Export data with role-based filtering</returns>
        Task<object> GetExportAnalyticsDataAsync(
            string exportType, 
            DateTime? startDate, 
            DateTime? endDate, 
            Guid userId, 
            UserRole userRole, 
            Department? userDepartment);
    }
}
