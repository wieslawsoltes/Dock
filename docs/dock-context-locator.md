# Context locators

Dock assigns `dockable.Context` when a layout is initialized or loaded if the dockable does not already have a context. The objects are resolved through two properties on `IFactory`:
`ContextLocator` and `DefaultContextLocator`.

## `ContextLocator` dictionary

`ContextLocator` is a `Dictionary<string, Func<object?>>` mapping an identifier to
a factory method. Each entry returns the object that becomes `dockable.Context`
(often used as a view `DataContext`) for the dockable with the same `Id`.
Populate this dictionary before calling `InitLayout`, including when you
initialize a layout loaded with an `IDockSerializer`.

```csharp
public override void InitLayout(IDockable layout)
{
    ContextLocator = new Dictionary<string, Func<object?>>
    {
        ["Document1"] = () => new DocumentViewModel(),
        ["Tool1"] = () => new Tool1ViewModel(),
    };

    base.InitLayout(layout);
}
```

During `InitDockable` the factory looks up the dockable `Id` in
`ContextLocator` and assigns the returned object to `dockable.Context` when it
is still `null`. If the id is not found, it falls back to
`DefaultContextLocator`. Empty ids skip the lookup entirely, so ensure you set
`Id` on dockables you want resolved.

## `DefaultContextLocator` fallback

`DefaultContextLocator` is a `Func<object?>` invoked when
`ContextLocator` has no entry for the requested identifier. Use it to provide a
common fallback or to integrate with a dependency injection container.

```csharp
DefaultContextLocator = () => _services.GetService<MainViewModel>();
```

When `GetContext` cannot resolve a specific non-empty id it will call this
delegate. If it returns `null`, the dockable keeps its existing `Context`
(often `null`).

## DockControl default context

When `DockControl.InitializeFactory` is `true`, the control assigns
`DefaultContextLocator` for you and returns the value of
`DockControl.DefaultContext`. This is a convenient way to supply a shared
fallback without touching the factory:

```csharp
var dockControl = new DockControl
{
    DefaultContext = new MainViewModel(),
    InitializeFactory = true
};
```

If you want to configure `DefaultContextLocator` yourself, disable
`InitializeFactory` or assign your locator after the control initializes.

## Why it matters

Dockable views often rely on `dockable.Context` (directly or via templates) to
function correctly. When loading a layout that references custom documents or
tools, the factory must be able to recreate those view models. Register each
type in `ContextLocator` and provide a default via `DefaultContextLocator` so
that unknown ids do not break the layout.

These locators work alongside `DockableLocator` and `HostWindowLocator` which
resolve dockable instances and host windows. Ensure all locators are populated
before deserializing layouts or initializing new ones.

For a high level overview of the factory API see the
[Advanced guide](dock-advanced.md) and [Reference guide](dock-reference.md).
