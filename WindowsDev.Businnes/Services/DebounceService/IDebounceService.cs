using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsDev.Business.Services.DebounceService
{
    public interface IDebounceService
    {
        Task DebounceAsync(Func<Task> action, TimeSpan delay);
    }
}
