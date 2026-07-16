using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsDev.Business.Services.DebounceService
{
    public class DebounceService : IDebounceService
    {
        private CancellationTokenSource? _cts;
        public async Task DebounceAsync(Func<Task> action, TimeSpan delay)
        {
            _cts?.Cancel();

            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(delay, _cts.Token);
                await action();

            }
            catch (TaskCanceledException)
            {
                // Ignore the exception if the task was canceled
            }
        }
    }
}
