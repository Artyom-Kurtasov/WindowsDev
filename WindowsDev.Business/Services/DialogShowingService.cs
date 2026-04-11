using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services
{
    /// <summary>
    /// Service responsible for showing custom dialogs using MahApps.Metro.
    /// </summary>
    public class DialogShowingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogCoordinator _dialogCoordinator;
        private CustomDialog? _dialog;

        public DialogShowingService(IServiceProvider serviceProvider, IDialogCoordinator dialogCoordinator)
        {
            _serviceProvider = serviceProvider;
            _dialogCoordinator = dialogCoordinator;
        }

        /// <summary>
        /// Shows a dialog with a specified view and ViewModel.
        /// Supports optional data editing if the ViewModel implements IProjectDialogCreator.
        /// </summary>
        public async Task ShowCreateDialogAsync<TView, TViewModel>(object context, object? data)
            where TView : UserControl, new()
            where TViewModel : class, IProjectDialogCreator
        {
            var view = new TView();

            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            if (data is TasksInfo task)
            {
                viewModel.SetEditDialog(task);
            }
            if (data is int projectId)
            {
                viewModel.SetProjectId(projectId);
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
                viewModel.Close -= closeHandler;
            };

            viewModel.Close += closeHandler;

            await _dialogCoordinator.ShowMetroDialogAsync(context, _dialog);
        }
    }
}

