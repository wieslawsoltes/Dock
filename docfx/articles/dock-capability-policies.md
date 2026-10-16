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

Policies and overrides may remain `null`, including on docks created when floating a document. The built-in themes observe these optional values without logging null-path binding warnings. Replacing or clearing a policy updates the bindings; custom policies implementing `INotifyPropertyChanged` can also notify changes to individual capability properties.

Custom templates can use Avalonia's compiled bindings with the null-conditional `?.` operator alongside the capability converters:

```xml
<MultiBinding xmlns="https://github.com/avaloniaui"
              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
              xmlns:core="using:Dock.Model.Core"
              xmlns:converters="using:Dock.Avalonia.Converters"
              x:DataType="core:IDockable"
              x:CompileBindings="True"
              Converter="{x:Static converters:DockCapabilityConverters.CanDragMultiConverter}">
  <Binding />
  <Binding Path="CanDrag" />
  <Binding Path="DockCapabilityOverrides?.CanDrag" Mode="OneWay" />
  <Binding Path="((core:IDock)Owner)?.DockCapabilityPolicy?.CanDrag" Mode="OneWay" />
</MultiBinding>
```

For a dock data context, bind `DockCapabilityPolicy?.CanDrag` to observe its policy or `ActiveDockable?.DockCapabilityOverrides?.CanDrag` to observe its active dockable's overrides. The compiler validates these paths against the declared `x:DataType`; Avalonia owns their subscriptions and lifetime.

Avalonia 11.3.22 still logs a `Value is null` diagnostic when `Owner` itself is null in the interface-cast path above. Null policies and overrides on an existing owner are handled without warnings. Avalonia 12.1.3 includes the upstream [null-conditional cast fix (#22174)](https://github.com/AvaloniaUI/Avalonia/pull/22174), so a null owner is also warning-free there. Dock leaves genuine binding errors visible rather than suppressing diagnostics.

## Sample

`DockXamlReactiveUISample` includes:

- root, tool-dock, and document-dock capability policy controls,
- first generated item capability override controls,
- live testing against items-source generated windows.
