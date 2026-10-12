using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
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
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Local_Targets_Inherit_The_Surface_Theme_And_Release_It_When_Removed(bool floating)
    {
        Border surface = new() { Width = 240, Height = 180 };
        Window window = new() { Width = 400, Height = 300, Content = surface };
        ControlTheme theme = new(typeof(DockTarget));
        theme.Setters.Add(new Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate((_, scope) =>
        {
            Border selector = new() { Width = 40, Height = 40 };
            Panel indicator = new();
            DockProperties.SetIndicatorDockOperation(selector, DockOperation.OuterLeft);
            DockProperties.SetIndicatorDockOperation(indicator, DockOperation.OuterLeft);
            scope.Register("PART_OuterLeftSelector", selector);
            scope.Register("PART_OuterLeftIndicator", indicator);
            return new Panel { Children = { indicator, selector } };
        })));
        DockTarget.SetTargetTheme(window, theme);
        AdornerHelper<DockTarget> helper = new(floating);
        try
        {
            window.Show(); window.UpdateLayout();
            Assert.Same(theme, DockTarget.GetTargetTheme(surface));
            helper.AddAdorner(surface, false);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            DockTarget target = Assert.IsType<DockTarget>(helper.Adorner);
            target.UpdateLayout();
            Assert.Same(theme, target.Theme);
            Assert.NotNull(target.GetSelector(DockOperation.OuterLeft));
            helper.RemoveAdorner(surface);
            Assert.Null(target.Theme);
            DockTarget.SetTargetTheme(window, null);
            Assert.Null(DockTarget.GetTargetTheme(surface));
        }
        finally { helper.RemoveAdorner(surface); window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaFact]
    public void AddRemove_Reuses_Same_Instance()
    {
        var root = (AdornerLayer)Activator.CreateInstance(typeof(AdornerLayer), nonPublic: true)!;
        var control = new Border();
        root.Children.Add(control);

        var helper = new AdornerHelper<DockTarget>(false);

        helper.AddAdorner(control, false);
        var first = helper.Adorner;
        helper.RemoveAdorner(control);
        helper.AddAdorner(control, false);
        var second = helper.Adorner;

        Assert.Same(first, second);
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
