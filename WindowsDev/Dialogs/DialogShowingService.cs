using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels.Interfaces;

namespace WindowsDev.Dialogs
{
    /// <summary>
    /// Service responsible for showing custom dialogs using MahApps.Metro.
    /// </summary>
    public class DialogShowingService
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IDialogCoordinator _dialogCoordinator;
        private CustomDialog? _dialog;

        public DialogShowingService(IDialogCoordinator dialogCoordinator,
            IViewModelFactory viewModelFactory)
        {
            _dialogCoordinator = dialogCoordinator;
            _viewModelFactory = viewModelFactory;
        }

        /// <summary>
        /// Shows a dialog with a specified view and ViewModel.
        /// Supports optional data editing if the ViewModel implements IProjectDialogCreator.
        /// </summary>
        public async Task ShowTaskDialogAsync<TView, TViewModel>(object context, params object[] args)
            where TView : UserControl, new()
            where TViewModel : class, IProjectDialogCreator
        {
            var view = new TView();

            var viewModel = _viewModelFactory.Create<TViewModel>();

            if (viewModel is IInitializableAsync init)
            {
                await init.InitializationAsync(args);
            }

            view.DataContext = viewModel;   

            _dialog = new CustomDialog
            {
                Content = view
            };

            Func<Task>? closeHandler = null;
            closeHandler = async () =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(context, _dialog);
                viewModel.CloseRequested -= closeHandler;
            };

            viewModel.CloseRequested += closeHandler;

            await _dialogCoordinator.ShowMetroDialogAsync(context, _dialog);
        }
    }
}

