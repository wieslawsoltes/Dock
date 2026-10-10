using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SourceBindingTests
{
    [AvaloniaFact]
    public void Document_Source_Add_Replace_Remove_And_Reset_Update_Live_Models()
    {
        object first = new();
        object second = new();
        ObservableCollection<object> source = new() { first, second };
        DockingManager manager = new() { DocumentsSource = source };
        LayoutDocumentPane pane = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocumentPane>());
        Assert.Equal(source, pane.Children.Select(x => x.Content));
        Assert.All(pane.Children, document => Assert.True(Guid.TryParse(document.ContentId, out _)));
        LayoutContent removed = pane.Children[0];
        removed.CanClose = false;
        source.RemoveAt(0);
        Assert.Null(removed.Parent);
        Assert.Null(removed.Content);
        object replacement = new();
        source[0] = replacement;
        Assert.Same(replacement, Assert.Single(pane.Children).Content);
        pane.Children.Add(new LayoutDocument { Content = "manual" });
        manager.ActiveContent = replacement;
        source.Clear();
        Assert.Empty(pane.Children);
        Assert.Null(manager.ActiveContent);
        Assert.Same(pane, Assert.Single(manager.Layout.Descendents().OfType<LayoutDocumentPane>()));
        source.Add(first);
        Assert.Same(first, Assert.Single(pane.Children).Content);
        Assert.Same(pane.DockModel, pane.Children[0].DockModel.Owner);
    }

    [AvaloniaFact]
    public void Tool_Source_Removes_Pinned_And_Hidden_Tools()
    {
        object pinnedContent = new();
        object hiddenContent = new();
        ObservableCollection<object> source = new() { pinnedContent, hiddenContent };
        DockingManager manager = new() { AnchorablesSource = source };
        LayoutAnchorablePane pane = Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorablePane>());
        Assert.Equal(new GridLength(200), pane.DockWidth);
        LayoutAnchorable pinned = pane.Children[0];
        LayoutAnchorable hidden = pane.Children[1];
        pinned.ToggleAutoHide();
        hidden.Hide();
        source.Remove(pinnedContent);
        Assert.Null(pinned.Parent);
        Assert.Empty(manager.Layout.RightSide.Children);
        Assert.Same(pane, Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorablePane>()));
        source.Remove(hiddenContent);
        Assert.Empty(manager.Layout.Hidden);
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutAnchorable>());
        Assert.Null(hidden.Parent);
    }

    [AvaloniaFact]
    public void Replacing_Source_Detaches_Listener_And_Preserves_Unrelated_Content()
    {
        ObservableCollection<object> first = new() { new object() };
        ObservableCollection<object> second = new() { new object() };
        DockingManager manager = new() { DocumentsSource = first };
        LayoutDocumentPane pane = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocumentPane>());
        LayoutDocument manual = new() { Content = new object() };
        pane.Children.Add(manual);
        manager.DocumentsSource = second;
        first.Add(new object());
        Assert.Equal(2, pane.Children.Count);
        Assert.Same(second[0], pane.Children[1].Content);
        manager.DocumentsSource = null;
        second.Add(new object());
        Assert.Same(manual, Assert.Single(pane.Children));
    }

    [AvaloniaFact]
    public void Source_Updates_Preserve_User_Tab_Order_And_Do_Not_Reopen_Closed_Documents()
    {
        object first = new();
        object second = new();
        ObservableCollection<object> source = new() { first, second };
        DockingManager manager = new() { DocumentsSource = source };
        LayoutDocumentPane pane = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocumentPane>());
        source.Move(1, 0);
        Assert.Equal(new[] { first, second }, pane.Children.Select(x => x.Content));
        pane.Children[0].Close();
        object third = new();
        source.Add(third);
        Assert.Equal(new[] { second, third }, pane.Children.Select(x => x.Content));
        Assert.Equal(3, source.Count);
    }

    [AvaloniaFact]
    public void Layout_Replacement_And_Xml_Restore_Reuse_Existing_Source_Content()
    {
        object content = new();
        ObservableCollection<object> source = new() { content };
        DockingManager manager = new() { DocumentsSource = source };
        LayoutRoot old = manager.Layout;
        LayoutDocument existing = new() { Content = content, ContentId = "kept" };
        manager.Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(existing)) };
        Assert.Empty(old.Descendents().OfType<LayoutDocument>());
        Assert.Same(existing, Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>()));
        StringWriter writer = new();
        XmlLayoutSerializer serializer = new(manager);
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        Assert.Equal("kept", Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>()).ContentId);
        source.Add(new object());
        Assert.Equal(2, manager.Layout.Descendents().OfType<LayoutDocument>().Count());
        Assert.Empty(old.Descendents().OfType<LayoutDocument>());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Source_Strategy_Can_Create_Or_Replace_The_Default_Destination(bool custom)
    {
        SourceStrategy strategy = new(custom);
        DockingManager manager = new() { LayoutUpdateStrategy = strategy };
        manager.DocumentsSource = new[] { new object() };
        manager.AnchorablesSource = new[] { new object() };
        Assert.Equal(new[] { "document-before", "document-after", "tool-before", "tool-after" }, strategy.Calls);
        LayoutDocument document = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
        LayoutAnchorable tool = Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());
        Assert.Equal(custom, ReferenceEquals(strategy.Documents, document.Parent));
        Assert.Equal(custom, ReferenceEquals(strategy.Tools, tool.Parent));
        Assert.Same(manager.DockFactory, document.DockModel.Factory);
        Assert.Same(manager.DockFactory, tool.DockModel.Factory);
    }

    [AvaloniaFact]
    public void Last_Focused_Document_Pane_Receives_New_Source_Documents()
    {
        LayoutDocument first = new();
        LayoutDocument second = new();
        LayoutDocumentPane left = new(first);
        LayoutDocumentPane right = new(second);
        LayoutPanel panel = new(left);
        panel.Children.Add(right);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = panel } };
        second.IsActive = true;
        object content = new();
        manager.DocumentsSource = new[] { content };
        Assert.Equal(2, right.Children.Count);
        Assert.Same(content, right.Children[1].Content);
        Assert.Single(left.Children);
    }

    [AvaloniaFact]
    public void Collection_Subscription_Does_Not_Retain_Unused_Manager()
    {
        ObservableCollection<object> source = new() { new object() };
        WeakReference reference = CreateManager(source);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(reference.IsAlive);
        source.Add(new object());
        GC.KeepAlive(source);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateManager(ObservableCollection<object> source) =>
        new(new DockingManager { DocumentsSource = source });

    [AvaloniaFact]
    public void Xceed_Xml_Namespace_And_Compiled_Bindings_Render_Observable_Sources()
    {
        SourceViewModel viewModel = new();
        SourcesFixture window = new() { DataContext = viewModel };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            LayoutDocument document = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
            Assert.Same(viewModel.Documents[0], document.Content);
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Document from ViewModel" && ReferenceEquals(text.DataContext, viewModel.Documents[0]) && text.Bounds.Width > 0);
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Tool from ViewModel" && ReferenceEquals(text.DataContext, viewModel.Tools[0]) && text.Bounds.Width > 0);
            viewModel.Documents.Add(new SourceContent("New document"));
            LayoutDocument added = manager.Layout.Descendents().OfType<LayoutDocument>().Last();
            added.IsActive = true;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("New document", added.Title);
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "New document" && text.Bounds.Width > 0);
            SourceContent changed = new("Replaced document content");
            ((Model.Avalonia.Controls.Document)added.DockModel).Content = changed;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(changed, added.Content);
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), text => ReferenceEquals(text.DataContext, changed) && text.Text == changed.Text && text.Bounds.Width > 0);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Garbage_Collection_Keeps_Hidden_Return_Group_And_Collapses_Nested_Groups()
    {
        LayoutAnchorable tool = new() { Content = new object() };
        LayoutDocumentPane documentPane = new();
        LayoutDocumentPaneGroup inner = new(documentPane) { Orientation = Orientation.Vertical };
        LayoutDocumentPaneGroup outer = new();
        outer.Children.Add(inner);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(outer) };
        root.RootPanel.Children.Add(new LayoutAnchorablePane(tool));
        tool.ToggleAutoHide();
        tool.Hide();
        root.CollectGarbage();
        Assert.Same(root, tool.PreviousContainer!.Root);
        Assert.Same(documentPane, Assert.Single(outer.Children));
        Assert.Equal(Orientation.Vertical, outer.Orientation);
        tool.Show();
        Assert.True(tool.IsAutoHidden);
    }

    private sealed class SourceStrategy(bool custom) : ILayoutUpdateStrategy
    {
        internal List<string> Calls { get; } = new();
        internal LayoutDocumentPane Documents { get; } = new();
        internal LayoutAnchorablePane Tools { get; } = new();
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument document, ILayoutContainer? destination)
        {
            Calls.Add("document-before");
            if (custom) { layout.RootPanel.Children.Add(Documents); Documents.Children.Add(document); }
            return custom;
        }
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool, ILayoutContainer? destination)
        {
            Calls.Add("tool-before");
            if (custom) { layout.RootPanel.Children.Add(Tools); Tools.Children.Add(tool); }
            return custom;
        }
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument document) => Calls.Add("document-after");
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool) => Calls.Add("tool-after");
    }
}
