using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutBridgeTests
{
    [AvaloniaFact]
    public void Layout_Uses_The_Actual_Avalonia_Models_And_Factory()
    {
        LayoutDocument document = new() { ContentId = "document", Title = "Document", Content = "Editor" };
        LayoutDocumentPane pane = new(document);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };

        Assert.IsType<Document>(document.DockModel);
        Assert.IsType<DocumentDock>(pane.DockModel);
        Assert.IsType<RootDock>(root.DockModel);
        Assert.IsType<Model.Avalonia.Factory>(root.DockFactory);
        Assert.Same(root.DockFactory, document.DockModel.Factory);
        Assert.Same(document.DockModel, Assert.Single(((DocumentDock)pane.DockModel).VisibleDockables!));
        Assert.Same(pane, document.Parent);
        Assert.Same(root, document.Root);
    }

    [AvaloniaFact]
    public void Content_And_Metadata_Are_Bidirectional()
    {
        LayoutDocument document = new() { Title = "Initial", ContentId = "one", Content = "Text" };
        Document model = (Document)document.DockModel;
        List<string?> changes = new();
        document.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
        model.Title = "Changed";
        model.Id = "two";
        model.Content = "Other";
        Assert.Equal("Changed", document.Title);
        Assert.Equal("two", document.ContentId);
        Assert.Equal("Other", document.Content);
        Assert.Contains(nameof(LayoutContent.ContentId), changes);
        document.CanFloat = false;
        document.CanClose = false;
        document.CanMove = false;
        Assert.False(model.CanFloat);
        Assert.False(model.CanClose);
        Assert.False(model.CanDrag);
    }

    [AvaloniaFact]
    public void Collection_Changes_And_Dock_Moves_Preserve_Wrapper_Identity()
    {
        LayoutDocument document = new() { ContentId = "one" };
        LayoutDocumentPane first = new(document);
        LayoutDocumentPane second = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(first) };
        root.RootPanel.Children.Add(second);
        root.DockFactory.MoveDockable((DocumentDock)first.DockModel, (DocumentDock)second.DockModel, document.DockModel, null);
        Assert.Empty(first.Children);
        Assert.Same(document, Assert.Single(second.Children));
        Assert.Same(second, document.Parent);
        Assert.Same(root, document.Root);
    }

    [AvaloniaFact]
    public void Native_Addition_And_List_Replacement_Appear_In_Public_Collection()
    {
        LayoutDocumentPane pane = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        Document model = new() { Title = "Native" };
        root.DockFactory.AddDockable((DocumentDock)pane.DockModel, model);
        LayoutContent wrapper = Assert.Single(pane.Children);
        Assert.Same(model, wrapper.DockModel);
        ((DocumentDock)pane.DockModel).VisibleDockables = root.DockFactory.CreateList<Model.Core.IDockable>(model);
        Assert.Same(wrapper, Assert.Single(pane.Children));
    }

    [AvaloniaFact]
    public void Children_Move_Replace_And_Clear_Keep_Native_Order()
    {
        LayoutDocument a = new() { ContentId = "a" };
        LayoutDocument b = new() { ContentId = "b" };
        LayoutDocument c = new() { ContentId = "c" };
        LayoutDocumentPane pane = new(a);
        pane.Children.Add(b);
        pane.Children.Move(0, 1);
        Assert.Equal(new[] { "b", "a" }, ((DocumentDock)pane.DockModel).VisibleDockables!.Select(x => x.Id));
        pane.Children[0] = c;
        Assert.Null(b.Parent);
        Assert.Same(c.DockModel, ((DocumentDock)pane.DockModel).VisibleDockables![0]);
        pane.Children.Clear();
        Assert.Empty(((DocumentDock)pane.DockModel).VisibleDockables!);
        Assert.Null(a.Parent);
        Assert.Null(c.Parent);
    }

    [AvaloniaFact]
    public void Collection_Rejects_Cycles_And_Duplicates_Without_Changing_The_Tree()
    {
        LayoutPanel panel = new();
        LayoutPanel child = new();
        panel.Children.Add(child);
        Assert.Throws<InvalidOperationException>(() => child.Children.Add(panel));
        Assert.Throws<InvalidOperationException>(() => panel.Children.Add(child));
        Assert.Same(child, Assert.Single(panel.Children));
        Assert.Empty(child.Children);
    }

    [AvaloniaFact]
    public void Split_Panels_Insert_Dock_Splitters_Without_Exposing_Them_As_Children()
    {
        LayoutPanel panel = new(new LayoutDocumentPane());
        panel.Children.Add(new LayoutAnchorablePane());
        Assert.Equal(2, panel.Children.Count);
        Assert.Equal(3, ((ProportionalDock)panel.DockModel).VisibleDockables!.Count);
        Assert.IsType<ProportionalDockSplitter>(((ProportionalDock)panel.DockModel).VisibleDockables![1]);
        panel.Children.RemoveAt(0);
        Assert.Single(((ProportionalDock)panel.DockModel).VisibleDockables!);
    }

    [AvaloniaFact]
    public void Selection_And_Active_Content_Follow_Dock()
    {
        object firstContent = new();
        object secondContent = new();
        LayoutDocument first = new() { Content = firstContent };
        LayoutDocument second = new() { Content = secondContent };
        LayoutDocumentPane pane = new(first);
        pane.Children.Add(second);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        manager.ActiveContent = secondContent;
        Assert.Same(second, manager.Layout.ActiveContent);
        Assert.Same(secondContent, manager.ActiveContent);
        Assert.True(second.IsActive);
        manager.DockFactory.SetFocusedDockable(manager.DockLayout, first.DockModel);
        Assert.Same(firstContent, manager.ActiveContent);
        pane.SelectedContentIndex = 1;
        Assert.Same(second, pane.SelectedContent);
        Assert.True(second.IsSelected);
    }

    [AvaloniaFact]
    public void Closing_Can_Be_Canceled_From_Content_And_Manager()
    {
        LayoutDocument document = new();
        LayoutDocumentPane pane = new(document);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        bool cancel = true;
        int closed = 0;
        manager.DocumentClosing += (_, args) => args.Cancel = cancel;
        manager.DocumentClosed += (_, args) => { Assert.Same(document, args.Document); closed++; };
        document.Close();
        Assert.Single(pane.Children);
        Assert.Equal(0, closed);
        cancel = false;
        document.Close();
        Assert.Empty(pane.Children);
        Assert.Null(document.Parent);
        Assert.Equal(1, closed);
    }

    [AvaloniaFact]
    public void Hiding_And_Restoring_Use_Docks_Hidden_Collection()
    {
        LayoutAnchorable tool = new() { Title = "Properties" };
        LayoutAnchorablePane pane = new(tool);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        tool.Hide();
        Assert.True(tool.IsHidden);
        Assert.Empty(pane.Children);
        Assert.Contains(tool.DockModel, ((RootDock)root.DockModel).HiddenDockables!);
        tool.Show();
        Assert.False(tool.IsHidden);
        Assert.Same(tool, Assert.Single(pane.Children));
    }

    [AvaloniaFact]
    public void Replacing_Layout_Detaches_Manager_And_Rejects_Sharing()
    {
        DockingManager manager = new();
        LayoutRoot old = manager.Layout;
        manager.Layout = new LayoutRoot();
        Assert.Null(old.Manager);
        Assert.Same(manager, manager.Layout.Manager);
        DockingManager second = new();
        Assert.Throws<InvalidOperationException>(() => second.Layout = manager.Layout);
    }
}
