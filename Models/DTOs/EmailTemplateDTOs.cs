using System.ComponentModel.DataAnnotations;

namespace ProjectControlsReportingTool.API.Models.DTOs
{
    /// <summary>
    /// DTO for displaying email template information
    /// </summary>
    public class EmailTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string PlainTextContent { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSystemTemplate { get; set; }
        public bool IsDefault { get; set; }
        public List<string> Variables { get; set; } = new();
        public Dictionary<string, object> PreviewData { get; set; } = new();
        public int Version { get; set; }
        public int UsageCount { get; set; }
        public DateTime LastUsed { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating new email templates
    /// </summary>
    public class CreateEmailTemplateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TemplateType { get; set; } = string.Empty;

        [Required]
        [StringLength(300, MinimumLength = 5)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string HtmlContent { get; set; } = string.Empty;

        [MinLength(10)]
        public string PlainTextContent { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = "User";

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public List<string> Variables { get; set; } = new();

        public Dictionary<string, object> PreviewData { get; set; } = new();
    }

    /// <summary>
    /// DTO for updating existing email templates
    /// </summary>
    public class UpdateEmailTemplateDto
    {
        [StringLength(200, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(300, MinimumLength = 5)]
        public string? Subject { get; set; }

        [MinLength(10)]
        public string? HtmlContent { get; set; }

        [MinLength(10)]
        public string? PlainTextContent { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDefault { get; set; }

        public List<string>? Variables { get; set; }

        public Dictionary<string, object>? PreviewData { get; set; }
    }

    /// <summary>
    /// DTO for bulk email template operations
    /// </summary>
    public class BulkEmailTemplateOperationDto
    {
        [Required]
        public List<Guid> TemplateIds { get; set; } = new();

        [Required]
        [StringLength(50)]
        public string Operation { get; set; } = string.Empty; // activate, deactivate, delete, export

        public string? Category { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for email template search and filtering
    /// </summary>
    public class EmailTemplateSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? TemplateType { get; set; }
        public string? Category { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSystemTemplate { get; set; }
        public bool? IsDefault { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? LastUsedFrom { get; set; }
        public DateTime? LastUsedTo { get; set; }
        public int? MinUsageCount { get; set; }
        public int? MaxUsageCount { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for email template preview and testing
    /// </summary>
    public class EmailTemplatePreviewDto
    {
        [Required]
        public Guid TemplateId { get; set; }

        public Dictionary<string, object> TestData { get; set; } = new();

        public string? RecipientEmail { get; set; }

        public bool SendTestEmail { get; set; } = false;
    }

    /// <summary>
    /// DTO for rendered email template result
    /// </summary>
    public class RenderedEmailTemplateDto
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string PlainTextContent { get; set; } = string.Empty;
        public Dictionary<string, object> UsedVariables { get; set; } = new();
        public List<string> MissingVariables { get; set; } = new();
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public DateTime RenderedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for email template statistics
    /// </summary>
    public class EmailTemplateStatsDto
    {
        public int TotalTemplates { get; set; }
        public int ActiveTemplates { get; set; }
        public int InactiveTemplates { get; set; }
        public int SystemTemplates { get; set; }
        public int UserTemplates { get; set; }
        public int DefaultTemplates { get; set; }
        public Dictionary<string, int> TemplatesByType { get; set; } = new();
        public Dictionary<string, int> TemplatesByCategory { get; set; } = new();
        public Dictionary<string, int> TemplatesByCreator { get; set; } = new();
        public int TotalUsageCount { get; set; }
        public EmailTemplateUsageStatsDto MostUsedTemplate { get; set; } = new();
        public EmailTemplateUsageStatsDto RecentlyCreated { get; set; } = new();
        public EmailTemplateUsageStatsDto RecentlyUpdated { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for email template usage statistics
    /// </summary>
    public class EmailTemplateUsageStatsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public DateTime LastUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for email template export/import operations
    /// </summary>
    public class EmailTemplateExportDto
    {
        public List<Guid> TemplateIds { get; set; } = new();
        public string Format { get; set; } = "json"; // json, xml, csv
        public bool IncludeSystemTemplates { get; set; } = false;
        public bool IncludeUsageStats { get; set; } = true;
        public string? Category { get; set; }
        public string? TemplateType { get; set; }
    }

    /// <summary>
    /// DTO for email template import operations
    /// </summary>
    public class EmailTemplateImportDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Format { get; set; } = "json";

        public bool OverwriteExisting { get; set; } = false;

        public bool ValidateOnly { get; set; } = false;

        public string? DefaultCategory { get; set; }

        public bool SetAsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for email template validation results
    /// </summary>
    public class EmailTemplateValidationDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> DetectedVariables { get; set; } = new();
        public Dictionary<string, string> SuggestedCorrections { get; set; } = new();
        public int EstimatedRenderTime { get; set; } // in milliseconds
    }

    /// <summary>
    /// DTO for email template duplication
    /// </summary>
    public class DuplicateEmailTemplateDto
    {
        [Required]
        public Guid SourceTemplateId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string NewName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? NewDescription { get; set; }

        [StringLength(100)]
        public string? NewCategory { get; set; }

        public bool CopyUsageStats { get; set; } = false;

        public bool SetAsActive { get; set; } = true;

        public bool SetAsDefault { get; set; } = false;
    }
}
