using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;

namespace PackageConsumer;

public sealed class DocumentPanelProbe : DocumentPaneTabPanel
{
    public int Measures { get; private set; }
    public int Arranges { get; private set; }
    public int Exits { get; private set; }
    protected override Size MeasureOverride(Size availableSize) { Measures++; return base.MeasureOverride(availableSize); }
    protected override Size ArrangeOverride(Size finalSize) { Arranges++; return base.ArrangeOverride(finalSize); }
    protected override void OnMouseLeave(PointerEventArgs e) { Exits++; base.OnMouseLeave(e); }
}

public sealed class ToolPanelProbe : AnchorablePaneTabPanel
{
    public int Measures { get; private set; }
    public int Arranges { get; private set; }
    protected override Size MeasureOverride(Size availableSize) { Measures++; return base.MeasureOverride(availableSize); }
    protected override Size ArrangeOverride(Size finalSize) { Arranges++; return base.ArrangeOverride(finalSize); }
}

internal static class TabPanelProbe
{
    internal static void Verify()
    {
        ConsumerWindow resources = new();
        foreach (string key in new[] { "DocumentPanel", "LegacyDocumentPanel", "ToolPanel", "LegacyToolPanel" })
        {
            Panel panel = ((ITemplate<Panel>)resources.Resources[key]!).Build();
            if ((key.Contains("Document") ? panel is not DocumentPaneTabPanel : panel is not AnchorablePaneTabPanel) ||
                panel.FlowDirection != FlowDirection.LeftToRight)
                throw new Exception("Both XML namespaces must construct the public tab panels with left-to-right flow.");
        }
        ToolPanelProbe tools = new();
        tools.Children.Add(new NaturalHeader(30));
        tools.Children.Add(new NaturalHeader(90));
        tools.Measure(new Size(60, 30)); tools.Arrange(new Rect(0, 0, 60, 30));
        if (tools.Children[0].Bounds.Width != 30 || tools.Children[1].Bounds.X != 30 || tools.Children[1].Bounds.Width != 30 ||
            tools.Measures == 0 || tools.Arranges == 0)
            throw new Exception("The packaged tool panel did not compress headers through its external sizing hooks.");
        VerifyOverflowLayers();
        VerifySelection(resources);
    }

    private static void VerifyOverflowLayers()
    {
        DocumentPanelProbe panel = new() { Background = Brushes.Transparent };
        Border source = new() { Opacity = .6 };
        Button hidden = new() { Content = "Second", Width = 80, Height = 20 };
        hidden.Bind(Visual.OpacityProperty, source.GetObservable(Visual.OpacityProperty));
        panel.Children.Add(new Border { Width = 80, Height = 20 });
        panel.Children.Add(hidden);
        Window window = new() { Content = panel, Width = 100, Height = 40 };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            if (hidden.Opacity != 0 || hidden.IsHitTestVisible || hidden.Focus())
                throw new Exception("The packaged document panel did not suppress overflow rendering and input.");
            source.Opacity = .75;
            window.Width = 220;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            if (hidden.Opacity != .75 || !hidden.IsHitTestVisible || !hidden.IsEnabled)
                throw new Exception($"Expanding the packaged document panel did not restore its binding: width={panel.Bounds.Width}, opacity={hidden.Opacity}, input={hidden.IsHitTestVisible}, enabled={hidden.IsEnabled}.");
            window.MouseMove(new Point(10, 10));
            window.MouseMove(new Point(250, 70));
            if (panel.Measures == 0 || panel.Arranges == 0 || panel.Exits == 0)
                throw new Exception("The external document panel hooks were not invoked.");
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static void VerifySelection(ConsumerWindow resources)
    {
        LayoutDocument first = new() { ContentId = "first", Title = "First" };
        LayoutDocument second = new() { ContentId = "second", Title = "Second" };
        LayoutDocument last = new() { ContentId = "last", Title = "Last" };
        LayoutDocumentPane pane = new(first);
        pane.Children.Add(second); pane.Children.Add(last);
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
        first.IsActive = true;
        ItemsControl row = (ItemsControl)((IDataTemplate)resources.Resources["OverflowDocumentTabs"]!).Build(pane)!;
        row.DataContext = pane;
        Window window = new() { Width = 150, Height = 60 };
        window.Styles.Add(new AvalonDockTheme());
        window.Content = row;
        try
        {
            window.Show(); Update();
            DocumentPaneTabPanel panel = row.GetVisualDescendants().OfType<DocumentPaneTabPanel>().Single();
            if (panel.Children[2].Opacity != 0) throw new Exception("The compiled packaged row did not overflow.");
            last.IsSelected = true;
            Update();
            if (!pane.Children.SequenceEqual(new[] { last, first, second }) || pane.SelectedContentIndex != 0 ||
                !ReferenceEquals(panel.Children[0].DataContext, last) || panel.Children[0].Opacity != 1 ||
                !ReferenceEquals(manager.Layout.ActiveContent, first))
                throw new Exception("Selecting a hidden packaged tab must move it forward without changing activation.");
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }

    private sealed class NaturalHeader(double width) : Control
    {
        protected override Size MeasureOverride(Size availableSize) => new(width, 20);
    }
}
