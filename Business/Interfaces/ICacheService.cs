namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Cache statistics data
    /// </summary>
    public class CacheStatistics
    {
        public int TotalKeys { get; set; }
        public long TotalMemoryUsage { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public double HitRatio => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Interface for distributed caching service
    /// Provides comprehensive caching capabilities for performance optimization
    /// </summary>
    public interface ICacheService
    {
        #region Generic Cache Operations

        /// <summary>
        /// Gets cached item by key
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached item or null if not found</returns>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Sets cached item with key and expiration
        /// </summary>
        /// <typeparam name="T">Type of item to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Item to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Removes cached item by key
        /// </summary>
        /// <param name="key">Cache key</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Gets or sets cached item using a factory function
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="factory">Factory function to create item if not cached</param>
        /// <param name="expiration">Optional expiration time</param>
        /// <returns>Cached or newly created item</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;

        #endregion

        #region Report Caching

        /// <summary>
        /// Caches report data
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="reportData">Report data to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheReportAsync(Guid reportId, object reportData, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached report data
        /// </summary>
        /// <typeparam name="T">Type of report data</typeparam>
        /// <param name="reportId">Report ID</param>
        /// <returns>Cached report data or null</returns>
        Task<T?> GetCachedReportAsync<T>(Guid reportId) where T : class;

        /// <summary>
        /// Removes cached report data
        /// </summary>
        /// <param name="reportId">Report ID</param>
        Task RemoveCachedReportAsync(Guid reportId);

        /// <summary>
        /// Caches report list for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="reports">Report list to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheUserReportsAsync(Guid userId, object reports, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached report list for user
        /// </summary>
        /// <typeparam name="T">Type of report list</typeparam>
        /// <param name="userId">User ID</param>
        /// <returns>Cached report list or null</returns>
        Task<T?> GetCachedUserReportsAsync<T>(Guid userId) where T : class;

        #endregion

        #region User Caching

        /// <summary>
        /// Caches user data
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userData">User data to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheUserAsync(Guid userId, object userData, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached user data
        /// </summary>
        /// <typeparam name="T">Type of user data</typeparam>
        /// <param name="userId">User ID</param>
        /// <returns>Cached user data or null</returns>
        Task<T?> GetCachedUserAsync<T>(Guid userId) where T : class;

        /// <summary>
        /// Removes cached user data
        /// </summary>
        /// <param name="userId">User ID</param>
        Task RemoveCachedUserAsync(Guid userId);

        /// <summary>
        /// Caches user permissions
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="permissions">Permissions to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheUserPermissionsAsync(Guid userId, object permissions, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached user permissions
        /// </summary>
        /// <typeparam name="T">Type of permissions data</typeparam>
        /// <param name="userId">User ID</param>
        /// <returns>Cached permissions or null</returns>
        Task<T?> GetCachedUserPermissionsAsync<T>(Guid userId) where T : class;

        #endregion

        #region Analytics Caching

        /// <summary>
        /// Caches analytics data
        /// </summary>
        /// <param name="analyticsKey">Analytics key</param>
        /// <param name="analyticsData">Analytics data to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheAnalyticsAsync(string analyticsKey, object analyticsData, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached analytics data
        /// </summary>
        /// <typeparam name="T">Type of analytics data</typeparam>
        /// <param name="analyticsKey">Analytics key</param>
        /// <returns>Cached analytics data or null</returns>
        Task<T?> GetCachedAnalyticsAsync<T>(string analyticsKey) where T : class;

        /// <summary>
        /// Caches dashboard data for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dashboardType">Dashboard type</param>
        /// <param name="dashboardData">Dashboard data to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheDashboardAsync(Guid userId, string dashboardType, object dashboardData, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached dashboard data
        /// </summary>
        /// <typeparam name="T">Type of dashboard data</typeparam>
        /// <param name="userId">User ID</param>
        /// <param name="dashboardType">Dashboard type</param>
        /// <returns>Cached dashboard data or null</returns>
        Task<T?> GetCachedDashboardAsync<T>(Guid userId, string dashboardType) where T : class;

        /// <summary>
        /// Caches system performance metrics
        /// </summary>
        /// <param name="metricsData">Metrics data to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheSystemMetricsAsync(object metricsData, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached system performance metrics
        /// </summary>
        /// <typeparam name="T">Type of metrics data</typeparam>
        /// <returns>Cached metrics data or null</returns>
        Task<T?> GetCachedSystemMetricsAsync<T>() where T : class;

        #endregion

        #region Compliance Caching

        /// <summary>
        /// Caches compliance report
        /// </summary>
        /// <param name="reportType">Report type</param>
        /// <param name="complianceData">Compliance data to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheComplianceReportAsync(string reportType, object complianceData, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached compliance report
        /// </summary>
        /// <typeparam name="T">Type of compliance data</typeparam>
        /// <param name="reportType">Report type</param>
        /// <returns>Cached compliance data or null</returns>
        Task<T?> GetCachedComplianceReportAsync<T>(string reportType) where T : class;

        /// <summary>
        /// Caches audit trail data
        /// </summary>
        /// <param name="auditKey">Audit key</param>
        /// <param name="auditData">Audit data to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheAuditTrailAsync(string auditKey, object auditData, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached audit trail data
        /// </summary>
        /// <typeparam name="T">Type of audit data</typeparam>
        /// <param name="auditKey">Audit key</param>
        /// <returns>Cached audit data or null</returns>
        Task<T?> GetCachedAuditTrailAsync<T>(string auditKey) where T : class;

        #endregion

        #region Export Caching

        /// <summary>
        /// Caches export result
        /// </summary>
        /// <param name="exportId">Export ID</param>
        /// <param name="exportData">Export data to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheExportAsync(Guid exportId, object exportData, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached export result
        /// </summary>
        /// <typeparam name="T">Type of export data</typeparam>
        /// <param name="exportId">Export ID</param>
        /// <returns>Cached export data or null</returns>
        Task<T?> GetCachedExportAsync<T>(Guid exportId) where T : class;

        /// <summary>
        /// Removes cached export
        /// </summary>
        /// <param name="exportId">Export ID</param>
        Task RemoveCachedExportAsync(Guid exportId);

        #endregion

        #region Notification Caching

        /// <summary>
        /// Caches user notifications
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notifications">Notifications to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        Task CacheUserNotificationsAsync(Guid userId, object notifications, TimeSpan? expiration = null);

        /// <summary>
        /// Gets cached user notifications
        /// </summary>
        /// <typeparam name="T">Type of notifications data</typeparam>
        /// <param name="userId">User ID</param>
        /// <returns>Cached notifications or null</returns>
        Task<T?> GetCachedUserNotificationsAsync<T>(Guid userId) where T : class;

        /// <summary>
        /// Removes cached user notifications
        /// </summary>
        /// <param name="userId">User ID</param>
        Task RemoveCachedUserNotificationsAsync(Guid userId);

        #endregion

        #region Cache Management

        /// <summary>
        /// Clears all cache entries with a specific pattern
        /// </summary>
        /// <param name="pattern">Cache key pattern</param>
        Task ClearCachePatternAsync(string pattern);

        /// <summary>
        /// Invalidates report-related caches when a report is modified
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="userId">User ID</param>
        Task InvalidateReportCachesAsync(Guid reportId, Guid userId);

        /// <summary>
        /// Invalidates user-related caches when user data is modified
        /// </summary>
        /// <param name="userId">User ID</param>
        Task InvalidateUserCachesAsync(Guid userId);

        /// <summary>
        /// Warms up frequently accessed cache entries
        /// </summary>
        Task WarmupCacheAsync();

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        /// <returns>Cache statistics</returns>
        Task<CacheStatistics> GetCacheStatisticsAsync();

        #endregion
    }
}
