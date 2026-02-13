using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using System;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>
/// Custom IHostWindow implementation for testing the new DockControl main window functionality
/// </summary>
public class TestHostWindow : Window, IHostWindow
{
    private readonly HostWindowState _hostWindowState;
    private bool _isTracked;
    private IDockWindow? _window;
    private bool _presented;
    private bool _exited;
    private double _x, _y, _width, _height;
    private DockWindowState _windowState = DockWindowState.Normal;
    private string? _title;
    private IDock? _layout;
    private bool _activated;

    public TestHostWindow()
    {
        _hostWindowState = new HostWindowState(new DockManager(new DockService()), new HostWindow());
        Width = 800;
        Height = 600;
    }

    public IHostWindowState? HostWindowState => _hostWindowState;

    public bool IsTracked
    {
        get => _isTracked;
        set => _isTracked = value;
    }

    public IDockWindow? Window
    {
        get => _window;
        set => _window = value;
    }

    public bool Presented => _presented;
    public bool Exited => _exited;
    public bool IsActivated => _activated;
    public double TestX => _x;
    public double TestY => _y;
    public double TestWidth => _width;
    public double TestHeight => _height;
    public string? TestTitle => _title;
    public IDock? TestLayout => _layout;
    public DockWindowState TestWindowState => _windowState;

    public void Present(bool isDialog)
    {
        _presented = true;
        // In headless mode, we don't actually show the window
    }

    public void Exit()
    {
        _exited = true;
        // In headless mode, we don't actually close the window
    }

    public void SetPosition(double x, double y)
    {
        _x = x;
        _y = y;
        Position = new PixelPoint((int)x, (int)y);
    }

    public void GetPosition(out double x, out double y)
    {
        x = _x;
        y = _y;
    }

    public void SetSize(double width, double height)
    {
        _width = width;
        _height = height;
        Width = width;
        Height = height;
    }

    public void GetSize(out double width, out double height)
    {
        width = _width;
        height = _height;
    }

    public void SetWindowState(DockWindowState windowState)
    {
        _windowState = windowState;
    }

    public DockWindowState GetWindowState()
    {
        return _windowState;
    }

    public void SetTitle(string? title)
    {
        _title = title;
        Title = title;
    }

    public void SetLayout(IDock layout)
    {
        _layout = layout;
    }

    public void SetActive()
    {
        _activated = true;
        // In headless mode, we don't actually activate the window
    }
}

public class DockControlMainWindowTests
{
    [AvaloniaFact]
    public void Factory_InitDockWindow_With_HostWindow_Works()
    {
        // Arrange
        var factory = new Factory();
        var layout = factory.CreateLayout();
        layout.Factory = factory;

        var hostWindow = new TestHostWindow();
        var root = factory.FindRoot(layout);
        Assert.NotNull(root);
        var windowModel = factory.CreateWindowFrom(root);
        Assert.NotNull(windowModel);

        // Act
        factory.InitDockWindow(windowModel, root, hostWindow);

        // Assert
        Assert.NotNull(windowModel.Host);
        Assert.Same(hostWindow, windowModel.Host);
    }

    [AvaloniaFact]
    public void Factory_InitDockWindow_Without_HostWindow_Works()
    {
        // Arrange
        var factory = new Factory();
        var layout = factory.CreateLayout();
        layout.Factory = factory;

        var root = factory.FindRoot(layout);
        Assert.NotNull(root);
        var windowModel = factory.CreateWindowFrom(root);
        Assert.NotNull(windowModel);

        // Act
        factory.InitDockWindow(windowModel, root, null);

        // Assert
        // Should not throw when hostWindow is null
    }

    [AvaloniaFact]
    public void HostWindow_Position_And_Size_Methods_Work()
    {
        // Arrange
        var hostWindow = new TestHostWindow();

        // Act
        hostWindow.SetPosition(100, 200);
        hostWindow.SetSize(800, 600);

        // Assert
        hostWindow.GetPosition(out var x, out var y);
        hostWindow.GetSize(out var width, out var height);

        Assert.Equal(100, x);
        Assert.Equal(200, y);
        Assert.Equal(800, width);
        Assert.Equal(600, height);
    }

    [AvaloniaFact]
    public void HostWindow_WindowState_Methods_Work()
    {
        var hostWindow = new TestHostWindow();

        hostWindow.SetWindowState(DockWindowState.Maximized);
        var state = hostWindow.GetWindowState();

        Assert.Equal(DockWindowState.Maximized, state);
        Assert.Equal(DockWindowState.Maximized, hostWindow.TestWindowState);
    }

