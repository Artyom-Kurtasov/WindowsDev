using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;
using WindowsDev.Domain.Messages;
using WindowsDev.Factories;
using WindowsDev.ViewModels.Interfaces;

namespace WindowsDev.Services.Dialogs;

internal class DialogService : IDialogService
{
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IDialogContextProvider _contextProvider;

    public DialogService(
        IDialogCoordinator dialogCoordinator,
        IViewModelFactory viewModelFactory,
        IDialogContextProvider contextProvider
    )
    {
        _dialogCoordinator = dialogCoordinator;
        _viewModelFactory = viewModelFactory;
        _contextProvider = contextProvider;
    }

    public async Task ShowDialogAsync<TView, TViewModel>(params object[] args)
        where TView : UserControl, new()
        where TViewModel : class, IDialogViewModel
    {
        var view = new TView();
        var viewModel = _viewModelFactory.Create<TViewModel>(args);
        var context = _contextProvider.Context;

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

    public async Task ShowErrorDialogAsync(string message, params object[] args)
    {
        await _dialogCoordinator.ShowMessageAsync(
            _contextProvider.Context,
            DialogTitles.Error,
            message,
            MessageDialogStyle.Affirmative
        );
    }

    public async Task ShowMessageAsync(string title, string message, MessageDialogStyle style)
    {
        await _dialogCoordinator.ShowMessageAsync(
            _contextProvider.Context,
            title,
            message,
            style
            );
    }
}