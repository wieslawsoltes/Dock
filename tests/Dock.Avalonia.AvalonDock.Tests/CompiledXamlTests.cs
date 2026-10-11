using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class CompiledXamlTests
{
    [AvaloniaFact]
    public void Splitter_Drag_Updates_Public_Width_And_Survives_Window_Resize()
    {
        XamlFixture window = new();
        try
        {
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            LayoutAnchorablePane pane = manager.Layout.Descendents().OfType<LayoutAnchorablePane>().Single();
            pane.DockWidth = new GridLength(180);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ProportionalStackPanelSplitter splitter = manager.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>().First();
            Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), window)!.Value;
            window.MouseDown(start, MouseButton.Left);
            window.MouseMove(start + new Vector(60, 0));
            window.MouseUp(start + new Vector(60, 0), MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.InRange(pane.DockWidth.Value, 236, 244);
            window.Width = 1000;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            ToolDockControl control = Assert.Single(manager.GetVisualDescendants().OfType<ToolDockControl>());
            Assert.InRange(control.Bounds.Width, 236, 244);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Vertical_Layout_Applies_Pixel_Height()
    {
        XamlFixture window = new();
        try
        {
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            manager.Layout.RootPanel.Orientation = global::Avalonia.Layout.Orientation.Vertical;
            LayoutAnchorablePane pane = manager.Layout.Descendents().OfType<LayoutAnchorablePane>().Single();
            pane.DockHeight = new GridLength(140);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ToolDockControl control = Assert.Single(manager.GetVisualDescendants().OfType<ToolDockControl>());
            Assert.InRange(control.Bounds.Height, 136, 144);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Pixel_Pane_Width_Is_Applied_And_Can_Be_Changed_At_Runtime()
    {
        XamlFixture window = new();
        try
        {
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            LayoutAnchorablePane pane = manager.Layout.Descendents().OfType<LayoutAnchorablePane>().Single();
            pane.DockWidth = new GridLength(180);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ToolDockControl control = Assert.Single(manager.GetVisualDescendants().OfType<ToolDockControl>());
            Assert.InRange(control.Bounds.Width, 176, 184);
            pane.DockWidth = new GridLength(240);
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.InRange(control.Bounds.Width, 236, 244);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Avalonia_Default_Xml_Namespace_Builds_And_Renders_Dock_Controls()
    {
        XamlFixture window = new();
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            DockControl dock = Assert.Single(manager.GetVisualDescendants().OfType<DockControl>());
            Assert.IsType<global::AvalonDock.Converters.LayoutItemFromLayoutModelConverter>(window.Resources["ItemFromModel"]);
            Assert.IsType<global::AvalonDock.Converters.LayoutItemFromLayoutModelConverter>(window.Resources["LegacyItemFromModel"]);
            Assert.Same(manager.DockLayout, dock.Layout);
            Assert.Same(manager.DockFactory, dock.Factory);
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), x => x.Text == "First document");
            TextBox editor = window.FindControl<TextBox>("Editor")!;
            Assert.True(editor.Bounds.Width > 0);
            editor.Focus();
            editor.CaretIndex = editor.Text!.Length;
            window.KeyTextInput(" edited");
            Assert.Equal("Editable document edited", editor.Text);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Document_Tab_Can_Be_Selected_With_Headless_Pointer_Input()
    {
        XamlFixture window = new();
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            LayoutDocument second = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => x.ContentId == "two");
            TextBlock header = window.GetVisualDescendants().OfType<TextBlock>().First(x => x.Text == "Second document");
            Point position = header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), window)!.Value;
            window.MouseDown(position, MouseButton.Left);
            window.MouseUp(position, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.True(second.IsSelected);
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), x => x.Text == "Second document content");
        }
        finally { window.Close(); }
    }
}
