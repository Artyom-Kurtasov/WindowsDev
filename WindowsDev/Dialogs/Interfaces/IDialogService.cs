using System.Windows.Controls;
using WindowsDev.Business.Services.ProjectService.Interfaces;

namespace WindowsDev.Dialogs.Interfaces
{
    public interface IDialogService
    {
        Task ShowTaskDialogAsync<TView, TViewModel>(object context, params object[] args)
            where TView : UserControl, new()
            where TViewModel : class, IProjectDialogCreator;
    }
}
