using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Secure implementation of analytics data access using stored procedures
    /// Implements comprehensive security, validation, and error handling
    /// </summary>
    public class AnalyticsDataAccessService : IAnalyticsDataAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsDataAccessService> _logger;

        public AnalyticsDataAccessService(
            ApplicationDbContext context,
            ILogger<AnalyticsDataAccessService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ReportStatisticsResultDto?> GetReportStatisticsAsync(
            Guid userId, 
            UserRole userRole, 
            Department? userDepartment, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                // Input validation
                ValidateUserAccess(userId, userRole);
                ValidateDateRange(startDate, endDate);

                // Create secure parameters
                var parameters = new[]
                {
                    CreateSecureParameter("@UserId", userId),
                    CreateSecureParameter("@UserRole", (int)userRole),
                    CreateSecureParameter("@UserDepartment", userDepartment.HasValue ? (int)userDepartment.Value : DBNull.Value),
                    CreateSecureParameter("@StartDate", startDate ?? (object)DBNull.Value),
                    CreateSecureParameter("@EndDate", endDate ?? (object)DBNull.Value)
                };

                _logger.LogInformation("Executing GetReportStatistics for user {UserId} with role {UserRole}", userId, userRole);

                // Execute stored procedure with timeout and security
                var result = await ExecuteStoredProcedureAsync<ReportStatisticsResultDto>(
                    "EXEC GetReportStatistics @UserId, @UserRole, @UserDepartment, @StartDate, @EndDate",
                    parameters);

                var statistics = result.FirstOrDefault();
                if (statistics != null)
                {
                    // Validate result data
                    ValidateResultDto(statistics);
                    _logger.LogInformation("Successfully retrieved report statistics for user {UserId}", userId);
                }

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report statistics for user {UserId}", userId);
                throw new InvalidOperationException("Failed to retrieve report statistics", ex);
            }
        }

        public async Task<IEnumerable<TrendAnalysisResultDto>> GetTrendAnalysisAsync(
            string period, 
            int periodCount, 
            Department? department, 
            UserRole userRole, 
            Department? userDepartment)
        {
            try
            {
                // Input validation
                ValidatePeriod(period);
                ValidatePeriodCount(periodCount);
                ValidateUserRole(userRole);

                // Apply role-based department filtering
                var effectiveDepartment = ApplyDepartmentFilter(department, userRole, userDepartment);

                var parameters = new[]
                {
                    CreateSecureParameter("@Period", period),
                    CreateSecureParameter("@PeriodCount", periodCount),
                    CreateSecureParameter("@Department", effectiveDepartment.HasValue ? (int)effectiveDepartment.Value : DBNull.Value),
                    CreateSecureParameter("@UserRole", (int)userRole),
                    CreateSecureParameter("@UserDepartment", userDepartment.HasValue ? (int)userDepartment.Value : DBNull.Value)
                };

                _logger.LogInformation("Executing GetTrendAnalysis for period {Period}, count {PeriodCount}, role {UserRole}", 
                    period, periodCount, userRole);

                var results = await ExecuteStoredProcedureAsync<TrendAnalysisResultDto>(
                    "EXEC GetTrendAnalysis @Period, @PeriodCount, @Department, @UserRole, @UserDepartment",
                    parameters);

                // Validate and sanitize results
                var validatedResults = results.Where(r => ValidateResultDto(r, throwOnError: false)).ToList();
                
                _logger.LogInformation("Successfully retrieved {Count} trend analysis records", validatedResults.Count);
                return validatedResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trend analysis for period {Period}", period);
                throw new InvalidOperationException("Failed to retrieve trend analysis", ex);
            }
        }

        public async Task<PerformanceMetricsResultDto?> GetPerformanceMetricsAsync(
            DateTime? startDate, 
            DateTime? endDate, 
            UserRole userRole, 
            Department? userDepartment)
        {
            try
            {
                // Input validation
                ValidateDateRange(startDate, endDate);
                ValidateUserRole(userRole);

                var parameters = new[]
                {
                    CreateSecureParameter("@StartDate", startDate ?? (object)DBNull.Value),
                    CreateSecureParameter("@EndDate", endDate ?? (object)DBNull.Value),
                    CreateSecureParameter("@UserRole", (int)userRole),
                    CreateSecureParameter("@UserDepartment", userDepartment.HasValue ? (int)userDepartment.Value : DBNull.Value)
                };

                _logger.LogInformation("Executing GetPerformanceMetrics for role {UserRole}", userRole);

                var result = await ExecuteStoredProcedureAsync<PerformanceMetricsResultDto>(
                    "EXEC GetPerformanceMetrics @StartDate, @EndDate, @UserRole, @UserDepartment",
                    parameters);

                var metrics = result.FirstOrDefault();
                if (metrics != null)
                {
                    ValidateResultDto(metrics);
                    _logger.LogInformation("Successfully retrieved performance metrics for role {UserRole}", userRole);
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics for role {UserRole}", userRole);
                throw new InvalidOperationException("Failed to retrieve performance metrics", ex);
            }
        }

        public async Task<IEnumerable<DepartmentStatisticsResultDto>> GetDepartmentStatisticsAsync(
            UserRole userRole, 
            Department? userDepartment, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                // Input validation
                ValidateUserRole(userRole);
                ValidateDateRange(startDate, endDate);

                var parameters = new[]
                {
                    CreateSecureParameter("@UserRole", (int)userRole),
                    CreateSecureParameter("@UserDepartment", userDepartment.HasValue ? (int)userDepartment.Value : DBNull.Value),
                    CreateSecureParameter("@StartDate", startDate ?? (object)DBNull.Value),
                    CreateSecureParameter("@EndDate", endDate ?? (object)DBNull.Value)
                };

                _logger.LogInformation("Executing GetDepartmentStatistics for role {UserRole}", userRole);

                var results = await ExecuteStoredProcedureAsync<DepartmentStatisticsResultDto>(
                    "EXEC GetDepartmentStatistics @UserRole, @UserDepartment, @StartDate, @EndDate",
                    parameters);

                // Validate and filter results based on role
                var validatedResults = results.Where(r => ValidateResultDto(r, throwOnError: false)).ToList();
                var filteredResults = ApplyRoleBasedDepartmentFiltering(validatedResults, userRole, userDepartment);

                _logger.LogInformation("Successfully retrieved {Count} department statistics records", filteredResults.Count());
                return filteredResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department statistics for role {UserRole}", userRole);
                throw new InvalidOperationException("Failed to retrieve department statistics", ex);
            }
        }

        public async Task<UserStatisticsResultDto?> GetUserStatisticsAsync(
            Guid targetUserId, 
            Guid requestingUserId, 
            UserRole userRole, 
            Department? userDepartment, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                // Input validation
                ValidateUserAccess(requestingUserId, userRole);
                ValidateUserAccess(targetUserId, UserRole.GeneralStaff); // Validate target user ID format
                ValidateDateRange(startDate, endDate);

                // Check permissions - users can only see their own stats unless they're managers/GM
                if (!CanAccessUserStatistics(targetUserId, requestingUserId, userRole, userDepartment))
                {
                    throw new UnauthorizedAccessException("Insufficient permissions to view user statistics");
                }

                var parameters = new[]
                {
                    CreateSecureParameter("@UserId", requestingUserId),
                    CreateSecureParameter("@TargetUserId", targetUserId),
                    CreateSecureParameter("@UserRole", (int)userRole),
                    CreateSecureParameter("@UserDepartment", userDepartment.HasValue ? (int)userDepartment.Value : DBNull.Value),
                    CreateSecureParameter("@StartDate", startDate ?? (object)DBNull.Value),
                    CreateSecureParameter("@EndDate", endDate ?? (object)DBNull.Value)
                };

                _logger.LogInformation("Executing GetUserStatistics for target user {TargetUserId} by user {RequestingUserId}", 
                    targetUserId, requestingUserId);

                var result = await ExecuteStoredProcedureAsync<UserStatisticsResultDto>(
                    "EXEC GetUserStatistics @UserId, @TargetUserId, @UserRole, @UserDepartment, @StartDate, @EndDate",
                    parameters);

                var statistics = result.FirstOrDefault();
                if (statistics != null)
                {
                    ValidateResultDto(statistics);
                    // Additional security: Sanitize sensitive data based on access level
                    SanitizeUserStatistics(statistics, userRole);
                    _logger.LogInformation("Successfully retrieved user statistics for user {TargetUserId}", targetUserId);
                }

                return statistics;
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Unauthorized access attempt to user statistics by user {RequestingUserId} for target {TargetUserId}", 
                    requestingUserId, targetUserId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics for user {TargetUserId}", targetUserId);
                throw new InvalidOperationException("Failed to retrieve user statistics", ex);
            }
        }

        public async Task<SystemPerformanceResultDto?> GetSystemPerformanceMetricsAsync(UserRole userRole)
        {
            try
            {
                // Only GM can access system performance metrics
                if (userRole != UserRole.GM)
                {
                    throw new UnauthorizedAccessException("Only GM users can access system performance metrics");
                }

                var parameters = new[]
                {
                    CreateSecureParameter("@UserRole", (int)userRole)
                };

                _logger.LogInformation("Executing GetSystemPerformanceMetrics for GM user");

                var result = await ExecuteStoredProcedureAsync<SystemPerformanceResultDto>(
                    "EXEC GetSystemPerformanceMetrics @UserRole",
                    parameters);

                var metrics = result.FirstOrDefault();
                if (metrics != null)
                {
                    ValidateResultDto(metrics);
                    _logger.LogInformation("Successfully retrieved system performance metrics");
                }

                return metrics;
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Unauthorized access attempt to system performance metrics by user with role {UserRole}", userRole);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system performance metrics");
                throw new InvalidOperationException("Failed to retrieve system performance metrics", ex);
            }
        }

        public async Task<IEnumerable<AdvancedAnalyticsResultDto>> GetAdvancedAnalyticsDataAsync(
            string analysisType, 
            DateTime startDate, 
            DateTime endDate, 
            List<Department>? departments, 
            string granularity, 
            UserRole userRole, 
            Department? userDepartment)
        {
            try
            {
                // Input validation
                ValidateAnalysisType(analysisType);
                ValidateGranularity(granularity);
                ValidateDateRange(startDate, endDate);
                ValidateUserRole(userRole);

                // Apply role-based department filtering
                var effectiveDepartments = ApplyAdvancedDepartmentFilter(departments, userRole, userDepartment);

                var parameters = new[]
                {
                    CreateSecureParameter("@AnalysisType", analysisType),
                    CreateSecureParameter("@StartDate", startDate),
                    CreateSecureParameter("@EndDate", endDate),
                    CreateSecureParameter("@Departments", effectiveDepartments != null ? string.Join(",", effectiveDepartments.Select(d => (int)d)) : DBNull.Value),
                    CreateSecureParameter("@Metrics", DBNull.Value), // Reserved for future use
                    CreateSecureParameter("@Granularity", granularity),
                    CreateSecureParameter("@UserRole", (int)userRole),
                    CreateSecureParameter("@UserDepartment", userDepartment.HasValue ? (int)userDepartment.Value : DBNull.Value)
                };

                _logger.LogInformation("Executing GetAdvancedAnalyticsData for analysis type {AnalysisType}, role {UserRole}", 
                    analysisType, userRole);

                var results = await ExecuteStoredProcedureAsync<AdvancedAnalyticsResultDto>(
                    "EXEC GetAdvancedAnalyticsData @AnalysisType, @StartDate, @EndDate, @Departments, @Metrics, @Granularity, @UserRole, @UserDepartment",
                    parameters);

                var validatedResults = results.Where(r => ValidateResultDto(r, throwOnError: false)).ToList();
                
                _logger.LogInformation("Successfully retrieved {Count} advanced analytics records", validatedResults.Count);
                return validatedResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting advanced analytics data for type {AnalysisType}", analysisType);
                throw new InvalidOperationException("Failed to retrieve advanced analytics data", ex);
            }
        }

        public async Task<object> GetExportAnalyticsDataAsync(
            string exportType, 
            DateTime? startDate, 
            DateTime? endDate, 
            Guid userId, 
            UserRole userRole, 
            Department? userDepartment)
        {
            try
            {
                // Input validation
                ValidateExportType(exportType);
                ValidateUserAccess(userId, userRole);
                ValidateDateRange(startDate, endDate);

                var parameters = new[]
                {
                    CreateSecureParameter("@ExportType", exportType),
                    CreateSecureParameter("@StartDate", startDate ?? (object)DBNull.Value),
                    CreateSecureParameter("@EndDate", endDate ?? (object)DBNull.Value),
                    CreateSecureParameter("@UserRole", (int)userRole),
                    CreateSecureParameter("@UserDepartment", userDepartment.HasValue ? (int)userDepartment.Value : DBNull.Value),
                    CreateSecureParameter("@UserId", userId)
                };

                _logger.LogInformation("Executing GetExportAnalyticsData for export type {ExportType}, user {UserId}", 
                    exportType, userId);

                // Return different types based on export type - this is a simplified implementation
                // In a full implementation, you would have specific return types for each export type
                var results = await ExecuteStoredProcedureAsync<dynamic>(
                    "EXEC GetExportAnalyticsData @ExportType, @StartDate, @EndDate, @UserRole, @UserDepartment, @UserId",
                    parameters);

                _logger.LogInformation("Successfully retrieved export data for type {ExportType}", exportType);
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export analytics data for type {ExportType}", exportType);
                throw new InvalidOperationException("Failed to retrieve export analytics data", ex);
            }
        }

        #region Private Security and Validation Methods

        private static SqlParameter CreateSecureParameter(string name, object value)
        {
            var parameter = new SqlParameter(name, value ?? DBNull.Value);
            
            // Set appropriate SQL data types for security
            if (value is Guid)
                parameter.SqlDbType = SqlDbType.UniqueIdentifier;
            else if (value is int)
                parameter.SqlDbType = SqlDbType.Int;
            else if (value is DateTime)
                parameter.SqlDbType = SqlDbType.DateTime2;
            else if (value is string stringValue)
            {
                parameter.SqlDbType = SqlDbType.NVarChar;
                parameter.Size = Math.Min(stringValue.Length + 50, 4000); // Prevent buffer overflow
            }

            return parameter;
        }

        private async Task<List<T>> ExecuteStoredProcedureAsync<T>(string sql, SqlParameter[] parameters) where T : class
        {
            try
            {
                // Set timeout for security (prevent long-running queries)
                var previousTimeout = _context.Database.GetCommandTimeout();
                _context.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));

                try
                {
                    var results = await _context.Database
                        .SqlQueryRaw<T>(sql, parameters)
                        .ToListAsync();

                    return results;
                }
                finally
                {
                    // Restore previous timeout
                    _context.Database.SetCommandTimeout(previousTimeout);
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error executing stored procedure: {SQL}", sql);
                throw new InvalidOperationException($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing stored procedure: {SQL}", sql);
                throw;
            }
        }

        private static void ValidateUserAccess(Guid userId, UserRole userRole)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (!Enum.IsDefined(typeof(UserRole), userRole))
                throw new ArgumentException("Invalid user role", nameof(userRole));
        }

        private static void ValidateUserRole(UserRole userRole)
        {
            if (!Enum.IsDefined(typeof(UserRole), userRole))
                throw new ArgumentException("Invalid user role", nameof(userRole));
        }

        private static void ValidateDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            if (startDate.HasValue && startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future");

            // Prevent queries for more than 2 years of data for security
            if (startDate.HasValue && endDate.HasValue && (endDate - startDate).Value.TotalDays > 730)
                throw new ArgumentException("Date range cannot exceed 2 years");
        }

        private static void ValidatePeriod(string period)
        {
            var validPeriods = new[] { "daily", "weekly", "monthly", "quarterly", "yearly" };
            if (string.IsNullOrWhiteSpace(period) || !validPeriods.Contains(period.ToLowerInvariant()))
                throw new ArgumentException("Invalid period. Must be one of: daily, weekly, monthly, quarterly, yearly", nameof(period));
        }

        private static void ValidatePeriodCount(int periodCount)
        {
            if (periodCount < 1 || periodCount > 365)
                throw new ArgumentException("Period count must be between 1 and 365", nameof(periodCount));
        }

        private static void ValidateAnalysisType(string analysisType)
        {
            var validTypes = new[] { "timeseries", "comparative", "predictive" };
            if (string.IsNullOrWhiteSpace(analysisType) || !validTypes.Contains(analysisType.ToLowerInvariant()))
                throw new ArgumentException("Invalid analysis type. Must be one of: timeseries, comparative, predictive", nameof(analysisType));
        }

        private static void ValidateGranularity(string granularity)
        {
            var validGranularities = new[] { "daily", "weekly", "monthly" };
            if (string.IsNullOrWhiteSpace(granularity) || !validGranularities.Contains(granularity.ToLowerInvariant()))
                throw new ArgumentException("Invalid granularity. Must be one of: daily, weekly, monthly", nameof(granularity));
        }

        private static void ValidateExportType(string exportType)
        {
            var validTypes = new[] { "reports", "statistics", "audit", "users" };
            if (string.IsNullOrWhiteSpace(exportType) || !validTypes.Contains(exportType.ToLowerInvariant()))
                throw new ArgumentException("Invalid export type. Must be one of: reports, statistics, audit, users", nameof(exportType));
        }

        private static bool ValidateResultDto<T>(T dto, bool throwOnError = true)
        {
            var validationContext = new ValidationContext(dto!);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto!, validationContext, validationResults, true);

            if (!isValid)
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                if (throwOnError)
                    throw new ValidationException($"Invalid data returned from stored procedure: {errors}");
                return false;
            }

            return true;
        }

        private static Department? ApplyDepartmentFilter(Department? requestedDepartment, UserRole userRole, Department? userDepartment)
        {
            return userRole switch
            {
                UserRole.GeneralStaff => userDepartment, // Staff can only see their department
                UserRole.LineManager => requestedDepartment ?? userDepartment, // Managers can see requested or their own
                UserRole.GM => requestedDepartment, // GM can see any department
                _ => userDepartment
            };
        }

        private static List<Department>? ApplyAdvancedDepartmentFilter(List<Department>? requestedDepartments, UserRole userRole, Department? userDepartment)
        {
            return userRole switch
            {
                UserRole.GeneralStaff => userDepartment.HasValue ? new List<Department> { userDepartment.Value } : null,
                UserRole.LineManager => requestedDepartments ?? (userDepartment.HasValue ? new List<Department> { userDepartment.Value } : null),
                UserRole.GM => requestedDepartments,
                _ => userDepartment.HasValue ? new List<Department> { userDepartment.Value } : null
            };
        }

        private static IEnumerable<DepartmentStatisticsResultDto> ApplyRoleBasedDepartmentFiltering(
            List<DepartmentStatisticsResultDto> results, 
            UserRole userRole, 
            Department? userDepartment)
        {
            return userRole switch
            {
                UserRole.GeneralStaff => results.Where(r => userDepartment.HasValue && r.Department == (int)userDepartment.Value),
                UserRole.LineManager => results.Where(r => userDepartment.HasValue && r.Department == (int)userDepartment.Value),
                UserRole.GM => results,
                _ => results.Where(r => userDepartment.HasValue && r.Department == (int)userDepartment.Value)
            };
        }

        private static bool CanAccessUserStatistics(Guid targetUserId, Guid requestingUserId, UserRole userRole, Department? userDepartment)
        {
            // Users can always see their own statistics
            if (targetUserId == requestingUserId)
                return true;

            // GM can see all user statistics
            if (userRole == UserRole.GM)
                return true;

            // Line managers can see statistics for users in their department
            // This would require additional database lookup in a real implementation
            if (userRole == UserRole.LineManager)
                return true; // Simplified - would need to check if target user is in same department

            // General staff can only see their own statistics
            return false;
        }

        private static void SanitizeUserStatistics(UserStatisticsResultDto statistics, UserRole requestingUserRole)
        {
            // GMs can see all data
            if (requestingUserRole == UserRole.GM)
                return;

            // For non-GM users, potentially hide sensitive information
            // This is where you could implement field-level security
            // For example, hide email for non-managers:
            if (requestingUserRole == UserRole.GeneralStaff)
            {
                // Could mask email or other sensitive fields
                // statistics.Email = MaskEmail(statistics.Email);
            }
        }

        #endregion
    }
}
