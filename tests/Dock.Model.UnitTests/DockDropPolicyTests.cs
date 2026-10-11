using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockDropPolicyTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Either_Factory_Can_Veto_Visibility_Validation_And_Execution(bool sourcePolicy, bool execute)
    {
        Factory ordinary = new();
        PolicyFactory policy = new() { Allowed = false };
        Tool source = new() { Id = "Source" };
        Tool other = new() { Id = "Other" };
        ToolDock sourceDock = new() { VisibleDockables = ordinary.CreateList<IDockable>(source) };
        ToolDock targetDock = new() { VisibleDockables = ordinary.CreateList<IDockable>(other) };
        (sourcePolicy ? policy : ordinary).InitLayout(sourceDock);
        (sourcePolicy ? ordinary : policy).InitLayout(targetDock);
        DockManager manager = new(new DockService());
        Assert.False(manager.IsDockTargetVisible(source, targetDock, DockOperation.Fill));
        Assert.False(manager.ValidateDockable(source, targetDock, DragAction.Move, DockOperation.Fill, execute));
        Assert.Same(sourceDock, source.Owner);
        Assert.Single(sourceDock.VisibleDockables!);
        Assert.Single(targetDock.VisibleDockables!);
        Assert.True(policy.Calls > 0);
        Assert.Same(source, policy.Source);
        Assert.Same(targetDock, policy.Target);
        Assert.Equal(DockOperation.Fill, policy.Operation);
        policy.Allowed = true;
        Assert.True(manager.ValidateDockable(source, targetDock, DragAction.Move, DockOperation.Fill, false));
    }

    [Fact]
    public void One_Shared_Policy_Is_Consulted_Once_When_Checking_Target_Visibility()
    {
        PolicyFactory factory = new() { Allowed = true };
        Tool source = new() { Factory = factory };
        ToolDock target = new() { Factory = factory };
        DockManager manager = new(new DockService());
        Assert.True(manager.IsDockTargetVisible(source, target, DockOperation.Left));
        Assert.Equal(1, factory.Calls);
    }

    private sealed class PolicyFactory : Factory, IDockDropPolicy
    {
        public bool Allowed { get; set; }
        public int Calls { get; private set; }
        public IDockable? Source { get; private set; }
        public IDockable? Target { get; private set; }
        public DockOperation Operation { get; private set; }
        public bool CanDrop(IDockable source, IDockable target, DockOperation operation)
        { Calls++; Source = source; Target = target; Operation = operation; return Allowed; }
    }
}
