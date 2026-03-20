using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;

namespace WindowsDev.Businnes.Services
{
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

        public async Task ShowCreateProjectDialogAsync<TView, TViewModel>(object Context)
            where TView : UserControl, new()
            where TViewModel : class, IProjectDialogCreator
        {
            var view = new TView();
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            view.DataContext = viewModel;

            _dialog = new CustomDialog
            {
                Content = view
            };

            Func<Task>? handler = null;
            handler = async () =>
            {   
                await _dialogCoordinator.HideMetroDialogAsync(Context, _dialog);

                viewModel.Close -= handler;
            };

            viewModel.Close += handler;

            await _dialogCoordinator.ShowMetroDialogAsync(Context, _dialog);
        }
    }
}
