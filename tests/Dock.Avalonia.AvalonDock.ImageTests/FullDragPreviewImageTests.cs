using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.AvalonDock.ImageTests;

public class FullDragPreviewImageTests
{
    public static IEnumerable<object[]> Cases()
    {
        for (int shape = 0; shape < 3; shape++)
            foreach (bool managed in new[] { false, true })
                foreach (double scale in new[] { 1d, 2d })
                    yield return new object[] { shape, managed, scale };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Full_Preview_Renders_The_Original_Editor_While_The_Source_Selects_Another_Tab(int shape, bool managed, double scale)
    {
        bool oldManaged = DockSettings.UseManagedWindows;
        bool oldPreview = DockSettings.ShowDockablePreviewOnDrag;
        DockSettings.UseManagedWindows = managed;
        DockSettings.ShowDockablePreviewOnDrag = false;
        bool tool = shape != 0;
        bool wholePane = shape == 2;
        LayoutContent first = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutContent second = tool ? new LayoutAnchorable() : new LayoutDocument();
        first.Title = "Drag this editor";
        second.Title = "Other editor";
        first.Content = new Border { Background = Brushes.Lime, Child = new TextBox { Text = "Original content", Background = Brushes.Transparent } };
        second.Content = new Border { Background = Brushes.Magenta };
        ILayoutPanelElement pane = tool
            ? new LayoutAnchorablePane((LayoutAnchorable)first) { Children = { (LayoutAnchorable)second } }
            : new LayoutDocumentPane(first) { Children = { second } };
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { pane } } } };
        Window owner = new() { Width = 850, Height = 550, Content = manager };
        DragPreviewWindow? previewWindow = null;
        using IDisposable subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((window, _) => previewWindow = window);
        try
        {
            owner.Show();
            owner.SetRenderScaling(scale);
            first.IsActive = true;
            Update();
            Control drag = wholePane
                ? manager.GetVisualDescendants().OfType<ToolChromeControl>().Single(control => ReferenceEquals(control.DataContext, first.DockModel.Owner))
                : manager.GetVisualDescendants().OfType<Control>().Single(control => tool
                    ? control is LayoutAnchorableTabItem anchor && ReferenceEquals(anchor.Model, first)
                    : control is LayoutDocumentTabItem document && ReferenceEquals(document.Model, first));
            Point start = drag.TranslatePoint(new Point(wholePane ? 30 : drag.Bounds.Width / 2, wholePane ? 10 : drag.Bounds.Height / 2), owner)!.Value;
            owner.MouseMove(start);
            owner.MouseDown(start, MouseButton.Left);
            owner.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            owner.MouseMove(start + new Vector(30, tool && !wholePane ? -80 : 80), RawInputModifiers.LeftMouseButton);
            Update();
            DragPreviewControl preview = managed
                ? Assert.Single(owner.GetVisualDescendants().OfType<DragPreviewControl>())
                : Assert.IsType<DragPreviewControl>(Assert.IsType<DragPreviewWindow>(previewWindow).Content);
            Image image = Assert.IsAssignableFrom<Image>(preview.PreviewContent);
            Bitmap bitmap = Assert.IsAssignableFrom<Bitmap>(image.Source);
            byte[] original = Pixels(bitmap);
            Assert.True(GreenPixels(original) > bitmap.PixelSize.Width * bitmap.PixelSize.Height / 3,
                "The preview must contain the original green editor, not the newly selected magenta tab.");
            Assert.Same(wholePane ? first : second, ((ILayoutContentSelector)pane).SelectedContent);
            Assert.True(preview.Bounds.Width > 300 && preview.Bounds.Height > 300);
            if (!managed)
            {
                previewWindow!.UpdateLayout();
                using Bitmap rendered = previewWindow.CaptureRenderedFrame()!;
                Assert.True(GreenPixels(Pixels(rendered)) > rendered.PixelSize.Width * rendered.PixelSize.Height / 3,
                    "The full preview must be visible in the rendered preview window.");
            }
            first.Content = new Border { Background = Brushes.Blue };
            Update();
            Assert.Equal(original, Pixels(bitmap));
            owner.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            owner.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            owner.MouseUp(start, MouseButton.Left);
            Update();
            Assert.Null(image.Source);
            Assert.Same(pane, first.Parent);
            Assert.Same(first, ((ILayoutContentSelector)pane).SelectedContent);
            Assert.Empty(manager.Layout.FloatingWindows);
            Assert.False(manager.GetVisualDescendants().OfType<DockControl>().Single().IsDraggingDock);
        }
        finally
        {
            owner.Close();
            Dispatcher.UIThread.RunJobs();
            DockSettings.UseManagedWindows = oldManaged;
            DockSettings.ShowDockablePreviewOnDrag = oldPreview;
        }
        void Update() { Dispatcher.UIThread.RunJobs(); owner.UpdateLayout(); }
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

    private static int GreenPixels(byte[] pixels)
    {
        int count = 0;
        for (int index = 0; index < pixels.Length; index += 4)
            if (pixels[index] == 0 && pixels[index + 1] == 255 && pixels[index + 2] == 0 && pixels[index + 3] == 255) count++;
        return count;
    }
}
