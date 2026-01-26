using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal class DragPreviewHelper
{
    private static readonly object s_sync = new();
    private static DragPreviewWindow? s_window;
    private static DragPreviewControl? s_control;
    private static bool s_templatesInitialized;

    private static PixelPoint GetPositionWithinWindow(Window window, PixelPoint position, PixelPoint offset)
    {
        var screen = window.Screens.ScreenFromPoint(position);
        if (screen is { })
        {
            var target = position + offset;
            if (screen.WorkingArea.Contains(target))
            {
                return target;
            }
        }
        return position;
    }

    private static Size GetPreviewSize(IDockable dockable)
    {
        dockable.GetVisibleBounds(out _, out _, out var width, out var height);

        IDock? owner = dockable.Owner as IDock;
        double ownerWidth = double.NaN;
        double ownerHeight = double.NaN;

        if (owner is not null)
        {
            owner.GetVisibleBounds(out _, out _, out ownerWidth, out ownerHeight);
        }

        if (double.IsNaN(width) || width <= 0)
        {
            width = double.IsNaN(ownerWidth) || ownerWidth <= 0 ? 300 : ownerWidth;
        }

        if (double.IsNaN(height) || height <= 0)
        {
            height = double.IsNaN(ownerHeight) || ownerHeight <= 0 ? 400 : ownerHeight;
        }

        return new Size(width, height);
    }

    private static void EnsureDataTemplates(DragPreviewControl control)
    {
        if (s_templatesInitialized)
        {
            return;
        }

        foreach (var template in DockDataTemplateHelper.CreateDefaultDataTemplates())
        {
            control.DataTemplates.Add(template);
        }

        s_templatesInitialized = true;
    }

    private static double ClampOpacity(double value)
    {
        if (double.IsNaN(value))
        {
            return 1.0;
        }

        if (value < 0.0)
        {
            return 0.0;
        }

        return value > 1.0 ? 1.0 : value;
    }

    public void Show(IDockable dockable, PixelPoint position, PixelPoint offset)
    {
        lock (s_sync)
        {
            if (s_window is null || s_control is null)
            {
                s_control = new DragPreviewControl
                {
                    Status = string.Empty
                };
                EnsureDataTemplates(s_control);

                s_window = new DragPreviewWindow
                {
                    Content = s_control
                };
            }

            var showDockablePreview = DockSettings.ShowDockablePreviewOnDrag;
            s_control.ShowContent = showDockablePreview;
            if (showDockablePreview)
            {
                var size = GetPreviewSize(dockable);
                s_control.PreviewContentWidth = size.Width;
                s_control.PreviewContentHeight = size.Height;
            }
            else
            {
                s_control.PreviewContentWidth = double.NaN;
                s_control.PreviewContentHeight = double.NaN;
            }

            s_window.DataContext = dockable;
            s_control.Status = string.Empty;
            s_window.Opacity = ClampOpacity(DockSettings.DragPreviewOpacity);
            s_window.Position = GetPositionWithinWindow(s_window, position, offset);

            if (!s_window.IsVisible)
            {
                s_window.Show();
            }
        }
    }

    public void Move(PixelPoint position, PixelPoint offset, string status)
    {
        lock (s_sync)
        {
            if (s_window is null || s_control is null)
            {
                return;
            }

            s_control.Status = status;
            s_window.Position = GetPositionWithinWindow(s_window, position, offset);
        }
    }

    public void Hide()
    {
        lock (s_sync)
        {
            if (s_window is null)
            {
                return;
            }

            s_window.Close();
            s_window = null;
            s_control = null;
            s_templatesInitialized = false;
        }
    }
}
