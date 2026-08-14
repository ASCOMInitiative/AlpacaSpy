using Microsoft.Extensions.Logging;
using System;

namespace AlpacaSpy
{
    public sealed class ConsoleLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly LogLevel _minLevel;

        public ConsoleLogger(string categoryName, LogLevel minLevel = LogLevel.Information)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _minLevel = minLevel;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) 
        {
            if (!IsEnabled(logLevel))
                return;

            string message = formatter(state, exception);
            string levelString = GetLevelString(logLevel);

            lock (Globals.writeLogLock)
            {
                // Write the message to the console and color appropriately
                Console.Write($"{DateTime.Now:HH:mm:ss.fff} ");
                ConsoleColor originalColour = Console.ForegroundColor;

                // Select an appropriate colour for the log level
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case LogLevel.Information:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                Console.Write($"{levelString,-13} ");
                Console.ForegroundColor = originalColour;
                Console.WriteLine(message);

                if (exception != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception.ToString());
                }

                Console.ResetColor();
            }
        }

        private static string GetLevelString(LogLevel level) => level switch
        {
            LogLevel.Trace => "Trace",
            LogLevel.Debug => "Debug",
            LogLevel.Information => "Information",
            LogLevel.Warning => "Warning",
            LogLevel.Error => "Fail",
            LogLevel.Critical => "Critical",
            _ => "Unknown"
        };

        private static ConsoleColor GetLevelColor(LogLevel level) => level switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Information => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.Magenta,
            _ => ConsoleColor.White
        };

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
