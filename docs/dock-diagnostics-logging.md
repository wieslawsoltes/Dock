# Diagnostics Logging

Dock includes a lightweight diagnostics logger in `Dock.Settings.DockLogger`. It is disabled by default and writes messages only when diagnostics logging is enabled.

## Enable logging

Enable diagnostics logging globally:

```csharp
using Dock.Settings;

DockSettings.EnableDiagnosticsLogging = true;
```

## Capture log output

By default, Dock writes to `Debug.WriteLine`. You can intercept messages by setting `DockSettings.DiagnosticsLogHandler`:

```csharp
DockSettings.DiagnosticsLogHandler = message => Console.WriteLine(message);
```

This handler is invoked for every diagnostics message while logging is enabled.

## Logging from custom code

Use `DockLogger` to emit messages that follow Dock's formatting:

```csharp
using Dock.Settings;

DockLogger.Log("Layout", "Layout initialized");
DockLogger.LogDebug("Docking", "Validated dock operation");
```

Both `Log` and `LogDebug` respect `DockSettings.EnableDiagnosticsLogging`. If logging is disabled, they do nothing.

## Recommended usage

- Enable logging temporarily while diagnosing docking behavior.
- Provide a handler in debug builds to route messages to your preferred sink.
- Leave logging disabled in production unless you need detailed diagnostics.

For related settings see [Dock settings](dock-settings.md).
