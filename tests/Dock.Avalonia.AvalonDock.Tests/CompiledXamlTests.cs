using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class CompiledXamlTests
{
    [AvaloniaFact]
    public void Xceed_Xml_Namespace_Builds_And_Renders_Dock_Controls()
    {
        XamlFixture window = new();
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            DockControl dock = Assert.Single(manager.GetVisualDescendants().OfType<DockControl>());
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
