using AlpacaSpy.Models;
using ASCOM;
using ASCOM.Alpaca.Clients;
using ASCOM.Common.DeviceInterfaces;

namespace AlpacaSpy.ProxyDevices
{
    public class ProxyDome : IDomeV3
    {
        private readonly ConfiguredDevice _config;
        private readonly State _state;
        private readonly AlpacaSpyLogger _logger;
        private AlpacaDome? _client;

        private AlpacaDome Client => _client ?? throw new NotConnectedException($"Not connected to {_config.Name}");

        public ProxyDome(ConfiguredDevice config, State state, AlpacaSpyLogger logger)
        {
            _config = config;
            _state = state;
            _logger = logger;
        }

        internal void ConnectDevice()
        {
            if (_client?.Connected == true) return;
            DisconnectDevice();
            _client = new AlpacaDome(ASCOM.Common.Alpaca.ServiceType.Http, _config.IpAddress, _config.PortNumber, _config.RemoteDeviceNumber, false, null);
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

        public double Altitude => Client.Altitude;
        public bool AtHome => Client.AtHome;
        public bool AtPark => Client.AtPark;
        public double Azimuth => Client.Azimuth;
        public bool CanFindHome => Client.CanFindHome;
        public bool CanPark => Client.CanPark;
        public bool CanSetAltitude => Client.CanSetAltitude;
        public bool CanSetAzimuth => Client.CanSetAzimuth;
        public bool CanSetPark => Client.CanSetPark;
        public bool CanSetShutter => Client.CanSetShutter;
        public bool CanSlave => Client.CanSlave;
        public bool CanSyncAzimuth => Client.CanSyncAzimuth;
        public ShutterState ShutterStatus => Client.ShutterStatus;
        public bool Slaved { get => Client.Slaved; set => Client.Slaved = value; }
        public bool Slewing => Client.Slewing;

        public void AbortSlew() => Client.AbortSlew();
        public void CloseShutter() => Client.CloseShutter();
        public void FindHome() => Client.FindHome();
        public void OpenShutter() => Client.OpenShutter();
        public void Park() => Client.Park();
        public void SetPark() => Client.SetPark();
        public void SlewToAltitude(double altitude) => Client.SlewToAltitude(altitude);
        public void SlewToAzimuth(double azimuth) => Client.SlewToAzimuth(azimuth);
        public void SyncToAzimuth(double azimuth) => Client.SyncToAzimuth(azimuth);
    }
}
