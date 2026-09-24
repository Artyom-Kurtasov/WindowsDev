namespace WindowsDev.Application.Common.Utils.DebounceService;

public interface IDebounceService
{
    Task DebounceAsync(Func<Task> action, TimeSpan delay);
}