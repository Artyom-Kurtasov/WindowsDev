using AutoMapper;
using WindowsDev.Api.DTO.Request.ProjectsController;
using WindowsDev.Api.DTO.Request.ProjectService;
using WindowsDev.Api.DTO.Request.TasksController;
using WindowsDev.Api.DTO.Response.ProjectsController;
using WindowsDev.Api.DTO.Response.TasksController;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Api.Mappings;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // Projects Controller
        CreateMap<ProjectsInfo, AddResponse>();
        CreateMap<ProjectsInfo, UpdateResponse>();
        CreateMap<AddProjectRequest, ProjectsInfo>();
        CreateMap<ProjectsInfo, GetProjectsResponse>();
        CreateMap<ProjectsInfo, GetByIdResponse>();
        CreateMap<UpdateProjectRequest, ProjectsInfo>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Tasks Controller
        CreateMap<TasksInfo, AddTaskResponse>();
        CreateMap<AddTaskRequest, TasksInfo>();
        CreateMap<TasksInfo, UpdateTaskResponse>();
        CreateMap<UpdateTaskRequest, TasksInfo>()
            .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
            .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null));
        CreateMap<GetTasksRequest, TaskFilter>();
        CreateMap<TasksInfo, GetTasksResponse>();
        CreateMap<TasksInfo, GetTaskByIdResponse>();
    }
}
