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
    public class ProportionalStackPanelSplitterConstraintsTests
    {
        private void InvokeSetTargetProportion(ProportionalStackPanelSplitter splitter, double dragDelta)
        {
            var method = typeof(ProportionalStackPanelSplitter).GetMethod("SetTargetProportion", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(splitter, new object[] { dragDelta });
        }

        [AvaloniaFact]
        public void SetTargetProportion_Respects_Target_MinWidth_Constraint()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                MinWidth = 150,
                [ProportionalStackPanel.ProportionProperty] = 0.4
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.6
            };

            panel.Children.Add(child1);
            panel.Children.Add(splitter);
            panel.Children.Add(child2);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Simulate dragging splitter to shrink child1 beyond its minimum
            InvokeSetTargetProportion(splitter, -200); // Try to shrink by 200px

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(child1.Bounds.Width >= 150, 
                $"Target element should respect MinWidth=150 after splitter drag, got {child1.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void SetTargetProportion_Respects_Target_MaxWidth_Constraint()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                MaxWidth = 250,
                [ProportionalStackPanel.ProportionProperty] = 0.3
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.7
            };

            panel.Children.Add(child1);
            panel.Children.Add(splitter);
            panel.Children.Add(child2);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Simulate dragging splitter to expand child1 beyond its maximum
            InvokeSetTargetProportion(splitter, 200); // Try to expand by 200px

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(child1.Bounds.Width <= 250, 
                $"Target element should respect MaxWidth=250 after splitter drag, got {child1.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void SetTargetProportion_Respects_Neighbor_MinWidth_Constraint()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.4
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                MinWidth = 200,
                [ProportionalStackPanel.ProportionProperty] = 0.6
            };

            panel.Children.Add(child1);
            panel.Children.Add(splitter);
            panel.Children.Add(child2);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Simulate dragging splitter to expand child1, which would shrink child2 beyond its minimum
            InvokeSetTargetProportion(splitter, 250); // Try to expand child1 by 250px

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(child2.Bounds.Width >= 200, 
                $"Neighbor element should respect MinWidth=200 after splitter drag, got {child2.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void SetTargetProportion_Respects_Neighbor_MaxWidth_Constraint()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 600,
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                [ProportionalStackPanel.ProportionProperty] = 0.6
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                MaxWidth = 150,
                [ProportionalStackPanel.ProportionProperty] = 0.4
            };

            panel.Children.Add(child1);
            panel.Children.Add(splitter);
            panel.Children.Add(child2);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Simulate dragging splitter to shrink child1, which would expand child2 beyond its maximum
            InvokeSetTargetProportion(splitter, -200); // Try to shrink child1 by 200px

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(child2.Bounds.Width <= 150, 
                $"Neighbor element should respect MaxWidth=150 after splitter drag, got {child2.Bounds.Width}px");
        }

        [AvaloniaFact]
        public void SetTargetProportion_Works_In_Vertical_Orientation()
        {
            var panel = new ProportionalStackPanel
            {
                Width = 100,
                Height = 600,
                Orientation = Orientation.Vertical
            };

            var child1 = new Border
            {
                MinHeight = 150,
                MaxHeight = 300,
                [ProportionalStackPanel.ProportionProperty] = 0.4
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                MinHeight = 100,
                [ProportionalStackPanel.ProportionProperty] = 0.6
            };

            panel.Children.Add(child1);
            panel.Children.Add(splitter);
            panel.Children.Add(child2);

            // Initial layout
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Test minimum constraint
            InvokeSetTargetProportion(splitter, -200); // Try to shrink child1

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(child1.Bounds.Height >= 150, 
                $"Child1 should respect MinHeight=150 in vertical orientation, got {child1.Bounds.Height}px");
            Assert.True(child2.Bounds.Height >= 100, 
                $"Child2 should respect MinHeight=100 in vertical orientation, got {child2.Bounds.Height}px");

            // Test maximum constraint
            InvokeSetTargetProportion(splitter, 200); // Try to expand child1

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            Assert.True(child1.Bounds.Height <= 300, 
                $"Child1 should respect MaxHeight=300 in vertical orientation, got {child1.Bounds.Height}px");
        }

        [AvaloniaFact]
        public void SetTargetProportion_Handles_Conflicting_Constraints_Gracefully()
        {
            // Test case where both children have constraints that can't be satisfied simultaneously
            var panel = new ProportionalStackPanel
            {
                Width = 300, // Small total width
                Height = 100,
                Orientation = Orientation.Horizontal
            };

            var child1 = new Border
            {
                MinWidth = 200, // Large minimum
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            var splitter = new ProportionalStackPanelSplitter();

            var child2 = new Border
            {
                MinWidth = 150, // Large minimum  
                [ProportionalStackPanel.ProportionProperty] = 0.5
            };

            panel.Children.Add(child1);
            panel.Children.Add(splitter);
            panel.Children.Add(child2);

            // Should not throw exceptions even with conflicting constraints
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Try to drag splitter
            InvokeSetTargetProportion(splitter, 50);

            panel.InvalidateMeasure();
            panel.Measure(Size.Infinity);
            panel.Arrange(new Rect(panel.DesiredSize));

            // Both minimums should still be respected even if they cause overflow
            Assert.True(child1.Bounds.Width >= 200, 
                $"Child1 should respect MinWidth=200 even with conflicting constraints, got {child1.Bounds.Width}px");
            Assert.True(child2.Bounds.Width >= 150, 
                $"Child2 should respect MinWidth=150 even with conflicting constraints, got {child2.Bounds.Width}px");
        }
    }
}
