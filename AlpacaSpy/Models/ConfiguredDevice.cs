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
        public bool LogClientHeaders { get; set; } = true;
        public bool LogClientQueryParams { get; set; } = true;
        public bool LogClientBody { get; set; } = true;
        public bool LogDeviceHeaders { get; set; } = true;
        public bool LogDeviceJson { get; set; } = true;
    }
}
