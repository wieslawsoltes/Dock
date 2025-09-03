# PrismDockSample

This sample demonstrates how to integrate **Dock** with the
[Prism](https://prismlibrary.com/) framework using 
[Prism.Avalonia](https://github.com/AvaloniaCommunity/Prism.Avalonia) and
the `Dock.Model.Prism` package.  The project registers the Dock services
with the Prism container and creates a small layout from a custom factory.

## Prism registration

`App` derives from `PrismApplication`. In `RegisterTypes` the Dock factory and
serializer are registered as singletons so they can be resolved from the
container.

```csharp
public override void Initialize()
{
    ThemeManager = new FluentThemeManager();
    AvaloniaXamlLoader.Load(this);
        
    // Required by Prism.Avalonia when overriding Initialize()
    base.Initialize();
}

protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.RegisterSingleton<IFactory, Resolve<DockFactory>();
    containerRegistry.RegisterSingleton<IDockSerializer, DockSerializer>();
}
```

## Dock factory setup

`DockFactory` derives from `Dock.Model.Prism.Factory` and builds the layout in
code. The factory is resolved in `OnInitialized` to initialize
`DockControl`.

```csharp
public MainWindowViewModel(IFactory dockFactory)
{
    var factory = dockFactory; // Container.Resolve<DockFactory>();
    var layout = factory.CreateLayout();
    factory.InitLayout(layout);
    // ...
}
```

See `DockFactory.cs` and `App.axaml.cs` for the complete implementation.
