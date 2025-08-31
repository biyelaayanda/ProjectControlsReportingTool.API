using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Repositories.Interfaces;

namespace ProjectControlsReportingTool.API.Business.Services
{
    public class ReportTemplateService : IReportTemplateService
    {
        private readonly IReportTemplateRepository _templateRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public ReportTemplateService(
            IReportTemplateRepository templateRepository,
            IReportRepository reportRepository,
            IAuditLogRepository auditLogRepository)
        {
            _templateRepository = templateRepository;
            _reportRepository = reportRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<ServiceResultDto> CreateTemplateAsync(CreateReportTemplateDto dto, Guid userId)
        {
            try
            {
                // Check if name already exists
                if (await _templateRepository.TemplateNameExistsAsync(dto.Name))
                {
                    return ServiceResultDto.ErrorResult("A template with this name already exists.");
                }

                var template = new ReportTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    ContentTemplate = dto.ContentTemplate,
                    Type = dto.Type,
                    DefaultPriority = dto.DefaultPriority,
                    DefaultDepartment = dto.DefaultDepartment,
                    Tags = dto.Tags,
                    SortOrder = dto.SortOrder,
                    DefaultTitle = dto.DefaultTitle,
                    DefaultDueDays = dto.DefaultDueDays,
                    Instructions = dto.Instructions,
                    IsActive = true,
                    IsSystemTemplate = false,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = userId
                };

                var createdTemplate = await _templateRepository.AddAsync(template);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Created,
                    userId,
                    null,
                    $"Created template: {template.Name}"
                );

                var result = MapToTemplateDto(createdTemplate);
                return ServiceResultDto.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResultDto.ErrorResult($"Failed to create template: {ex.Message}");
            }
        }

        public async Task<ServiceResultDto> UpdateTemplateAsync(Guid id, UpdateReportTemplateDto dto, Guid userId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(id);
                if (template == null)
                {
                    return ServiceResultDto.ErrorResult("Template not found.");
                }

                if (template.IsSystemTemplate)
                {
                    return ServiceResultDto.ErrorResult("System templates cannot be modified.");
                }

                // Check if name already exists (excluding current template)
                if (await _templateRepository.TemplateNameExistsAsync(dto.Name, id))
                {
                    return ServiceResultDto.ErrorResult("A template with this name already exists.");
                }

                // Update properties
                template.Name = dto.Name;
                template.Description = dto.Description;
                template.ContentTemplate = dto.ContentTemplate;
                template.Type = dto.Type;
                template.DefaultPriority = dto.DefaultPriority;
                template.DefaultDepartment = dto.DefaultDepartment;
                template.IsActive = dto.IsActive;
                template.Tags = dto.Tags;
                template.SortOrder = dto.SortOrder;
                template.DefaultTitle = dto.DefaultTitle;
                template.DefaultDueDays = dto.DefaultDueDays;
                template.Instructions = dto.Instructions;
                template.LastModifiedDate = DateTime.UtcNow;
                template.LastModifiedBy = userId;

                await _templateRepository.UpdateAsync(template);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated,
                    userId,
                    null,
                    $"Updated template: {template.Name}"
                );

