# AlpacaSpy – Copilot Instructions

## Project overview

AlpacaSpy is a .NET 10 ASP.NET Core web application intended as a spy/inspector tool for [ASCOM Alpaca](https://ascom-standards.org/api/) astronomy devices. Alpaca is the HTTP-based REST protocol used by modern ASCOM instrument drivers.

## Build & run

```bash
# Build
dotnet build

# Run (dev, http://localhost:5059)
dotnet run --project AlpacaSpy/AlpacaSpy.csproj
```

## Solution structure

```
AlpacaSpy.slnx          # Visual Studio solution (new .slnx format)
AlpacaSpy/
  Program.cs            # App entry point and middleware pipeline
  appsettings.json
  appsettings.Development.json
```

## Project settings

- **Target framework:** `net10.0`
- **Nullable reference types:** enabled — all code must be null-safe
- **ImplicitUsings:** enabled — `System`, `System.Collections.Generic`, `Microsoft.AspNetCore.*`, etc. are available without explicit `using` directives

## NuGet sources

A local package cache is configured at `J:\NuGetPackages`. ASCOM Alpaca packages are available from:
- `https://nuget.cloudsmith.io/ascom/rc/v3/index.json` (ASCOM release candidates)

When adding ASCOM/Alpaca packages, prefer the official ASCOM feed above.

## Reference implementation

**ASCOM Sentinel** (`J:\ascomsentinel\sentinel.slnx`) is the architectural model for AlpacaSpy — mirror its UX, patterns, and conventions. Key files to study:

| File | Role |
|---|---|
| `Sentinel/Program.cs` | Startup, Kestrel port binding, device registration, auto-connect |
| `Sentinel/SentinelLogger.cs` | Logger extending `TraceLogger`, writing to `state.ApplicationLog`, raising `MessageLogChanged` |
| `Sentinel/State.cs` | Singleton state, capacity-capped `StringBuilder` log, `field`-backed properties raising `StateChanged` |
| `Sentinel/Settings.cs` | JSON-persisted settings in `LocalApplicationData`, `ConfigurationChanged` event |
| `Sentinel/AlpacaConfiguration.cs` | `IAlpacaConfiguration` implementation |
| `Sentinel/Pages/Index.razor` | `RadzenTextArea` + 250 ms timer refresh loop + JS auto-scroll |

## Key packages

- `ASCOM.Alpaca.Clients` — communicate with real Alpaca devices (as a client)
- `ASCOM.Alpaca.Razor` — expose proxy Alpaca devices and handle discovery/swagger/auth
- `ASCOM.Tools` — `TraceLogger` for per-device log files
- `Radzen.Blazor` — UI components

All ASCOM packages come from `https://nuget.cloudsmith.io/ascom/rc/v3/index.json`.

## Default port

AlpacaSpy runs on **32325** by default (Sentinel uses 32324). Port is configurable on the Settings page.

## Implementation Rules
- When defining variables, always use the correct variable type, do not use "var".

## Requirements

See `docs/requirements.md` for full feature requirements.
