namespace WindowsDev.ViewModels.Interfaces
{
    public interface IInitializableAsync
    {
        Task InitializationAsync(params object[] parameters);
    }   
}