                var result = MapToTemplateDto(template);
                return ServiceResultDto.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResultDto.ErrorResult($"Failed to update template: {ex.Message}");
            }
        }

        public async Task<ServiceResultDto> DeleteTemplateAsync(Guid id, Guid userId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(id);
                if (template == null)
                {
                    return ServiceResultDto.ErrorResult("Template not found.");
                }

                if (!await _templateRepository.CanDeleteTemplateAsync(id))
                {
                    return ServiceResultDto.ErrorResult("Cannot delete template. It may be a system template or in use by existing reports.");
                }

                await _templateRepository.DeleteAsync(template);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Deleted,
                    userId,
                    null,
                    $"Deleted template: {template.Name}"
                );

                return ServiceResultDto.SuccessResult();
            }
            catch (Exception ex)
            {
                return ServiceResultDto.ErrorResult($"Failed to delete template: {ex.Message}");
            }
        }

        public async Task<ReportTemplateDto?> GetTemplateByIdAsync(Guid id)
        {
            var template = await _templateRepository.GetTemplateWithUsageCountAsync(id);
            if (template == null) return null;

            var usageCount = await _templateRepository.GetTemplateUsageCountAsync(id);
            return MapToTemplateDto(template, usageCount);
        }

        public async Task<PagedResultDto<ReportTemplateSummaryDto>> GetTemplatesAsync(ReportTemplateFilterDto filter)
        {
            var result = await _templateRepository.GetFilteredTemplatesAsync(filter);
            
            var summaries = new List<ReportTemplateSummaryDto>();
            foreach (var template in result.Items)
            {
                var usageCount = await _templateRepository.GetTemplateUsageCountAsync(template.Id);
                summaries.Add(MapToTemplateSummaryDto(template, usageCount));
            }

            return new PagedResultDto<ReportTemplateSummaryDto>
            {
                Items = summaries,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                HasNext = result.HasNext,
                HasPrevious = result.HasPrevious
            };
        }

        public async Task<ServiceResultDto> CreateReportFromTemplateAsync(CreateReportFromTemplateDto dto, Guid userId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(dto.TemplateId);
                if (template == null)
                {
                    return ServiceResultDto.ErrorResult("Template not found.");
                }

                if (!template.IsActive)
                {
                    return ServiceResultDto.ErrorResult("Template is not active.");
                }

                // Process template content with variable replacements
                var processedContent = ProcessTemplateVariables(template.ContentTemplate, dto.VariableReplacements);

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    Title = dto.CustomTitle ?? template.DefaultTitle ?? template.Name,
                    Content = processedContent,
                    Description = dto.CustomDescription ?? template.Description,
                    Type = template.Type,
                    Priority = dto.CustomPriority ?? template.DefaultPriority,
                    Department = dto.CustomDepartment ?? template.DefaultDepartment ?? Department.ProjectSupport,
                    DueDate = dto.CustomDueDate ?? (template.DefaultDueDays.HasValue ? DateTime.UtcNow.AddDays(template.DefaultDueDays.Value) : null),
                    Status = ReportStatus.Draft,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    TemplateId = template.Id
                };

                var createdReport = await _reportRepository.AddAsync(report);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Created,
                    userId,
                    createdReport.Id,
                    $"Created report from template: {template.Name}"
                );

                return ServiceResultDto.SuccessResult(new { reportId = createdReport.Id, templateUsed = template.Name });
            }
            catch (Exception ex)
            {
                return ServiceResultDto.ErrorResult($"Failed to create report from template: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ReportTemplateSummaryDto>> GetActiveTemplatesAsync()
        {
            var templates = await _templateRepository.GetActiveTemplatesAsync();
            var result = new List<ReportTemplateSummaryDto>();
            
            foreach (var template in templates)
            {
                var usageCount = await _templateRepository.GetTemplateUsageCountAsync(template.Id);
                result.Add(MapToTemplateSummaryDto(template, usageCount));
            }
            
            return result;
        }

        public async Task<IEnumerable<ReportTemplateSummaryDto>> GetTemplatesByDepartmentAsync(Department department)
        {
            var templates = await _templateRepository.GetTemplatesByDepartmentAsync(department);
            var result = new List<ReportTemplateSummaryDto>();
            
            foreach (var template in templates)
            {
                var usageCount = await _templateRepository.GetTemplateUsageCountAsync(template.Id);
                result.Add(MapToTemplateSummaryDto(template, usageCount));
            }
            
            return result;
        }

        public async Task<IEnumerable<ReportTemplateSummaryDto>> GetUserTemplatesAsync(Guid userId)
        {
            var templates = await _templateRepository.GetUserTemplatesAsync(userId);
            var result = new List<ReportTemplateSummaryDto>();
            
            foreach (var template in templates)
            {
                var usageCount = await _templateRepository.GetTemplateUsageCountAsync(template.Id);
                result.Add(MapToTemplateSummaryDto(template, usageCount));
            }
            
            return result;
        }

        public async Task<ServiceResultDto> ActivateTemplateAsync(Guid id, Guid userId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(id);
                if (template == null)
                {
                    return ServiceResultDto.ErrorResult("Template not found.");
                }

                template.IsActive = true;
                template.LastModifiedDate = DateTime.UtcNow;
                template.LastModifiedBy = userId;

                await _templateRepository.UpdateAsync(template);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated,
                    userId,
                    null,
                    $"Activated template: {template.Name}"
                );

                return ServiceResultDto.SuccessResult();
            }
            catch (Exception ex)
            {
                return ServiceResultDto.ErrorResult($"Failed to activate template: {ex.Message}");
            }
        }

        public async Task<ServiceResultDto> DeactivateTemplateAsync(Guid id, Guid userId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(id);
                if (template == null)
                {
                    return ServiceResultDto.ErrorResult("Template not found.");
                }

                if (template.IsSystemTemplate)
                {
                    return ServiceResultDto.ErrorResult("System templates cannot be deactivated.");
                }

                template.IsActive = false;
                template.LastModifiedDate = DateTime.UtcNow;
                template.LastModifiedBy = userId;

                await _templateRepository.UpdateAsync(template);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Updated,
                    userId,
                    null,
                    $"Deactivated template: {template.Name}"
                );

                return ServiceResultDto.SuccessResult();
            }
            catch (Exception ex)
            {
                return ServiceResultDto.ErrorResult($"Failed to deactivate template: {ex.Message}");
            }
        }

        public async Task<ServiceResultDto> DuplicateTemplateAsync(Guid id, string newName, Guid userId)
        {
            try
            {
                var originalTemplate = await _templateRepository.GetByIdAsync(id);
                if (originalTemplate == null)
                {
                    return ServiceResultDto.ErrorResult("Template not found.");
                }

                if (await _templateRepository.TemplateNameExistsAsync(newName))
                {
                    return ServiceResultDto.ErrorResult("A template with this name already exists.");
                }

                var duplicatedTemplate = new ReportTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = newName,
                    Description = originalTemplate.Description,
                    ContentTemplate = originalTemplate.ContentTemplate,
                    Type = originalTemplate.Type,
                    DefaultPriority = originalTemplate.DefaultPriority,
                    DefaultDepartment = originalTemplate.DefaultDepartment,
                    Tags = originalTemplate.Tags,
                    SortOrder = originalTemplate.SortOrder,
                    DefaultTitle = originalTemplate.DefaultTitle,
                    DefaultDueDays = originalTemplate.DefaultDueDays,
                    Instructions = originalTemplate.Instructions,
                    IsActive = true,
                    IsSystemTemplate = false,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = userId
                };

                var createdTemplate = await _templateRepository.AddAsync(duplicatedTemplate);

                await _auditLogRepository.LogActionAsync(
                    AuditAction.Created,
                    userId,
                    null,
                    $"Duplicated template: {originalTemplate.Name} -> {newName}"
                );

                var result = MapToTemplateDto(createdTemplate);
                return ServiceResultDto.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResultDto.ErrorResult($"Failed to duplicate template: {ex.Message}");
            }
        }

        public async Task<IEnumerable<string>> GetTemplateTypesAsync()
        {
            return await _templateRepository.GetAllTemplateTypesAsync();
        }

        public async Task<IEnumerable<string>> GetTemplateTagsAsync()
        {
            return await _templateRepository.GetAllTagsAsync();
        }

        public async Task<IEnumerable<ReportTemplateSummaryDto>> SearchTemplatesAsync(string searchTerm)
        {
            var templates = await _templateRepository.SearchTemplatesAsync(searchTerm);
            var result = new List<ReportTemplateSummaryDto>();
            
            foreach (var template in templates)
            {
                var usageCount = await _templateRepository.GetTemplateUsageCountAsync(template.Id);
                result.Add(MapToTemplateSummaryDto(template, usageCount));
            }
            
            return result;
        }

        public async Task<bool> ValidateTemplateNameAsync(string name, Guid? excludeId = null)
        {
            return !await _templateRepository.TemplateNameExistsAsync(name, excludeId);
        }

        public async Task<ServiceResultDto> PreviewTemplateAsync(Guid templateId, Dictionary<string, string>? variables = null)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return ServiceResultDto.ErrorResult("Template not found.");
                }

                var processedContent = ProcessTemplateVariables(template.ContentTemplate, variables);
                
                var preview = new
                {
                    title = template.DefaultTitle ?? template.Name,
                    content = processedContent,
                    description = template.Description,
                    type = template.Type,
                    priority = template.DefaultPriority,
                    department = template.DefaultDepartment?.ToString(),
                    instructions = template.Instructions
                };

                return ServiceResultDto.SuccessResult(preview);
            }
            catch (Exception ex)
            {
                return ServiceResultDto.ErrorResult($"Failed to preview template: {ex.Message}");
            }
        }

        #region Private Helper Methods

        private static string ProcessTemplateVariables(string content, Dictionary<string, string>? variables)
        {
            if (variables == null || variables.Count == 0)
                return content;

            var processedContent = content;
            foreach (var variable in variables)
            {
                var placeholder = $"{{{variable.Key}}}";
                processedContent = processedContent.Replace(placeholder, variable.Value);
            }

            return processedContent;
        }

        private static ReportTemplateDto MapToTemplateDto(ReportTemplate template, int usageCount = 0)
        {
            return new ReportTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                ContentTemplate = template.ContentTemplate,
                Type = template.Type,
                DefaultPriority = template.DefaultPriority,
                DefaultDepartment = template.DefaultDepartment,
                DepartmentName = template.DepartmentName,
                IsActive = template.IsActive,
                IsSystemTemplate = template.IsSystemTemplate,
                IsEditable = template.IsEditable,
                CreatedDate = template.CreatedDate,
                LastModifiedDate = template.LastModifiedDate,
                CreatorName = template.Creator?.FullName ?? "System",
                LastModifiedByName = template.LastModifiedByUser?.FullName,
                Tags = template.Tags,
                TagList = template.GetTagList(),
                SortOrder = template.SortOrder,
                DefaultTitle = template.DefaultTitle,
                DefaultDueDays = template.DefaultDueDays,
                Instructions = template.Instructions,
                UsageCount = usageCount
            };
        }

        private static ReportTemplateSummaryDto MapToTemplateSummaryDto(ReportTemplate template, int usageCount = 0)
        {
            return new ReportTemplateSummaryDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Type = template.Type,
                DefaultDepartment = template.DefaultDepartment,
                DepartmentName = template.DepartmentName,
                IsActive = template.IsActive,
                IsSystemTemplate = template.IsSystemTemplate,
                CreatedDate = template.CreatedDate,
                CreatorName = template.Creator?.FullName ?? "System",
                TagList = template.GetTagList(),
                UsageCount = usageCount
            };
        }

        #endregion
    }
}
