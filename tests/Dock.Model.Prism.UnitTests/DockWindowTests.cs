using Dock.Model.Controls;
using Dock.Model.Prism.Controls;
using Dock.Model.Prism.Core;
using Xunit;

namespace Dock.Model.Prism.UnitTests;

public class DockWindowTests
{
    private class FakeHostWindow : IHostWindow
    {
        public IDockManager? DockManager => null;
        public IHostWindowState? HostWindowState => null;
        public bool IsTracked { get; set; }
        public IDockWindow? Window { get; set; }

        public bool PresentCalled; public bool ExitCalled;
        public (double X,double Y)? SetPositionValue;
        public (double W,double H)? SetSizeValue;
        public string? Title;
        public IDock? Layout;
        public double GetX=1, GetY=2, GetW=3, GetH=4;

        public void Present(bool isDialog) { PresentCalled = true; }
        public void Exit() { ExitCalled = true; }
        public void SetPosition(double x, double y) { SetPositionValue=(x,y); }
        public void GetPosition(out double x, out double y) { x=GetX; y=GetY; }
        public void SetSize(double width, double height) { SetSizeValue=(width,height); }
        public void GetSize(out double width, out double height) { width=GetW; height=GetH; }
        public void SetTitle(string title) { Title = title; }
        public void SetLayout(IDock layout) { Layout = layout; }
    }

    [Fact]
    public void Save_Stores_Host_Bounds()
    {
        var host = new FakeHostWindow { GetX = 10, GetY = 20, GetW = 30, GetH = 40 };
        var window = new DockWindow { Host = host };

        window.Save();

        Assert.Equal(10, window.X);
        Assert.Equal(20, window.Y);
        Assert.Equal(30, window.Width);
        Assert.Equal(40, window.Height);
    }

    [Fact]
    public void Present_Sends_State_To_Host()
    {
        var host = new FakeHostWindow();
        var root = new RootDock();
        var window = new DockWindow
        {
            Host = host,
            Layout = root,
            X = 5,
            Y = 6,
            Width = 7,
            Height = 8,
            Title = "test"
        };

        window.Present(false);

        Assert.True(host.PresentCalled);
        Assert.True(host.IsTracked);
        Assert.Equal((5d,6d), host.SetPositionValue);
        Assert.Equal((7d,8d), host.SetSizeValue);
        Assert.Equal("test", host.Title);
        Assert.Same(root, host.Layout);
    }

    [Fact]
    public void Exit_Calls_Host_Exit_And_Untracks()
    {
        var host = new FakeHostWindow();
        var window = new DockWindow { Host = host };

        window.Exit();

        Assert.True(host.ExitCalled);
        Assert.Null(window.Host);
        Assert.False(host.IsTracked);
    }
}
