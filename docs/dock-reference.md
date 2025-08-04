# Dock API Reference

This reference summarizes the most commonly used classes in Dock. It is based on the samples found in the repository and the public APIs in the `Dock.Model` packages. The MVVM, ReactiveUI and XAML approaches share the same underlying types so the following sections apply to all three.

## Core interfaces

| Type | Description |
| --- | --- |
| `IDockable` | Base interface for items that can be shown in a dock. Provides `Id`, `Title`, `Context`, optional size limits and lifecycle methods like `OnClose`. |
| `IDock` | Extends `IDockable` with collections of visible dockables and commands such as `GoBack`, `GoForward`, `Navigate` or `Close`. The `CanCloseLastDockable` flag controls whether the final item may be closed. |
| `IRootDock` | The top level container. In addition to the `IDock` members it exposes pinned dock collections and commands to manage windows. |
| `IProportionalDock` | A dock that lays out its children horizontally or vertically using a `Proportion` value. |
| `IStackDock` | Dock based on `StackPanel` with `Orientation` and `Spacing`. |
| `IGridDock` | Dock using `Grid` layout via `ColumnDefinitions` and `RowDefinitions`. |
| `IWrapDock` | Dock built on `WrapPanel` exposing `Orientation`. |
| `IUniformGridDock` | Dock that arranges items in equally sized cells using `Rows` and `Columns`. |
| `IToolDock` / `IDocumentDock` | Specialized docks used for tools and documents. |
| `ITool` | Represents a tool pane. Tools can specify `MinWidth`, `MaxWidth`, `MinHeight` and `MaxHeight` to control their size. |
| `IProportionalDockSplitter` | Thin splitter placed between proportional docks. Exposes `CanResize` and `ResizePreview` to control dragging. With preview enabled the splitter highlights while dragging. |
| `IGridDockSplitter` | Splitter for `IGridDock` controlling the resize direction. |
| `IDockWindow` / `IHostWindow` | Interfaces representing floating windows created when dockables are detached. |

## Tracking bounds and pointer positions

`IDockable` includes a set of methods used by the docking logic to remember the
position of a dockable and the pointer during drag operations. These methods are
implemented by `DockableBase` which stores the values in a `TrackingAdapter`.
The adapter starts with `NaN` coordinates until a value is set.

- `GetVisibleBounds(out x, out y, out width, out height)` and
  `SetVisibleBounds(x, y, width, height)` return or store the last known bounds
  of the dockable while it is visible in a dock. `OnVisibleBoundsChanged` is
  invoked whenever the values are updated.
- `GetPinnedBounds`/`SetPinnedBounds`/`OnPinnedBoundsChanged` track the
  dimensions used when a tool is pinned to one of the layout edges.
- `GetTabBounds`/`SetTabBounds`/`OnTabBoundsChanged` hold the bounds of a
  dockable displayed as a tab inside another dock.
- `GetPointerPosition`/`SetPointerPosition`/`OnPointerPositionChanged` store the
  pointer coordinates relative to the dock control.
- `GetPointerScreenPosition`/`SetPointerScreenPosition`/
  `OnPointerScreenPositionChanged` do the same using screen coordinates.

These values are consulted when calculating drop targets or restoring a layout
from a saved state.

## Document dock options

`IDocumentDock` exposes several key properties for controlling its behavior:

- `EnableWindowDrag` allows the entire window to be dragged via the tab area
- `TabsLayout` chooses where the tabs appear using the `DocumentTabLayout` enum
- `ItemsSource` enables automatic document creation from data collections (see `IItemsSourceDock`)
- `DocumentTemplate` defines how document content is rendered when using `ItemsSource`

The factory provides helper methods `SetDocumentDockTabsLayoutLeft`, `SetDocumentDockTabsLayoutTop` and `SetDocumentDockTabsLayoutRight` to change the layout at runtime.

To create new documents programmatically, set the `DocumentFactory`
delegate. The `CreateDocument` command invokes this factory and then
adds the returned document via `AddDocument`.

## ItemsSource document generation

`IItemsSourceDock` is implemented by `DocumentDock` in the Avalonia package to provide automatic document management:

- `ItemsSource` - Collection of data objects that become documents
- `IsDocumentFromItemsSource(IDockable)` - Checks if a document was auto-generated
- `RemoveItemFromSource(object)` - Removes an item from the source collection

When `ItemsSource` is set, `DocumentDock` automatically creates `Document` instances for each item in the collection. The document's `Title` is derived from common properties like `Title`, `Name`, or `DisplayName` on the source object. The `Context` property is set to the source item, making it accessible in `DocumentTemplate` bindings.

Changes to `ObservableCollection<T>` automatically add or remove corresponding documents. When a document generated from `ItemsSource` is closed, the factory attempts to remove the source item from the collection if it implements `IList`.

## Host window options

`HostWindow` exposes `IsToolWindow`, `ToolChromeControlsWholeWindow` and
`DocumentChromeControlsWholeWindow`. These properties toggle pseudo classes on
the window so styles can react to different chrome configurations.

## Control customization properties

Several Dock controls expose properties for customizing their context menus, flyouts, and button themes:

### Context Menus and Flyouts

