using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsDev.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SubscribeToServiceProperty<T>(T service, string servicePropertyName, string viewModelPropertyName) 
            where T : INotifyPropertyChanged
        {
            service.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == servicePropertyName)
                {
                    OnPropertyChanged(viewModelPropertyName);
                }
            };
        }
    }
}
