using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class AdornerGeometryTests
{
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Expanded_Adorners_Follow_Layout_And_Release_The_Surface_On_Detach(int hosting)
    {
        bool oldManaged = DockSettings.UseManagedWindows;
        DockSettings.UseManagedWindows = hosting == 2;
        Border surface = new();
        Border host = new() { Child = surface, Padding = new Thickness(30, 20, 50, 40) };
        Panel content = new() { Children = { host } };
        if (hosting == 2) content.Children.Add(new ManagedWindowLayer());
        Window window = new() { Width = 600, Height = 400, Content = content };
        FixedHost provider = new(host);
        DockTarget.SetAdornerHostProvider(window, provider);
        AdornerHelper<DockTarget> helper = new(hosting != 0);
        try
        {
            window.Show(); Update();
            Assert.Same(provider, DockTarget.GetAdornerHostProvider(surface));
            helper.AddAdorner(surface, false); Update();
            DockTarget target = Assert.IsType<DockTarget>(helper.Adorner);
            target.UpdateLayout();
            Assert.Equal(host.Padding, target.TargetMargin);
            AssertSameScreenBounds(host, target);
            Assert.Equal(1, provider.Calls);
            Assert.Same(surface, provider.Surface);

            host.Padding = new Thickness(45, 35, 25, 15);
            host.Width = 450; host.Height = 250;
            window.Position = new PixelPoint(70, 90);
            Update(); target.UpdateLayout();
            Assert.Equal(host.Padding, target.TargetMargin);
            AssertSameScreenBounds(host, target);
            Assert.Equal(1, provider.Calls);

            window.Content = null; Update();
            Assert.Null(helper.Adorner);
            Assert.Equal(default, target.TargetMargin);
            Assert.Null(AdornerLayer.GetAdornedElement(target));
            Assert.Null(target.GetVisualParent());
            window.Width += 100; Update();
            Assert.Equal(default, target.TargetMargin);

            window.Content = content;
            DockTarget.SetAdornerHostProvider(window, null);
            Update();
            helper.AddAdorner(surface, false); Update(); target.UpdateLayout();
            Assert.Same(target, helper.Adorner);
            Assert.Null(DockTarget.GetAdornerHostProvider(surface));
            Assert.Equal(default, target.TargetMargin);
            AssertSameScreenBounds(surface, target);
        }
        finally
        {
            helper.RemoveAdorner(surface); window.Close(); Dispatcher.UIThread.RunJobs();
            DockSettings.UseManagedWindows = oldManaged;
        }
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }

    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    public void Null_Self_And_Unrelated_Hosts_Keep_The_Original_Surface(bool floating, int kind)
    {
        Border surface = new() { Width = 220, Height = 140 };
        Window window = new() { Width = 600, Height = 400, Content = surface };
        DockTarget.SetAdornerHostProvider(window, new FixedHost(kind switch { 1 => surface, 2 => new Border(), _ => null }));
        AdornerHelper<DockTarget> helper = new(floating);
        try
        {
            window.Show(); window.UpdateLayout();
            helper.AddAdorner(surface, false); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            DockTarget target = Assert.IsType<DockTarget>(helper.Adorner);
            target.UpdateLayout();
            Assert.Equal(default, target.TargetMargin);
            AssertSameScreenBounds(surface, target);
        }
        finally { helper.RemoveAdorner(surface); window.Close(); }
    }

    [AvaloniaFact]
    public void Managed_Local_And_Global_Adorners_Keep_Separate_Controls_And_Lifetimes()
    {
        bool oldManaged = DockSettings.UseManagedWindows;
        DockSettings.UseManagedWindows = true;
        Border surface = new() { Width = 220, Height = 140 };
        ManagedWindowLayer layer = new();
        Panel content = new() { Children = { surface, layer } };
        Window window = new() { Width = 600, Height = 400, Content = content };
        AdornerHelper<DockTarget> local = new(true);
        AdornerHelper<GlobalDockTarget> global = new(true);
        try
        {
            window.Show(); window.UpdateLayout();
            local.AddAdorner(surface, false);
            global.AddAdorner(content, false);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.Contains(local.Adorner, layer.GetVisualDescendants());
            Assert.Contains(global.Adorner, layer.GetVisualDescendants());
            AssertSameScreenBounds(surface, local.Adorner!);
            AssertSameScreenBounds(content, global.Adorner!);
            local.RemoveAdorner(surface);
            Assert.Contains(global.Adorner, layer.GetVisualDescendants());
            Assert.Empty(layer.GetVisualDescendants().OfType<DockTarget>());
            global.RemoveAdorner(content);
            Assert.Empty(layer.GetVisualDescendants().OfType<GlobalDockTarget>());
        }
        finally
        {
            local.RemoveAdorner(surface); global.RemoveAdorner(content); window.Close();
            DockSettings.UseManagedWindows = oldManaged;
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selector_Hit_Testing_Uses_The_Same_Point_Across_Transformed_Visuals(bool floating)
    {
        Border surface = new() { Width = 400, Height = 250 };
        Border host = new() { Child = surface, RenderTransformOrigin = RelativePoint.TopLeft,
            RenderTransform = new ScaleTransform(0.65, 0.8), Margin = new Thickness(70, 45, 0, 0) };
        Window window = new() { Width = 900, Height = 600, Content = host };
        AdornerHelper<DockTarget> helper = new(floating);
        try
        {
            window.Show(); window.UpdateLayout();
            helper.AddAdorner(surface, false); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            DockTarget target = Assert.IsType<DockTarget>(helper.Adorner);
            target.UpdateLayout();
            Control selector = Assert.IsAssignableFrom<Control>(target.GetSelector(DockOperation.Left));
            PixelPoint screenPoint = selector.PointToScreen(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2));
            Point point = host.PointToClient(screenPoint);
            Assert.Equal(DockOperation.Left, target.GetDockOperation(point, surface, host, DragAction.Move,
                (actual, _, _, relative) => { Assert.Equal(point, actual); Assert.Same(host, relative); return true; }));
            Assert.Equal(DockOperation.Left, target.GetDockOperation(window.PointToClient(screenPoint), surface, window,
                DragAction.Move, (_, _, _, _) => true));
        }
        finally { helper.RemoveAdorner(surface); window.Close(); }
    }

    private static void AssertSameScreenBounds(Control expected, Control actual)
    {
        Assert.Equal(expected.PointToScreen(default), actual.PointToScreen(default));
        Assert.Equal(expected.PointToScreen(new Point(expected.Bounds.Width, expected.Bounds.Height)),
            actual.PointToScreen(new Point(actual.Bounds.Width, actual.Bounds.Height)));
    }

    private sealed class FixedHost(Control? host) : IDockAdornerHostProvider
    {
        public int Calls { get; private set; }
        public Visual? Surface { get; private set; }
        public Control? GetAdornerHost(Visual surface) { Calls++; Surface = surface; return host; }
    }
}
