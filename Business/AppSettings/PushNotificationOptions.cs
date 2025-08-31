namespace ProjectControlsReportingTool.API.Business.AppSettings
{
    /// <summary>
    /// Configuration settings for push notifications
    /// </summary>
    public class PushNotificationOptions
    {
        /// <summary>
        /// VAPID subject (email or URL)
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// VAPID public key for Web Push API
        /// </summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// VAPID private key for Web Push API
        /// </summary>
        public string PrivateKey { get; set; } = string.Empty;

        /// <summary>
        /// Default icon URL for notifications
        /// </summary>
        public string DefaultIcon { get; set; } = "/assets/logo.png";

        /// <summary>
        /// Default badge URL for notifications
        /// </summary>
        public string DefaultBadge { get; set; } = "/assets/badge.png";

        /// <summary>
        /// Maximum number of retries for failed push notifications
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retries in seconds
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 30;

        /// <summary>
        /// Whether to automatically cleanup expired subscriptions
        /// </summary>
        public bool AutoCleanupExpiredSubscriptions { get; set; } = true;

        /// <summary>
        /// How often to run cleanup in hours
        /// </summary>
        public int CleanupIntervalHours { get; set; } = 24;

        /// <summary>
        /// Maximum number of subscriptions per user
        /// </summary>
        public int MaxSubscriptionsPerUser { get; set; } = 10;

        /// <summary>
        /// Default Time-To-Live for push notifications in seconds (24 hours)
        /// </summary>
        public int DefaultTtlSeconds { get; set; } = 86400;
    }
}
