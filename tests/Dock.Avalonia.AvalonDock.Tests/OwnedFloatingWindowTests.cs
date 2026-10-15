using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class OwnedFloatingWindowTests
{
    [AvaloniaFact]
    public void Closing_The_Owner_Bypasses_Child_Close_Policies_And_Preserves_The_Layout()
    {
        LayoutDocument document = new() { Title = "Document", CanClose = false };
        LayoutAnchorable tool = new() { Title = "Tool", CanClose = false, CanHide = false };
        LayoutPanel panel = new(new LayoutDocumentPane(document));
        panel.Children.Add(new LayoutAnchorablePane(tool));
        DockingManager manager = new() { Layout = new() { RootPanel = panel } };
        Window owner = new() { Content = manager, Width = 800, Height = 500 };
        bool cancelOwner = true;
        owner.Closing += (_, e) => e.Cancel = cancelOwner;
        try
        {
            owner.Show();
            Dispatcher.UIThread.RunJobs();
            document.Float();
            tool.Float();
            Dispatcher.UIThread.RunJobs();
            LayoutFloatingWindowControl[] hosts = manager.FloatingWindows.ToArray();
            Assert.Equal(2, hosts.Length);
            int childClosing = 0;
            foreach (LayoutFloatingWindowControl host in hosts)
            {
                Assert.Same(owner, host.Owner);
                host.Closing += (_, e) => { childClosing++; e.Cancel = true; };
            }
            owner.Close();
            Assert.True(owner.IsVisible);
            Assert.All(hosts, host => Assert.True(host.IsVisible));
            Assert.Equal(0, childClosing);

            cancelOwner = false;
            owner.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.False(owner.IsVisible);
            Assert.All(hosts, host => Assert.False(host.IsVisible));
            Assert.Empty(manager.FloatingWindows);
            Assert.Equal(0, childClosing);
            Assert.Equal(2, manager.Layout.FloatingWindows.Count);
            Assert.Same(manager.Layout, document.Root);
            Assert.Same(manager.Layout, tool.Root);
        }
        finally { cancelOwner = false; owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
