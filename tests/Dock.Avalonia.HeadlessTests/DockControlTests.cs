using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockControlTests
{
    [AvaloniaFact]
    public void Setting_Layout_Adds_DockControl_To_Factory()
    {
        var factory = new Factory();
        var layout = factory.CreateLayout();
        layout.Factory = factory;

        var control = new DockControl { Factory = factory, Layout = layout };

        Assert.Contains(control, factory.DockControls);
    }

    [AvaloniaFact]
    public void Changing_Layout_Replaces_DockControl_In_Factory()
    {
        var factory = new Factory();
        var layout1 = factory.CreateLayout();
        layout1.Factory = factory;

        var control = new DockControl { Factory = factory, Layout = layout1 };
        Assert.Single(factory.DockControls);

        var layout2 = factory.CreateLayout();
        layout2.Factory = factory;

        control.Layout = layout2;

        Assert.Single(factory.DockControls);
        Assert.Contains(control, factory.DockControls);
    }

    [AvaloniaFact]
    public void Setting_Null_Layout_Removes_DockControl_From_Factory()
    {
        var factory = new Factory();
        var layout = factory.CreateLayout();
        layout.Factory = factory;

        var control = new DockControl { Factory = factory, Layout = layout };
        Assert.Contains(control, factory.DockControls);

        control.Layout = null;

        Assert.DoesNotContain(control, factory.DockControls);
    }
}
