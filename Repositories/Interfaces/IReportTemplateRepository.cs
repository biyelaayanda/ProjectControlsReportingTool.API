using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Models.DTOs;

namespace ProjectControlsReportingTool.API.Repositories.Interfaces
{
    public interface IReportTemplateRepository : IBaseRepository<ReportTemplate>
    {
        Task<IEnumerable<ReportTemplate>> GetActiveTemplatesAsync();
        Task<IEnumerable<ReportTemplate>> GetTemplatesByDepartmentAsync(Department department);
        Task<IEnumerable<ReportTemplate>> GetTemplatesByTypeAsync(string type);
        Task<IEnumerable<ReportTemplate>> GetSystemTemplatesAsync();
        Task<IEnumerable<ReportTemplate>> GetUserTemplatesAsync(Guid userId);
        Task<PagedResultDto<ReportTemplate>> GetFilteredTemplatesAsync(ReportTemplateFilterDto filter);
        Task<ReportTemplate?> GetTemplateWithUsageCountAsync(Guid id);
        Task<int> GetTemplateUsageCountAsync(Guid templateId);
        Task<IEnumerable<ReportTemplate>> SearchTemplatesAsync(string searchTerm);
        Task<bool> TemplateNameExistsAsync(string name, Guid? excludeId = null);
        Task<bool> CanDeleteTemplateAsync(Guid templateId);
        Task<IEnumerable<string>> GetAllTemplateTypesAsync();
        Task<IEnumerable<string>> GetAllTagsAsync();
    }
}
