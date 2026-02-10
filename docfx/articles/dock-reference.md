# Dock API Reference

This reference summarizes the most commonly used classes in Dock. It is based on the samples found in the repository and the public APIs in the `Dock.Model` packages. The MVVM, ReactiveUI and XAML approaches share the same underlying types so the following sections apply to all three.

## Core interfaces

| Type | Description |
| --- | --- |
| `IDockable` | Base interface for items that can be shown in a dock. Provides `Id`, `Title`, `Context`, `DockingState`, optional size limits and lifecycle methods like `OnClose`. |
| `IDock` | Extends `IDockable` with visible dockables (`VisibleDockables`, `ActiveDockable`, `DefaultDockable`, `FocusedDockable`) and navigation commands (`GoBack`, `GoForward`, `Navigate`, `Close`). It also exposes `OpenedDockablesCount`, `IsActive`, `EnableGlobalDocking`, and `CanCloseLastDockable`. |
| `IRootDock` | The top-level container. In addition to the `IDock` members it exposes pinned dock collections and commands to manage windows. |
| `IProportionalDock` | A dock that lays out its children horizontally or vertically using a `Proportion` value. |
| `IStackDock` | Dock based on `StackPanel` with `Orientation` and `Spacing`. |
| `IGridDock` | Dock using `Grid` layout via `ColumnDefinitions` and `RowDefinitions`. |
| `IWrapDock` | Dock built on `WrapPanel` exposing `Orientation`. |
| `IUniformGridDock` | Dock that arranges items in equally sized cells using `Rows` and `Columns`. |
| `IToolDock` / `IDocumentDock` | Specialized docks used for tools and documents. |
| `IDocumentDockFactory` | Adds a `DocumentFactory` delegate used to create documents on demand. |
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

## Docking state flags

`IDockable.DockingState` tracks logical placement using `DockingWindowState` flags:

- `Docked`, `Pinned`, or `Document` describe primary location.
- `Floating` is added when the dockable is hosted in a floating window.
- `Hidden` is added when the dockable is moved to `IRootDock.HiddenDockables`.

Common combinations include `Docked | Floating`, `Pinned | Floating`, and `Document | Floating | Hidden`.

## Document dock options

`IDocumentDock` exposes several key properties for controlling its behavior:

- `EnableWindowDrag` allows the entire window to be dragged via the tab area
- `TabsLayout` chooses where the tabs appear using the `DocumentTabLayout` enum
- `LayoutMode` switches between `Tabbed` and `Mdi` layouts
- `CloseButtonShowMode` controls when document close buttons appear
- `EmptyContent` defines placeholder content shown when a tabbed or MDI document host has no visible dockables
- `CanCreateDocument` and `CreateDocument` control the new-document command
- `CascadeDocuments`, `TileDocumentsHorizontal`, `TileDocumentsVertical`, and `RestoreDocuments` are MDI helpers

The factory provides helper methods `SetDocumentDockTabsLayoutLeft`, `SetDocumentDockTabsLayoutTop` and `SetDocumentDockTabsLayoutRight` to change the layout at runtime.

`IDocumentDockContent` (implemented by the Avalonia model layer) adds `DocumentTemplate` and `CreateDocumentFromTemplate`. `IDocumentDockFactory` exposes a `DocumentFactory` delegate; the built-in document docks in `Dock.Model.Avalonia`, `Dock.Model.Mvvm`, `Dock.Model.ReactiveUI`, `Dock.Model.Prism`, `Dock.Model.Inpc`, `Dock.Model.CaliburMicro`, and `Dock.Model.ReactiveProperty` implement it. `CreateDocument` invokes the delegate when set and adds the resulting document via `AddDocument`. In the Avalonia model, if `DocumentFactory` is not set, `CreateDocument` falls back to `CreateDocumentFromTemplate`.

## ItemsSource dockable generation

`IItemsSourceDock` and `IToolItemsSourceDock` are implemented in `Dock.Model.Avalonia.Controls` to provide automatic source-backed dockables:

