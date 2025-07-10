using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockTargetTests
{
    private class TestDockTarget : DockTarget
    {
        public Dictionary<DockOperation, Control> IndicatorOps => IndicatorOperations;
        public Dictionary<DockOperation, Control> SelectorOps => SelectorsOperations;
        public void AddIndicatorPublic(string name, INameScope scope) => AddIndicator(name, scope);
        public void AddSelectorPublic(string name, INameScope scope) => AddSelector(name, scope);
        public bool IsSelector(Control selector) => IsDockTargetSelector(selector);
    }

    private class TestGlobalDockTarget : GlobalDockTarget
    {
        public Dictionary<DockOperation, Control> IndicatorOps => IndicatorOperations;
        public Dictionary<DockOperation, Control> SelectorOps => SelectorsOperations;
        public void AddIndicatorPublic(string name, INameScope scope) => AddIndicator(name, scope);
        public void AddSelectorPublic(string name, INameScope scope) => AddSelector(name, scope);
    }

    [AvaloniaFact]
    public void DockTarget_DefaultDockOperation_Window()
    {
        var target = new DockTarget();
        var result = target.GetDockOperation(new Point(), new Panel(), target, DragAction.Move, (_,_,_,_) => true);
        Assert.Equal(DockOperation.Window, result);
    }

    [AvaloniaFact]
    public void GlobalDockTarget_DefaultDockOperation_None()
    {
        var target = new GlobalDockTarget();
        var result = target.GetDockOperation(new Point(), new Panel(), target, DragAction.Move, (_,_,_,_) => true);
        Assert.Equal(DockOperation.None, result);
    }

    [AvaloniaFact]
    public void AddIndicator_Adds_Mapping()
    {
        var target = new TestDockTarget();
        var scope = new NameScope();
        var panel = new Panel();
        DockProperties.SetIndicatorDockOperation(panel, DockOperation.Left);
        scope.Register("test", panel);

        target.AddIndicatorPublic("test", scope);

        Assert.Single(target.IndicatorOps);
        Assert.Same(panel, target.IndicatorOps[DockOperation.Left]);
    }

    [AvaloniaFact]
    public void AddSelector_Adds_Mapping()
    {
        var target = new TestDockTarget();
        var scope = new NameScope();
        var ctrl = new Panel();
        DockProperties.SetIndicatorDockOperation(ctrl, DockOperation.Top);
        scope.Register("sel", ctrl);

        target.AddSelectorPublic("sel", scope);

        Assert.Single(target.SelectorOps);
        Assert.Same(ctrl, target.SelectorOps[DockOperation.Top]);
        Assert.True(target.IsSelector(ctrl));
    }

    [AvaloniaFact]
    public void MissingIndicator_IsIgnored()
    {
        var target = new TestGlobalDockTarget();
        var scope = new NameScope();

        target.AddIndicatorPublic("none", scope);

        Assert.Empty(target.IndicatorOps);
    }

    [AvaloniaFact]
    public void MissingSelector_IsIgnored()
    {
        var target = new TestDockTarget();
        var scope = new NameScope();

        target.AddSelectorPublic("none", scope);

        Assert.Empty(target.SelectorOps);
    }
}
