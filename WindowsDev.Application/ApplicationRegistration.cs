using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Application.Services.Authorization;
using WindowsDev.Application.Services.DebounceService;
using WindowsDev.Application.Services.PasswordManager;
using WindowsDev.Application.Services.PasswordManager.Hasher;
using WindowsDev.Application.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Application.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Application.Services.Profile;
using WindowsDev.Application.Services.ProjectService;
using WindowsDev.Application.Services.Registration;
using WindowsDev.Application.Services.TaskService;
using WindowsDev.Application.Services.TaskService.Attachment;
using WindowsDev.Application.Services.TaskService.Comment;
using WindowsDev.Application.Services.UserManager;

namespace WindowsDev.Application
{
    public static class ApplicationRegistration
    {
        public static IServiceCollection RegistrateApplication(this IServiceCollection services)
        {
            // Auth
            services.AddTransient<IAuthorization, Authorization>();
            services.AddTransient<IRegistration, Registration>();

            // Hasher
            services.AddTransient<IHasherFactory, HasherFactory>();
            services.AddTransient<DefaultHasher>();
            services.AddTransient<SimpleHasher>();

            // Passwords
            services.AddTransient<IPasswordRecoveryService, PasswordRecoveryService>();
            services.AddTransient<IPasswordChanger, PasswordChanger>();

            // Projects
            services.AddTransient<IProjectService, ProjectService>();

            // Tasks
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<ICommentService, CommentsService>();
            services.AddTransient<IAttacmentService, AttachmentService>();

            // Profile
            services.AddTransient<IProfileService, ProfileService>();

            // User
            services.AddSingleton<ICurrentUserService, CurrentUserService>();

            // Debounce
            services.AddTransient<IDebounceService, DebounceService>();

            return services;
        }
    }
}
