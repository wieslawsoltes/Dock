# Dock Tracking Controls

Dock tracks the UI elements that represent each dockable at runtime.  The `IFactory` interface exposes several collections so applications can look up or manage these controls when they need to manipulate the visual tree directly.

## Why controls are tracked

Dock layouts are described using view models but the framework also creates visual controls for each dockable, window and host.  When advanced scenarios require accessing these visuals—such as customizing the appearance of a tab strip or integrating with a platform windowing API—the factory needs to know which control belongs to which dockable.  The tracking collections provide this mapping.

## Tracking collections

The following properties on `IFactory` keep track of runtime controls.  Each dictionary uses the dockable as the key so it is easy to locate the corresponding control.

| Property | Description |
| -------- | ----------- |
| `VisibleDockableControls` | Mapping of visible dockables to their controls inside a dock. |
| `VisibleRootControls` | Mapping of root dockables to the control hosting them, typically a window. |
| `PinnedDockableControls` | Controls for dockables that are currently pinned. |
| `PinnedRootControls` | Root controls for pinned dockables. |
| `TabDockableControls` | Controls displayed in a tab strip. |
| `TabRootControls` | Root controls for dockables in tab strips. |
| `ToolControls` | Lookup for controls that present tools. |
| `DocumentControls` | Lookup for controls that present documents. |
| `DockControls` | List of `DockControl` instances displaying layouts. |
| `HostWindows` | List of floating windows created by the factory. |

These collections are populated automatically by `FactoryBase` when layouts are initialised.  They can be inspected or modified at runtime if you create custom host controls or windows.

The default Avalonia controls update these dictionaries whenever the `DataContext`
changes. `ToolContentControl` and `DocumentContentControl` subscribe to the
`DataContext` property so the `ToolControls` and `DocumentControls` mappings stay
in sync with the currently visible dockables.

## Typical usage

Most applications rely on these collections indirectly via the factory helpers.  For example `ShowWindows` iterates `HostWindows` to open any floating tools and documents.  You can also retrieve a specific control:

```csharp
var documentControl = factory.VisibleDockableControls[myDocument];
```

When implementing your own visuals or integrating with other frameworks, update the dictionaries accordingly when controls are created or destroyed.  Removing entries prevents stale references after dockables are closed.

For an overview of all guides see the [documentation index](README.md).
