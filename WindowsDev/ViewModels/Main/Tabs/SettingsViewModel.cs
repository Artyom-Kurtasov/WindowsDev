using System.Windows.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using WindowsDev.Application.Common.Utils.Localization;
using WindowsDev.Application.Database;
using WindowsDev.Command;
using WindowsDev.Domain.Enums;
using WindowsDev.Settings.UserSettings;

namespace WindowsDev.ViewModels.Main.Tabs;

internal class SettingsViewModel : LocalizedViewModelBase
{
    private const string LightTheme = "Light.Blue";
    private const string DarkTheme = "Dark.Blue";

    public SettingsViewModel(ILanguageChanger languageChanger)
        : base(languageChanger)
    {
        ChangeThemeCommand = new AsyncRelayCommand(ChangeThemeAsync);
        ApplyLanguageCommand = new RelayCommand(ApplyLanguage);

        Initialize();
    }

    public ICommand ApplyLanguageCommand { get; }
    public ICommand ChangeThemeCommand { get; }

    private string _newConnectionString = string.Empty;
    public string NewConnectionString
    {
        get => _newConnectionString;
        set
        {
            if (_newConnectionString == value)
                return;

            _newConnectionString = value;
            OnPropertyChanged(nameof(NewConnectionString));
        }
    }

    private Language _selectedLang;

    public Language SelectedLang
    {
        get => _selectedLang;
        set
        {
            if (_selectedLang == value)
                return;

            _selectedLang = value;
            OnPropertyChanged(nameof(SelectedLang));
        }
    }

    private double _selectedTheme;

    public double SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme == value)
                return;

            _selectedTheme = value;
            OnPropertyChanged(nameof(SelectedTheme));

            _ = ChangeThemeAsync();
        }
    }

    public IEnumerable<Language> Languages { get; } = Enum.GetValues<Language>();

    public async Task ChangeThemeAsync()
    {
        switch (_selectedTheme)
        {
            case 0:
                ThemeManager.Current.ChangeTheme(App.Current, DarkTheme);

                UserSettings.Default.Theme = DarkTheme;
                break;

            case 1:
                ThemeManager.Current.ChangeTheme(App.Current, LightTheme);

                UserSettings.Default.Theme = LightTheme;
                break;
        }

        UserSettings.Default.Save();

        await Task.CompletedTask;
    }

    public void ApplyLanguage()
    {
        var languageCode = SelectedLang.ToString().ToLower();

        LanguageChanger.ChangeLanguage(languageCode);

        UserSettings.Default.LanguageCode = languageCode;
        UserSettings.Default.Save();
    }

    private void Initialize()
    {
        LoadSavedLanguages();
        LoadSavedTheme();
    }

    private void LoadSavedTheme()
    {
        SelectedTheme = UserSettings.Default.Theme == DarkTheme ? 0 : 1;
    }

    private void LoadSavedLanguages()
    {
        SelectedLang = Enum.Parse<Language>(UserSettings.Default.LanguageCode, true);
    }
}
