using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class FactoryPinnedDockInitTests
{
    [Fact]
    public void InitLayout_InitializesPinnedDock()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();

        var tool = new Tool { Id = "PinnedTool" };
        root.LeftPinnedDockables = factory.CreateList<IDockable>(tool);

        root.PinnedDock = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>(tool),
            ActiveDockable = tool,
            IsEmpty = true
        };

        factory.InitLayout(root);

        Assert.NotNull(root.PinnedDock);
        Assert.Same(factory, root.PinnedDock!.Factory);
        Assert.False(root.PinnedDock.IsEmpty);
        Assert.Same(root, root.PinnedDock.Owner);
        Assert.Same(tool, root.PinnedDock.ActiveDockable);
    }
}
