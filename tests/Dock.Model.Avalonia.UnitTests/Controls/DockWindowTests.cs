using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DockWindowTests
{
    private class FakeHostWindow : IHostWindow
    {
        public IDockManager? DockManager => null;
        public IHostWindowState? HostWindowState => null;
        public bool IsTracked { get; set; }
        public IDockWindow? Window { get; set; }
        public List<string> Calls { get; } = new();

        private double _x, _y, _w, _h;
        private string? _title;
        private IDock? _layout;

        public void Present(bool isDialog) => Calls.Add($"Present:{isDialog}");
        public void Exit() => Calls.Add("Exit");
        public void SetPosition(double x, double y) { _x = x; _y = y; Calls.Add($"SetPosition:{x}:{y}"); }
        public void GetPosition(out double x, out double y) { Calls.Add("GetPosition"); x = _x; y = _y; }
        public void SetSize(double width, double height) { _w = width; _h = height; Calls.Add($"SetSize:{width}:{height}"); }
        public void GetSize(out double width, out double height) { Calls.Add("GetSize"); width = _w; height = _h; }
        public void SetTitle(string title) { _title = title; Calls.Add($"SetTitle:{title}"); }
        public void SetLayout(IDock layout) { _layout = layout; Calls.Add("SetLayout"); }
    }

    [AvaloniaFact]
    public void Save_Reads_Position_And_Size_From_Host()
    {
        var window = new DockWindow();
        var host = new FakeHostWindow();
        host.SetPosition(10, 20);
        host.SetSize(300, 400);
        window.Host = host;

        window.Save();

        Assert.Equal(10, window.X);
        Assert.Equal(20, window.Y);
        Assert.Equal(300, window.Width);
        Assert.Equal(400, window.Height);
        Assert.Contains("GetPosition", host.Calls);
        Assert.Contains("GetSize", host.Calls);
    }

    [AvaloniaFact]
    public void Present_Uses_Host_To_Show_Window()
    {
        var window = new DockWindow
        {
            X = 1,
            Y = 2,
            Width = 100,
            Height = 200,
            Title = "Test",
            Layout = new RootDock(),
            Host = new FakeHostWindow()
        };
        var host = (FakeHostWindow)window.Host!;

        window.Present(false);

        Assert.Contains("SetPosition:1:2", host.Calls);
        Assert.Contains("SetSize:100:200", host.Calls);
        Assert.Contains("SetTitle:Test", host.Calls);
        Assert.Contains("SetLayout", host.Calls);
        Assert.Contains("Present:False", host.Calls);
        Assert.True(host.IsTracked);
    }

    [AvaloniaFact]
    public void Exit_Saves_And_Closes_Host()
    {
        var window = new DockWindow
        {
            Host = new FakeHostWindow(),
            Layout = new RootDock()
        };
        var host = (FakeHostWindow)window.Host!;
        host.SetPosition(5,6);
        host.SetSize(50,60);
        host.IsTracked = true;

        window.Exit();

        Assert.Contains("GetPosition", host.Calls);
        Assert.Contains("GetSize", host.Calls);
        Assert.Contains("Exit", host.Calls);
        Assert.False(host.IsTracked);
        Assert.Null(window.Host);
    }
}
