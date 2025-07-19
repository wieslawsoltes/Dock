# PrismDockSample

This sample demonstrates how to integrate **Dock** with the
[Prism](https://prismlibrary.com/) framework using the `Dock.Model.Prism`
package.  The project registers the Dock services with the Prism container and
creates a small layout from a custom factory.

## Prism registration

`App` derives from `PrismApplication`. In `RegisterTypes` the Dock factory and
serializer are registered as singletons so they can be resolved from the
container.

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.RegisterSingleton<DockFactory>();
    containerRegistry.RegisterSingleton<IFactory>(c => c.Resolve<DockFactory>());
    containerRegistry.RegisterSingleton<DockSerializer>();
    containerRegistry.RegisterSingleton<IDockSerializer>(c => c.Resolve<DockSerializer>());
}
```

## Dock factory setup

`DockFactory` derives from `Dock.Model.Prism.Factory` and builds the layout in
code. The factory is resolved in `OnInitialized` to initialize
`DockControl`.

```csharp
var factory = Container.Resolve<DockFactory>();
var layout = factory.CreateLayout();
factory.InitLayout(layout);
```

See `DockFactory.cs` and `App.axaml.cs` for the complete implementation.
