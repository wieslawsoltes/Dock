using System.Text.Json;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Json;
using Dock.Model.Controls;
using Dock.Model.Core;
using System.Linq;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class AvaloniaDockSerializerTests
{
    [AvaloniaFact]
    public void Serialize_RootDock_IncludesPinnedDockDisplayMode()
    {
        var serializer = new AvaloniaDockSerializer();
        var root = new RootDock
        {
            PinnedDockDisplayMode = PinnedDockDisplayMode.Inline
        };

        var json = serializer.Serialize(root);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("PinnedDockDisplayMode", out var value));
        Assert.Equal((int)PinnedDockDisplayMode.Inline, value.GetInt32());
    }

    [AvaloniaFact]
    public void Serialize_Tool_IncludesPinnedBounds()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool();
        tool.SetPinnedBounds(1, 2, 300, 400);
        Assert.NotNull(tool.PinnedBounds);

        var json = serializer.Serialize(tool);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("PinnedBounds", out var bounds), json);
        Assert.Equal(1, bounds.GetProperty("X").GetDouble());
        Assert.Equal(2, bounds.GetProperty("Y").GetDouble());
        Assert.Equal(300, bounds.GetProperty("Width").GetDouble());
        Assert.Equal(400, bounds.GetProperty("Height").GetDouble());
    }

    [AvaloniaFact]
    public void Serialize_Tool_IncludesDockingState()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool
        {
            DockingState = DockingWindowState.Document | DockingWindowState.Floating
        };

        var json = serializer.Serialize(tool);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("DockingState", out var state), json);
        Assert.Equal((int)(DockingWindowState.Document | DockingWindowState.Floating), state.GetInt32());
    }

    [AvaloniaFact]
    public void Deserialize_Tool_RestoresDockingState()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool
        {
            DockingState = DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden
        };

        var json = serializer.Serialize(tool);
        var restored = serializer.Deserialize<Tool>(json);

        Assert.NotNull(restored);
        Assert.Equal(tool.DockingState, restored!.DockingState);
    }

    [AvaloniaFact]
    public void Serialize_RootDock_IncludesPinnedDock()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool { Id = "PinnedTool" };
        var pinnedDock = new ToolDock
        {
            VisibleDockables = new AvaloniaList<IDockable> { tool },
            ActiveDockable = tool
        };
        var root = new RootDock
        {
            PinnedDock = pinnedDock
        };

        var json = serializer.Serialize(root);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("PinnedDock", out var pinnedDockElement), json);
        Assert.True(pinnedDockElement.TryGetProperty("VisibleDockables", out _), json);
    }

    [AvaloniaFact]
    public void Deserialize_RootDock_RestoresPinnedDock()
    {
        var serializer = new AvaloniaDockSerializer();
        var factory = new Factory();

        var tool = new Tool { Id = "PinnedTool" };
        tool.SetPinnedBounds(0, 0, 240, 180);

        var pinnedDock = new ToolDock
        {
            VisibleDockables = new AvaloniaList<IDockable> { tool },
            ActiveDockable = tool
        };

        var root = new RootDock
        {
            LeftPinnedDockables = new AvaloniaList<IDockable> { tool },
            PinnedDock = pinnedDock
        };

        var json = serializer.Serialize(root);
        var restored = serializer.Deserialize<IRootDock>(json);

        Assert.NotNull(restored);

        factory.InitLayout((IDockable)restored!);

        Assert.NotNull(restored.PinnedDock);
        Assert.NotNull(restored.PinnedDock!.VisibleDockables);
        Assert.Single(restored.PinnedDock.VisibleDockables!);
        Assert.NotNull(restored.LeftPinnedDockables);
        Assert.Single(restored.LeftPinnedDockables!);

        var pinnedTool = restored.PinnedDock.VisibleDockables!.OfType<ITool>().Single();

        Assert.Same(restored.LeftPinnedDockables![0], pinnedTool);
        Assert.Same(pinnedTool, restored.PinnedDock.ActiveDockable);

        pinnedTool.GetPinnedBounds(out _, out _, out var width, out var height);
        Assert.Equal(240, width, 3);
        Assert.Equal(180, height, 3);
    }

    [AvaloniaFact]
    public void Serialize_DockWindow_IncludesWindowState()
    {
        var serializer = new AvaloniaDockSerializer();
        var window = new DockWindow
        {
            Id = "Window",
            Title = "Window",
            WindowState = DockWindowState.Maximized
        };

        var json = serializer.Serialize(window);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("WindowState", out var value), json);
        Assert.Equal((int)DockWindowState.Maximized, value.GetInt32());
    }
}
