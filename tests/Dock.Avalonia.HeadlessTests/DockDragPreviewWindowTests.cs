using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Mvvm.Controls;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockDragPreviewWindowTests
{
    [AvaloniaTheory]
    [InlineData(1.0, 1.0, true)]
    [InlineData(1.0, 1.0, false)]
    // Avalonia 12 exposes headless scale changes; v11 does not.
#if !AVALONIA_11
    [InlineData(1.0, 2.0, true)]
    [InlineData(2.0, 1.0, true)]
    [InlineData(2.0, 2.0, true)]
    [InlineData(1.0, 2.0, false)]
    [InlineData(2.0, 1.0, false)]
    [InlineData(2.0, 2.0, false)]
#endif
    public void Native_preview_keeps_screen_geometry_across_scaling_changes(double initialScale, double nextScale, bool showContent)
    {
        var helper = new DragPreviewHelper();
        var originalManaged = DockSettings.UseManagedWindows;
        var originalPreview = DockSettings.ShowDockablePreviewOnDrag;
        DragPreviewWindow? window = null;
        using var subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((opened, _) => window = opened);
        helper.Hide();

        try
        {
            DockSettings.UseManagedWindows = false;
            DockSettings.ShowDockablePreviewOnDrag = showContent;
            var size = new Size(208, 143);
            var offset = new PixelPoint(-20, -10);
            helper.Show(new Document { Title = "Preview" }, new PixelPoint(1800, 300), offset, preferredSize: showContent ? size : null);
            Assert.NotNull(window);
            SetRenderScaling(window, initialScale);
            window.UpdateLayout();
            helper.Move(new PixelPoint(1810, 300), offset, "Float");
            Dispatcher.UIThread.RunJobs();
            var content = Assert.IsType<DragPreviewControl>(window.Content);
            var originalSize = new Size(window.Width, window.Height);
            Assert.True(originalSize.Width > 0 && originalSize.Height > 0);
            if (showContent)
            {
                Assert.Equal(size, originalSize);
            }

            // Cross the same monitor boundary repeatedly. macOS screen coordinates
            // are points, independent of the backing pixel scale; other backends
            // use physical screen pixels.
            for (var i = 0; i < 6; i++)
            {
                var scale = i % 2 == 0 ? nextScale : initialScale;
                SetRenderScaling(window, scale);
                var pointer = new PixelPoint(1910 + i, 300);
                helper.Move(pointer, offset, "Float");
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();

                var ratio = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 1 : initialScale / scale;
                Assert.Equal(originalSize.Width * ratio, window.Width, 5);
                Assert.Equal(originalSize.Height * ratio, window.Height, 5);
                if (showContent)
                {
                    Assert.Equal(size.Width * ratio, content.PreviewContentWidth, 5);
                    Assert.Equal(size.Height * ratio, content.PreviewContentHeight, 5);
                }
                else
                {
                    Assert.True(double.IsNaN(content.PreviewContentWidth));
                    Assert.True(double.IsNaN(content.PreviewContentHeight));
                }
                Assert.Equal(pointer + offset, window.Position);
            }
        }
        finally
        {
            helper.Hide();
            Dispatcher.UIThread.RunJobs();
            DockSettings.UseManagedWindows = originalManaged;
            DockSettings.ShowDockablePreviewOnDrag = originalPreview;
        }
    }

    private static void SetRenderScaling(Window window, double scale)
    {
#if AVALONIA_11
        Assert.Equal(1.0, scale);
#else
        window.SetRenderScaling(scale);
#endif
    }
}
