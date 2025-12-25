using System.Collections.ObjectModel;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class MdiLayoutHelperTests
{
    [AvaloniaFact]
    public void CascadeDocuments_AssignsBounds()
    {
        var dock = new DocumentDock();
        dock.SetVisibleBounds(0, 0, 800, 600);

        var doc1 = new Document { Title = "Doc1", Owner = dock };
        var doc2 = new Document { Title = "Doc2", Owner = dock };

        dock.VisibleDockables = new ObservableCollection<IDockable> { doc1, doc2 };

        MdiLayoutHelper.CascadeDocuments(dock);

        Assert.True(doc1.MdiBounds.Width > 0);
        Assert.True(doc1.MdiBounds.Height > 0);
        Assert.Equal(MdiWindowState.Normal, doc1.MdiState);
    }

    [AvaloniaFact]
    public void RestoreDocuments_ResetsState()
    {
        var dock = new DocumentDock();
        var doc = new Document { Title = "Doc", Owner = dock, MdiState = MdiWindowState.Minimized };
        dock.VisibleDockables = new ObservableCollection<IDockable> { doc };

        MdiLayoutHelper.RestoreDocuments(dock);

        Assert.Equal(MdiWindowState.Normal, doc.MdiState);
    }
}
