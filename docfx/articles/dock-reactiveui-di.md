# Dock ReactiveUI with Dependency Injection Getting Started Guide

This guide explains how to get started with Dock using ReactiveUI and dependency injection. This approach provides a clean separation of concerns, testability, and proper service management for complex applications.

The sample project `DockReactiveUIDiSample` in the repository demonstrates this approach. For interface details refer to the [Dock API Reference](dock-reference.md).

> **💡 Modern Approach**: For easier source-backed layout management, consider using [DocumentDock/ToolDock ItemsSource](dock-itemssource.md) which automatically creates and manages documents and tools from collections. The ItemsSource approach works seamlessly with dependency injection and ReactiveUI. This approach is covered in detail in the [Document and Tool Content Guide](dock-content-guide.md).

## Step-by-step tutorial

Follow these instructions to create a ReactiveUI application with dependency injection using Dock.

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MyDockApp
   cd MyDockApp
   ```

2. **Install the required packages**

   ```bash
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.ReactiveUI
   dotnet add package Dock.Avalonia.Themes.Fluent
   dotnet add package Microsoft.Extensions.DependencyInjection
   dotnet add package Microsoft.Extensions.Hosting
   dotnet add package ReactiveUI.Avalonia
   ```

   **Optional packages:**
   ```bash
   # For serialization (choose one):
   dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
   dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
   ```

   Dock does not ship a DI helper package. Register the Dock services manually, or copy the helper from `samples/DockReactiveUIDiSample/ServiceCollectionExtensions.cs`.

3. **Set up Dependency Injection View Locator**

   Create a service-based ViewLocator that resolves views through the DI container:

   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;
   using ReactiveUI;
   using Microsoft.Extensions.DependencyInjection;

   namespace MyDockApp;

   public class ViewLocator : IDataTemplate, IViewLocator
   {
       private readonly IServiceProvider _provider;

       public ViewLocator(IServiceProvider provider)
       {
           _provider = provider;
       }

       private IViewFor? Resolve(object viewModel)
       {
           var vmType = viewModel.GetType();
           var serviceType = typeof(IViewFor<>).MakeGenericType(vmType);
           
           if (_provider.GetService(serviceType) is IViewFor view)
           {
               view.ViewModel = viewModel;
               return view;
           }

           // Fallback: try to resolve by naming convention
           var viewName = vmType.FullName?.Replace("ViewModel", "View");
           if (viewName is not null)
           {
               var viewType = Type.GetType(viewName);
               if (viewType != null && _provider.GetService(viewType) is IViewFor view2)
               {
                   view2.ViewModel = viewModel;
                   return view2;
               }
           }

           return null;
       }

       public Control? Build(object? data)
       {
           if (data is null)
               return null;

           if (Resolve(data) is IViewFor view && view is Control control)
               return control;

           var viewName = data.GetType().FullName?.Replace("ViewModel", "View");
           return new TextBlock { Text = $"Not Found: {viewName}" };
       }

       public bool Match(object? data)
       {
           if (data is null)
           {
               return false;
           }

           if (data is IDockable)
           {
               return true;
           }

           return Resolve(data) is not null;
       }
       
       IViewFor? IViewLocator.ResolveView<T>(T? viewModel, string? contract) where T : default =>
           viewModel is null ? null : Resolve(viewModel);
   }
   ```

4. **Create your data models and services**

   Create a simple data service and model:

   ```csharp
   namespace MyDockApp.Models;

   public class DemoData
   {
       public string Message { get; set; } = "Hello from Dependency Injection!";
       public DateTime LastUpdated { get; set; } = DateTime.Now;
   }

   public interface IDataService
   {
       DemoData GetData();
       void UpdateData(DemoData data);
   }

   public class DataService : IDataService
   {
       private DemoData _data = new();

       public DemoData GetData() => _data;

       public void UpdateData(DemoData data)
       {
           _data = data;
           _data.LastUpdated = DateTime.Now;
       }
   }
   ```

