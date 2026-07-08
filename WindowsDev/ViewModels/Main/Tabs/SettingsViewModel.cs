using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System.Windows.Input;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Warnings;
using WindowsDev.Domain.Enums;
using WindowsDev.Infrastructure;
using WindowsDev.Settings;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IDbHealthChecker _healthChecker;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IDbManager _dbManager;
        private readonly ILanguageChanger _languageChanger;

        // SelectedTheme: 0 = Dakr, 1 = Light 
        const string lightTheme = "Light.Blue";
        const string darkTheme = "Dark.Blue";

        public SettingsViewModel(IDbHealthChecker dbHealthChecker,
            IDbManager dbManager,
            IDialogCoordinator dialogCoordinator,
            ILanguageChanger languageChanger)
        {
            _dialogCoordinator = dialogCoordinator;
            _healthChecker = dbHealthChecker;
            _languageChanger = languageChanger;
            _dbManager = dbManager;

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
                _selectedTheme = value;
                OnPropertyChanged(nameof(SelectedTheme));

                // Apply theme immediately when selection changes
                _ = ChangeThemeAsync();
            }
        }

        public IEnumerable<Language> Languages => Enum.GetValues<Language>().Cast<Language>();

        public void Initialize()
        {
            LoadSavedLanguages();
            LoadSavedTheme();
        }

        private void LoadSavedTheme()
        {
            string savedTheme = UserSettings.Default.Theme;

            if (savedTheme == darkTheme)
            {
                _selectedTheme = 0;
            }
            else if (savedTheme == lightTheme)
            {
                _selectedTheme = 1;
            }
        }

        private void LoadSavedLanguages()
        {
            string savedLangCode = UserSettings.Default.LanguageCode;

            SelectedLang = Enum.Parse<Language>(savedLangCode, true);
        }

        public async Task SetNewConnectionStringAsync()
        {
            // Save current connection before attempting to change,
            // so we can roll back if the new one is invalid
            var old = _dbManager.ConnectionString;

            _dbManager.ConnectionString = NewConnectionString;

            try
            {
                _healthChecker.Check();

                UserSettings.Default.ConnectionString = NewConnectionString;
                UserSettings.Default.Save();
            }
            catch
            {
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Warning),
                    Translate(SettingsWarnings.InvalidConnectionString),
                    MessageDialogStyle.Affirmative);

                // Roll back to the working connection string
                _dbManager.ConnectionString = old;
            }
        }

        public async Task ChangeThemeAsync()
        {
            if (_selectedTheme == 0)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, darkTheme);
                UserSettings.Default.Theme = darkTheme;
            }
            else if (_selectedTheme == 1)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, lightTheme);
                UserSettings.Default.Theme = lightTheme;
            }

            UserSettings.Default.Save();
        }

        public void ApplyLanguage()
        {
            // LanguageChanger expects lowercase code
            string langCode = SelectedLang.ToString().ToLower();

            _languageChanger.ChangeLanguage(langCode);

            UserSettings.Default.LanguageCode = langCode;
            UserSettings.Default.Save();
        }
    }
}
