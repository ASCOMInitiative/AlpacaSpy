using ASCOM.Alpaca;

namespace AlpacaSpy.Models
{
    internal class AlpacaConfiguration : IAlpacaConfiguration
    {
        public bool RunInStrictAlpacaMode => Program.settings.RunInStrictAlpacaMode;
        public bool PreventRemoteDisconnects => true;
        public string ServerName => Program.ServerName;
        public string Manufacturer => Program.Manufacturer;
        public string ServerVersion => Program.ServerVersion;
        public string Location => Program.settings.Location;
        public bool AllowImageBytesDownload => false;
        public bool AllowDiscovery => Program.settings.AllowDiscovery;
        public int ServerPort => Program.settings.ServerPort;
        public bool AllowRemoteAccess => Program.settings.BindToAllNetworkAddresses;
        public bool LocalRespondOnlyToLocalHost => !Program.settings.SendDiscoveryResponsesOnAllNetworks;
        public bool RunSwagger => false;
    }
}
