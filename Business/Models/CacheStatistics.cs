namespace ProjectControlsReportingTool.API.Business.Models
{
    /// <summary>
    /// Cache statistics for monitoring performance
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// Total number of cache hits
        /// </summary>
        public long TotalHits { get; set; }

        /// <summary>
        /// Total number of cache misses
        /// </summary>
        public long TotalMisses { get; set; }

        /// <summary>
        /// Cache hit ratio (hits / total requests)
        /// </summary>
        public double HitRatio => TotalRequests > 0 ? (double)TotalHits / TotalRequests : 0;

        /// <summary>
        /// Total number of cache requests
        /// </summary>
        public long TotalRequests => TotalHits + TotalMisses;

        /// <summary>
        /// Number of currently cached entries
        /// </summary>
        public long CurrentEntries { get; set; }

        /// <summary>
        /// Total memory usage in bytes
        /// </summary>
        public long MemoryUsage { get; set; }

        /// <summary>
        /// Number of expired entries
        /// </summary>
        public long ExpiredEntries { get; set; }

        /// <summary>
        /// Number of evicted entries
        /// </summary>
        public long EvictedEntries { get; set; }

        /// <summary>
        /// Cache performance metrics by category
        /// </summary>
        public Dictionary<string, CacheCategoryStats> CategoryStats { get; set; } = new();

        /// <summary>
        /// Last reset timestamp
        /// </summary>
        public DateTime LastReset { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Uptime since last reset
        /// </summary>
        public TimeSpan Uptime => DateTime.UtcNow - LastReset;
    }

    /// <summary>
    /// Cache statistics for a specific category
    /// </summary>
    public class CacheCategoryStats
    {
        /// <summary>
        /// Category name (e.g., "reports", "users", "analytics")
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Number of hits for this category
        /// </summary>
        public long Hits { get; set; }

        /// <summary>
        /// Number of misses for this category
        /// </summary>
        public long Misses { get; set; }

        /// <summary>
        /// Hit ratio for this category
        /// </summary>
        public double HitRatio => TotalRequests > 0 ? (double)Hits / TotalRequests : 0;

        /// <summary>
        /// Total requests for this category
        /// </summary>
        public long TotalRequests => Hits + Misses;

        /// <summary>
        /// Number of entries in this category
        /// </summary>
        public long EntryCount { get; set; }

        /// <summary>
        /// Memory usage for this category in bytes
        /// </summary>
        public long MemoryUsage { get; set; }

        /// <summary>
        /// Average expiration time for this category
        /// </summary>
        public TimeSpan AverageExpiration { get; set; }

        /// <summary>
        /// Last access time for any entry in this category
        /// </summary>
        public DateTime LastAccessed { get; set; }
    }
}
