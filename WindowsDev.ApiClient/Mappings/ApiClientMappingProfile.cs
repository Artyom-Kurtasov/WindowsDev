using AutoMapper;
using WindowsDev.Contracts.Api.Attachments;
using WindowsDev.Contracts.Api.Comments;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Contracts.Api.PasswordRecovery;
using WindowsDev.Contracts.Api.Profiles;
using WindowsDev.Contracts.Api.Projects;
using WindowsDev.Contracts.Api.Tasks;
using WindowsDev.Contracts.UI.Attachments;
using WindowsDev.Contracts.UI.Comments;
using WindowsDev.Contracts.UI.Identity;
using WindowsDev.Contracts.UI.PasswordRecovery;
using WindowsDev.Contracts.UI.Profiles;
using WindowsDev.Contracts.UI.Projects;
using WindowsDev.Contracts.UI.Tasks;

namespace WindowsDev.ApiClient.Mappings;

internal class ApiClientMappingProfile : Profile
{
    public ApiClientMappingProfile()
    {
        // Auth clients
        CreateMap<RegisterForm, UserRegisterRequest>();
        CreateMap<LoginForm, UserLoginRequest>();
        CreateMap<UserRegisterResponse, RegisterInfo>();
        CreateMap<UserLoginResponse, TokensInfo>();
        CreateMap<UsernameAvailabilityForm, IsUsernameAvailableRequest>();
        CreateMap<LoginAvailabilityForm, IsLoginAvailableRequest>();
        CreateMap<IsLoginAvailableResponse, AvailabilityInfo>();
        CreateMap<IsUsernameAvailableResponse, AvailabilityInfo>();

        // Attachment clients
        CreateMap<FileListItemResponse, FileItem>();

        // Comment clients
        CreateMap<CommentForm, AddCommentRequest>();
        CreateMap<GetCommentResponse, CommentInfo>();
        CreateMap<AddCommentResponse, CommentInfo>();

        // Project clients
        CreateMap<ProjectForm, AddProjectRequest>();
        CreateMap<GetProjectsResponse, ProjectInfo>();
        CreateMap<AddProjectResponse, ProjectInfo>();
        CreateMap<UpdateProjectResponse, UpdateProjectInfo>();

        // Password recovery
        CreateMap<IsUserExistForm, IsUserExistRequest>();
        CreateMap<IsUserExistResponse, IsUserExistInfo>();
        CreateMap<VerifyRecoveryCodeForm, VerifyRecoveryCodeRequest>();
        CreateMap<VerifyRecoveryCodeResponse, VerifyRecoveryCodeInfo>();
        CreateMap<ResetPasswordForm, ResetPasswordRequest>();
        CreateMap<ResetPasswordResponse, ResetPasswordInfo>();

        // Task clients
        CreateMap<TaskForm, AddTaskRequest>();
        CreateMap<GetTasksResponse, TaskInfo>();
        CreateMap<AddTaskResponse, TaskInfo>();
        CreateMap<UpdateTaskRequest, TaskForm>();
        CreateMap<UpdateTaskResponse, UpdateTaskInfo>();
        CreateMap<GetTaskByIdResponse, TaskInfo>();
        CreateMap<UpdateTaskForm, UpdateTaskRequest>();

        // Profile clients
        CreateMap<GetProfileResponse, ProfileInfo>();
        CreateMap<ChangePasswordForm, ChangePasswordRequest>();
        CreateMap<ChangeUsernameForm, ChangeUsernameRequest>();
        CreateMap<ChangePasswordResponse, ChangePasswordInfo>();
        CreateMap<ChangeUsernameResponse, ChangeUsernameInfo>();
    }
}
