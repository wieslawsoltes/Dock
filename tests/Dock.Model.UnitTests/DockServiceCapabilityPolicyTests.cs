using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockServiceCapabilityPolicyTests
{
    private static (Factory Factory, IRootDock Root, IDock SourceDock, IDockable Source, IDockable Target) CreateScenario()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Id = "Root";
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var sourceDock = factory.CreateToolDock();
        sourceDock.Id = "SourceDock";
        sourceDock.VisibleDockables = factory.CreateList<IDockable>();

        var targetDock = factory.CreateToolDock();
        targetDock.Id = "TargetDock";
        targetDock.VisibleDockables = factory.CreateList<IDockable>();

        var source = factory.CreateTool();
        source.Id = "SourceTool";

        var target = factory.CreateTool();
        target.Id = "TargetTool";

        factory.AddDockable(root, sourceDock);
        factory.AddDockable(root, targetDock);
        factory.AddDockable(sourceDock, source);
        factory.AddDockable(targetDock, target);

        sourceDock.ActiveDockable = source;
        targetDock.ActiveDockable = target;
        root.ActiveDockable = sourceDock;

        return (factory, root, sourceDock, source, target);
    }

    [Fact]
    public void DockDockableIntoWindow_ReturnsFalse_When_Float_Is_Disabled_By_Policy()
    {
        var (_, root, _, source, target) = CreateScenario();
        var service = new DockService();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanFloat = false
        };

        var result = service.DockDockableIntoWindow(source, target, new DockPoint(100, 100), bExecute: false);

        Assert.False(result);
    }

    [Fact]
    public void DockDockableIntoWindow_ReturnsTrue_When_Float_Is_Allowed()
    {
        var (_, _, _, source, target) = CreateScenario();
        var service = new DockService();

        var result = service.DockDockableIntoWindow(source, target, new DockPoint(100, 100), bExecute: false);

        Assert.True(result);
    }
}
