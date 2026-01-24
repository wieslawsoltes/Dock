# Workspace Profiles

Workspace profiles let you capture, name, and restore layout snapshots without wiring serialization yourself. This is useful for pro-app scenarios (IDE layouts, drawing workspaces, role-based layouts) where users switch between saved layouts quickly.

## API overview

- `DockWorkspaceManager` manages named workspaces.
- `DockWorkspace` stores the serialized layout plus optional `DockState` for in-memory restores.
- `DockWorkspaceManager.Capture` serializes the current layout and (optionally) captures dock state.
- `DockWorkspaceManager.Restore` deserializes the layout and re-applies the saved state snapshot.

## Capture a workspace

```csharp
var serializer = new DockSerializer();
var workspaceManager = new DockWorkspaceManager(serializer);

if (dockControl.Layout is IDock layout)
{
    var workspace = workspaceManager.Capture(
        id: "editing",
        layout: layout,
        includeState: true,
        name: "Editing",
        description: "Editing layout with tool windows");
}
```

## Restore a workspace

When you restore a layout, initialize it using your factory (or let `DockControl.InitializeLayout` do it).

```csharp
var restored = workspaceManager.Restore(workspace);
if (restored is not null)
{
    factory.InitLayout(restored);
    dockControl.Layout = restored;
}
```

## Persisting to disk

`DockWorkspace.Layout` is the serialized layout string. You can store it alongside the workspace id/name in your own settings store. If you use `includeState`, the `DockState` snapshot is intended for in-memory switching during the same app session and should not be serialized.

## Notes

- `DockState` captures content references (documents/tools) so it can restore them after deserialization.
- To reset to defaults, keep a baseline workspace captured at startup and restore it as needed.