5. **Create view models using dependency injection**

   ```csharp
   using System.Reactive.Disposables;
   using ReactiveUI;
   using Dock.Model.ReactiveUI.Controls;
   using MyDockApp.Models;

   namespace MyDockApp.ViewModels.Documents
   {
       public class DocumentViewModel : Document
       {
           private readonly IDataService _dataService;
           private readonly CompositeDisposable _disposables = new();

           public DocumentViewModel(IDataService dataService)
           {
               _dataService = dataService;
               
               var data = _dataService.GetData();
               Content = data.Message;
               
               // Set up reactive properties and commands as needed
               this.WhenAnyValue(x => x.Content)
                   .Subscribe(content => 
                   {
                       var updatedData = new DemoData { Message = content };
                       _dataService.UpdateData(updatedData);
                   })
                   .DisposeWith(_disposables);
           }

           private string _content = string.Empty;
           public string Content
           {
               get => _content;
               set => this.RaiseAndSetIfChanged(ref _content, value);
           }
       }
   }

   namespace MyDockApp.ViewModels.Tools
   {
       public class ToolViewModel : Tool
       {
           private readonly IDataService _dataService;

           public ToolViewModel(IDataService dataService)
           {
               _dataService = dataService;
               RefreshData();
           }

           private string _status = string.Empty;
           public string Status
           {
               get => _status;
               set => this.RaiseAndSetIfChanged(ref _status, value);
           }

           private void RefreshData()
           {
               var data = _dataService.GetData();
               Status = $"Last updated: {data.LastUpdated:HH:mm:ss}";
           }
       }
   }
   ```

6. **Create a factory with dependency injection**

   ```csharp
   using System;
   using System.Collections.Generic;
   using Dock.Model.Core;
   using Dock.Model.ReactiveUI;
   using Dock.Model.ReactiveUI.Controls;
   using MyDockApp.Models;
   using MyDockApp.ViewModels.Documents;
   using MyDockApp.ViewModels.Tools;
   using Microsoft.Extensions.DependencyInjection;

   namespace MyDockApp.ViewModels;

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

           var proportionalDock = CreateProportionalDock();
           proportionalDock.Orientation = Orientation.Horizontal;
           proportionalDock.VisibleDockables = CreateList<IDockable>(
               new DocumentDock
               {
                   VisibleDockables = CreateList<IDockable>(document),
                   ActiveDockable = document
               },
               CreateProportionalDockSplitter(),
               new ToolDock
               {
                   VisibleDockables = CreateList<IDockable>(tool),
                   ActiveDockable = tool
               });

           var root = CreateRootDock();
           root.VisibleDockables = CreateList<IDockable>(proportionalDock);
           root.ActiveDockable = proportionalDock;
           root.DefaultDockable = proportionalDock;

           return root;
       }

       public override void InitLayout(IDockable layout)
       {
           DockableLocator = new Dictionary<string, Func<IDockable?>>
           {
               ["Document1"] = () =>
               {
                   var vm = _provider.GetRequiredService<DocumentViewModel>();
                   vm.Id = "Document1";
                   vm.Title = "Document";
                   return vm;
               },
               ["Tool1"] = () =>
               {
                   var vm = _provider.GetRequiredService<ToolViewModel>();
                   vm.Id = "Tool1";
                   vm.Title = "Tool";
                   return vm;
               }
           };

           ContextLocator = new Dictionary<string, Func<object?>>
           {
               ["Document1"] = () => _provider.GetRequiredService<DemoData>(),
               ["Tool1"] = () => _provider.GetRequiredService<DemoData>()
           };

           DefaultContextLocator = () => _provider.GetService(typeof(DemoData));

           base.InitLayout(layout);
       }
   }
   ```

7. **Create views**

   **DocumentView.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="MyDockApp.Views.Documents.DocumentView">
     <TextBox Text="{Binding Content}" AcceptsReturn="True" />
   </UserControl>
   ```

   **DocumentView.axaml.cs:**
   ```csharp
   using ReactiveUI.Avalonia;
   using MyDockApp.ViewModels.Documents;

   namespace MyDockApp.Views.Documents;

   public partial class DocumentView : ReactiveUserControl<DocumentViewModel>
   {
       public DocumentView()
       {
           InitializeComponent();
       }
   }
   ```

   **ToolView.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="MyDockApp.Views.Tools.ToolView">
     <StackPanel Margin="5">
       <TextBlock Text="Tool Panel" FontWeight="Bold" />
       <TextBlock Text="{Binding Status}" />
     </StackPanel>
   </UserControl>
   ```

   **ToolView.axaml.cs:**
   ```csharp
   using ReactiveUI.Avalonia;
   using MyDockApp.ViewModels.Tools;

   namespace MyDockApp.Views.Tools;

   public partial class ToolView : ReactiveUserControl<ToolViewModel>
   {
       public ToolView()
       {
           InitializeComponent();
       }
   }
   ```

