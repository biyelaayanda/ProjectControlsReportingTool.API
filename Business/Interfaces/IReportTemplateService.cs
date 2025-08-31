using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    public interface IReportTemplateService
    {
        // Template CRUD operations
        Task<ServiceResultDto> CreateTemplateAsync(CreateReportTemplateDto dto, Guid userId);
        Task<ServiceResultDto> UpdateTemplateAsync(Guid id, UpdateReportTemplateDto dto, Guid userId);
        Task<ServiceResultDto> DeleteTemplateAsync(Guid id, Guid userId);
        Task<ReportTemplateDto?> GetTemplateByIdAsync(Guid id);
        Task<PagedResultDto<ReportTemplateSummaryDto>> GetTemplatesAsync(ReportTemplateFilterDto filter);
        
        // Template usage
        Task<ServiceResultDto> CreateReportFromTemplateAsync(CreateReportFromTemplateDto dto, Guid userId);
        Task<IEnumerable<ReportTemplateSummaryDto>> GetActiveTemplatesAsync();
        Task<IEnumerable<ReportTemplateSummaryDto>> GetTemplatesByDepartmentAsync(Department department);
        Task<IEnumerable<ReportTemplateSummaryDto>> GetUserTemplatesAsync(Guid userId);
        
        // Template management
        Task<ServiceResultDto> ActivateTemplateAsync(Guid id, Guid userId);
        Task<ServiceResultDto> DeactivateTemplateAsync(Guid id, Guid userId);
        Task<ServiceResultDto> DuplicateTemplateAsync(Guid id, string newName, Guid userId);
        
        // Template discovery
        Task<IEnumerable<string>> GetTemplateTypesAsync();
        Task<IEnumerable<string>> GetTemplateTagsAsync();
        Task<IEnumerable<ReportTemplateSummaryDto>> SearchTemplatesAsync(string searchTerm);
        
        // Template validation
        Task<bool> ValidateTemplateNameAsync(string name, Guid? excludeId = null);
        Task<ServiceResultDto> PreviewTemplateAsync(Guid templateId, Dictionary<string, string>? variables = null);
    }
}
