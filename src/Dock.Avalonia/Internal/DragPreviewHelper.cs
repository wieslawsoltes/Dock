using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal class DragPreviewHelper
{
    private static readonly object s_sync = new();
    private static DragPreviewWindow? s_window;
    private static DragPreviewControl? s_control;
    private static bool s_windowTemplatesInitialized;
    private static bool s_managedTemplatesInitialized;
    private static DragPreviewControl? s_managedControl;
    private static ManagedWindowLayer? s_managedLayer;
    private static readonly bool s_useWindowMoveCoalescing = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private static PixelPoint s_pendingWindowPosition;
    private static string s_pendingStatus = string.Empty;
    private static bool s_hasPendingWindowMove;
    private static bool s_windowMoveFlushScheduled;
    private static bool s_windowSizeFrozen;
    private static bool s_windowSizeFreezeScheduled;
    private static double s_frozenWindowWidthPixels;
    private static double s_frozenWindowHeightPixels;
    private static double s_frozenContentWidthPixels = double.NaN;
    private static double s_frozenContentHeightPixels = double.NaN;
    private static double s_lastFrozenWindowScaling = 1.0;

    private static PixelPoint GetPositionWithinWindow(Window window, PixelPoint position, PixelPoint offset)
    {
        _ = window;
        return position + offset;
    }

    private static Size GetPreviewSize(IDockable dockable)
    {
        if (dockable is ManagedDockWindowDocument managedDockable)
        {
            var bounds = managedDockable.MdiBounds;
            if (!double.IsNaN(bounds.Width) && bounds.Width > 0
                && !double.IsNaN(bounds.Height) && bounds.Height > 0)
            {
                return new Size(bounds.Width, bounds.Height);
            }
        }

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

    private static void EnsureDataTemplates(DragPreviewControl control, ref bool templatesInitialized)
    {
        if (templatesInitialized)
        {
            return;
        }

        foreach (var template in DockDataTemplateHelper.CreateDefaultDataTemplates())
        {
            control.DataTemplates.Add(template);
        }

        templatesInitialized = true;
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

    private static void QueueWindowMove(DragPreviewWindow window, DragPreviewControl control, PixelPoint targetPosition, string status)
    {
        if (!s_useWindowMoveCoalescing)
        {
            ApplyWindowMove(window, control, targetPosition, status);
            return;
        }

        s_pendingWindowPosition = targetPosition;
        s_pendingStatus = status;
        s_hasPendingWindowMove = true;
        if (!s_windowMoveFlushScheduled)
        {
            s_windowMoveFlushScheduled = true;
            Dispatcher.UIThread.Post(FlushPendingWindowMove, DispatcherPriority.Render);
        }
    }

    private static void FlushPendingWindowMove()
    {
        lock (s_sync)
        {
            s_windowMoveFlushScheduled = false;
            if (!s_hasPendingWindowMove || s_window is null || s_control is null)
            {
                s_hasPendingWindowMove = false;
                return;
            }

            ApplyWindowMove(s_window, s_control, s_pendingWindowPosition, s_pendingStatus);
            s_hasPendingWindowMove = false;

            if (s_hasPendingWindowMove && !s_windowMoveFlushScheduled)
            {
                s_windowMoveFlushScheduled = true;
                Dispatcher.UIThread.Post(FlushPendingWindowMove, DispatcherPriority.Render);
            }
        }
    }

    private static void ApplyWindowMove(DragPreviewWindow window, DragPreviewControl control, PixelPoint targetPosition, string status)
    {
        var hadStatus = !string.IsNullOrEmpty(control.Status);
        if (!string.Equals(control.Status, status, StringComparison.Ordinal))
        {
            control.Status = status;
        }

        if (window.Position != targetPosition)
        {
            window.Position = targetPosition;
        }

        if (s_windowSizeFrozen)
        {
            MaintainFrozenWindowSize(window, control);
        }

        if (!s_windowSizeFrozen && !s_windowSizeFreezeScheduled && !hadStatus && !string.IsNullOrEmpty(status))
        {
            s_windowSizeFreezeScheduled = true;
            Dispatcher.UIThread.Post(FreezeWindowSizeIfNeeded, DispatcherPriority.Render);
        }
    }

    private static double GetWindowScaling(Window window)
    {
        var scaling = window.RenderScaling;
        return scaling > 0 ? scaling : 1.0;
    }

    private static void MaintainFrozenWindowSize(DragPreviewWindow window, DragPreviewControl control)
    {
        var scaling = GetWindowScaling(window);
        if (Math.Abs(scaling - s_lastFrozenWindowScaling) < 0.0001)
        {
            return;
        }

        s_lastFrozenWindowScaling = scaling;

        var width = s_frozenWindowWidthPixels / scaling;
        var height = s_frozenWindowHeightPixels / scaling;

        if (Math.Abs(window.Width - width) > 0.01)
        {
            window.Width = width;
        }

        if (Math.Abs(window.Height - height) > 0.01)
        {
            window.Height = height;
        }

        if (!double.IsNaN(s_frozenContentWidthPixels))
        {
            var contentWidth = s_frozenContentWidthPixels / scaling;
            if (Math.Abs(control.PreviewContentWidth - contentWidth) > 0.01)
            {
                control.PreviewContentWidth = contentWidth;
            }
        }

        if (!double.IsNaN(s_frozenContentHeightPixels))
        {
            var contentHeight = s_frozenContentHeightPixels / scaling;
            if (Math.Abs(control.PreviewContentHeight - contentHeight) > 0.01)
            {
                control.PreviewContentHeight = contentHeight;
            }
        }
    }

    private static void FreezeWindowSizeIfNeeded()
    {
        lock (s_sync)
        {
            s_windowSizeFreezeScheduled = false;
            if (s_windowSizeFrozen || s_window is null || !s_window.IsVisible)
            {
                return;
            }

            var bounds = s_window.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var scaling = GetWindowScaling(s_window);
            s_lastFrozenWindowScaling = scaling;
            s_frozenWindowWidthPixels = bounds.Width * scaling;
            s_frozenWindowHeightPixels = bounds.Height * scaling;
            s_frozenContentWidthPixels = !double.IsNaN(s_control?.PreviewContentWidth ?? double.NaN)
                ? (s_control!.PreviewContentWidth * scaling)
                : double.NaN;
            s_frozenContentHeightPixels = !double.IsNaN(s_control?.PreviewContentHeight ?? double.NaN)
                ? (s_control!.PreviewContentHeight * scaling)
                : double.NaN;

            s_window.SizeToContent = SizeToContent.Manual;
            s_window.Width = bounds.Width;
            s_window.Height = bounds.Height;
            s_windowSizeFrozen = true;
        }
    }

    public void Show(IDockable dockable, PixelPoint position, PixelPoint offset, Visual? context = null, Size? preferredSize = null)
    {
        lock (s_sync)
        {
            if (dockable is ManagedDockWindowDocument)
            {
                Hide();
                return;
            }

            var showDockablePreview = ShouldShowPreviewContent(dockable);
            DragPreviewContext.IsActive = showDockablePreview && dockable is ManagedDockWindowDocument;
            DragPreviewContext.Dockable = dockable;
            if (DockHelpers.IsManagedWindowHostingEnabled(dockable) && TryGetManagedLayer(dockable, context, out var layer))
            {
                ShowManaged(layer, dockable, position, offset, showDockablePreview);
                return;
            }

            if (s_window is null || s_control is null)
            {
                s_control = new DragPreviewControl
                {
                    Status = string.Empty
                };
                EnsureDataTemplates(s_control, ref s_windowTemplatesInitialized);

                s_window = new DragPreviewWindow
                {
                    Content = s_control
                };
            }

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

            s_control.PreviewContent = null;
            s_window.DataContext = dockable;
            s_control.Status = string.Empty;
            s_window.Opacity = ClampOpacity(DockSettings.DragPreviewOpacity);
            s_window.Position = GetPositionWithinWindow(s_window, position, offset);
            s_pendingWindowPosition = s_window.Position;
            s_pendingStatus = s_control.Status;
            s_hasPendingWindowMove = false;
            s_windowMoveFlushScheduled = false;
            s_windowSizeFrozen = false;
            s_windowSizeFreezeScheduled = false;
            s_frozenWindowWidthPixels = 0;
            s_frozenWindowHeightPixels = 0;
            s_frozenContentWidthPixels = double.NaN;
            s_frozenContentHeightPixels = double.NaN;
            s_lastFrozenWindowScaling = 1.0;

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
            if (DockHelpers.IsManagedWindowHostingEnabled(DragPreviewContext.Dockable) && s_managedLayer is { } layer && s_managedControl is { })
            {
                MoveManaged(layer, position, offset, status);
                return;
            }

            if (s_window is null || s_control is null)
            {
                return;
            }

            var targetPosition = GetPositionWithinWindow(s_window, position, offset);
            QueueWindowMove(s_window, s_control, targetPosition, status);
        }
    }

    public void Hide()
    {
        lock (s_sync)
        {
            if (s_managedLayer is { })
            {
                s_managedLayer.HideOverlay("DragPreview");
                if (s_managedControl is { })
                {
                    s_managedControl.PreviewContent = null;
                }
                s_managedLayer = null;
                s_managedControl = null;
                s_managedTemplatesInitialized = false;
                DragPreviewContext.Clear();
                return;
            }

            DragPreviewContext.Clear();

            if (s_window is null)
            {
                return;
            }

            s_hasPendingWindowMove = false;
            s_windowMoveFlushScheduled = false;
            s_windowSizeFrozen = false;
            s_windowSizeFreezeScheduled = false;
            s_frozenWindowWidthPixels = 0;
            s_frozenWindowHeightPixels = 0;
            s_frozenContentWidthPixels = double.NaN;
            s_frozenContentHeightPixels = double.NaN;
            s_lastFrozenWindowScaling = 1.0;
            s_window.Close();
            s_window = null;
            s_control = null;
            s_windowTemplatesInitialized = false;
        }
    }

    private static bool TryGetManagedLayer(IDockable dockable, Visual? context, out ManagedWindowLayer layer)
    {
        layer = null!;
        var contextRoot = context?.GetVisualRoot();
        if (context is { })
        {
            var resolved = ManagedWindowLayer.TryGetLayer(context);
            if (IsLayerReady(resolved) && IsLayerInRoot(resolved, contextRoot))
            {
                layer = resolved!;
                return true;
            }
        }

        var factory = FindFactory(dockable);
        if (factory is null)
        {
            return false;
        }

        var registeredLayer = ManagedWindowRegistry.TryGetLayer(factory);
        if (IsLayerReady(registeredLayer) && IsLayerInRoot(registeredLayer, contextRoot))
        {
            layer = registeredLayer!;
            return true;
        }

        foreach (var dockControl in factory.DockControls.OfType<DockControl>())
        {
            var candidate = dockControl.GetVisualDescendants().OfType<ManagedWindowLayer>().FirstOrDefault();
            if (IsLayerReady(candidate) && IsLayerInRoot(candidate, contextRoot))
            {
                layer = candidate!;
                return true;
            }
        }

        return false;
    }

    private static IFactory? FindFactory(IDockable? dockable)
    {
        var current = dockable;
        while (current is not null)
        {
            if (current.Factory is { } factory)
            {
                return factory;
            }

            current = current.Owner;
        }

        return null;
    }

    private static bool IsLayerReady(ManagedWindowLayer? layer)
    {
        return layer is { IsVisible: true } && layer.GetVisualRoot() is not null;
    }

    private static bool IsLayerInRoot(ManagedWindowLayer? layer, object? contextRoot)
    {
        if (layer is null)
        {
            return false;
        }

        if (contextRoot is null)
        {
            return true;
        }

        return ReferenceEquals(layer.GetVisualRoot(), contextRoot);
    }

    private static void ShowManaged(ManagedWindowLayer layer, IDockable dockable, PixelPoint position, PixelPoint offset, bool showDockablePreview)
    {
        if (s_managedControl is null)
        {
            s_managedControl = new DragPreviewControl
            {
                Status = string.Empty
            };
            EnsureDataTemplates(s_managedControl, ref s_managedTemplatesInitialized);
        }

        s_managedLayer = layer;

        var targetSize = showDockablePreview ? GetPreviewSize(dockable) : new Size(double.NaN, double.NaN);

        s_managedControl.DataContext = dockable;
        s_managedControl.Status = string.Empty;
        s_managedControl.Opacity = ClampOpacity(DockSettings.DragPreviewOpacity);
        s_managedControl.ShowContent = showDockablePreview;

        if (showDockablePreview)
        {
            s_managedControl.PreviewContentWidth = targetSize.Width;
            s_managedControl.PreviewContentHeight = targetSize.Height;
        }
        else
        {
            s_managedControl.PreviewContentWidth = double.NaN;
            s_managedControl.PreviewContentHeight = double.NaN;
        }

        s_managedControl.PreviewContent = BuildManagedPreviewContent(dockable, showDockablePreview);

        var localPosition = GetManagedPosition(layer, position, offset);
        layer.ShowOverlay("DragPreview", s_managedControl, localPosition, null, false);
        s_managedControl.ApplyTemplate();
        s_managedControl.Measure(Size.Infinity);
        var sizeMeasured = s_managedControl.DesiredSize;

        if (showDockablePreview
            && dockable is ManagedDockWindowDocument
            && !double.IsNaN(targetSize.Height)
            && targetSize.Height > 0)
        {
            var overage = sizeMeasured.Height - targetSize.Height;
            if (overage > 0)
            {
                var adjustedHeight = targetSize.Height - overage;
                if (adjustedHeight < 0)
                {
                    adjustedHeight = 0;
                }

                s_managedControl.PreviewContentHeight = adjustedHeight;
                s_managedControl.Measure(Size.Infinity);
                sizeMeasured = s_managedControl.DesiredSize;
            }
        }

        layer.ShowOverlay("DragPreview", s_managedControl, localPosition, sizeMeasured, false);
    }

    private static void MoveManaged(ManagedWindowLayer layer, PixelPoint position, PixelPoint offset, string status)
    {
        if (s_managedControl is null)
        {
            return;
        }

        var localPosition = GetManagedPosition(layer, position, offset);
        if (!string.Equals(s_managedControl.Status, status, StringComparison.Ordinal))
        {
            s_managedControl.Status = status;
            s_managedControl.Measure(Size.Infinity);
            layer.ShowOverlay("DragPreview", s_managedControl, localPosition, s_managedControl.DesiredSize, false);
            return;
        }

        layer.ShowOverlay("DragPreview", s_managedControl, localPosition, s_managedControl.DesiredSize, false);
    }

    private static Rect GetManagedBounds(ManagedWindowLayer layer, PixelPoint position, PixelPoint offset, Size size)
    {
        var local = GetManagedPosition(layer, position, offset);
        return new Rect(local, size);
    }

    private static Point GetManagedPosition(ManagedWindowLayer layer, PixelPoint position, PixelPoint offset)
    {
        if (layer.GetVisualRoot() is not TopLevel topLevel)
        {
            return new Point(0, 0);
        }

        var target = new PixelPoint(position.X + offset.X, position.Y + offset.Y);
        var clientPoint = topLevel.PointToClient(target);
        var layerOrigin = layer.TranslatePoint(new Point(0, 0), topLevel) ?? new Point(0, 0);
        var local = new Point(clientPoint.X - layerOrigin.X, clientPoint.Y - layerOrigin.Y);
        return local;
    }

    private static bool ShouldShowPreviewContent(IDockable dockable)
    {
        return DockSettings.ShowDockablePreviewOnDrag;
    }

    private static Control? BuildManagedPreviewContent(IDockable dockable, bool showDockablePreview)
    {
        if (!showDockablePreview)
        {
            return null;
        }

        if (dockable is ManagedDockWindowDocument managedDockable)
        {
            return managedDockable.Build(null);
        }

        return null;
    }
}
