# Programmatic Floating Window Example

This example demonstrates how to programmatically create floating windows with custom UserControl content in a non-MVVM application.

## Complete Working Example

```csharp
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace FloatingWindowExample;

// Custom UserControl for the utility
public class ExampleUtilityUserControl : UserControl
{
    public ExampleUtilityUserControl()
    {
        var stackPanel = new StackPanel
        {
            Margin = new Thickness(10),
            Spacing = 10
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = "Example Utility",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = "This is a custom UserControl that can be floated as a separate window.",
            TextWrapping = TextWrapping.Wrap
        });

        stackPanel.Children.Add(new Button
        {
            Content = "Click Me",
            HorizontalAlignment = HorizontalAlignment.Left
        });

        Content = stackPanel;
    }
}

// Main application
public class App : Application
{
    private Factory? _factory;
    private RootDock? _rootDock;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new DockFluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dockControl = new DockControl();
            _factory = new Factory();

            // Create the main document area
            var documentDock = new DocumentDock
            {
                Id = "Documents",
                IsCollapsable = false,
                CanCreateDocument = true
            };

            var welcomeDocument = new Document
            {
                Id = "Welcome",
                Title = "Welcome",
                Content = new Func<IServiceProvider, object>(_ => new TextBlock
                {
                    Text = "Click the button below to create a floating utility window.",
                    Margin = new Thickness(20),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                })
            };
            
            documentDock.VisibleDockables = _factory.CreateList<IDockable>(welcomeDocument);
            documentDock.ActiveDockable = welcomeDocument;

            // Create a tool dock with a button to create floating utilities
            var controlPanel = new StackPanel 
            { 
                Margin = new Thickness(10), 
                Spacing = 5 
            };
            
            var createUtilityButton = new Button 
            { 
                Content = "Create Floating Utility",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            createUtilityButton.Click += (s, e) => CreateNewFloatingUtility();
            controlPanel.Children.Add(createUtilityButton);

            var leftTool = new Tool
            {
                Id = "ControlPanel",
                Title = "Control Panel",
                Content = new Func<IServiceProvider, object>(_ => controlPanel)
            };

            var leftToolDock = new ToolDock
            {
                Id = "LeftPane",
                Alignment = Alignment.Left,
                Proportion = 0.2,
                VisibleDockables = _factory.CreateList<IDockable>(leftTool),
                ActiveDockable = leftTool
            };

            // Create the main layout
            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = _factory.CreateList<IDockable>(
                    leftToolDock,
                    new ProportionalDockSplitter(),
                    documentDock
                )
            };

            // Create root dock
            _rootDock = _factory.CreateRootDock();
            _rootDock.VisibleDockables = _factory.CreateList<IDockable>(mainLayout);
            _rootDock.DefaultDockable = mainLayout;
            _rootDock.ActiveDockable = mainLayout;

            _factory.InitLayout(_rootDock);
            
            dockControl.Factory = _factory;
            dockControl.Layout = _rootDock;

            desktop.MainWindow = new Window
            {
                Title = "Floating Window Example",
                Width = 1000,
                Height = 600,
                Content = dockControl
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void CreateNewFloatingUtility()
    {
        if (_factory == null || _rootDock == null)
            return;

        // Create a UserControl instance
        var userControl = new ExampleUtilityUserControl();

        // Wrap it in a Tool with function-based content
        var tool = new Tool
        {
            Id = $"Utility{Guid.NewGuid()}",
            Title = $"Utility {DateTime.Now:HH:mm:ss}",
            CanClose = true,
            CanFloat = true,
            // Use function-based content to wrap the UserControl
            Content = new Func<IServiceProvider, object>(_ => userControl)
        };

        // Float the tool to create a floating window
        // This creates a new window that can be docked back into the main window
        _factory.FloatDockable(tool);
    }
}

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
```

## Key Points

1. **Function-Based Content**: The UserControl is wrapped in a `Func<IServiceProvider, object>` to avoid the "Unexpected content" error:
   ```csharp
   Content = new Func<IServiceProvider, object>(_ => userControl)
   ```

2. **FloatDockable**: The `FloatDockable` method automatically:
   - Creates a window structure (RootDock + ToolDock wrapper)
   - Opens the window at an appropriate position
   - Enables the window to be docked back into the main window

3. **Docking Behavior**: The floating window can be dragged and docked into any valid drop target in the main window, just like windows created by dragging tabs out of the main window.

4. **Alternative: ViewModel Pattern**: For MVVM applications, create a ViewModel that inherits from `Tool` and use DataTemplates instead of function-based content. See the [Document and Tool Content Guide](dock-content-guide.md) for details.

## Running the Example

1. Build and run the application
2. Click the "Create Floating Utility" button
3. A new floating window appears with the custom UserControl content
4. Drag the floating window over the main window to see docking indicators
5. Drop it in any valid location to dock it back into the main window

## Troubleshooting

**Error: "Unexpected content"**
- Make sure you're using the function-based content pattern, not setting Content directly to a UserControl instance

**Error: "Unable to cast object of type 'UserControl' to type 'TemplateResult'"**
- This should be fixed with the latest version. If you still see this, ensure you're using Dock version that includes the TemplateHelper fix

**Floating window doesn't dock**
- Ensure your RootDock is properly initialized with `factory.InitLayout(root)`
- Make sure `CanFloat` and `CanDrop` are not set to false on the dockable or dock

## See Also

- [Floating Windows](dock-windows.md)
- [Document and Tool Content Guide](dock-content-guide.md)
- [Dock FAQ](dock-faq.md)
