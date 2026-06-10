using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.ViewModels;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Main.Tabs;
using Xunit;

namespace WindowsDev.Tests.ViewModels.Main
{
    public class MainWindowViewModelTest
    {
        private MainWindowViewModel CreateVm(NavigationStore store)
        {
            return new MainWindowViewModel(
                store,
                new ProjectsViewModel(
                    Mock.Of<IDialogCoordinator>(),
                    Mock.Of<IProjectService>(),
                    Mock.Of<INavigationService>(),
                    Mock.Of<ILogger<ProjectsViewModel>>(),
                    Mock.Of<IDialogService>()),

                new SettingsViewModel(
                    Mock.Of<IDbHealthChecker>(),
                    new LanguageChanger(),
                    Mock.Of<IDbManager>(),
                    Mock.Of<IDialogCoordinator>()));
        }

        [Fact]
        public void CurrentViewModel_ReturnsStoreValue()
        {
            var store = new NavigationStore();
            var vm = CreateVm(store);

            var expected = new ViewModelBase();

            store.CurrentViewModel = expected;

            Assert.Equal(expected, vm.CurrentViewModel);
        }

        [Fact]
        public void NavigationStoreChange_RaisesPropertyChanged()
        {
            var store = new NavigationStore();
            var vm = CreateVm(store);

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CurrentViewModel))
                    raised = true;
            };

            store.CurrentViewModel = new ViewModelBase();

            Assert.True(raised);
        }
    }
}