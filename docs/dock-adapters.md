# Adapter Classes

Dock exposes a small set of helper classes that adapt the core interfaces to different host environments or provide additional runtime services. The built-in model base classes (`DockWindow`, `DockBase`, `DockableBase`) wire these adapters in, so most applications interact with them through `IDock`, `IDockable`, and `IDockWindow`. You can use the adapters directly when implementing custom base types or host integrations.

## HostAdapter

`HostAdapter` implements `IHostAdapter` and bridges a dock window (`IDockWindow`) with a platform-specific host window (`IHostWindow`). It resolves the host window via `IFactory.GetHostWindow` when needed, pushes layout, title, size, and position into the host when presenting, and reads the current size and position back when saving.

Typical usage looks like the following:

```csharp
var hostAdapter = new HostAdapter(dockWindow);
hostAdapter.Present(isDialog: false);
```

The adapter fetches the host window instance from `IFactory.GetHostWindow` if it has not already been assigned. Calling `Save` stores the last known position and dimensions so they can be restored later. `Exit` closes the host and clears the reference, and `SetActive` forwards the activation request to the host window.

## NavigateAdapter

`NavigateAdapter` implements `INavigateAdapter` and provides history-based navigation for docks. It maintains two stacks for back and forward navigation and exposes methods such as `GoBack`, `GoForward`, and `Navigate`. `Navigate` accepts an `IDockable`, a dockable ID (`string`), or `null` to reset navigation.

Create an instance for a given `IDock` and use it to issue navigation commands:

```csharp
var adapter = new NavigateAdapter(rootDock);
adapter.Navigate(document, bSnapshot: true);
```

The adapter cooperates with the factory to activate dockables and can also show or close floating windows via `ShowWindows` and `ExitWindows`. The built-in `DockBase` types already use this adapter to implement `GoBack`, `GoForward`, `Navigate`, and `Close`.

## TrackingAdapter

`TrackingAdapter` stores bounds and pointer positions used when dockables are visible, pinned, or displayed as tabs. It tracks both control-local and screen pointer positions. The docking logic reads these values when calculating drop targets or restoring the layout after a drag operation.

A new adapter contains `NaN` values for all coordinates until you call the various `Set*` methods. `DockableBase` uses it to implement the `IDockable` tracking methods, so most applications interact with `TrackingAdapter` indirectly through the built-in docking controls.

## When to use adapters

The provided adapters give you a ready-made implementation for common behaviors such as window hosting and dock navigation. You can derive from them or implement the corresponding interfaces yourself if your application has special requirements.

For an overview of other Dock concepts see the [documentation index](README.md).