- `DocumentDock.ItemsSource` + `DocumentTemplate` generate `Document` dockables.
- `ToolDock.ItemsSource` + `ToolTemplate` generate `Tool` dockables.
- `DocumentDock.ItemContainerGenerator` and `ToolDock.ItemContainerGenerator` accept `IDockItemContainerGenerator` for custom create/prepare/clear pipelines.
- `IsDocumentFromItemsSource(IDockable)` / `IsToolFromItemsSource(IDockable)` report whether the dockable was generated from the bound source.
- `RemoveItemFromSource(object)` removes source items from supported list collections.

When `ItemsSource` is set, Dock automatically creates dockables for each source item through the configured `IDockItemContainerGenerator`. With the default generator, generation requires the corresponding template (`DocumentTemplate` or `ToolTemplate`). The generated `Title` is derived from `Title`, `Name`, or `DisplayName` on the source object and `Context` stores the source object for template bindings.

`DockItemContainerGenerator` is the built-in default implementation. Subclass it or implement `IDockItemContainerGenerator` directly to customize container type, source-to-container mapping, or container cleanup behavior.

Changes to `INotifyCollectionChanged` collections (for example, `ObservableCollection<T>`) automatically add or remove corresponding dockables. When a generated document or tool is closed, the factory attempts to remove the source item from the collection if it implements `IList`.

## Host window options

`HostWindow` exposes `IsToolWindow`, `ToolChromeControlsWholeWindow` and
`DocumentChromeControlsWholeWindow`. These properties toggle pseudo classes on
the window so styles can react to different chrome configurations.

## DockControl properties

`DockControl` is the main Avalonia control that hosts layouts. Common properties include:

| Property | Description |
| --- | --- |
| `Layout` | The root dock layout assigned to the control. |
| `Factory` | Factory used to initialize or manipulate the layout when `Layout.Factory` is `null`. |
| `InitializeFactory` | When `true`, assigns default locators and fallback delegates. |
| `InitializeLayout` | When `true`, calls `InitLayout` on the factory. |
| `DefaultContext` | Fallback context used by `DefaultContextLocator` when `InitializeFactory` is enabled. |
| `AutoCreateDataTemplates` | When `true` (default), injects built-in DataTemplates for dock types. |
| `DragOffsetCalculator` | Controls how the drag preview window is positioned. |
| `IsDraggingDock` | Set while a drag operation is active; useful for styling. |

See [DockControl initialization](dock-control-initialization.md) and [DataTemplates and custom dock types](dock-datatemplates.md) for details.

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

## Tab header and MDI template properties

Dock exposes template properties for tab headers and MDI windows:

- `DocumentControl`: `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate`, `CloseTemplate`, `EmptyContentTemplate`, `CloseButtonTheme`, `IsActive`, `TabsLayout`, `HasVisibleDockables`.
- `ToolControl`: `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate`.
- `MdiDocumentControl`: `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate`, `CloseTemplate`, `EmptyContentTemplate`, `CloseButtonTheme`, `LayoutManager`, `IsActive`, `HasVisibleDocuments`.
- `MdiDocumentWindow`: `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate`, `CloseTemplate`, `CloseButtonTheme`, `DocumentContextMenu`, `IsActive`, `MdiState`.

Use these to customize headers, icons, and modified indicators in styles or templates. See [Styling and theming](dock-styling.md) and [MDI document layout](dock-mdi.md) for details.

## Tab strip properties

The tab strip controls expose additional properties used by the templates:

- `DocumentTabStrip`: `CanCreateItem`, `IsActive`, `EnableWindowDrag`, `Orientation`, `CreateButtonTheme`.
- `ToolTabStrip`: `CanCreateItem`.
- `DocumentTabStripItem`: `IsActive`.
- `ToolPinnedControl`: `Orientation` (propagated to `ToolPinItemControl`).

These properties drive pseudo classes and layout behaviors for tab strips and pinned tool tabs.

## Tool chrome state properties

`ToolChromeControl` exposes state properties that drive its pseudo classes and chrome behavior:

