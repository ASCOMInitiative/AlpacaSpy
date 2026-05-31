using ASCOM.Alpaca;
using ASCOM.Common;
using ASCOM.Tools;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Radzen;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AlpacaSpy
{
    public class Program
    {
        internal const string DriverID = "ASCOMAlpacaSpy";
        internal const string Manufacturer = "Peter Simpson";
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

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                        .Any(con => con.LocalEndPoint.Port == settings.ServerPort &&
                                    (con.State == TcpState.Listen || con.State == TcpState.Established)))
                    {
                        logger.LogMessage(string.Empty, "Detected port already in use, starting browser.");
                        StartBrowser(settings.ServerPort);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Main", ex.Message);
            }

            if (args?.Any(str => str.Contains("--reset")) ?? false)
            {
                logger.LogMessage(string.Empty, "Resetting settings to defaults.");
                settings.ResetToDefaults();
                return;
            }

            if (args?.Any(str => str.Contains("--local-address")) ?? false)
                Console.WriteLine($"http://localhost:{settings.ServerPort}");

            logger.LogBlankLine();

            var builder = WebApplication.CreateBuilder(args ?? []);

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

            var app = builder.Build();

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

            try
            {
                if (settings.StartBrowserOnLaunch && !(args?.Any(str => str.Contains("--nobrowser")) ?? false))
                    StartBrowser(settings.ServerPort);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Main", ex.Message);
            }

            applicationLifetime = app.Lifetime;
            applicationLifetime.ApplicationStarted.Register(() =>
            {
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

            if (RestartRequested)
            {
                try
                {
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

        private static void LoadProxyDevices(Settings settings)
        {
            state.ConfiguredDevices = settings.ConfiguredDevices?.ToList() ?? new List<Models.ConfiguredDevice>();
            settings.ConfiguredDevices ??= new List<Models.ConfiguredDevice>();

            foreach (var config in settings.ConfiguredDevices)
            {
                string proxyName = $"AlpacaSpy - {config.Name}";
                try
                {
                    // Create the proxy object once and register the SAME instance with both
                    // DeviceManager (serves Alpaca HTTP) and state.ProxyDevices (managed by ConnectionManager).
                    object proxy = config.DeviceType switch
                    {
                        Models.AlpacaDeviceType.Camera => new ProxyDevices.ProxyCamera(config, state, logger),
                        Models.AlpacaDeviceType.CoverCalibrator => new ProxyDevices.ProxyCoverCalibrator(config, state, logger),
                        Models.AlpacaDeviceType.Dome => new ProxyDevices.ProxyDome(config, state, logger),
                        Models.AlpacaDeviceType.FilterWheel => new ProxyDevices.ProxyFilterWheel(config, state, logger),
                        Models.AlpacaDeviceType.Focuser => new ProxyDevices.ProxyFocuser(config, state, logger),
                        Models.AlpacaDeviceType.ObservingConditions => new ProxyDevices.ProxyObservingConditions(config, state, logger),
                        Models.AlpacaDeviceType.Rotator => new ProxyDevices.ProxyRotator(config, state, logger),
                        Models.AlpacaDeviceType.SafetyMonitor => new ProxyDevices.ProxySafetyMonitor(config, state, logger),
                        Models.AlpacaDeviceType.Switch => new ProxyDevices.ProxySwitch(config, state, logger),
                        Models.AlpacaDeviceType.Telescope => new ProxyDevices.ProxyTelescope(config, state, logger),
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
                    var deviceLogger = new TraceLogger($"AlpacaSpy.{safeName}", true);
                    state.DeviceLoggers[config.UniqueId] = deviceLogger;
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
                FileName = $"http://localhost:{port}",
                UseShellExecute = true
            });
        }

        private static Microsoft.Extensions.Logging.LogLevel ToMicrosoftLogLevel(ASCOM.Common.Interfaces.LogLevel logLevel)
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