8. **Set up dependency injection in Program.cs**

   ```csharp
   using System;
   using Avalonia;
   using ReactiveUI.Avalonia;
   using Dock.Model.Core;
   using Dock.Serializer;
   using MyDockApp.Models;
   using MyDockApp.ViewModels;
   using MyDockApp.ViewModels.Documents;
   using MyDockApp.ViewModels.Tools;
   using MyDockApp.Views;
   using MyDockApp.Views.Documents;
   using MyDockApp.Views.Tools;
   using ReactiveUI;
   using Microsoft.Extensions.DependencyInjection;

   namespace MyDockApp;

   internal class Program
   {
       [STAThread]
       private static void Main(string[] args)
       {
           using var provider = Initialize();
           BuildAvaloniaApp(provider).StartWithClassicDesktopLifetime(args);
       }

       private static ServiceProvider Initialize()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           return services.BuildServiceProvider();
       }

       private static void ConfigureServices(IServiceCollection services)
       {
           services.AddSingleton<App>();
           services.AddSingleton<IViewLocator, ViewLocator>();
           
           // Register data services
           services.AddSingleton<DemoData>();
           services.AddSingleton<IDataService, DataService>();
           
           // Register view models
           services.AddTransient<DocumentViewModel>();
           services.AddTransient<ToolViewModel>();
           services.AddSingleton<MainWindowViewModel>();
           
           // Register views
           services.AddTransient<IViewFor<DocumentViewModel>, DocumentView>();
           services.AddTransient<IViewFor<ToolViewModel>, ToolView>();
           services.AddTransient<IViewFor<MainWindowViewModel>, MainWindow>();

           // Register Dock services
           services.AddSingleton<IDockState, DockState>();
           services.AddSingleton<DockFactory>();
           services.AddSingleton<IFactory>(static sp => sp.GetRequiredService<DockFactory>());
           services.AddSingleton<DockSerializer>();
           services.AddSingleton<IDockSerializer>(static sp => sp.GetRequiredService<DockSerializer>());
       }

       public static AppBuilder BuildAvaloniaApp(IServiceProvider provider)
           => AppBuilder.Configure(() => provider.GetRequiredService<App>())
               .UsePlatformDetect()
               .WithInterFont()
               .UseReactiveUI()
               .LogToTrace();
   }
   ```

9. **Update App.axaml.cs to use dependency injection**

   ```csharp
   using System;
   using Avalonia;
   using Avalonia.Controls;
   using Avalonia.Controls.ApplicationLifetimes;
   using Avalonia.Controls.Templates;
   using Avalonia.Markup.Xaml;
   using Microsoft.Extensions.DependencyInjection;
   using MyDockApp.ViewModels;
   using ReactiveUI;

   namespace MyDockApp;

   public partial class App : Application
   {
       public IServiceProvider? ServiceProvider { get; }
       private readonly IViewLocator _viewLocator;

       public App()
       {
       }

       public App(IServiceProvider? serviceProvider, IViewLocator viewLocator)
       {
           ServiceProvider = serviceProvider;
           _viewLocator = viewLocator;
       }

   public override void Initialize()
   {
       AvaloniaXamlLoader.Load(this);
       DataTemplates.Insert(0, (IDataTemplate)_viewLocator);
   }

   public override void OnFrameworkInitializationCompleted()
   {
       if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && ServiceProvider != null)
       {
           var viewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
           var view = ServiceProvider.GetRequiredService<IViewFor<MainWindowViewModel>>();
           view.ViewModel = viewModel;
           if (view is Window window)
           {
               desktop.MainWindow = window;
           }
       }

       base.OnFrameworkInitializationCompleted();
   }
   }
   ```

10. **Run the application**

    ```bash
    dotnet run
    ```

## Dependency Injection Benefits

Using dependency injection with Dock provides several advantages:

1. **Testability**: Easy to mock services for unit testing
2. **Separation of Concerns**: Clear separation between data access, business logic, and UI
3. **Service Management**: Automatic lifetime management of services
4. **Configuration**: Easy to configure different implementations for different environments
5. **Extensibility**: Easy to add new services and features

## Key Points

- **Service Registration**: Register all view models, views, and services in the DI container
- **Factory Dependencies**: Pass the service provider to the factory for dynamic service resolution
- **View Locator**: Use a DI-aware view locator that resolves views from the container
- **Reactive Commands**: Leverage ReactiveUI's reactive programming model with injected services
- **Proper Disposal**: Ensure proper cleanup of reactive subscriptions using `DisposeWith()`

This approach is ideal for complex applications that require proper service management, testability, and clean architecture principles.
