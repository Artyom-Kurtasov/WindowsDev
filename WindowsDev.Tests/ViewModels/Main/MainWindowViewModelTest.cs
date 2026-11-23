using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Moq;
using WindowsDev.Application.Common.Events;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Factories;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.Tests.ViewModels.Main.TestViewModels;
using WindowsDev.ViewModels.Authorization;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Main.Tabs;
using Xunit;

namespace WindowsDev.Tests.ViewModels.Main;

public class MainWindowViewModelTest
{
    private readonly Mock<IDialogContextProvider> _dialogContextProviderMock;
    private readonly Mock<IViewModelFactory> _factoryMock;
    private readonly Mock<ISessionEvent> _sessionEventMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;
    private readonly NavigationStore _navigationStore;

    public MainWindowViewModelTest()
    {
        _factoryMock = new Mock<IViewModelFactory>();
        _navigationStore = new NavigationStore();
        _dialogContextProviderMock = new Mock<IDialogContextProvider>();

        _sessionEventMock = new Mock<ISessionEvent>();
        _dialogServiceMock = new Mock<IDialogService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _languageChangerMock = new Mock<ILanguageChanger>();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);
    }

    private MainWindowViewModel CreateVm()
    {
        return new MainWindowViewModel(
            _navigationStore,
            _factoryMock.Object,
            _dialogContextProviderMock.Object,
            _sessionEventMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _languageChangerMock.Object);
    }

    [Fact]
    public void Constructor_WhenCalled_SubscribesToNavigationStoreChanges()
    {
        var vm = CreateVm();
        bool raised = false;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.CurrentViewModel))
                raised = true;
        };

        _navigationStore.CurrentViewModel = new TestSettingsViewModel();

        Assert.True(raised);
    }

    [Fact]
    public void CurrentViewModel_WhenNavigationStoreChanges_ReturnsUpdatedValue()
    {
        var vm = CreateVm();
        var expected = new TestSettingsViewModel();

        _navigationStore.CurrentViewModel = expected;

        Assert.Equal(expected, vm.CurrentViewModel);
    }

    [Fact]
    public void Constructor_CreatesProjectsViewModel()
    {
        var projects = new TestProjectsViewModel();

        _factoryMock.Setup(x => x.Create<ProjectsViewModel>()).Returns(projects);

        var vm = CreateVm();

        var result = vm.Projects;

        Assert.Same(projects, result);
        _factoryMock.Verify(x => x.Create<ProjectsViewModel>(), Times.Once);
    }

    [Fact]
    public void Projects_WhenAccessedMultipleTimes_ReturnsExistingInstance()
    {
        var projects = new TestProjectsViewModel();

        _factoryMock.Setup(x => x.Create<ProjectsViewModel>()).Returns(projects);

        var vm = CreateVm();

        var first = vm.Projects;
        var second = vm.Projects;

        Assert.Same(first, second);
        _factoryMock.Verify(x => x.Create<ProjectsViewModel>(), Times.Once);
    }

    [Fact]
    public void SelectedTabIndex_WhenSetTo1_CreatesSettingsViewModel()
    {
        var settings = new TestSettingsViewModel();

        _factoryMock.Setup(x => x.Create<SettingsViewModel>()).Returns(settings);

        var vm = CreateVm();

        vm.SelectedTabIndex = 1;

        Assert.Same(settings, vm.Settings);
        _factoryMock.Verify(x => x.Create<SettingsViewModel>(), Times.Once);
    }

    [Fact]
    public void SelectedTabIndex_WhenSetTo2_CreatesProfileViewModel()
    {
        var profile = new TestProfileViewModel();

        _factoryMock.Setup(x => x.Create<ProfileViewModel>()).Returns(profile);

        var vm = CreateVm();

        vm.SelectedTabIndex = 2;

        Assert.Same(profile, vm.Profile);
        _factoryMock.Verify(x => x.Create<ProfileViewModel>(), Times.Once);
    }

    [Fact]
    public void SelectedTabIndex_WhenChanged_RaisesPropertyChanged()
    {
        var vm = CreateVm();
        bool raised = false;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.SelectedTabIndex))
                raised = true;
        };

        vm.SelectedTabIndex = 1;

        Assert.True(raised);
    }

    [Fact]
    public void Settings_WhenAlreadyCreated_DoesNotCreateAgain()
    {
        var settings = new TestSettingsViewModel();

        _factoryMock.Setup(x => x.Create<SettingsViewModel>()).Returns(settings);

        var vm = CreateVm();

        vm.SelectedTabIndex = 1;
        var first = vm.Settings;

        vm.SelectedTabIndex = 0;
        vm.SelectedTabIndex = 1;
        var second = vm.Settings;

        Assert.Same(first, second);
        _factoryMock.Verify(x => x.Create<SettingsViewModel>(), Times.Once);
    }

    [Fact]
    public void Profile_WhenAlreadyCreated_DoesNotCreateAgain()
    {
        var profile = new TestProfileViewModel();

        _factoryMock.Setup(x => x.Create<ProfileViewModel>()).Returns(profile);

        var vm = CreateVm();

        vm.SelectedTabIndex = 2;
        var first = vm.Profile;

        vm.SelectedTabIndex = 0;
        vm.SelectedTabIndex = 2;
        var second = vm.Profile;

        Assert.Same(first, second);
        _factoryMock.Verify(x => x.Create<ProfileViewModel>(), Times.Once);
    }

    [Fact]
    public void Dispose_WhenCalled_ClearsAllViewModels()
    {
        var projects = new TestProjectsViewModel();
        var settings = new TestSettingsViewModel();
        var profile = new TestProfileViewModel();

        _factoryMock.Setup(x => x.Create<ProjectsViewModel>()).Returns(projects);
        _factoryMock.Setup(x => x.Create<SettingsViewModel>()).Returns(settings);
        _factoryMock.Setup(x => x.Create<ProfileViewModel>()).Returns(profile);

        var vm = CreateVm();

        vm.SelectedTabIndex = 1;
        vm.SelectedTabIndex = 2;

        Assert.NotNull(vm.Projects);
        Assert.NotNull(vm.Settings);
        Assert.NotNull(vm.Profile);

        vm.Dispose();

        Assert.Null(vm.Projects);
        Assert.Null(vm.Settings);
        Assert.Null(vm.Profile);
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        var vm = CreateVm();

        vm.Dispose();

        var exception = Record.Exception(() => vm.Dispose());

        Assert.Null(exception);
    }

}