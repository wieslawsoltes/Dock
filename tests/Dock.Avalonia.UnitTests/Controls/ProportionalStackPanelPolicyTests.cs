using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Dock.Controls.ProportionalStackPanel;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class ProportionalStackPanelPolicyTests
{
    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void Measurement_Policies_Resolve_Infinite_Space_And_Preserve_Intrinsic_Child_Constraints(Orientation orientation)
    {
        MeasurementPanel panel = new() { Orientation = orientation };
        MeasuringControl intrinsic = new(), proportional = new();
        panel.Children.Add(intrinsic);
        panel.Children.Add(new ProportionalStackPanelSplitter { Thickness = 10 });
        panel.Children.Add(proportional);
        Size offered = orientation == Orientation.Horizontal ? new Size(double.PositiveInfinity, 400) : new Size(400, double.PositiveInfinity);
        panel.Measure(offered);
        panel.Arrange(new Rect(panel.DesiredSize));
        Assert.Equal(offered, panel.Offered);
        Assert.Equal(10, panel.Gaps);
        Assert.Equal(310, orientation == Orientation.Horizontal ? panel.DesiredSize.Width : panel.DesiredSize.Height);
        Assert.Equal(double.PositiveInfinity, orientation == Orientation.Horizontal ? intrinsic.Constraint.Width : intrinsic.Constraint.Height);
        Assert.Equal(150, orientation == Orientation.Horizontal ? proportional.Constraint.Width : proportional.Constraint.Height);
        AssertExtent(intrinsic, orientation, 150);
        AssertExtent(proportional, orientation, 150);

        ProportionalStackPanel defaults = new() { Orientation = orientation };
        defaults.Children.Add(new Border());
        Assert.Throws<Exception>(() => defaults.Measure(offered));
    }

    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void Desired_Extent_Uses_The_Overridden_Dimension_When_Gaps_Exceed_The_Viewport(Orientation orientation)
    {
        PolicyPanel panel = Create(orientation);
        Size desired = panel.MeasureWithPolicy(new Size(4, 4));
        Assert.Equal(300, orientation == Orientation.Horizontal ? desired.Width : desired.Height);
    }

    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void Assignment_And_Dimension_Overrides_Preserve_Explicit_Space(Orientation orientation)
    {
        PolicyPanel panel = Create(orientation);
        Window window = new() { Content = panel, Width = 600, Height = 500 };
        try
        {
            window.Show(); Update(window);
            Assert.True(panel.Assignments >= 2);
            Assert.True(panel.IsMeasureValid);
            AssertExtent(panel.Children[0], orientation, 120);
            AssertExtent(panel.Children[2], orientation, 180);
            window.Width += 100; window.Height += 100;
            Update(window);
            AssertExtent(panel.Children[0], orientation, 120);
            AssertExtent(panel.Children[2], orientation, 180);
            // The native default continues to normalize proportions to fill space.
            ProportionalStackPanel defaults = new() { Orientation = orientation };
            defaults.Children.Add(new Border { [ProportionalStackPanel.ProportionProperty] = .25 });
            defaults.Children.Add(new Border { [ProportionalStackPanel.ProportionProperty] = .25 });
            defaults.Measure(new Size(400, 400)); defaults.Arrange(new Rect(0, 0, 400, 400));
            AssertExtent(defaults.Children[0], orientation, 200);
            Assert.Equal(.5, ProportionalStackPanel.GetProportion(defaults.Children[0]));
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal, false)]
    [InlineData(Orientation.Vertical, false)]
    [InlineData(Orientation.Horizontal, true)]
    [InlineData(Orientation.Vertical, true)]
    public void Native_Pointer_And_Preview_Input_Invoke_The_Resize_Override(Orientation orientation, bool preview)
    {
        PolicyPanel panel = Create(orientation);
        ProportionalStackPanelSplitter splitter = (ProportionalStackPanelSplitter)panel.Children[1];
        splitter.PreviewResize = preview;
        ProportionalStackPanelSplitter.SetMinimumProportionSize(splitter, 25);
        Window window = new() { Content = panel, Width = 600, Height = 500 };
        try
        {
            window.Show(); Update(window);
            Point point = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), window)!.Value;
            Vector delta = orientation == Orientation.Horizontal ? new Vector(30, 0) : new Vector(0, 30);
            window.MouseDown(point, MouseButton.Left);
            window.MouseMove(point + delta);
            Assert.Equal(preview ? 0 : 1, panel.Resizes);
            window.MouseUp(point + delta, MouseButton.Left);
            Assert.Equal(1, panel.Resizes);
            Assert.Same(panel.Children[0], panel.Target);
            Assert.Same(panel.Children[2], panel.Neighbor);
            Assert.Equal(30, panel.Delta);
            Assert.Equal(25, panel.Minimum);
        }
        finally { window.Close(); }
    }

    private static PolicyPanel Create(Orientation orientation)
    {
        PolicyPanel panel = new() { Orientation = orientation };
        panel.Children.Add(new Border { Background = Brushes.White });
        panel.Children.Add(new ProportionalStackPanelSplitter
        {
            Thickness = 10,
            Template = new FuncControlTemplate<ProportionalStackPanelSplitter>((_, _) => new Border { Background = Brushes.Gray })
        });
        panel.Children.Add(new Border { Background = Brushes.White });
        return panel;
    }

    private static void Update(Window window) { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    private static void AssertExtent(Control control, Orientation orientation, double expected) =>
        Assert.InRange(orientation == Orientation.Horizontal ? control.Bounds.Width : control.Bounds.Height, expected - 1, expected + 1);

    private sealed class PolicyPanel : ProportionalStackPanel
    {
        public int Assignments { get; private set; }
        public int Resizes { get; private set; }
        public Control? Target { get; private set; }
        public Control? Neighbor { get; private set; }
        public double Delta { get; private set; }
        public double Minimum { get; private set; }
        public Size MeasureWithPolicy(Size size) => base.MeasureOverride(size);
        protected override double GetProportionalDimension(Size size, double splitterThickness) =>
            Math.Max(1, base.GetProportionalDimension(size, splitterThickness));
        protected override void OnAssignProportions(Size size, double splitterThickness)
        {
            Assignments++;
            double dimension = GetProportionalDimension(size, splitterThickness);
            SetProportion(Children[0], 120 / dimension);
            SetProportion(Children[2], 180 / dimension);
        }
        protected override void ResizeProportions(Control target, Control neighbor, double dragDelta, double minimumSize)
        {
            Resizes++; Target = target; Neighbor = neighbor; Delta = dragDelta; Minimum = minimumSize;
        }
    }

    private sealed class MeasuringControl : Control
    {
        public Size Constraint { get; private set; }
        protected override Size MeasureOverride(Size availableSize)
        {
            Constraint = availableSize;
            return new Size(100, 100);
        }
    }

    private sealed class MeasurementPanel : ProportionalStackPanel
    {
        public Size Offered { get; private set; }
        public double Gaps { get; private set; }
        protected override Size GetMeasureConstraint(Size constraint, double splitterThickness)
        {
            Offered = constraint;
            Gaps = splitterThickness;
            return Orientation == Orientation.Horizontal ? constraint.WithWidth(300 + splitterThickness) : constraint.WithHeight(300 + splitterThickness);
        }
        protected override Size GetChildMeasureConstraint(Control child, Size constraint) =>
            ReferenceEquals(child, Children[0])
                ? Orientation == Orientation.Horizontal ? constraint.WithWidth(double.PositiveInfinity) : constraint.WithHeight(double.PositiveInfinity)
                : constraint;
    }
}
