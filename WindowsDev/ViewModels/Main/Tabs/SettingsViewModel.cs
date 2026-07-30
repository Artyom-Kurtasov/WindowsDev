using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using WindowsDev.Application.DatabaseInterfaces;
using WindowsDev.Application.Services.Localization;
using WindowsDev.Command;
using WindowsDev.Domain.Common;
using WindowsDev.Domain.Common.DialogsMessages.Warnings;
using WindowsDev.Domain.Enums;
using WindowsDev.Settings.UserSettings;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class SettingsViewModel : LocalizedViewModelBase
    {
        private const string LightTheme = "Light.Blue";
        private const string DarkTheme = "Dark.Blue";

        private readonly IDbHealthChecker _healthChecker;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IDatabaseConfig _databaseConfig;

        public SettingsViewModel(
            IDbHealthChecker dbHealthChecker,
            IDialogCoordinator dialogCoordinator,
            ILanguageChanger languageChanger,
            IDatabaseConfig databaseConfig
        )
            : base(languageChanger)
        {
            _healthChecker = dbHealthChecker;
            _dialogCoordinator = dialogCoordinator;
            _databaseConfig = databaseConfig;

            ChangeThemeCommand = new AsyncRelayCommand(ChangeThemeAsync);
            SetNewConnectionStringCommand = new AsyncRelayCommand(SetNewConnectionStringAsync);
            ApplyLanguageCommand = new RelayCommand(ApplyLanguage);

            Initialize();
        }

        public ICommand ApplyLanguageCommand { get; }
        public ICommand ChangeThemeCommand { get; }
        public ICommand SetNewConnectionStringCommand { get; }

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

        public async Task SetNewConnectionStringAsync()
        {
            var oldConnection = _databaseConfig.ConnectionString;

            _databaseConfig.ConnectionString = NewConnectionString;

            try
            {
                _healthChecker.Check();

                UserSettings.Default.ConnectionString = NewConnectionString;
                UserSettings.Default.Save();
            }
            catch (Exception)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Warning),
                    Translate(SettingsWarnings.InvalidConnectionString),
                    MessageDialogStyle.Affirmative
                );

                _databaseConfig.ConnectionString = oldConnection;
            }
        }

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
}
