using System.Linq;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Json;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Serialization;

public class PinnedToolSerializationTests
{
    [AvaloniaFact]
    public void SaveLoad_WithPinnedTool_PreservesState()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock
        {
            Id = "toolDock",
            Alignment = Alignment.Left,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(root, toolDock);

        var tool = new Tool { Id = "tool" };
        factory.AddDockable(toolDock, tool);

        factory.PreviewPinnedDockable(tool);
        factory.PinDockable(tool);

        Assert.Single(root.LeftPinnedDockables!);
        Assert.False(toolDock.IsExpanded);
        Assert.True(toolDock.AutoHide);

        var serializer = new AvaloniaDockSerializer();
        var json = serializer.Serialize(root);
        var loaded = serializer.Deserialize<RootDock>(json);

        Assert.NotNull(loaded);
        Assert.Single(loaded!.LeftPinnedDockables!);
        var loadedToolDock = loaded.VisibleDockables!.OfType<IToolDock>().First();
        Assert.False(loadedToolDock.IsExpanded);
        Assert.True(loadedToolDock.AutoHide);
    }
}

