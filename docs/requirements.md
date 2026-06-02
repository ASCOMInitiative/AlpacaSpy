# AlpacaSpy – Requirements

## Purpose

AlpacaSpy is a developer debugging tool that acts as a **man-in-the-middle proxy** between an ASCOM Alpaca client and one or more real Alpaca devices. It logs all HTTP traffic passing through so developers can inspect the full Alpaca protocol exchange.

---

## Architecture & Technology

AlpacaSpy follows the same architecture as **ASCOM Sentinel** (`j:\ascomsentinel\sentinel.slnx`), which must be used as the model for both UX appearance and technical implementation.

| Concern | Technology |
|---|---|
| UI framework | Server-side Blazor with **Radzen** components |
| Alpaca client (talk to real devices) | `ASCOM.Alpaca.Clients` NuGet package |
| Alpaca server (expose proxy devices) | `ASCOM.Alpaca.Razor` NuGet package |
| Per-device log files | `ASCOM.Tools.TraceLogger` (via `ASCOM.Tools` NuGet package) |
| App-level logging | Custom logger class extending `TraceLogger`, same pattern as `SentinelLogger` |
| Settings persistence | JSON file in `LocalApplicationData`, same pattern as Sentinel `Settings.cs` |
| Runtime state | Singleton `State` class with `StringBuilder ApplicationLog` and `StateChanged` event, same pattern as Sentinel `State.cs` |
| Alpaca configuration | `IAlpacaConfiguration` implementation, same pattern as Sentinel `AlpacaConfiguration.cs` |

### Key Sentinel patterns to replicate
- `Program.cs` — Kestrel startup, port binding, `DeviceManager.LoadXxx()`, `ASCOM.Alpaca.Razor.StartupHelpers.*`, auto-connect via `applicationLifetime.ApplicationStarted`
- `SentinelLogger` — extends `TraceLogger`, writes to `state.ApplicationLog` (a capacity-capped `StringBuilder`), raises `MessageLogChanged` event, logs are colour-coded to console by `LogLevel`
- `State` — singleton, properties use `field`-backed setters that call `RaiseChangeEvent()`; `ApplicationLog` is a capacity-capped `StringBuilder`
- `Settings` — JSON-serialised to `LocalApplicationData`, loaded at startup, saved on change, raises `ConfigurationChanged` event
- `Index.razor` — `RadzenTextArea` bound to `state.ApplicationLog`, timer-based refresh loop (`LOG_REFRESH_INTERVAL = 250 ms`), auto-scroll via JS interop

---

## Network Port

- Default operating port: **32325** (avoids conflict with Sentinel on 32324).
- The port must be configurable on the **Settings** page.

---

## Device Discovery & Re-advertisement

- AlpacaSpy must discover available Alpaca devices using the standard **Alpaca UDP Discovery** mechanism.
- Up to **10 devices** may be selected from discovered devices. The architecture must make this limit easily expandable.
- Selected devices may be of **any Alpaca device type** (Telescope, Camera, Focuser, etc.).
- AlpacaSpy must **re-advertise** all configured devices via Alpaca Discovery (handled automatically by `ASCOM.Alpaca.Razor` `StartupHelpers.ConfigureDiscovery()`).
- Re-advertised device names must match the source device names with the suffix ` (AlpacaSpy)` appended.

---

## Proxy Behaviour

- From the perspective of **Alpaca clients**: AlpacaSpy appears as an Alpaca device server hosting the configured proxy devices (implemented using `ASCOM.Alpaca.Razor`).
- From the perspective of **configured devices**: AlpacaSpy appears as an Alpaca client (implemented using `ASCOM.Alpaca.Clients`).
- All Alpaca API calls supported by the source device must be **passed through transparently** — no functionality must be altered.

---

## Logging

Each configured device has its own **`TraceLogger`** log file instance (via `ASCOM.Tools`).

### Logged from the client (inbound request)
1. HTTP headers
2. Query string and body parameters
3. JSON payload

### Logged from the device (outbound response)
1. HTTP headers
2. JSON payload

Each of the above five categories must be **individually configurable on/off per configured device** through the Settings page.

---

## Real-time Display

- All logged traffic must appear in a **`RadzenTextArea` on the Index page in real time**, using the same timer-based refresh loop pattern as Sentinel (`Index.razor`).

---

## Connection Management

- A **Connect / Disconnect button** on the Index page connects to or disconnects from all configured devices.
- A **Settings option** (`AutoConnect`) causes AlpacaSpy to automatically connect to configured devices on startup (triggered via `applicationLifetime.ApplicationStarted`, same as Sentinel).

## Selective Logging Feature
- The settings page must allow users to enable or disable logging of each member of the selected device's ASCOM device interface 
(e.g., for a telescope: `RightAscension`, `Declination`, `SlewToCoordinates()`, etc.). This should be implemented as a list of checkboxes generated dynamically 
based on the device's supported interface members, allowing granular control over what is logged for each device.
- The logging configuration for each device must be persisted in the settings JSON file and applied in real time without requiring a restart.
- Update the alpacaproxymiddleware.cs class to check the logging configuration before logging each API call, ensuring that only enabled members are logged for each device.