using System.Windows.Input;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Infrastructure;
using WindowsDev.Settings;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly DbManager _dbManager;
        private readonly LanguageChanger _languageChanger;

        private string _newConnectionString = string.Empty;
        /// <summary>
        /// Database connection string to be set by the user.
        /// </summary>
        public string NewConnectionString
        {
            get => _newConnectionString;
            set
            {
                _newConnectionString = value;
                OnPropertyChanged();
            }
        }

        private string _selectedLang = string.Empty;
        /// <summary>
        /// Currently selected language code for UI localization.
        /// </summary>
        public string SelectedLang
        {
            get => _selectedLang;
            set
            {
                _selectedLang = value;
                _languageChanger.ChangeLanguage(_selectedLang);

                UserSettings.Default.LanguageCode = _selectedLang;
                UserSettings.Default.Save();
            }
        }

        /// <summary>
        /// Command to set a new database connection string.
        /// </summary>
        public ICommand SetNewConnectionStringCommand { get; }

        public SettingsViewModel(LanguageChanger languageChanger, DbManager dbManager)
        {
            _languageChanger = languageChanger;
            _dbManager = dbManager;

            SetNewConnectionStringCommand = new AsyncRelayCommand(SetNewConnectionStringAsync);
        }

        /// <summary>
        /// Sets a new database connection string and saves it to user settings.
        /// </summary>
        public async Task SetNewConnectionStringAsync()
        {
            if (await _dbManager.SetConnection(NewConnectionString))
            {
                UserSettings.Default.ConnectionString = NewConnectionString;
                UserSettings.Default.Save();
            }
        }
    }
}
