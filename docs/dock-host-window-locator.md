# Host Window Locators

Dock can display floating windows when a dockable is detached from the main layout.
`IFactory` uses *host window locators* to supply the platform window objects
that wrap these floating docks. This guide explains how the `HostWindowLocator`
and `DefaultHostWindowLocator` properties work and shows typical usage patterns.

## Why locators are required

The `HostAdapter` component bridges an `IDockWindow` with an `IHostWindow`
implementation. When a dockable is floated the adapter queries the factory using
`GetHostWindow` to obtain the actual host window instance. Without a locator the
factory cannot create platform windows and floating docks will fail to appear.

Registering locators also allows you to integrate dependency injection or apply
platform specific customisation when new windows are created.

## Providing host windows

`HostWindowLocator` is a dictionary mapping string keys to functions that return
an `IHostWindow`. When `GetHostWindow` is called with a key, the factory invokes
the matching function. A common setup registers a default entry for
`IDockWindow`:

```csharp
HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
{
    [nameof(IDockWindow)] = () => new HostWindow()
};
```

When different window styles are required, add additional keys and choose which
one to pass to `GetHostWindow`.

## Fallback locator

If no entry matches the provided key, `GetHostWindow` calls
`DefaultHostWindowLocator`. Set this delegate to return a generic host window
when the dictionary does not contain a specific mapping:

```csharp
DefaultHostWindowLocator = () => new HostWindow();
```

The fallback ensures floating windows still open even if a key is missing.

## Host window factory

`FactoryBase` automatically falls back to `HostWindowFactory.CreateDefaultHostWindow()`
when no locators are registered. Platform implementations can also expose
`CreateOwnedHostWindow(Window owner)` to return a host bound to an existing
window. `Dock.Avalonia` provides defaults for these helpers.

## Recommended setup

1. Populate `HostWindowLocator` inside `InitLayout` or during application
   startup.
2. Provide a `DefaultHostWindowLocator` so floating windows always have a host.
3. Call `GetHostWindow(id)` from custom code or rely on `HostAdapter`, which
   invokes it automatically when presenting windows.

Using locators in this way keeps window creation centralized in the factory and
makes it easy to customize the hosting behaviour.

For further details on floating windows see the [Floating windows guide](dock-windows.md).
