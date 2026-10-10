using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingReturnTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Last_Tool_Returns_To_Its_Pane_And_Reuses_Its_Floating_Window(bool useNativeFactory)
    {
        LayoutAnchorable tool = new() { ContentId = "tool", CanClose = true };
        LayoutAnchorablePane pane = new(tool);
        LayoutRoot root = CreateRoot(pane);
        if (useNativeFactory) root.DockFactory.FloatDockable(tool.DockModel);
        else tool.Float();

        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        LayoutAnchorablePane floatingPane = Assert.IsType<LayoutAnchorablePane>(tool.Parent);
        Assert.Same(pane, tool.PreviousContainer);
        Assert.Same(root.RootPanel, pane.Parent);
        Assert.Equal(0, tool.PreviousContainerIndex);

        tool.Dock();
        Assert.Same(tool, Assert.Single(pane.Children));
        Assert.Same(pane, tool.Parent);
        Assert.False(tool.IsFloating);
        Assert.Same(tool, root.ActiveContent);
        Assert.Same(floatingPane, tool.PreviousContainer);
        Assert.Same(window, Assert.Single(root.FloatingWindows));
        Assert.False(window.IsVisible);

        tool.Float();
        Assert.Same(window, Assert.Single(root.FloatingWindows));
        Assert.Same(floatingPane, tool.Parent);
        Assert.Same(pane, tool.PreviousContainer);
        Assert.True(window.IsVisible);
        tool.Close();
        Assert.Empty(root.FloatingWindows);
        Assert.DoesNotContain(pane, root.RootPanel.Children);
        Assert.Null(tool.Parent);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Native_Float_And_Dock_Restore_The_Original_Tab_Index(int index)
    {
        LayoutAnchorable[] tools = { new(), new(), new() };
        LayoutAnchorablePane pane = new();
        foreach (LayoutAnchorable tool in tools) pane.Children.Add(tool);
        LayoutRoot root = CreateRoot(pane);
        root.DockFactory.FloatDockable(tools[index].DockModel);
        Assert.Equal(index, tools[index].PreviousContainerIndex);
        tools[index].Dock();
        Assert.Equal(tools, pane.Children);
        Assert.Same(tools[index], pane.SelectedContent);
        Assert.Same(tools[index], root.ActiveContent);
    }

    [AvaloniaFact]
    public void Native_Docking_And_Docked_Tab_Moves_Preserve_The_Floating_Return_Location()
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane original = new(tool);
        LayoutAnchorablePane destination = new(new LayoutAnchorable());
        LayoutRoot root = CreateRoot(original);
        root.RootPanel.Children.Add(destination);
        root.DockFactory.FloatDockable(tool.DockModel);
        LayoutAnchorablePane floatingPane = (LayoutAnchorablePane)tool.Parent!;
        LayoutFloatingWindow window = Assert.Single(root.FloatingWindows);

        root.DockFactory.MoveDockable((IDock)tool.DockModel.Owner!, (IDock)destination.DockModel, tool.DockModel, null);
        Assert.Same(destination, tool.Parent);
        Assert.Same(floatingPane, tool.PreviousContainer);
        Assert.DoesNotContain(original, root.RootPanel.Children);
        LayoutAnchorablePane another = new(new LayoutAnchorable());
        root.RootPanel.Children.Add(another);
        root.DockFactory.MoveDockable((IDock)destination.DockModel, (IDock)another.DockModel, tool.DockModel, null);
        Assert.Same(floatingPane, tool.PreviousContainer);

        tool.Float();
        Assert.Same(window, Assert.Single(root.FloatingWindows));
        Assert.Same(floatingPane, tool.Parent);
        Assert.Same(another, tool.PreviousContainer);
    }

    [AvaloniaFact]
    public void Both_Docked_And_Floating_Return_Locations_Survive_Xml()
    {
        LayoutAnchorable tool = new() { ContentId = "tool", Content = new object() };
        LayoutAnchorablePane pane = new(tool) { Name = "Original" };
        DockingManager manager = new() { Layout = CreateRoot(pane) };
        tool.Float();
        RoundTrip(manager);
        tool = Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());
        pane = Assert.IsType<LayoutAnchorablePane>(tool.PreviousContainer);
        Assert.Equal("Original", pane.Name);
        Assert.Same(manager.Layout, pane.Root);
        tool.Dock();
        Assert.Same(pane, tool.Parent);

        RoundTrip(manager);
        tool = Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());
        LayoutFloatingWindow window = Assert.Single(manager.Layout.FloatingWindows);
        Assert.Same(window, tool.PreviousContainer!.FindParent<LayoutFloatingWindow>());
        tool.Float();
        Assert.Same(window, Assert.Single(manager.Layout.FloatingWindows));
        Assert.Equal("Original", Assert.IsType<LayoutAnchorablePane>(tool.PreviousContainer).Name);
    }

    [AvaloniaFact]
    public void Declared_Floating_Content_Docks_Through_The_Protected_Fallback_And_Strategy()
    {
        DockingManager manager = new();
        DockStrategy strategy = new();
        manager.LayoutUpdateStrategy = strategy;
        FallbackDocument document = new();
        manager.Layout.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = document });
        document.Dock();
        Assert.Equal(1, document.FallbackCalls);
        Assert.Same(manager.Layout.RootPanel.Children[0], document.Parent);
        Assert.Same(document, manager.Layout.ActiveContent);
        Assert.Equal(1, strategy.DocumentsInserted);

        LayoutAnchorable tool = new();
        manager.Layout.FloatingWindows.Add(new LayoutAnchorableFloatingWindow
        {
            RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(tool))
        });
        tool.Dock();
        Assert.Same(document.Parent, tool.Parent);
        Assert.Same(tool, manager.Layout.ActiveContent);
        Assert.Equal(1, strategy.ToolsInserted);
        Assert.Empty(manager.Layout.FloatingWindows);
    }

    [AvaloniaFact]
    public void Reused_Tool_Host_Reopens_And_Content_Close_Honors_Cancellation()
    {
        SourcesFixture view = new() { DataContext = new SourceViewModel() };
        DockingManager manager = view.FindControl<DockingManager>("Manager")!;
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            LayoutAnchorable tool = Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());
            tool.CanClose = true;
            tool.Float();
            Dispatcher.UIThread.RunJobs();
            LayoutFloatingWindow window = Assert.Single(manager.Layout.FloatingWindows);
            HostWindow firstHost = Assert.IsAssignableFrom<HostWindow>(window.DockWindow.Host);
            Assert.True(firstHost.IsVisible);
            tool.Dock();
            Dispatcher.UIThread.RunJobs();
            Assert.False(firstHost.IsVisible);
            Assert.Null(window.DockWindow.Host);
            tool.Float();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(window, Assert.Single(manager.Layout.FloatingWindows));
            HostWindow secondHost = Assert.IsAssignableFrom<HostWindow>(window.DockWindow.Host);
            Assert.NotSame(firstHost, secondHost);
            Assert.True(secondHost.IsVisible);

            int closed = 0;
            tool.Closed += (_, _) => closed++;
            EventHandler<CancelEventArgs> cancel = (_, args) => args.Cancel = true;
            tool.Closing += cancel;
            tool.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.True(secondHost.IsVisible);
            Assert.Same(manager.Layout, tool.Root);
            Assert.Equal(0, closed);
            tool.Closing -= cancel;
            tool.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.False(secondHost.IsVisible);
            Assert.Empty(manager.Layout.FloatingWindows);
            Assert.Null(tool.Parent);
            Assert.Equal(1, closed);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view.Close();
        }
    }

    private static LayoutRoot CreateRoot(LayoutAnchorablePane tools)
    {
        LayoutPanel panel = new(new LayoutDocumentPane());
        panel.Children.Add(tools);
        return new LayoutRoot { RootPanel = panel };
    }

    private static void RoundTrip(DockingManager manager)
    {
        XmlLayoutSerializer serializer = new(manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
    }

    private sealed class FallbackDocument : LayoutDocument
    {
        internal int FallbackCalls { get; private set; }
        protected override void InternalDock()
        {
            FallbackCalls++;
            base.InternalDock();
        }
    }

    private sealed class DockStrategy : ILayoutUpdateStrategy
    {
        internal int ToolsInserted { get; private set; }
        internal int DocumentsInserted { get; private set; }
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool, ILayoutContainer? destination) => false;
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool) => ToolsInserted++;
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument document, ILayoutContainer? destination) => false;
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument document) => DocumentsInserted++;
    }
}
