namespace WindowsDev.Business.Services.DebounceService
{
    public interface IDebounceService
    {
        Task DebounceAsync(Func<Task> action, TimeSpan delay);
    }
}
