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
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockTargetInitialPreviewTests
{
    [AvaloniaFact]
    public void Newly_applied_template_keeps_targets_hidden_until_input_validation()
    {
        var target = new DockTarget();
        var window = new Window { Width = 500, Height = 300, Content = target };
        try
        {
            window.Show(); window.UpdateLayout();
            var selectors = target.GetVisualDescendants().OfType<Control>()
                .Where(x => x.Name?.EndsWith("Selector") == true).ToArray();
            Assert.NotEmpty(selectors);
            Assert.All(selectors, x => Assert.Equal(0, x.Opacity));
            target.GetDockOperation(new Point(10, 10), target, target, DragAction.Move,
                (_, _, _, _) => true, (_, operation, _, _) => operation == DockOperation.Fill);
            Assert.Equal(1, target.GetSelector(DockOperation.Fill)!.Opacity);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void First_render_and_retemplate_filter_selectors_before_the_next_pointer_move(int hosting, bool allowFill)
    {
        var previousManaged = DockSettings.UseManagedWindows;
        DockSettings.UseManagedWindows = hosting == 2;
        var surface = new Border { Width = 500, Height = 300 };
        var content = new Panel { Children = { surface } };
        if (hosting == 2) content.Children.Add(new ManagedWindowLayer());
        var window = new Window { Width = 800, Height = 600, Content = content };
        var helper = new AdornerHelper<DockTarget>(hosting != 0);
        void Settle() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
        try
        {
            window.Show(); Settle();
            // Exercise cold creation, reuse, then replacing the template on the
            // cached adorner. A render can happen before another pointer move.
            for (var pass = 0; pass < 3; pass++)
            {
                if (pass == 2)
                {
                    var stock = Assert.IsType<ControlTheme>(surface.FindResource(typeof(DockTarget)));
                    var custom = new ControlTheme(typeof(DockTarget)) { BasedOn = stock };
                    custom.Setters.Add(new Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate<DockTarget>((_, scope) =>
                    {
                        var panel = new Panel();
                        foreach (var name in new[] { "PART_LeftSelector", "PART_LeftIndicator", "PART_CenterSelector", "PART_CenterIndicator" })
                        {
                            var part = new Border { Name = name, Width = 40, Height = 40 };
                            DockProperties.SetIndicatorDockOperation(part, name.Contains("Left") ? DockOperation.Left : DockOperation.Fill);
                            scope.Register(name, part); panel.Children.Add(part);
                        }
                        return panel;
                    })));
                    DockTarget.SetTargetTheme(surface, custom);
                }
                helper.AddAdorner(surface, false);
                var target = Assert.IsType<DockTarget>(helper.Adorner);
                target.GetDockOperation(new Point(10, 10), surface, surface, DragAction.Move,
                    (_, _, _, _) => true, (_, operation, _, _) => allowFill && operation == DockOperation.Fill);
                Settle();
                var selectors = target.GetVisualDescendants().OfType<Control>()
                    .Where(x => x.Name?.EndsWith("Selector") == true).ToArray();
                Assert.NotEmpty(selectors);
                Assert.All(selectors, selector => Assert.Equal(allowFill && selector.Name == "PART_CenterSelector" ? 1 : 0, selector.Opacity));
                target.GetDockOperation(new Point(10, 10), surface, surface, DragAction.Move,
                    (_, _, _, _) => true, (_, operation, _, _) => operation == DockOperation.Fill);
                Settle();
                Assert.Equal(1, target.GetSelector(DockOperation.Fill)!.Opacity);
                Assert.Equal(0, target.GetSelector(DockOperation.Left)!.Opacity);
                helper.RemoveAdorner(surface);
            }
        }
        finally { helper.RemoveAdorner(surface); window.Close(); DockSettings.UseManagedWindows = previousManaged; }
    }
}
