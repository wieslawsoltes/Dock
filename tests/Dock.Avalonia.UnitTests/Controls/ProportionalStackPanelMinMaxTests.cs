using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Dock.Controls.ProportionalStackPanel;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class ProportionalStackPanelMinMaxTests
    {
        [AvaloniaFact]
        public void Respects_MinWidth_During_Layout_With_Constraints()
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
                        MinWidth = 200,
                        MaxWidth = 300,
                        [ProportionalStackPanel.ProportionProperty] = 0.2 // Would be 100px without constraints
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 0.8
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            Assert.True(child1.Bounds.Width >= 200, $"Child width {child1.Bounds.Width} should be >= 200");
            Assert.True(child1.Bounds.Width <= 300, $"Child width {child1.Bounds.Width} should be <= 300");
        }

        [AvaloniaFact]
        public void Respects_MaxWidth_During_Layout_With_Constraints()
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
                        MinWidth = 100,
                        MaxWidth = 150,
                        [ProportionalStackPanel.ProportionProperty] = 0.8 // Would be 400px without constraints
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 0.2
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            Assert.True(child1.Bounds.Width >= 100, $"Child width {child1.Bounds.Width} should be >= 100");
            Assert.True(child1.Bounds.Width <= 150, $"Child width {child1.Bounds.Width} should be <= 150");
        }

        [AvaloniaFact]
        public void Respects_MinHeight_During_Layout_With_Constraints()
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
                        MaxHeight = 300,
                        [ProportionalStackPanel.ProportionProperty] = 0.2 // Would be 100px without constraints
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 0.8
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            Assert.True(child1.Bounds.Height >= 200, $"Child height {child1.Bounds.Height} should be >= 200");
            Assert.True(child1.Bounds.Height <= 300, $"Child height {child1.Bounds.Height} should be <= 300");
        }

        [AvaloniaFact]
        public void Maintains_Constraint_Stability_When_Proportions_Change()
        {
            var target = new ProportionalStackPanel
            {
                Width = 800,
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MinWidth = 150,
                        MaxWidth = 400,
                        [ProportionalStackPanel.ProportionProperty] = 0.4
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        MinWidth = 100,
                        [ProportionalStackPanel.ProportionProperty] = 0.6
                    }
                }
            };

            // Initial layout
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            var child2 = target.Children[2];

            // Verify initial constraints are respected
            Assert.True(child1.Bounds.Width >= 150);
            Assert.True(child1.Bounds.Width <= 400);
            Assert.True(child2.Bounds.Width >= 100);

            // Simulate extreme proportion changes that would violate constraints
            ProportionalStackPanel.SetProportion(child1, 0.1); // Try to make very small
            ProportionalStackPanel.SetProportion(child2, 0.9);

            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            // Should still respect constraints
            Assert.True(child1.Bounds.Width >= 150, $"After resize, child1 width {child1.Bounds.Width} should be >= 150");
            Assert.True(child1.Bounds.Width <= 400, $"After resize, child1 width {child1.Bounds.Width} should be <= 400");
            Assert.True(child2.Bounds.Width >= 100, $"After resize, child2 width {child2.Bounds.Width} should be >= 100");

            // Try opposite extreme
            ProportionalStackPanel.SetProportion(child1, 0.9); // Try to make very large
            ProportionalStackPanel.SetProportion(child2, 0.1);

            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            // Should still respect constraints
            Assert.True(child1.Bounds.Width >= 150, $"After second resize, child1 width {child1.Bounds.Width} should be >= 150");
            Assert.True(child1.Bounds.Width <= 400, $"After second resize, child1 width {child1.Bounds.Width} should be <= 400");
            Assert.True(child2.Bounds.Width >= 100, $"After second resize, child2 width {child2.Bounds.Width} should be >= 100");
        }

        [AvaloniaFact]
        public void Respects_MinWidth_With_Different_Panel_Sizes()
        {
            var target = new ProportionalStackPanel
            {
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MinWidth = 200,
                        MaxWidth = 300,
                        [ProportionalStackPanel.ProportionProperty] = 0.3
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 0.7
                    }
                }
            };

            // Test Case 1: Panel width = 500px - child1 should be 150px (30% of 496), but min is 200
            target.Width = 500;
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            Assert.True(child1.Bounds.Width >= 200, $"At 500px width, child width {child1.Bounds.Width} should be >= 200");
            Assert.True(child1.Bounds.Width <= 300, $"At 500px width, child width {child1.Bounds.Width} should be <= 300");

            // Test Case 2: Panel width = 1000px - child1 should be 300px (30% of 996), max is 300
            target.Width = 1000;
            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            Assert.True(child1.Bounds.Width >= 200, $"At 1000px width, child width {child1.Bounds.Width} should be >= 200");
            Assert.True(child1.Bounds.Width <= 300, $"At 1000px width, child width {child1.Bounds.Width} should be <= 300");

            // Test Case 3: Panel width = 2000px - child1 should be 600px (30% of 1996), but max is 300
            target.Width = 2000;
            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            Assert.True(child1.Bounds.Width >= 200, $"At 2000px width, child width {child1.Bounds.Width} should be >= 200");
            Assert.True(child1.Bounds.Width <= 300, $"At 2000px width, child width {child1.Bounds.Width} should be <= 300");
        }

        [AvaloniaFact]
        public void Simulates_Splitter_Dragging_Behavior_With_Constraints()
        {
            var target = new ProportionalStackPanel
            {
                Width = 800,
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MinWidth = 150,
                        MaxWidth = 400,
                        [ProportionalStackPanel.ProportionProperty] = 0.4  // 320px initially
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        MinWidth = 100,
                        [ProportionalStackPanel.ProportionProperty] = 0.6  // 480px initially
                    }
                }
            };

            // Initial layout
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));
            
            var child1 = target.Children[0];
            var child2 = target.Children[2];
            
            // Verify initial layout respects constraints
            Assert.True(child1.Bounds.Width >= 150, $"Initial child1 width {child1.Bounds.Width} should be >= 150");
            Assert.True(child1.Bounds.Width <= 400, $"Initial child1 width {child1.Bounds.Width} should be <= 400");
            Assert.True(child2.Bounds.Width >= 100, $"Initial child2 width {child2.Bounds.Width} should be >= 100");
            
            // Simulate dragging splitter to shrink child1 below minimum
            ProportionalStackPanel.SetProportion(child1, 0.1);  // Try to set to ~80px, but min is 150px
            ProportionalStackPanel.SetProportion(child2, 0.9);
            
            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));
            
            Assert.True(child1.Bounds.Width >= 150, $"After shrinking, child1 width {child1.Bounds.Width} should be >= 150");
            Assert.True(child2.Bounds.Width >= 100, $"After shrinking, child2 width {child2.Bounds.Width} should be >= 100");
            
            // Simulate dragging splitter to expand child1 beyond maximum
            ProportionalStackPanel.SetProportion(child1, 0.8);  // Try to set to ~640px, but max is 400px
            ProportionalStackPanel.SetProportion(child2, 0.2);
            
            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));
            
            Assert.True(child1.Bounds.Width >= 150, $"After expanding, child1 width {child1.Bounds.Width} should be >= 150");
            Assert.True(child1.Bounds.Width <= 400, $"After expanding, child1 width {child1.Bounds.Width} should be <= 400");
            Assert.True(child2.Bounds.Width >= 100, $"After expanding, child2 width {child2.Bounds.Width} should be >= 100");
        }

        [AvaloniaFact]
        public void CalculateDimensionWithConstraints_Respects_MinWidth()
        {
            var target = new ProportionalStackPanel
            {
                Width = 400,
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MinWidth = 200,
                        [ProportionalStackPanel.ProportionProperty] = 0.1  // Would be 40px without constraints
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 0.9
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            Assert.True(child1.Bounds.Width >= 200, $"Child with MinWidth=200 should be at least 200px, got {child1.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void CalculateDimensionWithConstraints_Respects_MaxWidth()
        {
            var target = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MaxWidth = 150,
                        [ProportionalStackPanel.ProportionProperty] = 0.8  // Would be 480px without constraints
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 0.2
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            Assert.True(child1.Bounds.Width <= 150, $"Child with MaxWidth=150 should be at most 150px, got {child1.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void CalculateDimensionWithConstraints_Respects_MinHeight_Vertical()
        {
            var target = new ProportionalStackPanel
            {
                Width = 100,
                Height = 400,
                Orientation = Orientation.Vertical,
                Children =
                {
                    new Border
                    {
                        MinHeight = 200,
                        [ProportionalStackPanel.ProportionProperty] = 0.1  // Would be 40px without constraints
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 0.9
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            Assert.True(child1.Bounds.Height >= 200, $"Child with MinHeight=200 should be at least 200px, got {child1.Bounds.Height}px");
        }

        [AvaloniaFact]
        public void CalculateDimensionWithConstraints_Respects_MaxHeight_Vertical()
        {
            var target = new ProportionalStackPanel
            {
                Width = 100,
                Height = 600,
                Orientation = Orientation.Vertical,
                Children =
                {
                    new Border
                    {
                        MaxHeight = 150,
                        [ProportionalStackPanel.ProportionProperty] = 0.8  // Would be 480px without constraints
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 0.2
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            Assert.True(child1.Bounds.Height <= 150, $"Child with MaxHeight=150 should be at most 150px, got {child1.Bounds.Height}px");
        }

        [AvaloniaFact]
        public void Multiple_Children_With_Mixed_Constraints()
        {
            var target = new ProportionalStackPanel
            {
                Width = 1000,
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MinWidth = 100,
                        MaxWidth = 200,
                        [ProportionalStackPanel.ProportionProperty] = 0.3
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        MinWidth = 150,
                        [ProportionalStackPanel.ProportionProperty] = 0.4
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        MaxWidth = 250,
                        [ProportionalStackPanel.ProportionProperty] = 0.3
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            var child2 = target.Children[2];
            var child3 = target.Children[4];

            // Child 1: MinWidth=100, MaxWidth=200
            Assert.True(child1.Bounds.Width >= 100, $"Child1 width {child1.Bounds.Width} should be >= 100");
            Assert.True(child1.Bounds.Width <= 200, $"Child1 width {child1.Bounds.Width} should be <= 200");

            // Child 2: MinWidth=150
            Assert.True(child2.Bounds.Width >= 150, $"Child2 width {child2.Bounds.Width} should be >= 150");

            // Child 3: MaxWidth=250
            Assert.True(child3.Bounds.Width <= 250, $"Child3 width {child3.Bounds.Width} should be <= 250");
        }

        [AvaloniaFact]
        public void Splitter_Dragging_Respects_Target_MinWidth()
        {
            var target = new ProportionalStackPanel
            {
                Width = 500,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                MinWidth = 100,
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            target.Children.Add(child1);
            target.Children.Add(splitter);
            target.Children.Add(child2);

            // Initial layout
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            // Simulate splitter dragging that would violate minimum
            splitter.GetType().GetMethod("SetTargetProportion", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(splitter, new object[] { -150.0 }); // Try to shrink child1 by 150px

            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            Assert.True(child1.Bounds.Width >= 100, $"After splitter drag, child1 width {child1.Bounds.Width} should respect MinWidth=100");
        }

        [AvaloniaFact]
        public void Splitter_Dragging_Respects_Target_MaxWidth()
        {
            var target = new ProportionalStackPanel
            {
                Width = 500,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                MaxWidth = 200,
                [ProportionalStackPanel.ProportionProperty] = 0.3
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.7
            };

            target.Children.Add(child1);
            target.Children.Add(splitter);
            target.Children.Add(child2);

            // Initial layout
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            // Simulate splitter dragging that would violate maximum
            splitter.GetType().GetMethod("SetTargetProportion", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(splitter, new object[] { 200.0 }); // Try to expand child1 by 200px

            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            Assert.True(child1.Bounds.Width <= 200, $"After splitter drag, child1 width {child1.Bounds.Width} should respect MaxWidth=200");
        }

        [AvaloniaFact]
        public void Splitter_Dragging_Respects_Neighbor_MinWidth()
        {
            var target = new ProportionalStackPanel
            {
                Width = 500,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                MinWidth = 100,
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            target.Children.Add(child1);
            target.Children.Add(splitter);
            target.Children.Add(child2);

            // Initial layout
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            // Simulate splitter dragging that would violate neighbor's minimum
            splitter.GetType().GetMethod("SetTargetProportion", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(splitter, new object[] { 150.0 }); // Try to expand child1, shrinking child2

            target.InvalidateMeasure();
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            Assert.True(child2.Bounds.Width >= 100, $"After splitter drag, child2 width {child2.Bounds.Width} should respect MinWidth=100");
        }

        [AvaloniaFact]
        public void Constraints_Work_With_NaN_Proportions()
        {
            var target = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MinWidth = 150,
                        MaxWidth = 250
                        // No proportion set (should be NaN)
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        MinWidth = 100
                        // No proportion set (should be NaN)
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            var child2 = target.Children[2];

            Assert.True(child1.Bounds.Width >= 150, $"Child1 with NaN proportion should respect MinWidth=150, got {child1.Bounds.Width}px");
            Assert.True(child1.Bounds.Width <= 250, $"Child1 with NaN proportion should respect MaxWidth=250, got {child1.Bounds.Width}px");
            Assert.True(child2.Bounds.Width >= 100, $"Child2 with NaN proportion should respect MinWidth=100, got {child2.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Constraints_Work_With_Zero_Proportions()
        {
            var target = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MinWidth = 150,
                        [ProportionalStackPanel.ProportionProperty] = 0.0
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        [ProportionalStackPanel.ProportionProperty] = 1.0
                    }
                }
            };

            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            
            // Even with 0 proportion, MinWidth should be respected
            Assert.True(child1.Bounds.Width >= 150, $"Child1 with 0 proportion should still respect MinWidth=150, got {child1.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Extreme_Constraints_Dont_Break_Layout()
        {
            var target = new ProportionalStackPanel
            {
                Width = 200, // Very small panel
                Height = 100,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        MinWidth = 150, // Min larger than available space
                        [ProportionalStackPanel.ProportionProperty] = 0.5
                    },
                    new ProportionalStackPanelSplitter(),
                    new Border
                    {
                        MinWidth = 100, // Min larger than remaining space
                        [ProportionalStackPanel.ProportionProperty] = 0.5
                    }
                }
            };

            // Should not throw exceptions
            target.Measure(Size.Infinity);
            target.Arrange(new Rect(target.DesiredSize));

            var child1 = target.Children[0];
            var child2 = target.Children[2];

            // Constraints should still be respected even if it causes overflow
            Assert.True(child1.Bounds.Width >= 150, $"Child1 should respect MinWidth=150 even in constrained space, got {child1.Bounds.Width}px");
            Assert.True(child2.Bounds.Width >= 100, $"Child2 should respect MinWidth=100 even in constrained space, got {child2.Bounds.Width}px");
        }
    }
}
