using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
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
    private DockControl? _control;
    private Rect _viewport;
    private ProportionalStackPanelSplitter? _splitter;
    private double _splitterThickness;
    private double _scaling;
    private int _version;
    private Rect? _bounds;
    private bool _active;

    internal bool HasProjection => _active;
    internal void Deactivate() => _active = false;

    internal bool IsCurrent(IDockable source, IDockable target, DockOperation operation, DockControl control, double proportion) =>
        ReferenceEquals(source, _source) && ReferenceEquals(target, _target) && operation == _operation
        && proportion.Equals(_proportion) && ReferenceEquals(control, _control)
        && LayoutHelper.GetLayoutScale(control).Equals(_scaling)
        && control.PreviewViewport == _viewport && GetSplitterThickness(control).Equals(_splitterThickness)
        && HashCode.Combine(Fingerprint(LayoutRoot(source)), Fingerprint(LayoutRoot(target))) == _version;

    internal Rect? GetBounds(IDockable source, IDockable target, DockOperation operation, DockControl dockControl, double proportion)
    {
        var viewport = dockControl.PreviewViewport;
        var thickness = GetSplitterThickness(dockControl);
        var scaling = LayoutHelper.GetLayoutScale(dockControl);
        var sourceRoot = LayoutRoot(source);
        var targetRoot = LayoutRoot(target);
        var version = HashCode.Combine(Fingerprint(sourceRoot), Fingerprint(targetRoot));
        if (ReferenceEquals(source, _source) && ReferenceEquals(target, _target) && operation == _operation
            && proportion.Equals(_proportion) && ReferenceEquals(dockControl, _control)
            && viewport == _viewport && scaling.Equals(_scaling) && thickness.Equals(_splitterThickness) && version == _version)
        {
            _active = _bounds.HasValue;
            return _bounds;
        }

        _source = source;
        _target = target;
        _operation = operation;
        _proportion = proportion;
        _control = dockControl;
        _viewport = viewport;
        _splitterThickness = thickness;
        _scaling = scaling;
        _version = version;
        _bounds = null;
        _active = false;
        if (viewport.Width <= 0 || viewport.Height <= 0) return null;
        var preview = DockSplitPreview.Create(source, target, operation, proportion);
        if (preview is null) return null;
        var measured = DockPreviewLayout.Measure(preview, viewport.Size, thickness, dockControl);
        if (measured is { } bounds)
            _bounds = new Rect(bounds.Position + (Vector)viewport.Position, bounds.Size);
        _active = _bounds.HasValue;
        return _bounds;
    }

    internal void Clear()
    {
        _active = false;
        _source = _target = null;
        _control = null;
        _splitter = null;
        _bounds = null;
    }

    private double GetSplitterThickness(DockControl control)
    {
        // Retain the visual between hovers, but discard it if a template or layout replacement
        // removed it. Read the styled value every time, including immediately before release.
        if (_splitter is { } cached)
        {
            for (Visual? parent = cached; parent is not null; parent = parent.GetVisualParent())
                if (ReferenceEquals(parent, control)) return cached.Thickness;
            _splitter = null;
        }
        _splitter = FindSplitter(control);
        return _splitter?.Thickness ?? control.PreviewSplitterThickness;
    }

    private static ProportionalStackPanelSplitter? FindSplitter(Visual visual)
    {
        if (visual is ProportionalStackPanelSplitter splitter) return splitter;
        // Avalonia exposes its visual collection as IEnumerable; index the underlying list
        // so an unsplit layout also has an allocation-free hover path.
        var children = visual.GetVisualChildren();
        if (children is IReadOnlyList<Visual> list)
        {
            for (var index = 0; index < list.Count; index++)
                if (FindSplitter(list[index]) is { } found) return found;
        }
        else
        {
            foreach (var child in children)
                if (FindSplitter(child) is { } found) return found;
        }
        return null;
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
        hash.Add(model.Factory is { } factory ? RuntimeHelpers.GetHashCode(factory) : 0);
        hash.Add(model.Proportion);
        hash.Add(model.CollapsedProportion);
        hash.Add(model.IsCollapsable);
        hash.Add(model.IsEmpty);
        hash.Add(model.MinWidth);
        hash.Add(model.MaxWidth);
        hash.Add(model.MinHeight);
        hash.Add(model.MaxHeight);
        if (model is IRootDock) hash.Add(DockPreviewLayout.GetRootPadding(model));
        if (model is IProportionalDock proportional) hash.Add(proportional.Orientation);
        if (model is IEqualProportionalDock equal) hash.Add(equal.KeepProportionsEqual);
        if (model is IDock { VisibleDockables: { } children })
            for (var index = 0; index < children.Count; index++) hash.Add(Fingerprint(children[index]));
        return hash.ToHashCode();
    }
}
