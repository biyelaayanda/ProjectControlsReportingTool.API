namespace ProjectControlsReportingTool.API.Business.Models
{
    /// <summary>
    /// Configuration options for SMS service
    /// </summary>
    public class SmsOptions
    {
        public const string SectionName = "SmsSettings";

        /// <summary>
        /// Whether SMS functionality is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Default SMS provider to use
        /// </summary>
        public string DefaultProvider { get; set; } = "Twilio";

        /// <summary>
        /// Maximum daily SMS messages across the system
        /// </summary>
        public int MaxDailyMessages { get; set; } = 10000;

        /// <summary>
        /// Maximum SMS messages per user per day
        /// </summary>
        public int MaxMessagesPerUser { get; set; } = 100;

        /// <summary>
        /// Daily SMS budget limit
        /// </summary>
        public decimal DailyBudget { get; set; } = 1000m;

        /// <summary>
        /// Whether bulk SMS operations require approval
        /// </summary>
        public bool RequireApprovalForBulk { get; set; } = true;

        /// <summary>
        /// Threshold for considering an operation as bulk
        /// </summary>
        public int BulkMessageThreshold { get; set; } = 50;

        /// <summary>
        /// Maximum recipients allowed in a single bulk operation
        /// </summary>
        public int MaxBulkRecipients { get; set; } = 1000;

        /// <summary>
        /// Maximum retry attempts for failed messages
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Phone numbers that are restricted from receiving SMS
        /// </summary>
        public string[] RestrictedNumbers { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Allowed country codes for SMS delivery
        /// </summary>
        public string[] AllowedCountryCodes { get; set; } = { "+27", "+1", "+44", "+61" };

        /// <summary>
        /// Default country code for local numbers
        /// </summary>
        public string DefaultCountryCode { get; set; } = "+27";

        /// <summary>
        /// Whether to log all SMS messages for audit purposes
        /// </summary>
        public bool LogAllMessages { get; set; } = true;

        /// <summary>
        /// Number of days to retain SMS message history
        /// </summary>
        public int RetentionDays { get; set; } = 90;

        /// <summary>
        /// Twilio configuration
        /// </summary>
        public TwilioConfiguration Twilio { get; set; } = new();

        /// <summary>
        /// BulkSMS configuration
        /// </summary>
        public BulkSmsConfiguration BulkSms { get; set; } = new();

        /// <summary>
        /// Webhook configuration for delivery receipts
        /// </summary>
        public WebhookConfiguration Webhooks { get; set; } = new();

        /// <summary>
        /// Rate limiting configuration
        /// </summary>
        public RateLimitConfiguration RateLimit { get; set; } = new();

        /// <summary>
        /// Emergency alert configuration
        /// </summary>
        public EmergencyConfiguration Emergency { get; set; } = new();
    }

    /// <summary>
    /// Twilio SMS provider configuration
    /// </summary>
    public class TwilioConfiguration
    {
        /// <summary>
        /// Twilio Account SID
        /// </summary>
        public string AccountSid { get; set; } = string.Empty;

        /// <summary>
        /// Twilio Auth Token
        /// </summary>
        public string AuthToken { get; set; } = string.Empty;

        /// <summary>
        /// Twilio phone number for sending SMS
        /// </summary>
        public string FromPhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Twilio API endpoint
        /// </summary>
        public string ApiEndpoint { get; set; } = "https://api.twilio.com";

        /// <summary>
        /// Webhook URL for delivery receipts
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Cost per SMS message in USD
        /// </summary>
        public decimal CostPerMessage { get; set; } = 0.05m;

        /// <summary>
        /// Whether this provider is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Priority of this provider (lower = higher priority)
        /// </summary>
        public int Priority { get; set; } = 1;
    }

    /// <summary>
    /// BulkSMS provider configuration
    /// </summary>
    public class BulkSmsConfiguration
    {
        /// <summary>
        /// BulkSMS username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// BulkSMS password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// BulkSMS API endpoint
        /// </summary>
        public string ApiEndpoint { get; set; } = "https://api.bulksms.com/v1";

        /// <summary>
        /// Cost per SMS message in ZAR
        /// </summary>
        public decimal CostPerMessage { get; set; } = 0.29m;

        /// <summary>
        /// Whether this provider is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Priority of this provider (lower = higher priority)
        /// </summary>
        public int Priority { get; set; } = 2;

        /// <summary>
        /// Supported countries (ISO country codes)
        /// </summary>
        public string[] SupportedCountries { get; set; } = { "ZA" };
    }

    /// <summary>
    /// Webhook configuration for SMS delivery receipts
    /// </summary>
    public class WebhookConfiguration
    {
        /// <summary>
        /// Base URL for SMS webhooks
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Webhook secret for signature verification
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// Timeout for webhook processing in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Whether to verify webhook signatures
        /// </summary>
        public bool VerifySignatures { get; set; } = true;

        /// <summary>
        /// Maximum number of webhook retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
    }

    /// <summary>
    /// Rate limiting configuration for SMS operations
    /// </summary>
    public class RateLimitConfiguration
    {
        /// <summary>
        /// Maximum SMS messages per minute per user
        /// </summary>
        public int MessagesPerMinutePerUser { get; set; } = 10;

        /// <summary>
        /// Maximum SMS messages per hour per user
        /// </summary>
        public int MessagesPerHourPerUser { get; set; } = 100;

        /// <summary>
        /// Maximum SMS messages per minute system-wide
        /// </summary>
        public int MessagesPerMinuteSystem { get; set; } = 1000;

        /// <summary>
        /// Rate limit window duration in minutes
        /// </summary>
        public int WindowMinutes { get; set; } = 1;

        /// <summary>
        /// Whether to enable rate limiting
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Rate limit storage type (Memory, Redis, Database)
        /// </summary>
        public string StorageType { get; set; } = "Memory";
    }

    /// <summary>
    /// Emergency alert configuration
    /// </summary>
    public class EmergencyConfiguration
    {
        /// <summary>
        /// Whether emergency alerts bypass rate limits
        /// </summary>
        public bool BypassRateLimits { get; set; } = true;

        /// <summary>
        /// Whether emergency alerts bypass user limits
        /// </summary>
        public bool BypassUserLimits { get; set; } = true;

        /// <summary>
        /// Whether emergency alerts bypass budget limits
        /// </summary>
        public bool BypassBudgetLimits { get; set; } = true;

        /// <summary>
        /// Phone numbers for emergency escalation
        /// </summary>
        public string[] EscalationNumbers { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Maximum number of emergency alerts per day
        /// </summary>
        public int MaxAlertsPerDay { get; set; } = 10;

        /// <summary>
        /// Minimum time between emergency alerts in minutes
        /// </summary>
        public int MinIntervalMinutes { get; set; } = 15;
    }

    /// <summary>
    /// SMS template configuration
    /// </summary>
    public class SmsTemplateConfiguration
    {
        /// <summary>
        /// Maximum template length
        /// </summary>
        public int MaxTemplateLength { get; set; } = 1600;

        /// <summary>
        /// Maximum number of variables per template
        /// </summary>
        public int MaxVariables { get; set; } = 20;

        /// <summary>
        /// Default template category
        /// </summary>
        public string DefaultCategory { get; set; } = "General";

        /// <summary>
        /// Whether to auto-detect template variables
        /// </summary>
        public bool AutoDetectVariables { get; set; } = true;

        /// <summary>
        /// Template variable pattern (regex)
        /// </summary>
        public string VariablePattern { get; set; } = @"\{([^}]+)\}";
    }

    /// <summary>
    /// SMS monitoring and alerting configuration
    /// </summary>
    public class MonitoringConfiguration
    {
        /// <summary>
        /// Whether to enable SMS monitoring
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Delivery rate threshold for alerts (percentage)
        /// </summary>
        public double DeliveryRateThreshold { get; set; } = 95.0;

        /// <summary>
        /// Error rate threshold for alerts (percentage)
        /// </summary>
        public double ErrorRateThreshold { get; set; } = 5.0;

        /// <summary>
        /// Cost threshold for daily budget alerts
        /// </summary>
        public decimal CostThreshold { get; set; } = 800m;

        /// <summary>
        /// Email addresses for monitoring alerts
        /// </summary>
        public string[] AlertEmails { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Phone numbers for SMS alerts about SMS system issues
        /// </summary>
        public string[] AlertPhoneNumbers { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Monitoring check interval in minutes
        /// </summary>
        public int CheckIntervalMinutes { get; set; } = 5;
    }
}
