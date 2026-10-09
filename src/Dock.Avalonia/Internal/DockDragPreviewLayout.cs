using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

/// <summary>Temporarily arranges existing panes as if the dragged pane had been removed.</summary>
internal sealed class DockDragPreviewLayout : IDisposable
{
    private readonly List<ProportionalStackPanel> _panels = new();
    private readonly List<IDisposable> _visibility = new();
    private DockControl? _host;
    private IDockable? _root;
    private Rect _viewport;
    private double _scale;
    private int _version;
    private ProportionalStackPanelSplitter? _splitter;
    private double _thickness;
    private IDock? _tabPane;
    private IDockable? _originalActive;
    private IDockable? _temporaryActive;

    internal bool IsCurrent => _host is { } host && _root is { } root
        && host.PreviewViewport == _viewport && LayoutHelper.GetLayoutScale(host).Equals(_scale)
        && (_splitter?.Thickness ?? host.PreviewSplitterThickness).Equals(_thickness)
        && DockDropPreviewService.Fingerprint(root) == _version;

    internal static DockDragPreviewLayout? TryCreate(IDockable source, DockControl host)
    {
        var pane = source as IDock ?? source.Owner as IDock;
        if (pane is not (IToolDock or IDocumentDock) || host.GetVisualRoot() is null)
            return null;
        if (source is not IDock && pane.VisibleDockables is { Count: > 1 })
            return CreateTabPreview(source, pane, host);
        if (!pane.IsCollapsable || (source is not IDock && pane.VisibleDockables?.Count != 1))
            return null;
        var projection = DockSplitPreview.CreateSourceRemoval(source);
        if (projection is null) return null;
        var thickness = host.PreviewSplitterThickness;
        ProportionalStackPanelSplitter? liveSplitter = null;
        foreach (var visual in host.GetVisualDescendants())
            if (visual is ProportionalStackPanelSplitter splitter)
            {
                thickness = splitter.Thickness;
                liveSplitter = splitter;
                break;
            }
        var bounds = DockPreviewLayout.MeasureSourceRemoval(projection, host.PreviewViewport.Size, thickness, host);
        if (bounds is null) return null;
        var root = DockDropPreviewService.LayoutRoot(source);
        var session = new DockDragPreviewLayout
        {
            _host = host, _root = root, _viewport = host.PreviewViewport,
            _splitter = liveSplitter, _thickness = thickness,
            _scale = LayoutHelper.GetLayoutScale(host), _version = DockDropPreviewService.Fingerprint(root)
        };
        foreach (var visual in host.GetVisualDescendants())
        {
            if (visual is not ProportionalStackPanel panel
                || !ReferenceEquals(panel.FindAncestorOfType<DockControl>(), host)
                || panel.DataContext is not IProportionalDock model)
                continue;
            var parentBounds = FindBounds(model, bounds);
            var slots = new Dictionary<Control, Rect>();
            foreach (var child in panel.Children)
            {
                var childBounds = child.DataContext is IDockable childModel ? FindBounds(childModel, bounds) : null;
                if (parentBounds is { } parent && childBounds is { } rect)
                    slots[child] = new Rect(rect.Position - parent.Position, rect.Size);
                else
                    session._visibility.Add(child.SetValue(Visual.IsVisibleProperty, false, BindingPriority.Animation)!);
            }
            panel.SetPreviewSlots(slots);
            session._panels.Add(panel);
        }
        host.DetachedFromVisualTree += session.OnDetached;
        host.UpdateLayout();
        // Application templates may impose constraints on a wrapper that the real
        // removal would eliminate. Do not leave a partially reflowed preview behind.
        foreach (var pair in bounds)
        {
            if (pair.Key is not (IToolDock or IDocumentDock)
                || pair.Key.Factory?.VisibleDockableControls.TryGetValue(pair.Key, out var visible) != true
                || visible is not Control control) continue;
            var expected = new Rect(pair.Value.Position + (Vector)host.PreviewViewport.Position, pair.Value.Size);
            var position = control.TranslatePoint(default, host);
            if (position is null || Math.Abs(position.Value.X - expected.X) > 1
                || Math.Abs(position.Value.Y - expected.Y) > 1
                || Math.Abs(control.Bounds.Width - expected.Width) > 1
                || Math.Abs(control.Bounds.Height - expected.Height) > 1)
            {
                session.Dispose();
                host.UpdateLayout();
                return null;
            }
        }
        return session;
    }

    private static DockDragPreviewLayout CreateTabPreview(IDockable source, IDock pane, DockControl host)
    {
        var session = new DockDragPreviewLayout
        {
            _host = host, _root = DockDropPreviewService.LayoutRoot(source),
            _viewport = host.PreviewViewport, _scale = LayoutHelper.GetLayoutScale(host),
            _thickness = host.PreviewSplitterThickness
        };
        // Retain ownership and collection order until the drop commits. Normal tab
        // selection lets custom and cached-content templates reveal the remaining item.
        foreach (var visual in host.GetVisualDescendants())
            if (visual is Control control && control is ToolTabStripItem or DocumentTabStripItem
                && ReferenceEquals(control.DataContext, source))
                session._visibility.Add(control.SetValue(Visual.IsVisibleProperty, false, BindingPriority.Animation)!);
        if (ReferenceEquals(pane.ActiveDockable, source))
        {
            var index = pane.VisibleDockables!.IndexOf(source);
            session._tabPane = pane;
            session._originalActive = source;
            session._temporaryActive = pane.VisibleDockables[index > 0 ? index - 1 : 1];
            pane.ActiveDockable = session._temporaryActive;
        }
        session._version = DockDropPreviewService.Fingerprint(session._root);
        host.DetachedFromVisualTree += session.OnDetached;
        host.UpdateLayout();
        return session;
    }

    private static Rect? FindBounds(IDockable model, Dictionary<IDockable, Rect> bounds)
    {
        if (bounds.TryGetValue(model, out var rect)) return rect;
        // Removal can promote the remaining child of a proportional container.
        // Keep the original visual wrapper, using the promoted children's extent.
        Rect? union = null;
        if (model is IProportionalDock { VisibleDockables: { } children })
            foreach (var child in children)
                if (FindBounds(child, bounds) is { } childBounds)
                    union = union is { } previous ? previous.Union(childBounds) : childBounds;
        return union;
    }

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs args) => Dispose();

    public void Dispose()
    {
        if (_host is not { } host) return;
        _host = null;
        _root = null;
        _splitter = null;
        host.DetachedFromVisualTree -= OnDetached;
        foreach (var panel in _panels) panel.SetPreviewSlots(null);
        foreach (var visibility in _visibility) visibility.Dispose();
        _panels.Clear();
        _visibility.Clear();
        if (_tabPane is { } pane && ReferenceEquals(pane.ActiveDockable, _temporaryActive)
            && _originalActive is { } original && pane.VisibleDockables?.Contains(original) == true)
            pane.ActiveDockable = original;
        _tabPane = null;
        _originalActive = null;
        _temporaryActive = null;
    }
}
