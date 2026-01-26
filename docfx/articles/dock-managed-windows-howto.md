# Managed Windows How-To

This guide walks through enabling managed windows and wiring the managed window layer into your layout.

## Enable managed windows

Set the global flag before creating layouts:

```csharp
using Dock.Settings;

DockSettings.UseManagedWindows = true;
```

Or configure it via `AppBuilder`:

```csharp
using Dock.Settings;

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseManagedWindows();
```

## Ensure the managed layer is present

The built-in Dock themes include a `ManagedWindowLayer` in the `DockControl` template. If you provide a custom template, keep the part and name it `PART_ManagedWindowLayer`.

```xml
<controls:DockControl EnableManagedWindowLayer="True" />
```

Custom template snippet:

```xml
<controls:ManagedWindowLayer x:Name="PART_ManagedWindowLayer"
                             IsVisible="False" />
```

If you disable `EnableManagedWindowLayer`, managed floating windows will not appear.

## Provide a managed host window (optional)

If you override host window creation, return `ManagedHostWindow` when managed windows are enabled:

```csharp
dockControl.HostWindowFactory = () => new ManagedHostWindow();
```

You can also supply a factory mapping through `IFactory.HostWindowLocator` or `IFactory.DefaultHostWindowLocator`.

## Customize managed window layout

`ManagedWindowLayer` uses `ClassicMdiLayoutManager` by default. You can swap it with your own implementation:

```csharp
managedWindowLayer.LayoutManager = new MyMdiLayoutManager();
```

## Drag preview and overlays

Drag previews and dock adorners are rendered inside the managed layer:

```csharp
DockSettings.ShowDockablePreviewOnDrag = true;
DockSettings.DragPreviewOpacity = 0.7;
```

When dragging a managed floating window, the window itself moves so no preview overlay is shown.

## Troubleshooting

- Floating windows do not appear: verify `DockSettings.UseManagedWindows` is `true` and `EnableManagedWindowLayer` is enabled.
- Custom templates: ensure `PART_ManagedWindowLayer` exists in the `DockControl` template.
- Custom host window factory: return `ManagedHostWindow` when managed mode is enabled.
