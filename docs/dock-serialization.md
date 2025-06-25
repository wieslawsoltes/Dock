# Dock Serialization and Persistence

This guide expands on the FAQ and shows how layouts can be serialized and restored using `DockSerializer`.

`DockSerializer` lives in the `Dock.Serializer` package and works together with `DockState` to track focus changes. The code snippets below use asynchronous file APIs but any `Stream` works.

## Saving a layout

Persist the current layout when the application closes so the user can continue where they left off.

```csharp
await using var stream = File.Create("layout.json");
_serializer.Save(dockControl.Layout!, stream);
```

`dockControl.Layout` must reference the current `IDock` root. The serializer writes JSON to the stream.

## Loading a layout

When starting the application, load the previously saved layout and restore the last active and focused dockables.

```csharp
await using var stream = File.OpenRead("layout.json");
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dockControl.Layout = layout;
    _dockState.Restore(layout); // reapply focus information
}
```

Before calling `Load` ensure that custom dockables are registered with `DockableLocator` and `ContextLocator`. Unknown identifiers return `null`.

## Typical pattern

1. Register view models with `DockableLocator` at startup.
2. On application exit, save `dockControl.Layout` with `DockSerializer`.
3. On the next run, load the layout and call `_dockState.Restore`.
4. Handle the case where the layout file does not exist or fails to deserialize.

Following this pattern keeps the window arrangement consistent across sessions.

For an overview of all guides see the [documentation index](README.md).