    [AvaloniaFact]
    public void HostWindow_Title_Setting_Works()
    {
        // Arrange
        var hostWindow = new TestHostWindow();
        var testTitle = "Test Window Title";

        // Act
        hostWindow.SetTitle(testTitle);

        // Assert
        Assert.Equal(testTitle, hostWindow.TestTitle);
        Assert.Equal(testTitle, hostWindow.Title);
    }

    [AvaloniaFact]
    public void HostWindow_Layout_Setting_Works()
    {
        // Arrange
        var hostWindow = new TestHostWindow();
        var factory = new Factory();
        var layout = factory.CreateLayout();

        // Act
        hostWindow.SetLayout(layout);

        // Assert
        Assert.Same(layout, hostWindow.TestLayout);
    }

    [AvaloniaFact]
    public void HostWindow_Present_And_Exit_Methods_Work()
    {
        // Arrange
        var hostWindow = new TestHostWindow();

        // Act
        hostWindow.Present(false);
        hostWindow.Exit();

        // Assert
        Assert.True(hostWindow.Presented);
        Assert.True(hostWindow.Exited);
    }

    [AvaloniaFact]
    public void HostWindow_SetActive_Method_Works()
    {
        // Arrange
        var hostWindow = new TestHostWindow();

        // Act
        hostWindow.SetActive();

        // Assert
        Assert.True(hostWindow.IsActivated);
    }

    [AvaloniaFact]
    public void HostWindow_IsTracked_Property_Works()
    {
        // Arrange
        var hostWindow = new TestHostWindow();

        // Act
        hostWindow.IsTracked = true;

        // Assert
        Assert.True(hostWindow.IsTracked);

        // Act
        hostWindow.IsTracked = false;

        // Assert
        Assert.False(hostWindow.IsTracked);
    }

    [AvaloniaFact]
    public void HostWindow_Window_Property_Works()
    {
        // Arrange
        var hostWindow = new TestHostWindow();
        var factory = new Factory();
        var window = factory.CreateDockWindow();

        // Act
        hostWindow.Window = window;

        // Assert
        Assert.Same(window, hostWindow.Window);
    }

    [AvaloniaFact]
    public void HostWindow_HostWindowState_Property_Works()
    {
        // Arrange
        var hostWindow = new TestHostWindow();

        // Assert
        Assert.NotNull(hostWindow.HostWindowState);
        Assert.IsType<HostWindowState>(hostWindow.HostWindowState);
    }

    [AvaloniaFact]
    public void HostWindow_State_Management_Works_With_Factory()
    {
        // Arrange
        var factory = new Factory();
        var layout = factory.CreateLayout();
        layout.Factory = factory;

        var hostWindow = new TestHostWindow();
        var root = factory.FindRoot(layout);
        Assert.NotNull(root);
        var windowModel = factory.CreateWindowFrom(root);
        Assert.NotNull(windowModel);

        // Act
        factory.InitDockWindow(windowModel, root, hostWindow);

        // Test host window state
        hostWindow.SetPosition(150, 250);
        hostWindow.SetSize(900, 700);
        hostWindow.SetTitle("State Test Window");
        hostWindow.IsTracked = true;

        // Assert
        Assert.Equal(150, hostWindow.TestX);
        Assert.Equal(250, hostWindow.TestY);
        Assert.Equal(900, hostWindow.TestWidth);
        Assert.Equal(700, hostWindow.TestHeight);
        Assert.Equal("State Test Window", hostWindow.TestTitle);
        Assert.True(hostWindow.IsTracked);
        Assert.NotNull(hostWindow.HostWindowState);
        Assert.Same(hostWindow, windowModel.Host);
    }

    [AvaloniaFact]
    public void Factory_InitDockWindow_Multiple_Times_Works()
    {
        // Arrange
        var factory = new Factory();
        var layout1 = factory.CreateLayout();
        layout1.Factory = factory;

        var hostWindow = new TestHostWindow();
        var root1 = factory.FindRoot(layout1);
        Assert.NotNull(root1);
        var windowModel1 = factory.CreateWindowFrom(root1);
        Assert.NotNull(windowModel1);

        // Act - Initialize first window
        factory.InitDockWindow(windowModel1, root1, hostWindow);

        // Create second layout and window
        var layout2 = factory.CreateLayout();
        layout2.Factory = factory;
        var root2 = factory.FindRoot(layout2);
        Assert.NotNull(root2);
        var windowModel2 = factory.CreateWindowFrom(root2);
        Assert.NotNull(windowModel2);

        // Act - Initialize second window
        factory.InitDockWindow(windowModel2, root2, hostWindow);

        // Assert
        Assert.Same(hostWindow, windowModel1.Host);
        Assert.Same(hostWindow, windowModel2.Host);
        Assert.NotSame(windowModel1, windowModel2);
    }

