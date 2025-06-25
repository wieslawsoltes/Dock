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

For a general overview of Dock see the [documentation index](README.md).
