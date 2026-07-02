using AlpacaSpy.Models;
using ASCOM;
using ASCOM.Alpaca.Clients;
using ASCOM.Common.Alpaca;
using ASCOM.Common.DeviceInterfaces;

namespace AlpacaSpy.ProxyDevices
{
    public class ProxyCamera : ICameraV4, IDisposable
    {
        private readonly ConfiguredDevice _config;
        private readonly State _state;
        private readonly Settings _settings;
        private readonly AlpacaSpyLogger _logger;
        private AlpacaCamera? _client;

        private AlpacaCamera Client => _client ?? throw new NotConnectedException($"Not connected to {_config.Name}");

        public ProxyCamera(ConfiguredDevice config, State state, Settings settings, AlpacaSpyLogger logger)
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
            _client = new AlpacaCamera(ServiceType.Http, _config.IpAddress, _config.PortNumber, _config.RemoteDeviceNumber, false, _settings.LogLevel < ASCOM.Common.Interfaces.LogLevel.Information ? _logger : null);
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

        public short BayerOffsetX => Client.BayerOffsetX;
        public short BayerOffsetY => Client.BayerOffsetY;
        public short BinX { get => Client.BinX; set => Client.BinX = value; }
        public short BinY { get => Client.BinY; set => Client.BinY = value; }
        public CameraState CameraState => Client.CameraState;
        public int CameraXSize => Client.CameraXSize;
        public int CameraYSize => Client.CameraYSize;
        public bool CanAbortExposure => Client.CanAbortExposure;
        public bool CanAsymmetricBin => Client.CanAsymmetricBin;
        public bool CanFastReadout => Client.CanFastReadout;
        public bool CanGetCoolerPower => Client.CanGetCoolerPower;
        public bool CanPulseGuide => Client.CanPulseGuide;
        public bool CanSetCCDTemperature => Client.CanSetCCDTemperature;
        public bool CanStopExposure => Client.CanStopExposure;
        public double CCDTemperature => Client.CCDTemperature;
        public bool CoolerOn { get => Client.CoolerOn; set => Client.CoolerOn = value; }
        public double CoolerPower => Client.CoolerPower;
        public double ElectronsPerADU => Client.ElectronsPerADU;
        public double ExposureMax => Client.ExposureMax;
        public double ExposureMin => Client.ExposureMin;
        public double ExposureResolution => Client.ExposureResolution;
        public bool FastReadout { get => Client.FastReadout; set => Client.FastReadout = value; }
        public double FullWellCapacity => Client.FullWellCapacity;
        public short Gain { get => Client.Gain; set => Client.Gain = value; }
        public short GainMax => Client.GainMax;
        public short GainMin => Client.GainMin;
        public IList<string> Gains => Client.Gains;
        public bool HasShutter => Client.HasShutter;
        public double HeatSinkTemperature => Client.HeatSinkTemperature;
        public object ImageArray => Client.ImageArray;
        public object ImageArrayVariant => Client.ImageArrayVariant;
        public bool ImageReady => Client.ImageReady;
        public bool IsPulseGuiding => Client.IsPulseGuiding;
        public double LastExposureDuration => Client.LastExposureDuration;
        public string LastExposureStartTime => Client.LastExposureStartTime;
        public int MaxADU => Client.MaxADU;
        public short MaxBinX => Client.MaxBinX;
        public short MaxBinY => Client.MaxBinY;
        public int NumX { get => Client.NumX; set => Client.NumX = value; }
        public int NumY { get => Client.NumY; set => Client.NumY = value; }
        public int Offset { get => Client.Offset; set => Client.Offset = value; }
        public int OffsetMax => Client.OffsetMax;
        public int OffsetMin => Client.OffsetMin;
        public IList<string> Offsets => Client.Offsets;
        public short PercentCompleted => Client.PercentCompleted;
        public double PixelSizeX => Client.PixelSizeX;
        public double PixelSizeY => Client.PixelSizeY;
        public short ReadoutMode { get => Client.ReadoutMode; set => Client.ReadoutMode = value; }
        public IList<string> ReadoutModes => Client.ReadoutModes;
        public string SensorName => Client.SensorName;
        public SensorType SensorType => Client.SensorType;
        public double SetCCDTemperature { get => Client.SetCCDTemperature; set => Client.SetCCDTemperature = value; }
        public int StartX { get => Client.StartX; set => Client.StartX = value; }
        public int StartY { get => Client.StartY; set => Client.StartY = value; }
        public double SubExposureDuration { get => Client.SubExposureDuration; set => Client.SubExposureDuration = value; }

        public void AbortExposure() => Client.AbortExposure();
        public void PulseGuide(GuideDirection direction, int duration) => Client.PulseGuide(direction, duration);
        public void StartExposure(double duration, bool light) => Client.StartExposure(duration, light);
        public void StopExposure() => Client.StopExposure();
    }
}
