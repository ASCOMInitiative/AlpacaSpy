using AlpacaSpy.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace AlpacaSpy
{
    public class Settings : IDisposable
    {
        private static LogLevel LOGGING_LEVEL = LogLevel.Information;
        private const int SETTINGS_COMPATIBILTY_VERSION = 1;
        private bool disposedValue;
        private readonly int settingsFileVersion;
        private static readonly JsonSerializerOptions jsonSerialisationOptions;

        static Settings()
        {
            jsonSerialisationOptions = new()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            jsonSerialisationOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public Settings()
        {
            LogMessage(LogLevel.Debug, "Settings() Initiator");
            Status = "Default settings in use.";
        }

        public Settings(string configurationFile)
        {
            LogMessage(LogLevel.Debug, $"Settings(configurationFile) Initiator - {(string.IsNullOrEmpty(configurationFile) ? "Using default file location" : $"Using supplied file location: {configurationFile}")}");
            try
            {
                if (string.IsNullOrEmpty(configurationFile))
                {
                    string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME);
                    SettingsFileName = Path.Combine(folderName, Globals.SETTINGS_FILENAME);
                }
                else
                {
                    SettingsFileName = configurationFile;
                }
                LogMessage(LogLevel.Information, $"Loading settings from file: {SettingsFileName}");

                if (File.Exists(SettingsFileName))
                {
                    LogMessage(LogLevel.Debug, "File exists, about to read it...");
                    string serialisedSettingsString = File.ReadAllText(SettingsFileName);

                    LogMessage(LogLevel.Debug, "Found compatibility version element...");
                    try
                    {
                        using JsonDocument appSettingsDocument = JsonDocument.Parse(serialisedSettingsString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
                        settingsFileVersion = appSettingsDocument.RootElement.GetProperty(nameof(SettingsCompatibilityVersion)).GetInt32();
                        LogMessage(LogLevel.Debug, $"Found settings version: {settingsFileVersion}");

                        switch (settingsFileVersion)
                        {
                            case 1:
                                try
                                {
                                    Settings? settings = JsonSerializer.Deserialize<Settings>(serialisedSettingsString, jsonSerialisationOptions);
                                    if (settings is null)
                                        settings = new Settings();

                                    if (settings.SettingsCompatibilityVersion == SETTINGS_COMPATIBILTY_VERSION)
                                    {
                                        Status = "Settings read OK.";
                                        LogMessage(LogLevel.Information, "Settings read OK");
                                        CopyPropertiesFrom(settings);
                                        EnsureDefaults();
                                    }
                                    else
                                    {
                                        int originalSettingsCompatibilityVersion = settings.SettingsCompatibilityVersion;
                                        try
                                        {
                                            string badVersionSettingsFileName = $"{SettingsFileName}.badversion";
                                            File.Delete(badVersionSettingsFileName);
                                            File.Move(SettingsFileName, badVersionSettingsFileName);
                                            ResetToDefaults();
                                            Status = $"The current settings version: {originalSettingsCompatibilityVersion} does not match the required version: {SETTINGS_COMPATIBILTY_VERSION}. Application settings have been reset to default values and the original settings file renamed to {badVersionSettingsFileName}.";
                                            LogMessage(LogLevel.Warning, Status);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogMessage(LogLevel.Error, $"Error persisting new settings file: {ex.Message}\r\n{ex}");
                                            Status = $"The current settings version:{originalSettingsCompatibilityVersion} does not match the required version: {SETTINGS_COMPATIBILTY_VERSION} but the new settings could not be saved: {ex.Message}.";
                                        }
                                    }
                                }
                                catch (JsonException ex)
                                {
                                    LogMessage(LogLevel.Error, $"Error de-serialising settings file: {ex.Message}\r\n{ex}");
                                    Status = $"There was an error de-serialising the settings file and application default settings are in effect.\r\n\r\nPlease correct the error in the file or use the reset option to save new values.\r\n\r\nJSON parser error message:\r\n{ex.Message}";
                                }
                                catch (Exception ex)
                                {
                                    LogMessage(LogLevel.Error, ex.ToString());
                                    Status = "Exception reading the settings file, default values are in effect.";
                                }
                                break;

                            default:
                                try
                                {
                                    string badVersionSettingsFileName = $"{SettingsFileName}.unknownversion";
                                    File.Delete(badVersionSettingsFileName);
                                    File.Move(SettingsFileName, badVersionSettingsFileName);
                                    ResetToDefaults();
                                    Status = $"An unsupported settings version was found: {settingsFileVersion}. Settings have been reset to defaults and the original settings file has been renamed to {badVersionSettingsFileName}.";
                                    LogMessage(LogLevel.Warning, Status);
                                }
                                catch (Exception ex2)
                                {
                                    LogMessage(LogLevel.Error, $"An unsupported settings version was found: {settingsFileVersion} but an error occurred when saving new settings: {ex2}");
                                    Status = $"An unsupported settings version was found: {settingsFileVersion} but an error occurred when saving new settings: {ex2.Message}.";
                                }
                                break;
                        }
                    }
                    catch (JsonException ex)
                    {
                        LogMessage(LogLevel.Error, $"Error getting settings file version from settings file: {ex.Message}\r\n{ex}");
                        Status = $"An error occurred when reading the settings file version and application default settings are in effect.\r\n\r\nPlease correct the error in the file or use the reset option to create a new settings file.\r\n\r\nJSON parser error message:\r\n{ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        LogMessage(LogLevel.Error, $"Exception parsing the settings file: {ex.Message}\r\n{ex}");
                        Status = $"Exception parsing the settings file: {ex.Message}";
                    }
                }
                else
                {
                    LogMessage(LogLevel.Information, $"Settings file does not exist, initialising new file: {SettingsFileName}");
                    ResetToDefaults();
                    Status = "First time use - configuration set to default values.";
                }
            }
            catch (Exception ex)
            {
                LogMessage(LogLevel.Error, $"Load settings exception: {ex.Message}\r\n{ex}");
                Status = "Unexpected exception reading the settings file, default values are in use.";
            }
        }

        public List<ConfiguredDevice> ConfiguredDevices { get; set; } = new();
        public bool LogDiscoveryMessages { get; set; } = true;
        public bool StartBrowserOnLaunch { get; set; } = true;
        public double AlpacaDiscoveryDuration { get; set; } = Globals.ALPACA_DISCOVERY_DURATION_SECONDS;
        public int AlpacaGetPropertyTimeout { get; set; } = 2;
        public bool AutoConnect { get; set; } = true;
        public int AlpacaConnectTimeout { get; set; } = 10;
        public bool IncludeAlpacaTrace { get; set; } = false;
        public int SettingsCompatibilityVersion { get; set; } = SETTINGS_COMPATIBILTY_VERSION;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public string Location { get; set; } = "My Observatory";
        public ushort ServerPort { get; set; } = (ushort)Globals.DEFAULT_ALPACA_PORT;
        public bool BindToAllNetworkAddresses { get; set; } = true;
        public bool AllowDiscovery { get; set; } = true;
        public bool DiscoveryResponseOnlyOnLocalHost { get; set; } = true;
        public bool RunInStrictAlpacaMode { get; set; } = true;

        public void ResetToDefaults()
        {
            try
            {
                Settings defaults = new Settings();
                string serialisedSettings = JsonSerializer.Serialize(defaults, jsonSerialisationOptions);
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName) ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME));
                File.WriteAllText(SettingsFileName, serialisedSettings);
                CopyPropertiesFrom(defaults);
                EnsureDefaults();
                RaiseChangeEvent();
                Status = $"Settings reset at {DateTime.Now:HH:mm:ss}.";
            }
            catch (Exception ex)
            {
                LogMessage(LogLevel.Error, $"ResetToDefaults - Exception during Reset: {ex.Message}\r\n{ex}");
                throw;
            }
        }

        public void Save()
        {
            LogMessage(LogLevel.Debug, "Saving settings to settings file");
            EnsureDefaults();
            bool saved = PersistSettings();
            Status = saved
                ? $"Settings saved at {DateTime.Now:HH:mm:ss}."
                : $"ERROR: Settings could not be saved at {DateTime.Now:HH:mm:ss}.";
            RaiseChangeEvent();
        }

        public void RaiseChangeEvent()
        {
            if (ConfigurationChanged is not null)
            {
                try
                {
                    EventArgs args = new();
                    LogMessage(LogLevel.Debug, "Save settings - About to call configuration changed event handler");
                    ConfigurationChanged(this, args);
                    LogMessage(LogLevel.Debug, "Save settings - Returned from configuration changed event handler");
                }
                catch (Exception ex)
                {
                    LogMessage(LogLevel.Debug, $"RaiseChangeEvent - Exception during event handling: {ex.Message}\r\n{ex}");
                }
            }
        }

        internal string SettingsFileName { get; private set; } = string.Empty;
        internal string Status { get; private set; } = string.Empty;

        public event EventHandler? ConfigurationChanged;

        public void LogMessage(LogLevel logLevel, string message)
        {
            try
            {
                if (logLevel >= LOGGING_LEVEL)
                {
                    lock (Globals.writeLogLock)
                    {
                        Console.Write($"{DateTime.Now:HH:mm:ss.fff} ");
                        var originalColour = Console.ForegroundColor;
                        switch (logLevel)
                        {
                            case LogLevel.Debug:
                                Console.ForegroundColor = ConsoleColor.Blue;
                                break;
                            case LogLevel.Information:
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                break;
                            case LogLevel.Warning:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            case LogLevel.Error:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                        }

                        Console.Write($"{logLevel,-13} ");
                        Console.ForegroundColor = originalColour;
                        Console.WriteLine(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Settings.LogMessage Exception: {ex.Message}\r\n{ex}");
            }
        }

        private void CopyPropertiesFrom(Settings source)
        {
            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                if (property.Name == nameof(SettingsFileName) || property.Name == nameof(Status))
                    continue;

                LogMessage(LogLevel.Debug, $"CopyPropertiesFrom - {property.Name} = {property.GetValue(source) ?? "null"}");
                try
                {
                    property.SetValue(this, property.GetValue(source));
                }
                catch
                {
                }
            }
        }

        private bool PersistSettings()
        {
            try
            {
                SettingsCompatibilityVersion = SETTINGS_COMPATIBILTY_VERSION;
                LogMessage(LogLevel.Debug, $"PersistSettings - Settings file: {SettingsFileName}");
                string serialisedSettingsString = JsonSerializer.Serialize(this, jsonSerialisationOptions);
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName) ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME));
                LogMessage(LogLevel.Debug, $"PersistSettings - Created directory. Writing to {SettingsFileName}");
                File.WriteAllText(SettingsFileName, serialisedSettingsString);
                return true;
            }
            catch (Exception ex)
            {
                LogMessage(LogLevel.Error, $"PersistSettings exception: {ex.Message}\r\n{ex}");
                return false;
            }
        }

        private void EnsureDefaults()
        {
            ConfiguredDevices ??= new List<ConfiguredDevice>();
            Location ??= "My Observatory";
            if (ServerPort == 0)
                ServerPort = (ushort)Globals.DEFAULT_ALPACA_PORT;
            if (AlpacaDiscoveryDuration <= 0)
                AlpacaDiscoveryDuration = Globals.ALPACA_DISCOVERY_DURATION_SECONDS;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LogMessage(LogLevel.Debug, "Settings.Dispose()...");
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
