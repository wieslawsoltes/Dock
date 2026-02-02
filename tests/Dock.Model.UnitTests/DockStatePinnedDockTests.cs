using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockStatePinnedDockTests
{
    [Fact]
    public void SaveRestore_IncludesPinnedDock()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();

        var tool = new Tool { Id = "PinnedTool", Content = "payload" };
        root.PinnedDock = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>(tool),
            ActiveDockable = tool
        };

        var state = new DockState();
        state.Save(root);

        tool.Content = null;
        state.Restore(root);

        Assert.Equal("payload", tool.Content);
    }
}
