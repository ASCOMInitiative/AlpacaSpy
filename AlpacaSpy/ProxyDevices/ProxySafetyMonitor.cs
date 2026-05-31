using AlpacaSpy.Models;
using ASCOM;
using ASCOM.Alpaca.Clients;
using ASCOM.Common.DeviceInterfaces;

namespace AlpacaSpy.ProxyDevices
{
    public class ProxySafetyMonitor : ISafetyMonitorV3
    {
        private readonly ConfiguredDevice _config;
        private readonly State _state;
        private readonly AlpacaSpyLogger _logger;
        private AlpacaSafetyMonitor? _client;

        private AlpacaSafetyMonitor Client => _client ?? throw new NotConnectedException($"Not connected to {_config.Name}");

        public ProxySafetyMonitor(ConfiguredDevice config, State state, AlpacaSpyLogger logger)
        {
            _config = config;
            _state = state;
            _logger = logger;
        }

        internal void ConnectDevice()
        {
            if (_client?.Connected == true) return;
            DisconnectDevice();
            _client = new AlpacaSafetyMonitor(ASCOM.Common.Alpaca.ServiceType.Http, _config.IpAddress, _config.PortNumber, _config.RemoteDeviceNumber, false, null);
            _client.Connected = true;
        }

        internal void DisconnectDevice()
        {
            try { _client?.Connected = false; } catch { }
            try { _client?.Dispose(); } catch { }
            _client = null;
        }

        public string Action(string actionName, string actionParameters) => Client.Action(actionName, actionParameters);
        public void CommandBlind(string command, bool raw = false) => Client.CommandBlind(command, raw);
        public bool CommandBool(string command, bool raw = false) => Client.CommandBool(command, raw);
        public string CommandString(string command, bool raw = false) => Client.CommandString(command, raw);
        public bool Connected { get => _client?.Connected ?? false; set { if (value) ConnectDevice(); else DisconnectDevice(); } }
        public string Description => Client.Description;
        public string DriverInfo => Client.DriverInfo;
        public string DriverVersion => Client.DriverVersion;
        public short InterfaceVersion => Client.InterfaceVersion;
        public string Name => "AlpacaSpy - " + _config.Name;
        public IList<string> SupportedActions => Client.SupportedActions;
        public void Connect() => ConnectDevice();
        public void Disconnect() => DisconnectDevice();
        public bool Connecting => _client?.Connecting ?? false;
        public List<StateValue> DeviceState => Client.DeviceState;
        public void Dispose() => DisconnectDevice();

        public bool IsSafe => Client.IsSafe;
    }
}
