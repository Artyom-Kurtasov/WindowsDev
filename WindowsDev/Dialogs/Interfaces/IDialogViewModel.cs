namespace WindowsDev.Dialogs.Interfaces
{
    public interface IDialogViewModel
    {
        event Func<Task>? CloseRequested;
        event Func<Task>? Completed;
    }
}
