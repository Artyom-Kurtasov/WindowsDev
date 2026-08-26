using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Application.DatabaseInterfaces;
using WindowsDev.Application.RepositoriesInterfaces;
using WindowsDev.Application.Services.TaskService.Attachment.FileServiceInterfaces;
using WindowsDev.Infrastructure.Database;
using WindowsDev.Infrastructure.Database.Interfaces;
using WindowsDev.Infrastructure.Repositories;

namespace WindowsDev.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection RegistrateInfrastructure(this IServiceCollection services)
    {
        // File
        services.AddTransient<IFileOpener, FileOpener.FileOpener>();

        // Repositories
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();
        services.AddTransient<ITaskRepository, TaskRepository>();
        services.AddTransient<IAttachmentRepository, AttachmentRepository>();
        services.AddTransient<ICommentRepository, CommentRepository>();

        // Database
        services.AddSingleton<IDbCreator, DbCreator>();
        services.AddSingleton<IDatabaseConfig, DatabaseConfig>();
        services.AddTransient<IDbHealthChecker, DbHealthChecker>();

        return services;
    }
}