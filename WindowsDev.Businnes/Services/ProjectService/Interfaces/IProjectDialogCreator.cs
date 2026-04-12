using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.ProjectService.Interfaces
{
    public interface IProjectDialogCreator
    {
        event Func<Task> CloseRequested;
        void SetEditDialog(object? task) { }
        void SetProjectId(int  projectId) { }
    }
}


