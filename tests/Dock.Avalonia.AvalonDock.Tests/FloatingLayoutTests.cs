using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingLayoutTests
{
    [AvaloniaFact]
    public void Document_Window_Uses_Native_Root_And_Pane_With_Public_Document_Parent()
    {
        LayoutDocument document = new() { ContentId = "floating" };
        LayoutDocumentPane previous = new(document);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(previous) };
        LayoutDocumentFloatingWindow window = new() { RootDocument = document };
        root.FloatingWindows.Add(window);

        Assert.Same(window.DockWindow, Assert.Single(((RootDock)root.DockModel).Windows!));
        RootDock nativeRoot = Assert.IsType<RootDock>(window.DockWindow.Layout);
        DocumentDock nativePane = Assert.IsType<DocumentDock>(Assert.Single(nativeRoot.VisibleDockables!));
        Assert.Same(document.DockModel, Assert.Single(nativePane.VisibleDockables!));
        Assert.Same(nativePane, document.DockModel.Owner);
        Assert.Same(window, document.Parent);
        Assert.Same(root, document.Root);
        Assert.Same(root.DockFactory, document.DockModel.Factory);
        Assert.Same(previous, document.PreviousContainer);
        Assert.Empty(previous.Children);
        Assert.True(document.IsFloating);
        Assert.True(window.IsValid);
        Assert.Equal(1, window.ChildrenCount);
        Assert.Same(document, Assert.Single(window.Children));
        Assert.Contains(window, root.Children);
        Assert.Equal(root.Children.Count(), root.ChildrenCount);
    }

    [AvaloniaFact]
    public void Document_Replacement_And_Native_Changes_Preserve_Wrappers_And_Notify()
    {
        LayoutDocument first = new();
        LayoutDocument second = new();
        LayoutRoot root = new();
        LayoutDocumentFloatingWindow window = new() { RootDocument = first };
        root.FloatingWindows.Add(window);
        int changed = 0;
        List<string?> properties = new();
        window.RootDocumentChanged += (_, _) => changed++;
        window.PropertyChanged += (_, args) => properties.Add(args.PropertyName);

        window.ReplaceChild(first, second);
        Assert.Null(first.Parent);
        Assert.Same(window, second.Parent);
        Assert.Equal(1, changed);
        Assert.Contains(nameof(LayoutDocumentFloatingWindow.RootDocument), properties);
        DocumentDock nativePane = (DocumentDock)second.DockModel.Owner!;
        Document nativeDocument = new() { Title = "Native replacement" };
        nativePane.VisibleDockables = new AvaloniaList<IDockable>(nativeDocument);
        root.DockFactory.InitDockable(nativeDocument, nativePane);
        Assert.Same(nativeDocument, window.RootDocument!.DockModel);
        Assert.Same(window, window.RootDocument.Parent);
        Assert.Same(root, window.RootDocument.Root);
        Assert.Equal(2, changed);

        window.RemoveChild(window.RootDocument);
        Assert.Null(window.RootDocument);
        Assert.Empty(window.Children);
        Assert.Equal(0, window.ChildrenCount);
        Assert.False(window.IsValid);
        Assert.Equal(3, changed);
    }

    [AvaloniaFact]
    public void Native_Float_And_Return_Update_Public_Window_Collection()
    {
        LayoutDocument document = new();
        LayoutDocumentPane pane = new(document);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        document.Float();
        LayoutDocumentFloatingWindow window = Assert.IsType<LayoutDocumentFloatingWindow>(Assert.Single(root.FloatingWindows));
        Assert.Same(document, window.RootDocument);
        Assert.Same(window, document.Parent);
        Assert.Same(root, document.Root);
        Assert.Same(document, root.ActiveContent);

        document.Dock();
        Assert.Empty(root.FloatingWindows);
        Assert.Same(document, Assert.Single(pane.Children));
        Assert.Same(pane, document.Parent);
        Assert.False(document.IsFloating);
    }

    [AvaloniaFact]
    public void Native_Tool_Window_Exposes_The_Existing_Content_Through_Typed_Groups()
    {
        LayoutAnchorable tool = new() { Title = "Tool" };
        LayoutAnchorablePane source = new(tool);
        LayoutPanel panel = new(new LayoutDocumentPane());
        panel.Children.Add(source);
        LayoutRoot root = new() { RootPanel = panel };
        root.DockFactory.FloatDockable(tool.DockModel);

        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        LayoutAnchorablePane pane = Assert.IsType<LayoutAnchorablePane>(Assert.Single(window.RootPanel!.Children));
        Assert.Same(tool, Assert.Single(pane.Children));
        Assert.Same(window.RootPanel, pane.Parent);
        Assert.Same(window, window.RootPanel.Parent);
        Assert.Same(root, tool.Root);
        Assert.True(tool.IsFloating);
        Assert.True(window.IsVisible);
        Assert.True(window.IsSinglePane);
        Assert.Same(pane, window.SinglePane);
        Assert.Same(tool, root.ActiveContent);
    }

    [AvaloniaFact]
    public void Tool_Window_Visibility_And_Single_Pane_Follow_Nested_Collection_Changes()
    {
        LayoutAnchorablePane first = new(new LayoutAnchorable());
        LayoutAnchorablePane second = new(new LayoutAnchorable());
        LayoutAnchorablePaneGroup nested = new(first);
        LayoutAnchorablePaneGroup panel = new();
        panel.Children.Add(nested);
        LayoutAnchorableFloatingWindow window = new() { RootPanel = panel };
        int visibilityChanges = 0;
        window.IsVisibleChanged += (_, _) => visibilityChanges++;
        Assert.True(window.IsSinglePane);
        Assert.Same(first, window.SinglePane);

        nested.Children.Add(second);
        Assert.False(window.IsSinglePane);
        Assert.Null(window.SinglePane);
        Assert.True(window.IsVisible);
        second.Children.Clear();
        Assert.True(window.IsSinglePane);
        Assert.Same(first, window.SinglePane);
        first.Children.Clear();
        Assert.False(window.IsVisible);
        Assert.Equal(1, visibilityChanges);
        first.Children.Add(new LayoutAnchorable());
        Assert.True(window.IsVisible);
        Assert.Equal(2, visibilityChanges);
        ((ILayoutElementWithVisibility)window).ComputeVisibility();
        Assert.Equal(2, visibilityChanges);

        LayoutAnchorablePaneGroup replacement = new(new LayoutAnchorablePane(new LayoutAnchorable()));
        window.ReplaceChild(panel, replacement);
        Assert.Null(panel.Parent);
        Assert.Same(window, replacement.Parent);
        window.RemoveChild(replacement);
        Assert.Null(replacement.Parent);
        Assert.False(window.IsValid);
        Assert.False(window.IsVisible);
        Assert.Empty(window.Children);
    }

    [AvaloniaFact]
    public void Floating_Collection_Move_Replace_Detach_And_Reattach_Preserve_Contents()
    {
        LayoutRoot root = new();
        LayoutDocument document = new();
        LayoutDocumentFloatingWindow first = new() { RootDocument = document };
        LayoutDocumentFloatingWindow second = new() { RootDocument = new LayoutDocument() };
        root.FloatingWindows.Add(first);
        root.FloatingWindows.Add(second);
        root.FloatingWindows.Move(0, 1);
        Assert.Same(first.DockWindow, ((RootDock)root.DockModel).Windows![1]);
        Assert.Throws<InvalidOperationException>(() => root.FloatingWindows.Add(first));
        Assert.Throws<ArgumentNullException>(() => root.FloatingWindows[0] = null!);
        Assert.Same(second, root.FloatingWindows[0]);

        LayoutDocumentFloatingWindow replacement = new() { RootDocument = new LayoutDocument() };
        root.ReplaceChild(second, replacement);
        Assert.Null(second.Parent);
        root.RemoveChild(first);
        Assert.Null(first.Parent);
        Assert.Null(document.Root);
        Assert.Same(first, document.Parent);
        Assert.Same(document, first.RootDocument);

        root.FloatingWindows.Add(first);
        Assert.Same(root, document.Root);
        Assert.Same(first.DockModel, first.DockWindow.Layout);
        root.FloatingWindows.Clear();
        Assert.Empty(((RootDock)root.DockModel).Windows!);
        Assert.Null(first.Parent);
        Assert.Null(replacement.Parent);
    }

    [AvaloniaFact]
    public void Native_Window_List_Replacement_Updates_Public_Collection()
    {
        LayoutRoot root = new();
        LayoutDocumentFloatingWindow first = new() { RootDocument = new LayoutDocument() };
        LayoutDocumentFloatingWindow second = new() { RootDocument = new LayoutDocument() };
        root.FloatingWindows.Add(first);
        root.FloatingWindows.Add(second);
        ((RootDock)root.DockModel).Windows = new AvaloniaList<IDockWindow>(second.DockWindow);
        Assert.Same(second, Assert.Single(root.FloatingWindows));
        Assert.Null(first.Parent);
        Assert.Same(root, second.Parent);
    }

    [AvaloniaFact]
    public void Compiled_Floating_Layout_Renders_In_Native_Host_And_Detaches_Without_Closing_Content()
    {
        FloatingFixture window = new();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        LayoutDocumentFloatingWindow floating = Assert.IsType<LayoutDocumentFloatingWindow>(Assert.Single(manager.Layout.FloatingWindows));
        LayoutDocument document = floating.RootDocument!;
        int closed = 0;
        document.Closed += (_, _) => closed++;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, closed);
            Assert.Contains(floating, manager.Layout.FloatingWindows);
            HostWindow host = Assert.IsAssignableFrom<HostWindow>(floating.DockWindow.Host);
            Assert.True(host.IsVisible);
            Assert.Contains(host.GetVisualDescendants().OfType<TextBox>(), x => x.Text == "Floating editor" && x.Bounds.Width > 0);
            manager.Layout.RootPanel = new LayoutPanel(new LayoutDocumentPane());
            Assert.Same(host, floating.DockWindow.Host);
            Assert.True(host.IsVisible);
            manager.Layout.FloatingWindows.Remove(floating);
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsVisible);
            Assert.Equal(0, closed);
            Assert.Same(document, floating.RootDocument);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Detaching_And_Reattaching_Manager_Reopens_Host_With_The_Same_Document()
    {
        FloatingFixture window = new();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        LayoutDocumentFloatingWindow floating = (LayoutDocumentFloatingWindow)manager.Layout.FloatingWindows[0];
        LayoutDocument document = floating.RootDocument!;
        int closed = 0;
        document.Closed += (_, _) => closed++;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            HostWindow originalHost = Assert.IsAssignableFrom<HostWindow>(floating.DockWindow.Host);
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            Assert.False(originalHost.IsVisible);
            Assert.Null(floating.DockWindow.Host);
            Assert.Same(document, floating.RootDocument);
            Assert.Equal(0, closed);

            window.Content = manager;
            Dispatcher.UIThread.RunJobs();
            HostWindow newHost = Assert.IsAssignableFrom<HostWindow>(floating.DockWindow.Host);
            Assert.NotSame(originalHost, newHost);
            Assert.True(newHost.IsVisible);
            Assert.Same(document, floating.RootDocument);
            Assert.Same(manager.Layout, document.Root);
            Assert.Equal(0, closed);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Layout_Replacement_Closes_Old_Hosts_And_Runtime_Addition_Presents_New_Ones()
    {
        FloatingFixture window = new();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        LayoutRoot previous = manager.Layout;
        LayoutDocumentFloatingWindow oldFloating = (LayoutDocumentFloatingWindow)previous.FloatingWindows[0];
        LayoutDocument document = oldFloating.RootDocument!;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            HostWindow oldHost = Assert.IsAssignableFrom<HostWindow>(oldFloating.DockWindow.Host);
            LayoutRoot replacement = new();
            LayoutDocumentFloatingWindow newFloating = new() { RootDocument = new LayoutDocument { Content = new TextBox { Text = "New editor" } } };
            replacement.FloatingWindows.Add(newFloating);
            manager.Layout = replacement;
            Dispatcher.UIThread.RunJobs();
            Assert.False(oldHost.IsVisible);
            Assert.Same(document, oldFloating.RootDocument);
            Assert.Null(previous.Manager);
            Assert.Same(previous, document.Root);
            Assert.True(Assert.IsAssignableFrom<HostWindow>(newFloating.DockWindow.Host).IsVisible);

            LayoutAnchorableFloatingWindow added = new()
            {
                RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable
                {
                    Content = new TextBlock { Text = "New tool" }
                }))
            };
            manager.Layout.FloatingWindows.Add(added);
            Dispatcher.UIThread.RunJobs();
            HostWindow toolHost = Assert.IsAssignableFrom<HostWindow>(added.DockWindow.Host);
            Assert.True(toolHost.IsVisible);
            Assert.Contains(toolHost.GetVisualDescendants().OfType<TextBlock>(), x => x.Text == "New tool" && x.Bounds.Width > 0);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            previous.FloatingWindows.Clear();
            window.Close();
        }
    }
}
