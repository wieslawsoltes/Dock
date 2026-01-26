# Dependency injection

Dock does not ship a dedicated dependency injection helper package. Register the core services manually, or copy the `AddDock` helper from `samples/DockReactiveUIDiSample/ServiceCollectionExtensions.cs` into your application.

## Installation

Install your preferred serializer package:

```bash
# Choose one serializer:
dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
dotnet add package Dock.Serializer.Protobuf          # Binary
dotnet add package Dock.Serializer.Xml               # XML  
dotnet add package Dock.Serializer.Yaml              # YAML
```

If you are using the default .NET service container, ensure you have the DI package:

```bash
dotnet add package Microsoft.Extensions.DependencyInjection
```

## Register services

Register your factory and serializer types during startup:

```csharp
using Dock.Model.Core;
using Dock.Serializer; // or your preferred serializer namespace
using Microsoft.Extensions.DependencyInjection;

services.AddSingleton<IDockState, DockState>();
services.AddSingleton<MyDockFactory>();
services.AddSingleton<IFactory>(static sp => sp.GetRequiredService<MyDockFactory>());

services.AddSingleton<DockSerializer>();
services.AddSingleton<IDockSerializer>(static sp => sp.GetRequiredService<DockSerializer>());
```

This registers `IDockState`, your factory implementation as `IFactory`, and the serializer as `IDockSerializer`.

If you prefer an `AddDock` helper, copy the sample extension from `samples/DockReactiveUIDiSample/ServiceCollectionExtensions.cs`.

## Complete example

```csharp
using Dock.Model.Core;
using Dock.Serializer;
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<MyDocumentViewModel>();
    services.AddTransient<MyToolViewModel>();

    services.AddSingleton<IDockState, DockState>();
    services.AddSingleton<MyDockFactory>();
    services.AddSingleton<IFactory>(static sp => sp.GetRequiredService<MyDockFactory>());

    services.AddSingleton<DockSerializer>();
    services.AddSingleton<IDockSerializer>(static sp => sp.GetRequiredService<DockSerializer>());

    services.AddSingleton<MyApplicationService>();
}
```

## Using the services

After registering the services, inject them into your view models or other services:

```csharp
public class MainWindowViewModel
{
    private readonly IFactory _factory;
    private readonly IDockState _dockState;
    private readonly IDockSerializer _serializer;

    public MainWindowViewModel(
        IFactory factory,
        IDockState dockState,
        IDockSerializer serializer)
    {
        _factory = factory;
        _dockState = dockState;
        _serializer = serializer;

        Layout = _factory.CreateLayout();
        _factory.InitLayout(Layout);
    }

    public IRootDock Layout { get; }
}
```

## Framework-specific registration

For ReactiveUI applications, register your ReactiveUI factory in the same way:

```csharp
using Dock.Model.Core;
using Dock.Serializer;
using Microsoft.Extensions.DependencyInjection;

services.AddSingleton<IDockState, DockState>();
services.AddSingleton<MyReactiveUIFactory>();
services.AddSingleton<IFactory>(static sp => sp.GetRequiredService<MyReactiveUIFactory>());

services.AddSingleton<DockSerializer>();
services.AddSingleton<IDockSerializer>(static sp => sp.GetRequiredService<DockSerializer>());
```

Where `MyReactiveUIFactory` derives from `Dock.Model.ReactiveUI.Factory`.

For an overview of all guides see the [documentation index](README.md).
