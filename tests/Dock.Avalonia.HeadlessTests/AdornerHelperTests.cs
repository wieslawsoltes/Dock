using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class AdornerHelperTests
{
    [AvaloniaFact]
    public void AddRemove_Reuses_Same_Instance()
    {
        var root = new AdornerLayer();
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
}
