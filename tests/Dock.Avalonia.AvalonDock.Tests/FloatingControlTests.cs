using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Converters;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingControlTests
{
    [AvaloniaFact]
    public void Collection_Exposes_Actual_Typed_Native_Hosts_And_Reports_Their_Identities()
    {
        using Workspace workspace = new();
        List<NotifyCollectionChangedEventArgs> changes = new();
        workspace.Manager.LayoutFloatingWindowControlCollectionChanged += (sender, args) =>
        {
            Assert.Same(workspace.Manager, sender);
            changes.Add(args.CollectionChangedEventArgs);
            if (args.CollectionChangedEventArgs.NewItems is { } added)
                foreach (LayoutFloatingWindowControl host in added)
                {
                    Assert.Same(host, ((LayoutFloatingWindow)host.Model).DockWindow.Host);
                    Assert.Same(((LayoutFloatingWindow)host.Model).DockWindow, host.Window);
                    Assert.Contains(host, workspace.Manager.FloatingWindows);
                }
        };
        workspace.Document.Float();
        workspace.Tool.Float();
        LayoutDocumentFloatingWindowControl documentHost = Assert.IsType<LayoutDocumentFloatingWindowControl>(workspace.Manager.FloatingWindows.First());
        LayoutAnchorableFloatingWindowControl toolHost = Assert.IsType<LayoutAnchorableFloatingWindowControl>(workspace.Manager.FloatingWindows.Last());
        Assert.Same(workspace.Document.Parent, ((ILayoutControl)documentHost).Model);
        Assert.Same(workspace.Tool.FindParent<LayoutFloatingWindow>(), toolHost.Model);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Document), documentHost.RootDocumentLayoutItem);
        Assert.Equal(new[] { 0, 1 }, changes.Select(args => args.NewStartingIndex));
        Assert.All(changes, args => Assert.Equal(NotifyCollectionChangedAction.Add, args.Action));
        workspace.Document.Dock();
        Assert.Same(toolHost, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Equal(NotifyCollectionChangedAction.Remove, changes[2].Action);
        Assert.Same(documentHost, Assert.Single(changes[2].OldItems!));
        Assert.Equal(0, changes[2].OldStartingIndex);
    }

    [AvaloniaFact]
    public void Tool_Host_Uses_Compiled_Single_Pane_Selection_And_Existing_Containers()
    {
        using Workspace workspace = new();
        LayoutAnchorable second = new() { ContentId = "second", Title = "Second" };
        workspace.Tools.Children.Add(second);
        workspace.Manager.DockFactory.FloatAllDockables(workspace.Tool.DockModel);
        Dispatcher.UIThread.RunJobs();
        LayoutAnchorableFloatingWindowControl host = Assert.IsType<LayoutAnchorableFloatingWindowControl>(Assert.Single(workspace.Manager.FloatingWindows));
        LayoutAnchorableFloatingWindow model = (LayoutAnchorableFloatingWindow)host.Model;
        LayoutAnchorablePane pane = Assert.IsType<LayoutAnchorablePane>(model.SinglePane);
        pane.SelectedContentIndex = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(second), host.SingleContentLayoutItem);
        pane.SelectedContentIndex = 0;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Tool), host.SingleContentLayoutItem);
        LayoutItem explicitItem = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        host.SingleContentLayoutItem = explicitItem;
        pane.SelectedContentIndex = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(explicitItem, host.SingleContentLayoutItem);
        host.ClearValue(LayoutAnchorableFloatingWindowControl.SingleContentLayoutItemProperty);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(second), host.SingleContentLayoutItem);
        model.RootPanel!.Children.Add(new LayoutAnchorablePane(new LayoutAnchorable { Title = "Third" }));
        Dispatcher.UIThread.RunJobs();
        Assert.Null(host.SingleContentLayoutItem);
    }

    [AvaloniaFact]
    public void Canceling_Close_Keeps_The_Control_And_Accepting_Close_Removes_It_Once()
    {
        using Workspace workspace = new();
        workspace.Document.Float();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        int removed = 0;
        workspace.Manager.LayoutFloatingWindowControlCollectionChanged += (_, args) =>
        {
            if (args.CollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove) removed++;
        };
        bool cancel = true;
        workspace.Document.Closing += (_, args) => args.Cancel = cancel;
        host.Close();
        Assert.Same(host, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.True(host.IsVisible);
        Assert.Equal(0, removed);
        cancel = false;
        host.Close();
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Equal(1, removed);
    }

    [AvaloniaFact]
    public void View_Reattachment_Creates_New_Controls_For_The_Same_Floating_Models()
    {
        using Workspace workspace = new();
        workspace.Document.Float();
        LayoutFloatingWindowControl first = Assert.Single(workspace.Manager.FloatingWindows);
        IEnumerable<LayoutFloatingWindowControl> controls = workspace.Manager.FloatingWindows;
        workspace.Window.Content = null;
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(controls);
        Assert.Same(first.Model, Assert.Single(workspace.Manager.Layout.FloatingWindows));
        workspace.Window.Content = workspace.Manager;
        Dispatcher.UIThread.RunJobs();
        LayoutDocumentFloatingWindowControl second = Assert.IsType<LayoutDocumentFloatingWindowControl>(Assert.Single(controls));
        Assert.NotSame(first, second);
        Assert.Same(first.Model, second.Model);
        Assert.True(second.IsVisible);
    }

    [AvaloniaFact]
    public void Xml_Replacement_Removes_Old_Controls_And_Observes_Only_The_New_Layout()
    {
        using Workspace workspace = new();
        workspace.Document.Float();
        LayoutRoot oldRoot = workspace.Manager.Layout;
        LayoutFloatingWindowControl first = Assert.Single(workspace.Manager.FloatingWindows);
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using System.IO.StringWriter text = new();
        serializer.Serialize(text);
        serializer.Deserialize(new System.IO.StringReader(text.ToString()));
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindowControl replacement = Assert.Single(workspace.Manager.FloatingWindows);
        Assert.NotSame(first, replacement);
        Assert.Same(workspace.Manager.Layout, replacement.Model.Root);
        oldRoot.FloatingWindows.Clear();
        Assert.Same(replacement, Assert.Single(workspace.Manager.FloatingWindows));
    }

    [AvaloniaFact]
    public void Native_Maximization_Updates_The_Public_Control_State()
    {
        using Workspace workspace = new();
        workspace.Document.Float();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        Assert.False(host.IsMaximized);
        host.WindowState = WindowState.Maximized;
        Assert.True(host.IsMaximized);
        Assert.True(workspace.Document.IsMaximized);
        host.WindowState = WindowState.Normal;
        Assert.False(host.IsMaximized);
        Assert.False(workspace.Document.IsMaximized);
    }

    [AvaloniaFact]
    public void Reentrant_Collection_Notifications_Preserve_The_Model_For_Each_Host()
    {
        using Workspace workspace = new();
        List<LayoutFloatingWindowControl> added = new();
        workspace.Manager.LayoutFloatingWindowControlCollectionChanged += (_, args) =>
        {
            if (args.CollectionChangedEventArgs.Action != NotifyCollectionChangedAction.Add) return;
            LayoutFloatingWindowControl host = Assert.IsAssignableFrom<LayoutFloatingWindowControl>(Assert.Single(args.CollectionChangedEventArgs.NewItems!));
            added.Add(host);
            if (host is LayoutDocumentFloatingWindowControl) workspace.Tool.Float();
        };
        workspace.Document.Float();
        Assert.Equal(2, added.Count);
        Assert.Same(workspace.Document.Parent, added[0].Model);
        Assert.Same(workspace.Tool.FindParent<LayoutFloatingWindow>(), added[1].Model);
        Assert.Equal(added, workspace.Manager.FloatingWindows);
    }

    [AvaloniaFact]
    public void Native_Locators_Preserve_Nested_Model_Requests_And_Release_Them_After_Exceptions()
    {
        using Workspace workspace = new();
        IFactory factory = workspace.Manager.DockFactory;
        Func<IHostWindow?> original = factory.HostWindowLocator![nameof(IDockWindow)];
        bool nested = false;
        factory.HostWindowLocator[nameof(IDockWindow)] = () =>
        {
            if (!nested) { nested = true; workspace.Tool.Float(); }
            return original();
        };
        workspace.Document.Float();
        Assert.Same(workspace.Document.Parent, Assert.Single(workspace.Manager.FloatingWindows.OfType<LayoutDocumentFloatingWindowControl>()).Model);
        Assert.Same(workspace.Tool.FindParent<LayoutFloatingWindow>(), Assert.Single(workspace.Manager.FloatingWindows.OfType<LayoutAnchorableFloatingWindowControl>()).Model);
        factory.HostWindowLocator[nameof(IDockWindow)] = () => throw new InvalidOperationException("Host creation failed.");
        try
        {
            LayoutFloatingWindow model = (LayoutFloatingWindow)workspace.Document.Parent!;
            Assert.Throws<InvalidOperationException>(() => ((IDockWindowHostFactory)factory).CreateHostWindow(model.DockWindow));
            IHostWindow fallback = workspace.Manager.DockHostWindowFactory()!;
            Assert.False(fallback is LayoutFloatingWindowControl);
            fallback.Exit();
        }
        finally { factory.HostWindowLocator[nameof(IDockWindow)] = original; }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Content_Immutability_Applies_Only_After_Loading(bool immutable)
    {
        ProbeWindow window = new(new LayoutDocumentFloatingWindow(), immutable);
        TextBlock original = new() { Text = "Original" };
        TextBlock replacement = new() { Text = "Replacement" };
        window.Content = original;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.IsLoaded);
            Assert.Equal(immutable, window.IsContentImmutable);
            window.Content = replacement;
            Assert.Same(immutable ? original : replacement, window.Content);
            window.Drag(true);
            window.Drag(false);
            Assert.Equal(new[] { true, false }, window.DragChanges);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Public_Converter_Uses_The_Current_Manager_Container()
    {
        using Workspace workspace = new();
        LayoutItemFromLayoutModelConverter converter = new();
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Document), converter.Convert(workspace.Document, typeof(LayoutItem), null, CultureInfo.InvariantCulture));
        Assert.Null(converter.Convert(new LayoutDocument(), typeof(LayoutItem), null, CultureInfo.InvariantCulture));
        Assert.Null(converter.Convert(new object(), typeof(LayoutItem), null, CultureInfo.InvariantCulture));
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(null, typeof(LayoutContent), null, CultureInfo.InvariantCulture));
        NotifyCollectionChangedEventArgs original = new(NotifyCollectionChangedAction.Reset);
        Assert.Same(original, new LayoutFloatingWindowControlCollectionChangedEventArgs(original).CollectionChangedEventArgs);
    }

    [AvaloniaFact]
    public void Retaining_The_Empty_Control_Collection_Does_Not_Retain_A_Closed_Manager()
    {
        (IEnumerable<LayoutFloatingWindowControl> controls, WeakReference manager) = CreateClosedManager();
        for (int i = 0; i < 8 && manager.IsAlive; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.Empty(controls);
        Assert.False(manager.IsAlive);
        GC.KeepAlive(controls);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (IEnumerable<LayoutFloatingWindowControl>, WeakReference) CreateClosedManager()
    {
        using Workspace workspace = new();
        workspace.Document.Float();
        workspace.Tool.Float();
        return (workspace.Manager.FloatingWindows, new WeakReference(workspace.Manager));
    }

    private sealed class ProbeWindow : LayoutFloatingWindowControl
    {
        public ProbeWindow(ILayoutElement model, bool immutable) : base(model, immutable) => Model = model;
        public override ILayoutElement Model { get; }
        public List<bool> DragChanges { get; } = new();
        public void Drag(bool value) => SetIsDragging(value);
        protected override void OnIsDraggingChanged(AvaloniaPropertyChangedEventArgs e) => DragChanges.Add(IsDragging);
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new();
        internal LayoutDocument Document { get; } = new() { ContentId = "document", Title = "Document", Content = "Document content" };
        internal LayoutAnchorable Tool { get; } = new() { ContentId = "tool", Title = "Tool", Content = "Tool content" };
        internal LayoutAnchorablePane Tools { get; }
        internal Window Window { get; }

        internal Workspace()
        {
            Tools = new(Tool);
            LayoutPanel panel = new(new LayoutDocumentPane(Document));
            panel.Children.Add(Tools);
            Manager.Layout = new() { RootPanel = panel };
            Window = new() { Width = 800, Height = 500, Content = Manager };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose() { Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
