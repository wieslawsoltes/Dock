using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ManagerPropertyTests
{
    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Conditional_Source_Styles_Attach_And_Release_Collections(bool documents)
    {
        DockingManager manager = new();
        AvaloniaProperty property = documents ? DockingManager.DocumentsSourceProperty : DockingManager.AnchorablesSourceProperty;
        ObservableCollection<object> source = new() { new object() };
        manager.Styles.Add(new Style(selector => selector.OfType<DockingManager>().Class("sources"))
        { Setters = { new Setter(property, source) } });
        manager.ApplyStyling();
        Assert.Null(manager.GetValue(property));
        manager.Classes.Add("sources");
        Assert.Same(source, manager.GetValue(property));
        LayoutContent first = Assert.Single(Contents(manager, documents));
        source.Add(new object());
        Assert.Equal(source, Contents(manager, documents).Select(content => content.Content));
        manager.Classes.Remove("sources");
        Assert.Null(manager.GetValue(property));
        Assert.Empty(Contents(manager, documents));
        Assert.Null(first.Parent);
        source.Add(new object());
        Assert.Empty(Contents(manager, documents));
        manager.Classes.Add("sources");
        Assert.Equal(source, Contents(manager, documents).Select(content => content.Content));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Clearing_A_Local_Source_Reveals_The_Style_And_Replaces_Subscriptions(bool documents)
    {
        DockingManager manager = new();
        AvaloniaProperty property = documents ? DockingManager.DocumentsSourceProperty : DockingManager.AnchorablesSourceProperty;
        ObservableCollection<object> styled = new() { new object() };
        ObservableCollection<object> local = new() { new object() };
        manager.Styles.Add(new Style { Setters = { new Setter(property, styled) } });
        manager.ApplyStyling();
        manager.SetValue(property, local);
        Assert.Equal(local, Contents(manager, documents).Select(content => content.Content));
        manager.ClearValue(property);
        Assert.Same(styled, manager.GetValue(property));
        local.Add(new object());
        styled.Add(new object());
        Assert.Equal(styled, Contents(manager, documents).Select(content => content.Content));
    }

    [AvaloniaFact]
    public void Styled_Strategies_Follow_Priority_And_Control_Live_Insertion()
    {
        DockingManager manager = new();
        ObservableCollection<object> source = new();
        ManagerPropertyStrategy styled = new("styled");
        ManagerPropertyStrategy local = new("local");
        manager.Styles.Add(new Style(selector => selector.OfType<DockingManager>().Class("strategy"))
        { Setters = { new Setter(DockingManager.LayoutUpdateStrategyProperty, styled) } });
        manager.ApplyStyling();
        manager.DocumentsSource = source;
        manager.Classes.Add("strategy");
        source.Add(new object());
        Assert.Equal("styled", Contents(manager, true).Last().Title);
        manager.LayoutUpdateStrategy = local;
        source.Add(new object());
        Assert.Equal("local", Contents(manager, true).Last().Title);
        manager.ClearValue(DockingManager.LayoutUpdateStrategyProperty);
        Assert.Same(styled, manager.LayoutUpdateStrategy);
        source.Add(new object());
        Assert.Equal("styled", Contents(manager, true).Last().Title);
        manager.Classes.Remove("strategy");
        Assert.Null(manager.LayoutUpdateStrategy);
    }

    [AvaloniaFact]
    public void The_Default_Layout_Is_Per_Manager_And_Contains_A_Document_Pane_Group()
    {
        DockingManager first = new();
        DockingManager second = new();
        Assert.NotSame(first.Layout, second.Layout);
        AssertDefault(first);
        AssertDefault(second);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Null_Layout_Assignment_Creates_A_Fresh_Default_Without_LayoutChanging(bool propertyApi)
    {
        DockingManager manager = new();
        int changing = 0, changed = 0;
        manager.LayoutChanging += (_, _) => changing++;
        manager.LayoutChanged += (_, _) => changed++;
        for (int i = 0; i < 2; i++)
        {
            LayoutRoot previous = manager.Layout;
            if (propertyApi) manager.SetValue(DockingManager.LayoutProperty, null!);
            else manager.Layout = null;
            Assert.NotSame(previous, manager.Layout);
            Assert.Null(previous.Manager);
            AssertDefault(manager);
        }
        Assert.Equal(0, changing);
        Assert.Equal(2, changed);
    }

    [AvaloniaFact]
    public void Layout_Hook_Encloses_Ownership_Transfer_And_The_Changed_Event()
    {
        TraceManager manager = new();
        manager.Trace.Clear();
        LayoutRoot previous = manager.Layout;
        LayoutRoot next = new();
        manager.LayoutChanging += (_, _) =>
        {
            Assert.Same(previous, manager.Layout);
            Assert.Same(manager, previous.Manager);
            manager.Trace.Add("changing");
        };
        manager.Before = (oldLayout, newLayout) =>
        {
            Assert.Same(previous, oldLayout);
            Assert.Same(next, newLayout);
            Assert.Same(next, manager.Layout);
            Assert.Same(manager, previous.Manager);
            Assert.Null(next.Manager);
        };
        manager.LayoutChanged += (_, _) =>
        {
            Assert.Null(previous.Manager);
            Assert.Same(manager, next.Manager);
            manager.Trace.Add("changed");
        };
        manager.Layout = next;
        Assert.Equal(new[] { "changing", "hook-before", "changed", "hook-after" }, manager.Trace);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Suppressing_Or_Throwing_Before_Base_Leaves_Layout_Side_Effects_Untouched(bool fail)
    {
        TraceManager manager = new() { SkipBase = true, Fail = fail };
        LayoutRoot previous = manager.Layout;
        LayoutRoot next = new();
        int changed = 0;
        manager.LayoutChanged += (_, _) => changed++;
        if (fail) Assert.Throws<InvalidOperationException>(() => manager.Layout = next);
        else manager.Layout = next;
        Assert.Same(next, manager.Layout);
        Assert.Same(manager, previous.Manager);
        Assert.Null(next.Manager);
        Assert.Equal(0, changed);
    }

    [AvaloniaFact]
    public void Constructor_Invokes_The_Layout_Hook_With_A_Null_Previous_Value()
    {
        InitialTraceManager manager = new();
        Assert.Equal(1, manager.Calls);
        Assert.Null(manager.OldLayout);
        Assert.Same(manager.Layout, manager.NewLayout);
        Assert.Null(manager.OwnerBeforeBase);
        Assert.Same(manager, manager.OwnerAfterBase);
    }

    [AvaloniaFact]
    public void Reassigning_The_Same_Layout_Calls_Changing_Without_Reattaching_Contents()
    {
        DockingManager manager = new() { DocumentsSource = new[] { new object() } };
        LayoutContent document = Assert.Single(Contents(manager, true));
        object? item = manager.GetLayoutItemFromModel(document);
        int changing = 0, changed = 0;
        manager.LayoutChanging += (_, _) => changing++;
        manager.LayoutChanged += (_, _) => changed++;
        manager.Layout = manager.Layout;
        Assert.Equal(1, changing);
        Assert.Equal(0, changed);
        Assert.Same(item, manager.GetLayoutItemFromModel(document));
    }

    [AvaloniaFact]
    public void A_Failed_Changing_Callback_Does_Not_Transfer_Layout_Ownership()
    {
        DockingManager manager = new();
        LayoutRoot previous = manager.Layout;
        LayoutRoot next = new();
        manager.LayoutChanging += (_, _) => throw new InvalidOperationException("Changing failed.");
        Assert.Throws<InvalidOperationException>(() => manager.Layout = next);
        Assert.Same(previous, manager.Layout);
        Assert.Same(manager, previous.Manager);
        Assert.Null(next.Manager);
    }

    [AvaloniaFact]
    public void Layout_Styles_Respect_The_Constructor_Local_Value_Then_Transfer_Source_Ownership()
    {
        DockingManager manager = new();
        LayoutRoot initial = manager.Layout;
        LayoutRoot styled = new();
        LayoutRoot local = new();
        ObservableCollection<object> source = new() { new object() };
        manager.DocumentsSource = source;
        manager.Styles.Add(new Style(selector => selector.OfType<DockingManager>().Class("layout"))
        { Setters = { new Setter(DockingManager.LayoutProperty, styled) } });
        manager.Classes.Add("layout");
        manager.ApplyStyling();
        Assert.Same(initial, manager.Layout);
        manager.ClearValue(DockingManager.LayoutProperty);
        Assert.Same(styled, manager.Layout);
        Assert.Null(initial.Manager);
        Assert.Empty(initial.Descendents().OfType<LayoutDocument>());
        Assert.Equal(source, Contents(manager, true).Select(content => content.Content));
        LayoutContent retired = Assert.Single(Contents(manager, true));
        manager.Layout = local;
        Assert.Null(manager.GetLayoutItemFromModel(retired));
        manager.ClearValue(DockingManager.LayoutProperty);
        Assert.Same(styled, manager.Layout);
        manager.Classes.Remove("layout");
        Assert.NotSame(styled, manager.Layout);
        Assert.Null(styled.Manager);
        source.Add(new object());
        Assert.Equal(source, Contents(manager, true).Select(content => content.Content));
        Assert.Same(manager, manager.Layout.Manager);
    }

    [AvaloniaFact]
    public void Compiled_Style_Bindings_Replace_Layouts_Sources_And_Strategies()
    {
        ManagerPropertiesViewModel first = new();
        ManagerPropertiesViewModel second = new();
        ManagerPropertiesFixture manager = new() { DataContext = first };
        manager.ClearValue(DockingManager.LayoutProperty);
        manager.ApplyStyling();
        manager.Classes.Add("workspace");
        Assert.Same(first.Root, manager.Layout);
        Assert.Same(first.Strategy, manager.LayoutUpdateStrategy);
        Assert.Equal(first.Documents, Contents(manager, true).Select(content => content.Content));
        Assert.Equal(first.Tools, Contents(manager, false).Select(content => content.Content));
        manager.DataContext = second;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(second.Root, manager.Layout);
        Assert.Same(second.Strategy, manager.LayoutUpdateStrategy);
        first.Documents.Add(new object());
        first.Tools.Add(new object());
        second.Documents.Add(new object());
        second.Tools.Add(new object());
        Assert.Equal(second.Documents, Contents(manager, true).Select(content => content.Content));
        Assert.Equal(second.Tools, Contents(manager, false).Select(content => content.Content));
        manager.Classes.Remove("workspace");
        Assert.Null(manager.DocumentsSource);
        Assert.Null(manager.AnchorablesSource);
        Assert.Null(manager.LayoutUpdateStrategy);
        Assert.NotSame(second.Root, manager.Layout);
    }

    private static IEnumerable<LayoutContent> Contents(DockingManager manager, bool documents) =>
        manager.Layout.Descendents().OfType<LayoutContent>().Where(content => documents ? content is LayoutDocument : content is LayoutAnchorable);

    private static void AssertDefault(DockingManager manager)
    {
        LayoutDocumentPaneGroup group = Assert.IsType<LayoutDocumentPaneGroup>(Assert.Single(manager.Layout.RootPanel.Children));
        Assert.Empty(Assert.IsType<LayoutDocumentPane>(Assert.Single(group.Children)).Children);
        Assert.Same(manager, manager.Layout.Manager);
        Assert.Same(manager.Layout, manager.GetValue(DockingManager.LayoutProperty));
    }

    private sealed class TraceManager : DockingManager
    {
        public List<string> Trace { get; } = new();
        public Action<LayoutRoot, LayoutRoot>? Before { get; set; }
        public bool SkipBase { get; set; }
        public bool Fail { get; set; }
        protected override void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
        {
            Trace.Add("hook-before");
            Before?.Invoke(oldLayout, newLayout);
            if (Fail) throw new InvalidOperationException("Layout callback failure.");
            if (!SkipBase) base.OnLayoutChanged(oldLayout, newLayout);
            Trace.Add("hook-after");
        }
    }

    private sealed class InitialTraceManager : DockingManager
    {
        public int Calls { get; private set; }
        public LayoutRoot? OldLayout { get; private set; }
        public LayoutRoot? NewLayout { get; private set; }
        public DockingManager? OwnerBeforeBase { get; private set; }
        public DockingManager? OwnerAfterBase { get; private set; }
        protected override void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
        {
            Calls++;
            OldLayout = oldLayout;
            NewLayout = newLayout;
            OwnerBeforeBase = newLayout.Manager;
            base.OnLayoutChanged(oldLayout, newLayout);
            OwnerAfterBase = newLayout.Manager;
        }
    }
}
