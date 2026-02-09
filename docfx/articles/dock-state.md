# DockState Usage Guide

This document explains why `DockState` exists and how to use it when saving and restoring layouts. It builds on the [serialization guide](dock-serialization.md) and provides additional background for restoring document/tool content and document templates after loading layouts.

## Why DockState is needed

`DockState` captures values that are not serialized with the layout itself, such as `IToolContent.Content`, `IDocumentContent.Content`, and `IDocumentDockContent.DocumentTemplate`. These properties often reference controls or templates that are intentionally ignored by serializers. Without `DockState`, documents or tools can appear with missing content after a layout is loaded.

Active and focused dockables are part of the layout model and are serialized directly, so `DockState` is not required for focus restoration. Dockable logical placement (`IDockable.DockingState`) is also part of the layout model and is updated by factory operations.

## When to create a DockState

Create a `DockState` instance for as long as you need to preserve content/templates between `Save` and `Restore` calls. It can be stored in a view model, a service container, or directly in the window code-behind. Reuse the same instance whenever you save or load a layout within the same session.

```csharp
private readonly DockState _dockState = new DockState();
```

## Saving the state

Call `DockState.Save` with the root `IDock` before serializing the layout. This captures document/tool content and document templates so they can be reapplied later.

```csharp
var layout = dockControl.Layout;
if (layout is not null)
{
    _dockState.Save(layout);
    await using var stream = File.Create("layout.json");
    _serializer.Save(stream, layout);
}
```

## Restoring the state

After loading the layout, assign it to `DockControl.Layout` and call `DockState.Restore` to reapply content and templates.

```csharp
await using var stream = File.OpenRead("layout.json");
var layout = _serializer.Load<IDock?>(stream);
if (layout is not null)
{
    dockControl.Layout = layout;
    _dockState.Restore(layout);
}
```

Call `DockState.Reset()` when you start with a fresh layout or no longer need the cached content/templates. This clears the internal dictionaries so old values are forgotten.

```csharp
_dockState.Reset();
```

## Recommendations

- Save the state whenever you persist the layout, typically on application exit or when the user triggers a save command.
- Restore the state immediately after assigning the layout to `DockControl` during application startup.
- Keep the same `DockState` instance around while you need to restore cached content or templates.
- Reset the state if the user opens a new layout file or chooses to discard cached content.

With these practices the docking framework can restore non-serialized content and templates after layouts are reloaded.

For a working example see the [DockXamlSample](https://github.com/wieslawsoltes/Dock/tree/master/samples/DockXamlSample) project.
