using AlpacaSpy.Models;
using ASCOM.Tools;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AlpacaSpy
{
    public class State
    {
        private static uint serverTransactionId;

        public State()
        {
            ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Not set";
            ApplicationFileversion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Not set";
            InformationalVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Not set";
            InstanceId = RandomHex10Secure();
        }

        public string InstanceId { get; set; }

        public string ApplicationVersion { get; set; } = "Not set";

        public string ApplicationFileversion { get; set; } = "Not set";

        public string InformationalVersion { get; set; } = "Not set";

        public bool Connected { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(Connected)); } } } = false;

        public bool DisplayRestartMessage { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(DisplayRestartMessage)); } } } = false;

        public string StatusText { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(StatusText)); } } } = string.Empty;

        public bool OperationUnderway { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(OperationUnderway)); } } } = false;

        public string OperationUnderwayMessage { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(OperationUnderwayMessage)); } } } = "Operation Underway";

        public bool ConnectingToDevices { get; set; } = false;

        public StringBuilder ApplicationLog { get; set; } = new StringBuilder(Globals.MAXIMUM_LOG_SIZE_CHARACTERS, Globals.MAXIMUM_LOG_SIZE_CHARACTERS).Append($"{Globals.WELCOME_MESSAGE}\r\n");

        public List<ConfiguredDevice> ConfiguredDevices { get; set; } = new();

        public Dictionary<ConfiguredDevice, FixedCapacityList<AlpacaTransaction>> Transactions { get; set; } = new(Globals.MAXIMUM_RECORDING_FILE_ENTRIES);

        public List<object> ProxyDevices { get; set; } = new();

        /// <summary>Per-device TraceLogger instances keyed by ConfiguredDevice.UniqueId.</summary>
        public Dictionary<string, TraceLogger> DeviceLoggers { get; set; } = new();

        public List<ASCOM.Alpaca.Discovery.AscomDevice> DiscoveredDevices { get; set; } = new();

        public Lock DiscoveredDevicesLock { get; } = new();

        public bool DiscoveryHasRun { get; set; } = false;

        public uint GetServerTransactionId()
        {
            return Interlocked.Increment(ref serverTransactionId);
        }

        public void ResetState()
        {
            Connected = false;
            DisplayRestartMessage = false;
            StatusText = string.Empty;
            OperationUnderway = false;
            OperationUnderwayMessage = "Operation Underway";
            ConnectingToDevices = false;
            ApplicationLog = new StringBuilder(Globals.MAXIMUM_LOG_SIZE_CHARACTERS, Globals.MAXIMUM_LOG_SIZE_CHARACTERS).Append($"{Globals.WELCOME_MESSAGE}\r\n");
            ConfiguredDevices = new List<ConfiguredDevice>();
            ProxyDevices = new List<object>();
            foreach (TraceLogger tl in DeviceLoggers.Values) try { tl.Dispose(); } catch { }
            foreach (IDisposable device in ProxyDevices) try { device.Dispose(); } catch { }
            DeviceLoggers = new Dictionary<string, TraceLogger>();
            lock (DiscoveredDevicesLock)
            {
                DiscoveredDevices = new List<ASCOM.Alpaca.Discovery.AscomDevice>();
            }
            DiscoveryHasRun = false;
        }

        public void RaiseChangeEvent(string memberName)
        {
            try
            {
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler? StateChanged;

        private static string RandomHex10Secure()
        {
            byte[] bytes = new byte[5];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
