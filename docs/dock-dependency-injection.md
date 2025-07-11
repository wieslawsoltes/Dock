# Dependency injection

Dock works well with dependency injection (DI) containers such as
`Microsoft.Extensions.DependencyInjection`. A DI container can manage
view models, services and factory instances so that layout creation and
serialization are fully automated.

## Registering services

Create a service collection and register your Dock factory, dock state
and serializer in addition to your own view models:

```csharp
var services = new ServiceCollection();
services.AddTransient<DocumentViewModel>();
services.AddTransient<ToolViewModel>();
services.AddSingleton<DockFactory>();
services.AddSingleton<IFactory>(sp => sp.GetRequiredService<DockFactory>());
services.AddSingleton<IDockSerializer, DockSerializer>();
services.AddSingleton<IDockState, DockState>();
```

Build the provider and store it for later use:

```csharp
var provider = services.BuildServiceProvider();
App.ServiceProvider = provider;
```

## Resolving dockables in a factory

Inject `IServiceProvider` into your factory so it can create dockables
and view models on demand:

```csharp
public class DockFactory : Factory
{
    private readonly IServiceProvider _provider;

    public DockFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public override IRootDock CreateLayout()
    {
        var document = _provider.GetRequiredService<DocumentViewModel>();
        document.Id = "Document1";
        document.Title = "Document";

        var tool = _provider.GetRequiredService<ToolViewModel>();
        tool.Id = "Tool1";
        tool.Title = "Tool";

        var root = CreateRootDock();
        root.VisibleDockables = CreateList<IDockable>(
            new DocumentDock
            {
                VisibleDockables = CreateList<IDockable>(document),
                ActiveDockable = document
            },
            new ToolDock
            {
                VisibleDockables = CreateList<IDockable>(tool),
                ActiveDockable = tool
            });
        root.DefaultDockable = root.VisibleDockables[0];
        return root;
    }
}
```

`CreateLayout` uses the provider to resolve view models and then builds
the dock hierarchy. Additional services can be resolved during
`InitLayout` to populate `ContextLocator`, `DockableLocator` or
`HostWindowLocator`.

## Sample project

The `DockReactiveUIDiSample` demonstrates dependency injection in a full
application. `Program.cs` configures the service collection and
`App.axaml.cs` retrieves the main window view model from the provider.
Explore that sample for a complete implementation.

For an overview of the other guides see the [documentation index](README.md).
