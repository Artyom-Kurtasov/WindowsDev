using AutoMapper;
using WindowsDev.Contracts.Api.Attachments;
using WindowsDev.Contracts.Api.Comments;
using WindowsDev.Contracts.Api.Projects;
using WindowsDev.Contracts.Api.Tasks;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Api.Mappings;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // Projects Controller
        CreateMap<ProjectsInfo, AddProjectResponse>();
        CreateMap<ProjectsInfo, UpdateProjectResponse>();
        CreateMap<AddProjectRequest, ProjectsInfo>();
        CreateMap<ProjectsInfo, GetProjectsResponse>();
        CreateMap<ProjectsInfo, GetProjectByIdResponse>();
        CreateMap<UpdateProjectRequest, ProjectsInfo>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Tasks Controller
        CreateMap<TasksInfo, AddTaskResponse>();
        CreateMap<AddTaskRequest, TasksInfo>();
        CreateMap<TasksInfo, UpdateTaskResponse>();
        CreateMap<UpdateTaskRequest, TasksInfo>()
            .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
            .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null));
        CreateMap<TasksInfo, GetTasksResponse>();
        CreateMap<TasksInfo, GetTaskByIdResponse>();

        // Comments Controller
        CreateMap<TaskComment, AddCommentResponse>();
        CreateMap<TaskComment, GetCommentResponse>();

        // Attachment Controller
        CreateMap<TaskAttachment, GetAttachmentResponse>();
        CreateMap<TaskAttachment, AddAttachmentResponse>();
        CreateMap<AddAttachmentRequest, TaskAttachment>();
    }
}
