using AlpacaSpy.Models;
using ASCOM.Common.Interfaces;
using ILogger = ASCOM.Common.Interfaces.ILogger;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace AlpacaSpy
{
    public interface IAppLogger : ITraceLogger, ILogger, IDisposable
    {
        event EventHandler<MessageEventArgs>? MessageLogChanged;

        string LogFileName { get; }
        string LogFilePath { get; }

        void LogMessage(string method, LogLevel logLevel, string message, bool logToScreen = true);
        new void LogMessage(string method, string message);
        void LogDebug(string method, string message);
        void LogWarning(string method, string message);
        void LogError(string method, string message);
        void LogVerbose(string method, string message);
        void LogMessageConsole(string method, string message);
        void LogDebugConsole(string method, string message);
        void LogWarningConsole(string method, string message);
        void LogErrorConsole(string method, string message);
        void LogBlankLine();
        void LogWarning(string message);
        void LogError(string message);
        void ClearScreen();
        void Newlines(int count);
    }
}
