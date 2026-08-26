namespace WindowsDev.Services.Dialogs;

public interface IDialogViewModel
{
    event Func<Task>? CloseRequested;
    event Func<Task>? Completed;
}