using AutoMapper;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Business.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => GetDepartmentName(src.Department)));

            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLowerInvariant()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());

            // Report mappings
            CreateMap<Report, ReportDto>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => $"{src.Creator.FirstName} {src.Creator.LastName}"))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => GetDepartmentName(src.Department)))
                .ForMember(dest => dest.RejectedByName, opt => opt.MapFrom(src => src.RejectedByUser != null ? $"{src.RejectedByUser.FirstName} {src.RejectedByUser.LastName}" : null))
                .ForMember(dest => dest.CanBeEdited, opt => opt.MapFrom(src => src.Status == ReportStatus.Draft))
                .ForMember(dest => dest.CanBeSubmitted, opt => opt.MapFrom(src => src.Status == ReportStatus.Draft))
                .ForMember(dest => dest.IsInProgress, opt => opt.MapFrom(src => src.Status != ReportStatus.Draft && src.Status != ReportStatus.Completed && src.Status != ReportStatus.Rejected));

            CreateMap<Report, ReportSummaryDto>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => $"{src.Creator.FirstName} {src.Creator.LastName}"))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => GetDepartmentName(src.Department)));

            CreateMap<Report, ReportDetailDto>()
                .IncludeBase<Report, ReportDto>();

            CreateMap<CreateReportDto, Report>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ReportStatus.Draft))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Report Signature mappings
            CreateMap<ReportSignature, ReportSignatureDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.SignatureTypeName, opt => opt.MapFrom(src => src.SignatureType.ToString()));

            // Report Attachment mappings
            CreateMap<ReportAttachment, ReportAttachmentDto>()
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => $"{src.UploadedByUser.FirstName} {src.UploadedByUser.LastName}"));
        }

        private static string GetDepartmentName(Department department)
        {
            return department switch
            {
                Department.ProjectSupport => "Project Support",
                Department.DocManagement => "Document Management",
                Department.QS => "Quantity Surveying",
                Department.ContractsManagement => "Contracts Management",
                Department.BusinessAssurance => "Business Assurance",
                _ => "Unknown"
            };
        }
    }
}
