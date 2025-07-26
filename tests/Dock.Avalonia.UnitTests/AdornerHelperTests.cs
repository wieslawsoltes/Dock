using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Xunit;

namespace Dock.Avalonia.UnitTests;

public class AdornerHelperTests
{
    [AvaloniaFact]
    public void AddAdorner_Reuses_Instance()
    {
        var helper = new AdornerHelper<DockTarget>(true);
        var v1 = new Border();
        var v2 = new Border();

        helper.AddAdorner(v1, false);
        var a1 = helper.Adorner;
        helper.AddAdorner(v2, false);
        var a2 = helper.Adorner;

        Assert.Same(a1, a2);
    }

    [AvaloniaFact]
    public void RemoveAdorner_And_Add_Reuses_Instance()
    {
        var helper = new AdornerHelper<DockTarget>(true);
        var visual = new Border();

        helper.AddAdorner(visual, false);
        var adorner1 = helper.Adorner;
        helper.RemoveAdorner(visual);
        helper.AddAdorner(visual, false);
        var adorner2 = helper.Adorner;

        Assert.Same(adorner1, adorner2);
    }

    [AvaloniaFact]
    public void GlobalDockTarget_AddAdorner_Multiple_Times_Does_Not_Throw()
    {
        // This test reproduces the exact scenario from the issue:
        // Using GlobalDockTarget with floating adorner should not throw 
        // "already has a visual parent" exception when called multiple times
        var helper = new AdornerHelper<GlobalDockTarget>(true);
        var visual1 = new Border();
        var visual2 = new Border();

        // This should not throw an exception
        helper.AddAdorner(visual1, false);
        helper.AddAdorner(visual2, false); // This would previously throw InvalidOperationException

        // Verify the adorner is still functional
        Assert.NotNull(helper.Adorner);
        Assert.IsType<GlobalDockTarget>(helper.Adorner);
    }
}
