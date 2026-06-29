namespace AlpacaSpy.Models
{
    public class ConfiguredDevice
    {
        // Public values that will be persisted when configuration is saved
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
        public List<string>? EnabledLogMembers { get; set; }

        // Internal values that are not persisted
        internal bool Recording { get; set; } = false;
    }
}
