# Dock Serialization State Guide

This guide explains how to persist and restore dock state using `DockState`. The [DockXamlSample](https://github.com/wieslawsoltes/Dock/tree/master/samples/DockXamlSample) shows these steps in a working application.

`DockState` stores tool and document content (and document templates for document docks) so that those values can be restored after a layout is loaded. Active and focused dockables are part of the layout model and are serialized with the layout itself.

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

For more details see the [XAML sample project](https://github.com/wieslawsoltes/Dock/tree/master/samples/DockXamlSample).
