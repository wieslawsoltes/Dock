using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowMethodsTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_MoveDrag_Uses_Factory_Cancellation_And_Ends_On_Pointer_Release(bool cancel)
    {
        Factory factory = new();
        HostWindow host = new() { Window = new DockWindow { Factory = factory }, Width = 400, Height = 300 };
        int began = 0;
        int ended = 0;
        bool? accepted = null;
        factory.WindowMoveDragBegin += (_, args) => { began++; args.Cancel = cancel; };
        factory.WindowMoveDragEnd += (_, _) => ended++;
        host.PointerPressed += (_, args) => accepted = host.TryBeginMoveDrag(args, WindowDragDockScope.FullWindow);
        try
        {
            host.Show();
            host.MouseDown(new Point(10, 10), MouseButton.Left);
            Assert.Equal(!cancel, accepted);
            Assert.Equal(1, began);
            Assert.Equal(!cancel, host.Classes.Contains(":dragging"));
            host.MouseUp(new Point(10, 10), MouseButton.Left);
            Assert.DoesNotContain(":dragging", host.Classes);
            Assert.Equal(cancel ? 0 : 1, ended);
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public void Explicit_MoveDrag_Rejects_A_Missing_Pointer_Press()
    {
        HostWindow host = new();
        Assert.Throws<ArgumentNullException>(() => host.TryBeginMoveDrag(null!, WindowDragDockScope.FullWindow));
    }

    [AvaloniaFact]
    public void Reopening_A_Hidden_Host_Does_Not_Duplicate_The_Native_Registration()
    {
        Factory factory = new();
        HostWindow host = new() { Window = new DockWindow { Factory = factory } };
        try
        {
            for (int i = 0; i < 3; i++)
            {
                host.Present(false);
                Assert.Same(host, Assert.Single(factory.HostWindows));
                host.Hide();
                Assert.Same(host, Assert.Single(factory.HostWindows));
            }
        }
        finally { host.Close(); }
        Assert.Empty(factory.HostWindows);
    }

    [AvaloniaFact]
    public void SetPosition_And_GetPosition_Work()
    {
        var window = new HostWindow();
        window.SetPosition(100, 50);
        window.GetPosition(out var x, out var y);
        Assert.Equal(100, x);
        Assert.Equal(50, y);
    }

    [AvaloniaFact]
    public void SetSize_And_GetSize_Work()
    {
        var window = new HostWindow();
        window.SetSize(800, 600);
        window.GetSize(out var w, out var h);
        Assert.Equal(800, w);
        Assert.Equal(600, h);
    }

    [AvaloniaFact]
    public void SetPosition_Ignores_NaN()
    {
        var window = new HostWindow();
        window.SetPosition(double.NaN, double.NaN);
        window.GetPosition(out var x, out var y);
        Assert.Equal(0, x);
        Assert.Equal(0, y);
    }

    [AvaloniaFact]
    public void SetSize_Ignores_NaN()
    {
        var window = new HostWindow();
        window.SetSize(double.NaN, double.NaN);
        window.GetSize(out var w, out var h);
        Assert.True(double.IsNaN(w));
        Assert.True(double.IsNaN(h));
    }

    [AvaloniaFact]
    public void SetWindowState_And_GetWindowState_Work()
    {
        var window = new HostWindow();

        window.SetWindowState(DockWindowState.Maximized);

        Assert.Equal(DockWindowState.Maximized, window.GetWindowState());
    }
}
