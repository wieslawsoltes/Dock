# Dependency injection

Dock provides an integration library for the default .NET service container. The `Dock.Model.Extensions.DependencyInjection` package exposes an `AddDock` extension for `IServiceCollection`.

## Installation

First, install the dependency injection package:

```bash
dotnet add package Dock.Model.Extensions.DependencyInjection
```

Also install your preferred serializer package:

```bash
# Choose one serializer:
dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
dotnet add package Dock.Serializer.Protobuf          # Binary
dotnet add package Dock.Serializer.Xml               # XML  
dotnet add package Dock.Serializer.Yaml              # YAML
```

## Register services

Reference the package and register your factory and serializer types during startup:

```csharp
using Dock.Model.Extensions.DependencyInjection;
using Dock.Serializer; // or your preferred serializer namespace

// Register with JSON serializer (Newtonsoft.Json)
services.AddDock<MyDockFactory, DockSerializer>();

// Or with System.Text.Json serializer
services.AddDock<MyDockFactory, Dock.Serializer.SystemTextJson.DockSerializer>();

// Or with Protobuf serializer
services.AddDock<MyDockFactory, ProtobufDockSerializer>();

// Or with XML serializer
services.AddDock<MyDockFactory, DockXmlSerializer>();

// Or with YAML serializer  
services.AddDock<MyDockFactory, DockYamlSerializer>();
```

This registers `IDockState`, your factory implementation as `IFactory`, and the serializer as `IDockSerializer`.

## Complete example

Here's a full example showing how to set up dependency injection with Dock:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Dock.Model.Extensions.DependencyInjection;
using Dock.Serializer;

public void ConfigureServices(IServiceCollection services)
{
    // Register your view models and views
    services.AddTransient<MyDocumentViewModel>();
    services.AddTransient<MyToolViewModel>();
    
    // Register Dock services
    services.AddDock<MyDockFactory, DockSerializer>();
    
    // Register other application services
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
        
        // Create layout using the factory
        Layout = _factory.CreateLayout();
        _factory.InitLayout(Layout);
    }
    
    public IRootDock Layout { get; }
}
```

## Framework-specific registration

For ReactiveUI applications, you can also register the ReactiveUI-specific factory:

```csharp
using Dock.Model.Extensions.DependencyInjection;
using Dock.Model.ReactiveUI;

services.AddDock<MyReactiveUIFactory, DockSerializer>();
```

Where `MyReactiveUIFactory` derives from `Dock.Model.ReactiveUI.Factory`.

For an overview of all guides see the [documentation index](README.md).


