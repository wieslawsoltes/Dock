using System;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowTitleBarChromeTests
{
    [AvaloniaFact]
    public void HostWindowTitleBar_Uses_Window_Decoration_Roles()
    {
        var (window, titleBar) = ShowHostWindow();

        try
        {
            var mouseTracker = titleBar.GetVisualDescendants()
                .OfType<Panel>()
                .FirstOrDefault(control => control.Name == "PART_MouseTracker");
            var container = titleBar.GetVisualDescendants()
                .OfType<Grid>()
                .FirstOrDefault(control => control.Name == "PART_Container");
            var captionButtons = titleBar.GetVisualDescendants()
                .OfType<StackPanel>()
                .FirstOrDefault(control => control.Name == "PART_CaptionButtons");

            Assert.NotNull(mouseTracker);
            Assert.NotNull(container);
            Assert.NotNull(captionButtons);
            Assert.NotNull(titleBar.CloseButton);
            Assert.NotNull(titleBar.MinimizeButton);
            Assert.NotNull(titleBar.MaximizeRestoreButton);

            Assert.Equal(WindowDecorationsElementRole.TitleBar, WindowDecorationProperties.GetElementRole(mouseTracker!));
            Assert.Equal(WindowDecorationsElementRole.TitleBar, WindowDecorationProperties.GetElementRole(container!));
            Assert.Equal(WindowDecorationsElementRole.DecorationsElement, WindowDecorationProperties.GetElementRole(captionButtons!));
            Assert.Equal(WindowDecorationsElementRole.CloseButton, WindowDecorationProperties.GetElementRole(titleBar.CloseButton!));
            Assert.Equal(WindowDecorationsElementRole.MinimizeButton, WindowDecorationProperties.GetElementRole(titleBar.MinimizeButton!));
            Assert.Equal(WindowDecorationsElementRole.MaximizeButton, WindowDecorationProperties.GetElementRole(titleBar.MaximizeRestoreButton!));

            if (OperatingSystem.IsMacOS())
            {
                Assert.Same(titleBar.CloseButton, captionButtons.Children[0]);
                Assert.Same(titleBar.MinimizeButton, captionButtons.Children[1]);
                Assert.Same(titleBar.MaximizeRestoreButton, captionButtons.Children[2]);
            }
            else
            {
                Assert.Same(titleBar.MinimizeButton, captionButtons.Children[0]);
                Assert.Same(titleBar.MaximizeRestoreButton, captionButtons.Children[1]);
                Assert.Same(titleBar.CloseButton, captionButtons.Children[2]);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HostWindowTitleBar_Tracks_Window_State_And_Button_Capabilities()
    {
        var (window, titleBar) = ShowHostWindow();

        try
        {
            Assert.DoesNotContain(":maximized", titleBar.Classes);
            Assert.DoesNotContain(":fullscreen", titleBar.Classes);
            Assert.True(titleBar.MinimizeButton!.IsVisible);
            Assert.True(titleBar.MaximizeRestoreButton!.IsVisible);

            window.WindowState = WindowState.Maximized;
            Dispatcher.UIThread.RunJobs();

            Assert.Contains(":maximized", titleBar.Classes);
            Assert.DoesNotContain(":fullscreen", titleBar.Classes);

            window.WindowState = WindowState.FullScreen;
            Dispatcher.UIThread.RunJobs();

            Assert.Contains(":fullscreen", titleBar.Classes);
            Assert.DoesNotContain(":maximized", titleBar.Classes);

            window.CanMinimize = false;
            window.CanMaximize = false;
            window.CanResize = false;
            Dispatcher.UIThread.RunJobs();

            Assert.False(titleBar.MinimizeButton.IsVisible);
            Assert.False(titleBar.MaximizeRestoreButton.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HostWindow_DocumentChromeMode_Uses_Theme_Driven_TitleBar_Extension()
    {
        var window = new HostWindow
        {
            Width = 900,
            Height = 600,
            DocumentChromeControlsWholeWindow = true,
            Content = new TextBlock { Text = "Dock" }
        };

        window.Styles.Add(new DockFluentTheme());
        window.Show();
        window.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            Assert.Contains(":documentwindow", window.Classes);
            Assert.Contains(":documentchromecontrolswindow", window.Classes);
            Assert.True(window.ExtendClientAreaToDecorationsHint);
            Assert.Equal(WindowDecorations.None, window.WindowDecorations);
            Assert.Equal(30d, window.ExtendClientAreaTitleBarHeightHint);

            var titleBar = window.GetVisualDescendants()
                .OfType<HostWindowTitleBar>()
                .FirstOrDefault();

            Assert.NotNull(titleBar);
            Assert.Equal(30d, titleBar!.Height);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HostWindow_ToolChromeMode_Is_Applied_Before_Show()
    {
        var tool = new Tool
        {
            Id = "Tool1",
            Title = "Tool1"
        };

        var toolDock = new ToolDock
        {
            Id = "ToolDock",
            ActiveDockable = tool,
            VisibleDockables = new AvaloniaList<IDockable> { tool }
        };

        var layout = new RootDock
        {
            Id = "Root",
            ActiveDockable = toolDock,
            OpenedDockablesCount = 1,
            VisibleDockables = new AvaloniaList<IDockable> { toolDock }
        };

        var window = new HostWindow
        {
            Width = 900,
            Height = 600
        };

        window.Styles.Add(new DockFluentTheme());
        window.SetLayout(layout);

        Assert.True(window.IsToolWindow);
        Assert.Contains(":toolwindow", window.Classes);
        Assert.Contains(":toolchromecontrolswindow", window.Classes);

        window.Show();
        window.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            Assert.True(window.ExtendClientAreaToDecorationsHint);
            Assert.Equal(WindowDecorations.None, window.WindowDecorations);

            var titleBar = window.GetVisualDescendants()
                .OfType<HostWindowTitleBar>()
                .FirstOrDefault();

            Assert.Null(titleBar);
        }
        finally
        {
            window.Close();
        }
    }

    private static (HostWindow Window, HostWindowTitleBar TitleBar) ShowHostWindow()
    {
        var window = new HostWindow
        {
            Width = 900,
            Height = 600,
            ExtendClientAreaToDecorationsHint = true,
            WindowDecorations = WindowDecorations.None,
            Content = new TextBlock { Text = "Dock" }
        };

        window.Styles.Add(new DockFluentTheme());
        window.Show();
        window.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var titleBar = window.GetVisualDescendants()
            .OfType<HostWindowTitleBar>()
            .FirstOrDefault();

        Assert.NotNull(titleBar);

        titleBar!.ApplyTemplate();
        titleBar.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        return (window, titleBar);
    }
}
