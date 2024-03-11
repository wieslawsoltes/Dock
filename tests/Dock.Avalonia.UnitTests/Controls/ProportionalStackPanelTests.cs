using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class ProportionalStackPanelTests
{
    [Fact]
    public void ProportionalStackPanel_Ctor()
    {
        var actual = new ProportionalStackPanel();
        Assert.NotNull(actual);
    }

    [Fact]
    public void Lays_Out_Children_Horizontal()
    {
        var target = new ProportionalStackPanel()
        {
            Width = 300,
            Height = 100,
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Border(),
                new ProportionalStackPanelSplitter(),
                new Border()
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(new Size(300, 100), target.Bounds.Size);
        Assert.Equal(new Rect(0, 0, 148, 100), target.Children[0].Bounds);
        Assert.Equal(new Rect(148, 0, 4, 100), target.Children[1].Bounds);
        Assert.Equal(new Rect(152, 0, 148, 100), target.Children[2].Bounds);
    }

    [Fact]
    public void Lays_Out_Children_Vertical()
    {
        var target = new ProportionalStackPanel()
        {
            Width = 100,
            Height = 300,
            Orientation = Orientation.Vertical,
            Children =
            {
                new Border(),
                new ProportionalStackPanelSplitter(),
                new Border()
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(new Size(100, 300), target.Bounds.Size);
        Assert.Equal(new Rect(0, 0, 100, 148), target.Children[0].Bounds);
        Assert.Equal(new Rect(0, 148, 100, 4), target.Children[1].Bounds);
        Assert.Equal(new Rect(0, 152, 100, 148), target.Children[2].Bounds);
    }

    [Fact]
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
                            Background = Brushes.Red,
                            [ProportionalStackPanel.ProportionProperty] = 0.5
                        },
                        new ProportionalStackPanelSplitter(),
                        new Border()
                        {
                            Background = Brushes.Green
                        },
                        new ProportionalStackPanelSplitter(),
                        new Border()
                        {
                            Background = Brushes.Blue
                        }
                    }
                },
                new ProportionalStackPanelSplitter(),
                new ProportionalStackPanel()
                {
                    Children =
                    {
                        new Border()
                        {
                            Background = Brushes.Blue,
                        },
                        new ProportionalStackPanelSplitter(),
                        new Border()
                        {
                            Background = Brushes.Red
                        },
                        new ProportionalStackPanelSplitter(),
                        new Border()
                        {
                            Background=Brushes.Green
                        }
                    }
                },
                new ProportionalStackPanelSplitter(),
                new ProportionalStackPanel()
                {
                    Children =
                    {
                        new Border()
                        {
                            Background = Brushes.Green,
                        },
                        new ProportionalStackPanelSplitter(),
                        new Border()
                        {
                            Background = Brushes.Blue
                        },
                        new ProportionalStackPanelSplitter(),
                        new Border()
                        {
                            Background=Brushes.Red,
                            [ProportionalStackPanel.ProportionProperty] = 0.5
                        }
                    }
                },
            }
        };

        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(new Size(1000, 500), target.Bounds.Size);
        Assert.Equal(new Rect(0, 0, 331, 500), target.Children[0].Bounds);
        Assert.Equal(new Rect(331, 0, 4, 500), target.Children[1].Bounds);
        Assert.Equal(new Rect(335, 0, 331, 500), target.Children[2].Bounds);
        Assert.Equal(new Rect(665, 0, 4, 500), target.Children[3].Bounds);
        Assert.Equal(new Rect(669, 0, 331, 500), target.Children[4].Bounds);
    }

    [Fact]
    public void Lays_Out_Children_ItemsControl()
    {
        var target1 = new ItemsControl()
        {
            Width = 1000,
            Height = 500,
            ItemsPanel = new ItemsPanelTemplate()
            {
                Content = new ProportionalStackPanel()
                {
                    Orientation = Orientation.Horizontal
                }
            },
            ItemsSource = new List<Control>()
            {
                new Border()
                {
                    Background = Brushes.Green
                },
                new ProportionalStackPanelSplitter(),
                new Border()
                {
                    Background = Brushes.Blue
                },
                new ProportionalStackPanelSplitter(),
                new ItemsControl()
                {
                    ItemsPanel = new ItemsPanelTemplate()
                    {
                        Content = new ProportionalStackPanel()
                        {
                            Orientation = Orientation.Vertical,
                        }
                    },
                    ItemsSource = new List<Control>()
                    {
                        new Border()
                        {
                            Background = Brushes.Green
                        },
                        new ProportionalStackPanelSplitter(),
                        new Border()
                        {
                            Background = Brushes.Blue
                        },
                        new Border()
                        {
                            Background = Brushes.Red
                        }
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
}