- `Title`, `IsActive`, `IsPinned`, `IsFloating`, `IsMaximized`.

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
var factory = new Dock.Model.Mvvm.Factory();
var root = factory.CreateRootDock();
var docDock = factory.CreateDocumentDock();
root.VisibleDockables = factory.CreateList<IDockable>(docDock);
factory.InitLayout(root);
```

Refer to the factory classes under `samples/DockMvvmSample`, `samples/DockReactiveUISample`, and `samples/DockReactiveUIItemsSourceSample` for practical examples of these methods in use.

## Using the MVVM library

`Dock.Model.Mvvm` provides base classes that implement property change notification via `INotifyPropertyChanged`. Create a custom `Factory` derived from `Dock.Model.Mvvm.Factory` and override `CreateLayout` to build your docks. View models should derive from the MVVM versions of `Dockable`, `Tool` and `Document`.

The MVVM sample demonstrates how to subscribe to factory events and update the UI through commands.

## Using the Caliburn.Micro library

`Dock.Model.CaliburMicro` mirrors the MVVM model but derives from Caliburn.Micro's `PropertyChangedBase`. Use `Dock.Model.CaliburMicro.Factory` alongside `Dock.Model.CaliburMicro.Controls.Document`, `Tool`, and the dock types when you want Caliburn.Micro change tracking. The package also includes a lightweight `RelayCommand` helper for dock commands.

## Using the ReactiveUI library

`Dock.Model.ReactiveUI` exposes the same API surface as the MVVM version but replaces the implementation with ReactiveUI types. Commands are typically created using `ReactiveCommand` and properties use `ReactiveObject` helpers. The layout and event handling is otherwise identical to the MVVM approach.

## Declaring layouts in XAML

The XAML sample shows that a layout can be declared entirely in markup using `DockControl` and the various dock types. Multiple serialization packages are available for persisting these layouts:

- `Dock.Serializer.Newtonsoft` - JSON using Newtonsoft.Json
- `Dock.Serializer.SystemTextJson` - JSON using System.Text.Json  
- `Dock.Serializer.Protobuf` - Binary using protobuf-net
- `Dock.Serializer.Xml` - XML serialization
- `Dock.Serializer.Yaml` - YAML serialization

In XAML you place `RootDock`, `ProportionalDock`, `ToolDock` or `DocumentDock` elements inside a `DockControl`, optionally providing a factory for runtime behavior. The samples register serializer instances through dependency injection so commands can call `Save` and `Load` to persist user changes.

## Further reading

**Sample applications:**
- `samples/DockMvvmSample` – Full MVVM example with commands and data binding
- `samples/DockReactiveUISample` – ReactiveUI variant with observables
- `samples/DockReactiveUIItemsSourceSample` – ReactiveUI sample using both `DocumentDock.ItemsSource` and `ToolDock.ItemsSource`
- `samples/DockReactiveUIWorkspaceSample` – ReactiveUI workspace snapshots and layout locking
- `samples/DockReactiveUIRoutingSample` – Navigation using `IScreen` and `Router`
- `samples/DockReactiveUIDiSample` – ReactiveUI with dependency injection
- `samples/DockReactivePropertySample` – ReactiveProperty framework integration
- `samples/DockInpcSample` – Basic INotifyPropertyChanged implementation
- `samples/DockPrismSample` – Prism framework integration
- `samples/DockXamlSample` – XAML-only layout with serialization
- `samples/DockCodeOnlySample` – Layout defined fully in C#
- `samples/DockCodeOnlyMvvmSample` – Code-only MVVM layout
- `samples/DockFluentCodeOnlySample` – Fluent theme code-only layout
- `samples/DockSimplifiedSample` – Minimal Dock setup
- `samples/DockSimplifiedFluentSample` – Minimal Dock setup with Fluent theme
- `samples/DockControlPanelsSample` – Dock layout panels and controls
- `samples/DockSplitViewSample` – SplitView integration
- `samples/NestedDockSample` – Complex nested layouts
- `samples/Notepad` – Real-world text editor example
- `samples/WebViewSample` – Embedding web content in dockables

For an overview of all guides see the [documentation index](README.md).
