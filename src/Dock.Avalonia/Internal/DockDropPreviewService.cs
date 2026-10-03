using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal sealed class DockDropPreviewService
{
    private IDockable? _source;
    private IDockable? _target;
    private DockOperation _operation;
    private double _proportion;
    private Size _size;
    private int _version;
    private Rect? _bounds;
    private bool _active;

    internal bool HasProjection => _active;
    internal void Deactivate() => _active = false;

    internal bool IsCurrent(IDockable source, IDockable target, DockOperation operation, DockControl control, double proportion) =>
        ReferenceEquals(source, _source) && ReferenceEquals(target, _target) && operation == _operation
        && proportion.Equals(_proportion) && control.Bounds.Size == _size
        && HashCode.Combine(Fingerprint(LayoutRoot(source)), Fingerprint(LayoutRoot(target))) == _version;

    internal Rect? GetBounds(IDockable source, IDockable target, DockOperation operation, DockControl dockControl, double proportion)
    {
        var size = dockControl.Bounds.Size;
        var sourceRoot = LayoutRoot(source);
        var targetRoot = LayoutRoot(target);
        var version = HashCode.Combine(Fingerprint(sourceRoot), Fingerprint(targetRoot));
        if (ReferenceEquals(source, _source) && ReferenceEquals(target, _target) && operation == _operation
            && proportion.Equals(_proportion) && size == _size && version == _version)
        {
            _active = _bounds.HasValue;
            return _bounds;
        }

        _source = source;
        _target = target;
        _operation = operation;
        _proportion = proportion;
        _size = size;
        _version = version;
        _bounds = null;
        _active = false;
        if (size.Width <= 0 || size.Height <= 0) return null;
        var preview = DockSplitPreview.Create(source, target, operation, proportion);
        if (preview is null) return null;
        var thickness = 4.0;
        foreach (var visual in dockControl.GetVisualDescendants())
        {
            if (visual is ProportionalStackPanelSplitter splitter) { thickness = splitter.Thickness; break; }
        }
        _bounds = DockPreviewLayout.Measure(preview, size, thickness);
        _active = _bounds.HasValue;
        return _bounds;
    }

    internal void Clear()
    {
        _active = false;
        _source = _target = null;
        _bounds = null;
    }

    private static IDockable LayoutRoot(IDockable dockable)
    {
        while (dockable.Owner is IDock owner) dockable = owner;
        return dockable;
    }

    private static int Fingerprint(IDockable model)
    {
        var hash = new HashCode();
        hash.Add(RuntimeHelpers.GetHashCode(model));
        hash.Add(model.Proportion);
        hash.Add(model.CollapsedProportion);
        hash.Add(model.IsCollapsable);
        hash.Add(model.IsEmpty);
        hash.Add(model.MinWidth);
        hash.Add(model.MaxWidth);
        hash.Add(model.MinHeight);
        hash.Add(model.MaxHeight);
        if (model is IProportionalDock proportional) hash.Add(proportional.Orientation);
        if (model is IDock { VisibleDockables: { } children })
            for (var index = 0; index < children.Count; index++) hash.Add(Fingerprint(children[index]));
        return hash.ToHashCode();
    }
}
