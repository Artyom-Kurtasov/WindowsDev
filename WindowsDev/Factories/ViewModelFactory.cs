using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels;

namespace WindowsDev.Factories
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IServiceProvider _provider;

        public ViewModelFactory(IServiceProvider provider)
        {
            _provider = provider;
        }
        public T Create<T>(params object[] args) where T : ViewModelBase
        {
            return ActivatorUtilities.CreateInstance<T>(_provider, args);
        }
    }
}

