using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Core;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests;

public class DockWindowTests
{
    private class TestHostWindow : IHostWindow
    {
        public IDockManager? DockManager { get; }
        public IHostWindowState? HostWindowState { get; }
        public bool IsTracked { get; set; }
        public IDockWindow? Window { get; set; }

        public bool Presented { get; private set; }
        public bool PresentedAsDialog { get; private set; }
        public bool Exited { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public string? Title { get; private set; }
        public IDock? Layout { get; private set; }

        public void Present(bool isDialog)
        {
            Presented = true;
            PresentedAsDialog = isDialog;
        }

        public void Exit()
        {
            Exited = true;
        }

        public void SetPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void GetPosition(out double x, out double y)
        {
            x = X; y = Y;
        }

        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public void GetSize(out double width, out double height)
        {
            width = Width; height = Height;
        }

        public void SetTitle(string? title)
        {
            Title = title;
        }

        public void SetLayout(IDock layout)
        {
            Layout = layout;
        }

        public void SetActive()
        {
            // Test implementation - could add a flag to track if called
        }
    }

    private class HostFactory : Factory
    {
        public TestHostWindow Host { get; } = new();
        public override IHostWindow? GetHostWindow(string id) => Host;
    }

    [Fact]
    public void DockWindow_Defaults_Are_Correct()
    {
        var window = new DockWindow();

        Assert.Equal(nameof(IDockWindow), window.Id);
        Assert.Equal(nameof(IDockWindow), window.Title);
        Assert.Equal(0, window.X);
        Assert.Equal(0, window.Y);
        Assert.Equal(0, window.Width);
        Assert.Equal(0, window.Height);
        Assert.False(window.Topmost);
        Assert.Equal(DockWindowOwnerMode.Default, window.OwnerMode);
        Assert.Null(window.ParentWindow);
        Assert.False(window.IsModal);
        Assert.Null(window.ShowInTaskbar);
    }

    [Fact]
    public void Save_Updates_Position_And_Size_From_Host()
    {
        var window = new DockWindow();
        var host = new TestHostWindow();
        window.Host = host;
        host.SetPosition(10, 20);
        host.SetSize(300, 200);

        window.Save();

        Assert.Equal(10, window.X);
        Assert.Equal(20, window.Y);
        Assert.Equal(300, window.Width);
        Assert.Equal(200, window.Height);
    }

    [Fact]
    public void Present_Creates_Host_And_Presents()
    {
        var factory = new HostFactory();
        var window = new DockWindow { Factory = factory, Layout = new RootDock() };

        window.Present(false);

        var host = factory.Host;
        Assert.True(host.Presented);
        Assert.False(host.PresentedAsDialog);
        Assert.True(host.IsTracked);
        Assert.Same(window, host.Window);
        Assert.Equal(window.Title, host.Title);
        Assert.Equal(window.Layout, host.Layout);
    }

    [Fact]
    public void Exit_Saves_And_Resets_Host()
    {
        var factory = new HostFactory();
        var window = new DockWindow { Factory = factory, Layout = new RootDock(), Host = factory.Host };
        factory.Host.SetPosition(5, 6);
        factory.Host.SetSize(100, 120);

        window.Exit();

        Assert.True(factory.Host.Exited);
        Assert.False(factory.Host.IsTracked);
        Assert.Null(window.Host);
        Assert.Equal(5, window.X);
        Assert.Equal(6, window.Y);
        Assert.Equal(100, window.Width);
        Assert.Equal(120, window.Height);
    }
}
