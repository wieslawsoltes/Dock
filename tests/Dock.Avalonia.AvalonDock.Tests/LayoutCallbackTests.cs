using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutCallbackTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Redirecting_Before_Base_Assigns_Ownership_And_Containers_To_Current_Layout(bool suppressNested)
    {
        Manager manager = new();
        LayoutRoot previous = manager.Layout;
        LayoutRoot supplied = Root("supplied");
        LayoutRoot current = Root("current");
        manager.Before = (_, next) =>
        {
            if (!ReferenceEquals(next, supplied)) return;
            manager.Suppress = suppressNested;
            manager.Layout = current;
            manager.Suppress = false;
        };

        manager.Layout = supplied;

        Assert.Same(current, manager.Layout);
        Assert.Same(manager, current.Manager);
        Assert.Null(previous.Manager);
        Assert.Null(supplied.Manager);
        Assert.NotNull(manager.GetLayoutItemFromModel(Content(current)));
        Assert.Null(manager.GetLayoutItemFromModel(Content(supplied)));
    }

    [AvaloniaFact]
    public void Redirecting_After_Base_Completes_The_Nested_Transition()
    {
        Manager manager = new();
        LayoutRoot supplied = Root("supplied");
        LayoutRoot current = Root("current");
        List<LayoutRoot> changed = new();
        manager.LayoutChanged += (_, _) => changed.Add(manager.Layout);
        manager.After = (_, next) => { if (ReferenceEquals(next, supplied)) manager.Layout = current; };
        manager.Layout = supplied;
        Assert.Equal(new[] { supplied, current }, changed);
        Assert.Null(supplied.Manager);
        Assert.Same(manager, current.Manager);
        Assert.Null(manager.GetLayoutItemFromModel(Content(supplied)));
        Assert.NotNull(manager.GetLayoutItemFromModel(Content(current)));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Replayed_Arguments_Import_Their_Root_While_Live_Sources_Update_Current_Layout(bool documents)
    {
        Manager manager = new();
        ObservableCollection<object> source = new() { new object() };
        SetSource(manager, source, documents);
        LayoutRoot previous = manager.Layout;
        LayoutRoot supplied = Root("supplied");
        LayoutRoot current = Root("current");
        manager.Suppress = true;
        manager.Layout = current;
        manager.Apply(previous, supplied);

        Assert.Same(current, manager.Layout);
        Assert.Same(manager, current.Manager);
        Assert.Null(supplied.Manager);
        Assert.Null(previous.Manager);
        Assert.DoesNotContain(Contents(previous), item => ReferenceEquals(item.Content, source[0]));
        LayoutContent imported = Assert.Single(Contents(supplied), item => ReferenceEquals(item.Content, source[0]));
        Assert.NotNull(manager.GetLayoutItemFromModel(imported));
        Assert.NotNull(manager.GetLayoutItemFromModel(Content(current)));
        Assert.DoesNotContain(Contents(current), item => ReferenceEquals(item.Content, source[0]));
        source.Add(new object());
        Assert.Contains(Contents(current), item => ReferenceEquals(item.Content, source[1]));
        Assert.DoesNotContain(Contents(supplied), item => ReferenceEquals(item.Content, source[1]));
    }

    [AvaloniaFact]
    public void Replaying_A_Foreign_Root_Does_Not_Transfer_Its_Ownership_Or_Containers()
    {
        DockingManager owner = new() { Layout = Root("foreign") };
        Manager manager = new() { Layout = Root("current") };
        LayoutRoot foreign = owner.Layout;
        LayoutItem item = owner.GetLayoutItemFromModel(Content(foreign))!;
        manager.Apply(null, foreign);
        Assert.Same(owner, foreign.Manager);
        Assert.Same(manager, manager.Layout.Manager);
        Assert.Same(item, owner.GetLayoutItemFromModel(Content(foreign)));
        Assert.Null(manager.GetLayoutItemFromModel(Content(foreign)));
    }

    [AvaloniaFact]
    public void A_Null_Replayed_New_Argument_Retains_Current_Ownership_And_Containers()
    {
        Manager manager = new() { Layout = Root("current") };
        LayoutRoot current = manager.Layout;
        manager.Apply(current, null);
        Assert.Same(current, manager.Layout);
        Assert.Same(manager, current.Manager);
        Assert.NotNull(manager.GetLayoutItemFromModel(Content(current)));
    }

    [AvaloniaFact]
    public void Root_Notifications_Use_The_Supplied_Argument_And_Read_Current_Presentation()
    {
        Manager manager = InitializedManager();
        LayoutRoot previous = manager.Layout;
        LayoutRoot current = Root("current");
        LayoutRoot supplied = Root("supplied");
        manager.Suppress = true;
        manager.Layout = current;
        manager.Apply(previous, supplied);
        LayoutPanelControl presented = manager.LayoutRootPanel!;
        current.RootPanel = Panel("replacement");
        Assert.Same(presented, manager.LayoutRootPanel);
        supplied.RootPanel = Panel("notification");
        Assert.Same(current.RootPanel, manager.LayoutRootPanel!.Model);
        current.LeftSide = new LayoutAnchorSide();
        Assert.NotSame(current.LeftSide, manager.LeftSidePanel!.Model);
        supplied.LeftSide = new LayoutAnchorSide();
        Assert.Same(current.LeftSide, manager.LeftSidePanel!.Model);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Source_Import_Precedes_Presentation_And_New_Root_Notifications(bool documents)
    {
        Manager manager = InitializedManager();
        SetSource(manager, new[] { new object() }, documents);
        LayoutPanelControl previousPanel = manager.LayoutRootPanel!;
        LayoutRoot next = Root("next");
        manager.LayoutUpdateStrategy = new Strategy
        {
            After = (root, _) =>
            {
                Assert.Same(next, root);
                Assert.Same(previousPanel, manager.LayoutRootPanel);
                root.LeftSide = new LayoutAnchorSide();
                Assert.NotSame(root.LeftSide, manager.LeftSidePanel!.Model);
            }
        };
        manager.Layout = next;
        Assert.Same(next.RootPanel, manager.LayoutRootPanel!.Model);
        Assert.Same(next.LeftSide, manager.LeftSidePanel!.Model);
        next.RootPanel = Panel("after import");
        Assert.Same(next.RootPanel, manager.LayoutRootPanel.Model);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Failed_Imports_Do_Not_Replace_Presentation_Or_Register_New_Root_Notifications(bool documents)
    {
        Manager manager = InitializedManager();
        SetSource(manager, new[] { new object() }, documents);
        LayoutPanelControl previousPanel = manager.LayoutRootPanel!;
        LayoutRoot next = Root("next");
        manager.LayoutUpdateStrategy = new Strategy { After = (_, _) => throw new InvalidOperationException("Import failed.") };
        Assert.Throws<InvalidOperationException>(() => manager.Layout = next);
        Assert.Same(next, manager.Layout);
        Assert.Same(manager, next.Manager);
        Assert.Same(previousPanel, manager.LayoutRootPanel);
        next.RootPanel = Panel("later mutation");
        Assert.Same(previousPanel, manager.LayoutRootPanel);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Replays_Add_Independent_Root_Registrations_And_Detach_One_Old_Argument(bool detachOne)
    {
        Manager manager = InitializedManager();
        LayoutRoot previous = manager.Layout;
        LayoutRoot current = Root("current");
        manager.Suppress = true;
        manager.Layout = current;
        NotificationRoot supplied = new();
        manager.Apply(previous, null);
        for (int i = 0; i < 3; i++) manager.Apply(null, supplied);
        if (detachOne) manager.Apply(supplied, Root("other"));
        int changes = 0;
        manager.PanelChanged = () => current.RootPanel = Panel("callback " + ++changes);
        current.RootPanel = Panel("pending");
        supplied.NotifyPanel();
        Assert.Equal(detachOne ? 2 : 3, changes);
    }

    [AvaloniaFact]
    public void Replayed_Root_Registrations_Preserve_Application_Handler_Order()
    {
        Manager manager = InitializedManager();
        LayoutRoot previous = manager.Layout;
        LayoutRoot current = Root("current");
        manager.Suppress = true;
        manager.Layout = current;
        NotificationRoot supplied = new();
        List<string> calls = new();
        manager.Apply(previous, supplied);
        supplied.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(LayoutRoot.RootPanel)) return;
            calls.Add("application");
            current.RootPanel = Panel("application");
        };
        manager.Apply(null, supplied);
        manager.PanelChanged = () => calls.Add("manager");
        current.RootPanel = Panel("pending");
        supplied.NotifyPanel();
        Assert.Equal(new[] { "manager", "application", "manager" }, calls);
    }

    [AvaloniaFact]
    public void Supplied_Root_Updates_Refresh_Current_Container_Commands()
    {
        Manager manager = new() { Layout = Root("current") };
        LayoutRoot supplied = Root("supplied");
        manager.Apply(null, supplied);
        DockCapabilityPolicy policy = new() { CanClose = true };
        manager.DockLayout.RootDockCapabilityPolicy = policy;
        System.Windows.Input.ICommand close = manager.GetLayoutItemFromModel(Content(manager.Layout))!.CloseCommand!;
        Assert.True(close.CanExecute(null));
        int notifications = 0;
        close.CanExecuteChanged += (_, _) => notifications++;
        policy.CanClose = false;
        Assert.False(close.CanExecute(null));
        Assert.Equal(0, notifications);
        supplied.LeftSide = new LayoutAnchorSide();
        Assert.False(close.CanExecute(null));
        Assert.True(notifications > 0);
    }

    [AvaloniaFact]
    public void Loaded_Floating_And_Pinned_Presentations_Follow_The_Redirected_Current_Root()
    {
        Manager manager = new();
        Window window = new() { Content = manager, Width = 800, Height = 500 };
        LayoutRoot current = Root("current");
        LayoutDocument document = (LayoutDocument)Content(current);
        LayoutDocumentFloatingWindow floating = new() { RootDocument = document };
        current.FloatingWindows.Add(floating);
        LayoutAnchorable tool = new() { Content = "pinned", Title = "Pinned" };
        current.RootPanel.Children.Add(new LayoutAnchorablePane(tool));
        tool.ToggleAutoHide();
        LayoutRoot supplied = Root("supplied");
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            manager.Before = (_, next) =>
            {
                if (!ReferenceEquals(next, supplied)) return;
                manager.Suppress = true;
                manager.Layout = current;
                manager.Suppress = false;
            };
            manager.Layout = supplied;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(current.RootPanel, manager.LayoutRootPanel!.Model);
            Assert.Same(current.LeftSide, manager.LeftSidePanel!.Model);
            LayoutFloatingWindowControl host = Assert.Single(manager.FloatingWindows);
            Assert.Same(floating, host.Model);
            Assert.True(host.IsVisible);
            manager.DockFactory.PreviewPinnedDockable(tool.DockModel);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(tool, manager.AutoHideWindow!.Model);
            LayoutDocumentControl view = Assert.Single(host.GetVisualDescendants().OfType<LayoutDocumentControl>(), control => ReferenceEquals(control.Model, document));
            Assert.Same(manager.GetLayoutItemFromModel(document), view.LayoutItem);
        }
        finally
        {
            current.FloatingWindows.Clear();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void A_Retained_Callback_Argument_Does_Not_Retain_An_Unused_Manager()
    {
        LayoutRoot argument = Root("argument");
        WeakReference manager = ReplayAndReleaseManager(argument);
        Collect();
        Assert.False(manager.IsAlive);
        argument.LeftSide = new LayoutAnchorSide();
        GC.KeepAlive(argument);
    }

    [AvaloniaFact]
    public void A_Manager_Does_Not_Retain_Obsolete_Callback_Arguments()
    {
        Manager manager = new();
        WeakReference argument = ReplayAndReleaseArgument(manager);
        Collect();
        Assert.False(argument.IsAlive);
        GC.KeepAlive(manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ReplayAndReleaseManager(LayoutRoot argument)
    {
        Manager manager = new();
        manager.Apply(null, argument);
        return new(manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ReplayAndReleaseArgument(Manager manager)
    {
        LayoutRoot argument = Root("argument");
        manager.Apply(null, argument);
        return new(argument);
    }

    private static void Collect()
    {
        Dispatcher.UIThread.RunJobs();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static Manager InitializedManager()
    {
        Manager manager = new();
        manager.Initialize();
        manager.OnApplyTemplate();
        return manager;
    }

    private static LayoutRoot Root(string value) => new() { RootPanel = Panel(value) };
    private static LayoutPanel Panel(string value) => new(new LayoutDocumentPane(new LayoutDocument { Content = value }));
    private static IEnumerable<LayoutContent> Contents(LayoutRoot root) => root.Descendents().OfType<LayoutContent>();
    private static LayoutContent Content(LayoutRoot root) => Assert.Single(Contents(root));
    private static void SetSource(Manager manager, IEnumerable source, bool documents)
    {
        if (documents) manager.DocumentsSource = source;
        else manager.AnchorablesSource = source;
    }

    private sealed class Manager : DockingManager
    {
        internal Action<LayoutRoot, LayoutRoot>? Before { get; set; }
        internal Action<LayoutRoot, LayoutRoot>? After { get; set; }
        internal bool Suppress { get; set; }
        internal Action? PanelChanged { get; set; }
        internal void Initialize() => InitializeIfNeeded();
        internal void Apply(LayoutRoot? previous, LayoutRoot? next) => base.OnLayoutChanged(previous!, next!);
        protected override void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
        {
            Before?.Invoke(oldLayout, newLayout);
            if (!Suppress) base.OnLayoutChanged(oldLayout, newLayout);
            After?.Invoke(oldLayout, newLayout);
        }
        protected override void OnLayoutRootPanelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnLayoutRootPanelChanged(e);
            PanelChanged?.Invoke();
        }
    }

    private sealed class NotificationRoot : LayoutRoot
    {
        internal void NotifyPanel() => RaisePropertyChanged(nameof(RootPanel));
    }

    private sealed class Strategy : ILayoutUpdateStrategy
    {
        internal Action<LayoutRoot, LayoutContent>? After { get; init; }
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument document, ILayoutContainer? destination) => false;
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool, ILayoutContainer? destination) => false;
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument document) => After?.Invoke(layout, document);
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool) => After?.Invoke(layout, tool);
    }
}
