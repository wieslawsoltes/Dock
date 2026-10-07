using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class AdornerHelperTests
{
    [AvaloniaFact]
    public void AddRemove_Reuses_Same_Instance()
    {
        var control = new Border();
        using var window = new TestWindow(control);

        var helper = new AdornerHelper<DockTarget>(false);

        helper.AddAdorner(control, false);
        var first = helper.Adorner;
        Assert.NotNull(first);
        helper.RemoveAdorner(control);
        helper.AddAdorner(control, false);
        var second = helper.Adorner;

        Assert.Same(first, second);
        helper.RemoveAdorner(control);
    }

    [AvaloniaTheory]
    [InlineData(0)] // Re-enter the same target.
    [InlineData(1)] // Move between targets in the same window.
    [InlineData(2)] // Move between windows with different adorner layers.
    public void Regular_DockTarget_Can_Be_Added_Again(int targetKind)
    {
        AssertRegularAdornerCanBeAddedAgain<DockTarget>(targetKind);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Regular_GlobalDockTarget_Can_Be_Added_Again(int targetKind)
    {
        AssertRegularAdornerCanBeAddedAgain<GlobalDockTarget>(targetKind);
    }

    private static void AssertRegularAdornerCanBeAddedAgain<T>(int targetKind)
        where T : DockTargetBase, IDockTarget, new()
    {
        var first = new Border();
        var second = targetKind == 0 ? first : new Border();
        var panel = new Panel { Children = { first } };
        if (targetKind == 1)
        {
            panel.Children.Add(second);
        }
        using var firstWindow = new TestWindow(panel);
        using var secondWindow = targetKind == 2 ? new TestWindow(second) : null;

        var firstLayer = AdornerLayer.GetAdornerLayer(first);
        var secondLayer = AdornerLayer.GetAdornerLayer(second);
        Assert.NotNull(firstLayer);
        Assert.NotNull(secondLayer);
        var helper = new AdornerHelper<T>(false);
        helper.AddAdorner(first, false);
        var adorner = Assert.IsType<T>(helper.Adorner);
        Assert.Same(firstLayer, adorner.GetVisualParent());

        helper.AddAdorner(second, true, false, false);

        Assert.Same(adorner, helper.Adorner);
        Assert.Same(secondLayer, adorner.GetVisualParent());
        Assert.Same(second, adorner.Parent);
        Assert.Same(second, AdornerLayer.GetAdornedElement(adorner));
        Assert.Single(secondLayer.Children);
        if (targetKind == 2)
        {
            Assert.Empty(firstLayer.Children);
        }
        Assert.True(adorner.ShowIndicatorsOnly);
        Assert.False(adorner.ShowHorizontalTargets);
        Assert.False(adorner.ShowVerticalTargets);

        helper.RemoveAdorner(second);

        Assert.Null(helper.Adorner);
        Assert.Null(adorner.GetVisualParent());
        Assert.Null(adorner.Parent);
        Assert.Null(AdornerLayer.GetAdornedElement(adorner));
        Assert.Empty(firstLayer.Children);
        Assert.Empty(secondLayer.Children);
    }

    [AvaloniaFact]
    public void Regular_Adorner_Is_Removed_When_New_Target_Has_No_Layer()
    {
        var control = new Border();
        using var window = new TestWindow(control);
        var layer = AdornerLayer.GetAdornerLayer(control);
        Assert.NotNull(layer);
        var helper = new AdornerHelper<DockTarget>(false);
        helper.AddAdorner(control, false);
        var adorner = Assert.IsType<DockTarget>(helper.Adorner);

        helper.AddAdorner(new Border(), false);

        Assert.Null(helper.Adorner);
        Assert.Null(adorner.GetVisualParent());
        Assert.Null(adorner.Parent);
        Assert.Null(AdornerLayer.GetAdornedElement(adorner));
        Assert.Empty(layer.Children);

        helper.AddAdorner(control, false);
        Assert.Same(adorner, helper.Adorner);
        helper.RemoveAdorner(control);
        helper.RemoveAdorner(control);
        Assert.Empty(layer.Children);
    }

    private sealed class TestWindow : IDisposable
    {
        private readonly Window _window;

        public TestWindow(Control content)
        {
            _window = new Window { Content = content };
            _window.Show();
        }

        public void Dispose() => _window.Close();
    }

    [AvaloniaFact]
    public void Floating_Adorner_Center_Selector_Should_Resolve_Fill_Operation()
    {
        var hostWindow = new Window
        {
            Width = 400,
            Height = 300,
            Position = new PixelPoint(100, 100)
        };
        var dockSurface = new Border
        {
            Width = 240,
            Height = 180
        };
        var helper = new AdornerHelper<DockTarget>(true);

        hostWindow.Content = dockSurface;
        hostWindow.Show();
        hostWindow.UpdateLayout();
        dockSurface.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            helper.AddAdorner(dockSurface, indicatorsOnly: false);
            Dispatcher.UIThread.RunJobs();

            var dockTarget = Assert.IsType<DockTarget>(helper.Adorner);
            dockTarget.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var centerSelector = dockTarget
                .GetVisualDescendants()
                .OfType<Control>()
                .Single(control => DockProperties.GetIndicatorDockOperation(control) == DockOperation.Fill
                                   && control is Image);

            var centerScreenPoint = centerSelector.PointToScreen(new Point(
                centerSelector.Bounds.Width / 2,
                centerSelector.Bounds.Height / 2));
            var hostPoint = hostWindow.PointToClient(centerScreenPoint);
            var dockSurfaceOrigin = dockSurface.TranslatePoint(new Point(), hostWindow) ?? new Point();
            var dockSurfacePoint = hostPoint - dockSurfaceOrigin;

            var operation = dockTarget.GetDockOperation(
                dockSurfacePoint,
                new Border(),
                dockSurface,
                DragAction.Move,
                (_, _, _, _) => true);

            Assert.Equal(DockOperation.Fill, operation);
        }
        finally
        {
            helper.RemoveAdorner(dockSurface);
            hostWindow.Close();
        }
    }
}
