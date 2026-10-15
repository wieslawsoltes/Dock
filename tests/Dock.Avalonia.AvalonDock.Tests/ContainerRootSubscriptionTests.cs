using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContainerRootSubscriptionTests
{
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void Each_Successful_Style_Base_Call_Adds_An_Independent_Root_Registration(int replays)
    {
        ProbeManager manager = new() { Layout = Root("current") };
        Selector selector = new();
        manager.LayoutItemContainerStyleSelector = selector;
        for (int index = 0; index < replays; index++) manager.ApplyStyle();
        selector.Calls.Clear();
        int notifications = AddEmptyPane(manager.Layout);
        Assert.Equal(notifications * (2 + replays), selector.Calls.Count);
        Assert.All(selector.Calls, item => Assert.Equal("current", item.Model));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Suppressing_Or_Throwing_Before_Base_Does_Not_Add_A_Registration(bool fail)
    {
        ProbeManager manager = new() { Layout = Root("current"), SuppressStyle = !fail, FailStyle = fail };
        Selector selector = new();
        if (fail) Assert.Throws<InvalidOperationException>(() => manager.LayoutItemContainerStyleSelector = selector);
        else manager.LayoutItemContainerStyleSelector = selector;
        manager.SuppressStyle = manager.FailStyle = false;

        int notifications = AddEmptyPane(manager.Layout);
        Assert.Equal(notifications, selector.Calls.Count);
        manager.ApplyStyle();
        selector.Calls.Clear();
        notifications = AddEmptyPane(manager.Layout);
        Assert.Equal(notifications * 2, selector.Calls.Count);
    }

    [AvaloniaFact]
    public void A_Selector_Exception_Does_Not_Commit_The_Pending_Registration()
    {
        ProbeManager manager = new() { Layout = Root("current") };
        Selector selector = new() { Selecting = _ => throw new InvalidOperationException("Style failure") };
        Assert.Throws<InvalidOperationException>(() => manager.LayoutItemContainerStyleSelector = selector);
        selector.Selecting = null;
        selector.Calls.Clear();

        int notifications = AddEmptyPane(manager.Layout);
        Assert.Equal(notifications, selector.Calls.Count);
        manager.ApplyStyle();
        selector.Calls.Clear();
        notifications = AddEmptyPane(manager.Layout);
        Assert.Equal(notifications * 2, selector.Calls.Count);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_Retained_Old_Root_Refreshes_Current_Containers_Without_Importing_Its_New_Content(bool documents)
    {
        LayoutRoot oldRoot = Root("old", documents);
        ProbeManager manager = new() { Layout = oldRoot };
        Selector selector = new();
        manager.LayoutItemContainerStyleSelector = selector;
        manager.ApplyStyle();
        manager.Layout = Root("current", documents);
        selector.Calls.Clear();
        int notifications = 0;
        oldRoot.ElementAdded += (_, _) => notifications++;
        LayoutContent foreign = documents ? new LayoutDocument { Content = "foreign" } : new LayoutAnchorable { Content = "foreign" };
        ILayoutGroup pane = (ILayoutGroup)oldRoot.Descendents().OfType<LayoutContent>().Single().Parent!;

        pane.InsertChildAt(1, foreign);
        Assert.True(notifications > 0);
        Assert.Equal(notifications * 3, selector.Calls.Count);
        Assert.All(selector.Calls, item => Assert.Equal("current", item.Model));
        Assert.Null(manager.GetLayoutItemFromModel(foreign));
        Assert.Null(oldRoot.Manager);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(manager.GetLayoutItemFromModel(manager.Layout.Descendents().OfType<LayoutContent>().Single()));
    }

    [AvaloniaFact]
    public void Returning_To_A_Root_Removes_One_Registration_Before_Attaching_Again()
    {
        LayoutRoot first = Root("first");
        LayoutRoot second = Root("second");
        ProbeManager manager = new() { Layout = first };
        Selector selector = new();
        manager.LayoutItemContainerStyleSelector = selector;
        manager.ApplyStyle();
        manager.Layout = second;
        manager.Layout = first;
        selector.Calls.Clear();

        int notifications = AddEmptyPane(first);
        Assert.Equal(notifications * 3, selector.Calls.Count);
        selector.Calls.Clear();
        notifications = AddEmptyPane(second);
        Assert.Equal(notifications, selector.Calls.Count);
        Assert.All(selector.Calls, item => Assert.Equal("first", item.Model));
    }

    [AvaloniaFact]
    public void New_Registrations_Do_Not_Join_A_Notification_Already_In_Flight()
    {
        ProbeManager manager = new() { Layout = Root("current") };
        List<string> calls = new();
        Selector second = new() { Selecting = _ => { calls.Add("second"); return null; } };
        bool replace = false;
        Selector first = new()
        {
            Selecting = _ =>
            {
                calls.Add("first");
                if (replace)
                {
                    replace = false;
                    manager.LayoutItemContainerStyleSelector = second;
                }
                return null;
            }
        };
        manager.LayoutItemContainerStyleSelector = first;
        calls.Clear();
        replace = true;

        Assert.Equal(1, AddEmptyPane(manager.Layout));
        Assert.Equal(new[] { "first", "second", "second" }, calls);
        calls.Clear();
        Assert.Equal(1, AddEmptyPane(manager.Layout));
        Assert.Equal(new[] { "second", "second", "second" }, calls);
    }

    [AvaloniaFact]
    public void Independent_Registrations_Preserve_Their_Order_Around_Application_Handlers()
    {
        ProbeManager manager = new() { Layout = Root("current") };
        List<string> calls = new();
        manager.Layout.ElementAdded += (_, _) => calls.Add("application");
        manager.LayoutItemContainerStyleSelector = new Selector
        {
            Selecting = _ => { calls.Add("selector"); return null; }
        };
        manager.ApplyStyle();
        calls.Clear();

        Assert.Equal(1, AddEmptyPane(manager.Layout));
        Assert.Equal(new[] { "selector", "application", "selector", "selector" }, calls);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Style_Attachment_Observes_The_Current_Root_After_A_Suppressed_Layout_Transition(bool documents)
    {
        LayoutRoot oldRoot = Root("old", documents);
        LayoutRoot current = Root("current", documents);
        ProbeManager manager = new() { Layout = oldRoot };
        Selector selector = new();
        manager.LayoutItemContainerStyleSelector = selector;
        manager.SuppressLayout = true;
        manager.Layout = current;
        manager.ApplyStyle();
        LayoutContent content = current.Descendents().OfType<LayoutContent>().Single();
        LayoutItem item = manager.GetLayoutItemFromModel(content)!;
        Assert.NotNull(item);

        AddEmptyPane(oldRoot);
        Dispatcher.UIThread.RunJobs();
        Assert.Same(item, manager.GetLayoutItemFromModel(content));
        Assert.Null(manager.GetLayoutItemFromModel(oldRoot.Descendents().OfType<LayoutContent>().Single()));
        selector.Calls.Clear();
        int notifications = AddEmptyPane(current);
        Assert.Equal(notifications, selector.Calls.Count);
        Assert.All(selector.Calls, value => Assert.Same(item, value));
    }

    [AvaloniaFact]
    public void Initial_Attachment_Processes_Documents_Before_Tools_Then_Registers_The_Root()
    {
        ProbeManager manager = new();
        LayoutDocument document = new() { Content = "document" };
        LayoutAnchorable existing = new() { Content = "existing" };
        LayoutAnchorable inserted = new() { Content = "inserted" };
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) };
        root.RootPanel.Children.Add(new LayoutAnchorablePane(existing));
        Selector selector = new()
        {
            Selecting = item =>
            {
                if (!ReferenceEquals(item.LayoutElement, document)) return null;
                root.RootPanel.Children.Add(new LayoutAnchorablePane(inserted));
                Assert.Null(manager.GetLayoutItemFromModel(existing));
                Assert.Null(manager.GetLayoutItemFromModel(inserted));
                return null;
            }
        };
        manager.LayoutItemContainerStyleSelector = selector;

        manager.Layout = root;
        Assert.Equal(new[] { "document", "existing", "inserted" }, selector.Calls.Select(item => item.Model));
        Assert.NotNull(manager.GetLayoutItemFromModel(inserted));
    }

    [AvaloniaFact]
    public void Failed_Initial_Attachment_Does_Not_Start_Membership_Observation()
    {
        ProbeManager manager = new();
        Selector selector = new() { Selecting = _ => throw new InvalidOperationException("Initial style failure") };
        manager.LayoutItemContainerStyleSelector = selector;
        LayoutRoot root = Root("first");
        Assert.Throws<InvalidOperationException>(() => manager.Layout = root);
        selector.Selecting = null;
        selector.Calls.Clear();
        LayoutDocument second = new() { Content = "second" };
        root.Descendents().OfType<LayoutDocumentPane>().Single().Children.Add(second);
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(selector.Calls);
        Assert.Null(manager.GetLayoutItemFromModel(second));

        manager.ApplyStyle();
        Assert.Equal(2, selector.Calls.Count);
        Assert.NotNull(manager.GetLayoutItemFromModel(second));
    }

    [AvaloniaFact]
    public void A_Retained_Old_Root_Does_Not_Retain_An_Unused_Manager()
    {
        LayoutRoot oldRoot = Root("old");
        WeakReference manager = CreateUnusedManager(oldRoot);
        Collect();
        Assert.False(manager.IsAlive);
        AddEmptyPane(oldRoot);
        GC.KeepAlive(oldRoot);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateUnusedManager(LayoutRoot oldRoot)
    {
        ProbeManager manager = new() { Layout = oldRoot, LayoutItemContainerStyleSelector = new Selector() };
        manager.ApplyStyle();
        manager.Layout = Root("current");
        return new(manager);
    }

    [AvaloniaFact]
    public void A_Manager_Does_Not_Retain_An_Old_Root_Through_Its_Registration_History()
    {
        ProbeManager manager = new();
        WeakReference root = ReplaceRoot(manager);
        Collect();
        Assert.False(root.IsAlive);
        GC.KeepAlive(manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ReplaceRoot(ProbeManager manager)
    {
        LayoutRoot oldRoot = Root("old");
        manager.Layout = oldRoot;
        manager.LayoutItemContainerStyleSelector = new Selector();
        manager.ApplyStyle();
        manager.Layout = Root("current");
        // The test recorder and captured change arguments must not own models.
        manager.LayoutItemContainerStyleSelector = new Selector();
        return new(oldRoot);
    }

    private static void Collect()
    {
        Dispatcher.UIThread.RunJobs();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static LayoutRoot Root(object content, bool documents = true)
    {
        ILayoutPanelElement pane = documents
            ? new LayoutDocumentPane(new LayoutDocument { Content = content })
            : new LayoutAnchorablePane(new LayoutAnchorable { Content = content });
        return new() { RootPanel = new LayoutPanel(pane) };
    }

    private static int AddEmptyPane(LayoutRoot root)
    {
        int count = 0;
        void Added(object? sender, LayoutElementEventArgs args) => count++;
        root.ElementAdded += Added;
        try { root.RootPanel.Children.Add(new LayoutAnchorablePane()); }
        finally { root.ElementAdded -= Added; }
        Assert.True(count > 0);
        return count;
    }

    private sealed class Selector : StyleSelector
    {
        internal Func<LayoutItem, ControlTheme?>? Selecting { get; set; }
        internal List<LayoutItem> Calls { get; } = new();
        public override ControlTheme? SelectStyle(object? item, AvaloniaObject container)
        {
            LayoutItem layoutItem = (LayoutItem)container;
            Calls.Add(layoutItem);
            return Selecting?.Invoke(layoutItem);
        }
    }

    private sealed class ProbeManager : DockingManager
    {
        internal bool SuppressStyle { get; set; }
        internal bool FailStyle { get; set; }
        internal bool SuppressLayout { get; set; }
        private AvaloniaPropertyChangedEventArgs? _lastStyleChange;
        internal void ApplyStyle() => base.OnLayoutItemContainerStyleSelectorChanged(_lastStyleChange!);
        protected override void OnLayoutItemContainerStyleSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            _lastStyleChange = e;
            if (FailStyle) throw new InvalidOperationException("Style hook failure");
            if (!SuppressStyle) base.OnLayoutItemContainerStyleSelectorChanged(e);
        }
        protected override void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
        {
            if (!SuppressLayout) base.OnLayoutChanged(oldLayout, newLayout);
        }
    }
}
