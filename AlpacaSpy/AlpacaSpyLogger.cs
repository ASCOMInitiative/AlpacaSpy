using ASCOM.Common;
using ASCOM.Common.Interfaces;
using ASCOM.Tools;
using ILogger = ASCOM.Common.Interfaces.ILogger;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace AlpacaSpy
{
    public class AlpacaSpyLogger : TraceLogger, ITraceLogger, ILogger
    {
        private readonly State state;
        private readonly Settings settings;

        public AlpacaSpyLogger(State state, Settings settings) : base("AlpacaSpy", true)
        {
            this.state = state;
            this.settings = settings;
            SetMinimumLoggingLevel(settings.LogLevel);
        }

        public AlpacaSpyLogger(string logFileName, State state, Settings settings) : base("AlpacaSpy", true)
        {
            this.state = state;
            this.settings = settings;
            SetMinimumLoggingLevel(settings.LogLevel);
        }

        public event EventHandler<MessageEventArgs>? MessageLogChanged;

        void ILogger.Log(LogLevel level, string message)
        {
            LogMessage(string.Empty, level, message);
        }

        public void LogMessage(string method, LogLevel logLevel, string message, bool logToScreen = true)
        {
            try
            {
                message ??= string.Empty;

                if (message.Contains(Globals.DISCOVERY_PACKET_MESSAGE, StringComparison.OrdinalIgnoreCase) && !settings.LogDiscoveryMessages)
                    return;

                if (logLevel >= settings.LogLevel)
                {
                    lock (Globals.writeLogLock)
                    {
                        string formattedMessage = $"{DateTime.Now:HH:mm:ss.fff} {logLevel,-13} {message}";
                        Console.Write($"{DateTime.Now:HH:mm:ss.fff} ");
                        ConsoleColor originalColour = Console.ForegroundColor;

                        switch (logLevel)
                        {
                            case LogLevel.Verbose:
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                break;
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

                        Console.Write($"{logLevel,-13} ");
                        Console.ForegroundColor = originalColour;
                        Console.WriteLine(message);

                        base.LogMessage(method, message);

                        if (logToScreen)
                        {
                            try
                            {
                                state.ApplicationLog.Append($"\r\n{formattedMessage}");
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                int originalLength = state.ApplicationLog.Length;
                                state.ApplicationLog.Remove(0, Globals.LOG_TRUNCATION_CHARACTERS);
                                int newLength = state.ApplicationLog.Length;
                                state.ApplicationLog.Insert(0, $"\r\n**** {ex.Message} Log truncated at {DateTime.Now:HH:mm:ss.fff} original length: {originalLength}, new length: {newLength} ****\r\n");
                                state.ApplicationLog.Append($"{formattedMessage}");
                            }

                            OnMessageLogChanged(formattedMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger.LogMessage Exception: {ex.Message}\r\n{ex}");
            }
        }

        public new void LogMessage(string method, string message) => LogMessage(method, LogLevel.Information, message);
        public void LogDebug(string method, string message) => LogMessage(method, LogLevel.Debug, message);
        public void LogWarning(string method, string message) => LogMessage(method, LogLevel.Warning, message);
        public void LogError(string method, string message) => LogMessage(method, LogLevel.Error, message);
        public void LogVerbose(string method, string message) => LogMessage(method, LogLevel.Verbose, message);
        public void LogMessageConsole(string method, string message) => LogMessage(method, LogLevel.Information, message, logToScreen: false);
        public void LogDebugConsole(string method, string message) => LogMessage(method, LogLevel.Debug, message, logToScreen: false);
        public void LogWarningConsole(string method, string message) => LogMessage(method, LogLevel.Warning, message, logToScreen: false);
        public void LogErrorConsole(string method, string message) => LogMessage(method, LogLevel.Error, message, logToScreen: false);
        public void LogBlankLine() => LogMessage(string.Empty, string.Empty);
        public new void BlankLine() => LogBlankLine();
        public void LogWarning(string message) => LogWarning(string.Empty, message);
        public void LogError(string message) => LogError(string.Empty, message);

        private void OnMessageLogChanged(string message)
        {
            MessageEventArgs eventArgs = new()
            {
                Message = $"{DateTime.Now:HH:mm:ss.fff} {message}"
            };

            MessageLogChanged?.Invoke(this, eventArgs);
        }
    }
}
