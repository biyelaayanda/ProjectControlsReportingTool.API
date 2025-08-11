using Microsoft.Extensions.Configuration;

namespace ProjectControlsReportingTool.API.Business.AppSettings
{
    public static class AppSettings
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string DbConnectionString => _configuration?.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Database connection string not configured");

        public static string JwtSecret => _configuration?["JwtSettings:Secret"] 
            ?? throw new InvalidOperationException("JWT secret not configured");

        public static string JwtIssuer => _configuration?["JwtSettings:Issuer"] 
            ?? throw new InvalidOperationException("JWT issuer not configured");

        public static string JwtAudience => _configuration?["JwtSettings:Audience"] 
            ?? throw new InvalidOperationException("JWT audience not configured");

        public static int JwtExpirationHours => int.Parse(_configuration?["JwtSettings:ExpirationHours"] ?? "24");

        public static string FileStoragePath => _configuration?["FileStorage:Path"] 
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public static long MaxFileSize => long.Parse(_configuration?["FileStorage:MaxFileSize"] ?? "10485760"); // 10MB

        public static string[] AllowedFileExtensions => _configuration?["FileStorage:AllowedExtensions"]?.Split(',') 
            ?? new[] { ".pdf", ".docx", ".doc", ".xlsx", ".xls" };

        public static string SmtpHost => _configuration?["EmailSettings:SmtpHost"] ?? "";
        public static int SmtpPort => int.Parse(_configuration?["EmailSettings:SmtpPort"] ?? "587");
        public static string SmtpUsername => _configuration?["EmailSettings:Username"] ?? "";
        public static string SmtpPassword => _configuration?["EmailSettings:Password"] ?? "";
        public static bool SmtpEnableSsl => bool.Parse(_configuration?["EmailSettings:EnableSsl"] ?? "true");
    }
}
