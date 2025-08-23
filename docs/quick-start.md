# Quick Start

This short guide shows how to set up Dock in a new Avalonia application. You will install the NuGet packages, create a minimal layout and run it.

## Step-by-step tutorial

1. **Create a new Avalonia project**
  

   *If you have not created an Avalonia project before, install the template package*
   ```bash
   dotnet new install Avalonia.Templates
   ```

   ```bash
   dotnet new avalonia.app -o DockQuickStart
   cd DockQuickStart
   ```

2. **Install the Dock packages**

   ```powershell
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Mvvm
   dotnet add package Dock.Avalonia.Themes.Fluent
   # or use Dock.Avalonia.Themes.Simple
   ```

   **Optional packages for specific scenarios:**
   ```powershell
   # For serialization (choose one):
   dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
   dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json) 
   dotnet add package Dock.Serializer.Protobuf          # Binary (protobuf-net)
   dotnet add package Dock.Serializer.Xml               # XML
   dotnet add package Dock.Serializer.Yaml              # YAML

   # For dependency injection:
   dotnet add package Dock.Model.Extensions.DependencyInjection

   # For diagnostics and debugging:
   dotnet add package Dock.Avalonia.Diagnostics
   ```

   **Note**: If you plan to use AXAML Controls `RootDock`, `ProportionalDock`, etc. also add:

   ```powershell
   dotnet add package Dock.Model.Avalonia
   ```

4. **Set up View Locator (Required)**

   Dock requires a view locator to map view models to their corresponding views. Choose one of the following approaches:

   **Option A: Static View Locator with Source Generators (Recommended)**

   Add the StaticViewLocator package:
   ```bash
   dotnet add package StaticViewLocator
   ```

   Create a `ViewLocator.cs` file:
   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using CommunityToolkit.Mvvm.ComponentModel;
   using Dock.Model.Core;
   using StaticViewLocator;

   namespace DockQuickStart;

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

   **Option B: Convention-Based View Locator (Avalonia Template Style)**

   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;

   namespace DockQuickStart;

   public class ViewLocator : IDataTemplate
   {
       public Control? Build(object? data)
       {
           if (data is null)
               return null;

           var name = data.GetType().FullName!.Replace("ViewModel", "View");
           var type = Type.GetType(name);

           if (type != null)
               return (Control)Activator.CreateInstance(type)!;

           return new TextBlock { Text = "Not Found: " + name };
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

           var name = data.GetType().FullName?.Replace("ViewModel", "View");
           return Type.GetType(name) is not null;
       }
   }
   ```

5. **Add Dock styles and View Locator**

   Reference the theme and register the view locator in `App.axaml`:

   ```xaml
   <Application xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:local="using:DockQuickStart"
                x:Class="DockQuickStart.App">

     <Application.DataTemplates>
       <local:ViewLocator />
     </Application.DataTemplates>

     <Application.Styles>
       <FluentTheme Mode="Dark" />
       <DockFluentTheme />
     </Application.Styles>
   </Application>
   ```

6. **Add a simple layout**

   **Option A: Traditional MVVM Approach**

   Create a `DockFactory` that derives from `Dock.Model.Mvvm.Factory` and returns a basic layout:

   ```csharp
   using Dock.Model.Core;
   using Dock.Model.Mvvm;
   using Dock.Model.Mvvm.Controls;

   public class DockFactory : Factory
   {
       public override IRootDock CreateLayout()
       {
           var document = new Document { Id = "Doc1", Title = "Document" };

           var root = CreateRootDock();
           root.VisibleDockables = CreateList<IDockable>(
               new DocumentDock
               {
                   VisibleDockables = CreateList<IDockable>(document),
                   ActiveDockable = document
               });
           root.DefaultDockable = root.VisibleDockables[0];
           return root;
       }
   }
   ```

   **Option B: Modern ItemsSource Approach (Recommended)**

   For more dynamic document management, use the ItemsSource approach. First, create a simple document model:

   ```csharp
   using System.ComponentModel;
   using System.Runtime.CompilerServices;

   public class FileDocument : INotifyPropertyChanged
   {
       private string _title = "Untitled";
       private string _content = "";

       public string Title
       {
           get => _title;
           set => SetProperty(ref _title, value);
       }

       public string Content
       {
           get => _content;
           set => SetProperty(ref _content, value);
       }

       public bool CanClose { get; set; } = true;

       public event PropertyChangedEventHandler? PropertyChanged;

       protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
       {
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
       }

       protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
       {
           if (EqualityComparer<T>.Default.Equals(field, value)) return false;
           field = value;
           OnPropertyChanged(propertyName);
           return true;
       }
   }
   ```

   Then create a MainWindow view model:

   ```csharp
   using System.Collections.ObjectModel;

   public class MainWindowViewModel
   {
       public ObservableCollection<FileDocument> Documents { get; } = new()
       {
           new FileDocument { Title = "Document 1", Content = "Content of document 1" },
           new FileDocument { Title = "Document 2", Content = "Content of document 2" }
       };
   }
   ```

   For the ItemsSource approach, set up the layout in XAML (`MainWindow.axaml`):

   ```xaml
   <Window x:Class="DockQuickStart.MainWindow"
           xmlns="https://github.com/avaloniaui"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           xmlns:dock="https://github.com/avaloniaui"
           x:DataType="MainWindowViewModel">
       <DockControl>
           <DocumentDock ItemsSource="{Binding Documents}">
               <DocumentDock.DocumentTemplate x:DataType="Document">
                   <DocumentTemplate>
                       <TextBox Text="(FileDocument)Context).Content" AcceptsReturn="True" />
                   </DocumentTemplate>
               </DocumentDock.DocumentTemplate>
           </DocumentDock>
       </DockControl>
   </Window>
   ```

   Initialize the view model in `MainWindow.axaml.cs`:

   ```csharp
   public partial class MainWindow : Window
   {
       public MainWindow()
       {
           InitializeComponent();
           DataContext = new MainWindowViewModel();
       }
   }
   ```

   **For Traditional MVVM (Option A)**, initialize the layout in `MainWindow.axaml.cs`:

   ```csharp
   public partial class MainWindow : Window
   {
       private readonly DockFactory _factory = new();

       public MainWindow()
       {
           InitializeComponent();
           var layout = _factory.CreateLayout();
           _factory.InitLayout(layout);
           Dock.Layout = layout;
       }
   }
   ```

   And add a `DockControl` placeholder to `MainWindow.axaml`:

   ```xaml
   <DockControl x:Name="Dock" />
   ```

   > **💡 Tip**: The ItemsSource approach (Option B) is recommended for most scenarios as it provides automatic document management, data binding, and easier maintenance. See the [DocumentDock ItemsSource guide](dock-itemssource.md) for more details.

   For instructions on mapping documents and tools to views see the [Views guide](dock-views.md).

7. **Run the application**

   ```bash
   dotnet run
   ```

The window should show document tabs that can be opened, closed, and managed automatically. The ItemsSource approach automatically creates document instances from your data models and keeps them synchronized with the collection.

For more advanced scenarios including tools, complex layouts, and custom content, see:
- [DocumentDock ItemsSource guide](dock-itemssource.md) - Complete ItemsSource documentation
- [Document and Tool Content Guide](dock-content-guide.md) - Comprehensive content setup
- [MVVM guide](dock-mvvm.md) - Traditional MVVM approach 
- [Complex layout tutorials](dock-complex-layouts.md) - Multi-pane layouts
