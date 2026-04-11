using WindowsDev.ViewModels;

namespace WindowsDev.Factories.Interfaces
{
    public interface IViewModelFactory
    {
        T Create<T>(params object[] args) where T : ViewModelBase;
    }
}


