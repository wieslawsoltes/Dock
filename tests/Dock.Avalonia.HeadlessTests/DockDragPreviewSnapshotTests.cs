using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockDragPreviewSnapshotTests
{
    [AvaloniaTheory]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public void Preview_Pixels_Survive_Source_Recycling_And_Are_Released_After_The_Drag(bool managed, double scale)
    {
        bool oldPreview = DockSettings.ShowDockablePreviewOnDrag;
        bool oldManaged = DockSettings.UseManagedWindows;
        Factory factory = new();
        IDockable document = factory.CreateDocument();
        document.Factory = factory;
        Border source = new() { Background = Brushes.Lime, Width = 200, Height = 140, Margin = new Thickness(45, 30, 0, 0) };
        ManagedWindowLayer layer = new();
        Window owner = new() { Width = 600, Height = 400, Content = new Grid { Children = { source, layer } } };
        DragPreviewWindow? window = null;
        using var subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((opened, _) => window = opened);
        DragPreviewHelper helper = new();
        try
        {
            DockSettings.ShowDockablePreviewOnDrag = true;
            DockSettings.UseManagedWindows = managed;
            owner.Show();
            owner.SetRenderScaling(scale);
            owner.UpdateLayout();
            factory.DocumentControls[document] = source;
            if (managed) ManagedWindowRegistry.RegisterLayer(factory, layer);
            helper.Show(document, new PixelPoint(50, 60), default, layer);
            Dispatcher.UIThread.RunJobs();
            owner.UpdateLayout();
            DragPreviewControl preview = managed
                ? Assert.Single(layer.GetVisualDescendants().OfType<DragPreviewControl>())
                : Assert.IsType<DragPreviewControl>(Assert.IsType<DragPreviewWindow>(window).Content);
            DragPreviewSnapshot image = Assert.IsType<DragPreviewSnapshot>(preview.PreviewContent);
            Bitmap bitmap = Assert.IsAssignableFrom<Bitmap>(image.Source);
            Assert.Equal(new PixelSize((int)(200 * scale), (int)(140 * scale)), bitmap.PixelSize);
            byte[] before = Pixels(bitmap);
            // Lime's bytes are the same in RGBA and BGRA formats.
            Assert.Equal(new byte[] { 0, 255, 0, 255 }, before[..4]);
            Assert.Equal(new byte[] { 0, 255, 0, 255 }, before[^4..]);
            source.Background = Brushes.Blue;
            ((Grid)owner.Content!).Children.Remove(source);
            owner.UpdateLayout();
            helper.Move(new PixelPoint(80, 90), default, "Float");
            Assert.Equal(before, Pixels(bitmap));
            Assert.Null(TopLevel.GetTopLevel(source));
            helper.Hide();
            Assert.Null(preview.PreviewContent);
            Assert.Null(image.Source);
            Assert.Null(preview.DataContext);
        }
        finally
        {
            helper.Hide();
            ManagedWindowRegistry.UnregisterLayer(factory, layer);
            owner.Close();
            Dispatcher.UIThread.RunJobs();
            DockSettings.ShowDockablePreviewOnDrag = oldPreview;
            DockSettings.UseManagedWindows = oldManaged;
        }
    }

    [AvaloniaFact]
    public void Replacing_An_Open_Preview_Releases_The_Previous_Snapshot()
    {
        bool previous = DockSettings.ShowDockablePreviewOnDrag;
        Factory factory = new();
        IDockable document = factory.CreateDocument();
        document.Factory = factory;
        Border source = new() { Background = Brushes.Lime };
        Window owner = new() { Content = source, Width = 100, Height = 100 };
        DragPreviewWindow? window = null;
        using var subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((opened, _) => window = opened);
        DragPreviewHelper helper = new();
        try
        {
            DockSettings.ShowDockablePreviewOnDrag = true;
            owner.Show();
            owner.UpdateLayout();
            factory.DocumentControls[document] = source;
            helper.Show(document, default, default);
            DragPreviewControl control = Assert.IsType<DragPreviewControl>(window!.Content);
            DragPreviewSnapshot first = Assert.IsType<DragPreviewSnapshot>(control.PreviewContent);
            source.Background = Brushes.Blue;
            helper.Show(document, default, default);
            Assert.Null(first.Source);
            Assert.NotSame(first, control.PreviewContent);
        }
        finally
        {
            helper.Hide();
            owner.Close();
            DockSettings.ShowDockablePreviewOnDrag = previous;
        }
    }

    private static byte[] Pixels(Bitmap bitmap)
    {
        PixelSize size = bitmap.PixelSize;
        byte[] bytes = new byte[size.Width * size.Height * 4];
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { bitmap.CopyPixels(new PixelRect(size), handle.AddrOfPinnedObject(), bytes.Length, size.Width * 4); }
        finally { handle.Free(); }
        return bytes;
    }
}
