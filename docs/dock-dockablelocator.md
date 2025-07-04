# Using DockableLocator

`DockableLocator` is a dictionary available on the `IFactory` interface. It maps the identifiers stored in a layout to the functions that create your view models. Whenever the framework needs to instantiate a dockable – for example during serialization or when `GetDockable` is called – it queries this dictionary.

## Why registration is required

Layouts persist only the `Id` of a dockable. During deserialization `DockSerializer` looks up this identifier in `DockableLocator` to obtain a factory method that returns a new instance. If an identifier is missing the serializer returns `null` which usually results in an incomplete layout. By populating `DockableLocator` up‑front you ensure that all custom documents and tools can be reconstructed.

`FactoryBase` also uses `DockableLocator` for operations that need to materialise dockables lazily. The `GetDockable` helper is a thin wrapper around the dictionary and returns the object typed as `IDockable`.

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

## Best practices

- Populate `DockableLocator` early in application startup, typically inside `InitLayout`.
- Ensure every dockable type that appears in a serialized layout has an entry.
- Keep id strings short but descriptive. They do not need to be unique at runtime but they must match the ids stored in the layout.
- If you use dependency injection, the factory methods can resolve services before creating the dockable.

Following these guidelines guarantees that serialization and dynamic creation work reliably.

For an overview of all documentation topics see the [documentation index](README.md).
