using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Dock.Controls.ProportionalStackPanel;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class ProportionalStackPanelSplitterTests
{
    [AvaloniaFact]
    public void Splitter_Ctor()
    {
        var splitter = new ProportionalStackPanelSplitter();
        Assert.NotNull(splitter);
    }

    [AvaloniaFact]
    public void IsSplitter_Returns_True_For_ContentPresenter()
    {
        var splitter = new ProportionalStackPanelSplitter();
        var presenter = new ContentPresenter { Content = splitter };

        var result = ProportionalStackPanelSplitter.IsSplitter(presenter, out var actual);

        Assert.True(result);
        Assert.Equal(splitter, actual);
    }

    [AvaloniaFact]
    public void MeasureOverride_Uses_Panel_Orientation()
    {
        var panel = new ProportionalStackPanel { Orientation = Orientation.Vertical };
        var before = new Border();
        var splitter = new ProportionalStackPanelSplitter { Thickness = 8 };
        panel.Children.Add(before);
        panel.Children.Add(splitter);

        panel.Measure(Size.Infinity);

        Assert.Equal(new Size(0, 8), splitter.DesiredSize);
    }
}
