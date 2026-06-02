using AlpacaSpy.Models;
using ASCOM;
using ASCOM.Alpaca.Clients;
using ASCOM.Common.DeviceInterfaces;

namespace AlpacaSpy.ProxyDevices
{
    public class ProxyTelescope : ITelescopeV4
    {
        private readonly ConfiguredDevice _config;
        private readonly State _state;
        private readonly AlpacaSpyLogger _logger;
        private AlpacaTelescope? _client;

        private AlpacaTelescope Client => _client ?? throw new NotConnectedException($"Not connected to {_config.Name}");

        public ProxyTelescope(ConfiguredDevice config, State state, AlpacaSpyLogger logger)
        {
            _config = config;
            _state = state;
            _logger = logger;
        }

        internal void ConnectDevice()
        {
            if (_client?.Connected == true) return;
            DisconnectDevice();
            _client = new AlpacaTelescope(ASCOM.Common.Alpaca.ServiceType.Http, _config.IpAddress, _config.PortNumber, _config.RemoteDeviceNumber, false, null);
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

        public AlignmentMode AlignmentMode => Client.AlignmentMode;
        public double Altitude => Client.Altitude;
        public double ApertureArea => Client.ApertureArea;
        public double ApertureDiameter => Client.ApertureDiameter;
        public bool AtHome => Client.AtHome;
        public bool AtPark => Client.AtPark;
        public double Azimuth => Client.Azimuth;
        public bool CanFindHome => Client.CanFindHome;
        public bool CanPark => Client.CanPark;
        public bool CanPulseGuide => Client.CanPulseGuide;
        public bool CanSetDeclinationRate => Client.CanSetDeclinationRate;
        public bool CanSetGuideRates => Client.CanSetGuideRates;
        public bool CanSetPark => Client.CanSetPark;
        public bool CanSetPierSide => Client.CanSetPierSide;
        public bool CanSetRightAscensionRate => Client.CanSetRightAscensionRate;
        public bool CanSetTracking => Client.CanSetTracking;
        public bool CanSlew => Client.CanSlew;
        public bool CanSlewAltAz => Client.CanSlewAltAz;
        public bool CanSlewAltAzAsync => Client.CanSlewAltAzAsync;
        public bool CanSlewAsync => Client.CanSlewAsync;
        public bool CanSync => Client.CanSync;
        public bool CanSyncAltAz => Client.CanSyncAltAz;
        public bool CanUnpark => Client.CanUnpark;
        public double Declination => Client.Declination;
        public double DeclinationRate { get => Client.DeclinationRate; set => Client.DeclinationRate = value; }
        public bool DoesRefraction { get => Client.DoesRefraction; set => Client.DoesRefraction = value; }
        public EquatorialCoordinateType EquatorialSystem => Client.EquatorialSystem;
        public double FocalLength => Client.FocalLength;
        public double GuideRateDeclination { get => Client.GuideRateDeclination; set => Client.GuideRateDeclination = value; }
        public double GuideRateRightAscension { get => Client.GuideRateRightAscension; set => Client.GuideRateRightAscension = value; }
        public bool IsPulseGuiding => Client.IsPulseGuiding;
        public double RightAscension => Client.RightAscension;
        public double RightAscensionRate { get => Client.RightAscensionRate; set => Client.RightAscensionRate = value; }
        public PointingState SideOfPier { get => Client.SideOfPier; set => Client.SideOfPier = value; }
        public double SiderealTime => Client.SiderealTime;
        public double SiteElevation { get => Client.SiteElevation; set => Client.SiteElevation = value; }
        public double SiteLatitude { get => Client.SiteLatitude; set => Client.SiteLatitude = value; }
        public double SiteLongitude { get => Client.SiteLongitude; set => Client.SiteLongitude = value; }
        public bool Slewing => Client.Slewing;
        public short SlewSettleTime { get => Client.SlewSettleTime; set => Client.SlewSettleTime = value; }
        public double TargetDeclination { get => Client.TargetDeclination; set => Client.TargetDeclination = value; }
        public double TargetRightAscension { get => Client.TargetRightAscension; set => Client.TargetRightAscension = value; }
        public bool Tracking { get => Client.Tracking; set => Client.Tracking = value; }
        public DriveRate TrackingRate { get => Client.TrackingRate; set => Client.TrackingRate = value; }
        public ITrackingRates TrackingRates => Client.TrackingRates;
        public DateTime UTCDate { get => Client.UTCDate; set => Client.UTCDate = value; }

        public void AbortSlew() => Client.AbortSlew();
        public IAxisRates AxisRates(TelescopeAxis axis) => Client.AxisRates(axis);
        public bool CanMoveAxis(TelescopeAxis axis) => Client.CanMoveAxis(axis);
        public PointingState DestinationSideOfPier(double rightAscension, double declination) => Client.DestinationSideOfPier(rightAscension, declination);
        public void FindHome() => Client.FindHome();
        public void MoveAxis(TelescopeAxis axis, double rate) => Client.MoveAxis(axis, rate);
        public void Park() => Client.Park();
        public void PulseGuide(GuideDirection direction, int duration) => Client.PulseGuide(direction, duration);
        public void SetPark() => Client.SetPark();
        public void SlewToAltAz(double azimuth, double altitude) => Client.SlewToAltAz(azimuth, altitude);
        public void SlewToAltAzAsync(double azimuth, double altitude) => Client.SlewToAltAzAsync(azimuth, altitude);
        public void SlewToCoordinates(double rightAscension, double declination) => Client.SlewToCoordinates(rightAscension, declination);
        public void SlewToCoordinatesAsync(double rightAscension, double declination) => Client.SlewToCoordinatesAsync(rightAscension, declination);
        public void SlewToTarget() => Client.SlewToTarget();
        public void SlewToTargetAsync() => Client.SlewToTargetAsync();
        public void SyncToAltAz(double azimuth, double altitude) => Client.SyncToAltAz(azimuth, altitude);
        public void SyncToCoordinates(double rightAscension, double declination) => Client.SyncToCoordinates(rightAscension, declination);
        public void SyncToTarget() => Client.SyncToTarget();
        public void Unpark() => Client.Unpark();
    }
}
