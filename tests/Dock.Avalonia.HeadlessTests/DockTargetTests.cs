using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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

    [AvaloniaFact]
    public void DockTarget_Pointer_Returns_Correct_Operations()
    {
        var target = new TestDockTarget { Width = 200, Height = 200 };
        var drop = new Panel();
        var window = new HostWindow { Content = target };

        var dict = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri("avares://Dock.Avalonia/Controls/DockTarget.axaml"));
        var theme = (ControlTheme)dict[typeof(DockTarget)];
        target.Template = theme.Template;

        window.ApplyTemplate();
        target.ApplyTemplate();
        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(5, target.SelectorOps.Count);

        foreach (var (operation, selector) in target.SelectorOps)
        {
            var center = selector.TranslatePoint(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2), target);
            Assert.NotNull(center);
            var result = target.GetDockOperation(center!.Value, drop, target, DragAction.Move, (_,_,_,_) => true);
            Assert.Equal(operation, result);
        }
    }

    [AvaloniaFact]
    public void GlobalDockTarget_Pointer_Returns_Correct_Operations()
    {
        var target = new TestGlobalDockTarget { Width = 200, Height = 200 };
        var drop = new Panel();
        var window = new HostWindow { Content = target };

        var dict = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri("avares://Dock.Avalonia/Controls/GlobalDockTarget.axaml"));
        var theme = (ControlTheme)dict[typeof(GlobalDockTarget)];
        target.Template = theme.Template;

        window.ApplyTemplate();
        target.ApplyTemplate();
        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(4, target.SelectorOps.Count);

        foreach (var (operation, selector) in target.SelectorOps)
        {
            var center = selector.TranslatePoint(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2), target);
            Assert.NotNull(center);
            var result = target.GetDockOperation(center!.Value, drop, target, DragAction.Move, (_,_,_,_) => true);
            Assert.Equal(operation, result);
        }
    }

    [AvaloniaFact]
    public void DockTarget_Pointer_Interactive_Sequence()
    {
        var target = new TestDockTarget { Width = 200, Height = 200 };
        var drop = new Panel();
        var window = new HostWindow { Content = target };

        var dict = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri("avares://Dock.Avalonia/Controls/DockTarget.axaml"));
        var theme = (ControlTheme)dict[typeof(DockTarget)];
        target.Template = theme.Template;

        window.ApplyTemplate();
        target.ApplyTemplate();
        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        var operations = new List<DockOperation>();

        operations.Add(target.GetDockOperation(new Point(0, 0), drop, target, DragAction.Move, (_,_,_,_) => true));

        var order = new[] { DockOperation.Top, DockOperation.Bottom, DockOperation.Left, DockOperation.Right, DockOperation.Fill };
        foreach (var op in order)
        {
            var selector = target.SelectorOps[op];
            var center = selector.TranslatePoint(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2), target);
            Assert.NotNull(center);
            operations.Add(target.GetDockOperation(center!.Value, drop, target, DragAction.Move, (_,_,_,_) => true));
        }

        var expected = new List<DockOperation> { DockOperation.Window };
        expected.AddRange(order);

        Assert.Equal(expected, operations);
    }

    [AvaloniaFact]
    public void GlobalDockTarget_Pointer_Interactive_Sequence()
    {
        var target = new TestGlobalDockTarget { Width = 200, Height = 200 };
        var drop = new Panel();
        var window = new HostWindow { Content = target };

        var dict = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri("avares://Dock.Avalonia/Controls/GlobalDockTarget.axaml"));
        var theme = (ControlTheme)dict[typeof(GlobalDockTarget)];
        target.Template = theme.Template;

        window.ApplyTemplate();
        target.ApplyTemplate();
        target.Measure(Size.Infinity);
        target.Arrange(new Rect(target.DesiredSize));

        var operations = new List<DockOperation>();

        operations.Add(target.GetDockOperation(new Point(0, 0), drop, target, DragAction.Move, (_,_,_,_) => true));

        var order = new[] { DockOperation.Top, DockOperation.Bottom, DockOperation.Left, DockOperation.Right };
        foreach (var op in order)
        {
            var selector = target.SelectorOps[op];
            var center = selector.TranslatePoint(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2), target);
            Assert.NotNull(center);
            operations.Add(target.GetDockOperation(center!.Value, drop, target, DragAction.Move, (_,_,_,_) => true));
        }

        var expected = new List<DockOperation> { DockOperation.None };
        expected.AddRange(order);

        Assert.Equal(expected, operations);
    }
}