    [AvaloniaFact]
    public void MainWindow_Close_Canceled_DoesNotExitFloatingWindows()
    {
        var originalSetting = DockSettings.CloseFloatingWindowsOnMainWindowClose;
        DockSettings.CloseFloatingWindowsOnMainWindowClose = true;

        Window? mainWindow = null;
        var cancelClose = true;
        try
        {
            var factory = new Factory();
            var layout = factory.CreateLayout();
            layout.Factory = factory;
            var root = factory.FindRoot(layout) as IRootDock;
            Assert.NotNull(root);

            var floatingHost = new TestHostWindow();
            var floatingWindow = factory.CreateDockWindow();
            floatingWindow.Layout = factory.CreateRootDock();
            factory.InitDockWindow(floatingWindow, root, floatingHost);
            root!.Windows ??= factory.CreateList<IDockWindow>();
            root.Windows!.Add(floatingWindow);

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = layout
            };

            mainWindow = new Window
            {
                Width = 800,
                Height = 600,
                Content = dockControl
            };
            mainWindow.Show();

            mainWindow.Closing += (_, e) =>
            {
                if (cancelClose)
                {
                    e.Cancel = true;
                }
            };

            mainWindow.Close();
            Dispatcher.UIThread.RunJobs();

            Assert.True(mainWindow.IsVisible);
            Assert.False(floatingHost.Exited);

            cancelClose = false;
            mainWindow.Close();
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            cancelClose = false;
            if (mainWindow?.IsVisible == true)
            {
                mainWindow.Close();
                Dispatcher.UIThread.RunJobs();
            }

            DockSettings.CloseFloatingWindowsOnMainWindowClose = originalSetting;
        }
    }

    [AvaloniaFact]
    public void MainWindow_Close_ExitsFloatingWindows()
    {
        var originalSetting = DockSettings.CloseFloatingWindowsOnMainWindowClose;
        DockSettings.CloseFloatingWindowsOnMainWindowClose = true;

        Window? mainWindow = null;
        try
        {
            var factory = new Factory();
            var layout = factory.CreateLayout();
            layout.Factory = factory;
            var root = factory.FindRoot(layout) as IRootDock;
            Assert.NotNull(root);

            var floatingHost = new TestHostWindow();
            var floatingWindow = factory.CreateDockWindow();
            floatingWindow.Layout = factory.CreateRootDock();
            factory.InitDockWindow(floatingWindow, root, floatingHost);
            root!.Windows ??= factory.CreateList<IDockWindow>();
            root.Windows!.Add(floatingWindow);

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = layout
            };

            mainWindow = new Window
            {
                Width = 800,
                Height = 600,
                Content = dockControl
            };
            mainWindow.Show();

            mainWindow.Close();
            Dispatcher.UIThread.RunJobs();

            Assert.False(mainWindow.IsVisible);
            Assert.True(floatingHost.Exited);
        }
        finally
        {
            if (mainWindow?.IsVisible == true)
            {
                mainWindow.Close();
                Dispatcher.UIThread.RunJobs();
            }

            DockSettings.CloseFloatingWindowsOnMainWindowClose = originalSetting;
        }
    }

    [AvaloniaFact]
    public void DockControl_With_HostWindow_Integration_Test()
    {
        // Arrange
        var factory = new Factory();
        var layout = factory.CreateLayout();
        layout.Factory = factory;

        var hostWindow = new TestHostWindow();
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = layout
        };

        hostWindow.Content = dockControl;

        // Act - Create window model manually to simulate what DockControl does
        var root = factory.FindRoot(layout);
        Assert.NotNull(root);
        var windowModel = factory.CreateWindowFrom(root);
        Assert.NotNull(windowModel);
        factory.InitDockWindow(windowModel, root, hostWindow);
        root.Window = windowModel;

        // Assert
        Assert.NotNull(root.Window);
        Assert.NotNull(root.Window.Host);
        Assert.Same(hostWindow, root.Window.Host);
        Assert.Same(root.Window, hostWindow.Window);
        Assert.Contains(dockControl, factory.DockControls);
    }
} 
