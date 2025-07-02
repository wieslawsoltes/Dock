# Dock Advanced Guide

This guide highlights advanced features from the Dock samples. The API is shared across the MVVM, ReactiveUI and XAML versions so the same concepts apply no matter which approach you use.
This guide assumes you are familiar with the basics from the other guides. Interface descriptions are available in the [Dock API Reference](dock-reference.md). It focuses on runtime customization and advanced APIs.

## Custom factories

All samples derive from `Factory` and override methods to configure the layout. In addition to `CreateLayout`, you can override:

- `CreateWindowFrom` to customize new floating windows
- `CreateDocumentDock` to provide a custom `IDocumentDock` implementation
- `InitLayout` to wire up `ContextLocator`, `DockableLocator` and `HostWindowLocator`

The MVVM and ReactiveUI samples use these hooks to register view models,
window factories and other services required at runtime. By overriding the
factory methods you can control how floating windows are created, inject your
own tool or document types and tie into application specific services such as
dependency injection containers.

```csharp
public override void InitLayout(IDockable layout)
{
    ContextLocator = new Dictionary<string, Func<object?>>
    {
        ["Document1"] = () => new DemoDocument(),
        ["Tool1"] = () => new Tool1(),
        // additional entries omitted
    };

    HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
    {
        [nameof(IDockWindow)] = () => new HostWindow()
    };

    base.InitLayout(layout);
}
```

## Handling events

`FactoryBase` exposes events for virtually every docking action. The samples subscribe to them to trace runtime changes:

Events are useful for hooking into your application's own logging or
analytics system. For example you might record when documents are opened
or closed so that the next run can restore them.

```csharp
factory.ActiveDockableChanged += (_, args) =>
{
    Debug.WriteLine($"[ActiveDockableChanged] {args.Dockable?.Title}");
};
factory.DockableAdded += (_, args) =>
{
    Debug.WriteLine($"[DockableAdded] {args.Dockable?.Title}");
};
```

// Example: track created and active documents
```csharp
factory.DockableAdded += (_, e) => Console.WriteLine($"Added {e.Dockable?.Id}");
factory.ActiveDockableChanged += (_, e) => Console.WriteLine($"Active {e.Dockable?.Id}");
```

You can react to focus changes, window moves or when dockables are pinned and unpinned.

## Saving and loading layouts

The XAML sample demonstrates persisting layouts with `DockSerializer`:

```csharp
await using var stream = await file.OpenReadAsync();
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dock.Layout = layout;
    _dockState.Restore(layout);
}
```

`DockState` tracks the active and focused dockables so the state can be restored after loading.

## Dynamic documents and tools

The Notepad sample shows how to create documents at runtime. New `FileViewModel` instances are added to an `IDocumentDock` using factory methods:

```csharp
_factory?.AddDockable(files, fileViewModel);
_factory?.SetActiveDockable(fileViewModel);
_factory?.SetFocusedDockable(Layout, fileViewModel);
```

Drag-and-drop handlers and file dialogs are used to open and save documents on the fly.

## Floating windows

Calling `FloatDockable` opens a dockable in a separate window. You can override `CreateWindowFrom` to tweak the new window:

```csharp
public override IHostWindow CreateWindowFrom(IDockWindow source)
{
    var window = base.CreateWindowFrom(source);
    window.Title = $"Floating - {source.Title}";
    window.Width = 800;
    window.Height = 600;
    return window;
}
```

## Global side docking

Dockables can also be docked to the edges of the root layout. Dropping a tab on
the outer borders of `RootDockControl` splits the root in that direction,
anchoring the dockable globally to the chosen side just like in Visual Studio.
The dock manager exposes new operations `GlobalLeft`, `GlobalRight`,
`GlobalTop` and `GlobalBottom` to represent these targets.

## Conclusion

Explore the samples under `samples/` for complete implementations. Mixing these techniques with the basics lets you build complex layouts that can be persisted and restored.

If you are new to Dock, start with the [MVVM Guide](dock-mvvm.md) before diving into these topics.

For an overview of all guides see the [documentation index](README.md).
