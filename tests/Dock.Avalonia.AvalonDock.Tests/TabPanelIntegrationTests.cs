using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabPanelTests
{
    [AvaloniaFact]
    public void Switching_To_A_Native_Vertical_Strip_Releases_Horizontal_Overflow_State()
    {
        using PanelWorkspace workspace = new(false);
        DocumentTabStrip strip = workspace.Manager.GetVisualDescendants().OfType<DocumentTabStrip>().Single();
        DocumentPaneTabPanel previous = strip.GetVisualDescendants().OfType<DocumentPaneTabPanel>().Single();
        Control hidden = previous.Children.Last();
        Assert.Equal(0, hidden.Opacity);
        strip.Orientation = global::Avalonia.Layout.Orientation.Vertical;
        workspace.Update();
        Assert.Empty(strip.GetVisualDescendants().OfType<DocumentPaneTabPanel>());
        Assert.Equal(1, hidden.Opacity);
        Assert.True(hidden.IsHitTestVisible);
        strip.Orientation = global::Avalonia.Layout.Orientation.Horizontal;
        workspace.Update();
        DocumentPaneTabPanel current = strip.GetVisualDescendants().OfType<DocumentPaneTabPanel>().Single();
        Assert.Equal(0, current.Children.Last().Opacity);
    }

    [AvaloniaFact]
    public void The_Native_Document_List_Activates_And_Reveals_An_Overflowed_Tab()
    {
        using PanelWorkspace workspace = new(false);
        DocumentPaneTabPanel panel = workspace.Manager.GetVisualDescendants().OfType<DocumentPaneTabPanel>().Single();
        LayoutContent last = workspace.Pane.Children.Cast<LayoutContent>().Last();
        DocumentTabStripItem hidden = panel.Children.OfType<DocumentTabStripItem>().Single(tab => ReferenceEquals(tab.DataContext, last.DockModel));
        Assert.Equal(0, hidden.Opacity);
        Assert.False(hidden.Focus());
        Button button = workspace.Manager.GetVisualDescendants().OfType<Button>().Single(control => control.Classes.Contains("avalondock-document-menu"));
        ClickPanelControl(button);
        MenuFlyout menu = (MenuFlyout)button.Flyout!;
        MenuFlyoutPresenter presenter = (MenuFlyoutPresenter)menu.Popup.Child!;
        MenuItem entry = presenter.GetRealizedContainers().OfType<MenuItem>().Single(item => ReferenceEquals(item.DataContext, last));
        ClickPanelControl(entry);
        workspace.Update();
        Assert.Same(last, workspace.Pane.Children.First());
        Assert.True(last.IsActive);
        Assert.True(last.IsSelected);
        Assert.Same(last.DockModel, panel.Children[0].DataContext);
        Assert.Equal(1, panel.Children[0].Opacity);
        Assert.True(panel.Children[0].IsEnabled);
        Assert.False(menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Tool_Tabs_Use_The_Public_Compressing_Panel_And_Clip_Their_Own_Headers(bool floating)
    {
        using PanelWorkspace workspace = new(true);
        if (floating)
        {
            workspace.Manager.DockFactory.FloatAllDockables(workspace.Pane.Children.Cast<LayoutContent>().First().DockModel);
            workspace.Manager.FloatingWindows.Single().Width = 220;
            workspace.Update();
        }
        Control scope = floating ? workspace.Manager.FloatingWindows.Single() : workspace.Manager;
        AnchorablePaneTabPanel panel = scope.GetVisualDescendants().OfType<AnchorablePaneTabPanel>().Single();
        ToolTabStripItem[] tabs = panel.Children.OfType<ToolTabStripItem>().ToArray();
        Assert.Equal(3, tabs.Length);
        Assert.All(tabs, tab => Assert.True(tab.ClipToBounds));
        Assert.All(tabs, tab => Assert.True(tab.Bounds.Width <= panel.Bounds.Width / 3 + 1));
        ClickPanelControl(tabs[2]);
        Assert.Same(tabs[2].DataContext, workspace.Manager.Layout.ActiveContent!.DockModel);
    }

    [AvaloniaFact]
    public void A_Floating_Document_Uses_The_Public_Panel_And_Restores_Its_Header_After_Expansion()
    {
        using PanelWorkspace workspace = new(false);
        LayoutContent first = workspace.Pane.Children.Cast<LayoutContent>().First();
        first.Float();
        LayoutFloatingWindowControl floating = workspace.Manager.FloatingWindows.Single();
        floating.Width = 110;
        workspace.Update();
        DocumentPaneTabPanel panel = floating.GetVisualDescendants().OfType<DocumentPaneTabPanel>().Single();
        Assert.Equal(0, panel.Children.Single().Opacity);
        floating.Width = 600;
        workspace.Update();
        Assert.Equal(1, panel.Children.Single().Opacity);
        Assert.True(panel.Children.Single().IsEnabled);
        Assert.Same(first.DockModel, panel.Children.Single().DataContext);
    }

    [AvaloniaFact]
    public void A_Native_Drag_Reorders_Only_The_Visible_Tab_Targets()
    {
        using PanelWorkspace workspace = new(false);
        workspace.Window.Width = 400;
        workspace.Update();
        DocumentPaneTabPanel panel = workspace.Manager.GetVisualDescendants().OfType<DocumentPaneTabPanel>().Single();
        Assert.Equal(new[] { 1d, 1d, 0d }, panel.Children.Select(control => control.Opacity));
        LayoutContent[] before = workspace.Pane.Children.Cast<LayoutContent>().ToArray();
        Control first = panel.Children[0];
        Control second = panel.Children[1];
        Point start = first.TranslatePoint(new Point(first.Bounds.Width / 2, first.Bounds.Height / 2), workspace.Window)!.Value;
        Point end = second.TranslatePoint(new Point(second.Bounds.Width - 2, second.Bounds.Height / 2), workspace.Window)!.Value;
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(end, MouseButton.Left);
        workspace.Update();
        Assert.Equal(new[] { before[1], before[0], before[2] }, workspace.Pane.Children);
    }

    [AvaloniaFact]
    public void A_Hidden_Header_Still_Responds_To_Content_Measure_Changes()
    {
        DocumentPaneTabPanel panel = new();
        Border first = new() { Width = 70, Height = 20 }, second = new() { Width = 70, Height = 20 };
        panel.Children.Add(first); panel.Children.Add(second);
        Window window = new() { Content = panel, Width = 100, Height = 30 };
        try
        {
            window.Show(); window.UpdateLayout();
            Assert.Equal(0, second.Opacity);
            second.Width = 20;
            window.UpdateLayout();
            Assert.Equal(1, second.Opacity);
            Assert.Equal(20, second.Bounds.Width);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Replacing_A_Header_Model_Detaches_The_Previous_Selection_Subscription(bool direct)
    {
        LayoutDocument previous = new(), replacement = new();
        DocumentPaneTabPanel panel = new();
        Control child = direct ? new LayoutDocumentTabItem { Model = previous } : new TabItem { Content = previous };
        panel.Children.Add(child);
        panel.Measure(new Size(100, 30)); panel.Arrange(new Rect(0, 0, 100, 30));
        if (direct) ((LayoutDocumentTabItem)child).Model = replacement;
        else ((TabItem)child).Content = replacement;
        panel.Measure(new Size(100, 30)); panel.Arrange(new Rect(0, 0, 100, 30));
        Assert.True(panel.IsArrangeValid);
        previous.IsSelected = true;
        Assert.True(panel.IsArrangeValid);
        replacement.IsSelected = true;
        Assert.False(panel.IsArrangeValid);
    }

    [AvaloniaTheory]
    [InlineData("opacity", false)]
    [InlineData("input", false)]
    [InlineData("enabled", false)]
    [InlineData("opacity", true)]
    [InlineData("input", true)]
    [InlineData("enabled", true)]
    public void Removing_A_Tab_Or_Detaching_The_Panel_During_Suppression_Restores_Properties(string property, bool detach)
    {
        DocumentPaneTabPanel panel = new();
        Border child = new() { Width = 100, Height = 20, Opacity = .8 };
        panel.Children.Add(child);
        Window? window = detach ? new Window { Content = panel, Width = 50, Height = 30 } : null;
        child.PropertyChanged += (_, args) =>
        {
            if (property == "opacity" && args.Property == Visual.OpacityProperty && child.Opacity == 0 ||
                property == "input" && args.Property == InputElement.IsHitTestVisibleProperty && !child.IsHitTestVisible ||
                property == "enabled" && args.Property == InputElement.IsEnabledProperty && !child.IsEnabled)
            {
                if (detach) window!.Content = null;
                else panel.Children.Remove(child);
            }
        };
        try
        {
            if (detach) { window!.Show(); window.UpdateLayout(); Assert.Null(window.Content); }
            else { panel.Measure(new Size(50, 30)); panel.Arrange(new Rect(0, 0, 50, 30)); Assert.Empty(panel.Children); }
            Assert.Equal(.8, child.Opacity);
            Assert.True(child.IsHitTestVisible);
            Assert.True(child.IsEnabled);
        }
        finally { window?.Close(); }
    }

    private static void ClickPanelControl(Control control)
    {
        TopLevel top = TopLevel.GetTopLevel(control)!;
        top.UpdateLayout();
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), top)!.Value;
        top.MouseMove(point); top.MouseDown(point, MouseButton.Left); top.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class PanelWorkspace : IDisposable
    {
        public Window Window { get; }
        public DockingManager Manager { get; }
        public ILayoutPane Pane { get; }

        public PanelWorkspace(bool tool)
        {
            LayoutContent[] contents = new LayoutContent[3];
            for (int i = 0; i < contents.Length; i++)
            {
                contents[i] = tool ? new LayoutAnchorable() : new LayoutDocument();
                contents[i].Title = "Header " + i;
            }
            if (tool)
            {
                LayoutAnchorablePane tools = new((LayoutAnchorable)contents[0]);
                tools.Children.Add((LayoutAnchorable)contents[1]); tools.Children.Add((LayoutAnchorable)contents[2]);
                Pane = tools;
            }
            else
            {
                LayoutDocumentPane documents = new(contents[0]);
                documents.Children.Add(contents[1]); documents.Children.Add(contents[2]);
                Pane = documents;
            }
            LayoutPanel root = new();
            root.Children.Add((ILayoutPanelElement)Pane);
            Manager = new() { Layout = new() { RootPanel = root } };
            IDataTemplate header = new FuncDataTemplate<LayoutContent>((content, _) => new TextBlock { Text = content!.Title, Width = 125, Height = 20 });
            Manager.DocumentHeaderTemplate = header;
            Manager.AnchorableHeaderTemplate = header;
            Window = new() { Content = Manager, Width = 300, Height = 250 };
            Window.Show(); Update();
        }

        public void Update()
        {
            Dispatcher.UIThread.RunJobs(); Window.UpdateLayout();
            foreach (LayoutFloatingWindowControl floating in Manager.FloatingWindows) floating.UpdateLayout();
        }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
