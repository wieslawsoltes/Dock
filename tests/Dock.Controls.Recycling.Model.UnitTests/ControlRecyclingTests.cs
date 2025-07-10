using Xunit;

namespace Dock.Controls.Recycling.Model.UnitTests;

public class ControlRecyclingTests
{
    [Fact]
    public void TryToUseIdAsKey_Property_Works()
    {
        var recycling = new DummyControlRecycling();
        recycling.TryToUseIdAsKey = true;
        Assert.True(recycling.TryToUseIdAsKey);
    }

    [Fact]
    public void Add_And_TryGetValue_ReturnsAddedControl()
    {
        var recycling = new DummyControlRecycling();
        var data = new object();
        var control = new object();
        recycling.Add(data, control);
        var found = recycling.TryGetValue(data, out var retrieved);
        Assert.True(found);
        Assert.Same(control, retrieved);
    }

    [Fact]
    public void Build_WithExisting_ReturnsExisting()
    {
        var recycling = new DummyControlRecycling();
        var data = new object();
        var existing = new object();
        var result = recycling.Build(data, existing, null);
        Assert.Same(existing, result);
        Assert.True(recycling.TryGetValue(data, out var retrieved));
        Assert.Same(existing, retrieved);
    }

    [Fact]
    public void Build_WithNullExisting_CreatesNewControl()
    {
        var recycling = new DummyControlRecycling();
        var data = new object();
        var result = recycling.Build(data, null, null);
        Assert.NotNull(result);
        Assert.True(recycling.TryGetValue(data, out var retrieved));
        Assert.Same(result, retrieved);
    }

    [Fact]
    public void Clear_RemovesAllControls()
    {
        var recycling = new DummyControlRecycling();
        var data = new object();
        recycling.Add(data, new object());
        recycling.Clear();
        Assert.False(recycling.TryGetValue(data, out _));
    }
}
