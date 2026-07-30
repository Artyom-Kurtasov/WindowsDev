using Microsoft.Extensions.Logging;

namespace WindowsDev.Logging.Common
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _path;

        public FileLoggerProvider(string path)
        {
            _path = path;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _path);
        }

        public void Dispose() { }
    }
}
