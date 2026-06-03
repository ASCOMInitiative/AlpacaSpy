using AlpacaSpy.Models;
using ASCOM;
using ASCOM.Alpaca.Clients;
using ASCOM.Common.DeviceInterfaces;

namespace AlpacaSpy.ProxyDevices
{
    public class ProxySwitch : ISwitchV3
    {
        private readonly ConfiguredDevice _config;
        private readonly State _state;
        private readonly Settings _settings;
        private readonly AlpacaSpyLogger _logger;
        private AlpacaSwitch? _client;

        private AlpacaSwitch Client => _client ?? throw new NotConnectedException($"Not connected to {_config.Name}");

        public ProxySwitch(ConfiguredDevice config, State state, Settings settings, AlpacaSpyLogger logger)
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
            _client = new AlpacaSwitch(
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
        public List<StateValue> DeviceState => throw new ASCOM.NotImplementedException("DeviceState not supported for Switch");
        public void Dispose() => DisconnectDevice();

        public short MaxSwitch => Client.MaxSwitch;

        public bool CanAsync(short id) => Client.CanAsync(id);
        public void CancelAsync(short id) => Client.CancelAsync(id);
        public bool CanWrite(short id) => Client.CanWrite(id);
        public bool GetSwitch(short id) => Client.GetSwitch(id);
        public string GetSwitchDescription(short id) => Client.GetSwitchDescription(id);
        public string GetSwitchName(short id) => Client.GetSwitchName(id);
        public double GetSwitchValue(short id) => Client.GetSwitchValue(id);
        public double MaxSwitchValue(short id) => Client.MaxSwitchValue(id);
        public double MinSwitchValue(short id) => Client.MinSwitchValue(id);
        public void SetAsync(short id, bool state) => Client.SetAsync(id, state);
        public void SetAsyncValue(short id, double value) => Client.SetAsyncValue(id, value);
        public void SetSwitch(short id, bool state) => Client.SetSwitch(id, state);
        public void SetSwitchName(short id, string name) => Client.SetSwitchName(id, name);
        public void SetSwitchValue(short id, double value) => Client.SetSwitchValue(id, value);
        public bool StateChangeComplete(short id) => Client.StateChangeComplete(id);
        public double SwitchStep(short id) => Client.SwitchStep(id);
    }
}
