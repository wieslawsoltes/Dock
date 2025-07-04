# DockState Usage Guide

This document explains why `DockState` exists and how to use it when saving and restoring layouts. It builds on the [serialization guide](dock-serialization.md) and provides additional background for managing active and focused dockables.

## Why DockState is needed

`DockState` tracks which document or tool had focus when you saved the layout. Without this information the deserialized layout could display a different dockable than the one the user previously worked on. Persisting the state ensures that the same document regains focus after loading, creating a seamless experience.

The state object also records which dockable was last active in a group. When layouts contain many documents or floating windows, restoring these relationships is important so that navigation history and dock commands behave as expected.

## When to create a DockState

Create one instance of `DockState` for the lifetime of your application. It can be stored in a view model, a service container, or directly in the window code-behind. Reuse the same instance whenever you save or load a layout.

```csharp
private readonly DockState _dockState = new DockState();
```

## Saving the state

Call `DockState.Save` with the root `IDock` before serializing the layout. This captures the active and focused dockables.

```csharp
var layout = dockControl.Layout;
if (layout is not null)
{
    _dockState.Save(layout);
    await using var stream = File.Create("layout.json");
    _serializer.Save(layout, stream);
}
```

## Restoring the state

After loading the layout, assign it to `DockControl.Layout` and call `DockState.Restore` to reapply the focus information.

```csharp
await using var stream = File.OpenRead("layout.json");
var layout = _serializer.Load<IDock?>(stream);
if (layout is not null)
{
    dockControl.Layout = layout;
    _dockState.Restore(layout);
}
```

Call `DockState.Reset()` when you start with a fresh layout or no longer need the saved data. This clears the internal lists so previously focused dockables are forgotten.

```csharp
_dockState.Reset();
```

## Recommendations

- Save the state whenever you persist the layout, typically on application exit or when the user triggers a save command.
- Restore the state immediately after assigning the layout to `DockControl` during application startup.
- Keep the same `DockState` instance around for the entire session so focus changes are recorded continuously.
- Reset the state if the user opens a new layout file or chooses to discard the previous session.

With these practices the docking framework can accurately restore which document or tool was active across runs.

For a working example see the [DockXamlSample](../samples/DockXamlSample) project.

