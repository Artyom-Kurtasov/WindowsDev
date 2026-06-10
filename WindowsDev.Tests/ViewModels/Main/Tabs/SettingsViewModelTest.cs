using MahApps.Metro.Controls.Dialogs;
using Moq;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Domain.Enums;
using WindowsDev.Settings;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.Tabs
{
    public class SettingsViewModelTest
    {
        private readonly Mock<IDbManager> _dbManagerMock;
        private readonly Mock<IDbHealthChecker> _healthCheckerMock;
        private readonly Mock<IDialogCoordinator> _dialogMock;
        private readonly LanguageChanger _languageChanger;

        public SettingsViewModelTest()
        {
            _dbManagerMock = new Mock<IDbManager>();
            _dbManagerMock.SetupProperty(x => x.ConnectionString);

            _healthCheckerMock = new Mock<IDbHealthChecker>();
            _dialogMock = new Mock<IDialogCoordinator>();

            _languageChanger = new LanguageChanger();
        }

        private SettingsViewModel CreateViewModel()
        {
            return new SettingsViewModel(
                _healthCheckerMock.Object,
                _languageChanger,
                _dbManagerMock.Object,
                _dialogMock.Object);
        }

        // Connection string

        [Fact]
        public async Task SetNewConnectionStringAsync_WhenValid_SavesConnection()
        {
            var vm = CreateViewModel();

            vm.NewConnectionString = "new_connection";

            _healthCheckerMock
                .Setup(x => x.Check());

            await vm.SetNewConnectionStringAsync();

            Assert.Equal("new_connection", _dbManagerMock.Object.ConnectionString);
        }

        [Fact]
        public async Task SetNewConnectionStringAsync_WhenHealthFails_RestoresOldConnection()
        {
            _dbManagerMock.Object.ConnectionString = "old";

            _healthCheckerMock
                .Setup(x => x.Check())
                .Throws(new Exception());

            var vm = CreateViewModel();

            vm.NewConnectionString = "bad_connection";

            await vm.SetNewConnectionStringAsync();

            Assert.Equal("old", _dbManagerMock.Object.ConnectionString);

            _dialogMock.Verify(x =>
                x.ShowMessageAsync(
                    It.IsAny<object>(),
                    "Warning",
                    "Nelza",
                    MessageDialogStyle.Affirmative),
                Times.Once);
        }

        // Language

        [Fact]
        public void SelectedLang_WhenChanged_ChangesValue()
        {
            var vm = CreateViewModel();

            vm.SelectedLang = Language.en;

            Assert.Equal(Language.en, vm.SelectedLang);
        }

        [Fact]
        public void LoadSavedLanguage_SetsFromSettings()
        {
            UserSettings.Default.LanguageCode = "ru";

            var vm = CreateViewModel();

            vm.Initialize();

            Assert.Equal(Language.ru, vm.SelectedLang);
        }
    }
}