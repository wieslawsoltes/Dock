using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SourceContainerTests
{
    [AvaloniaFact]
    public void Null_Lookup_Returns_Null_Without_Creating_Containers()
    {
        DockingManager manager = new();
        Assert.Null(manager.GetLayoutItemFromModel(null));
        Assert.Empty(manager.GetLogicalChildren());
    }

    [AvaloniaFact]
    public void Layout_Attachment_Styles_Documents_Before_Tools_Without_Requiring_Lookup()
    {
        List<string> styled = new();
        DockingManager manager = new()
        {
            LayoutItemContainerStyleSelector = new RecordingSelector { Selecting = item => styled.Add((string)item.Model!) }
        };
        manager.Layout = new()
        {
            RootPanel = new LayoutPanel
            {
                Children =
                {
                    new LayoutAnchorablePane(new LayoutAnchorable { Content = "tool" }),
                    new LayoutDocumentPane(new LayoutDocument { Content = "document" })
                }
            }
        };
        Assert.Equal(new[] { "document", "tool" }, styled);
        Assert.All(manager.Layout.Descendents().OfType<LayoutContent>(), content => Assert.NotNull(manager.GetLayoutItemFromModel(content)));
    }

    [AvaloniaTheory]
    [InlineData(true, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, true)]
    public void Containers_And_Styles_Appear_After_Each_Source_Callback_Even_When_It_Renders(bool documents, bool initialImport, bool render)
    {
        DockingManager manager = new();
        List<string> calls = new();
        List<LayoutContent> imported = new();
        RecordingSelector selector = new()
        {
            Selecting = item =>
            {
                Assert.Null(manager.GetLayoutItemFromModel(item.LayoutElement!));
                calls.Add("style:" + ((TextBox)item.Model!).Text);
                Assert.Equal("after callback", item.LayoutElement!.Title);
            }
        };
        manager.LayoutItemContainerStyleSelector = selector;
        Window window = new() { Content = manager, Width = 800, Height = 500 };
        try
        {
            if (render) { window.Show(); Pump(window); }
            manager.LayoutUpdateStrategy = new Strategy
            {
                Before = content =>
                {
                    if (imported.Count != 0) Assert.NotNull(manager.GetLayoutItemFromModel(imported[^1]));
                    imported.Add(content);
                    Assert.Null(manager.GetLayoutItemFromModel(content));
                    calls.Add("before:" + ((TextBox)content.Content!).Text);
                    return false;
                },
                After = content =>
                {
                    Assert.Null(manager.GetLayoutItemFromModel(content));
                    if (render) Pump(window);
                    else Dispatcher.UIThread.RunJobs();
                    Assert.Null(manager.GetLayoutItemFromModel(content));
                    Assert.DoesNotContain((ILogical)content.Content!, manager.GetLogicalChildren());
                    Assert.Equal(imported.Count - 1, selector.Calls);
                    calls.Add("after:" + ((TextBox)content.Content!).Text);
                    content.Title = "after callback";
                }
            };
            TextBox first = new() { Text = "first" };
            TextBox second = new() { Text = "second" };
            RangeSource source = new();
            if (initialImport)
            {
                source.AddRange(first, second);
                SetSource(manager, source, documents);
            }
            else
            {
                SetSource(manager, source, documents);
                source.AddRange(first, second);
            }

            Assert.Equal(new[] { "before:first", "after:first", "style:first", "before:second", "after:second", "style:second" }, calls);
            Assert.All(imported, content =>
            {
                LayoutItem item = Assert.IsAssignableFrom<LayoutItem>(manager.GetLayoutItemFromModel(content));
                Assert.Same(content.Content, item.Model);
                Assert.Same(manager, item.Parent);
                Assert.Equal("styled", content.Title);
                content.Title = "updated after registration";
                Assert.Equal(content.Title, item.Title);
            });
            if (render)
            {
                imported[0].IsActive = true;
                Pump(window);
                Assert.Same(imported[0], manager.Layout.ActiveContent);
                Assert.Same(imported[0], ((ILayoutContentSelector)imported[0].Parent!).SelectedContent);
                Assert.Contains(first, manager.GetVisualDescendants());
                IEnumerable<LayoutItem?> tabs = documents
                    ? manager.GetVisualDescendants().OfType<LayoutDocumentTabItem>().Select(tab => tab.LayoutItem)
                    : manager.GetVisualDescendants().OfType<LayoutAnchorableTabItem>().Select(tab => tab.LayoutItem);
                Assert.All(imported, content => Assert.Contains(manager.GetLayoutItemFromModel(content), tabs));
                Button close = documents
                    ? manager.GetVisualDescendants().OfType<Button>().First(button => button.Classes.Contains("avalondock-close"))
                    : manager.GetVisualDescendants().OfType<Button>().First(button => button.Name == "PART_CloseButton");
                Assert.NotNull(close.Command);
                Assert.True(close.Command!.CanExecute(close.CommandParameter));
                close.Command.Execute(close.CommandParameter);
                Pump(window);
                if (documents) Assert.Null(imported[0].Root);
                else Assert.True(((LayoutAnchorable)imported[0]).IsHidden);
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Initial_Handled_Imports_Register_Detached_Items_While_Live_Imports_Require_Ownership(bool documents, bool initialImport)
    {
        DockingManager manager = new();
        LayoutContent? content = null;
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = item => { content = item; return true; },
            After = item => Assert.Null(manager.GetLayoutItemFromModel(item))
        };
        ObservableCollection<object> source = new();
        if (initialImport) { source.Add(new object()); SetSource(manager, source, documents); }
        else { SetSource(manager, source, documents); source.Add(new object()); }

        Assert.NotNull(content);
        Assert.Null(content.Parent);
        LayoutItem? container = manager.GetLayoutItemFromModel(content);
        Assert.Equal(initialImport, container is not null);
        if (container is not null)
        {
            Assert.Same(content, container.LayoutElement);
            Assert.False(container.ActivateCommand!.CanExecute(null));
        }
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Lookup_Does_Not_Create_Manually_Added_Items_During_Source_Callbacks(bool documents)
    {
        DockingManager manager = new();
        LayoutContent manual = documents ? new LayoutDocument() : new LayoutAnchorable();
        LayoutContent? imported = null;
        manager.LayoutUpdateStrategy = new Strategy
        {
            After = content =>
            {
                imported = content;
                ILayoutGroup pane = (ILayoutGroup)content.Parent!;
                pane.InsertChildAt(pane.ChildrenCount, manual);
                Assert.Null(manager.GetLayoutItemFromModel(manual));
                Dispatcher.UIThread.RunJobs();
                Assert.Null(manager.GetLayoutItemFromModel(manual));
            }
        };
        SetSource(manager, new[] { new object() }, documents);
        Assert.NotNull(manager.GetLayoutItemFromModel(imported!));
        Assert.Null(manager.GetLayoutItemFromModel(manual));
        LayoutContent next = documents ? new LayoutDocument() : new LayoutAnchorable();
        ILayoutGroup target = (ILayoutGroup)manual.Parent!;
        target.InsertChildAt(target.ChildrenCount, next);
        Assert.NotNull(manager.GetLayoutItemFromModel(manual));
        Assert.NotNull(manager.GetLayoutItemFromModel(next));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Ordinary_Attachment_Creates_Containers_Before_Lookup_And_Defers_Removal(bool documents)
    {
        List<LayoutItem> created = new();
        DockingManager manager = new() { LayoutItemContainerStyleSelector = new RecordingSelector { Selecting = created.Add } };
        LayoutContent content = documents ? new LayoutDocument() : new LayoutAnchorable();
        ILayoutGroup pane = documents ? new LayoutDocumentPane() : new LayoutAnchorablePane();
        manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)pane);
        pane.InsertChildAt(0, content);
        LayoutItem item = Assert.Single(created.Distinct());
        Assert.Same(content, item.LayoutElement);
        Assert.Same(item, manager.GetLayoutItemFromModel(content));
        pane.RemoveChild(content);
        Assert.Same(item, manager.GetLayoutItemFromModel(content));
        Assert.False(item.ActivateCommand!.CanExecute(null));
        Dispatcher.UIThread.RunJobs();
        Assert.Null(manager.GetLayoutItemFromModel(content));
        Assert.Null(item.LayoutElement);
    }

    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void A_Floating_Host_Realized_In_A_Callback_Receives_The_Completed_Container(bool documents, bool initialImport)
    {
        DockingManager manager = new();
        Window window = new() { Content = manager, Width = 700, Height = 400 };
        TextBox editor = new() { Text = "source editor" };
        LayoutContent? imported = null;
        try
        {
            window.Show();
            manager.LayoutUpdateStrategy = new Strategy
            {
                After = content =>
                {
                    imported = content;
                    content.Title = "Floating source";
                    content.Float();
                    Pump(window);
                    Assert.Null(manager.GetLayoutItemFromModel(content));
                    Assert.True(content.IsFloating);
                }
            };
            ObservableCollection<object> source = new();
            if (initialImport) { source.Add(editor); SetSource(manager, source, documents); }
            else { SetSource(manager, source, documents); source.Add(editor); }
            Pump(window);
            LayoutFloatingWindowControl host = Assert.Single(manager.FloatingWindows);
            host.UpdateLayout();
            LayoutItem item = Assert.IsAssignableFrom<LayoutItem>(manager.GetLayoutItemFromModel(imported!));
            if (host is LayoutAnchorableFloatingWindowControl tools) Assert.Same(item, tools.SingleContentLayoutItem);
            else Assert.Same(item, ((LayoutDocumentFloatingWindowControl)host).RootDocumentLayoutItem);
            Assert.Contains(editor, host.GetVisualDescendants());
            Assert.Contains(host.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Floating source");
            Button close = documents
                ? host.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("avalondock-document-window-close"))
                : host.GetVisualDescendants().OfType<Button>().First(button => button.Name == "PART_CloseButton");
            Assert.NotNull(close.Command);
            Assert.True(close.Command!.CanExecute(close.CommandParameter));
            imported!.Title = "Renamed floating source";
            Pump(window);
            Assert.Equal(imported.Title, item.Title);
            Assert.Contains(host.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == imported.Title);
        }
        finally { manager.Layout.FloatingWindows.Clear(); window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void A_Throwing_Callback_Does_Not_Leave_Ordinary_Container_Creation_Suspended(bool documents, bool before)
    {
        DockingManager manager = new()
        {
            LayoutUpdateStrategy = new Strategy
            {
                Before = _ => { if (before) throw new InvalidOperationException("callback"); return false; },
                After = _ => { if (!before) throw new InvalidOperationException("callback"); }
            }
        };
        Assert.Throws<InvalidOperationException>(() => SetSource(manager, new[] { new object() }, documents));
        manager.LayoutUpdateStrategy = null;
        LayoutContent manual = documents ? new LayoutDocument() : new LayoutAnchorable();
        ILayoutGroup pane = documents ? new LayoutDocumentPane() : new LayoutAnchorablePane();
        pane.InsertChildAt(0, manual);
        manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)pane);
        Assert.NotNull(manager.GetLayoutItemFromModel(manual));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_Throwing_Style_Selector_Releases_The_Partial_Container_And_Allows_Later_Attachment(bool documents)
    {
        DockingManager manager = new()
        {
            LayoutItemContainerStyleSelector = new RecordingSelector
            {
                Selecting = _ => throw new InvalidOperationException("style")
            }
        };
        Assert.Throws<InvalidOperationException>(() => SetSource(manager, new[] { new object() }, documents));
        LayoutContent content = Assert.Single(manager.Layout.Descendents().OfType<LayoutContent>());
        Assert.Null(manager.GetLayoutItemFromModel(content));
        Assert.Empty(manager.GetLogicalChildren().OfType<LayoutItem>());
        manager.LayoutItemContainerStyleSelector = null;
        LayoutContent other = documents ? new LayoutDocument() : new LayoutAnchorable();
        ILayoutGroup pane = (ILayoutGroup)content.Parent!;
        pane.InsertChildAt(pane.ChildrenCount, other);
        Assert.NotNull(manager.GetLayoutItemFromModel(content));
        Assert.NotNull(manager.GetLayoutItemFromModel(other));
        Assert.Empty(manager.GetLogicalChildren().OfType<LayoutItem>());
    }

    [AvaloniaFact]
    public void A_Retained_Header_That_Waited_For_Registration_Releases_The_Closed_Workspace()
    {
        var (header, model, manager) = CreateClosedPendingHeader();
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.Null(header.Content);
        Assert.Null(header.ContentTemplate);
        Assert.Null(header.Child);
        Assert.Null(header.Parent);
        Assert.Null(header.GetVisualParent());
        Assert.Null(header.TemplatedParent);
        Assert.Null(header.DataContext);
        Assert.False(model.TryGetTarget(out _));
        Assert.False(manager.TryGetTarget(out _));
        GC.KeepAlive(header);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ContentPresenter, WeakReference<LayoutContent>, WeakReference<DockingManager>) CreateClosedPendingHeader()
    {
        DockingManager manager = new();
        Window window = new() { Content = manager, Width = 700, Height = 400 };
        window.Show();
        manager.LayoutUpdateStrategy = new Strategy { After = content => { content.Title = "Pending title"; content.Float(); Pump(window); } };
        manager.DocumentsSource = new[] { new TextBox() };
        Pump(window);
        LayoutContent model = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
        LayoutFloatingWindowControl host = Assert.Single(manager.FloatingWindows);
        ContentPresenter title = host.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(presenter => presenter.Classes.Contains("avalondock-document-title"));
        ContentPresenter header = Assert.IsAssignableFrom<ContentPresenter>(title.Child);
        WeakReference<LayoutContent> modelReference = new(model);
        WeakReference<DockingManager> managerReference = new(manager);
        manager.Layout.FloatingWindows.Clear();
        manager.DocumentsSource = null;
        // Upstream Reset retains the removed floating model's public parent;
        // source detachment only visits the current layout. Replace the layout
        // explicitly to dispose every retained item before checking retention.
        manager.Layout = new LayoutRoot();
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return (header, modelReference, managerReference);
    }

    private static void Pump(Window window) { window.UpdateLayout(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }

    private static void SetSource(DockingManager manager, IEnumerable source, bool documents)
    {
        if (documents) manager.DocumentsSource = source;
        else manager.AnchorablesSource = source;
    }

    private sealed class RangeSource : ObservableCollection<object>
    {
        internal void AddRange(params object[] values)
        {
            int index = Count;
            foreach (object value in values) Items.Add(value);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, values, index));
        }
    }

    private sealed class Strategy : ILayoutUpdateStrategy
    {
        internal Func<LayoutContent, bool>? Before { get; init; }
        internal Action<LayoutContent>? After { get; init; }
        public bool BeforeInsertDocument(LayoutRoot root, LayoutDocument document, ILayoutContainer? target) => Before?.Invoke(document) ?? false;
        public bool BeforeInsertAnchorable(LayoutRoot root, LayoutAnchorable tool, ILayoutContainer? target) => Before?.Invoke(tool) ?? false;
        public void AfterInsertDocument(LayoutRoot root, LayoutDocument document) => After?.Invoke(document);
        public void AfterInsertAnchorable(LayoutRoot root, LayoutAnchorable tool) => After?.Invoke(tool);
    }

    private sealed class RecordingSelector : StyleSelector
    {
        internal Action<LayoutItem>? Selecting { get; init; }
        internal int Calls { get; private set; }
        public override ControlTheme? SelectStyle(object? model, AvaloniaObject container)
        {
            Calls++;
            LayoutItem item = (LayoutItem)container;
            Selecting?.Invoke(item);
            return new ControlTheme(typeof(LayoutItem)) { Setters = { new Setter(LayoutItem.TitleProperty, "styled") } };
        }
    }
}
