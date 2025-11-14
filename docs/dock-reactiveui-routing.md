# Dock ReactiveUI with Routing Getting Started Guide

This guide explains how to get started with Dock using ReactiveUI's routing capabilities. This approach enables navigation between different views within documents and tools, creating a spa-like experience within a docking interface.

The sample project `DockReactiveUIRoutingSample` in the repository demonstrates this approach. For interface details refer to the [Dock API Reference](dock-reference.md).

> **💡 Navigation Pattern**: ReactiveUI's routing system provides powerful navigation capabilities that work seamlessly within Dock's dockable interface. This is particularly useful for creating complex multi-view documents or tools with internal navigation.

## Step-by-step tutorial

Follow these instructions to create a ReactiveUI application with routing using Dock.

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MyDockApp
   cd MyDockApp
   ```

2. **Install the required packages**

   ```bash
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.ReactiveUI
   dotnet add package Dock.Model.ReactiveUI.Navigation
   dotnet add package Dock.Avalonia.Themes.Fluent
   dotnet add package ReactiveUI.Avalonia
   ```

   **Optional packages:**
   ```bash
   # For serialization (choose one):
   dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
   dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
   
   # For dependency injection:
   dotnet add package Dock.Model.Extensions.DependencyInjection
   ```

3. **Set up View Locator with Routing Support**

   Create a view locator that supports both regular views and routable views:

   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;
   using ReactiveUI;
   using StaticViewLocator;

   namespace MyDockApp;

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
           {
               return false;
           }

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

4. **Create routable view models**

   Create view models that implement routing functionality:

   ```csharp
   using System;
   using System.Reactive;
   using System.Reactive.Disposables;
   using ReactiveUI;
   using Dock.Model.ReactiveUI.Navigation.Controls;

   namespace MyDockApp.ViewModels.Documents;

   public class DocumentViewModel : RoutableDocument
   {
       public ReactiveCommand<Unit, IRoutableViewModel> GoToPage1 { get; }
       public ReactiveCommand<Unit, IRoutableViewModel> GoToPage2 { get; }

       public DocumentViewModel(IScreen host) : base(host)
       {
           // Navigate to the default page
           Router.Navigate.Execute(new Page1ViewModel(this, "Document Home"));

           // Commands to navigate to different pages
           GoToPage1 = ReactiveCommand.CreateFromObservable(() =>
               Router.Navigate.Execute(new Page1ViewModel(this, "Page 1 Content")));

           GoToPage2 = ReactiveCommand.CreateFromObservable(() =>
               Router.Navigate.Execute(new Page2ViewModel(this, "Page 2 Content")));

           GoToPage1.DisposeWith(Disposables);
           GoToPage2.DisposeWith(Disposables);
       }

       public void InitNavigation(
           IRoutableViewModel? document2,
           IRoutableViewModel? tool1,
           IRoutableViewModel? tool2)
       {
           // Navigation to other dockables can be set up here
           // This is called from the factory after all view models are created
       }
   }

   public class ToolViewModel : RoutableTool
   {
       public ReactiveCommand<Unit, IRoutableViewModel> GoToDocument1 { get; set; }
       public ReactiveCommand<Unit, IRoutableViewModel> GoToDocument2 { get; set; }
       public ReactiveCommand<Unit, IRoutableViewModel> GoToNextTool { get; set; }

       public ToolViewModel(IScreen host) : base(host)
       {
           Router.Navigate.Execute(new InnerViewModel(this, "Tool Home"));
       }

       public void InitNavigation(
           IRoutableViewModel? document1,
           IRoutableViewModel? document2,
           IRoutableViewModel? nextTool)
       {
           if (document1 is not null)
           {
               GoToDocument1 = ReactiveCommand.Create(() =>
                   HostScreen.Router.Navigate.Execute(document1).Subscribe(_ => { }));
           }

           if (document2 is not null)
           {
               GoToDocument2 = ReactiveCommand.Create(() =>
                   HostScreen.Router.Navigate.Execute(document2).Subscribe(_ => { }));
           }

           if (nextTool is not null)
           {
               GoToNextTool = ReactiveCommand.Create(() =>
                   HostScreen.Router.Navigate.Execute(nextTool).Subscribe(_ => { }));
           }
       }
   }
   ```

5. **Create inner routable view models**

   Create the view models that represent different pages/views within documents and tools:

   ```csharp
   using ReactiveUI;

   namespace MyDockApp.ViewModels;

   public class Page1ViewModel : ReactiveObject, IRoutableViewModel
   {
       public string? UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
       public IScreen HostScreen { get; }

       private string _content;
       public string Content
       {
           get => _content;
           set => this.RaiseAndSetIfChanged(ref _content, value);
       }

       public Page1ViewModel(IScreen screen, string content)
       {
           HostScreen = screen;
           _content = content;
       }
   }

   public class Page2ViewModel : ReactiveObject, IRoutableViewModel
   {
       public string? UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
       public IScreen HostScreen { get; }

       private string _content;
       public string Content
       {
           get => _content;
           set => this.RaiseAndSetIfChanged(ref _content, value);
       }

       private DateTime _timestamp = DateTime.Now;
       public DateTime Timestamp
       {
           get => _timestamp;
           set => this.RaiseAndSetIfChanged(ref _timestamp, value);
       }

       public Page2ViewModel(IScreen screen, string content)
       {
           HostScreen = screen;
           _content = content;
       }
   }

   public class InnerViewModel : ReactiveObject, IRoutableViewModel
   {
       public string? UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
       public IScreen HostScreen { get; }

       private string _message;
       public string Message
       {
           get => _message;
           set => this.RaiseAndSetIfChanged(ref _message, value);
       }

       public InnerViewModel(IScreen screen, string message)
       {
           HostScreen = screen;
           _message = message;
       }
   }
   ```

6. **Create the factory with routing setup**

   ```csharp
   using Dock.Model.Core;
   using Dock.Model.ReactiveUI;
   using Dock.Model.ReactiveUI.Controls;
   using Dock.Model.ReactiveUI.Navigation.Controls;
   using MyDockApp.ViewModels.Documents;
   using ReactiveUI;

   namespace MyDockApp.ViewModels;

   public class DockFactory : Factory
   {
       private readonly IScreen _host;

       public DockFactory(IScreen host)
       {
           _host = host;
       }

       public override IRootDock CreateLayout()
       {
           var document1 = new DocumentViewModel(_host) { Id = "Doc1", Title = "Document 1" };
           var document2 = new DocumentViewModel(_host) { Id = "Doc2", Title = "Document 2" };
           var tool1 = new ToolViewModel(_host) { Id = "Tool1", Title = "Tool 1" };
           var tool2 = new ToolViewModel(_host) { Id = "Tool2", Title = "Tool 2" };

           // Set up cross-navigation between view models
           document1.InitNavigation(document2, tool1, tool2);
           document2.InitNavigation(document1, tool1, tool2);
           tool1.InitNavigation(document1, document2, tool2);
           tool2.InitNavigation(document1, document2, tool1);

           var documentDock = new DocumentDock
           {
               Id = "Documents",
               VisibleDockables = CreateList<IDockable>(document1, document2),
               ActiveDockable = document1
           };

           var toolDock = new ToolDock
           {
               Id = "Tools",
               VisibleDockables = CreateList<IDockable>(tool1, tool2),
               ActiveDockable = tool1
           };

           var proportionalDock = CreateProportionalDock();
           proportionalDock.Orientation = Orientation.Horizontal;
           proportionalDock.VisibleDockables = CreateList<IDockable>(
               documentDock,
               CreateProportionalDockSplitter(),
               toolDock
           );

           var root = CreateRootDock();
           root.VisibleDockables = CreateList<IDockable>(proportionalDock);
           root.ActiveDockable = proportionalDock;
           root.DefaultDockable = proportionalDock;

           return root;
       }
   }
   ```

7. **Create views for routable view models**

   **DocumentView.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:rxui="http://reactiveui.net"
                x:Class="MyDockApp.Views.Documents.DocumentView">
     <DockPanel>
       <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="5">
         <Button Content="Page 1" Command="{Binding GoToPage1}" Margin="0,0,5,0" />
         <Button Content="Page 2" Command="{Binding GoToPage2}" />
       </StackPanel>
       <rxui:RoutedViewHost Router="{Binding Router}" />
     </DockPanel>
   </UserControl>
   ```

   **ToolView.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:rxui="http://reactiveui.net"
                x:Class="MyDockApp.Views.Tools.ToolView">
     <DockPanel>
       <StackPanel DockPanel.Dock="Top" Margin="5">
         <TextBlock Text="Tool Navigation" FontWeight="Bold" Margin="0,0,0,5" />
         <StackPanel Orientation="Horizontal">
           <Button Content="Go to Doc 1" Command="{Binding GoToDocument1}" Margin="0,0,5,0" />
           <Button Content="Go to Doc 2" Command="{Binding GoToDocument2}" Margin="0,0,5,0" />
           <Button Content="Next Tool" Command="{Binding GoToNextTool}" />
         </StackPanel>
       </StackPanel>
       <rxui:RoutedViewHost Router="{Binding Router}" />
     </DockPanel>
   </UserControl>
   ```

   **Page1View.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="MyDockApp.Views.Page1View">
     <StackPanel Margin="10">
       <TextBlock Text="Page 1" FontSize="16" FontWeight="Bold" Margin="0,0,0,10" />
       <TextBox Text="{Binding Content}" AcceptsReturn="True" Height="200" />
     </StackPanel>
   </UserControl>
   ```

   **Page2View.axaml:**
   ```xaml
   <UserControl xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Class="MyDockApp.Views.Page2View">
     <StackPanel Margin="10">
       <TextBlock Text="Page 2" FontSize="16" FontWeight="Bold" Margin="0,0,0,10" />
       <TextBox Text="{Binding Content}" AcceptsReturn="True" Height="100" Margin="0,0,0,10" />
       <TextBlock Text="{Binding Timestamp, StringFormat='Created: {0:yyyy-MM-dd HH:mm:ss}'}" />
     </StackPanel>
   </UserControl>
   ```

