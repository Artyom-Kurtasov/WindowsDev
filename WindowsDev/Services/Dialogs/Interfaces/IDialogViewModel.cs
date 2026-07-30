namespace WindowsDev.Services.Dialogs.Interfaces
{
    public interface IDialogViewModel
    {
        event Func<Task>? CloseRequested;
        event Func<Task>? Completed;
    }
}
