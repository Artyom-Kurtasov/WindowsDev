namespace WindowsDev.Factories
{
    public interface IViewModelFactory
    {
        T Create<T>(params object[] args);
    }
}



