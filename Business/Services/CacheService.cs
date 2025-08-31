using Microsoft.Extensions.Caching.Distributed;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using System.Text.Json;
using System.Collections.Concurrent;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Redis-based distributed caching service for Phase 9.3 Performance Optimization
    /// Provides intelligent caching with type-specific strategies and cache statistics
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheService> _logger;
        private readonly ConcurrentDictionary<string, int> _hitCount = new();
        private readonly ConcurrentDictionary<string, int> _missCount = new();
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache key prefixes for different data types
        private const string REPORTS_PREFIX = "reports:";
        private const string USERS_PREFIX = "users:";
        private const string ANALYTICS_PREFIX = "analytics:";
        private const string COMPLIANCE_PREFIX = "compliance:";
        private const string DASHBOARD_PREFIX = "dashboard:";
        private const string EXPORT_PREFIX = "export:";
        private const string NOTIFICATIONS_PREFIX = "notifications:";
        private const string SYSTEM_METRICS_PREFIX = "system_metrics:";
        private const string AUDIT_PREFIX = "audit:";
        private const string PERMISSIONS_PREFIX = "permissions:";

        // Default cache expiration times
        private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _longTermExpiry = TimeSpan.FromHours(24);
        private readonly TimeSpan _shortTermExpiry = TimeSpan.FromMinutes(5);

        public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        #region Generic Cache Operations

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedData = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedData))
                {
                    RecordCacheMiss(key);
                    return null;
                }

                RecordCacheHit(key);
                return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiry
                };

                await _distributedCache.SetStringAsync(key, serializedValue, options);
                _logger.LogDebug("Cache set for key: {Key} with expiry: {Expiry}", key, expiration ?? _defaultExpiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            var cached = await GetAsync<T>(key);
            if (cached != null)
            {
                return cached;
            }

            var value = await factory();
            await SetAsync(key, value, expiration);
            return value;
        }

        #endregion

        #region Report Caching

        public async Task CacheReportAsync(Guid reportId, object reportData, TimeSpan? expiration = null)
        {
            var key = $"{REPORTS_PREFIX}{reportId}";
            await SetAsync(key, reportData as dynamic, expiration ?? _longTermExpiry);
        }

        public async Task<T?> GetCachedReportAsync<T>(Guid reportId) where T : class
        {
            var key = $"{REPORTS_PREFIX}{reportId}";
            return await GetAsync<T>(key);
        }

        public async Task RemoveCachedReportAsync(Guid reportId)
        {
            var key = $"{REPORTS_PREFIX}{reportId}";
            await RemoveAsync(key);
        }

        public async Task CacheUserReportsAsync(Guid userId, object reports, TimeSpan? expiration = null)
        {
            var key = $"{REPORTS_PREFIX}user:{userId}";
            await SetAsync(key, reports as dynamic, expiration ?? _defaultExpiry);
        }

        public async Task<T?> GetCachedUserReportsAsync<T>(Guid userId) where T : class
        {
            var key = $"{REPORTS_PREFIX}user:{userId}";
            return await GetAsync<T>(key);
        }

        #endregion

        #region User Caching

        public async Task CacheUserAsync(Guid userId, object userData, TimeSpan? expiration = null)
        {
            var key = $"{USERS_PREFIX}{userId}";
            await SetAsync(key, userData as dynamic, expiration ?? _defaultExpiry);
        }

        public async Task<T?> GetCachedUserAsync<T>(Guid userId) where T : class
        {
            var key = $"{USERS_PREFIX}{userId}";
            return await GetAsync<T>(key);
        }

        public async Task RemoveCachedUserAsync(Guid userId)
        {
            var key = $"{USERS_PREFIX}{userId}";
            await RemoveAsync(key);
        }

        public async Task CacheUserPermissionsAsync(Guid userId, object permissions, TimeSpan? expiration = null)
        {
            var key = $"{PERMISSIONS_PREFIX}{userId}";
            await SetAsync(key, permissions as dynamic, expiration ?? _defaultExpiry);
        }

        public async Task<T?> GetCachedUserPermissionsAsync<T>(Guid userId) where T : class
        {
            var key = $"{PERMISSIONS_PREFIX}{userId}";
            return await GetAsync<T>(key);
        }

        #endregion

        #region Analytics Caching

        public async Task CacheAnalyticsAsync(string analyticsKey, object analyticsData, TimeSpan? expiration = null)
        {
            var key = $"{ANALYTICS_PREFIX}{analyticsKey}";
            await SetAsync(key, analyticsData as dynamic, expiration ?? _shortTermExpiry);
        }

        public async Task<T?> GetCachedAnalyticsAsync<T>(string analyticsKey) where T : class
        {
            var key = $"{ANALYTICS_PREFIX}{analyticsKey}";
            return await GetAsync<T>(key);
        }

        public async Task CacheDashboardAsync(Guid userId, string dashboardType, object dashboardData, TimeSpan? expiration = null)
        {
            var key = $"{DASHBOARD_PREFIX}{userId}:{dashboardType}";
            await SetAsync(key, dashboardData as dynamic, expiration ?? _shortTermExpiry);
        }

        public async Task<T?> GetCachedDashboardAsync<T>(Guid userId, string dashboardType) where T : class
        {
            var key = $"{DASHBOARD_PREFIX}{userId}:{dashboardType}";
            return await GetAsync<T>(key);
        }

        public async Task CacheSystemMetricsAsync(object metricsData, TimeSpan? expiration = null)
        {
            var key = $"{SYSTEM_METRICS_PREFIX}current";
            await SetAsync(key, metricsData as dynamic, expiration ?? _shortTermExpiry);
        }

        public async Task<T?> GetCachedSystemMetricsAsync<T>() where T : class
        {
            var key = $"{SYSTEM_METRICS_PREFIX}current";
            return await GetAsync<T>(key);
        }

        #endregion

        #region Compliance Caching

        public async Task CacheComplianceReportAsync(string reportType, object complianceData, TimeSpan? expiration = null)
        {
            var key = $"{COMPLIANCE_PREFIX}{reportType}";
            await SetAsync(key, complianceData as dynamic, expiration ?? _longTermExpiry);
        }

        public async Task<T?> GetCachedComplianceReportAsync<T>(string reportType) where T : class
        {
            var key = $"{COMPLIANCE_PREFIX}{reportType}";
            return await GetAsync<T>(key);
        }

        public async Task CacheAuditTrailAsync(string auditKey, object auditData, TimeSpan? expiration = null)
        {
            var key = $"{AUDIT_PREFIX}{auditKey}";
            await SetAsync(key, auditData as dynamic, expiration ?? _shortTermExpiry);
        }

        public async Task<T?> GetCachedAuditTrailAsync<T>(string auditKey) where T : class
        {
            var key = $"{AUDIT_PREFIX}{auditKey}";
            return await GetAsync<T>(key);
        }

        #endregion

        #region Export Caching

        public async Task CacheExportAsync(Guid exportId, object exportData, TimeSpan? expiration = null)
        {
            var key = $"{EXPORT_PREFIX}{exportId}";
            await SetAsync(key, exportData as dynamic, expiration ?? _longTermExpiry);
        }

        public async Task<T?> GetCachedExportAsync<T>(Guid exportId) where T : class
        {
            var key = $"{EXPORT_PREFIX}{exportId}";
            return await GetAsync<T>(key);
        }

        public async Task RemoveCachedExportAsync(Guid exportId)
        {
            var key = $"{EXPORT_PREFIX}{exportId}";
            await RemoveAsync(key);
        }

        #endregion

        #region Notification Caching

        public async Task CacheUserNotificationsAsync(Guid userId, object notifications, TimeSpan? expiration = null)
        {
            var key = $"{NOTIFICATIONS_PREFIX}{userId}";
            await SetAsync(key, notifications as dynamic, expiration ?? _shortTermExpiry);
        }

        public async Task<T?> GetCachedUserNotificationsAsync<T>(Guid userId) where T : class
        {
            var key = $"{NOTIFICATIONS_PREFIX}{userId}";
            return await GetAsync<T>(key);
        }

        public async Task RemoveCachedUserNotificationsAsync(Guid userId)
        {
            var key = $"{NOTIFICATIONS_PREFIX}{userId}";
            await RemoveAsync(key);
        }

        #endregion

        #region Cache Management

        public async Task ClearCachePatternAsync(string pattern)
        {
            try
            {
                _logger.LogWarning("Pattern clearance requested: {Pattern}. Consider implementing Redis SCAN for efficiency.", pattern);
                // In a real implementation, you'd use Redis SCAN operations
                // For now, just log the operation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache pattern: {Pattern}", pattern);
            }
        }

        public async Task InvalidateReportCachesAsync(Guid reportId, Guid userId)
        {
            await RemoveCachedReportAsync(reportId);
            var userReportsKey = $"{REPORTS_PREFIX}user:{userId}";
            await RemoveAsync(userReportsKey);
        }

        public async Task InvalidateUserCachesAsync(Guid userId)
        {
            await RemoveCachedUserAsync(userId);
            var permissionsKey = $"{PERMISSIONS_PREFIX}{userId}";
            await RemoveAsync(permissionsKey);
            var notificationsKey = $"{NOTIFICATIONS_PREFIX}{userId}";
            await RemoveAsync(notificationsKey);
        }

        public async Task WarmupCacheAsync()
        {
            try
            {
                _logger.LogInformation("Starting cache warmup process");
                // This could be expanded to preload frequently accessed data
                _logger.LogInformation("Cache warmup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache warmup");
            }
        }

        public async Task<CacheStatistics> GetCacheStatisticsAsync()
        {
            try
            {
                var totalHits = _hitCount.Values.Sum();
                var totalMisses = _missCount.Values.Sum();
                
                return new CacheStatistics
                {
                    HitCount = totalHits,
                    MissCount = totalMisses,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache statistics");
                return new CacheStatistics();
            }
        }

        #endregion

        #region Private Helper Methods

        private void RecordCacheHit(string key)
        {
            var prefix = GetKeyPrefix(key);
            _hitCount.AddOrUpdate(prefix, 1, (k, v) => v + 1);
        }

        private void RecordCacheMiss(string key)
        {
            var prefix = GetKeyPrefix(key);
            _missCount.AddOrUpdate(prefix, 1, (k, v) => v + 1);
        }

        private static string GetKeyPrefix(string key)
        {
            var colonIndex = key.IndexOf(':');
            return colonIndex > 0 ? key.Substring(0, colonIndex + 1) : "unknown:";
        }

        #endregion
    }
}