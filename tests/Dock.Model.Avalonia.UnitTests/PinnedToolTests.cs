using System.Linq;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Avalonia.Json;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class PinnedToolTests
{
    [AvaloniaFact]
    public void PinnedTool_RestoredLayout_CanBeOpened()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();

        var toolDock = factory.CreateToolDock();
        toolDock.Alignment = Alignment.Left;
        var tool = new Tool { Id = "tool" };
        toolDock.VisibleDockables = factory.CreateList<IDockable>(tool);
        root.VisibleDockables = factory.CreateList<IDockable>(toolDock);

        factory.InitLayout(root);
        factory.PinDockable(tool);

        var serializer = new AvaloniaDockSerializer();
        var json = serializer.Serialize(root);
        var loaded = serializer.Deserialize<RootDock>(json)!;
        factory.InitLayout(loaded);
        var state = new DockState();
        state.Restore(loaded);

        var loadedTool = loaded.LeftPinnedDockables!.OfType<Tool>().First();
        factory.PreviewPinnedDockable(loadedTool);

        Assert.NotNull(loaded.PinnedDock);
        Assert.Contains(loadedTool, loaded.PinnedDock!.VisibleDockables!);
    }
}
