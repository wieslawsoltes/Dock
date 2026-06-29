// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using AvaloniaOrientation = Avalonia.Layout.Orientation;
using DockOrientation = Dock.Model.Core.Orientation;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Presents a proportional dock tree as a flat set of direct child visuals.
/// </summary>
public class FlatProportionalDockPanel : Panel
{
    private readonly Dictionary<IProportionalDock, DockableControl> _dockSurfaces = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IDockable, ContentPresenter> _presenters = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IProportionalDockSplitter, FlatProportionalDockSplitter> _splitters = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IProportionalDock, Rect> _dockBounds = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<INotifyPropertyChanged> _propertySubscriptions = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<INotifyCollectionChanged> _collectionSubscriptions = new(ReferenceEqualityComparer.Instance);
    private bool _isRebuilding;
    private bool _isAssigningProportions;

    /// <summary>
    /// Defines the <see cref="Dock"/> property.
    /// </summary>
    public static readonly StyledProperty<IProportionalDock?> DockProperty =
        AvaloniaProperty.Register<FlatProportionalDockPanel, IProportionalDock?>(nameof(Dock));

    /// <summary>
    /// Defines the <see cref="SplitterThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SplitterThicknessProperty =
        AvaloniaProperty.Register<FlatProportionalDockPanel, double>(nameof(SplitterThickness), 4.0);

    /// <summary>
    /// Defines the <see cref="MinimumProportionSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProportionSizeProperty =
        AvaloniaProperty.Register<FlatProportionalDockPanel, double>(nameof(MinimumProportionSize), 75.0);