| Control | Property | Type | Description |
|---------|----------|------|-------------|
| `ToolChromeControl` | `ToolFlyout` | `FlyoutBase?` | Custom flyout for the tool chrome grip button |
| `ToolTabStripItem` | `TabContextMenu` | `ContextMenu?` | Custom context menu for tool tab items |
| `DocumentTabStripItem` | `DocumentContextMenu` | `ContextMenu?` | Custom context menu for document tab items |
| `ToolPinItemControl` | `PinContextMenu` | `ContextMenu?` | Custom context menu for pinned tool items |

### Button Themes

| Control | Property | Type | Description |
|---------|----------|------|-------------|
| `DocumentTabStrip` | `CreateButtonTheme` | `ControlTheme?` | Custom theme for the create document button |
| `ToolChromeControl` | `CloseButtonTheme` | `ControlTheme?` | Custom theme for the close button |
| `ToolChromeControl` | `MaximizeButtonTheme` | `ControlTheme?` | Custom theme for the maximize/restore button |
| `ToolChromeControl` | `PinButtonTheme` | `ControlTheme?` | Custom theme for the pin button |
| `ToolChromeControl` | `MenuButtonTheme` | `ControlTheme?` | Custom theme for the menu button |
| `DocumentControl` | `CloseButtonTheme` | `ControlTheme?` | Custom theme for the document close button |

These properties allow you to customize the context menus, flyouts, and button themes for individual control instances. When not set, the controls use their default themes defined in the theme resources. See the [Context menus](dock-context-menus.md) guide for detailed examples.

## Factory API

The `IFactory` interface (implemented by `Factory` in `Dock.Model.Mvvm` and `Dock.Model.ReactiveUI`) contains numerous helpers used by the samples to build and manipulate layouts. Important members include:

- Methods such as `CreateRootDock`, `CreateProportionalDock`, `CreateToolDock` and `CreateDocumentDock` used when constructing the initial layout.
- `CreateLayout()` which returns an `IRootDock` ready to display.
- `InitLayout`, `InitDockable` and `InitDockWindow` for wiring up newly created objects.
- Runtime operations like `AddDockable`, `InsertDockable`, `MoveDockable`, `SwapDockable`, `PinDockable`, `FloatDockable` and the various `Close*` methods.
- Helpers on `DocumentDock` like `AddDocument` and on `ToolDock` like `AddTool` to insert and activate dockables programmatically.
- Events (found in `IFactory.Events`) that signal changes to the layout such as `DockableAdded`, `DockableRemoved`, `WindowOpened` and many more.

A minimal example of creating a layout manually:

```csharp
var factory = new DockFactory();
var root = factory.CreateRootDock();
var docDock = factory.CreateDocumentDock();
root.VisibleDockables = factory.CreateList<IDockable>(docDock);
factory.InitLayout(root);
```

Refer to the factory classes under `samples/DockMvvmSample` and `samples/DockReactiveUISample` for practical examples of these methods in use.

## Using the MVVM library

`Dock.Model.Mvvm` provides base classes that implement property change notification via `INotifyPropertyChanged`. Create a custom `Factory` derived from `Dock.Model.Mvvm.Factory` and override `CreateLayout` to build your docks. View models should derive from the MVVM versions of `Dockable`, `Tool` and `Document`.

The MVVM sample demonstrates how to subscribe to factory events and update the UI through commands.

## Using the ReactiveUI library

`Dock.Model.ReactiveUI` exposes the same API surface as the MVVM version but replaces the implementation with ReactiveUI types. Commands are typically created using `ReactiveCommand` and properties use `ReactiveObject` helpers. The layout and event handling is otherwise identical to the MVVM approach.

## Declaring layouts in XAML

The XAML sample shows that a layout can be declared entirely in markup using `DockControl` and the various dock types. Multiple serialization packages are available for persisting these layouts:

- `Dock.Serializer.Newtonsoft` - JSON using Newtonsoft.Json
- `Dock.Serializer.SystemTextJson` - JSON using System.Text.Json  
- `Dock.Serializer.Protobuf` - Binary using protobuf-net
- `Dock.Serializer.Xml` - XML serialization
- `Dock.Serializer.Yaml` - YAML serialization

In XAML you place `RootDock`, `ProportionalDock`, `ToolDock` or `DocumentDock` elements inside a `DockControl`, optionally providing a factory for runtime behaviour. The samples register serializer instances through dependency injection so commands can call `SaveAsync` and `LoadAsync` to persist user changes.

## Further reading

**Sample applications:**
- `samples/DockMvvmSample` – Full MVVM example with commands and data binding
- `samples/DockReactiveUISample` – ReactiveUI variant with observables
- `samples/DockReactiveUIRoutingSample` – Navigation using `IScreen` and `Router`
- `samples/DockReactiveUIDiSample` – ReactiveUI with dependency injection
- `samples/DockReactivePropertySample` – ReactiveProperty framework integration
- `samples/DockXamlSample` – XAML-only layout with serialization
- `samples/DockCodeOnlySample` – Layout defined fully in C#
- `samples/DockInpcSample` – Basic INotifyPropertyChanged implementation
- `samples/NestedDockSample` – Complex nested layouts
- `samples/Notepad` – Real-world text editor example
- `samples/VisualStudioDemo` – Visual Studio-like interface
- `samples/WebViewSample` – Embedding web content in dockables

For an overview of all guides see the [documentation index](README.md).
