using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Avalonia.Themes.Fluent;
using Dock.Controls.DeferredContentControl;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class LastDockableRemovalTests
{
    [AvaloniaFact]
    public void CloseDockable_Removing_Last_Item_From_Plain_Child_List_Does_Not_Throw()
    {
        var factory = new Factory();
        var leftDocument = new Document { Id = "left-document", Title = "Left" };
        var middleDocument = new Document { Id = "middle-document", Title = "Middle" };
        var rightDocument = new Document { Id = "right-document", Title = "Right" };
        var leftDock = CreateDocumentDock("left", leftDocument);
        var middleDock = CreateDocumentDock("middle", middleDocument);
        var rightDock = CreateDocumentDock("right", rightDocument);
        var mainDock = new ProportionalDock
        {
            Id = "main",
            VisibleDockables = factory.CreateList<IDockable>(
                leftDock,
                new ProportionalDockSplitter(),
                middleDock,
                new ProportionalDockSplitter(),
                rightDock),
            ActiveDockable = middleDock
        };
        var rootDock = new RootDock
        {
            Id = "root",
            VisibleDockables = factory.CreateList<IDockable>(mainDock),
            ActiveDockable = mainDock
        };
        var originalMiddleDockables = middleDock.VisibleDockables;
        var originalMainDockables = mainDock.VisibleDockables;
        factory.InitLayout(rootDock);

        Assert.NotSame(originalMiddleDockables, middleDock.VisibleDockables);
        Assert.IsAssignableFrom<INotifyCollectionChanged>(middleDock.VisibleDockables);
        Assert.Same(originalMainDockables, mainDock.VisibleDockables);

        var dockControl = new DockControl { Factory = factory, Layout = rootDock };
        var window = new Window { Width = 800, Height = 600, Content = dockControl };
        window.Styles.Add(new DockFluentTheme { CacheDocumentTabContent = true });
        window.Show();

        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            DeferredContentPresentationQueue.FlushPendingBatchForTesting();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Equal(3, dockControl.GetVisualDescendants().OfType<DocumentTabStrip>().Count());

            factory.CloseDockable(middleDocument);

            Assert.DoesNotContain(middleDock, mainDock.VisibleDockables!);
        }
        finally
        {
            window.Close();
        }
    }

    private static DocumentDock CreateDocumentDock(string id, Document document)
    {
        return new DocumentDock
        {
            Id = id,
            VisibleDockables = new List<IDockable> { document },
            ActiveDockable = document
        };
    }
}
