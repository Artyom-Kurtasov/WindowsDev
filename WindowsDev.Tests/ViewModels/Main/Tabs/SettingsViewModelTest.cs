using MahApps.Metro.Controls.Dialogs;
using Moq;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Warnings;
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
        private readonly Mock<ILanguageChanger> _languageChangerMock;

        public SettingsViewModelTest()
        {
            _dbManagerMock = new Mock<IDbManager>();
            _dbManagerMock.SetupProperty(x => x.ConnectionString);

            _healthCheckerMock = new Mock<IDbHealthChecker>();
            _dialogMock = new Mock<IDialogCoordinator>();
            _languageChangerMock = new Mock<ILanguageChanger>();

            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);
        }

        private SettingsViewModel CreateViewModel()
        {
            return new SettingsViewModel(
                _healthCheckerMock.Object,
                _dbManagerMock.Object,
                _dialogMock.Object,
                _languageChangerMock.Object);
        }

        private void SetupWarningDialog()
        {
            _dialogMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<SettingsViewModel>(),
                    DialogTitles.Warning,
                    SettingsWarnings.InvalidConnectionString,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }

        [Fact]
        public async Task SetNewConnectionString_WhenValid_SavesConnection()
        {
            var vm = CreateViewModel();

            vm.NewConnectionString = "new_connection";

            await vm.SetNewConnectionStringAsync();

            Assert.Equal(
                "new_connection",
                _dbManagerMock.Object.ConnectionString);

            _healthCheckerMock.Verify(
                x => x.Check(),
                Times.Once);
        }

        [Fact]
        public async Task SetNewConnectionString_WhenInvalid_RestoresOldConnection()
        {
            SetupWarningDialog();

            _dbManagerMock.Object.ConnectionString = "old_connection";

            _healthCheckerMock
                .Setup(x => x.Check())
                .Throws(new Exception());

            var vm = CreateViewModel();

            vm.NewConnectionString = "bad_connection";

            await vm.SetNewConnectionStringAsync();

            Assert.Equal(
                "old_connection",
                _dbManagerMock.Object.ConnectionString);

            _dialogMock.Verify(
                x => x.ShowMessageAsync(
                    vm,
                    DialogTitles.Warning,
                    SettingsWarnings.InvalidConnectionString,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }

        [Fact]
        public async Task SetNewConnectionString_WhenValid_SavesUserSettings()
        {
            var vm = CreateViewModel();

            vm.NewConnectionString = "valid_connection";

            await vm.SetNewConnectionStringAsync();

            Assert.Equal(
                "valid_connection",
                UserSettings.Default.ConnectionString);
        }

        [Fact]
        public void SelectedLanguage_WhenChanged_ReturnsCorrectValue()
        {
            var vm = CreateViewModel();

            vm.SelectedLang = Language.en;

            Assert.Equal(
                Language.en,
                vm.SelectedLang);
        }

        [Fact]
        public void Constructor_LoadsSavedLanguage()
        {
            UserSettings.Default.LanguageCode = "ru";

            var vm = CreateViewModel();

            Assert.Equal(
                Language.ru,
                vm.SelectedLang);
        }

        [Fact]
        public void ApplyLanguage_SavesLanguage()
        {
            var vm = CreateViewModel();

            vm.SelectedLang = Language.ru;

            vm.ApplyLanguage();

            Assert.Equal(
                "ru",
                UserSettings.Default.LanguageCode);
        }

        [Fact]
        public void Constructor_LoadsDarkTheme()
        {
            UserSettings.Default.Theme = "Dark.Blue";

            var vm = CreateViewModel();

            Assert.Equal(
                0,
                vm.SelectedTheme);
        }

        [Fact]
        public void Constructor_LoadsLightTheme()
        {
            UserSettings.Default.Theme = "Light.Blue";

            var vm = CreateViewModel();

        Assert.Equal(
                1,
                vm.SelectedTheme);
        }

        [Fact]
        public void Languages_ReturnsAllLanguages()
        {
            var vm = CreateViewModel();

            Assert.NotEmpty(vm.Languages);

            Assert.Contains(
                Language.en,
                vm.Languages);
        }
    }
}