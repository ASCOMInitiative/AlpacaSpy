using AlpacaSpy.Models;
using ASCOM.Common;
using System.Numerics;

namespace AlpacaSpy
{
    internal static class Globals
    {
        internal const string APPLICATION_SHORT_NAME = "AlpacaSpy";
        internal const string APPLICATION_NAME = "ASCOM AlpacaSpy";
        internal const int MESSAGE_LEVEL_WIDTH = 8;
        internal const int TEST_NAME_WIDTH = 35;
        internal const string APPLICATION_FOLDER_NAME = @"ASCOM\AlpacaSpy";
        internal const string SETTINGS_FILENAME = "alpacaspy.settings.json";
        internal const string LOG_FILENAME = "alpacaspy.log";
        internal const string WELCOME_MESSAGE = $"Welcome to {APPLICATION_NAME}!";
        internal const int DEFAULT_ALPACA_PORT = 32325;
        internal const int MAXIMUM_LOG_SIZE_CHARACTERS = 120000;
        internal const int LOG_TRUNCATION_CHARACTERS = 12000;
        internal const string DISCOVERY_PACKET_MESSAGE = "Received a discovery packet from";

        // Shutdown is usually initiated from a live Blazor Server circuit, so keep the host and WebSocket graceful-close windows short to avoid an unnecessary pause before exit.
        internal const int APPLICATION_SHUTDOWN_TIMEOUT = 1;
        internal const int WEBSOCKET_CLOSE_TIMEOUT = 1;
        internal const int RESTART_DELAY = 1;

        internal const int DISCONNECTED_CIRCUIT_RETENTION_PERIOD = 180;
        internal const int LOG_REFRESH_INTERVAL = 250;
        internal const int MAX_CONFIGURED_DEVICES = 10;
        internal const double ALPACA_DISCOVERY_DURATION_SECONDS = 1.0;

        internal const int MAXIMUM_RECORDING_FILE_ENTRIES = 100000;

        internal const string IMAGEBYTES_PROXY_RESPONSE = @"{""Value"": ""ImageArray response not recorded or checked due to length and JSON/ImageBytes complexity."", ""ClientTransactionID"": 1, ""ServerTransactionID"": 1, ""ErrorNumber"": 0, ""ErrorMessage"": """"}";

        // The number of columns to display for each device type in the setup window's property list.
        internal static readonly Dictionary<AlpacaDeviceType, int> DevicetypeDisplayColumns = new()
        {
            { AlpacaDeviceType.Camera, 5 },
            { AlpacaDeviceType.CoverCalibrator, 3 },
            { AlpacaDeviceType.Dome, 3 },
            { AlpacaDeviceType.FilterWheel, 3 },
            { AlpacaDeviceType.Focuser, 3 },
            { AlpacaDeviceType.ObservingConditions, 3 },
            { AlpacaDeviceType.Rotator, 3 },
            { AlpacaDeviceType.SafetyMonitor, 3 },
            { AlpacaDeviceType.Switch, 3 },
            { AlpacaDeviceType.Telescope, 5 }
        };

        internal static readonly Lock writeLogLock = new();
        internal static readonly SemaphoreSlim ConnectSemaphore = new(1, 1);
        internal static Lock StateLock = new();
    }
}
