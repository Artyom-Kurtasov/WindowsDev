using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Tasks;

namespace WindowsDev.Dialogs
{
    public class DialogService : IDialogService
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IDialogCoordinator _dialogCoordinator;

        public DialogService(
            IDialogCoordinator dialogCoordinator,
            IViewModelFactory viewModelFactory)
        {
            _dialogCoordinator = dialogCoordinator;
            _viewModelFactory = viewModelFactory;
        }

        public async Task ShowTaskDialogAsync<TView, TViewModel>(object context, params object[] args)
            where TView : UserControl, new()
            where TViewModel : class, IProjectDialogCreator
        {
            var view = new TView();

            var viewModel = _viewModelFactory.Create<TViewModel>();

            // Initialize ViewModel if it supports async initialization
            if (viewModel is IInitializableAsync init)
            {
                await init.InitializationAsync(args);
            }

            // Handle completion event (refresh parent context)
            if (viewModel is IProjectDialogCreator vm)
            {
                Func<Task>? completedHandler = null;

                completedHandler = async () =>
                {
                    if (context is TaskViewModel taskVm)
                    {
                        taskVm.RefreshTask();
                    }
                    else if (context is IInitializableAsync parent)
                    {
                        await parent.InitializationAsync();
                    }

                    vm.Completed -= completedHandler;
                };

                vm.Completed += completedHandler;
            }

            view.DataContext = viewModel;

            // Wrap view into MahApps dialog container
            CustomDialog dialog = new CustomDialog
            {
                Content = view
            };

            // Handle request to close dialog from ViewModel
            Func<Task>? closeHandler = null;
            closeHandler = async () =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(context, dialog);
                viewModel.CloseRequested -= closeHandler;
            };

            viewModel.CloseRequested += closeHandler;

            // Show dialog
            await _dialogCoordinator.ShowMetroDialogAsync(context, dialog);
        }
    }
}