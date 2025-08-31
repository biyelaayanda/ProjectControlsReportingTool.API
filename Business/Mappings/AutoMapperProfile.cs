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
                .ForMember(dest => dest.CreatorRole, opt => opt.MapFrom(src => src.Creator.Role))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => GetDepartmentName(src.Department)))
                .ForMember(dest => dest.RejectedByName, opt => opt.MapFrom(src => src.RejectedByUser != null ? $"{src.RejectedByUser.FirstName} {src.RejectedByUser.LastName}" : null))
                .ForMember(dest => dest.CanBeEdited, opt => opt.MapFrom(src => src.Status == ReportStatus.Draft))
                .ForMember(dest => dest.CanBeSubmitted, opt => opt.MapFrom(src => src.Status == ReportStatus.Draft))
                .ForMember(dest => dest.IsInProgress, opt => opt.MapFrom(src => src.Status != ReportStatus.Draft && src.Status != ReportStatus.Completed && src.Status != ReportStatus.Rejected));

            CreateMap<Report, ReportSummaryDto>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => $"{src.Creator.FirstName} {src.Creator.LastName}"))
                .ForMember(dest => dest.CreatorRole, opt => opt.MapFrom(src => src.Creator.Role))
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
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.UploadedByName) ? src.UploadedByName : 
                    src.UploadedByUser != null ? $"{src.UploadedByUser.FirstName} {src.UploadedByUser.LastName}" : "Unknown"))
                .ForMember(dest => dest.ApprovalStageName, opt => opt.MapFrom(src => src.ApprovalStage.ToString()))
                .ForMember(dest => dest.UploadedByRoleName, opt => opt.MapFrom(src => src.UploadedByRole.ToString()));

            // EmailTemplate mappings
            CreateMap<EmailTemplate, EmailTemplateDto>()
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : "System"))
                .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => src.LastUpdater != null ? $"{src.LastUpdater.FirstName} {src.LastUpdater.LastName}" : "System"))
                .ForMember(dest => dest.Variables, opt => opt.MapFrom(src => src.GetVariableNames()))
                .ForMember(dest => dest.PreviewData, opt => opt.MapFrom(src => src.GetPreviewData()));

            CreateMap<CreateEmailTemplateDto, EmailTemplate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsSystemTemplate, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => 1))
                .ForMember(dest => dest.UsageCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateEmailTemplateDto, EmailTemplate>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EmailTemplateDto, CreateEmailTemplateDto>();
            CreateMap<EmailTemplateDto, UpdateEmailTemplateDto>();

            // Push Notification mappings
            CreateMap<PushNotificationSubscription, PushNotificationSubscriptionDto>();

            CreateMap<CreatePushNotificationSubscriptionDto, PushNotificationSubscription>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.HasPermission, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.SuccessfulNotifications, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.FailedNotifications, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // SMS mappings
            CreateMap<SmsMessage, SmsMessageDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
                .ForMember(dest => dest.RelatedReportTitle, opt => opt.MapFrom(src => src.RelatedReport != null ? src.RelatedReport.Title : null));

            CreateMap<SmsMessage, SmsDeliveryDto>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id));

            CreateMap<SmsTemplate, SmsTemplateDto>()
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}"))
                .ForMember(dest => dest.Variables, opt => opt.MapFrom(src => src.VariablesList));

            CreateMap<CreateSmsTemplateDto, SmsTemplate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsSystemTemplate, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.UsageCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
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
                (Department)0 => "Unknown Department", // Handle 0 value explicitly
                _ => "Unknown Department"
            };
        }
    }
}
