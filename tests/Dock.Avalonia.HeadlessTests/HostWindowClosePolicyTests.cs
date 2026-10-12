using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowClosePolicyTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Preserved_Layout_Retains_Its_Model_And_Can_Be_Presented_Again(bool cancelFirst)
    {
        PolicyFactory factory = new();
        RootDock owner = new() { Factory = factory, Windows = factory.CreateList<IDockWindow>() };
        Document document = new();
        RootDock layout = new() { Factory = factory, VisibleDockables = factory.CreateList<IDockable>(document) };
        DockWindow model = new() { Layout = layout };
        HostWindow host = new();
        layout.Window = model;
        document.Owner = layout;
        document.Factory = factory;
        factory.AddWindow(owner, model);
        factory.InitDockWindow(model, owner, host);
        int closed = 0;
        bool cancel = cancelFirst;
        factory.WindowClosing += (_, args) => args.Cancel = cancel;
        factory.WindowClosed += (_, _) => closed++;
        model.Present(false);
        try
        {
            host.Close();
            if (cancelFirst)
            {
                Assert.True(host.IsVisible);
                Assert.True(host.IsTracked);
                Assert.Same(host, model.Host);
                Assert.Equal(0, closed);
                cancel = false;
                host.Close();
            }
            Assert.True(((IHostWindowCloseState)host).IsClosed);
            Assert.False(host.IsTracked);
            Assert.Null(model.Host);
            Assert.Empty(factory.HostWindows);
            Assert.Same(model, Assert.Single(owner.Windows!));
            Assert.Same(layout, model.Layout);
            Assert.Same(model, layout.Window);
            Assert.Same(layout, document.Owner);
            Assert.Equal(0, factory.ContentCloseCalls);
            Assert.Equal(1, closed);
            factory.DefaultHostWindowLocator = () => new HostWindow();
            model.Present(false);
            Assert.NotSame(host, model.Host);
            Assert.True(model.Host!.IsTracked);
        }
        finally { cancel = false; model.Exit(); factory.RemoveWindow(model); }
    }

    [AvaloniaFact]
    public void A_Closed_Callback_Can_Install_A_Replacement_Host()
    {
        PolicyFactory factory = new();
        DockWindow model = new() { Factory = factory };
        HostWindow host = new() { Window = model, IsTracked = true };
        HostWindow replacement = new() { Window = model };
        model.Host = host;
        host.Closed += (_, _) => model.Host = replacement;
        host.Show();
        host.Close();
        Assert.Same(replacement, model.Host);
        Assert.DoesNotContain(host, factory.HostWindows);
        replacement.Close();
    }

    private sealed class PolicyFactory : Factory, IDockWindowClosePolicy
    {
        public int ContentCloseCalls { get; private set; }
        public bool PreserveLayoutOnHostClose(IDockWindow window) => true;
        public override void CloseWindow(IDockWindow window) { ContentCloseCalls++; base.CloseWindow(window); }
    }
}
