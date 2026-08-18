using System.Collections.Specialized;
using System.Linq;
using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using DockSimplifiedSample;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockSimplifiedSampleLayoutTests
{
    [AvaloniaFact]
    public void Tool_CanFloatAndRedock_WithObservableLayoutUpdates()
    {
        var factory = new DockFactory();
        var root = factory.CreateLayout();
        factory.InitLayout(root);

        var mainLayout = Assert.IsAssignableFrom<IProportionalDock>(
            Assert.Single(root.VisibleDockables!));
        var documentDock = Assert.Single(mainLayout.VisibleDockables!.OfType<IDocumentDock>());
        var tool = Assert.IsAssignableFrom<ITool>(
            Assert.Single(factory.Find(root, dockable => dockable.Id == "Tool1")));
        var layoutChanges = 0;
        var observableLayout = Assert.IsAssignableFrom<INotifyCollectionChanged>(mainLayout.VisibleDockables);
        observableLayout.CollectionChanged += (_, _) => layoutChanges++;

        factory.FloatDockable(tool);

        Assert.NotEqual(0, layoutChanges);
        Assert.Single(root.Windows!);
        Assert.NotSame(root, factory.FindRoot(tool, _ => true));

        var dockManager = new DockManager(new DockService());
        var redocked = dockManager.ValidateTool(
            tool,
            documentDock,
            DragAction.Move,
            DockOperation.Right,
            bExecute: true);

        Assert.True(redocked);
        Assert.Same(root, factory.FindRoot(tool, _ => true));
        Assert.Empty(root.Windows!);
        Assert.Same(tool, Assert.Single(factory.Find(root, dockable => dockable.Id == "Tool1")));
        Assert.Single(factory.Find(root, dockable => dockable.Id == "Document1"));
    }
}
