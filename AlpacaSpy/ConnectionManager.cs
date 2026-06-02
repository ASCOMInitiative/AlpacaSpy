using AlpacaSpy.Models;
using System.Text.RegularExpressions;

namespace AlpacaSpy
{
    internal static class ConnectionManager
    {
        internal static async Task ChangeConnectedStateAsync(State state, Settings settings, AlpacaSpyLogger logger, Func<Task> invokeStateHasChanged, bool connectOnly = false)
        {
            lock (Globals.StateLock)
            {
                if (state.ConnectingToDevices) return;
                if (connectOnly && state.Connected) return;
                state.ConnectingToDevices = true;
            }
            try
            {
                await invokeStateHasChanged();
                if (state.Connected)
                {
                    Disconnect(state, logger);
                    await invokeStateHasChanged();
                }
                else
                {
                    await ConnectAsync(state, settings, logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("ChangeConnectedStateAsync", $"Overall exception: \r\n{ex}");
            }
            finally
            {
                state.ConnectingToDevices = false;
                await invokeStateHasChanged();
            }
        }

        internal static void Disconnect(State state, AlpacaSpyLogger logger)
        {
            logger.LogMessage("Disconnect", "Proxy monitoring stopped.");
            logger.LogBlankLine();
            state.Connected = false;
        }

        internal static async Task ConnectAsync(State state, Settings settings, AlpacaSpyLogger logger)
        {
            await Globals.ConnectSemaphore.WaitAsync();
            try
            {
                List<ConfiguredDevice> devices = state.ConfiguredDevices;
                if (devices.Count == 0)
                {
                    logger.LogMessage("Connect", "No devices configured. Add devices in Setup and restart.");
                    return;
                }

                logger.LogMessage("Connect", $"Checking connectivity to {devices.Count} configured device(s)...");
                int successCount = 0;

                using HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
                foreach (ConfiguredDevice device in devices)
                {
                    try
                    {
                        logger.LogMessage("Connect", $"  Checking {device.Name} at {device.IpAddress}:{device.PortNumber}...");
                        string deviceTypePath = DeviceTypePath(device.DeviceType);
                        string url = $"http://{device.IpAddress}:{device.PortNumber}/api/v1/{deviceTypePath}/{device.RemoteDeviceNumber}/connected";
                        HttpResponseMessage response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            successCount++;
                            logger.LogMessage("Connect", $"  {device.Name}: reachable ✓");
                        }
                        else
                        {
                            logger.LogWarning("Connect", $"  {device.Name}: HTTP {(int)response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Connect", $"  {device.Name}: {ex.Message}");
                    }
                }

                if (successCount > 0)
                {
                    state.Connected = true;
                    logger.LogMessage("Connect", $"Connected — {successCount}/{devices.Count} device(s) reachable. Proxy active.");
                }
                else
                {
                    logger.LogWarning("Connect", "No devices reachable. Check device configuration.");
                }
                logger.LogBlankLine();
            }
            finally
            {
                Globals.ConnectSemaphore.Release();
            }
        }

        private static string DeviceTypePath(Models.AlpacaDeviceType deviceType) =>
            deviceType switch
            {
                Models.AlpacaDeviceType.Camera              => "camera",
                Models.AlpacaDeviceType.CoverCalibrator     => "covercalibrator",
                Models.AlpacaDeviceType.Dome                => "dome",
                Models.AlpacaDeviceType.FilterWheel         => "filterwheel",
                Models.AlpacaDeviceType.Focuser             => "focuser",
                Models.AlpacaDeviceType.ObservingConditions => "observingconditions",
                Models.AlpacaDeviceType.Rotator             => "rotator",
                Models.AlpacaDeviceType.SafetyMonitor       => "safetymonitor",
                Models.AlpacaDeviceType.Switch              => "switch",
                Models.AlpacaDeviceType.Telescope           => "telescope",
                _                                           => deviceType.ToString().ToLowerInvariant()
            };
    }
}
