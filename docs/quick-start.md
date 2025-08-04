# Quick Start

This short guide shows how to set up Dock in a new Avalonia application. You will install the NuGet packages, create a minimal layout and run it.

## Step-by-step tutorial

1. **Create a new Avalonia project**

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

4. **Add Dock styles**

   Reference one of the built-in themes in `App.axaml` so the controls are styled:

   ```xaml
   <Application.Styles>
     <FluentTheme Mode="Dark" />
     <DockFluentTheme />
   </Application.Styles>
   ```

5. **Add a simple layout**

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
           xmlns:dock="https://github.com/avaloniaui">
       <DockControl>
           <DocumentDock ItemsSource="{Binding Documents}">
               <DocumentDock.DocumentTemplate>
                   <DocumentTemplate>
                       <TextBox Text="{Binding Content}" AcceptsReturn="True" />
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

6. **Run the application**

   ```bash
   dotnet run
   ```

The window should show document tabs that can be opened, closed, and managed automatically. The ItemsSource approach automatically creates document instances from your data models and keeps them synchronized with the collection.

For more advanced scenarios including tools, complex layouts, and custom content, see:
- [DocumentDock ItemsSource guide](dock-itemssource.md) - Complete ItemsSource documentation
- [Document and Tool Content Guide](dock-content-guide.md) - Comprehensive content setup
- [MVVM guide](dock-mvvm.md) - Traditional MVVM approach 
- [Complex layout tutorials](dock-complex-layouts.md) - Multi-pane layouts
