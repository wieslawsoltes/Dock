using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Recycling.Model;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace Dock.Controls.Recycling.UnitTests.Controls;

public class ControlRecyclingTests
{
    private class IdData(string id) : AvaloniaObject, IControlRecyclingIdProvider
    {
        public string? Id { get; } = id;
        public string? GetControlRecyclingId() => Id;
    }

    [AvaloniaFact]
    public void Add_And_TryGetValue_Work()
    {
        var recycling = new ControlRecycling();
        var data = new object();
        var control = new TextBlock();

        recycling.Add(data, control);

        var found = recycling.TryGetValue(data, out var cached);
        Assert.True(found);
        Assert.Same(control, cached);
    }

    [AvaloniaFact]
    public void TryGetValue_With_Null_Returns_False()
    {
        var recycling = new ControlRecycling();
        var result = recycling.TryGetValue(null, out var cached);
        Assert.False(result);
        Assert.Null(cached);
    }

    [AvaloniaFact]
    public void Clear_Removes_Items()
    {
        var recycling = new ControlRecycling();
        var data = new object();
        recycling.Add(data, new TextBlock());
        recycling.Clear();
        Assert.False(recycling.TryGetValue(data, out _));
    }

    [AvaloniaFact]
    public void Build_Returns_Null_For_Null_Data()
    {
        var recycling = new ControlRecycling();
        Assert.Null(recycling.Build(null, null, null));
    }

    [AvaloniaFact]
    public void Build_Uses_DataTemplate_And_Caches_Result()
    {
        var recycling = new ControlRecycling();
        var parent = new Control();
        parent.DataTemplates.Add(new FuncDataTemplate<int>((_, _) => new Border { Background = Brushes.Red }, true));

        var result1 = recycling.Build(10, null, parent) as Control;
        var result2 = recycling.Build(10, null, parent) as Control;

        Assert.NotNull(result1);
        Assert.Same(result1, result2);
    }

    [AvaloniaFact]
    public void Build_Uses_Id_When_Enabled()
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = true };
        var parent = new Control();
        parent.DataTemplates.Add(new FuncDataTemplate<IdData>((_, _) => new Border(), true));

        var data1 = new IdData("a");
        var data2 = new IdData("a");

        var result1 = recycling.Build(data1, null, parent);
        var result2 = recycling.Build(data2, null, parent);

        Assert.Same(result1, result2);
    }

    [AvaloniaFact]
    public void Build_Ignores_Id_When_Disabled()
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = false };
        var parent = new Control();
        parent.DataTemplates.Add(new FuncDataTemplate<IdData>((_, _) => new Border(), true));

        var data1 = new IdData("a");
        var data2 = new IdData("a");

        var result1 = recycling.Build(data1, null, parent);
        var result2 = recycling.Build(data2, null, parent);

        Assert.NotSame(result1, result2);
    }
}
