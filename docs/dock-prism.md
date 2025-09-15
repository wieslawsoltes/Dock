# PrismDockSample

This sample demonstrates how to integrate **Dock** with the
[Prism](https://prismlibrary.com/) framework using 
[Prism.Avalonia](https://github.com/AvaloniaCommunity/Prism.Avalonia) and
the `Dock.Model.Prism` package.  The project registers the Dock services
with Prism's container and creates a small layout from a custom factory.
For interface details refer to the [Dock API Reference](dock-reference.md) and see
the [Advanced Guide](dock-advanced.md) for more customization options.


## Step-by-Step Tutorial

1. Create a new Avalonia project
   ```
   dotnet new avalonia.app -o MyDockApp
   cd MyDockApp
   ```

2. Install the Dock and Prism packages
   ```powershell
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Prism
   dotnet add package Dock.Avalonia.Themes.Fluent
   dotnet add package Prism.Avalonia
   dotnet add package Prism.DryIoc.Avalonia
   ```

3. **Set up View Locator (Required)**

   Add the StaticViewLocator package:
   ```bash
   dotnet add package StaticViewLocator
   ```

   Create a `ViewLocator.cs` file:
   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;
   using StaticViewLocator;

   namespace DockPrismSample;

   [StaticViewLocator]
   public partial class ViewLocator : IDataTemplate
   {
     public Control? Build(object? data)
     {
       if (data is null)
         return null;

       var type = data.GetType();

       if (s_views.TryGetValue(type, out var func))
         return func.Invoke();

       throw new Exception($"Unable to create view for type: {type}");
     }

     public bool Match(object? data)
     {
       if (data is null)
         return false;

       var type = data.GetType();
       return data is IDockable || s_views.ContainsKey(type);
     }
   }
   ```

   Register the view locator in `App.axaml`:
   ```xaml
   <Application xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:local="using:MyDockApp"
                x:Class="MyDockApp.App">

     <Application.DataTemplates>
       <local:ViewLocator />
     </Application.DataTemplates>

     <Application.Styles>
       <FluentTheme />
       <DockFluentTheme />
     </Application.Styles>
   </Application>
   ```

4. Create a factory and view models

   Derive from `Dock.Model.Prism.Factory` and implement `CreateLayout`. Your documents and
   tools should inherit from the ReactiveUI versions of `Document` and `Tool`.


## Prism Registration

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
