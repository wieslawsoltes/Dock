# Dock API Reference

This reference summarizes the most commonly used classes in Dock. It is based on the samples found in the repository and the public APIs in the `Dock.Model` packages. The MVVM, ReactiveUI and XAML approaches share the same underlying types so the following sections apply to all three.

## Core interfaces

| Type | Description |
| --- | --- |
| `IDockable` | Base interface for items that can be shown in a dock. Provides `Id`, `Title`, `Context` and lifecycle methods like `OnClose`. |
| `IDock` | Extends `IDockable` with collections of visible dockables and commands such as `GoBack`, `GoForward`, `Navigate` or `Close`. |
| `IRootDock` | The top level container. In addition to the `IDock` members it exposes pinned dock collections and commands to manage windows. |
| `IProportionalDock` | A dock that lays out its children horizontally or vertically using a `Proportion` value. |
| `IToolDock` / `IDocumentDock` | Specialized docks used for tools and documents. |
| `IProportionalDockSplitter` | Thin splitter placed between proportional docks. |
| `IDockWindow` / `IHostWindow` | Interfaces representing floating windows created when dockables are detached. |

## Factory API

The `IFactory` interface (implemented by `Factory` in `Dock.Model.Mvvm` and `Dock.Model.ReactiveUI`) contains numerous helpers used by the samples to build and manipulate layouts. Important members include:

- Methods such as `CreateRootDock`, `CreateProportionalDock`, `CreateToolDock` and `CreateDocumentDock` used when constructing the initial layout.
- `CreateLayout()` which returns an `IRootDock` ready to display.
- `InitLayout`, `InitDockable` and `InitDockWindow` for wiring up newly created objects.
- Runtime operations like `AddDockable`, `InsertDockable`, `MoveDockable`, `SwapDockable`, `PinDockable`, `FloatDockable` and the various `Close*` methods.
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

The XAML sample shows that a layout can be declared entirely in markup using `DockControl` and the various dock types. The `Dock.Serializer` package can persist these layouts to disk. In XAML you place `RootDock`, `ProportionalDock`, `ToolDock` or `DocumentDock` elements inside a `DockControl`, optionally providing a factory for runtime behaviour.

## Further reading

- `samples/DockMvvmSample` – full MVVM example.
- `samples/DockReactiveUISample` – ReactiveUI variant.
- `samples/DockXamlSample` – XAML-only layout with serialization.

For an overview of all guides see the [documentation index](README.md).
