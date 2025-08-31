using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Repositories
{
    /// <summary>
    /// Repository for user notification preferences data access
    /// Provides efficient querying and caching for notification preferences
    /// </summary>
    public class UserNotificationPreferenceRepository : IUserNotificationPreferenceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserNotificationPreferenceRepository> _logger;

        // Cache configuration
        private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(15);
        private const string UserPreferencesCacheKeyPrefix = "user_preferences_";

        public UserNotificationPreferenceRepository(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<UserNotificationPreferenceRepository> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        #region Basic CRUD Operations

        public async Task<List<UserNotificationPreference>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                // Check cache first
                var cached = await GetCachedUserPreferencesAsync(userId);
                if (cached != null)
                {
                    _logger.LogDebug("Retrieved {Count} cached preferences for user {UserId}", cached.Count, userId);
                    return cached;
                }

                // Query database
                var preferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.NotificationType)
                    .ToListAsync();

                // Cache the results
                await CacheUserPreferencesAsync(userId, preferences);

                _logger.LogDebug("Retrieved {Count} preferences from database for user {UserId}", preferences.Count, userId);
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preferences for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserNotificationPreference?> GetByUserAndTypeAsync(Guid userId, string notificationType)
        {
            try
            {
                // Try to get from cached user preferences first
                var cachedPreferences = await GetCachedUserPreferencesAsync(userId);
                if (cachedPreferences != null)
                {
                    var cachedPreference = cachedPreferences.FirstOrDefault(p => p.NotificationType == notificationType);
                    if (cachedPreference != null)
                    {
                        _logger.LogDebug("Retrieved cached preference for user {UserId}, type {Type}", userId, notificationType);
                        return cachedPreference;
                    }
                }

                // Query database
                var preference = await _context.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);

                _logger.LogDebug("Retrieved preference from database for user {UserId}, type {Type}", userId, notificationType);
                return preference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preference for user {UserId}, type {Type}", userId, notificationType);
                throw;
            }
        }

        public async Task<UserNotificationPreference?> GetByIdAsync(Guid id)
        {
            try
            {
                var preference = await _context.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.Id == id);

                _logger.LogDebug("Retrieved preference by ID {Id}: {Found}", id, preference != null);
                return preference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preference by ID {Id}", id);
                throw;
            }
        }

        public async Task<UserNotificationPreference> CreateAsync(UserNotificationPreference preference)
        {
            try
            {
                preference.Id = Guid.NewGuid();
                preference.CreatedAt = DateTime.UtcNow;
                preference.UpdatedAt = DateTime.UtcNow;

                _context.UserNotificationPreferences.Add(preference);
                await _context.SaveChangesAsync();

                // Invalidate cache for the user
                await InvalidateUserPreferencesCacheAsync(preference.UserId);

                _logger.LogDebug("Created preference {Id} for user {UserId}, type {Type}", 
                    preference.Id, preference.UserId, preference.NotificationType);

                return preference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating preference for user {UserId}, type {Type}", 
                    preference.UserId, preference.NotificationType);
                throw;
            }
        }

        public async Task<UserNotificationPreference> UpdateAsync(UserNotificationPreference preference)
        {
            try
            {
                preference.UpdatedAt = DateTime.UtcNow;
                _context.UserNotificationPreferences.Update(preference);
                await _context.SaveChangesAsync();

                // Invalidate cache for the user
                await InvalidateUserPreferencesCacheAsync(preference.UserId);

                _logger.LogDebug("Updated preference {Id} for user {UserId}, type {Type}", 
                    preference.Id, preference.UserId, preference.NotificationType);

                return preference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preference {Id}", preference.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(UserNotificationPreference preference)
        {
            try
            {
                _context.UserNotificationPreferences.Remove(preference);
                var result = await _context.SaveChangesAsync();

                // Invalidate cache for the user
                await InvalidateUserPreferencesCacheAsync(preference.UserId);

                _logger.LogDebug("Deleted preference {Id} for user {UserId}, type {Type}", 
                    preference.Id, preference.UserId, preference.NotificationType);

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting preference {Id}", preference.Id);
                throw;
            }
        }

        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            try
            {
                var preference = await _context.UserNotificationPreferences.FindAsync(id);
                if (preference == null)
                {
                    return false;
                }

                return await DeleteAsync(preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting preference by ID {Id}", id);
                throw;
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<int> CreateBulkAsync(List<UserNotificationPreference> preferences)
        {
            try
            {
                var now = DateTime.UtcNow;
                foreach (var preference in preferences)
                {
                    preference.Id = Guid.NewGuid();
                    preference.CreatedAt = now;
                    preference.UpdatedAt = now;
                }

                _context.UserNotificationPreferences.AddRange(preferences);
                var result = await _context.SaveChangesAsync();

                // Invalidate cache for affected users
                var userIds = preferences.Select(p => p.UserId).Distinct();
                foreach (var userId in userIds)
                {
                    await InvalidateUserPreferencesCacheAsync(userId);
                }

                _logger.LogDebug("Created {Count} preferences in bulk", preferences.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {Count} preferences in bulk", preferences.Count);
                throw;
            }
        }

        public async Task<int> UpdateBulkAsync(List<UserNotificationPreference> preferences)
        {
            try
            {
                var now = DateTime.UtcNow;
                foreach (var preference in preferences)
                {
                    preference.UpdatedAt = now;
                }

                _context.UserNotificationPreferences.UpdateRange(preferences);
                var result = await _context.SaveChangesAsync();

                // Invalidate cache for affected users
                var userIds = preferences.Select(p => p.UserId).Distinct();
                foreach (var userId in userIds)
                {
                    await InvalidateUserPreferencesCacheAsync(userId);
                }

                _logger.LogDebug("Updated {Count} preferences in bulk", preferences.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {Count} preferences in bulk", preferences.Count);
                throw;
            }
        }

        public async Task<int> DeleteByUserIdAsync(Guid userId)
        {
            try
            {
                var preferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                if (!preferences.Any())
                {
                    return 0;
                }

                _context.UserNotificationPreferences.RemoveRange(preferences);
                var result = await _context.SaveChangesAsync();

                // Invalidate cache for the user
                await InvalidateUserPreferencesCacheAsync(userId);

                _logger.LogDebug("Deleted {Count} preferences for user {UserId}", preferences.Count, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting preferences for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> DeleteByIdsAsync(List<Guid> ids)
        {
            try
            {
                var preferences = await _context.UserNotificationPreferences
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                if (!preferences.Any())
                {
                    return 0;
                }

                _context.UserNotificationPreferences.RemoveRange(preferences);
                var result = await _context.SaveChangesAsync();

                // Invalidate cache for affected users
                var userIds = preferences.Select(p => p.UserId).Distinct();
                foreach (var userId in userIds)
                {
                    await InvalidateUserPreferencesCacheAsync(userId);
                }

                _logger.LogDebug("Deleted {Count} preferences by IDs", preferences.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting preferences by IDs");
                throw;
            }
        }

        #endregion

        #region Query Operations

        public async Task<List<Guid>> GetUsersWithChannelEnabledAsync(string notificationType, string channel)
        {
            try
            {
                var query = _context.UserNotificationPreferences
                    .Where(p => p.NotificationType == notificationType);

                query = channel.ToLower() switch
                {
                    "email" => query.Where(p => p.EmailEnabled),
                    "realtime" => query.Where(p => p.RealTimeEnabled),
                    "push" => query.Where(p => p.PushEnabled),
                    "sms" => query.Where(p => p.SmsEnabled),
                    _ => throw new ArgumentException($"Invalid channel: {channel}")
                };

                var userIds = await query.Select(p => p.UserId).Distinct().ToListAsync();

                _logger.LogDebug("Found {Count} users with {Channel} enabled for {Type}", 
                    userIds.Count, channel, notificationType);

                return userIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with {Channel} enabled for {Type}", channel, notificationType);
                throw;
            }
        }

        public async Task<List<Guid>> GetUsersWithPriorityThresholdAsync(string notificationType, string priority)
        {
            try
            {
                var priorityLevels = new Dictionary<string, int>
                {
                    ["Low"] = 1,
                    ["Medium"] = 2,
                    ["High"] = 3,
                    ["Critical"] = 4
                };

                var messagePriorityLevel = priorityLevels.GetValueOrDefault(priority, 2);

                var userIds = await _context.UserNotificationPreferences
                    .Where(p => p.NotificationType == notificationType)
                    .Where(p => priorityLevels.GetValueOrDefault(p.MinimumPriority, 2) <= messagePriorityLevel)
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToListAsync();

                _logger.LogDebug("Found {Count} users meeting priority threshold {Priority} for {Type}", 
                    userIds.Count, priority, notificationType);

                return userIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with priority threshold {Priority} for {Type}", 
                    priority, notificationType);
                throw;
            }
        }

        public async Task<List<Guid>> GetUsersInQuietHoursAsync(DateTime? currentTime = null)
        {
            try
            {
                var currentUtc = currentTime ?? DateTime.UtcNow;
                var usersInQuietHours = new List<Guid>();

                // Get all users with quiet hours configured
                var usersWithQuietHours = await _context.UserNotificationPreferences
                    .Where(p => p.QuietHoursStart != null && p.QuietHoursEnd != null)
                    .Select(p => new { p.UserId, p.QuietHoursStart, p.QuietHoursEnd, p.TimeZone })
                    .Distinct()
                    .ToListAsync();

                foreach (var user in usersWithQuietHours)
                {
                    try
                    {
                        // Convert current time to user's timezone
                        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone ?? "UTC");
                        var userTime = TimeZoneInfo.ConvertTimeFromUtc(currentUtc, userTimeZone);

                        // Parse quiet hours
                        if (TimeOnly.TryParse(user.QuietHoursStart, out var startTime) &&
                            TimeOnly.TryParse(user.QuietHoursEnd, out var endTime))
                        {
                            var currentTimeOnly = TimeOnly.FromDateTime(userTime);

                            // Check if in quiet hours
                            bool inQuietHours;
                            if (startTime <= endTime)
                            {
                                inQuietHours = currentTimeOnly >= startTime && currentTimeOnly <= endTime;
                            }
                            else
                            {
                                inQuietHours = currentTimeOnly >= startTime || currentTimeOnly <= endTime;
                            }

                            if (inQuietHours)
                            {
                                usersInQuietHours.Add(user.UserId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error checking quiet hours for user {UserId}", user.UserId);
                        // Continue with other users
                    }
                }

                _logger.LogDebug("Found {Count} users currently in quiet hours", usersInQuietHours.Count);
                return usersInQuietHours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users in quiet hours");
                throw;
            }
        }

        public async Task<List<Guid>> GetUsersByScheduleAsync(string schedule)
        {
            try
            {
                var userIds = await _context.UserNotificationPreferences
                    .Where(p => p.Schedule == schedule)
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToListAsync();

                _logger.LogDebug("Found {Count} users with schedule {Schedule}", userIds.Count, schedule);
                return userIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by schedule {Schedule}", schedule);
                throw;
            }
        }

        public async Task<bool> UserHasPreferencesAsync(Guid userId)
        {
            try
            {
                var hasPreferences = await _context.UserNotificationPreferences
                    .AnyAsync(p => p.UserId == userId);

                _logger.LogDebug("User {UserId} has preferences: {HasPreferences}", userId, hasPreferences);
                return hasPreferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has preferences", userId);
                throw;
            }
        }

        public async Task<List<string>> GetConfiguredNotificationTypesAsync(Guid userId)
        {
            try
            {
                var notificationTypes = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .Select(p => p.NotificationType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                _logger.LogDebug("Found {Count} configured notification types for user {UserId}", 
                    notificationTypes.Count, userId);

                return notificationTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configured notification types for user {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Statistics & Analytics

        public async Task<int> GetTotalPreferencesCountAsync()
        {
            try
            {
                var count = await _context.UserNotificationPreferences.CountAsync();
                _logger.LogDebug("Total preferences count: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total preferences count");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetPreferencesCountByTypeAsync()
        {
            try
            {
                var counts = await _context.UserNotificationPreferences
                    .GroupBy(p => p.NotificationType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Type, x => x.Count);

                _logger.LogDebug("Retrieved preferences count by type for {TypeCount} types", counts.Count);
                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences count by type");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetChannelEnabledCountsAsync()
        {
            try
            {
                var emailCount = await _context.UserNotificationPreferences.CountAsync(p => p.EmailEnabled);
                var realTimeCount = await _context.UserNotificationPreferences.CountAsync(p => p.RealTimeEnabled);
                var pushCount = await _context.UserNotificationPreferences.CountAsync(p => p.PushEnabled);
                var smsCount = await _context.UserNotificationPreferences.CountAsync(p => p.SmsEnabled);

                var counts = new Dictionary<string, int>
                {
                    ["Email"] = emailCount,
                    ["RealTime"] = realTimeCount,
                    ["Push"] = pushCount,
                    ["Sms"] = smsCount
                };

                _logger.LogDebug("Retrieved channel enabled counts");
                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting channel enabled counts");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetPriorityDistributionAsync()
        {
            try
            {
                var distribution = await _context.UserNotificationPreferences
                    .GroupBy(p => p.MinimumPriority)
                    .Select(g => new { Priority = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Priority, x => x.Count);

                _logger.LogDebug("Retrieved priority distribution for {Count} priorities", distribution.Count);
                return distribution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting priority distribution");
                throw;
            }
        }

        public async Task<int> GetUsersWithQuietHoursCountAsync()
        {
            try
            {
                var count = await _context.UserNotificationPreferences
                    .Where(p => p.QuietHoursStart != null && p.QuietHoursEnd != null)
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();

                _logger.LogDebug("Users with quiet hours configured: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with quiet hours count");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetTimezoneDistributionAsync()
        {
            try
            {
                var distribution = await _context.UserNotificationPreferences
                    .GroupBy(p => p.TimeZone)
                    .Select(g => new { TimeZone = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.TimeZone ?? "UTC", x => x.Count);

                _logger.LogDebug("Retrieved timezone distribution for {Count} timezones", distribution.Count);
                return distribution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timezone distribution");
                throw;
            }
        }

        #endregion

        #region Caching Operations

        public async Task<List<UserNotificationPreference>?> GetCachedUserPreferencesAsync(Guid userId)
        {
            try
            {
                var cacheKey = $"{UserPreferencesCacheKeyPrefix}{userId}";
                var cached = _cache.Get<List<UserNotificationPreference>>(cacheKey);
                
                _logger.LogTrace("Cache {Status} for user preferences {UserId}", 
                    cached != null ? "HIT" : "MISS", userId);
                
                return await Task.FromResult(cached);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving cached preferences for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> CacheUserPreferencesAsync(Guid userId, List<UserNotificationPreference> preferences, TimeSpan? expiration = null)
        {
            try
            {
                var cacheKey = $"{UserPreferencesCacheKeyPrefix}{userId}";
                var cacheExpiration = expiration ?? DefaultCacheExpiration;

                _cache.Set(cacheKey, preferences, cacheExpiration);
                
                _logger.LogTrace("Cached {Count} preferences for user {UserId} with expiration {Expiration}", 
                    preferences.Count, userId, cacheExpiration);
                
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error caching preferences for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> InvalidateUserPreferencesCacheAsync(Guid userId)
        {
            try
            {
                var cacheKey = $"{UserPreferencesCacheKeyPrefix}{userId}";
                _cache.Remove(cacheKey);
                
                _logger.LogTrace("Invalidated cache for user preferences {UserId}", userId);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidating cache for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ClearAllPreferenceCachesAsync()
        {
            try
            {
                // Note: IMemoryCache doesn't have a built-in method to clear all entries
                // This would require a custom implementation or using a different caching solution
                // For now, we'll log a warning and return true
                
                _logger.LogWarning("ClearAllPreferenceCaches called but not implemented for IMemoryCache");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all preference caches");
                return false;
            }
        }

        #endregion
    }
}
