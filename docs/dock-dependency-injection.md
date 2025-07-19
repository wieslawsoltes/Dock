# Dependency injection

Dock provides an integration library for the default .NET service container. The `Dock.Model.Extensions.DependencyInjection` package exposes an `AddDock` extension for `IServiceCollection`.

## Register services

Reference the package and register your factory and serializer types during startup:

```csharp
services.AddDock<MyDockFactory, DockSerializer>();
```

This registers `IDockState`, your factory implementation as `IFactory`, and the serializer as `IDockSerializer`.

## Configure factory options

`AddDock` also has an overload accepting an `IConfiguration` section. The section is bound to `FactoryOptions` which you can inject using `IOptions<FactoryOptions>`:

```csharp
var section = configuration.GetSection("Dock:Factory");
services.AddDock<MyDockFactory, DockSerializer>(section);
```

Use the resolved options when initializing your layout:

```csharp
var options = provider.GetRequiredService<IOptions<FactoryOptions>>().Value;
factory.InitLayout(layout, options);
```

Only the `HideToolsOnClose` and `HideDocumentsOnClose` flags can be configured through configuration. The locator dictionaries should be set programmatically.

