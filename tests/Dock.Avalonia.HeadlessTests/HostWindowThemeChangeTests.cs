using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowThemeChangeTests
{
    private static IRootDock CreateLayout(Factory factory)
    {
        var root = factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();
        return root;
    }

    [AvaloniaFact]
    public void HostWindow_ThemeChange_DoesNotDuplicate_DockControls()
    {
        var app = Application.Current ?? throw new InvalidOperationException("Application is not initialized.");
        var factory = new Factory();
        var layout = CreateLayout(factory);
        var window = new HostWindow();
        var originalVariant = app.RequestedThemeVariant;

        try
        {
            window.SetLayout(layout);
            window.Show();
            window.UpdateLayout();

            Assert.Single(factory.DockControls);
            Assert.Same(layout, window.Content);

            app.RequestedThemeVariant = ThemeVariant.Dark;
            window.UpdateLayout();
            Assert.Single(factory.DockControls);

            app.RequestedThemeVariant = ThemeVariant.Light;
            window.UpdateLayout();
            Assert.Single(factory.DockControls);
        }
        finally
        {
            window.Close();
            app.RequestedThemeVariant = originalVariant;
        }
    }

    [AvaloniaFact]
    public void DockControl_Should_Ignore_Floating_Tool_Chrome_WindowDrag_Press()
    {
        var app = Application.Current ?? throw new InvalidOperationException("Application is not initialized.");
        var method = typeof(DockControl).GetMethod("ShouldIgnorePressedForWindowDrag", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var previousStyles = app.Styles.ToList();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());
        app.Styles.Add(new DockFluentTheme());

        var tool = new Tool { Title = "Tool1" };
        var toolDock = new ToolDock
        {
            VisibleDockables = new AvaloniaList<IDockable> { tool },
            ActiveDockable = tool,
            OpenedDockablesCount = 1
        };
        var layout = new RootDock
        {
            VisibleDockables = new AvaloniaList<IDockable> { toolDock },
            ActiveDockable = toolDock,
            DefaultDockable = toolDock,
            OpenedDockablesCount = 1
        };
        var hostWindow = new HostWindow();

        try
        {
            hostWindow.SetLayout(layout);
            hostWindow.Show();
            hostWindow.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var toolChrome = hostWindow.GetVisualDescendants().OfType<ToolChromeControl>().FirstOrDefault();
            Assert.NotNull(toolChrome);
            Assert.True(hostWindow.IsToolWindow);
            Assert.Same(hostWindow, TopLevel.GetTopLevel(toolChrome));

            var result = (bool?)method!.Invoke(null, new object?[] { toolChrome! });
            Assert.True(result);
        }
        finally
        {
            hostWindow.Close();
            app.Styles.Clear();
            foreach (IStyle style in previousStyles)
            {
                app.Styles.Add(style);
            }
        }
    }
}
