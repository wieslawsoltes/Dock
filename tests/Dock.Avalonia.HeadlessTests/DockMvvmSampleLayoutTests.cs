using Avalonia.Headless.XUnit;
using Dock.Model.Controls;
using Dock.Model.Core;
using DockMvvmSample.ViewModels;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockMvvmSampleLayoutTests
{
    [AvaloniaFact]
    public void CreateLayout_PrimaryToolDocksRemainFullyInteractive()
    {
        var factory = new DockFactory(new object());
        var root = factory.CreateLayout();
        factory.InitLayout(root);

        var tool3 = Assert.Single(factory.Find(root, dockable => dockable.Id == "Tool3"));
        var tool4 = Assert.Single(factory.Find(root, dockable => dockable.Id == "Tool4"));
        var tool5 = Assert.Single(factory.Find(root, dockable => dockable.Id == "Tool5"));
        var tool6 = Assert.Single(factory.Find(root, dockable => dockable.Id == "Tool6"));

        Assert.True(tool3.CanDrag);
        Assert.True(tool4.CanDrag);

        var lowerLeftDock = Assert.IsAssignableFrom<IToolDock>(tool3.Owner);
        Assert.Same(lowerLeftDock, tool4.Owner);
        Assert.True(lowerLeftDock.CanDrag);
        Assert.True(lowerLeftDock.CanDrop);

        var upperRightDock = Assert.IsAssignableFrom<IToolDock>(tool5.Owner);
        Assert.Same(upperRightDock, tool6.Owner);
        Assert.Equal(GripMode.Visible, upperRightDock.GripMode);
    }
}
