# Dependency injection

Dock provides an integration library for the default .NET service container. The `Dock.Model.Extensions.DependencyInjection` package exposes an `AddDock` extension for `IServiceCollection`.

## Register services

Reference the package and register your factory and serializer types during startup:

```csharp
services.AddDock<MyDockFactory, DockSerializer>();
```

This registers `IDockState`, your factory implementation as `IFactory`, and the serializer as `IDockSerializer`.


