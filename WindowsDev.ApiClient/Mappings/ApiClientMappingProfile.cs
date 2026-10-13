using AutoMapper;
using WindowsDev.Contracts.Api.Attachments;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Contracts.UI;

namespace WindowsDev.ApiClient.Mappings;

internal class ApiClientMappingProfile : Profile
{
    public ApiClientMappingProfile()
    {
        // Auth clients
        CreateMap<RegisterForm, UserRegisterRequest>();
        CreateMap<LoginForm, UserLoginRequest>();

        // Task clients
        CreateMap<AttachmentForm, AddAttachmentRequest>();
        CreateMap<AddAttachmentResponse, AttachmentInfo>();
        CreateMap<GetAttachmentResponse, AttachmentInfo>();
    }
}
