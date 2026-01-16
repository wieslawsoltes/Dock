# Model Control Interfaces

Dock ships with a small set of interfaces under `Dock.Model.Controls`.
These contracts describe the view models used to build a layout.
The interfaces are implemented by the MVVM and ReactiveUI libraries
and are referenced from the samples throughout this repository.

This page explains why each interface exists and how it should be used
when creating your own docks and documents.

## Overview

| Interface | Purpose |
| --- | --- |
| `IDockDock` | Basic dock panel that can optionally fill the remaining space with the last child. |
| `IDocument` | Represents a document item. Used for files or editor tabs. |
| `IMdiDocument` | Stores MDI window bounds, state, and stacking order for classic MDI layouts. |
| `IDocumentContent` | Document containing arbitrary `Content`. |
| `IDocumentDock` | Dock that hosts documents and exposes commands to create them. |
| `IDocumentDockContent` | Dock that creates documents from a `DocumentTemplate`. |
| `IDocumentTemplate` | Template object used when creating new documents on demand. |
| `IProportionalDock` | Dock that arranges children vertically or horizontally using proportions. |
| `IStackDock` | Dock based on `StackPanel` with `Orientation` and `Spacing`. |
| `IGridDock` | Dock that uses `Grid` layout defined by `ColumnDefinitions` and `RowDefinitions`. |
| `IWrapDock` | Dock built on `WrapPanel` exposing `Orientation`. |
| `IUniformGridDock` | Dock with equally sized cells configured by `Rows` and `Columns`. |
| `ISplitViewDock` | Dock that hosts a collapsible pane and a main content area. |
| `IProportionalDockSplitter` | Splitter element between proportional dock children. |
| `IGridDockSplitter` | Splitter used inside a grid dock to resize rows or columns. |
| `IRootDock` | Top level container responsible for pinned docks and windows. |
| `ITool` | Basic interface for tool panes such as explorers or output views. |
| `IToolContent` | Tool containing arbitrary `Content`. |
| `IToolDock` | Dock that hosts tools and supports auto-hide behavior. |

The following sections provide guidelines on applying these contracts in your projects.

## IDockDock

Use `IDockDock` for dock panels that hold a collection of child dockables.
The `LastChildFill` property mirrors the behavior of Avalonia's `DockPanel`.
Set it to `true` when the final child should consume all remaining space.
Typically this is combined with one or more `ProportionalDock` instances
that split the available area.

## IDocument and IDocumentContent

`IDocument` marks a dockable as a document. Documents are usually displayed
in an `IDocumentDock` and can be closed independently of tools. When a
document needs to expose additional data, implement `IDocumentContent` and
provide the object through its `Content` property. The MVVM and ReactiveUI
libraries include base classes that implement these interfaces and also
raise change notifications.

## IMdiDocument

`IMdiDocument` stores classic MDI window state for documents when an
`IDocumentDock` is configured to use an MDI layout. It exposes window bounds,
minimized or maximized state, and a Z-order index so documents can be arranged
and restored consistently.

## IDocumentDock and IDocumentDockContent

`IDocumentDock` is a specialized dock that maintains a tab strip of
documents. It contains optional commands for creating new documents
and allows dragging the host window via the tab area when
`EnableWindowDrag` is `true`. `TabsLayout` determines where the tabs
are placed.

`IDocumentDock.LayoutMode` switches between tabbed documents and classic MDI
windows. When MDI mode is enabled the dock exposes commands for cascade and
tile operations, and documents implement `IMdiDocument` to store window state.

`DocumentDock` also exposes a `DocumentFactory` delegate that is used by
the `CreateDocument` command. When assigned, this factory is invoked to
create a new document which is then added and activated through the
`AddDocument` helper.

`IDocumentDockContent` (implemented by the Avalonia model layer) extends this
concept by storing a `DocumentTemplate` object. `CreateDocumentFromTemplate`
typically creates a new document and sets its `IDocumentContent.Content` from
the template (for example `DocumentTemplate.Content` in the Avalonia
implementation). This is useful for "New" commands or ItemsSource-generated
documents where the view is defined in XAML.

## IDocumentTemplate

`IDocumentTemplate` represents template content used to build document views. In
Avalonia the built-in `DocumentTemplate` stores XAML content (or a
`Func<IServiceProvider, object>`) that becomes the document `Content` when a new
document is created.

## IProportionalDock and IProportionalDockSplitter

Proportional docks arrange their children either horizontally or
vertically. Each dockable specifies a `Proportion` value which is
interpreted relative to the other siblings. Insert an
`IProportionalDockSplitter` between dockables to allow the user to
resize the areas at runtime. The splitter exposes `CanResize`
which can disable dragging for fixed layouts. When `ResizePreview` is
true the splitter previews the drag and applies the size changes once
the pointer is released. The splitter is highlighted while dragging.

## ISplitViewDock

`ISplitViewDock` models a dock with two regions: a collapsible pane and a main content area. It mirrors Avalonia's `SplitView` behavior and exposes properties such as `DisplayMode`, `PanePlacement`, `IsPaneOpen`, `OpenPaneLength`, and `CompactPaneLength`. Use `PaneDockable` for the pane content and `ContentDockable` for the main content area.

## IRootDock

The root dock owns the entire layout including pinned tools and
floating windows. It exposes collections for hidden or pinned
dockables as well as commands to show or exit windows. Implement
`IRootDock` on your main view model so that the factory can create
windows and restore the layout state.

## ITool and IToolContent

Tools are dockables that provide auxiliary functionality like
solution explorers or output panes. Implement `ITool` for the view
model and optionally `IToolContent` when a separate content object
is required. Tools are typically hosted inside `IToolDock` panels
and can be pinned to the sides of the layout.

## IToolDock

`IToolDock` represents a panel that manages tool dockables. It can
auto-hide its content and exposes an `Alignment` property for
positioning. When `AutoHide` is enabled the dock collapses once the
pointer leaves the area. `GripMode` controls if the user can resize
the dock by dragging its border.

## Putting it all together

When building a layout you normally start with an `IRootDock` and add
`IToolDock`, `IDocumentDock` and `IProportionalDock` instances inside
it. Documents and tools implement `IDocument` or `ITool` and can store
extra data through the corresponding `*Content` interfaces.

Factory classes found in `Dock.Model.Mvvm` and `Dock.Model.ReactiveUI`
create these objects and wire them together. Understanding the
contracts listed on this page will help you customize the layout and
extend Dock with your own view models.
