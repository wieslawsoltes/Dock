# Floating window owner

`DockSettings` exposes options to make floating windows owned by the main window so they stay in front. This is the default when `IDockWindow.OwnerMode` is `DockWindowOwnerMode.Default`.

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

## Global owner policy

Use `DockSettings.FloatingWindowOwnerPolicy` to explicitly control global ownership behavior. When set to `Default`, Dock falls back to `UseOwnerForFloatingWindows` for backward compatibility.

```csharp
DockSettings.FloatingWindowOwnerPolicy = DockFloatingWindowOwnerPolicy.NeverOwned;
```

## Per-window ownership

Use `DockWindowOwnerMode` and `ParentWindow` to explicitly control ownership for a specific window. This overrides the global setting.

```csharp
var options = new DockWindowOptions
{
    OwnerMode = DockWindowOwnerMode.ParentWindow,
    ParentWindow = root.Window
};

factory.FloatDockable(tool, options);
```

To force a floating window to have no owner even when `UseOwnerForFloatingWindows` is enabled:

```csharp
var options = new DockWindowOptions
{
    OwnerMode = DockWindowOwnerMode.None
};

factory.FloatDockable(tool, options);
```

To always use the root window as the owner:

```csharp
var options = new DockWindowOptions
{
    OwnerMode = DockWindowOwnerMode.RootWindow
};

factory.FloatDockable(tool, options);
```

To use the window that currently hosts the dockable being floated:

```csharp
var options = new DockWindowOptions
{
    OwnerMode = DockWindowOwnerMode.DockableWindow
};

factory.FloatDockable(tool, options);
```

Managed windows do not use OS-level owners, so this setting applies to native floating windows only.
