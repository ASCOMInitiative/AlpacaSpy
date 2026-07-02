using ASCOM.Alpaca;
using ASCOM.Common;
using ASCOM.Tools;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Radzen;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AlpacaSpy
{
    public class Program
    {
        internal const string DriverID = "ASCOMAlpacaSpy";
        internal const string Manufacturer = "Peter Simpson";
        internal const string SingleInstanceMutexName = @"Global\ASCOMAlpacaSpy";
        internal static string ServerName = Globals.APPLICATION_NAME;
        internal static string ServerVersion = "Not set";

        internal static State state = new();
        internal static Settings settings = new Settings(string.Empty);
        internal static AlpacaSpyLogger logger = new(state, settings);

        internal static IHostApplicationLifetime? applicationLifetime;
        internal static bool RestartRequested;

        public static async Task Main(string[] args)
        {
            ServerVersion = state.ApplicationVersion;

            logger.LogMessage("Main", $"{ServerName} version {state.InformationalVersion}");
            logger.LogMessage("Main", $"Running on: {RuntimeInformation.OSDescription}.");
            logger.LogBlankLine();

            if (args?.Any(str => str.Contains("--reset")) ?? false)
            {
                logger.LogMessage(string.Empty, "Resetting settings to defaults.");
                settings.ResetToDefaults();
                return;
            }

            if (args?.Any(str => str.Contains("--local-address")) ?? false)
                Console.WriteLine($"http://localhost:{settings.ServerPort}");

            logger.LogBlankLine();

            Mutex? singleInstanceMutex = null;
            bool ownsSingleInstanceMutex = false;

            try
            {
                singleInstanceMutex = new Mutex(false, SingleInstanceMutexName, out _);
                try
                {
                    ownsSingleInstanceMutex = singleInstanceMutex.WaitOne(0);
                }
                catch (AbandonedMutexException)
                {
                    ownsSingleInstanceMutex = true;
                }

                if (!ownsSingleInstanceMutex)
                {
                    logger.LogMessage(nameof(Main), "Another AlpacaSpy instance is already running. Opening the browser.");
                    StartBrowser(settings.ServerPort);
                    return;
                }

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args ?? []);

                if (!(args?.Any(str => str.Contains("--urls")) ?? false))
                {
                    string host = settings.BindToAllNetworkAddresses ? "*" : "localhost";
                    builder.WebHost.UseUrls($"http://{host}:{settings.ServerPort}");
                }

                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
                builder.Logging.SetMinimumLevel(ToMicrosoftLogLevel(settings.LogLevel));

                Logging.AttachLogger(logger);

                DeviceManager.LoadConfiguration(new AlpacaConfiguration());
                LoadProxyDevices(settings);

                builder.Services.AddRazorPages();
                builder.Services.AddServerSideBlazor(options =>
                {
                    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(Globals.DISCONNECTED_CIRCUIT_RETENTION_PERIOD);
                });
                builder.Host.ConfigureHostOptions(options =>
                {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(Globals.APPLICATION_SHUTDOWN_TIMEOUT);
                });

                ASCOM.Alpaca.Razor.StartupHelpers.ConfigureAlpacaAPIBehavoir(builder.Services);
                ASCOM.Alpaca.Razor.StartupHelpers.ConfigureAuthentication(builder.Services);
                builder.Services.AddScoped<ASCOM.Alpaca.IUserService, Data.UserService>();
                builder.Services.AddHttpClient("AlpacaProxy");

                builder.Services.AddRadzenComponents();
                builder.Services.AddScoped<PerBrowserState>();
                builder.Services.AddSingleton<State>(_ => state);
                builder.Services.AddSingleton<AlpacaSpyLogger>(_ => logger);
                builder.Services.AddSingleton<Settings>(_ => settings);
                builder.Services.AddSingleton<CircuitHandler, CircuitHandlerService>();

                WebApplication app = builder.Build();

                if (!app.Environment.IsDevelopment())
                    app.UseExceptionHandler("/Error");

                ASCOM.Alpaca.Razor.StartupHelpers.ConfigureDiscovery(app);
                ASCOM.Alpaca.Razor.StartupHelpers.ConfigureAuthentication(app);

                app.UseStaticFiles();
                app.UseMiddleware<AlpacaProxyMiddleware>();
                app.UseRouting();
                app.MapBlazorHub(options =>
                {
                    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(Globals.WEBSOCKET_CLOSE_TIMEOUT);
                });
                app.MapControllers();
                app.MapFallbackToPage("/_Host");

                applicationLifetime = app.Lifetime;
                applicationLifetime.ApplicationStarted.Register(() =>
                {
                    if (settings.StartBrowserOnLaunch && !(args?.Any(str => str.Contains("--nobrowser")) ?? false))
                    {
                        Task.Run(() => StartBrowserWhenReadyAsync(settings.ServerPort));
                    }

                    if (settings.AutoConnect && !state.Connected)
                        Task.Run(async () =>
                        {
                            lock (Globals.StateLock)
                            {
                                if (state.ConnectingToDevices) return;
                                state.ConnectingToDevices = true;
                            }

                            try
                            {
                                state.OperationUnderway = true;
                                await ConnectionManager.ConnectAsync(state, settings, logger);
                            }
                            finally
                            {
                                state.ConnectingToDevices = false;
                                state.OperationUnderway = false;
                            }
                        });
                });

                applicationLifetime.ApplicationStopping.Register(() =>
                {
                    logger.LogMessage(nameof(Main), "Application shutting down...");
                });

                applicationLifetime.ApplicationStopped.Register(() =>
                {
                    logger.LogBlankLine();
                    logger.LogMessage(nameof(Main), "Application shutdown complete.");
                });

                app.Run();

                // Clear the proxy devices to release any file locks before attempting restart
                foreach (IDisposable device in state.ProxyDevices) try { device.Dispose(); } catch { }
                state.ProxyDevices.Clear();

                //Clear the device loggers to release file locks before attempting restart
                foreach (TraceLogger deviceLogger in state.DeviceLoggers.Values) try { deviceLogger.Dispose(); } catch { }
                state.DeviceLoggers.Clear();

                // If a restart was requested, wait a moment for the current process to exit before starting a new instance
                if (RestartRequested)
                {
                    try
                    {
                        if (ownsSingleInstanceMutex)
                        {
                            singleInstanceMutex.ReleaseMutex();
                            ownsSingleInstanceMutex = false;
                        }

                        string? processPath = Environment.ProcessPath;
                        if (!string.IsNullOrWhiteSpace(processPath))
                        {
                            Thread.Sleep(Globals.RESTART_DELAY * 1000);
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = processPath,
                                Arguments = "--nobrowser",
                                UseShellExecute = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(nameof(Main), $"Failed to restart: {ex.Message}");
                    }
                }
            }
            finally
            {
                if (singleInstanceMutex is not null)
                {
                    if (ownsSingleInstanceMutex)
                    {
                        try
                        {
                            singleInstanceMutex.ReleaseMutex();
                        }
                        catch { }
                    }

                    singleInstanceMutex.Dispose();
                }
            }
        }

        private static void LoadProxyDevices(Settings settings)
        {
            // Initialise configured devices in the state object
            state.ConfiguredDevices = settings.ConfiguredDevices?.ToList() ?? new List<Models.ConfiguredDevice>();
            settings.ConfiguredDevices ??= new List<Models.ConfiguredDevice>();

            foreach (Models.ConfiguredDevice config in settings.ConfiguredDevices)
            {
                string proxyName = $"{Globals.APPLICATION_SHORT_NAME} - {config.Name}";
                try
                {
                    // Create the proxy object once and register the SAME instance with both
                    // DeviceManager (serves Alpaca HTTP) and state.ProxyDevices (managed by ConnectionManager).
                    object proxy = config.DeviceType switch
                    {
                        Models.AlpacaDeviceType.Camera => new ProxyDevices.ProxyCamera(config, state, settings, logger),
                        Models.AlpacaDeviceType.CoverCalibrator => new ProxyDevices.ProxyCoverCalibrator(config, state, settings, logger),
                        Models.AlpacaDeviceType.Dome => new ProxyDevices.ProxyDome(config, state, settings, logger),
                        Models.AlpacaDeviceType.FilterWheel => new ProxyDevices.ProxyFilterWheel(config, state, settings, logger),
                        Models.AlpacaDeviceType.Focuser => new ProxyDevices.ProxyFocuser(config, state, settings, logger),
                        Models.AlpacaDeviceType.ObservingConditions => new ProxyDevices.ProxyObservingConditions(config, state, settings, logger),
                        Models.AlpacaDeviceType.Rotator => new ProxyDevices.ProxyRotator(config, state, settings, logger),
                        Models.AlpacaDeviceType.SafetyMonitor => new ProxyDevices.ProxySafetyMonitor(config, state, settings, logger),
                        Models.AlpacaDeviceType.Switch => new ProxyDevices.ProxySwitch(config, state, settings, logger),
                        Models.AlpacaDeviceType.Telescope => new ProxyDevices.ProxyTelescope(config, state, settings, logger),
                        _ => throw new ArgumentOutOfRangeException(nameof(config.DeviceType), config.DeviceType, "Unknown device type")
                    };

                    switch (config.DeviceType)
                    {
                        case Models.AlpacaDeviceType.Camera:
                            DeviceManager.LoadCamera(config.ProxyDeviceNumber, (ProxyDevices.ProxyCamera)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.CoverCalibrator:
                            DeviceManager.LoadCoverCalibrator(config.ProxyDeviceNumber, (ProxyDevices.ProxyCoverCalibrator)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.Dome:
                            DeviceManager.LoadDome(config.ProxyDeviceNumber, (ProxyDevices.ProxyDome)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.FilterWheel:
                            DeviceManager.LoadFilterWheel(config.ProxyDeviceNumber, (ProxyDevices.ProxyFilterWheel)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.Focuser:
                            DeviceManager.LoadFocuser(config.ProxyDeviceNumber, (ProxyDevices.ProxyFocuser)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.ObservingConditions:
                            DeviceManager.LoadObservingConditions(config.ProxyDeviceNumber, (ProxyDevices.ProxyObservingConditions)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.Rotator:
                            DeviceManager.LoadRotator(config.ProxyDeviceNumber, (ProxyDevices.ProxyRotator)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.SafetyMonitor:
                            DeviceManager.LoadSafetyMonitor(config.ProxyDeviceNumber, (ProxyDevices.ProxySafetyMonitor)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.Switch:
                            DeviceManager.LoadSwitch(config.ProxyDeviceNumber, (ProxyDevices.ProxySwitch)proxy, proxyName, config.UniqueId);
                            break;
                        case Models.AlpacaDeviceType.Telescope:
                            DeviceManager.LoadTelescope(config.ProxyDeviceNumber, (ProxyDevices.ProxyTelescope)proxy, proxyName, config.UniqueId);
                            break;
                    }

                    state.ProxyDevices.Add(proxy);
                    logger.LogMessage("LoadDevices", $"Registered {proxyName} as proxy device {config.ProxyDeviceNumber}");

                    // Create a per-device TraceLogger for file-based traffic logging
                    string safeName = Regex.Replace(config.Name, @"[^\w]", "_");
                    TraceLogger deviceLogger = new($"AlpacaSpy.{safeName}", true);
                    state.DeviceLoggers[config.UniqueId] = deviceLogger;

                    // Initialise the recording memory object for this device
                    state.Transactions[config] = new();
                }
                catch (Exception ex)
                {
                    logger.LogError("LoadDevices", $"Failed to load {proxyName}: {ex.Message}");
                }
            }
        }

        internal static void StartBrowser(int port)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"http://localhost:{port}/",
                UseShellExecute = true
            });
        }

        private static async Task StartBrowserWhenReadyAsync(int port)
        {
            string[] startupPaths =
            [
                "/",
                "/_framework/blazor.server.js",
                "/css/site.css",
                "/alpacaspy.styles.css",
                "/_content/Radzen.Blazor/css/standard-base.css",
                "/_content/Radzen.Blazor/Radzen.Blazor.js"
            ];

            try
            {
                using HttpClient httpClient = new()
                {
                    Timeout = TimeSpan.FromSeconds(2)
                };

                for (int attempt = 0; attempt < 20; attempt++)
                {
                    bool allReady = true;

                    foreach (string startupPath in startupPaths)
                    {
                        try
                        {
                            Uri startupUri = new($"http://localhost:{port}{startupPath}");
                            using HttpRequestMessage request = new(HttpMethod.Get, startupUri);
                            using HttpResponseMessage response = await httpClient.SendAsync(request);

                            if (!response.IsSuccessStatusCode)
                            {
                                allReady = false;
                                break;
                            }
                        }
                        catch (HttpRequestException)
                        {
                            allReady = false;
                            break;
                        }
                        catch (TaskCanceledException)
                        {
                            allReady = false;
                            break;
                        }
                    }

                    if (allReady)
                    {
                        await Task.Delay(500);
                        StartBrowser(port);
                        return;
                    }

                    await Task.Delay(250);
                }

                logger.LogWarning("Main", $"Timed out waiting for startup assets on http://localhost:{port} before opening the browser.");
                StartBrowser(port);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Main", ex.Message);
            }
        }

        private static LogLevel ToMicrosoftLogLevel(ASCOM.Common.Interfaces.LogLevel logLevel)
        {
            return logLevel switch
            {
                ASCOM.Common.Interfaces.LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Trace,
                ASCOM.Common.Interfaces.LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
                ASCOM.Common.Interfaces.LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
                ASCOM.Common.Interfaces.LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                ASCOM.Common.Interfaces.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                _ => Microsoft.Extensions.Logging.LogLevel.Information
            };
        }
    }
}
