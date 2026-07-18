using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels.Interfaces;

namespace WindowsDev.Dialogs
{
    public class DialogService : IDialogService
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IViewModelFactory _viewModelFactory;

        public DialogService(
            IDialogCoordinator dialogCoordinator,
            IViewModelFactory viewModelFactory
        )
        {
            _dialogCoordinator = dialogCoordinator;
            _viewModelFactory = viewModelFactory;
        }

        public async Task ShowDialogAsync<TView, TViewModel>(object context, params object[] args)
            where TView : UserControl, new()
            where TViewModel : class, IDialogViewModel
        {
            var view = new TView();
            var viewModel = _viewModelFactory.Create<TViewModel>(args);

            var dialog = new CustomDialog { Content = view };

            view.DataContext = viewModel;

            Func<Task>? completedHandler = null;
            Func<Task>? closeHandler = null;

            completedHandler = async () =>
            {
                if (context is IRefreshableViewModel refreshable)
                {
                    await refreshable.RefreshAsync();
                }

                viewModel.Completed -= completedHandler;
            };

            closeHandler = async () =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(context, dialog);
                viewModel.CloseRequested -= closeHandler;
            };

            viewModel.Completed += completedHandler;
            viewModel.CloseRequested += closeHandler;

            await _dialogCoordinator.ShowMetroDialogAsync(context, dialog);
        }

        public async Task ShowErrorDialogAsync(object context, string message, params object[] args)
        {
            await _dialogCoordinator.ShowMessageAsync(
                context,
                DialogTitles.Error,
                message,
                MessageDialogStyle.Affirmative
            );
        }
    }
}
