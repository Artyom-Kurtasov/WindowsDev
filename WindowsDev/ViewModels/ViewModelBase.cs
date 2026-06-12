using System.ComponentModel;
using System.Runtime.CompilerServices;
using WindowsDev.Business.Services.Localization;

namespace WindowsDev.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool IsTabSelected;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected static string Translate(string key)
        {
            return new LanguageChanger().Translate(key);
        }
    }
}
