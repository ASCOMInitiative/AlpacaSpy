using ASCOM.Alpaca;

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
            logger.LogMessage("Disconnect", "Disconnecting from all proxy devices...");
            try
            {
                foreach (var device in state.ProxyDevices)
                {
                    try
                    {
                        DisconnectProxy(device);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Disconnect", $"Error disconnecting device: {ex.Message}");
                    }
                }
                // Do NOT clear state.ProxyDevices — DeviceManager holds the same instances
                // and must continue to serve them for Alpaca HTTP even while disconnected.
            }
            catch (Exception ex)
            {
                logger.LogError("Disconnect", $"Exception during disconnect: {ex.Message}");
            }
            finally
            {
                logger.LogMessage("Disconnect", "All proxy devices disconnected.");
                logger.LogBlankLine();
                state.Connected = false;
            }
        }

        internal static async Task ConnectAsync(State state, Settings settings, AlpacaSpyLogger logger)
        {
            await Globals.ConnectSemaphore.WaitAsync();
            try
            {
                if (state.ProxyDevices.Count == 0)
                {
                    logger.LogMessage("Connect", "No proxy devices loaded. Configure devices in Setup and restart.");
                    return;
                }

                logger.LogMessage("Connect", $"Connecting to {state.ProxyDevices.Count} proxy device(s)...");
                int successCount = 0;

                for (int i = 0; i < state.ProxyDevices.Count; i++)
                {
                    string name = i < settings.ConfiguredDevices.Count ? settings.ConfiguredDevices[i].Name : "Unknown";
                    try
                    {
                        logger.LogMessage("Connect", $"Connecting to {name}...");
                        ConnectProxy(state.ProxyDevices[i]);
                        successCount++;
                        logger.LogMessage("Connect", $"Connected to {name}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Connect", $"Failed to connect to {name}: {ex.Message}");
                    }
                }

                if (successCount > 0)
                {
                    state.Connected = true;
                    logger.LogMessage("Connect", $"Connected to {successCount}/{state.ProxyDevices.Count} device(s).");
                }
                else
                {
                    logger.LogWarning("Connect", "No devices connected successfully.");
                }
                logger.LogBlankLine();
            }
            finally
            {
                Globals.ConnectSemaphore.Release();
            }
        }

        private static void ConnectProxy(object proxy)
        {
            switch (proxy)
            {
                case ProxyDevices.ProxyCamera p: p.ConnectDevice(); break;
                case ProxyDevices.ProxyCoverCalibrator p: p.ConnectDevice(); break;
                case ProxyDevices.ProxyDome p: p.ConnectDevice(); break;
                case ProxyDevices.ProxyFilterWheel p: p.ConnectDevice(); break;
                case ProxyDevices.ProxyFocuser p: p.ConnectDevice(); break;
                case ProxyDevices.ProxyObservingConditions p: p.ConnectDevice(); break;
                case ProxyDevices.ProxyRotator p: p.ConnectDevice(); break;
                case ProxyDevices.ProxySafetyMonitor p: p.ConnectDevice(); break;
                case ProxyDevices.ProxySwitch p: p.ConnectDevice(); break;
                case ProxyDevices.ProxyTelescope p: p.ConnectDevice(); break;
            }
        }

        private static void DisconnectProxy(object proxy)
        {
            switch (proxy)
            {
                case ProxyDevices.ProxyCamera p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxyCoverCalibrator p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxyDome p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxyFilterWheel p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxyFocuser p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxyObservingConditions p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxyRotator p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxySafetyMonitor p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxySwitch p: p.DisconnectDevice(); break;
                case ProxyDevices.ProxyTelescope p: p.DisconnectDevice(); break;
            }
        }
    }
}
