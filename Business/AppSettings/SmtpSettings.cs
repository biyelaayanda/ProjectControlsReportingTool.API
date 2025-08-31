namespace ProjectControlsReportingTool.API.Business.AppSettings
{
    /// <summary>
    /// SMTP configuration settings for email notifications
    /// </summary>
    public class SmtpSettings
    {
        public const string SectionName = "SmtpSettings";

        /// <summary>
        /// Whether email sending is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// SMTP server host
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// SMTP server port
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// Whether to use SSL/TLS
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// SMTP username for authentication
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// SMTP password for authentication
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// From email address
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>
        /// From display name
        /// </summary>
        public string FromName { get; set; } = "Rand Water Project Controls";

        /// <summary>
        /// Timeout for SMTP operations (in milliseconds)
        /// </summary>
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// Whether to save emails to database for audit trail
        /// </summary>
        public bool SaveToDatabase { get; set; } = true;

        /// <summary>
        /// Maximum number of retries for failed emails
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retries (in milliseconds)
        /// </summary>
        public int RetryDelay { get; set; } = 5000;
    }
}
