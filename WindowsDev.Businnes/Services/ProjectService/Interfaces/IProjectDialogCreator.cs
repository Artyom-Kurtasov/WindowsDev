namespace WindowsDev.Businnes.Services.ProjectService.Interfaces
{
    public interface IProjectDialogCreator
    {
        event Func<Task> Close;
    }
}
