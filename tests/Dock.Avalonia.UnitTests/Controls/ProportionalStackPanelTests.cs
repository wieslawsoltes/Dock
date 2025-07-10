using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Dock.Controls.ProportionalStackPanel;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class ProportionalStackPanelTests
{
    [AvaloniaFact]
    public void ProportionalStackPanel_Ctor()
    {
        var actual = new ProportionalStackPanel();
        Assert.NotNull(actual);
    }

    [AvaloniaFact]
    public void Lays_Out_Children_Horizontal()
    {
        var target = new ProportionalStackPanel()
        {
            Width = 300,
            Height = 100,
            Orientation = Orientation.Horizontal,
            Children = { new Border(), new ProportionalStackPanelSplitter(), new Border() }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(new Size(300, 100), target.Bounds.Size);
        Assert.Equal(new Rect(0, 0, 148, 100), target.Children[0].Bounds);
        Assert.Equal(new Rect(148, 0, 4, 100), target.Children[1].Bounds);
        Assert.Equal(new Rect(152, 0, 148, 100), target.Children[2].Bounds);
    }

    [AvaloniaFact]
    public void Lays_Out_Children_Vertical()
    {
        var target = new ProportionalStackPanel()
        {
            Width = 100,
            Height = 300,
            Orientation = Orientation.Vertical,
            Children = { new Border(), new ProportionalStackPanelSplitter(), new Border() }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(new Size(100, 300), target.Bounds.Size);
        Assert.Equal(new Rect(0, 0, 100, 148), target.Children[0].Bounds);
        Assert.Equal(new Rect(0, 148, 100, 4), target.Children[1].Bounds);
        Assert.Equal(new Rect(0, 152, 100, 148), target.Children[2].Bounds);
    }

    public static IEnumerable<object[]> GetBorderTestsData()
    {
        yield return [0.5, 604, 300, 300];
        yield return [0.25, 604, 150, 450];
        yield return [0.6283185307179586476925286766559, 604, 377, 223];
        yield return [0.3141592653589793238462643383279, 604, 188, 412];
    }

    [AvaloniaTheory]
    [MemberData(nameof(GetBorderTestsData))]
    public void Should_Not_Trim_Borders_Horizontal(
        double proportion,
        double expectedWidth,
        double expectedFirstChildHeight,
        double expectedSecondChildHeight)
    {
        var target = new ProportionalStackPanel()
        {
            Width = expectedWidth,
            Height = 100,
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Border { [ProportionalStackPanel.ProportionProperty] = proportion },
                new ProportionalStackPanelSplitter(),
                new Border { [ProportionalStackPanel.ProportionProperty] = 1 - proportion }
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        var width = target.Children.Sum(c => c.Bounds.Width);

        Assert.Equal(expectedFirstChildHeight, target.Children[0].Bounds.Width);
        Assert.Equal(expectedSecondChildHeight, target.Children[2].Bounds.Width);
        Assert.Equal(expectedWidth, width);
    }

    [AvaloniaTheory]
    [MemberData(nameof(GetBorderTestsData))]
    public void Should_Not_Trim_Borders_Vertical(
        double proportion,
        double expectedHeight,
        double expectedFirstChildHeight,
        double expectedSecondChildHeight)
    {
        var target = new ProportionalStackPanel()
        {
            Width = 100,
            Height = expectedHeight,
            Orientation = Orientation.Vertical,
            Children =
            {
                new Border { [ProportionalStackPanel.ProportionProperty] = proportion },
                new ProportionalStackPanelSplitter(),
                new Border { [ProportionalStackPanel.ProportionProperty] = 1 - proportion }
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        var height = target.Children.Sum(c => c.Bounds.Height);

        Assert.Equal(expectedFirstChildHeight, target.Children[0].Bounds.Height);
        Assert.Equal(expectedSecondChildHeight, target.Children[2].Bounds.Height);
        Assert.Equal(expectedHeight, height);
    }

    [AvaloniaFact]
    public void Lays_Out_Children_Default()
    {
        var target = new ProportionalStackPanel()
        {
            Width = 1000,
            Height = 500,
            Orientation = Orientation.Horizontal,
            Children =
            {
                new ProportionalStackPanel()
                {
                    Children =
                    {
                        new Border()
                        {
                            Background = Brushes.Red, [ProportionalStackPanel.ProportionProperty] = 0.5
                        },
                        new ProportionalStackPanelSplitter(),
                        new Border() { Background = Brushes.Green },
                        new ProportionalStackPanelSplitter(),
                        new Border() { Background = Brushes.Blue }
                    }
                },
                new ProportionalStackPanelSplitter(),
                new ProportionalStackPanel()
                {
                    Children =
                    {
                        new Border() { Background = Brushes.Blue, },
                        new ProportionalStackPanelSplitter(),
                        new Border() { Background = Brushes.Red },
                        new ProportionalStackPanelSplitter(),
                        new Border() { Background = Brushes.Green }
                    }
                },
                new ProportionalStackPanelSplitter(),
                new ProportionalStackPanel()
                {
                    Children =
                    {
                        new Border() { Background = Brushes.Green, },
                        new ProportionalStackPanelSplitter(),
                        new Border() { Background = Brushes.Blue },
                        new ProportionalStackPanelSplitter(),
                        new Border()
                        {
                            Background = Brushes.Red, [ProportionalStackPanel.ProportionProperty] = 0.5
                        }
                    }
                },
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        // values have to add up to width/height of the parent control
        Assert.Equal(new Size(1000, 500), target.Bounds.Size);
        Assert.Equal(new Rect(0, 0, 330, 500), target.Children[0].Bounds);
        Assert.Equal(new Rect(330, 0, 4, 500), target.Children[1].Bounds);
        Assert.Equal(new Rect(334, 0, 331, 500), target.Children[2].Bounds);
        Assert.Equal(new Rect(665, 0, 4, 500), target.Children[3].Bounds);
        Assert.Equal(new Rect(669, 0, 331, 500), target.Children[4].Bounds);
    }

    [AvaloniaFact]
    public void Lays_Out_Children_ItemsControl()
    {
        var target1 = new ItemsControl()
        {
            Width = 1000,
            Height = 500,
            ItemsPanel =
                new ItemsPanelTemplate()
                {
                    Content = new ProportionalStackPanel() { Orientation = Orientation.Horizontal }
                },
            ItemsSource = new List<Control>()
            {
                new Border() { Background = Brushes.Green },
                new ProportionalStackPanelSplitter(),
                new Border() { Background = Brushes.Blue },
                new ProportionalStackPanelSplitter(),
                new ItemsControl()
                {
                    ItemsPanel =
                        new ItemsPanelTemplate()
                        {
                            Content = new ProportionalStackPanel() { Orientation = Orientation.Vertical, }
                        },
                    ItemsSource = new List<Control>()
                    {
                        new Border() { Background = Brushes.Green },
                        new ProportionalStackPanelSplitter(),
                        new Border() { Background = Brushes.Blue },
                        new Border() { Background = Brushes.Red }
                    }
                }
            }
        };

        target1.ApplyTemplate();
        target1.Measure(Size.Infinity);
        target1.Arrange(new Rect(target1.DesiredSize));

        var items1 = target1.ItemsSource as List<Control>;
        var target2 = items1?[4] as ItemsControl;
        var items2 = target2?.ItemsSource as List<Control>;

        Assert.Equal(new Size(1000, 500), target1.Bounds.Size);
        Assert.Equal(new Rect(0, 0, 0, 0), items1?[0].Bounds);
        Assert.Equal(new Rect(0, 0, 0, 0), items1?[1].Bounds);
        Assert.Equal(new Rect(0, 0, 0, 0), items1?[2].Bounds);
        Assert.Equal(new Rect(0, 0, 0, 0), items1?[3].Bounds);
        Assert.Equal(new Rect(0, 0, 0, 0), items1?[4].Bounds);

        Assert.Equal(new Size(0, 0), target2?.Bounds.Size);
        Assert.Equal(new Rect(0, 0, 0, 0), items2?[0].Bounds);
        Assert.Equal(new Rect(0, 0, 0, 0), items2?[1].Bounds);
        Assert.Equal(new Rect(0, 0, 0, 0), items2?[2].Bounds);
        Assert.Equal(new Rect(0, 0, 0, 0), items2?[3].Bounds);
    }

    [AvaloniaFact]
    public void Respects_MinWidth_When_Assigning_Proportion()
    {
        var target = new ProportionalStackPanel
        {
            Width = 500,
            Height = 100,
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Border
                {
                    MinWidth = 150,
                    [ProportionalStackPanel.ProportionProperty] = 0.1
                },
                new ProportionalStackPanelSplitter(),
                new Border
                {
                    [ProportionalStackPanel.ProportionProperty] = 0.5
                }
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.True(target.Children[0].Bounds.Width >= 150);
    }

    [AvaloniaFact]
    public void Respects_MaxWidth_When_Assigning_Proportion()
    {
        var target = new ProportionalStackPanel
        {
            Width = 300,
            Height = 100,
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Border
                {
                    MaxWidth = 100,
                    [ProportionalStackPanel.ProportionProperty] = 0.9
                },
                new ProportionalStackPanelSplitter(),
                new Border
                {
                    [ProportionalStackPanel.ProportionProperty] = 0.7
                }
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.True(target.Children[0].Bounds.Width <= 100);
    }

    [AvaloniaFact]
    public void Respects_MinHeight_In_Vertical_Mode()
    {
        var target = new ProportionalStackPanel
        {
            Width = 100,
            Height = 500,
            Orientation = Orientation.Vertical,
            Children =
            {
                new Border
                {
                    MinHeight = 200,
                    [ProportionalStackPanel.ProportionProperty] = 0.1
                },
                new ProportionalStackPanelSplitter(),
                new Border
                {
                    [ProportionalStackPanel.ProportionProperty] = 0.5
                }
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.True(target.Children[0].Bounds.Height >= 200);
    }

    [AvaloniaFact]
    public void Collapsed_Child_Occupies_No_Space()
    {
        var first = new Border { [ProportionalStackPanel.ProportionProperty] = 0.5 };
        var splitter = new ProportionalStackPanelSplitter();
        var second = new Border { [ProportionalStackPanel.ProportionProperty] = 0.5 };
        var target = new ProportionalStackPanel
        {
            Width = 300,
            Height = 100,
            Orientation = Orientation.Horizontal,
            Children = { first, splitter, second }
        };

        ProportionalStackPanel.SetIsCollapsed(first, true);
        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(0, first.Bounds.Width);
        Assert.Equal(296, second.Bounds.Width);
        Assert.Equal(0, ProportionalStackPanel.GetProportion(first));
        Assert.Equal(0.5, ProportionalStackPanel.GetCollapsedProportion(first));

        ProportionalStackPanel.SetIsCollapsed(first, false);
        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(148, first.Bounds.Width);
        Assert.Equal(148, second.Bounds.Width);
    }

    [AvaloniaFact]
    public void MeasureOverride_Throws_On_Infinite_Size()
    {
        var target = new ProportionalStackPanel { Orientation = Orientation.Horizontal };
        Assert.Throws<Exception>(() => target.Measure(Size.Infinity));
    }
}
