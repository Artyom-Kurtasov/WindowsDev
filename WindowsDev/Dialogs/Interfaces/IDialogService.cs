using System.Windows.Controls;

namespace WindowsDev.Dialogs.Interfaces
{
    public interface IDialogService
    {
        Task ShowDialogAsync<TView, TViewModel>(object context, params object[] args)
            where TView : UserControl, new()
            where TViewModel : class, IDialogViewModel;
    }
}
