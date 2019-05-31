// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class ProportionalStackPanelTests
    {
        [Fact]
        public void Lays_Out_Children_Horizontal()
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
                            new Rectangle()
                            {
                                Fill = Brushes.Red,
                                [ProportionalStackPanelSplitter.ProportionProperty] = 0.5
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill = Brushes.Green
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill=Brushes.Blue
                            }
                        }
                    },
                    new ProportionalStackPanelSplitter(),
                    new ProportionalStackPanel()
                    {
                        Children =
                        {
                            new Rectangle()
                            {
                                Fill = Brushes.Blue,
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill = Brushes.Red
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill=Brushes.Green
                            }
                        }
                    },
                    new ProportionalStackPanelSplitter(),
                    new ProportionalStackPanel()
                    {
                        Children =
                        {
                            new Rectangle()
                            {
                                Fill = Brushes.Green,
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill = Brushes.Blue
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill=Brushes.Red,
                                [ProportionalStackPanelSplitter.ProportionProperty] = 0.5
                            }
                        }
                    },
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            Assert.Equal(new Size(1000, 500), target.Bounds.Size);
            Assert.Equal(new Rect(0, 0, 331, 500), target.Children[0].Bounds);
            Assert.Equal(new Rect(330, 0, 0, 500), target.Children[1].Bounds);
            Assert.Equal(new Rect(330, 0, 331, 500), target.Children[2].Bounds);
            Assert.Equal(new Rect(661, 0, 0, 500), target.Children[3].Bounds);
            Assert.Equal(new Rect(661, 0, 331, 500), target.Children[4].Bounds);
        }

        [Fact]
        public void Lays_Out_Children_Vertical()
        {
            var target = new ProportionalStackPanel()
            {
                Width = 1000,
                Height = 500,
                Orientation = Orientation.Vertical,
                Children =
                {
                    new ProportionalStackPanel()
                    {
                        Children =
                        {
                            new Rectangle()
                            {
                                Fill = Brushes.Red,
                                [ProportionalStackPanelSplitter.ProportionProperty] = 0.5
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill = Brushes.Green
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill=Brushes.Blue
                            }
                        }
                    },
                    new ProportionalStackPanelSplitter(),
                    new ProportionalStackPanel()
                    {
                        Children =
                        {
                            new Rectangle()
                            {
                                Fill = Brushes.Blue,
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill = Brushes.Red
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill=Brushes.Green
                            }
                        }
                    },
                    new ProportionalStackPanelSplitter(),
                    new ProportionalStackPanel()
                    {
                        Children =
                        {
                            new Rectangle()
                            {
                                Fill = Brushes.Green,
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill = Brushes.Blue
                            },
                            new ProportionalStackPanelSplitter(),
                            new Rectangle()
                            {
                                Fill=Brushes.Red,
                                [ProportionalStackPanelSplitter.ProportionProperty] = 0.5
                            }
                        }
                    },
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            Assert.Equal(new Size(1000, 500), target.Bounds.Size);
            Assert.Equal(new Rect(0, 0, 1000, 164), target.Children[0].Bounds);
            Assert.Equal(new Rect(0, 164, 1000, 0), target.Children[1].Bounds);
            Assert.Equal(new Rect(0, 164, 1000, 164), target.Children[2].Bounds);
            Assert.Equal(new Rect(0, 328, 1000, 0), target.Children[3].Bounds);
            Assert.Equal(new Rect(0, 328, 1000, 164), target.Children[4].Bounds);
        }
    }
}
