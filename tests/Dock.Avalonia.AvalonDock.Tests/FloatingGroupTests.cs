using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingGroupTests
{
    [AvaloniaFact]
    public void Float_All_Retains_Tab_Indices_Selection_And_Docked_Dimensions()
    {
        LayoutAnchorable[] tools = { CreateTool("first"), CreateTool("second"), CreateTool("third") };
        LayoutAnchorablePane pane = new() { DockWidth = new GridLength(225), DockMinWidth = 170 };
        foreach (LayoutAnchorable tool in tools) pane.Children.Add(tool);
        LayoutRoot root = CreateRoot(pane);
        pane.SelectedContentIndex = 1;
        root.DockFactory.FloatAllDockables(tools[1].DockModel);

        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        LayoutAnchorablePane floatingPane = Assert.IsType<LayoutAnchorablePane>(window.SinglePane);
        Assert.Equal(tools, floatingPane.Children);
        Assert.Same(tools[1], floatingPane.SelectedContent);
        Assert.Same(tools[1], root.ActiveContent);
        Assert.Equal(pane.DockWidth, floatingPane.DockWidth);
        Assert.Equal(pane.DockMinWidth, floatingPane.DockMinWidth);
        for (int i = 0; i < tools.Length; i++)
        {
            Assert.Same(pane, tools[i].PreviousContainer);
            Assert.Equal(i, tools[i].PreviousContainerIndex);
        }
        foreach (LayoutAnchorable tool in tools) tool.Dock();
        Assert.Equal(tools, pane.Children);
        Assert.False(window.IsVisible);
        // Each Dock records its current floating tab index; undo in reverse.
        foreach (LayoutAnchorable tool in tools.Reverse()) tool.Float();
        Assert.Same(window, Assert.Single(root.FloatingWindows));
        Assert.Equal(tools, floatingPane.Children);
    }

    [AvaloniaFact]
    public void Moving_The_Whole_Native_Tool_Pane_Keeps_A_Return_Pane_In_The_Same_Split_Position()
    {
        LayoutAnchorable first = CreateTool("first");
        LayoutAnchorable second = CreateTool("second");
        LayoutAnchorablePane pane = new(first) { Name = "Properties", DockWidth = new GridLength(240), DockMinWidth = 150 };
        pane.Children.Add(second);
        pane.SelectedContentIndex = 1;
        LayoutRoot root = CreateRoot(pane);
        root.DockFactory.FloatDockable(pane.DockModel);

        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        Assert.Same(pane, window.SinglePane);
        LayoutAnchorablePane destination = Assert.IsType<LayoutAnchorablePane>(first.PreviousContainer);
        Assert.NotSame(pane, destination);
        Assert.Same(destination, root.RootPanel.Children[1]);
        Assert.Equal(pane.Name, destination.Name);
        Assert.Equal(pane.DockWidth, destination.DockWidth);
        Assert.Equal(pane.DockMinWidth, destination.DockMinWidth);
        Assert.Same(second, pane.SelectedContent);
        Assert.Same(second, root.ActiveContent);

        first.Dock();
        second.Dock();
        Assert.Equal(new[] { first, second }, destination.Children);
        Assert.Same(pane, first.PreviousContainer);
        Assert.False(window.IsVisible);
    }

    [AvaloniaFact]
    public void Floating_A_Typed_Group_Preserves_Its_Native_Identity_And_Nested_Return_Layout()
    {
        LayoutAnchorable first = CreateTool("first");
        LayoutAnchorable second = CreateTool("second");
        LayoutAnchorablePane firstPane = new(first) { Name = "First", DockHeight = new GridLength(3, GridUnitType.Star) };
        LayoutAnchorablePane secondPane = new(second) { Name = "Second", DockHeight = new GridLength(1, GridUnitType.Star) };
        LayoutAnchorablePaneGroup group = new(firstPane) { Orientation = Orientation.Vertical, DockWidth = new GridLength(280) };
        group.Children.Add(secondPane);
        LayoutRoot root = CreateRoot(group);
        root.ActiveContent = second;
        root.DockFactory.FloatDockable(group.DockModel);

        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        Assert.Same(group, window.RootPanel);
        Assert.Same(firstPane, first.Parent);
        Assert.Same(secondPane, second.Parent);
        Assert.Same(second, root.ActiveContent);
        LayoutAnchorablePaneGroup destination = Assert.IsType<LayoutAnchorablePaneGroup>(root.RootPanel.Children[1]);
        Assert.Equal(Orientation.Vertical, destination.Orientation);
        Assert.Equal(group.DockWidth, destination.DockWidth);
        Assert.Equal(2, destination.Children.Count);
        Assert.Same(destination.Children[0], first.PreviousContainer);
        Assert.Same(destination.Children[1], second.PreviousContainer);
        first.Dock();
        second.Dock();
        Assert.Same(destination.Children[0], first.Parent);
        Assert.Same(destination.Children[1], second.Parent);
        Assert.False(window.IsVisible);
    }

    [AvaloniaFact]
    public void Nested_Group_Return_Panes_Survive_Xml_And_Reuse_The_Floating_Window()
    {
        LayoutAnchorable first = CreateTool("first");
        LayoutAnchorable second = CreateTool("second");
        LayoutAnchorablePane firstPane = new(first) { Name = "First", DockHeight = new GridLength(3, GridUnitType.Star) };
        LayoutAnchorablePane secondPane = new(second) { Name = "Second", DockHeight = new GridLength(1, GridUnitType.Star) };
        LayoutAnchorablePaneGroup group = new(firstPane) { Orientation = Orientation.Vertical };
        group.Children.Add(secondPane);
        DockingManager manager = new() { Layout = CreateRoot(group) };
        Save(manager);
        manager.DockFactory.FloatDockable(group.DockModel);
        RoundTrip(manager);

        LayoutAnchorable[] tools = manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
        Assert.Equal(2, tools.Length);
        LayoutAnchorablePaneGroup destination = Assert.IsType<LayoutAnchorablePaneGroup>(manager.Layout.RootPanel.Children[1]);
        Assert.Equal(Orientation.Vertical, destination.Orientation);
        Assert.Equal(new[] { "First", "Second" }, destination.Children.Cast<LayoutAnchorablePane>().Select(p => p.Name));
        Assert.Equal(new[] { 3d, 1d }, destination.Children.Cast<LayoutAnchorablePane>().Select(p => p.DockHeight.Value));
        foreach (LayoutAnchorable tool in tools) tool.Dock();
        Assert.Same(destination.Children[0], tools[0].Parent);
        Assert.Same(destination.Children[1], tools[1].Parent);

        RoundTrip(manager);
        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(manager.Layout.FloatingWindows));
        tools = manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
        foreach (LayoutAnchorable tool in tools) tool.Float();
        Assert.Same(window, Assert.Single(manager.Layout.FloatingWindows));
        Assert.Equal(Orientation.Vertical, window.RootPanel!.Orientation);
        Assert.Same(window.RootPanel.Children[0], tools[0].Parent);
        Assert.Same(window.RootPanel.Children[1], tools[1].Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Non_Floatable_Tab_Prevents_The_Whole_Pane_From_Floating(bool movePane)
    {
        LayoutAnchorable first = CreateTool("first");
        LayoutAnchorable second = CreateTool("second");
        second.CanFloat = false;
        LayoutAnchorablePane pane = new(first);
        pane.Children.Add(second);
        LayoutRoot root = CreateRoot(pane);
        if (movePane) root.DockFactory.FloatDockable(pane.DockModel);
        else root.DockFactory.FloatAllDockables(first.DockModel);
        Assert.Empty(root.FloatingWindows);
        Assert.Equal(new[] { first, second }, pane.Children);
        Assert.Same(pane, root.RootPanel.Children[1]);
        Assert.Null(first.PreviousContainer);
    }

    [AvaloniaFact]
    public void Batch_Floating_After_An_Earlier_Save_Round_Trips_Both_Return_Locations()
    {
        LayoutAnchorable first = CreateTool("first");
        LayoutAnchorable second = CreateTool("second");
        LayoutAnchorablePane pane = new(first);
        pane.Children.Add(second);
        DockingManager manager = new() { Layout = CreateRoot(pane) };
        // Saving assigns persistent container identifiers before Dock copies a pane.
        Save(manager);
        manager.DockFactory.FloatAllDockables(first.DockModel);
        RoundTrip(manager);
        LayoutAnchorable[] restored = manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
        Assert.Equal(2, restored.Length);
        foreach (LayoutAnchorable tool in restored) tool.Dock();
        Assert.All(restored, tool => Assert.False(tool.IsFloating));
        RoundTrip(manager);
        LayoutFloatingWindow window = Assert.Single(manager.Layout.FloatingWindows);
        restored = manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
        Assert.Equal(2, restored.Length);
        Assert.All(restored, tool => Assert.False(tool.IsFloating));
        foreach (LayoutAnchorable tool in restored.Reverse())
        {
            Assert.Same(window, tool.PreviousContainer!.FindParent<LayoutFloatingWindow>());
            Assert.Same(manager.DockFactory, tool.DockModel.Factory);
            tool.Float();
            Assert.Same(window, tool.FindParent<LayoutFloatingWindow>());
        }
        Assert.Same(window, Assert.Single(manager.Layout.FloatingWindows));
        Assert.Equal(new[] { "first", "second" }, window.Descendents().OfType<LayoutAnchorable>().Select(x => x.ContentId));
    }

    [AvaloniaFact]
    public void Floating_All_Tabs_From_An_Existing_Window_Retains_The_Docked_Return_Pane()
    {
        LayoutAnchorable first = CreateTool("first");
        LayoutAnchorable second = CreateTool("second");
        LayoutAnchorablePane pane = new(first);
        pane.Children.Add(second);
        LayoutRoot root = CreateRoot(pane);
        root.DockFactory.FloatAllDockables(first.DockModel);
        LayoutFloatingWindow original = Assert.Single(root.FloatingWindows);
        root.DockFactory.FloatAllDockables(second.DockModel);
        LayoutFloatingWindow replacement = Assert.Single(root.FloatingWindows);
        Assert.NotSame(original, replacement);
        Assert.Null(original.Parent);
        Assert.Same(pane, first.PreviousContainer);
        Assert.Same(pane, second.PreviousContainer);
        Assert.Equal(0, first.PreviousContainerIndex);
        Assert.Equal(1, second.PreviousContainerIndex);
        first.Dock();
        second.Dock();
        Assert.Equal(new[] { first, second }, pane.Children);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Existing_Floating_Pane_Honors_Child_Capabilities(bool movePane)
    {
        LayoutAnchorable first = CreateTool("first");
        LayoutAnchorable second = CreateTool("second");
        LayoutAnchorablePane pane = new(first);
        pane.Children.Add(second);
        LayoutRoot root = CreateRoot(pane);
        root.DockFactory.FloatAllDockables(first.DockModel);
        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        LayoutAnchorablePane floatingPane = Assert.IsType<LayoutAnchorablePane>(window.SinglePane);
        second.CanFloat = false;

        if (movePane) root.DockFactory.FloatDockable(floatingPane.DockModel);
        else root.DockFactory.FloatAllDockables(first.DockModel);

        Assert.Same(window, Assert.Single(root.FloatingWindows));
        Assert.Equal(new[] { first, second }, floatingPane.Children);
        Assert.Same(pane, first.PreviousContainer);
        Assert.Same(pane, second.PreviousContainer);
    }

    [AvaloniaFact]
    public void Moving_An_Existing_Floating_Group_Retains_Selection_And_The_Docked_Return_Pane()
    {
        LayoutAnchorable first = CreateTool("first");
        LayoutAnchorable second = CreateTool("second");
        LayoutAnchorablePane pane = new(first);
        pane.Children.Add(second);
        pane.SelectedContentIndex = 1;
        LayoutRoot root = CreateRoot(pane);
        root.DockFactory.FloatDockable(pane.DockModel);
        LayoutAnchorableFloatingWindow original = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        LayoutAnchorablePane destination = Assert.IsType<LayoutAnchorablePane>(first.PreviousContainer);

        root.DockFactory.FloatDockable(original.RootPanel!.DockModel);

        LayoutAnchorableFloatingWindow replacement = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        Assert.NotSame(original, replacement);
        Assert.Null(original.Parent);
        Assert.Same(pane, replacement.SinglePane);
        Assert.Same(second, pane.SelectedContent);
        Assert.Same(second, root.ActiveContent);
        Assert.Same(destination, first.PreviousContainer);
        Assert.Same(destination, second.PreviousContainer);
        first.Dock();
        second.Dock();
        Assert.Equal(new[] { first, second }, destination.Children);
    }

    [AvaloniaFact]
    public void A_Floating_Tool_Batch_Renders_Both_Templates_And_Docks_Without_Closing_Contents()
    {
        SourceViewModel viewModel = new();
        viewModel.Tools.Add(new SourceContent("Second tool"));
        SourcesFixture view = new() { DataContext = viewModel };
        DockingManager manager = view.FindControl<DockingManager>("Manager")!;
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            LayoutAnchorable[] tools = manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
            LayoutAnchorablePane originalPane = (LayoutAnchorablePane)tools[0].Parent!;
            int closed = 0;
            foreach (LayoutAnchorable tool in tools) tool.Closed += (_, _) => closed++;
            manager.DockFactory.FloatAllDockables(tools[0].DockModel);
            Dispatcher.UIThread.RunJobs();
            HostWindow host = Assert.IsAssignableFrom<HostWindow>(Assert.Single(manager.Layout.FloatingWindows).DockWindow.Host);
            foreach (LayoutAnchorable tool in tools)
            {
                tool.IsSelected = true;
                Dispatcher.UIThread.RunJobs();
                Assert.Contains(host.GetVisualDescendants().OfType<TextBlock>(), text =>
                    ReferenceEquals(text.DataContext, tool.Content) && text.Classes.Contains("manager-template") && text.Bounds.Width > 0);
            }
            foreach (LayoutAnchorable tool in tools) tool.Dock();
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsVisible);
            Assert.Equal(tools, originalPane.Children);
            Assert.Equal(0, closed);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view.Close();
        }
    }

    private static LayoutAnchorable CreateTool(string id) => new() { ContentId = id, Content = new object() };
    private static LayoutRoot CreateRoot(ILayoutPanelElement pane)
    {
        LayoutPanel panel = new(new LayoutDocumentPane());
        panel.Children.Add(pane);
        return new LayoutRoot { RootPanel = panel };
    }
    private static string Save(DockingManager manager)
    {
        using StringWriter writer = new();
        new XmlLayoutSerializer(manager).Serialize(writer);
        return writer.ToString();
    }
    private static void RoundTrip(DockingManager manager)
    {
        XmlLayoutSerializer serializer = new(manager);
        serializer.Deserialize(new StringReader(Save(manager)));
    }
}
