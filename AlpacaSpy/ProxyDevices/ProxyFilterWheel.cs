using AlpacaSpy.Models;
using ASCOM;
using ASCOM.Alpaca.Clients;
using ASCOM.Common.DeviceInterfaces;

namespace AlpacaSpy.ProxyDevices
{
    public class ProxyFilterWheel : IFilterWheelV3, IDisposable
    {
        private readonly ConfiguredDevice _config;
        private readonly State _state;
        private readonly Settings _settings;
        private readonly AlpacaSpyLogger _logger;
        private AlpacaFilterWheel? _client;

        private AlpacaFilterWheel Client => _client ?? throw new NotConnectedException($"Not connected to {_config.Name}");

        public ProxyFilterWheel(ConfiguredDevice config, State state, Settings settings, AlpacaSpyLogger logger)
        {
            _config = config;
            _state = state;
            _settings = settings;
            _logger = logger;
        }

        internal void ConnectDevice()
        {
            if (_client?.Connected == true) return;
            DisconnectDevice();
            _client = new AlpacaFilterWheel(
                ASCOM.Common.Alpaca.ServiceType.Http,
                _config.IpAddress,
                _config.PortNumber,
                _config.RemoteDeviceNumber,
                false,
                _settings.LogLevel < ASCOM.Common.Interfaces.LogLevel.Information ? _logger : null);
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
        public string Name => Client.Name;
        public IList<string> SupportedActions => Client.SupportedActions;
        public void Connect() => ConnectDevice();
        public void Disconnect() => DisconnectDevice();
        public bool Connecting => _client?.Connecting ?? false;
        public List<StateValue> DeviceState => Client.DeviceState;
        public void Dispose() => DisconnectDevice();

        public int[] FocusOffsets => Client.FocusOffsets;
        public string[] Names => Client.Names;
        public short Position { get => Client.Position; set => Client.Position = value; }
    }
}
