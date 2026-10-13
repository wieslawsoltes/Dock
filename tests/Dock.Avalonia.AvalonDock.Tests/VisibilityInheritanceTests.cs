using System;
using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class VisibilityInheritanceTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Concrete_Visibility_Hooks_Recompute_The_Parent_Before_Their_Base_Call(bool tools)
    {
        IProbe probe = Create(tools);
        ParentPanel parent = new();
        ((LayoutElement)probe).Parent = parent;
        List<string> calls = new();
        probe.Before = () => calls.Add("before");
        probe.After = () => calls.Add("after");
        parent.Compute = () => calls.Add("parent");
        probe.Notify();
        Assert.Equal(new[] { "before", "parent", "parent", "after" }, calls);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void The_Base_Visibility_Pass_Uses_The_Parent_Left_By_The_First_Pass(bool tools)
    {
        IProbe probe = Create(tools);
        LayoutElement element = (LayoutElement)probe;
        ParentPanel first = new();
        ParentPanel second = new();
        element.Parent = first;
        List<string> calls = new();
        first.Compute = () => { calls.Add("old"); element.Parent = second; };
        second.Compute = () => calls.Add("new");
        probe.Notify();
        Assert.Equal(new[] { "old", "new" }, calls);
        Assert.Same(second, element.Parent);
        Assert.Same(second.DockModel, element.DockModel.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Throwing_First_Visibility_Pass_Stops_The_Base_Pass_And_The_Derived_Continuation(bool tools)
    {
        IProbe probe = Create(tools);
        ParentPanel parent = new();
        ((LayoutElement)probe).Parent = parent;
        ApplicationException failure = new("parent observer");
        int calls = 0;
        parent.Compute = () => { calls++; throw failure; };
        probe.After = () => throw new Exception("The hook continued after failure.");
        Assert.Same(failure, Assert.Throws<ApplicationException>(probe.Notify));
        Assert.Equal(1, calls);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Derived_Override_Can_Suppress_Both_Parent_Visibility_Passes(bool tools)
    {
        IProbe probe = Create(tools);
        ParentPanel parent = new();
        ((LayoutElement)probe).Parent = parent;
        probe.Suppress = true;
        parent.Compute = () => throw new Exception("The suppressed base hook ran.");
        int calls = 0;
        probe.After = () => calls++;
        probe.Notify();
        Assert.Equal(1, calls);
    }

    private static IProbe Create(bool tools) => tools ? new ToolGroup() : new DocumentPane();
    private interface IProbe
    {
        Action? Before { get; set; }
        Action? After { get; set; }
        bool Suppress { get; set; }
        void Notify();
    }
    private sealed class ParentPanel : LayoutPanel
    {
        internal Action? Compute { get; set; }
        protected override bool GetVisibility() { Compute?.Invoke(); return base.GetVisibility(); }
    }
    private sealed class DocumentPane : LayoutDocumentPane, IProbe
    {
        public Action? Before { get; set; }
        public Action? After { get; set; }
        public bool Suppress { get; set; }
        public void Notify() => OnIsVisibleChanged();
        protected override void OnIsVisibleChanged() { Before?.Invoke(); if (!Suppress) base.OnIsVisibleChanged(); After?.Invoke(); }
    }
    private sealed class ToolGroup : LayoutAnchorablePaneGroup, IProbe
    {
        public Action? Before { get; set; }
        public Action? After { get; set; }
        public bool Suppress { get; set; }
        public void Notify() => OnIsVisibleChanged();
        protected override void OnIsVisibleChanged() { Before?.Invoke(); if (!Suppress) base.OnIsVisibleChanged(); After?.Invoke(); }
    }
}
