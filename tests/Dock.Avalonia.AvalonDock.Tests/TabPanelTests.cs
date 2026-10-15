using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabPanelTests
{
    [AvaloniaTheory]
    [InlineData("DocumentPanel", false)]
    [InlineData("LegacyDocumentPanel", false)]
    [InlineData("ToolPanel", true)]
    [InlineData("LegacyToolPanel", true)]
    public void Both_XML_Namespaces_Compile_Public_Items_Panels(string key, bool tool)
    {
        TabPanelFixture resources = new();
        Panel panel = ((ITemplate<Panel>)resources.Resources[key]!).Build();
        if (tool) Assert.IsType<AnchorablePaneTabPanel>(panel);
        else Assert.IsType<DocumentPaneTabPanel>(panel);
        Assert.Equal(FlowDirection.LeftToRight, panel.FlowDirection);
    }

    [AvaloniaTheory]
    [InlineData(200, 30, 90)]
    [InlineData(80, 30, 40)]
    [InlineData(60, 30, 30)]
    [InlineData(0, 0, 0)]
    public void Tool_Panel_Measures_Natural_Widths_And_Constrains_Visible_Headers(double width, double firstWidth, double secondWidth)
    {
        AnchorablePaneTabPanel panel = new();
        SizeProbe first = new(30, 20), second = new(90, 30), collapsed = new(1000, 1000) { IsVisible = false };
        panel.Children.Add(first); panel.Children.Add(collapsed); panel.Children.Add(second);
        panel.Measure(new Size(width, 100));
        panel.Arrange(new Rect(0, 0, width, 40));
        Assert.Equal(new Size(Math.Min(width, 120), 30), panel.DesiredSize);
        Assert.Equal(firstWidth, first.Bounds.Width);
        Assert.Equal(secondWidth, second.Bounds.Width);
        Assert.Equal(firstWidth, second.Bounds.X);
        Assert.Equal(0, collapsed.Measures);
        Assert.Equal(width < 120 ? width / 2 : double.PositiveInfinity, first.Constraint.Width);
        Assert.Equal(100, first.Constraint.Height);
    }

    [AvaloniaFact]
    public void Tool_Panel_Shares_The_Arrange_Width_When_It_Does_Not_Exceed_The_Measured_Total()
    {
        AnchorablePaneTabPanel panel = new();
        SizeProbe first = new(30, 20), second = new(90, 20);
        panel.Children.Add(first); panel.Children.Add(second);
        panel.Measure(Size.Infinity);
        panel.Arrange(new Rect(0, 0, 120, 20));
        Assert.Equal(60, first.Bounds.Width);
        Assert.Equal(new Rect(60, 0, 60, 20), second.Bounds);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Empty_And_Collapsed_Panels_Have_No_Desired_Extent(bool tool)
    {
        Panel panel = tool ? new AnchorablePaneTabPanel() : new DocumentPaneTabPanel();
        panel.Measure(Size.Infinity);
        panel.Arrange(new Rect(0, 0, 100, 20));
        Assert.Equal(default, panel.DesiredSize);
        panel.Children.Add(new SizeProbe(500, 80) { IsVisible = false });
        panel.Measure(Size.Infinity);
        panel.Arrange(new Rect(0, 0, 100, 20));
        Assert.Equal(default, panel.DesiredSize);
    }

    [AvaloniaFact]
    public void Document_Overflow_Preserves_Measurement_And_Restores_Application_Values()
    {
        DocumentPaneTabPanel panel = new();
        Border source = new() { Opacity = .4, IsHitTestVisible = true, IsEnabled = false };
        SizeProbe first = new(60, 20), second = new(60, 20), third = new(20, 30);
        second.Bind(Visual.OpacityProperty, source.GetObservable(Visual.OpacityProperty));
        second.Bind(InputElement.IsHitTestVisibleProperty, source.GetObservable(InputElement.IsHitTestVisibleProperty));
        second.Bind(InputElement.IsEnabledProperty, source.GetObservable(InputElement.IsEnabledProperty));
        panel.Children.Add(first); panel.Children.Add(second); panel.Children.Add(third);
        panel.Measure(new Size(100, 40));
        panel.Arrange(new Rect(0, 0, 100, 40));
        Assert.Equal(new Size(100, 30), panel.DesiredSize);
        Assert.Equal(Size.Infinity, second.Constraint);
        Assert.Equal(new Size(60, 20), second.DesiredSize);
        Assert.True(second.IsVisible);
        Assert.Equal(0, second.Opacity);
        Assert.False(second.IsHitTestVisible);
        Assert.False(second.IsEnabled);
        Assert.Equal(0, third.Opacity); // Later small headers do not fill a gap after overflow.
        source.Opacity = .7;
        source.IsHitTestVisible = false;
        source.IsEnabled = true;
        panel.Measure(new Size(200, 40));
        panel.Arrange(new Rect(0, 0, 200, 40));
        Assert.Equal(.7, second.Opacity);
        Assert.False(second.IsHitTestVisible);
        Assert.True(second.IsEnabled);
        Assert.Equal(1, third.Opacity);
        Assert.Equal(120, third.Bounds.X);
    }

    [AvaloniaFact]
    public void Overflow_Removal_And_Panel_Detachment_Release_Only_Their_Temporary_Values()
    {
        DocumentPaneTabPanel panel = new();
        Border first = new() { Width = 70, Height = 30 }, second = new() { Width = 70, Height = 30, Opacity = .6 };
        panel.Children.Add(first); panel.Children.Add(second);
        Window window = new() { Content = panel, Width = 100, Height = 50 };
        try
        {
            window.Show(); window.UpdateLayout();
            Assert.Equal(0, second.Opacity);
            panel.Children.Remove(second);
            Assert.Equal(.6, second.Opacity);
            Assert.True(second.IsEnabled);
            panel.Children.Add(second);
            window.UpdateLayout();
            Assert.Equal(0, second.Opacity);
            window.Content = null;
            Assert.Equal(.6, second.Opacity);
            Assert.True(second.IsEnabled);
            window.Content = panel;
            window.UpdateLayout();
            Assert.Equal(0, second.Opacity);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Selecting_An_Overflowed_Document_Reorders_The_Live_Pane_And_Its_Bound_Row()
    {
        LayoutDocument first = new() { Title = "First" }, second = new() { Title = "Second" }, third = new() { Title = "Third" };
        LayoutDocumentPane pane = new(first);
        pane.Children.Add(second); pane.Children.Add(third);
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
        first.IsActive = true;
        TabPanelFixture window = new() { Width = 150 };
        ItemsControl row = (ItemsControl)((IDataTemplate)window.Resources["DocumentTabs"]!).Build(pane)!;
        row.DataContext = pane;
        window.Content = row;
        try
        {
            window.Show(); Update();
            DocumentPaneTabPanel panel = row.GetVisualDescendants().OfType<DocumentPaneTabPanel>().Single();
            Assert.Equal(0, panel.Children[2].Opacity);
            third.IsSelected = true;
            Update();
            Assert.Equal(new[] { third, first, second }, pane.Children);
            Assert.Equal(0, pane.SelectedContentIndex);
            Assert.Same(third, panel.Children[0].DataContext);
            Assert.Equal(1, panel.Children[0].Opacity);
            Assert.True(panel.Children[0].IsEnabled);
            Assert.Equal(new[] { third.DockModel, first.DockModel, second.DockModel }, ((Dock.Model.Core.IDock)third.DockModel.Owner!).VisibleDockables!);
            Assert.Same(first, manager.Layout.ActiveContent); // Selection does not activate another document.
        }
        finally { window.Close(); }
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }

    [AvaloniaFact]
    public void Overwide_First_Header_Stays_Suppressed_Without_A_Selection_Reorder_Loop()
    {
        LayoutDocument first = new() { IsSelected = true };
        LayoutDocumentPane pane = new(first);
        pane.Children.Add(new LayoutDocument());
        DocumentPaneTabPanel panel = new();
        panel.Children.Add(new TabItem { Content = first, Width = 120, Height = 30 });
        panel.Children.Add(new TabItem { Content = pane.Children[1], Width = 20, Height = 30 });
        panel.Measure(new Size(100, 40));
        panel.Arrange(new Rect(0, 0, 100, 40));
        Assert.Equal(0, panel.Children[0].Opacity);
        Assert.Equal(0, panel.Children[1].Opacity);
        Assert.Same(first, pane.Children[0]);
        Assert.True(panel.IsArrangeValid);
    }

    [AvaloniaFact]
    public void Public_Panel_Measure_Arrange_And_Pointer_Exit_Hooks_Are_Invoked()
    {
        DocumentPanelProbe panel = new() { Background = Brushes.Transparent };
        Window window = new() { Content = panel, Width = 200, Height = 100 };
        try
        {
            window.Show(); window.UpdateLayout();
            window.MouseMove(new Point(10, 10));
            window.MouseMove(new Point(250, 110));
            Assert.True(panel.Measures > 0);
            Assert.True(panel.Arranges > 0);
            Assert.Equal(1, panel.Exits);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Keeping_A_Model_Does_Not_Keep_An_Unused_Overflow_Panel_Alive()
    {
        LayoutDocument document = new();
        WeakReference<DocumentPaneTabPanel> reference = CreatePanel(document);
        for (int i = 0; i < 3; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Assert.False(reference.TryGetTarget(out _));
        document.IsSelected = true;
        GC.KeepAlive(document);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<DocumentPaneTabPanel> CreatePanel(LayoutDocument document)
    {
        DocumentPaneTabPanel panel = new();
        panel.Children.Add(new LayoutDocumentTabItem { Model = document, Width = 100 });
        panel.Measure(new Size(50, 30)); panel.Arrange(new Rect(0, 0, 50, 30));
        return new(panel);
    }

    private sealed class SizeProbe(double width, double height) : Control
    {
        public int Measures { get; private set; }
        public Size Constraint { get; private set; }
        protected override Size MeasureOverride(Size availableSize)
        {
            Measures++;
            Constraint = availableSize;
            return new Size(width, height);
        }
    }

    private sealed class DocumentPanelProbe : DocumentPaneTabPanel
    {
        public int Measures { get; private set; }
        public int Arranges { get; private set; }
        public int Exits { get; private set; }
        protected override Size MeasureOverride(Size availableSize) { Measures++; return base.MeasureOverride(availableSize); }
        protected override Size ArrangeOverride(Size finalSize) { Arranges++; return base.ArrangeOverride(finalSize); }
        protected override void OnMouseLeave(PointerEventArgs e) { Exits++; base.OnMouseLeave(e); }
    }
}
