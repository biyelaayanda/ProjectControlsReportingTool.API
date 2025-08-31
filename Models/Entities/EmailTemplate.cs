using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    /// <summary>
    /// Entity representing customizable email templates for administrators
    /// Allows admin users to create, modify, and manage email templates for various notification types
    /// </summary>
    [Table("EmailTemplates")]
    public class EmailTemplate
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TemplateType { get; set; } = string.Empty; // NotificationEmail, ReportStatus, Welcome, etc.

        [Required]
        [StringLength(300)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string HtmlContent { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string PlainTextContent { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // System, User, Report, Workflow, etc.

        public bool IsActive { get; set; } = true;

        public bool IsSystemTemplate { get; set; } = false;

        public bool IsDefault { get; set; } = false;

        [StringLength(2000)]
        public string Variables { get; set; } = string.Empty; // JSON string of available variables

        [StringLength(1000)]
        public string PreviewData { get; set; } = string.Empty; // JSON string for template preview

        public int Version { get; set; } = 1;

        public int UsageCount { get; set; } = 0;

        public DateTime LastUsed { get; set; } = DateTime.UtcNow;

        // Audit fields
        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual User? LastUpdater { get; set; }

        // Template validation and parsing
        public bool IsValidTemplate()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Subject) &&
                   !string.IsNullOrWhiteSpace(HtmlContent) &&
                   !string.IsNullOrWhiteSpace(TemplateType);
        }

        public List<string> GetVariableNames()
        {
            if (string.IsNullOrWhiteSpace(Variables))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Variables) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public void SetVariableNames(List<string> variableNames)
        {
            Variables = System.Text.Json.JsonSerializer.Serialize(variableNames);
        }

        public Dictionary<string, object> GetPreviewData()
        {
            if (string.IsNullOrWhiteSpace(PreviewData))
                return new Dictionary<string, object>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(PreviewData) ?? 
                       new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        public void SetPreviewData(Dictionary<string, object> previewData)
        {
            PreviewData = System.Text.Json.JsonSerializer.Serialize(previewData);
        }

        public void IncrementUsage()
        {
            UsageCount++;
            LastUsed = DateTime.UtcNow;
        }
    }
}
