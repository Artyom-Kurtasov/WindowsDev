using Moq;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Domain.Enums;
using WindowsDev.Settings.UserSettings;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.Tabs;

public class SettingsViewModelTest
{
    private readonly Mock<ILanguageChanger> _languageChangerMock;

    public SettingsViewModelTest()
    {
        _languageChangerMock = new Mock<ILanguageChanger>();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);

        _languageChangerMock
            .Setup(x => x.ChangeLanguage(It.IsAny<string>()))
            .Verifiable();
    }

    private SettingsViewModel CreateViewModel()
    {
        return new SettingsViewModel(
            _languageChangerMock.Object
        );
    }

    [Fact]
    public void SelectedLanguage_WhenChanged_ReturnsCorrectValue()
    {
        var vm = CreateViewModel();

        vm.SelectedLang = Language.en;

        Assert.Equal(Language.en, vm.SelectedLang);
    }

    [Fact]
    public void Constructor_LoadsSavedLanguage()
    {
        UserSettings.Default.LanguageCode = "ru";

        var vm = CreateViewModel();

        Assert.Equal(Language.ru, vm.SelectedLang);
    }

    [Fact]
    public void ApplyLanguageCommand_SavesLanguage()
    {
        var vm = CreateViewModel();

        vm.SelectedLang = Language.ru;

        vm.ApplyLanguage();

        Assert.Equal("ru", UserSettings.Default.LanguageCode);
    }

    [Fact]
    public void Constructor_LoadsDarkTheme()
    {
        UserSettings.Default.Theme = "Dark.Blue";

        var vm = CreateViewModel();

        Assert.Equal(0, vm.SelectedTheme);
    }

    [Fact]
    public void Constructor_LoadsLightTheme()
    {
        UserSettings.Default.Theme = "Light.Blue";

        var vm = CreateViewModel();

        Assert.Equal(1, vm.SelectedTheme);
    }

    [Fact]
    public void Languages_ReturnsAllLanguages()
    {
        var vm = CreateViewModel();

        Assert.NotEmpty(vm.Languages);

        Assert.Contains(Language.en, vm.Languages);
    }
}