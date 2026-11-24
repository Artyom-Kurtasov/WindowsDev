using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using WindowsDev.Application.Common.Events;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Command;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Information;
using WindowsDev.Factories;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Authorization;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.ViewModels.Main;

internal class MainWindowViewModel : LocalizedViewModelBase, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IDialogContextProvider _contextProvider;
    private readonly ISessionEvent _sessionEvent;
    private readonly IViewModelFactory _factory;
    private readonly NavigationStore _navigationStore;

    public MainWindowViewModel(
        NavigationStore navigationStore,
        IViewModelFactory factory,
        IDialogContextProvider dialogContextProvider,
        ISessionEvent sessionEvent,
        IDialogService dialogService,
        INavigationService navigationService,
        ILanguageChanger languageChanger
    ) : base(languageChanger)
    {
        _navigationStore = navigationStore;
        _factory = factory;
        _contextProvider = dialogContextProvider;
        _sessionEvent = sessionEvent;
        _dialogService = dialogService;
        _navigationService = navigationService;

        _sessionEvent.SessionExpired += OnSessionExpired;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        Projects = _factory.Create<ProjectsViewModel>();

        LoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
    }

    public ICommand LoadedCommand { get; }

    public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

    private ProjectsViewModel? _projects;
    public ProjectsViewModel? Projects
    {
        get => _projects;
        set
        {
            if (_projects == value)
                return;
            _projects = value;
            OnPropertyChanged();
        }
    }

    private SettingsViewModel? _settings;
    public SettingsViewModel? Settings
    {
        get => _settings;
        set
        {
            if (_settings == value)
                return;
            _settings = value;
            OnPropertyChanged();
        }
    }

    private ProfileViewModel? _profile;
    public ProfileViewModel? Profile
    {
        get => _profile;
        set
        {
            if (_profile == value)
                return;
            _profile = value;
            OnPropertyChanged();
        }
    }

    private bool _disposedValue;

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex == value)
                return;
            _selectedTabIndex = value;
            OnPropertyChanged(nameof(SelectedTabIndex));

            _ = LoadTab(_selectedTabIndex);
        }
    }

    private async Task LoadTab(int tabIndex)
    {
        switch (tabIndex)
        {
            case 1:
                {
                    if (_settings == null)
                        Settings = _factory.Create<SettingsViewModel>();
                    break;
                }
            case 2:
                {
                    if (_profile == null)
                        Profile = _factory.Create<ProfileViewModel>();

                    if (Profile is IRefreshableViewModel)
                        await Profile.RefreshAsync();
                    break;
                }
        }
    }

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Projects = null;
                Settings = null;
                Profile = null;
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private async Task OnLoadedAsync()
    {
        _contextProvider.Context = this;

        await LoadTab(SelectedTabIndex);
    }

    private void OnSessionExpired(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await _dialogService.ShowMessageAsync(
                Translate(DialogTitles.Information),
                Translate(MainWindowInformations.SessionExpiredMessage),
                MessageDialogStyle.Affirmative
            );

            await _navigationService.NavigateTo<AuthorizationViewModel>();
        });
    }
}
