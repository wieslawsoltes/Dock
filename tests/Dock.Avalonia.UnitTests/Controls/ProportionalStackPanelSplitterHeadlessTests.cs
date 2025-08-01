using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Dock.Controls.ProportionalStackPanel;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class ProportionalStackPanelSplitterHeadlessTests
    {
        private void InvokeSetTargetProportion(ProportionalStackPanelSplitter splitter, double delta)
        {
            var method = typeof(ProportionalStackPanelSplitter).GetMethod("SetTargetProportion",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(splitter, new object[] { delta });
        }

        [AvaloniaFact]
        public void Splitter_Respects_Target_MinWidth_Constraint_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                MinWidth = 200,
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialTargetWidth = target.Bounds.Width;
            var initialNeighborWidth = neighbor.Bounds.Width;

            // Try to shrink target by 150px, which would make it 150px (below MinWidth=200)
            InvokeSetTargetProportion(splitter, -150.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(target.Bounds.Width >= 200, 
                $"Target should respect MinWidth=200, got {target.Bounds.Width}px");
            Assert.True(target.Bounds.Width < initialTargetWidth, 
                $"Target should be smaller than initial {initialTargetWidth}px, got {target.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Respects_Target_MaxWidth_Constraint_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 800,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                MaxWidth = 350,
                [ProportionalStackPanel.ProportionProperty] = 0.4 // 320px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.6 // 480px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialTargetWidth = target.Bounds.Width;

            // Try to expand target by 100px, which would make it 420px (above MaxWidth=350)
            InvokeSetTargetProportion(splitter, 100.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(target.Bounds.Width <= 350, 
                $"Target should respect MaxWidth=350, got {target.Bounds.Width}px");
            Assert.True(target.Bounds.Width > initialTargetWidth, 
                $"Target should be larger than initial {initialTargetWidth}px, got {target.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Respects_Neighbor_MinWidth_Constraint_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MinWidth = 150,
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialNeighborWidth = neighbor.Bounds.Width;

            // Try to expand target by 200px, which would shrink neighbor to 100px (below MinWidth=150)
            InvokeSetTargetProportion(splitter, 200.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(neighbor.Bounds.Width >= 150, 
                $"Neighbor should respect MinWidth=150, got {neighbor.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width < initialNeighborWidth, 
                $"Neighbor should be smaller than initial {initialNeighborWidth}px, got {neighbor.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Respects_Neighbor_MaxWidth_Constraint_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 800,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.6 // 480px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MaxWidth = 250,
                [ProportionalStackPanel.ProportionProperty] = 0.4 // 320px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialNeighborWidth = neighbor.Bounds.Width;

            // Try to shrink target by 100px, which would expand neighbor to 420px (above MaxWidth=250)
            InvokeSetTargetProportion(splitter, -100.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(neighbor.Bounds.Width <= 250, 
                $"Neighbor should respect MaxWidth=250, got {neighbor.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Respects_Target_MinHeight_Constraint_Vertical_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 100,
                Height = 600,
                Orientation = Orientation.Vertical
            };

            var target = new Border
            {
                MinHeight = 200,
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialTargetHeight = target.Bounds.Height;

            // Try to shrink target by 150px, which would make it 150px (below MinHeight=200)
            InvokeSetTargetProportion(splitter, -150.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(target.Bounds.Height >= 200, 
                $"Target should respect MinHeight=200, got {target.Bounds.Height}px");
        }

        [AvaloniaFact]
        public void Splitter_Respects_Target_MaxHeight_Constraint_Vertical_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 100,
                Height = 800,
                Orientation = Orientation.Vertical
            };

            var target = new Border
            {
                MaxHeight = 350,
                [ProportionalStackPanel.ProportionProperty] = 0.4 // 320px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.6 // 480px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialTargetHeight = target.Bounds.Height;

            // Try to expand target by 100px, which would make it 420px (above MaxHeight=350)
            InvokeSetTargetProportion(splitter, 100.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(target.Bounds.Height <= 350, 
                $"Target should respect MaxHeight=350, got {target.Bounds.Height}px");
            Assert.True(target.Bounds.Height > initialTargetHeight, 
                $"Target should be larger than initial {initialTargetHeight}px, got {target.Bounds.Height}px");
        }

        [AvaloniaFact]
        public void Splitter_Respects_Neighbor_MinHeight_Constraint_Vertical_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 100,
                Height = 600,
                Orientation = Orientation.Vertical
            };

            var target = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MinHeight = 150,
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialNeighborHeight = neighbor.Bounds.Height;

            // Try to expand target by 200px, which would shrink neighbor to 100px (below MinHeight=150)
            InvokeSetTargetProportion(splitter, 200.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(neighbor.Bounds.Height >= 150, 
                $"Neighbor should respect MinHeight=150, got {neighbor.Bounds.Height}px");
            Assert.True(neighbor.Bounds.Height < initialNeighborHeight, 
                $"Neighbor should be smaller than initial {initialNeighborHeight}px, got {neighbor.Bounds.Height}px");
        }

        [AvaloniaFact]
        public void Splitter_Respects_Neighbor_MaxHeight_Constraint_Vertical_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 100,
                Height = 800,
                Orientation = Orientation.Vertical
            };

            var target = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.6 // 480px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MaxHeight = 250,
                [ProportionalStackPanel.ProportionProperty] = 0.4 // 320px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialNeighborHeight = neighbor.Bounds.Height;

            // Try to shrink target by 100px, which would expand neighbor to 420px (above MaxHeight=250)
            InvokeSetTargetProportion(splitter, -100.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(neighbor.Bounds.Height <= 250, 
                $"Neighbor should respect MaxHeight=250, got {neighbor.Bounds.Height}px");
        }

        [AvaloniaFact]
        public void Splitter_Handles_Both_Target_And_Neighbor_MinWidth_Constraints_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 500,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                MinWidth = 150,
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 250px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MinWidth = 120,
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 250px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Try to shrink target significantly, which would violate both constraints
            InvokeSetTargetProportion(splitter, -200.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(target.Bounds.Width >= 150, 
                $"Target should respect MinWidth=150, got {target.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width >= 120, 
                $"Neighbor should respect MinWidth=120, got {neighbor.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Handles_Both_Target_And_Neighbor_MaxWidth_Constraints_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                MaxWidth = 250,
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MaxWidth = 200,
                [ProportionalStackPanel.ProportionProperty] = 0.5 // 300px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Try to expand target significantly, which would violate both constraints
            InvokeSetTargetProportion(splitter, 150.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(target.Bounds.Width <= 250, 
                $"Target should respect MaxWidth=250, got {target.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width <= 200, 
                $"Neighbor should respect MaxWidth=200, got {neighbor.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Handles_Mixed_MinMax_Constraints_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 800,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                MinWidth = 200,
                MaxWidth = 350,
                [ProportionalStackPanel.ProportionProperty] = 0.4 // 320px initially
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MinWidth = 150,
                MaxWidth = 300,
                [ProportionalStackPanel.ProportionProperty] = 0.6 // 480px initially
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Test 1: Try to shrink target below minimum
            InvokeSetTargetProportion(splitter, -150.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(target.Bounds.Width >= 200, 
                $"Target should respect MinWidth=200 after shrinking, got {target.Bounds.Width}px");
            Assert.True(target.Bounds.Width <= 350, 
                $"Target should respect MaxWidth=350 after shrinking, got {target.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width >= 150, 
                $"Neighbor should respect MinWidth=150 after target shrinking, got {neighbor.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width <= 300, 
                $"Neighbor should respect MaxWidth=300 after target shrinking, got {neighbor.Bounds.Width}px");

            // Test 2: Try to expand target beyond maximum
            InvokeSetTargetProportion(splitter, 200.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(target.Bounds.Width >= 200, 
                $"Target should respect MinWidth=200 after expanding, got {target.Bounds.Width}px");
            Assert.True(target.Bounds.Width <= 350, 
                $"Target should respect MaxWidth=350 after expanding, got {target.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width >= 150, 
                $"Neighbor should respect MinWidth=150 after target expanding, got {neighbor.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width <= 300, 
                $"Neighbor should respect MaxWidth=300 after target expanding, got {neighbor.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Handles_Conflicting_Constraints_Gracefully_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 300, // Very small panel
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                MinWidth = 200, // Min larger than half panel width
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MinWidth = 150, // Min larger than remaining space after target min
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Should not throw exceptions even with impossible constraints
            Exception? caughtException = null;
            try
            {
                panel.Measure(Size.Infinity);
                panel.Arrange(new Rect(panel.DesiredSize));

                // Try various drag operations
                InvokeSetTargetProportion(splitter, 50.0);
                panel.InvalidateMeasure();
                panel.Measure(Size.Infinity);
                panel.Arrange(new Rect(panel.DesiredSize));

                InvokeSetTargetProportion(splitter, -50.0);
                panel.InvalidateMeasure();
                panel.Measure(Size.Infinity);
                panel.Arrange(new Rect(panel.DesiredSize));
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
            
            Assert.Null(caughtException);

            // Constraints should still be respected even if they cause overflow
            Assert.True(target.Bounds.Width >= 200, 
                $"Target should respect MinWidth=200 even with conflicting constraints, got {target.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width >= 150, 
                $"Neighbor should respect MinWidth=150 even with conflicting constraints, got {neighbor.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Works_With_Multiple_Splitters_And_Constraints_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 1000,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                MinWidth = 100,
                MaxWidth = 300,
                [ProportionalStackPanel.ProportionProperty] = 0.25 // 250px initially
            };

            var splitter1 = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                MinWidth = 150,
                MaxWidth = 400,
                [ProportionalStackPanel.ProportionProperty] = 0.35 // 350px initially
            };

            var splitter2 = new ProportionalStackPanelSplitter();

            var child3 = new Border
            {
                MinWidth = 120,
                [ProportionalStackPanel.ProportionProperty] = 0.4 // 400px initially
            };

            panel.Children.Add(child1);
            panel.Children.Add(splitter1);
            panel.Children.Add(child2);
            panel.Children.Add(splitter2);
            panel.Children.Add(child3);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Test first splitter affecting child1 and child2
            InvokeSetTargetProportion(splitter1, -100.0); // Try to shrink child1

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(child1.Bounds.Width >= 100, 
                $"Child1 should respect MinWidth=100, got {child1.Bounds.Width}px");
            Assert.True(child1.Bounds.Width <= 300, 
                $"Child1 should respect MaxWidth=300, got {child1.Bounds.Width}px");
            Assert.True(child2.Bounds.Width >= 150, 
                $"Child2 should respect MinWidth=150, got {child2.Bounds.Width}px");
            Assert.True(child2.Bounds.Width <= 400, 
                $"Child2 should respect MaxWidth=400, got {child2.Bounds.Width}px");

            // Test second splitter affecting child2 and child3
            InvokeSetTargetProportion(splitter2, 80.0); // Try to expand child2

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(child2.Bounds.Width >= 150, 
                $"Child2 should respect MinWidth=150 after second splitter, got {child2.Bounds.Width}px");
            Assert.True(child2.Bounds.Width <= 400, 
                $"Child2 should respect MaxWidth=400 after second splitter, got {child2.Bounds.Width}px");
            Assert.True(child3.Bounds.Width >= 120, 
                $"Child3 should respect MinWidth=120, got {child3.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void Splitter_Handles_Zero_Delta_Without_Issues_Headless()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var target = new Border
            {
                MinWidth = 100,
                MaxWidth = 400,
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            var splitter = new ProportionalStackPanelSplitter();

            var neighbor = new Border
            {
                MinWidth = 80,
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            panel.Children.Add(target);
            panel.Children.Add(splitter);
            panel.Children.Add(neighbor);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            var initialTargetWidth = target.Bounds.Width;
            var initialNeighborWidth = neighbor.Bounds.Width;

            // Apply zero delta (no change)
            InvokeSetTargetProportion(splitter, 0.0);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Widths should remain the same
            Assert.Equal(initialTargetWidth, target.Bounds.Width);
            Assert.Equal(initialNeighborWidth, neighbor.Bounds.Width);

            // Constraints should still be respected
            Assert.True(target.Bounds.Width >= 100, 
                $"Target should respect MinWidth=100, got {target.Bounds.Width}px");
            Assert.True(target.Bounds.Width <= 400, 
                $"Target should respect MaxWidth=400, got {target.Bounds.Width}px");
            Assert.True(neighbor.Bounds.Width >= 80, 
                $"Neighbor should respect MinWidth=80, got {neighbor.Bounds.Width}px");
        }
    }
}
