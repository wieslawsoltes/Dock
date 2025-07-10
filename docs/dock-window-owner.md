# Floating window owner

`DockSettings` exposes an option to make floating windows owned by the main window so they stay in front.

```csharp
// Keep floating windows above the main window
DockSettings.UseOwnerForFloatingWindows = true;
```

```csharp
// Via the app builder
using Dock.Settings;

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseOwnerForFloatingWindows();
```

When enabled Dock sets the main window as the owner for floating windows, preventing them from appearing behind it. Disable this if you want windows to be independent.
