using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
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

    [AvaloniaTheory]
    [InlineData(DockOperation.Fill, "Center")]
    [InlineData(DockOperation.OuterLeft, "OuterLeft")]
    [InlineData(DockOperation.OuterTop, "OuterTop")]
    [InlineData(DockOperation.OuterRight, "OuterRight")]
    [InlineData(DockOperation.OuterBottom, "OuterBottom")]
    public void Restoring_The_Same_Template_Before_Layout_Restores_Selector_Hit_Testing(DockOperation operation, string part)
    {
        DockTarget target = new();
        Control selector = new Border { Width = 50, Height = 50 };
        Control indicator = new Panel { IsHitTestVisible = false };
        DockProperties.SetIndicatorDockOperation(selector, operation);
        DockProperties.SetIndicatorDockOperation(indicator, operation);
        IControlTemplate template = new FuncControlTemplate((_, scope) =>
        {
            scope.Register("PART_" + part + "Selector", selector);
            scope.Register("PART_" + part + "Indicator", indicator);
            return new Panel { Children = { indicator, selector } };
        });
        target.Template = template;
        Window window = new() { Width = 300, Height = 200, Content = target };
        try
        {
            window.Show(); window.UpdateLayout();
            Assert.Same(selector, target.GetSelector(operation));
            target.Template = null;
            Assert.Null(target.GetSelector(operation));
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            target.Template = template;
            window.UpdateLayout();
            Assert.Same(selector, target.GetSelector(operation));
            Point center = selector.TranslatePoint(new Point(25, 25), target)!.Value;
            Assert.Equal(operation, target.GetDockOperation(center, target, target, DragAction.Move, (_, _, _, _) => true));
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selectors_Use_Registered_Operations_And_Retemplating_Discards_Removed_Parts(bool global)
    {
        DockTargetBase target = global ? new TestGlobalDockTarget() : new TestDockTarget();
        Control selector = new Border { Width = 50, Height = 50 };
        Control indicator = new Panel { IsHitTestVisible = false };
        DockProperties.SetIndicatorDockOperation(selector, DockOperation.Right);
        DockProperties.SetIndicatorDockOperation(indicator, DockOperation.Right);
        target.Template = new FuncControlTemplate((_, scope) =>
        {
            scope.Register("PART_LeftSelector", selector);
            scope.Register("PART_LeftIndicator", indicator);
            return new Panel { Children = { indicator, selector } };
        });
        Window window = new() { Width = 300, Height = 200, Content = target };
        try
        {
            window.Show(); window.UpdateLayout();
            Assert.Same(selector, target.GetSelector(DockOperation.Right));
            Assert.Null(target.GetSelector(DockOperation.Left));
            Point center = selector.TranslatePoint(new Point(25, 25), target)!.Value;
            Assert.Equal(DockOperation.Right, target.GetDockOperation(center, target, target, DragAction.Move, (_, _, _, _) => true));
            selector.IsVisible = false;
            Assert.Same(selector, target.GetSelector(DockOperation.Right));
            selector.IsVisible = true;

            target.Template = new FuncControlTemplate((_, _) => new Border());
            window.UpdateLayout();
            Assert.Null(target.GetSelector(DockOperation.Right));
            Assert.Empty(global ? ((TestGlobalDockTarget)target).IndicatorOps : ((TestDockTarget)target).IndicatorOps);
            Assert.Equal(global ? DockOperation.None : DockOperation.Window,
                target.GetDockOperation(center, target, target, DragAction.Move, (_, _, _, _) => true));

            NameScope scope = new();
            scope.Register("custom", selector);
            if (global) ((TestGlobalDockTarget)target).AddSelectorPublic("custom", scope);
            else ((TestDockTarget)target).AddSelectorPublic("custom", scope);
            Assert.Same(selector, target.GetSelector(DockOperation.Right));
            target.Template = null;
            Assert.Null(target.GetSelector(DockOperation.Right));
        }
        finally { window.Close(); }
    }
}
