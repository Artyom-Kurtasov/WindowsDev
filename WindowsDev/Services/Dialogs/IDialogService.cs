using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;

namespace WindowsDev.Services.Dialogs;

public interface IDialogService
{
    Task ShowDialogAsync<TView, TViewModel>(params object[] args)
        where TView : UserControl, new()
        where TViewModel : class, IDialogViewModel;

    Task ShowErrorDialogAsync(string message, params object[] args);
    Task ShowMessageAsync(string title, string message, MessageDialogStyle style);
}