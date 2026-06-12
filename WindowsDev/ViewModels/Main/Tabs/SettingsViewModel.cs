using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System.Windows.Input;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Domain.Enums;
using WindowsDev.Infrastructure;
using WindowsDev.Settings;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IDbManager _dbManager;
        private readonly LanguageChanger _languageChanger;
        private readonly IDbHealthChecker _healthChecker;

        public SettingsViewModel(
            IDbHealthChecker dbHealthChecker,
            LanguageChanger languageChanger,
            IDbManager dbManager,
            IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            _healthChecker = dbHealthChecker;
            _languageChanger = languageChanger;
            _dbManager = dbManager;

            ChangeThemeCommand = new AsyncRelayCommand(ChangeThemeAsync);
            SetNewConnectionStringCommand = new AsyncRelayCommand(SetNewConnectionStringAsync);
            ApplyLanguageCommand = new RelayCommand(ApplyLanguage);
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

                _ = ChangeThemeAsync();
            }
        }

        public IEnumerable<Language> Languages => Enum.GetValues<Language>().Cast<Language>();

        public void Initialize()
        {
            LoadSavedLanguages();
        }

        private void LoadSavedLanguages()
        {
            string savedLangCode = UserSettings.Default.LanguageCode;
            SelectedLang = Enum.Parse<Language>(savedLangCode, true);
        }

        public async Task SetNewConnectionStringAsync()
        {
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
                await _dialogCoordinator.ShowMessageAsync(this, Translate("Warning_Title"), Translate("Warning_InvalidConnectionString"), MessageDialogStyle.Affirmative);

                _dbManager.ConnectionString = old;
            }
        }

        public async Task ChangeThemeAsync()
        {
            const string lightTheme = "Light.Blue";
            const string darkTheme = "Dark.Blue";

            if (_selectedTheme == 0)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, lightTheme);
            }
            else if (_selectedTheme == 1)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, darkTheme);
            }
        }

        public void ApplyLanguage()
        {
            string langCode = SelectedLang.ToString().ToLower();

            _languageChanger.ChangeLanguage(langCode);

            UserSettings.Default.LanguageCode = langCode;
            UserSettings.Default.Save();
        }
    }
}