    /// <summary>
    /// Gets or sets the root proportional dock to present.
    /// </summary>
    public IProportionalDock? Dock
    {
        get => GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    /// <summary>
    /// Gets or sets the default thickness assigned to flat splitters.
    /// </summary>
    public double SplitterThickness
    {
        get => GetValue(SplitterThicknessProperty);
        set => SetValue(SplitterThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum size a splitter keeps for each adjacent dockable.
    /// </summary>
    public double MinimumProportionSize
    {
        get => GetValue(MinimumProportionSizeProperty);
        set => SetValue(MinimumProportionSizeProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DockProperty)
        {
            RebuildVisualTree();
            return;
        }

        if (change.Property == SplitterThicknessProperty)
        {
            UpdateSplitterThickness();
            InvalidateMeasure();
            InvalidateArrange();
            return;
        }

        if (change.Property == MinimumProportionSizeProperty)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        UnsubscribeLayout();
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Dock is not { } dock)
        {
            return default;
        }

        MeasureDock(dock, availableSize);
        return NormalizeDesiredSize(availableSize);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            child.Arrange(default);
        }

        _dockBounds.Clear();

        if (Dock is { } dock)
        {
            ArrangeDock(dock, new Rect(finalSize));
        }

        return finalSize;
    }

    internal void ResizeSplitter(FlatProportionalDockSplitter splitterControl, double dragDelta)
    {
        if (splitterControl.OwnerDock is not { } ownerDock
            || splitterControl.Splitter is not { } splitter
            || ownerDock.VisibleDockables is not { } visibleDockables)
        {
            return;
        }

        var splitterIndex = visibleDockables.IndexOf(splitter);
        if (splitterIndex < 0)
        {
            return;
        }

        var target = FindResizeSibling(visibleDockables, splitterIndex, -1);
        var neighbor = FindResizeSibling(visibleDockables, splitterIndex, 1);
        if (target is null || neighbor is null)
        {
            return;
        }

        if (!_dockBounds.TryGetValue(ownerDock, out var ownerBounds))
        {
            ownerBounds = new Rect(Bounds.Size);
        }

        var availableSize = ownerDock.Orientation == DockOrientation.Vertical
            ? ownerBounds.Height
            : ownerBounds.Width;

        if (availableSize <= 0 || double.IsNaN(availableSize) || double.IsInfinity(availableSize))
        {
            return;
        }

        var targetProportion = ResolveValidProportion(target.Proportion, 0.5);
        var neighborProportion = ResolveValidProportion(neighbor.Proportion, 0.5);
        var deltaProportion = dragDelta / availableSize;

        if (targetProportion + deltaProportion < 0)
        {
            deltaProportion = -targetProportion;
        }

        if (neighborProportion - deltaProportion < 0)
        {
            deltaProportion = neighborProportion;
        }

        var nextTargetProportion = targetProportion + deltaProportion;
        var nextNeighborProportion = neighborProportion - deltaProportion;

        ApplyResizeConstraints(
            ownerDock.Orientation,
            availableSize,
            target,
            neighbor,
            ref nextTargetProportion,
            ref nextNeighborProportion);

        ApplyResizeConstraints(
            ownerDock.Orientation,
            availableSize,
            neighbor,
            target,
            ref nextNeighborProportion,
            ref nextTargetProportion);

        SetDockableProportion(target, Math.Max(0, nextTargetProportion), updateCollapsedProportion: true);
        SetDockableProportion(neighbor, Math.Max(0, nextNeighborProportion), updateCollapsedProportion: true);

        InvalidateMeasure();
        InvalidateArrange();
    }

    private void RebuildVisualTree()
    {
        if (_isRebuilding)
        {
            return;
        }

        _isRebuilding = true;
        try
        {
            UnsubscribeLayout();
            _dockSurfaces.Clear();
            _presenters.Clear();
            _splitters.Clear();
            _dockBounds.Clear();
            Children.Clear();

            if (Dock is { } dock)
            {
                AddDockSurfaces(dock);
                AddDockVisuals(dock);
                SubscribeLayout(dock);
            }
        }
        finally
        {
            _isRebuilding = false;
        }

        InvalidateMeasure();
        InvalidateArrange();
    }

    private void AddDockSurfaces(IProportionalDock dock)
    {
        var surface = CreateDockSurface(dock);
        _dockSurfaces[dock] = surface;
        Children.Add(surface);

        if (dock.VisibleDockables is null)
        {
            return;
        }

        foreach (var dockable in dock.VisibleDockables)
        {
            if (dockable is IProportionalDock childDock)
            {
                AddDockSurfaces(childDock);
            }
        }
    }

    private DockableControl CreateDockSurface(IProportionalDock dock)
    {
        var surface = new DockableControl
        {
            TrackingMode = TrackingMode.Visible,
            Background = Brushes.Transparent,
            DataContext = dock,
            [DockProperties.IsDropAreaProperty] = true
        };

        surface.Bind(DockProperties.IsDropEnabledProperty, new Binding(nameof(IDockable.CanDrop)));
        surface.Bind(DockProperties.DockGroupProperty, new Binding(nameof(IDockable.DockGroup)));

        return surface;
    }

    private void AddDockVisuals(IProportionalDock dock)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        foreach (var dockable in dock.VisibleDockables)
        {
            switch (dockable)
            {
                case IProportionalDockSplitter splitter:
                    AddSplitter(dock, splitter);
                    break;
                case IProportionalDock childDock:
                    AddDockVisuals(childDock);
                    break;
                default:
                    AddPresenter(dockable);
                    break;
            }
        }
    }

    private void AddSplitter(IProportionalDock ownerDock, IProportionalDockSplitter splitter)
    {
        var control = new FlatProportionalDockSplitter
        {
            DataContext = splitter,
            OwnerDock = ownerDock,
            Splitter = splitter,
            Orientation = ToAvaloniaOrientation(ownerDock.Orientation),
            Thickness = SplitterThickness
        };

        control.Bind(FlatProportionalDockSplitter.IsResizingEnabledProperty, new Binding(nameof(IProportionalDockSplitter.CanResize)));
        control.Bind(FlatProportionalDockSplitter.PreviewResizeProperty, new Binding(nameof(IProportionalDockSplitter.ResizePreview)));

        _splitters[splitter] = control;
        Children.Add(control);
    }

    private void AddPresenter(IDockable dockable)
    {
        var presenter = new ContentPresenter
        {
            Content = dockable,
            DataContext = dockable
        };

        _presenters[dockable] = presenter;
        Children.Add(presenter);
    }

    private void SubscribeLayout(IProportionalDock dock)
    {
        SubscribeDockable(dock);

        if (dock.VisibleDockables is INotifyCollectionChanged collectionChanged
            && _collectionSubscriptions.Add(collectionChanged))
        {
            collectionChanged.CollectionChanged += VisibleDockablesCollectionChanged;
        }

        if (dock.VisibleDockables is null)
        {
            return;
        }

        foreach (var dockable in dock.VisibleDockables)
        {
            SubscribeDockable(dockable);

            if (dockable is IProportionalDock childDock)
            {
                SubscribeLayout(childDock);
            }
        }
    }

