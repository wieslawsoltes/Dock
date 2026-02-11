# Capability Policies and Overrides

Dock supports layered capability resolution for all core interaction flags:

- `CanClose`
- `CanPin`
- `CanFloat`
- `CanDrag`
- `CanDrop`
- `CanDockAsDocument`

This enables global policy defaults, per-dock policy tuning, and per-window exceptions.

## Policy layers and precedence

Effective capability values are resolved in this order:

1. Base dockable flag (`IDockable.Can*`)
2. Root policy (`IRootDock.RootDockCapabilityPolicy`)
3. Dock policy (`IDock.DockCapabilityPolicy`)
4. Dockable override (`IDockable.DockCapabilityOverrides`)

Later layers override earlier layers when a value is set (`bool?` value is not `null`).

`null` means "inherit".

## API surface

- `DockCapabilityPolicy` (`Dock.Model.Core`)
- `DockCapabilityOverrides` (`Dock.Model.Core`)
- `IRootDock.RootDockCapabilityPolicy`
- `IDock.DockCapabilityPolicy`
- `IDockable.DockCapabilityOverrides`
- `DockCapabilityResolver` for explicit evaluation
- `IDockManager.LastCapabilityEvaluation` for diagnostics when validation is blocked by capability policy

## Example

```csharp
var root = factory.CreateRootDock();
root.RootDockCapabilityPolicy = new DockCapabilityPolicy
{
    CanFloat = false
};

var tools = factory.CreateToolDock();
tools.DockCapabilityPolicy = new DockCapabilityPolicy
{
    CanFloat = true
};

var explorer = factory.CreateTool();
explorer.DockCapabilityOverrides = new DockCapabilityOverrides
{
    CanFloat = false
};
```

In this example, floating is:

- globally disabled at root,
- re-enabled for the tool dock,
- disabled again for one specific tool.

## Validation and diagnostics

Dock validation (`IDockManager`) and core factory operations now use effective capability values.

When an operation is rejected by capability policy, `IDockManager.LastCapabilityEvaluation` provides:

- evaluated capability,
- source layer (`Dockable`, `RootPolicy`, `DockPolicy`, `DockableOverride`),
- effective value and a diagnostic message.

## UI behavior

Default Fluent menus and drag entry points use effective capability resolution, so blocked actions are hidden/disabled consistently with runtime validation.

## Sample

`DockXamlReactiveUISample` includes:

- root, tool-dock, and document-dock capability policy controls,
- first generated item capability override controls,
- live testing against items-source generated windows.
