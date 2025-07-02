# Adapter Classes

Dock exposes a small set of helper classes that adapt the core interfaces to different host environments or provide additional runtime services. These adapters are mainly used by the sample applications but are part of the public API and can be reused in your own projects.

## HostAdapter

`HostAdapter` implements `IHostAdapter` and bridges a dock window (`IDockWindow`) with a platform specific host window. It is responsible for presenting the floating window, updating its size and location and closing it when requested.

Typical usage looks like the following:

```csharp
var hostAdapter = new HostAdapter(dockWindow);
window.Host = hostAdapter;
hostAdapter.Present(isDialog: false);
```

The adapter fetches the window instance from `Factory.GetHostWindow` if it has not already been assigned. Calling `Save` stores the last known position and dimensions so that they can be restored later.

## NavigateAdapter

`NavigateAdapter` implements `INavigateAdapter` and provides history based navigation for docks. It maintains two stacks for back and forward navigation and exposes methods such as `GoBack`, `GoForward` and `Navigate`.

Create an instance for a given `IDock` and use it to issue navigation commands:

```csharp
var adapter = new NavigateAdapter(rootDock);
adapter.Navigate(document, bSnapshot: true);
```

The adapter cooperates with the factory to activate dockables and can also show or close floating windows via `ShowWindows` and `ExitWindows`.

## TrackingAdapter

`TrackingAdapter` stores bounds and pointer positions used when tools are pinned, visible or displayed as tabs. The docking logic reads these values when calculating drop targets or restoring the layout after a drag operation.

A new adapter contains `NaN` values for all coordinates until you call the various `Set*` methods. Most applications interact with `TrackingAdapter` indirectly through the built in docking controls.

## When to use adapters

The provided adapters give you a ready made implementation for common behaviours such as window hosting and dock navigation. You can derive from them or implement the corresponding interfaces yourself if your application has special requirements.

For an overview of other Dock concepts see the [documentation index](README.md).
