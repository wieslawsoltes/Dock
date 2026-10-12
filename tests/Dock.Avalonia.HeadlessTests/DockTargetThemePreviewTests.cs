using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using BrowserTheme = Dock.Avalonia.Themes.Browser.BrowserTabTheme;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockTargetThemePreviewTests
{
    [AvaloniaTheory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    public void Custom_theme_is_selected_before_any_template_is_built(int hosting, int themeSource)
    {
        var previousManaged = DockSettings.UseManagedWindows;
        DockSettings.UseManagedWindows = hosting == 2;
        var surface = new Border { Width = 500, Height = 300 };
        var content = new Panel { Children = { surface } };
        if (hosting == 2) content.Children.Add(new ManagedWindowLayer());
        var window = new Window { Width = 800, Height = 600, Content = content };
        var helper = new AdornerHelper<DockTarget>(hosting != 0);
        int fallbackBuilds = 0, customBuilds = 0;
        var fallback = new ControlTheme(typeof(DockTarget));
        fallback.Setters.Add(new Setter(TemplatedControl.TemplateProperty,
            new FuncControlTemplate<DockTarget>((_, _) => { fallbackBuilds++; return new Image(); })));
        var custom = new ControlTheme(typeof(DockTarget));
        custom.Setters.Add(new Setter(TemplatedControl.TemplateProperty,
            new FuncControlTemplate<DockTarget>((_, scope) =>
            {
                customBuilds++;
                var selector = new Border { Name = "PART_CenterSelector", Opacity = 0.4 };
                scope.Register(selector.Name, selector);
                return selector;
            })));
        var app = Application.Current!;
        bool hadFallback = app.Resources.TryGetValue(typeof(DockTarget), out var oldFallback);
        app.Resources[typeof(DockTarget)] = fallback;
        // Explicit TargetTheme must win even over a different scoped type theme.
        window.Resources[typeof(DockTarget)] = themeSource == 0 ? custom : fallback;
        if (themeSource == 1) DockTarget.SetTargetTheme(surface, custom);
        var themeStyle = new Style(x => x.OfType<DockTarget>());
        themeStyle.Setters.Add(new Setter(TemplatedControl.ThemeProperty, custom));
        if (themeSource == 2) app.Styles.Add(themeStyle);
        try
        {
            window.Show(); window.UpdateLayout();
            for (int pass = 0; pass < 3; pass++)
            {
                helper.AddAdorner(surface, false);
                Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
                var target = Assert.IsType<DockTarget>(helper.Adorner);
                target.ApplyTemplate(); target.UpdateLayout();
                Assert.True(customBuilds > 0);
                Assert.Equal(0, fallbackBuilds);
                Assert.Empty(target.GetVisualDescendants().OfType<Image>());
                if (pass == 0) Assert.Equal(0.4, target.GetVisualDescendants().OfType<Border>().Single().Opacity);
                helper.RemoveAdorner(surface);
            }
            app.Styles.Remove(themeStyle);
            // A cached target must not retain its previous surface's theme.
            window.Resources[typeof(DockTarget)] = fallback;
            DockTarget.SetTargetTheme(surface, null);
            helper.AddAdorner(surface, false);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            helper.Adorner!.ApplyTemplate();
            Assert.True(fallbackBuilds > 0);
        }
        finally
        {
            helper.RemoveAdorner(surface); window.Close();
            app.Styles.Remove(themeStyle);
            if (hadFallback) app.Resources[typeof(DockTarget)] = oldFallback!;
            else app.Resources.Remove(typeof(DockTarget));
            DockSettings.UseManagedWindows = previousManaged;
        }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Browser_edge_regions_respect_disabled_split_axes(bool horizontal, bool vertical)
    {
        var target = new DockTarget { ShowHorizontalTargets = horizontal, ShowVerticalTargets = vertical };
        var window = new Window { Width = 500, Height = 300 };
        window.Styles.Add(new BrowserTheme());
        window.Content = target;
        try
        {
            window.Show(); window.UpdateLayout();
            Assert.Empty(target.GetVisualDescendants().OfType<Image>());
            Assert.Equal(new Size(500, 300), target.Bounds.Size);
            var cases = new[]
            {
                (new Point(10, 150), horizontal ? DockOperation.Left : DockOperation.Fill),
                (new Point(490, 150), horizontal ? DockOperation.Right : DockOperation.Fill),
                (new Point(250, 10), vertical ? DockOperation.Top : DockOperation.Fill),
                (new Point(250, 290), vertical ? DockOperation.Bottom : DockOperation.Fill),
                (new Point(250, 150), DockOperation.Fill)
            };
            foreach (var (point, operation) in cases)
                Assert.Equal(operation, target.GetDockOperation(point, target, target, DragAction.Move, (_, _, _, _) => true));
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void Browser_template_contains_only_highlights_and_preserves_drop_operations(int hosting, bool indicatorsOnly)
    {
        var previousManaged = DockSettings.UseManagedWindows;
        DockSettings.UseManagedWindows = hosting == 2;
        var surface = new Border { Width = 500, Height = 300 };
        DockProperties.SetIndicatorDockOperation(surface, DockOperation.Fill);
        var content = new Panel { Children = { surface } };
        if (hosting == 2) content.Children.Add(new ManagedWindowLayer());
        var window = new Window { Width = 800, Height = 600, Content = content };
        window.Styles.Add(new BrowserTheme());
        var helper = new AdornerHelper<DockTarget>(hosting != 0);
        try
        {
            window.Show(); window.UpdateLayout();
            for (int pass = 0; pass < 2; pass++)
            {
                helper.AddAdorner(surface, indicatorsOnly);
                Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
                var target = Assert.IsType<DockTarget>(helper.Adorner);
                target.ApplyTemplate(); target.UpdateLayout();
                // Assert absence, not visibility: the stock icon template must never exist.
                Assert.Empty(target.GetVisualDescendants().OfType<Image>());
                foreach (var operation in new[] { DockOperation.Left, DockOperation.Right, DockOperation.Top, DockOperation.Bottom, DockOperation.Fill })
                {
                    var selector = target.GetSelector(operation)!;
                    Assert.NotNull(selector);
                    var point = indicatorsOnly ? new Point(250, 150)
                        : selector.TranslatePoint(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2), target)!.Value;
                    Assert.Equal(indicatorsOnly ? DockOperation.Fill : operation,
                        target.GetDockOperation(point, surface, hosting == 0 || indicatorsOnly ? surface : target,
                            DragAction.Move, (_, _, _, _) => true));
                }
                target.SetPreviewBounds(new Rect(20, 30, 100, 80));
                window.UpdateLayout(); target.UpdateLayout();
                var preview = target.GetVisualDescendants().OfType<Border>().Single(x => x.Name == "PART_PreviewIndicator");
                Assert.True(preview.IsEffectivelyVisible);
                Assert.Equal(100, preview.Bounds.Width);
                Assert.Equal(80, preview.Bounds.Height);
                helper.RemoveAdorner(surface);
            }
        }
        finally { helper.RemoveAdorner(surface); window.Close(); DockSettings.UseManagedWindows = previousManaged; }
    }
}
