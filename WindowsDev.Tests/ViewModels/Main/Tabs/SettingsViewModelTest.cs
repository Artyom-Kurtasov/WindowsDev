using MahApps.Metro.Controls.Dialogs;
using Moq;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization.Interfaces;
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
        private readonly Mock<ILanguageChanger> _languageChanger;

        public SettingsViewModelTest()
        {
            _dbManagerMock = new Mock<IDbManager>();
            _dbManagerMock.SetupProperty(x => x.ConnectionString);

            _healthCheckerMock = new Mock<IDbHealthChecker>();
            _dialogMock = new Mock<IDialogCoordinator>();

            _languageChanger = new Mock<ILanguageChanger>();
        }

        private SettingsViewModel CreateViewModel()
        {
            return new SettingsViewModel(
                _healthCheckerMock.Object,
                _dbManagerMock.Object,
                _dialogMock.Object,
                _languageChanger.Object);
        }

        [Fact]
        public async Task SetNewConnectionStringAsync_WhenValid_SavesConnection()
        {
            var vm = CreateViewModel();
            vm.NewConnectionString = "new_connection";

            _healthCheckerMock
                .Setup(x => x.Check())
                .Verifiable();

            await vm.SetNewConnectionStringAsync();

            Assert.Equal("new_connection", _dbManagerMock.Object.ConnectionString);
            _healthCheckerMock.Verify(x => x.Check(), Times.Once);
        }

        [Fact]
        public async Task SetNewConnectionStringAsync_WhenHealthFails_RestoresOldConnection()
        {
            string oldConnection = "old_connection";
            _dbManagerMock.Object.ConnectionString = oldConnection;

            _healthCheckerMock
                .Setup(x => x.Check())
                .Throws(new Exception("Database connection failed"));

            var vm = CreateViewModel();
            vm.NewConnectionString = "bad_connection";

            await vm.SetNewConnectionStringAsync();

            Assert.Equal(oldConnection, _dbManagerMock.Object.ConnectionString);

            _dialogMock.Verify(x =>
                x.ShowMessageAsync(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }

        [Fact]
        public async Task SetNewConnectionStringAsync_WhenValid_SavesToUserSettings()
        {
            var vm = CreateViewModel();
            vm.NewConnectionString = "valid_connection";

            _healthCheckerMock.Setup(x => x.Check());

            await vm.SetNewConnectionStringAsync();

            Assert.Equal("valid_connection", UserSettings.Default.ConnectionString);
        }

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

        [Fact]
        public void ApplyLanguage_ChangesLanguageAndSavesToSettings()
        {
            var vm = CreateViewModel();
            vm.SelectedLang = Language.ru;

            vm.ApplyLanguage();

            Assert.Equal("ru", UserSettings.Default.LanguageCode);
        }

        [Fact]
        public void LoadSavedTheme_SetsDarkThemeCorrectly()
        {
            UserSettings.Default.Theme = "Dark.Blue";
            var vm = CreateViewModel();

            vm.Initialize();

            Assert.Equal(0, vm.SelectedTheme);
        }

        [Fact]
        public void LoadSavedTheme_SetsLightThemeCorrectly()
        {
            UserSettings.Default.Theme = "Light.Blue";
            var vm = CreateViewModel();

            vm.Initialize();

            Assert.Equal(1, vm.SelectedTheme);
        }
    }
}