8. **Set up the main window and application**

   **MainWindow.axaml.cs:**
   ```csharp
   using Avalonia.Controls;
   using Dock.Avalonia.Controls;
   using MyDockApp.ViewModels;
   using ReactiveUI;

   namespace MyDockApp;

   public partial class MainWindow : Window, IScreen
   {
       public RoutingState Router { get; } = new RoutingState();

       public MainWindow()
       {
           InitializeComponent();
           InitializeDock();
       }

       private void InitializeDock()
       {
           var factory = new DockFactory(this);
           var layout = factory.CreateLayout();
           factory.InitLayout(layout);
           
           var dockControl = this.Find<DockControl>("Dock");
           if (dockControl != null)
           {
               dockControl.Layout = layout;
           }
       }
   }
   ```

   **MainWindow.axaml:**
   ```xaml
   <Window xmlns="https://github.com/avaloniaui"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           x:Class="MyDockApp.MainWindow"
           Title="Dock with ReactiveUI Routing">
     <DockControl x:Name="Dock" />
   </Window>
   ```

9. **Run the application**

   ```bash
   dotnet run
   ```

## ReactiveUI Routing Features

This approach provides several powerful features:

1. **Internal Navigation**: Each document or tool can have multiple internal views with navigation
2. **Cross-Dockable Navigation**: Navigate from any dockable to any other dockable
3. **State Management**: ReactiveUI's router maintains navigation history and state
4. **Dynamic Content**: Create rich, multi-page experiences within dockables
5. **URL-like Navigation**: Use UrlPathSegments for navigation tracking

## Key Concepts

- **IScreen**: The host that manages the router (typically your main window or view model)
- **RoutableDocument/RoutableTool**: Dockable implementations that include routing capabilities
- **IRoutableViewModel**: View models that can be navigated to within a router
- **RoutedViewHost**: XAML control that displays the current routed view
- **Router**: Manages navigation state and history

## Best Practices

1. **Navigation Setup**: Initialize navigation links after all view models are created
2. **Memory Management**: Dispose of commands and subscriptions properly
3. **State Persistence**: Consider how navigation state should be preserved during serialization
4. **User Experience**: Provide clear navigation cues and maintain consistent UX patterns

This pattern is excellent for creating complex applications that need rich navigation within a docking interface, such as IDEs, data analysis tools, or complex business applications.
