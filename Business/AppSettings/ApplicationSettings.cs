namespace ProjectControlsReportingTool.API.Business.AppSettings
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationHours { get; set; } = 24;
    }

    public class FileStorageSettings
    {
        public string Path { get; set; } = "uploads";
        public long MaxFileSize { get; set; } = 10485760; // 10MB
        public string[] AllowedExtensions { get; set; } = { ".pdf", ".docx", ".doc", ".xlsx", ".xls" };
    }

    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }

    public class ApplicationSettings
    {
        public JwtSettings JwtSettings { get; set; } = new();
        public FileStorageSettings FileStorage { get; set; } = new();
        public EmailSettings EmailSettings { get; set; } = new();
    }
}
