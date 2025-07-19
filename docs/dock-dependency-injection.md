# Dependency injection

Dock provides an integration library for the default .NET service container. The `Dock.Model.Extensions.DependencyInjection` package exposes an `AddDock` extension for `IServiceCollection`.

## Register services

Reference the package and register your factory and serializer types during startup:

```csharp
services.AddDock<MyDockFactory, DockSerializer>();
```

This registers `IDockState`, your factory implementation as `IFactory`, and the serializer as `IDockSerializer`.

## Configure the factory

`AddDock` accepts an optional callback allowing you to provide a default layout created by the factory:

```csharp
services.AddDock<MyDockFactory, DockSerializer>(options =>
{
    options.Layout = new MyDockFactory().CreateLayout();
});
```

The layout instance is registered as a singleton so it can be injected into your views or view models.

## Enable event logging

If you want to log docking events, use `AddDockWithLogger` instead. It registers `DockEventLogger` which subscribes to the factory events and writes them using `ILogger`:

```csharp
services.AddDockWithLogger<MyDockFactory, DockSerializer>();
```

## Dock settings options

Drag distances and window magnetism can be configured through `DockSettingsOptions`:

```csharp
services.Configure<DockSettingsOptions>(o =>
{
    o.MinimumHorizontalDragDistance = 6;
    o.MinimumVerticalDragDistance = 6;
    o.EnableWindowMagnetism = true;
    o.WindowMagnetDistance = 16;
});
```

`AddDock` registers a configurator that applies these values to `DockSettings` when the container is built.
