using Dock.Model.Adapters;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class NavigateAdapterTests
{
    [Fact]
    public void Navigate_Back_Forward_Works()
    {
        var factory = new Factory();
        var doc1 = new Document { Id = "1" };
        var doc2 = new Document { Id = "2" };
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>(doc1, doc2), ActiveDockable = doc1, IsActive = true };
        var control = new TestDockControl { Layout = root, Factory = factory };
        factory.DockControls.Add(control);

        var adapter = new NavigateAdapter(root);
        adapter.Navigate(doc2, true);

        Assert.True(adapter.CanGoBack);
        Assert.Same(doc2, root.ActiveDockable);

        adapter.GoBack();
        Assert.Same(doc1, root.ActiveDockable);
        Assert.True(adapter.CanGoForward);

        adapter.GoForward();
        Assert.Same(doc2, root.ActiveDockable);
    }
}
