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
| `IDocumentContent` | Document containing arbitrary `Content`. |
| `IDocumentDock` | Dock that hosts documents and exposes commands to create them. |
| `IDocumentDockContent` | Dock that creates documents from a `DocumentTemplate`. |
| `IDocumentTemplate` | Template object used when creating new documents on demand. |
| `IProportionalDock` | Dock that arranges children vertically or horizontally using proportions. |
| `IProportionalDockSplitter` | Splitter element between proportional dock children. |
| `IRootDock` | Top level container responsible for pinned docks and windows. |
| `ITool` | Basic interface for tool panes such as explorers or output views. |
| `IToolContent` | Tool containing arbitrary `Content`. |
| `IToolDock` | Dock that hosts tools and supports auto hide behaviour. |

The following sections provide guidelines on applying these contracts in your projects.

## IDockDock

Use `IDockDock` for dock panels that hold a collection of child dockables.
The `LastChildFill` property mirrors the behaviour of Avalonia's `DockPanel`.
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

## IDocumentDock and IDocumentDockContent

`IDocumentDock` is a specialized dock that maintains a tab strip of
documents. It contains optional commands for creating new documents
and allows dragging the host window via the tab area when
`EnableWindowDrag` is `true`. `TabsLayout` determines where the tabs
are placed.

`IDocumentDockContent` extends this concept by storing a
`DocumentTemplate` object. Calling `CreateDocumentFromTemplate`
should produce a new view model implementing `IDocument`. This is
useful when a document type can be instantiated from a template,
for example when creating a blank file.

## IDocumentTemplate

Templates are lightweight objects that store any information required
to create a document instance. A factory can inspect the template
and return a fully initialized document. Use this pattern to support
"New" commands that generate a specific kind of document without
hard-coding the creation logic in your views.

## IProportionalDock and IProportionalDockSplitter

Proportional docks arrange their children either horizontally or
vertically. Each dockable specifies a `Proportion` value which is
interpreted relative to the other siblings. Insert an
`IProportionalDockSplitter` between dockables to allow the user to
resize the areas at runtime. The splitter exposes `CanResize`
which can disable dragging for fixed layouts.

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
auto hide its content and exposes an `Alignment` property for
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

