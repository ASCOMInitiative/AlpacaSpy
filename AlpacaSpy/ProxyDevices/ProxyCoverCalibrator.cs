using AlpacaSpy.Models;
using ASCOM;
using ASCOM.Alpaca.Clients;
using ASCOM.Common.DeviceInterfaces;

namespace AlpacaSpy.ProxyDevices
{
    public class ProxyCoverCalibrator : ICoverCalibratorV2
    {
        private readonly ConfiguredDevice _config;
        private readonly State _state;
        private readonly AlpacaSpyLogger _logger;
        private AlpacaCoverCalibrator? _client;

        private AlpacaCoverCalibrator Client => _client ?? throw new NotConnectedException($"Not connected to {_config.Name}");

        public ProxyCoverCalibrator(ConfiguredDevice config, State state, AlpacaSpyLogger logger)
        {
            _config = config;
            _state = state;
            _logger = logger;
        }

        internal void ConnectDevice()
        {
            if (_client?.Connected == true) return;
            DisconnectDevice();
            _client = new AlpacaCoverCalibrator(ASCOM.Common.Alpaca.ServiceType.Http, _config.IpAddress, _config.PortNumber, _config.RemoteDeviceNumber, false, null);
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
        public string Name => _config.Name + " (AlpacaSpy)";
        public IList<string> SupportedActions => Client.SupportedActions;
        public void Connect() => ConnectDevice();
        public void Disconnect() => DisconnectDevice();
        public bool Connecting => _client?.Connecting ?? false;
        public List<StateValue> DeviceState => Client.DeviceState;
        public void Dispose() => DisconnectDevice();

        public int Brightness => Client.Brightness;
        public bool CalibratorChanging => Client.CalibratorChanging;
        public CalibratorStatus CalibratorState => Client.CalibratorState;
        public bool CoverMoving => Client.CoverMoving;
        public CoverStatus CoverState => Client.CoverState;
        public int MaxBrightness => Client.MaxBrightness;

        public void CalibratorOff() => Client.CalibratorOff();
        public void CalibratorOn(int brightness) => Client.CalibratorOn(brightness);
        public void CloseCover() => Client.CloseCover();
        public void HaltCover() => Client.HaltCover();
        public void OpenCover() => Client.OpenCover();
    }
}
