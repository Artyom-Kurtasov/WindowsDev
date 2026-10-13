namespace WindowsDev.Application.Common.DebounceService;

public interface IDebounceService
{
    Task DebounceAsync(Func<Task> action, TimeSpan delay);
}