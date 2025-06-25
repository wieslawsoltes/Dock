# Dock FAQ

This page answers common questions that come up when using Dock.

## Focus management

**Why does the active document lose focus when I load a saved layout?**

After deserializing a layout you need to restore the last active and focused dockables. Call `DockState.Restore` with the root dock once the layout has been assigned to `DockControl`:

```csharp
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dock.Layout = layout;
    _dockState.Restore(layout); // restores active and focused dockables
}
```

`DockState` tracks focus changes at runtime and can reapply them after a layout is loaded.

## Serialization pitfalls

**Deserialization fails with unknown types**

`DockSerializer` relies on `DockableLocator` to map identifiers to your view models. Register all custom dockables before loading:

```csharp
ContextLocator = new Dictionary<string, Func<object?>>
{
    ["Document1"] = () => new MyDocument(),
    ["Tool1"] = () => new MyTool(),
};
DockableLocator = new Dictionary<string, Func<IDockable?>>
{
    ["Document1"] = () => new MyDocument(),
    ["Tool1"] = () => new MyTool(),
};
```

If a dockable cannot be resolved the serializer will return `null`.

**What is `DockableLocator` and `ContextLocator`?**

`DockableLocator` is a dictionary of functions that create view models. The
serializer and factory query it using the identifiers stored in a layout to
recreate dockables at runtime. `ContextLocator` works the same way but returns
objects that become the `DataContext` of the views. Populate both dictionaries
when initializing your factory so that Dock can resolve your custom documents
and tools.

## Other questions

**Floating windows appear in the wrong place**

Override `CreateWindowFrom` in your factory to configure new windows when a dockable is floated. This allows you to center windows or set their dimensions.

```csharp
public override IHostWindow CreateWindowFrom(IDockWindow source)
{
    var window = base.CreateWindowFrom(source);
    window.Width = 800;
    window.Height = 600;
    window.Position = new PixelPoint(100, 100);
    return window;
}
```

**Can I cancel switching the active dockable or closing a dock?**

Dock currently raises `ActiveDockableChanged` only *after* the active dockable
has been updated, so the change cannot be cancelled. Likewise there is no
pre-close event for dockables. The only cancellable closing hook is
`WindowClosing`, which is fired when a host window is about to close. Set the
`Cancel` property on the event arguments to keep the window open:

```csharp
factory.WindowClosing += (_, args) =>
{
    if (!CanShutdown())
    {
        args.Cancel = true; // prevents the window from closing
    }
};
```

Cancelling individual dockables is not supported.

For a general overview of Dock see the [documentation index](README.md).
