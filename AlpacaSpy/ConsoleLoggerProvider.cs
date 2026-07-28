using System.Collections.Concurrent;

namespace AlpacaSpy
{
    public sealed class ConsoleLoggerProvider:ILoggerProvider
    {
        private readonly LogLevel _minLevel;
        private readonly ConcurrentDictionary<string, ConsoleLogger> _loggers = new();

        public ConsoleLoggerProvider(LogLevel minLevel = LogLevel.Debug)
        {
            _minLevel = minLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new ConsoleLogger(name, _minLevel));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
