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
