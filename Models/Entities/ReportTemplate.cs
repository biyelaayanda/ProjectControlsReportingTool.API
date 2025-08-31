using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Models.Entities
{
    [Table("ReportTemplates")]
    public class ReportTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string ContentTemplate { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Type { get; set; }

        [StringLength(20)]
        public string DefaultPriority { get; set; } = "Medium";

        public Department? DefaultDepartment { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystemTemplate { get; set; } = false;

        [Required]
        public Guid CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        public Guid? LastModifiedBy { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        public int SortOrder { get; set; } = 0;

        // Template metadata
        [StringLength(200)]
        public string? DefaultTitle { get; set; }

        public int? DefaultDueDays { get; set; }

        [StringLength(1000)]
        public string? Instructions { get; set; }

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; } = null!;

        [ForeignKey("LastModifiedBy")]
        public virtual User? LastModifiedByUser { get; set; }

        public virtual ICollection<Report> ReportsCreatedFromTemplate { get; set; } = new List<Report>();

        // Computed properties
        [NotMapped]
        public string DepartmentName => DefaultDepartment?.ToString() ?? "Any";

        [NotMapped]
        public bool IsEditable => !IsSystemTemplate;

        // Helper methods
        public List<string> GetTagList()
        {
            return string.IsNullOrEmpty(Tags) 
                ? new List<string>() 
                : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
        }
    }
}
