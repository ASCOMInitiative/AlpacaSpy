using AlpacaSpy.Models;
using ASCOM;
using ASCOM.Alpaca.Clients;
using ASCOM.Common.DeviceInterfaces;

namespace AlpacaSpy.ProxyDevices
{
    public class ProxyObservingConditions : IObservingConditionsV2
    {
        private readonly ConfiguredDevice _config;
        private readonly State _state;
        private readonly AlpacaSpyLogger _logger;
        private AlpacaObservingConditions? _client;

        private AlpacaObservingConditions Client => _client ?? throw new NotConnectedException($"Not connected to {_config.Name}");

        public ProxyObservingConditions(ConfiguredDevice config, State state, AlpacaSpyLogger logger)
        {
            _config = config;
            _state = state;
            _logger = logger;
        }

        internal void ConnectDevice()
        {
            if (_client?.Connected == true) return;
            DisconnectDevice();
            _client = new AlpacaObservingConditions(ASCOM.Common.Alpaca.ServiceType.Http, _config.IpAddress, _config.PortNumber, _config.RemoteDeviceNumber, false, null);
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

        public double AveragePeriod { get => Client.AveragePeriod; set => Client.AveragePeriod = value; }
        public double CloudCover => Client.CloudCover;
        public double DewPoint => Client.DewPoint;
        public double Humidity => Client.Humidity;
        public double Pressure => Client.Pressure;
        public double RainRate => Client.RainRate;
        public double SkyBrightness => Client.SkyBrightness;
        public double SkyQuality => Client.SkyQuality;
        public double SkyTemperature => Client.SkyTemperature;
        public double StarFWHM => Client.StarFWHM;
        public double Temperature => Client.Temperature;
        public double WindDirection => Client.WindDirection;
        public double WindGust => Client.WindGust;
        public double WindSpeed => Client.WindSpeed;

        public void Refresh() => Client.Refresh();
        public string SensorDescription(string propertyName) => Client.SensorDescription(propertyName);
        public double TimeSinceLastUpdate(string propertyName) => Client.TimeSinceLastUpdate(propertyName);
    }
}
