namespace AlpacaSpy.Models
{
    public class ConfiguredDevice
    {
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int PortNumber { get; set; } = 11111;
        public int RemoteDeviceNumber { get; set; } = 0;
        public AlpacaDeviceType DeviceType { get; set; } = AlpacaDeviceType.Telescope;
        public int ProxyDeviceNumber { get; set; } = 0;
        public string UniqueId { get; set; } = Guid.NewGuid().ToString();
        public bool LogClientHeaders { get; set; } = false;
        public bool LogClientParams { get; set; } = false;
        public bool LogDeviceHeaders { get; set; } = false;
        public bool LogJsonParameters { get; set; } = false;
    }
}
