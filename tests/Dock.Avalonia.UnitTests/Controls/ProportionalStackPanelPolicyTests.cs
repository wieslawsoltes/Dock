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
}
