using System.Text.Json;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Json;
using Dock.Model.Core;
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
}
