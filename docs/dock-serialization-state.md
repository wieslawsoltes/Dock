# Dock Serialization State Guide

This guide explains how to persist and restore dock state using `DockState`. The [DockXamlSample](../samples/DockXamlSample) shows these steps in a working application.

`DockState` stores the active and focused dockables so that documents and tools regain focus after a layout is loaded.

## Saving the state

Call `Save` with the root `IDock` before serializing the layout:

```csharp
var layout = dockControl.Layout;
if (layout is { })
{
    _dockState.Save(layout);
    await using var stream = File.Create("layout.json");
    _serializer.Save(stream, layout);
}
```

## Restoring the state

After loading the layout, apply the recorded state:

```csharp
await using var stream = File.OpenRead("layout.json");
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dockControl.Layout = layout;
    _dockState.Restore(layout);
}
```

Reset the state with `DockState.Reset()` when starting a new layout or when you no longer need the saved information.

For more details see the [XAML sample project](../samples/DockXamlSample).
