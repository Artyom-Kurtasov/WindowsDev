namespace WindowsDev.Business.Services.ProjectService.Interfaces
{
    public interface IProjectDialogCreator
    {
        event Func<Task>? CloseRequested;
        event Func<Task>? Completed;
        void SetEditDialog(object? task) { }
        void SetProjectId(int  projectId) { }
    }
}


