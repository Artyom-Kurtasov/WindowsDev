using Microsoft.Extensions.Logging;

namespace WindowsDev.Business.Services.Logger
{
    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _filePath;
        private static readonly Lock _lock = new();

        public FileLogger(string categoryName, string filePath)
        {
            _categoryName = categoryName;
            _filePath = filePath;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string msg =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                $"[{logLevel}] " +
                $"[{_categoryName}] " +
                $"{formatter(state, exception)}";

            if (exception is not null)
            {
                msg += $"\n{exception}";
            }

            lock (_lock)
            {
                File.AppendAllText(
                    _filePath,
                    msg + Environment.NewLine);
            }
        }
    }
}