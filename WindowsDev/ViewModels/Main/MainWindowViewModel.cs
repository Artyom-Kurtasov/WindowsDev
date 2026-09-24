using System.Windows.Input;
using WindowsDev.Command;
using WindowsDev.Factories;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.ViewModels.Main;

internal class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IDialogContextProvider _contextProvider;
    private readonly IViewModelFactory _factory;
    private readonly NavigationStore _navigationStore;

    public MainWindowViewModel(NavigationStore navigationStore, IViewModelFactory factory, IDialogContextProvider dialogContextProvider)
    {
        _navigationStore = navigationStore;
        _factory = factory;
        _contextProvider = dialogContextProvider;

        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

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
            case 0:
                {
                    if (_projects == null)
                    {
                        Projects = _factory.Create<ProjectsViewModel>();
                    }

                    if (Projects is IRefreshableViewModel)
                        await Projects.RefreshAsync();

                    break;
                }
            case 1:
                {
                    if (_settings == null)
                    {
                        Settings = _factory.Create<SettingsViewModel>();
                    }

                    break;
                }
            case 2:
                {
                    if (_profile == null)
                    {
                        Profile = _factory.Create<ProfileViewModel>();
                    }

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
}