    private void SubscribeDockable(IDockable dockable)
    {
        if (dockable is INotifyPropertyChanged propertyChanged
            && _propertySubscriptions.Add(propertyChanged))
        {
            propertyChanged.PropertyChanged += DockablePropertyChanged;
        }
    }

    private void UnsubscribeLayout()
    {
        foreach (var propertyChanged in _propertySubscriptions)
        {
            propertyChanged.PropertyChanged -= DockablePropertyChanged;
        }

        foreach (var collectionChanged in _collectionSubscriptions)
        {
            collectionChanged.CollectionChanged -= VisibleDockablesCollectionChanged;
        }

        _propertySubscriptions.Clear();
        _collectionSubscriptions.Clear();
    }

    private void VisibleDockablesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isRebuilding || _isAssigningProportions)
        {
            return;
        }

        RebuildVisualTree();
    }

    private void DockablePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isRebuilding || _isAssigningProportions)
        {
            return;
        }

        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName == nameof(IDock.VisibleDockables)
            || e.PropertyName == nameof(IProportionalDock.Orientation))
        {
            RebuildVisualTree();
            return;
        }

        InvalidateMeasure();
        InvalidateArrange();
    }

    private void UpdateSplitterThickness()
    {
        foreach (var splitter in _splitters.Values)
        {
            splitter.Thickness = SplitterThickness;
        }
    }

    private void MeasureDock(IProportionalDock dock, Size availableSize)
    {
        if (_dockSurfaces.TryGetValue(dock, out var surface))
        {
            surface.Measure(availableSize);
        }

        if (dock.VisibleDockables is not { } visibleDockables || visibleDockables.Count == 0)
        {
            return;
        }

        var splitterThickness = GetTotalSplitterThickness(visibleDockables);
        AssignProportions(dock, availableSize, splitterThickness);
        var availableLength = Math.Max(0, GetLength(availableSize, dock.Orientation) - splitterThickness);
        var sumOfFractions = 0.0;

        for (var i = 0; i < visibleDockables.Count; i++)
        {
            var dockable = visibleDockables[i];
            if (dockable is IProportionalDockSplitter splitter)
            {
                MeasureSplitter(dock, visibleDockables, i, splitter, availableSize);
                continue;
            }

            if (IsCollapsed(dockable))
            {
                MeasureCollapsed(dockable);
                continue;
            }

            var length = CalculateDimensionWithConstraints(
                dockable,
                dock.Orientation,
                availableLength,
                ResolveValidProportion(dockable.Proportion, 0),
                ref sumOfFractions);

            var childSize = CreateChildSize(availableSize, dock.Orientation, length);
            if (dockable is IProportionalDock childDock)
            {
                MeasureDock(childDock, childSize);
            }
            else if (_presenters.TryGetValue(dockable, out var presenter))
            {
                presenter.Measure(childSize);
            }
        }
    }

    private void MeasureSplitter(
        IProportionalDock dock,
        IList<IDockable> visibleDockables,
        int index,
        IProportionalDockSplitter splitter,
        Size availableSize)
    {
        if (!_splitters.TryGetValue(splitter, out var splitterControl))
        {
            return;
        }

        splitterControl.Orientation = ToAvaloniaOrientation(dock.Orientation);

        if (!ShouldUseSplitter(visibleDockables, index))
        {
            splitterControl.Measure(default);
            return;
        }

        var size = dock.Orientation == DockOrientation.Vertical
            ? new Size(availableSize.Width, splitterControl.Thickness)
            : new Size(splitterControl.Thickness, availableSize.Height);

        splitterControl.Measure(size);
    }

    private void MeasureCollapsed(IDockable dockable)
    {
        switch (dockable)
        {
            case IProportionalDock dock:
                MeasureDock(dock, default);
                break;
            default:
                if (_presenters.TryGetValue(dockable, out var presenter))
                {
                    presenter.Measure(default);
                }
                break;
        }
    }

    private void ArrangeDock(IProportionalDock dock, Rect bounds)
    {
        _dockBounds[dock] = bounds;

        if (_dockSurfaces.TryGetValue(dock, out var surface))
        {
            surface.Arrange(bounds);
        }

        if (dock.VisibleDockables is not { } visibleDockables || visibleDockables.Count == 0)
        {
            return;
        }

        var splitterThickness = GetTotalSplitterThickness(visibleDockables);
        AssignProportions(dock, bounds.Size, splitterThickness);
        var availableLength = Math.Max(0, GetLength(bounds.Size, dock.Orientation) - splitterThickness);
        var offset = 0.0;
        var sumOfFractions = 0.0;

        for (var i = 0; i < visibleDockables.Count; i++)
        {
            var dockable = visibleDockables[i];

            if (dockable is IProportionalDockSplitter splitter)
            {
                ArrangeSplitter(dock, visibleDockables, i, splitter, bounds, ref offset);
                continue;
            }

            if (IsCollapsed(dockable))
            {
                continue;
            }

            var length = CalculateDimensionWithConstraints(
                dockable,
                dock.Orientation,
                availableLength,
                ResolveValidProportion(dockable.Proportion, 0),
                ref sumOfFractions);

            var childBounds = CreateChildRect(bounds, dock.Orientation, offset, length);
            offset += length;

            if (dockable is IProportionalDock childDock)
            {
                ArrangeDock(childDock, childBounds);
            }
            else if (_presenters.TryGetValue(dockable, out var presenter))
            {
                presenter.Arrange(childBounds);
            }
        }
    }

    private void ArrangeSplitter(
        IProportionalDock dock,
        IList<IDockable> visibleDockables,
        int index,
        IProportionalDockSplitter splitter,
        Rect bounds,
        ref double offset)
    {
        if (!_splitters.TryGetValue(splitter, out var splitterControl))
        {
            return;
        }

        splitterControl.Orientation = ToAvaloniaOrientation(dock.Orientation);

        if (!ShouldUseSplitter(visibleDockables, index))
        {
            return;
        }

        var thickness = splitterControl.Thickness;
        var splitterBounds = CreateChildRect(bounds, dock.Orientation, offset, thickness);
        offset += thickness;
        splitterControl.Arrange(splitterBounds);
    }

    private double GetTotalSplitterThickness(IList<IDockable> visibleDockables)
    {
        var total = 0.0;

        for (var i = 0; i < visibleDockables.Count; i++)
        {
            if (visibleDockables[i] is IProportionalDockSplitter splitter
                && ShouldUseSplitter(visibleDockables, i)
                && _splitters.TryGetValue(splitter, out var splitterControl))
            {
                total += splitterControl.Thickness;
            }
        }

        return total;
    }

    private void AssignProportions(IProportionalDock dock, Size size, double splitterThickness)
    {
        if (dock.VisibleDockables is not { } visibleDockables)
        {
            return;
        }

        var dockables = new List<IDockable>();
        foreach (var dockable in visibleDockables)
        {
            if (dockable is not IProportionalDockSplitter)
            {
                dockables.Add(dockable);
            }
        }

        if (dockables.Count == 0)
        {
            return;
        }

        _isAssigningProportions = true;
        try
        {
            var availableLength = Math.Max(1.0, GetLength(size, dock.Orientation) - splitterThickness);
            var hasCollapsed = false;
            var assignedTotal = 0.0;
            var unassignedCount = 0;
            var targets = new Dictionary<IDockable, double>(ReferenceEqualityComparer.Instance);

            foreach (var dockable in dockables)
            {
                if (IsCollapsed(dockable))
                {
                    hasCollapsed = true;
                    if (IsValidProportion(dockable.Proportion) && dockable.Proportion > 0)
                    {
                        dockable.CollapsedProportion = dockable.Proportion;
                    }

                    targets[dockable] = 0.0;
                    continue;
                }

                var target = IsValidProportion(dockable.CollapsedProportion)
                    ? dockable.CollapsedProportion
                    : dockable.Proportion;

                if (IsValidProportion(target))
                {
                    assignedTotal += target;
                    targets[dockable] = target;
                }
                else
                {
                    unassignedCount++;
                    targets[dockable] = double.NaN;
                }
            }

            if (unassignedCount > 0)
            {
                var remaining = Math.Max(0, 1.0 - assignedTotal);
                var proportion = remaining / unassignedCount;
                foreach (var dockable in dockables)
                {
                    if (!IsCollapsed(dockable) && !IsValidProportion(targets[dockable]))
                    {
                        targets[dockable] = proportion;
                    }
                }
            }

            NormalizeActiveProportions(dockables, targets);

            foreach (var dockable in dockables)
            {
                var target = ClampProportion(dockable, dock.Orientation, availableLength, targets[dockable]);
                SetDockableProportion(dockable, target, !IsCollapsed(dockable) && !hasCollapsed);
            }
        }
        finally
        {
            _isAssigningProportions = false;
        }
    }

    private static void NormalizeActiveProportions(IList<IDockable> dockables, IDictionary<IDockable, double> targets)
    {
        var total = 0.0;
        foreach (var dockable in dockables)
        {
            if (!IsCollapsed(dockable))
            {
                total += targets[dockable];
            }
        }

        if (total <= 0 || Math.Abs(total - 1.0) < 1e-10)
        {
            return;
        }

        var scale = 1.0 / total;
        foreach (var dockable in dockables)
        {
            if (!IsCollapsed(dockable))
            {
                targets[dockable] *= scale;
            }
        }
    }

    private double ClampProportion(IDockable dockable, DockOrientation orientation, double availableLength, double proportion)
    {
        if (!IsValidProportion(proportion))
        {
            return proportion;
        }

        var min = GetMinimumLength(dockable, orientation);
        var max = GetMaximumLength(dockable, orientation);
        var minProportion = MinimumProportionSize > 0 ? MinimumProportionSize / availableLength : 0.0;
        var maxProportion = double.PositiveInfinity;

        if (!double.IsNaN(min) && min > 0)
        {
            minProportion = Math.Max(minProportion, min / availableLength);
        }

        if (!double.IsNaN(max) && !double.IsPositiveInfinity(max) && max > 0)
        {
            maxProportion = max / availableLength;
        }

        if (maxProportion < minProportion)
        {
            maxProportion = minProportion;
        }

        return Math.Clamp(proportion, minProportion, maxProportion);
    }

    private void ApplyResizeConstraints(
        DockOrientation orientation,
        double availableSize,
        IDockable primary,
        IDockable secondary,
        ref double primaryProportion,
        ref double secondaryProportion)
    {
        var primaryConstraints = GetProportionConstraints(primary, orientation, availableSize);
        var secondaryConstraints = GetProportionConstraints(secondary, orientation, availableSize);

        if (primaryProportion < primaryConstraints.Min)
        {
            var deficit = primaryConstraints.Min - primaryProportion;
            primaryProportion = primaryConstraints.Min;
            secondaryProportion = Math.Max(secondaryConstraints.Min, secondaryProportion - deficit);
        }
        else if (primaryProportion > primaryConstraints.Max)
        {
            var excess = primaryProportion - primaryConstraints.Max;
            primaryProportion = primaryConstraints.Max;
            secondaryProportion = Math.Min(secondaryConstraints.Max, secondaryProportion + excess);
        }
    }

    private (double Min, double Max) GetProportionConstraints(IDockable dockable, DockOrientation orientation, double availableSize)
    {
        var min = GetMinimumLength(dockable, orientation);
        var max = GetMaximumLength(dockable, orientation);
        var minProportion = MinimumProportionSize > 0 ? MinimumProportionSize / availableSize : 0.0;
        var maxProportion = double.PositiveInfinity;

        if (!double.IsNaN(min) && min > 0)
        {
            minProportion = Math.Max(minProportion, min / availableSize);
        }

        if (!double.IsNaN(max) && !double.IsPositiveInfinity(max) && max > 0)
        {
            maxProportion = max / availableSize;
        }

        if (maxProportion < minProportion)
        {
            maxProportion = minProportion;
        }

        return (minProportion, maxProportion);
    }

    private static IDockable? FindResizeSibling(IList<IDockable> dockables, int splitterIndex, int direction)
    {
        for (var index = splitterIndex + direction; index >= 0 && index < dockables.Count; index += direction)
        {
            var dockable = dockables[index];
            if (dockable is IProportionalDockSplitter || IsCollapsed(dockable))
            {
                continue;
            }

            return dockable;
        }

        return null;
    }

    private static bool ShouldUseSplitter(IList<IDockable> dockables, int splitterIndex)
    {
        if (dockables[splitterIndex] is not IProportionalDockSplitter)
        {
            return false;
        }

        var previous = FindAdjacentDockable(dockables, splitterIndex, -1);
        var next = FindAdjacentDockable(dockables, splitterIndex, 1);

        return previous is not null
               && next is not null
               && !IsCollapsed(previous)
               && !IsCollapsed(next);
    }

    private static IDockable? FindAdjacentDockable(IList<IDockable> dockables, int splitterIndex, int direction)
    {
        var index = splitterIndex + direction;
        if (index < 0 || index >= dockables.Count)
        {
            return null;
        }

        var dockable = dockables[index];
        return dockable is IProportionalDockSplitter ? null : dockable;
    }

    private static Size NormalizeDesiredSize(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;
        return new Size(width, height);
    }

    private static Size CreateChildSize(Size availableSize, DockOrientation orientation, double length)
    {
        return orientation == DockOrientation.Vertical
            ? new Size(availableSize.Width, length)
            : new Size(length, availableSize.Height);
    }

    private static Rect CreateChildRect(Rect bounds, DockOrientation orientation, double offset, double length)
    {
        return orientation == DockOrientation.Vertical
            ? new Rect(bounds.X, bounds.Y + offset, bounds.Width, length)
            : new Rect(bounds.X + offset, bounds.Y, length, bounds.Height);
    }

    private static double CalculateDimensionWithConstraints(
        IDockable dockable,
        DockOrientation orientation,
        double dimension,
        double proportion,
        ref double sumOfFractions)
    {
        var calculated = CalculateDimension(dimension, proportion, ref sumOfFractions);
        var min = GetMinimumLength(dockable, orientation);
        var max = GetMaximumLength(dockable, orientation);

        if (!double.IsNaN(min) && calculated < min)
        {
            calculated = min;
        }

        if (!double.IsNaN(max) && !double.IsPositiveInfinity(max) && calculated > max)
        {
            calculated = max;
        }

        return calculated;
    }

    private static double CalculateDimension(double dimension, double proportion, ref double sumOfFractions)
    {
        var childDimension = dimension * proportion;
        var flooredChildDimension = Math.Floor(childDimension);
        sumOfFractions += childDimension - flooredChildDimension;

        var round = Math.Round(sumOfFractions, 1);
        var clamp = Math.Clamp(Math.Floor(sumOfFractions), 1, double.MaxValue);
        if (round - clamp >= 0)
        {
            sumOfFractions -= Math.Round(sumOfFractions);
            return Math.Max(0, flooredChildDimension + 1);
        }

        return Math.Max(0, flooredChildDimension);
    }

    private static double GetLength(Size size, DockOrientation orientation)
    {
        return orientation == DockOrientation.Vertical ? size.Height : size.Width;
    }

    private static double GetMinimumLength(IDockable dockable, DockOrientation orientation)
    {
        return orientation == DockOrientation.Vertical ? dockable.MinHeight : dockable.MinWidth;
    }

    private static double GetMaximumLength(IDockable dockable, DockOrientation orientation)
    {
        return orientation == DockOrientation.Vertical ? dockable.MaxHeight : dockable.MaxWidth;
    }

    private static bool IsCollapsed(IDockable dockable)
    {
        return dockable.IsCollapsable && dockable.IsEmpty;
    }

    private static bool IsValidProportion(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }

    private static double ResolveValidProportion(double value, double fallback)
    {
        return IsValidProportion(value) ? value : fallback;
    }

    private static void SetDockableProportion(IDockable dockable, double value, bool updateCollapsedProportion)
    {
        if (!AreClose(dockable.Proportion, value))
        {
            dockable.Proportion = value;
        }

        if (updateCollapsedProportion && !AreClose(dockable.CollapsedProportion, value))
        {
            dockable.CollapsedProportion = value;
        }
    }

    private static bool AreClose(double left, double right)
    {
        if (double.IsNaN(left) && double.IsNaN(right))
        {
            return true;
        }

        return Math.Abs(left - right) < 1e-10;
    }

    private static AvaloniaOrientation ToAvaloniaOrientation(DockOrientation orientation)
    {
        return orientation == DockOrientation.Vertical ? AvaloniaOrientation.Vertical : AvaloniaOrientation.Horizontal;
    }
}
