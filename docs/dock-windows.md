# Floating Windows

Dock can detach dockables into separate floating windows. These windows are represented by the `IDockWindow` interface and hosted using `IHostWindow` implementations.

## Creating windows

`FactoryBase` exposes a `CreateWindowFrom` method which returns an `IHostWindow`. Override this method on your factory to customize the platform window used for floating docks:

```csharp
public override IHostWindow CreateWindowFrom(IDockWindow source)
{
    var window = base.CreateWindowFrom(source);
    window.Title = $"Dock - {source.Id}";
    return window;
}
```

Calling `FloatDockable` on the factory opens a dockable in a new window. The returned `IDockWindow` stores its bounds and title and can be serialized together with the layout.

## Creating floating windows programmatically

To create a new floating window with custom content that can be docked back into the main window:

### Option 1: Using function-based content (Recommended for non-MVVM)

```csharp
public void CreateNewFloatingUtility(UserControl userControl, IRootDock rootDock)
{
    // Create a Tool with UserControl content using a function
    var tool = new Tool
    {
        Id = Guid.NewGuid().ToString(),
        Title = "Floating Utility",
        CanClose = true,
        // Wrap the UserControl in a function that returns it
        Content = new Func<IServiceProvider, object>(_ => userControl)
    };
    
    // Float the tool to create a new window
    _factory?.FloatDockable(tool);
}
```

### Option 2: Using ViewModel pattern (Recommended for MVVM)

```csharp
// Create a ViewModel that inherits from Tool
public class MyUtilityViewModel : Tool
{
    public MyUtilityViewModel()
    {
        Id = "MyUtility";
        Title = "My Utility";
        CanClose = true;
    }
    
    // Add your properties here
}

// Register a DataTemplate in App.axaml
// <DataTemplate DataType="{x:Type local:MyUtilityViewModel}">
//   <local:MyUtilityView />
// </DataTemplate>

// Create and float the utility
public void CreateFloatingUtility(IRootDock rootDock)
{
    var utility = new MyUtilityViewModel();
    _factory?.FloatDockable(utility);
}
```

### Option 3: Float existing tools from a ToolDock

If you want to float a tool that's part of an existing ToolDock:

```csharp
public void FloatExistingTool(ITool tool)
{
    _factory?.FloatDockable(tool);
}
```

### Important Notes

- When using `Content = new Func<IServiceProvider, object>(_ => control)`, the function should return a Control instance. The framework will automatically wrap it properly.
- Floated dockables can be docked back into the main window by dragging them over valid drop targets.
- The factory automatically creates a proper window structure with a RootDock and appropriate ToolDock/DocumentDock container based on the dockable type.
- For best results with floating, ensure your RootDock is properly initialized and attached to a DockControl.

## Window chrome options

`HostWindow` provides two boolean properties that control how its chrome behaves:

- `ToolChromeControlsWholeWindow`
- `DocumentChromeControlsWholeWindow`

When enabled these toggle the `:toolchromecontrolswindow` or
`:documentchromecontrolswindow` pseudo class respectively. Styles can use these
states to remove the standard system chrome and let the Dock chrome occupy the
entire window.

## Presenting and closing

`IDockWindow` exposes `Present(bool isDialog)` to show the window and `Exit()` to close it programmatically. The default implementation uses `HostAdapter` to bridge between the interface and the underlying Avalonia `Window`.

Use the `WindowOpened` and `WindowClosed` events on the factory to track when floating windows appear or disappear.

```csharp
factory.WindowClosed += (_, e) =>
    Console.WriteLine($"Closed {e.Window?.Title}");
```

## Window magnetism

Floating windows can optionally snap to the edges of other floating windows while being dragged.
This behaviour is controlled by two settings on `DockSettings`:

- `EnableWindowMagnetism` enables or disables the feature.
- `WindowMagnetDistance` specifies how close windows must be before they snap together.

When magnetism is enabled, `HostWindow` compares its position against other tracked windows during a drag
and adjusts the position if it falls within the snap distance. This makes it easy to align multiple floating
windows.

## Bringing windows to front

If `DockSettings.BringWindowsToFrontOnDrag` is enabled, initiating a drag will activate
all floating windows and any main window hosting a `DockControl` so they stay above other
applications until the drag completes.

For more advanced scenarios see [Adapter Classes](dock-adapters.md) and the [Advanced Guide](dock-advanced.md).
