# Using DockableLocator

`DockableLocator` is a dictionary available on the `IFactory` interface. It maps dockable identifiers to factory methods that create dockable instances. The `GetDockable` helper looks up this dictionary when you need to resolve a dockable by id.

## Why registration is required

Layouts persist the full dockable graph, so an `IDockSerializer` restores dockable instances directly from the serialized data. `DockableLocator` is for cases where you want to resolve dockables by id at runtime, such as navigation, tool activation, or custom lookup helpers.

`FactoryBase` exposes `GetDockable<T>` as a thin wrapper around the dictionary and returns the object typed as `T`.

## Typical setup

Override `InitLayout` in your factory and register your dockables before calling the base implementation:

```csharp
public override void InitLayout(IDockable layout)
{
    DockableLocator = new Dictionary<string, Func<IDockable?>>
    {
        ["Document1"] = () => new MyDocument(),
        ["Tool1"] = () => new MyTool()
    };

    base.InitLayout(layout);
}
```

Each key should correspond to the `Id` assigned to the dockable in the layout. Keep the keys stable between application runs so previously saved layouts remain valid.

## Retrieving dockables

Use the `GetDockable<T>` method when you need a dockable instance by id:

```csharp
var tool = factory.GetDockable<MyTool>("Tool1");
```

This method simply forwards the call to `DockableLocator` and casts the result. It returns `null` if the id is not registered.
Empty ids also return `null`.

## Best practices

- Populate `DockableLocator` early in application startup, typically inside `InitLayout`.
- Register entries for dockables you intend to resolve by id at runtime.
- Keep id strings short but descriptive. They must match the ids you pass to `GetDockable`.
- If you use dependency injection, the factory methods can resolve services before creating the dockable.

Following these guidelines keeps id-based lookup predictable without affecting serialization.

For an overview of all documentation topics see the [documentation index](README.md).
