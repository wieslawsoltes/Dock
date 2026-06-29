// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Threading;
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
    private static readonly CubicEaseOut s_layoutTransitionEasing = new();
    private static readonly TimeSpan s_transitionCompletionSlack = TimeSpan.FromMilliseconds(16);

    private readonly Dictionary<IProportionalDock, DockableControl> _dockSurfaces = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IDockable, ContentPresenter> _presenters = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IProportionalDockSplitter, FlatProportionalDockSplitter> _splitters = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IProportionalDock, Rect> _dockBounds = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Control, Rect> _connectedStartBounds = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Control, ConnectedAnimation> _pendingConnectedAnimations = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Control, int> _connectedAnimationVersions = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<Control> _activeConnectedControls = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ContentPresenter> _enteringPresenters = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ContentPresenter> _pendingInsertAnimations = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ContentPresenter, int> _insertAnimationVersions = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ContentPresenter> _activeInsertPresenters = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ContentPresenter, ConnectedAnimation> _pendingLayoutAnimations = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ContentPresenter, int> _layoutAnimationVersions = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ContentPresenter> _activeLayoutPresenters = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ContentPresenter, Rect> _removalPresenterBounds = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IDockable, ContentPresenter> _removalPresentersByDockable = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ContentPresenter> _pendingRemovalPresenters = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ContentPresenter> _activeRemovalPresenters = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<IDockable> _layoutActionDockables = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<Control> _arrangedChildren = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<INotifyPropertyChanged> _propertySubscriptions = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<INotifyCollectionChanged> _collectionSubscriptions = new(ReferenceEqualityComparer.Instance);
    private Dictionary<IProportionalDock, DockableControl>? _reusableDockSurfaces;
    private Dictionary<IDockable, ContentPresenter>? _reusablePresenters;
    private Dictionary<IProportionalDockSplitter, FlatProportionalDockSplitter>? _reusableSplitters;
    private bool _isRebuilding;
    private bool _isAssigningProportions;
    private bool _hasCompletedArrange;
    private bool _hasPendingConnectedAnimationPost;
    private bool _hasPendingRebuild;
    private bool _suppressLayoutTransitions;
    private bool _hasScopedLayoutAction;

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
    /// Defines the <see cref="UseLayoutTransitions"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> UseLayoutTransitionsProperty =
        AvaloniaProperty.Register<FlatProportionalDockPanel, bool>(nameof(UseLayoutTransitions), true);

    /// <summary>
    /// Defines the <see cref="LayoutTransitionDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> LayoutTransitionDurationProperty =
        AvaloniaProperty.Register<FlatProportionalDockPanel, TimeSpan>(
            nameof(LayoutTransitionDuration),
            TimeSpan.FromMilliseconds(240));

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

    /// <summary>
    /// Gets or sets whether flat child bounds changes should animate on the compositor.
    /// </summary>
    public bool UseLayoutTransitions
    {
        get => GetValue(UseLayoutTransitionsProperty);
        set => SetValue(UseLayoutTransitionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the duration used for flat child bounds animations.
    /// </summary>
    public TimeSpan LayoutTransitionDuration
    {
        get => GetValue(LayoutTransitionDurationProperty);
        set => SetValue(LayoutTransitionDurationProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DockProperty)
        {
            RequestRebuildVisualTree();
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
            return;
        }

        if (change.Property == UseLayoutTransitionsProperty)
        {
            if (change.NewValue is false)
            {
                CancelLayoutTransitions(removeRemovalPresenters: true);
            }

            return;
        }

        if (change.Property == LayoutTransitionDurationProperty
            && LayoutTransitionDuration <= TimeSpan.Zero)
        {
            CancelLayoutTransitions(removeRemovalPresenters: true);
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        UnsubscribeLayout();
        CancelLayoutTransitions(removeRemovalPresenters: true);
        _hasPendingRebuild = false;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        ExecutePendingRebuild(invalidateLayout: false);

        if (Dock is not { } dock)
        {
            return default;
        }

        MeasureDock(dock, availableSize);
        MeasureRemovalPresenters();
        return NormalizeDesiredSize(availableSize);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        ExecutePendingRebuild(invalidateLayout: false);
        TrackStructuralLayoutActionDockables();

        _arrangedChildren.Clear();
        _dockBounds.Clear();

        if (Dock is { } dock)
        {
            ArrangeDock(dock, new Rect(finalSize));
        }

        ArrangeRemovalPresenters();

        foreach (var child in Children)
        {
            if (!_arrangedChildren.Contains(child))
            {
                child.Arrange(default);
            }
        }

        _arrangedChildren.Clear();
        _hasCompletedArrange = true;
        _suppressLayoutTransitions = false;
        RequestConnectedAnimations();
        ClearScopedLayoutAction();

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

        _suppressLayoutTransitions = true;
        InvalidateMeasure();
        InvalidateArrange();
    }

    private void RequestRebuildVisualTree()
    {
        if (_isRebuilding)
        {
            return;
        }

        _hasPendingRebuild = true;
        InvalidateMeasure();
        InvalidateArrange();
        Dispatcher.UIThread.Post(() => ExecutePendingRebuild(invalidateLayout: true), DispatcherPriority.Render);
    }

    private void ExecutePendingRebuild(bool invalidateLayout)
    {
        if (!_hasPendingRebuild || _isRebuilding)
        {
            return;
        }

        _hasPendingRebuild = false;
        RebuildVisualTree(invalidateLayout);
    }

    private void RebuildVisualTree(bool invalidateLayout = true)
    {
        if (_isRebuilding)
        {
            return;
        }

        _isRebuilding = true;
        try
        {
            CaptureConnectedStartBounds();
            TrackStructuralLayoutActionDockables();
            UnsubscribeLayout();
            _reusableDockSurfaces = new Dictionary<IProportionalDock, DockableControl>(_dockSurfaces, ReferenceEqualityComparer.Instance);
            _reusablePresenters = new Dictionary<IDockable, ContentPresenter>(_presenters, ReferenceEqualityComparer.Instance);
            _reusableSplitters = new Dictionary<IProportionalDockSplitter, FlatProportionalDockSplitter>(_splitters, ReferenceEqualityComparer.Instance);
            _dockSurfaces.Clear();
            _presenters.Clear();
            _splitters.Clear();
            _dockBounds.Clear();

            if (Dock is { } dock)
            {
                AddDockSurfaces(dock);
                AddDockVisuals(dock);
                CreateRemovalPresenters();
                RemoveUnusedVisuals();
                AddRemovalPresenters();
                SubscribeLayout(dock);
            }
            else
            {
                RemoveUnusedVisuals();
                RemoveRemovalPresenters();
            }
        }
        finally
        {
            _reusableDockSurfaces = null;
            _reusablePresenters = null;
            _reusableSplitters = null;
            _isRebuilding = false;
        }

        if (invalidateLayout)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }
    }

    private void AddDockSurfaces(IProportionalDock dock)
    {
        var surface = CreateDockSurface(dock);
        _dockSurfaces[dock] = surface;
        EnsureSurfaceChild(surface);

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
        if (_reusableDockSurfaces?.Remove(dock, out var reusableSurface) == true)
        {
            reusableSurface.DataContext = dock;
            return reusableSurface;
        }

        var surface = new DockableControl
        {
            TrackingMode = TrackingMode.Visible,
            Background = Brushes.Transparent,
            DataContext = dock,
            [DockProperties.IsDropAreaProperty] = true,
            [DockProperties.IsDockTargetProperty] = true
        };

        DockProperties.SetDockAdornerHost(surface, surface);

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
        FlatProportionalDockSplitter? reusableSplitter = null;
        var reused = _reusableSplitters is not null && _reusableSplitters.Remove(splitter, out reusableSplitter);
        var control = reused
            ? reusableSplitter!
            : new FlatProportionalDockSplitter
            {
                DataContext = splitter
            };

        control.OwnerDock = ownerDock;
        control.Splitter = splitter;
        control.Orientation = ToAvaloniaOrientation(ownerDock.Orientation);
        control.Thickness = SplitterThickness;

        if (control.DataContext is not IProportionalDockSplitter)
        {
            control.DataContext = splitter;
        }

        if (!reused)
        {
            control.Bind(FlatProportionalDockSplitter.IsResizingEnabledProperty, new Binding(nameof(IProportionalDockSplitter.CanResize)));
            control.Bind(FlatProportionalDockSplitter.PreviewResizeProperty, new Binding(nameof(IProportionalDockSplitter.ResizePreview)));
        }

        _splitters[splitter] = control;
        EnsureDockVisualChild(control);
    }

    private void AddPresenter(IDockable dockable)
    {
        ContentPresenter? reusablePresenter = null;
        var reused = _reusablePresenters is not null && _reusablePresenters.Remove(dockable, out reusablePresenter);
        if (!reused && _removalPresentersByDockable.Remove(dockable, out reusablePresenter))
        {
            reused = true;
            _removalPresenterBounds.Remove(reusablePresenter);
            _pendingRemovalPresenters.Remove(reusablePresenter);
            _activeRemovalPresenters.Remove(reusablePresenter);
            ResetPresenterComposition(reusablePresenter);
        }

        var presenter = reused
            ? reusablePresenter!
            : new ContentPresenter();

        presenter.Content = dockable;
        presenter.DataContext = dockable;

        if (reused)
        {
            if (_activeInsertPresenters.Contains(presenter))
            {
                _enteringPresenters.Remove(presenter);
                presenter.Opacity = 1.0;
                presenter.IsHitTestVisible = false;
            }
            else if (_pendingInsertAnimations.Contains(presenter))
            {
                presenter.Opacity = 0.0;
                presenter.IsHitTestVisible = false;
                _enteringPresenters.Add(presenter);
            }
            else if (_activeLayoutPresenters.Contains(presenter) || _pendingLayoutAnimations.ContainsKey(presenter))
            {
                _enteringPresenters.Remove(presenter);
                presenter.Opacity = 1.0;
                presenter.IsHitTestVisible = false;
            }
            else
            {
                _enteringPresenters.Remove(presenter);
                presenter.Opacity = 1.0;
                presenter.IsHitTestVisible = true;
            }
        }
        else if (_hasCompletedArrange && UseLayoutTransitions)
        {
            presenter.Opacity = 0.0;
            presenter.IsHitTestVisible = false;
            _enteringPresenters.Add(presenter);
        }
        else
        {
            presenter.Opacity = 1.0;
            presenter.IsHitTestVisible = true;
        }

        _presenters[dockable] = presenter;
        EnsureDockVisualChild(presenter);
    }

    private void CreateRemovalPresenters()
    {
        if (!UseLayoutTransitions
            || !_hasCompletedArrange
            || _reusablePresenters is null
            || _reusablePresenters.Count == 0)
        {
            return;
        }

        var removedPresenters = new List<KeyValuePair<IDockable, ContentPresenter>>(_reusablePresenters);
        foreach (var kvp in removedPresenters)
        {
            var dockable = kvp.Key;
            var presenter = kvp.Value;
            if (!_connectedStartBounds.TryGetValue(presenter, out var bounds) || !HasVisibleSize(bounds))
            {
                continue;
            }

            _reusablePresenters.Remove(dockable);
            _pendingInsertAnimations.Remove(presenter);
            _activeInsertPresenters.Remove(presenter);
            _insertAnimationVersions.Remove(presenter);
            _pendingLayoutAnimations.Remove(presenter);
            _layoutAnimationVersions.Remove(presenter);
            _activeLayoutPresenters.Remove(presenter);

            _removalPresenterBounds[presenter] = bounds;
            _removalPresentersByDockable[dockable] = presenter;
            _pendingRemovalPresenters.Add(presenter);
            presenter.Opacity = 1.0;
            presenter.IsHitTestVisible = false;
        }
    }

    private void AddRemovalPresenters()
    {
        foreach (var presenter in _removalPresenterBounds.Keys)
        {
            if (!Children.Contains(presenter))
            {
                Children.Add(presenter);
            }
        }
    }

    private void RemoveUnusedVisuals()
    {
        if (_reusableDockSurfaces is not null)
        {
            foreach (var surface in _reusableDockSurfaces.Values)
            {
                Children.Remove(surface);
                surface.DataContext = null;
            }
        }

        if (_reusableSplitters is not null)
        {
            foreach (var splitter in _reusableSplitters.Values)
            {
                Children.Remove(splitter);
                ClearConnectedControlState(splitter);
                ResetControlComposition(splitter);
                splitter.OwnerDock = null;
                splitter.Splitter = null;
                splitter.DataContext = null;
            }
        }

        if (_reusablePresenters is null)
        {
            return;
        }

        foreach (var presenter in _reusablePresenters.Values)
        {
            Children.Remove(presenter);
            _enteringPresenters.Remove(presenter);
            _pendingInsertAnimations.Remove(presenter);
            _activeInsertPresenters.Remove(presenter);
            _insertAnimationVersions.Remove(presenter);
            _pendingLayoutAnimations.Remove(presenter);
            _layoutAnimationVersions.Remove(presenter);
            _activeLayoutPresenters.Remove(presenter);
            presenter.Content = null;
            presenter.DataContext = null;
            presenter.Opacity = 1.0;
            presenter.IsHitTestVisible = true;
        }
    }

    private void RemoveRemovalPresenters()
    {
        var presenters = new List<ContentPresenter>(_removalPresenterBounds.Keys);
        foreach (var presenter in presenters)
        {
            RemoveRemovalPresenter(presenter);
        }
    }

    private void EnsureSurfaceChild(DockableControl surface)
    {
        if (Children.Contains(surface))
        {
            return;
        }

        var index = 0;
        while (index < Children.Count && Children[index] is DockableControl)
        {
            index++;
        }

        Children.Insert(index, surface);
    }

    private void EnsureDockVisualChild(Control control)
    {
        if (Children.Contains(control))
        {
            return;
        }

        var index = Children.Count;
        while (index > 0 && IsRemovalPresenter(Children[index - 1]))
        {
            index--;
        }

        Children.Insert(index, control);
    }

    private bool IsRemovalPresenter(Control control)
    {
        return control is ContentPresenter presenter && _removalPresenterBounds.ContainsKey(presenter);
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

        TrackLayoutActionDockables(e);
        RequestRebuildVisualTree();
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
            RequestRebuildVisualTree();
            return;
        }

        CaptureConnectedStartBounds();
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
            ArrangeChild(surface, bounds, useConnectedAnimation: false);
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
                ArrangeChild(presenter, childBounds, useConnectedAnimation: true);
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
        ArrangeChild(splitterControl, splitterBounds, useConnectedAnimation: true);
    }

    private void MeasureRemovalPresenters()
    {
        foreach (var kvp in _removalPresenterBounds)
        {
            kvp.Key.Measure(kvp.Value.Size);
        }
    }

    private void ArrangeRemovalPresenters()
    {
        foreach (var kvp in _removalPresenterBounds)
        {
            var presenter = kvp.Key;
            var bounds = kvp.Value;
            presenter.Measure(bounds.Size);
            presenter.Arrange(bounds);
            _arrangedChildren.Add(presenter);
        }
    }

    private void ArrangeChild(Control control, Rect bounds, bool useConnectedAnimation)
    {
        var previousBounds = _connectedStartBounds.TryGetValue(control, out var connectedStartBounds)
            ? connectedStartBounds
            : control.Bounds;

        control.Arrange(bounds);
        _arrangedChildren.Add(control);

        if (useConnectedAnimation)
        {
            if (control is ContentPresenter presenter && _enteringPresenters.Remove(presenter))
            {
                QueueInsertAnimation(presenter, bounds);
                QueueConnectedAnimation(presenter, previousBounds, bounds);
                return;
            }

            QueueConnectedAnimation(control, previousBounds, bounds);
        }
    }

    private void QueueInsertAnimation(ContentPresenter presenter, Rect bounds)
    {
        if (!UseLayoutTransitions
            || _suppressLayoutTransitions
            || !_hasCompletedArrange
            || LayoutTransitionDuration <= TimeSpan.Zero
            || !HasVisibleSize(bounds))
        {
            CompleteInsertAnimation(presenter);
            return;
        }

        _pendingInsertAnimations.Add(presenter);
    }

    private void QueueConnectedAnimation(Control control, Rect previousBounds, Rect nextBounds)
    {
        if (!UseLayoutTransitions
            || _suppressLayoutTransitions
            || !_hasCompletedArrange
            || LayoutTransitionDuration <= TimeSpan.Zero
            || !HasVisibleSize(nextBounds))
        {
            return;
        }

        if (control is ContentPresenter presenter)
        {
            if (!ShouldAnimateLayoutChange(presenter))
            {
                CompleteScopedExcludedPresenter(presenter);
                return;
            }

            var fromBounds = _pendingLayoutAnimations.TryGetValue(presenter, out var pendingLayoutAnimation)
                ? pendingLayoutAnimation.From
                : previousBounds;

            if (!HasVisibleSize(fromBounds))
            {
                return;
            }

            if (AreClose(fromBounds, nextBounds))
            {
                _pendingLayoutAnimations.Remove(presenter);
                if (_activeLayoutPresenters.Contains(presenter))
                {
                    CompleteLayoutAnimation(presenter);
                    return;
                }

                if (!_pendingInsertAnimations.Contains(presenter)
                    && !_activeInsertPresenters.Contains(presenter)
                    && !_activeLayoutPresenters.Contains(presenter))
                {
                    presenter.Opacity = 1.0;
                    presenter.IsHitTestVisible = true;
                }

                return;
            }

            var isPendingInsert = _pendingInsertAnimations.Contains(presenter);
            if (_activeLayoutPresenters.Contains(presenter))
            {
                NextLayoutAnimationVersion(presenter);
            }

            _pendingLayoutAnimations[presenter] = new ConnectedAnimation(fromBounds, nextBounds);
            presenter.Opacity = isPendingInsert ? 0.0 : 1.0;
            presenter.IsHitTestVisible = false;
            return;
        }

        if (IsScopedLayoutActionActive())
        {
            CompleteScopedExcludedControl(control);
            return;
        }

        var connectedFromBounds = _pendingConnectedAnimations.TryGetValue(control, out var pendingConnectedAnimation)
            ? pendingConnectedAnimation.From
            : previousBounds;

        if (!HasVisibleSize(connectedFromBounds))
        {
            return;
        }

        if (AreClose(connectedFromBounds, nextBounds))
        {
            _pendingConnectedAnimations.Remove(control);
            if (_activeConnectedControls.Contains(control))
            {
                CompleteConnectedAnimation(control);
            }

            return;
        }

        _pendingConnectedAnimations[control] = new ConnectedAnimation(connectedFromBounds, nextBounds);
    }

    private void RequestConnectedAnimations()
    {
        _connectedStartBounds.Clear();

        if ((_pendingConnectedAnimations.Count == 0
             && _pendingInsertAnimations.Count == 0
             && _pendingLayoutAnimations.Count == 0
             && _pendingRemovalPresenters.Count == 0)
            || _hasPendingConnectedAnimationPost)
        {
            if (_pendingConnectedAnimations.Count == 0
                && _pendingInsertAnimations.Count == 0
                && _pendingLayoutAnimations.Count == 0
                && _pendingRemovalPresenters.Count == 0)
            {
                ClearScopedLayoutAction();
            }

            return;
        }

        _hasPendingConnectedAnimationPost = true;

        if (this.IsAttachedToVisualTree()
            && ElementComposition.GetElementVisual(this)?.Compositor is { } compositor)
        {
            compositor.RequestCompositionUpdate(StartPendingConnectedAnimations);
            return;
        }

        Dispatcher.UIThread.Post(StartPendingConnectedAnimations, DispatcherPriority.Render);
    }

    private bool ShouldAnimateLayoutChange(ContentPresenter presenter)
    {
        if (IsScopedLayoutActionActive())
        {
            return presenter.Content is IDockable dockable
                   && _layoutActionDockables.Contains(dockable)
                   && _connectedStartBounds.ContainsKey(presenter);
        }

        return true;
    }

    private bool IsScopedLayoutActionActive()
    {
        return _hasScopedLayoutAction && _layoutActionDockables.Count > 0;
    }

    private void CompleteScopedExcludedPresenter(ContentPresenter presenter)
    {
        if (!IsScopedLayoutActionActive())
        {
            return;
        }

        if (_pendingLayoutAnimations.ContainsKey(presenter)
            || _activeLayoutPresenters.Contains(presenter))
        {
            CompleteLayoutAnimation(presenter);
            return;
        }

        if (!_pendingInsertAnimations.Contains(presenter)
            && !_activeInsertPresenters.Contains(presenter))
        {
            presenter.Opacity = 1.0;
            presenter.IsHitTestVisible = true;
            ResetPresenterComposition(presenter);
        }
    }

    private void CompleteScopedExcludedControl(Control control)
    {
        if (!IsScopedLayoutActionActive())
        {
            return;
        }

        if (_pendingConnectedAnimations.ContainsKey(control)
            || _activeConnectedControls.Contains(control))
        {
            CompleteConnectedAnimation(control);
            return;
        }

        ResetControlComposition(control);
    }

    private void StartPendingConnectedAnimations()
    {
        _hasPendingConnectedAnimationPost = false;

        if (!UseLayoutTransitions || LayoutTransitionDuration <= TimeSpan.Zero)
        {
            CancelLayoutTransitions(removeRemovalPresenters: true);
            return;
        }

        var animations = new List<KeyValuePair<Control, ConnectedAnimation>>(_pendingConnectedAnimations);
        var insertions = new List<ContentPresenter>(_pendingInsertAnimations);
        var layoutAnimations = new List<KeyValuePair<ContentPresenter, ConnectedAnimation>>(_pendingLayoutAnimations);
        var removals = new List<ContentPresenter>(_pendingRemovalPresenters);
        _pendingConnectedAnimations.Clear();
        _pendingInsertAnimations.Clear();
        _pendingLayoutAnimations.Clear();
        _pendingRemovalPresenters.Clear();
        ClearScopedLayoutAction();

        foreach (var kvp in animations)
        {
            StartConnectedControlAnimation(kvp.Key, kvp.Value);
        }

        foreach (var presenter in insertions)
        {
            StartInsertAnimation(presenter);
        }

        foreach (var kvp in layoutAnimations)
        {
            StartLayoutAnimation(kvp.Key, kvp.Value);
        }

        foreach (var presenter in removals)
        {
            StartRemovalAnimation(presenter);
        }
    }

    private bool StartConnectedAnimation(Control control, ConnectedAnimation animation)
    {
        return StartConnectedAnimation(control, animation, useScaleForSize: false, useCurrentCompositionStart: false);
    }

    private bool StartConnectedAnimation(
        Control control,
        ConnectedAnimation animation,
        bool useScaleForSize,
        bool useCurrentCompositionStart)
    {
        if (!control.IsAttachedToVisualTree()
            || !HasVisibleSize(animation.From)
            || !HasVisibleSize(animation.To)
            || AreClose(animation.From, animation.To))
        {
            return false;
        }

        var visual = ElementComposition.GetElementVisual(control);
        if (visual is null)
        {
            return false;
        }

        var compositor = visual.Compositor;
        if (compositor is null)
        {
            return false;
        }

        var to = animation.To;
        var from = animation.From;
        var finalOffset = new Vector3D(to.X, to.Y, visual.Offset.Z);
        var startOffset = new Vector3D(
            finalOffset.X + from.X - to.X,
            finalOffset.Y + from.Y - to.Y,
            finalOffset.Z);
        var finalSize = new Vector(to.Width, to.Height);
        var startSize = new Vector(from.Width, from.Height);
        var startScale = new Vector3D(
            to.Width > 0 ? from.Width / to.Width : 1.0,
            to.Height > 0 ? from.Height / to.Height : 1.0,
            1.0);
        var duration = LayoutTransitionDuration;
        var hasSizeDelta = !AreClose(from.Width, to.Width) || !AreClose(from.Height, to.Height);

        if (!useCurrentCompositionStart)
        {
            visual.StopAnimation(nameof(CompositionVisual.Offset));
            visual.StopAnimation(nameof(CompositionVisual.Scale));
            visual.StopAnimation(nameof(CompositionVisual.Size));
        }

        visual.CenterPoint = new Vector3D(0.0, 0.0, 0.0);
        if (!useCurrentCompositionStart)
        {
            visual.Offset = finalOffset;
            visual.Size = finalSize;
            visual.Scale = new Vector3D(1.0, 1.0, 1.0);
        }

        var offset = compositor.CreateVector3DKeyFrameAnimation();
        offset.Target = nameof(CompositionVisual.Offset);
        offset.Duration = duration;
        offset.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        if (useCurrentCompositionStart)
        {
            offset.InsertExpressionKeyFrame(0.0f, "this.StartingValue", s_layoutTransitionEasing);
        }
        else
        {
            offset.InsertKeyFrame(0.0f, startOffset, s_layoutTransitionEasing);
        }

        offset.InsertKeyFrame(1.0f, finalOffset, s_layoutTransitionEasing);

        var group = compositor.CreateAnimationGroup();
        group.Add(offset);

        if (useScaleForSize)
        {
            if (hasSizeDelta || useCurrentCompositionStart)
            {
                var scale = compositor.CreateVector3DKeyFrameAnimation();
                scale.Target = nameof(CompositionVisual.Scale);
                scale.Duration = duration;
                scale.StopBehavior = AnimationStopBehavior.SetToFinalValue;
                if (useCurrentCompositionStart)
                {
                    scale.InsertExpressionKeyFrame(0.0f, "this.StartingValue", s_layoutTransitionEasing);
                }
                else
                {
                    scale.InsertKeyFrame(0.0f, startScale, s_layoutTransitionEasing);
                }

                scale.InsertKeyFrame(1.0f, new Vector3D(1.0, 1.0, 1.0), s_layoutTransitionEasing);

                group.Add(scale);
            }

            visual.StartAnimationGroup(group);
            return true;
        }

        if (hasSizeDelta || useCurrentCompositionStart)
        {
            var size = compositor.CreateVectorKeyFrameAnimation();
            size.Target = nameof(CompositionVisual.Size);
            size.Duration = duration;
            size.StopBehavior = AnimationStopBehavior.SetToFinalValue;
            if (useCurrentCompositionStart)
            {
                size.InsertExpressionKeyFrame(0.0f, "this.StartingValue", s_layoutTransitionEasing);
            }
            else
            {
                size.InsertKeyFrame(0.0f, startSize, s_layoutTransitionEasing);
            }

            size.InsertKeyFrame(1.0f, finalSize, s_layoutTransitionEasing);

            group.Add(size);
        }

        visual.StartAnimationGroup(group);
        return true;
    }

    private void StartConnectedControlAnimation(Control control, ConnectedAnimation animation)
    {
        var useCurrentCompositionStart = _activeConnectedControls.Contains(control);
        var version = NextConnectedAnimationVersion(control);
        _activeConnectedControls.Add(control);
        var compositor = ElementComposition.GetElementVisual(control)?.Compositor;

        if (compositor is null
            || !StartConnectedAnimation(control, animation, useScaleForSize: false, useCurrentCompositionStart))
        {
            CompleteConnectedAnimation(control, version);
            return;
        }

        CompleteConnectedAnimationAfterCommit(compositor, control, version, GetTransitionCompletionDelay());
    }

    private async void CompleteConnectedAnimationAfterCommit(
        Compositor compositor,
        Control control,
        int version,
        TimeSpan delay)
    {
        await compositor.RequestCommitAsync();
        await Task.Delay(delay);
        Dispatcher.UIThread.Post(() => CompleteConnectedAnimation(control, version), DispatcherPriority.Background);
    }

    private int NextConnectedAnimationVersion(Control control)
    {
        var version = _connectedAnimationVersions.TryGetValue(control, out var currentVersion)
            ? currentVersion + 1
            : 1;

        _connectedAnimationVersions[control] = version;
        return version;
    }

    private void CompleteConnectedAnimation(Control control, int? version = null)
    {
        if (version.HasValue
            && _connectedAnimationVersions.TryGetValue(control, out var currentVersion)
            && currentVersion != version.Value)
        {
            return;
        }

        ClearConnectedControlState(control);

        if (Children.Contains(control))
        {
            ResetControlComposition(control);
        }
    }

    private void StartInsertAnimation(ContentPresenter presenter)
    {
        var version = NextInsertAnimationVersion(presenter);
        _activeInsertPresenters.Add(presenter);

        if (!presenter.IsAttachedToVisualTree())
        {
            CompleteInsertAnimation(presenter, version);
            return;
        }

        var visual = ElementComposition.GetElementVisual(presenter);
        if (visual is null)
        {
            CompleteInsertAnimation(presenter, version);
            return;
        }

        var compositor = visual.Compositor;
        if (compositor is null)
        {
            CompleteInsertAnimation(presenter, version);
            return;
        }

        presenter.Opacity = 1.0;
        presenter.IsHitTestVisible = false;
        visual.Opacity = 1.0f;
        visual.StopAnimation(nameof(CompositionVisual.Opacity));

        var opacity = compositor.CreateScalarKeyFrameAnimation();
        opacity.Target = nameof(CompositionVisual.Opacity);
        opacity.Duration = LayoutTransitionDuration;
        opacity.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        opacity.InsertKeyFrame(0.0f, 0.0f, s_layoutTransitionEasing);
        opacity.InsertKeyFrame(1.0f, 1.0f, s_layoutTransitionEasing);

        visual.StartAnimation(nameof(CompositionVisual.Opacity), opacity);

        CompleteInsertAnimationAfterCommit(compositor, presenter, version, GetTransitionCompletionDelay());
    }

    private void StartLayoutAnimation(ContentPresenter presenter, ConnectedAnimation animation)
    {
        var useCurrentCompositionStart = _activeLayoutPresenters.Contains(presenter);
        var version = NextLayoutAnimationVersion(presenter);
        _activeLayoutPresenters.Add(presenter);

        if (!presenter.IsAttachedToVisualTree())
        {
            CompleteLayoutAnimation(presenter, version);
            return;
        }

        presenter.Opacity = 1.0;
        presenter.IsHitTestVisible = false;
        var compositor = ElementComposition.GetElementVisual(presenter)?.Compositor;

        if (compositor is null
            || !StartConnectedAnimation(presenter, animation, useScaleForSize: false, useCurrentCompositionStart))
        {
            CompleteLayoutAnimation(presenter, version);
            return;
        }

        CompleteLayoutAnimationAfterCommit(compositor, presenter, version, GetTransitionCompletionDelay());
    }

    private async void CompleteLayoutAnimationAfterCommit(
        Compositor compositor,
        ContentPresenter presenter,
        int version,
        TimeSpan delay)
    {
        await compositor.RequestCommitAsync();
        await Task.Delay(delay);
        Dispatcher.UIThread.Post(() => CompleteLayoutAnimation(presenter, version), DispatcherPriority.Background);
    }

    private int NextLayoutAnimationVersion(ContentPresenter presenter)
    {
        var version = _layoutAnimationVersions.TryGetValue(presenter, out var currentVersion)
            ? currentVersion + 1
            : 1;

        _layoutAnimationVersions[presenter] = version;
        return version;
    }

    private void CompleteLayoutAnimation(ContentPresenter presenter, int? version = null)
    {
        if (version.HasValue
            && _layoutAnimationVersions.TryGetValue(presenter, out var currentVersion)
            && currentVersion != version.Value)
        {
            return;
        }

        _pendingLayoutAnimations.Remove(presenter);
        _layoutAnimationVersions.Remove(presenter);
        _activeLayoutPresenters.Remove(presenter);

        if (Children.Contains(presenter))
        {
            var hasInsertTransition = _pendingInsertAnimations.Contains(presenter)
                                      || _activeInsertPresenters.Contains(presenter);
            presenter.Opacity = 1.0;
            presenter.IsHitTestVisible = !hasInsertTransition;
            if (hasInsertTransition)
            {
                ResetControlComposition(presenter);
            }
            else
            {
                ResetPresenterComposition(presenter);
            }
        }
    }

    private async void CompleteInsertAnimationAfterCommit(
        Compositor compositor,
        ContentPresenter presenter,
        int version,
        TimeSpan delay)
    {
        await compositor.RequestCommitAsync();
        await Task.Delay(delay);
        Dispatcher.UIThread.Post(() => CompleteInsertAnimation(presenter, version), DispatcherPriority.Background);
    }

    private int NextInsertAnimationVersion(ContentPresenter presenter)
    {
        var version = _insertAnimationVersions.TryGetValue(presenter, out var currentVersion)
            ? currentVersion + 1
            : 1;

        _insertAnimationVersions[presenter] = version;
        return version;
    }

    private void CompleteInsertAnimation(ContentPresenter presenter, int? version = null)
    {
        if (version.HasValue
            && _insertAnimationVersions.TryGetValue(presenter, out var currentVersion)
            && currentVersion != version.Value)
        {
            return;
        }

        _pendingInsertAnimations.Remove(presenter);
        _insertAnimationVersions.Remove(presenter);
        _activeInsertPresenters.Remove(presenter);

        if (Children.Contains(presenter))
        {
            var hasLayoutTransition = _pendingLayoutAnimations.ContainsKey(presenter)
                                      || _activeLayoutPresenters.Contains(presenter);
            presenter.Opacity = 1.0;
            presenter.IsHitTestVisible = !hasLayoutTransition;
            if (hasLayoutTransition)
            {
                ResetPresenterOpacity(presenter);
            }
            else
            {
                ResetPresenterComposition(presenter);
            }
        }
    }

    private void StartRemovalAnimation(ContentPresenter presenter)
    {
        if (!_removalPresenterBounds.ContainsKey(presenter) || !_activeRemovalPresenters.Add(presenter))
        {
            return;
        }

        presenter.Opacity = 1.0;
        presenter.IsHitTestVisible = false;

        var visual = ElementComposition.GetElementVisual(presenter);
        Compositor? compositor = null;
        if (visual is { Compositor: { } visualCompositor } && presenter.IsAttachedToVisualTree())
        {
            compositor = visualCompositor;
            visual.StopAnimation(nameof(CompositionVisual.Opacity));

            var opacity = visualCompositor.CreateScalarKeyFrameAnimation();
            opacity.Target = nameof(CompositionVisual.Opacity);
            opacity.Duration = LayoutTransitionDuration;
            opacity.StopBehavior = AnimationStopBehavior.SetToFinalValue;
            opacity.InsertExpressionKeyFrame(0.0f, "this.StartingValue", s_layoutTransitionEasing);
            opacity.InsertKeyFrame(1.0f, 0.0f, s_layoutTransitionEasing);

            visual.StartAnimation(nameof(CompositionVisual.Opacity), opacity);
        }

        if (compositor is null)
        {
            RemoveRemovalPresenterAfterDelay(presenter, GetTransitionCompletionDelay());
            return;
        }

        RemoveRemovalPresenterAfterCommit(compositor, presenter, GetTransitionCompletionDelay());
    }

    private async void RemoveRemovalPresenterAfterCommit(
        Compositor compositor,
        ContentPresenter presenter,
        TimeSpan delay)
    {
        await compositor.RequestCommitAsync();
        await Task.Delay(delay);
        Dispatcher.UIThread.Post(() => RemoveRemovalPresenter(presenter), DispatcherPriority.Background);
    }

    private async void RemoveRemovalPresenterAfterDelay(ContentPresenter presenter, TimeSpan delay)
    {
        await Task.Delay(delay);
        Dispatcher.UIThread.Post(() => RemoveRemovalPresenter(presenter), DispatcherPriority.Background);
    }

    private void RemoveRemovalPresenter(ContentPresenter presenter)
    {
        if (!_removalPresenterBounds.ContainsKey(presenter)
            && !_pendingRemovalPresenters.Contains(presenter)
            && !_activeRemovalPresenters.Contains(presenter))
        {
            return;
        }

        _pendingRemovalPresenters.Remove(presenter);
        _activeRemovalPresenters.Remove(presenter);
        _removalPresenterBounds.Remove(presenter);

        if (presenter.Content is IDockable dockable
            && _removalPresentersByDockable.TryGetValue(dockable, out var removalPresenter)
            && ReferenceEquals(removalPresenter, presenter))
        {
            _removalPresentersByDockable.Remove(dockable);
        }

        ResetPresenterComposition(presenter);
        Children.Remove(presenter);
        presenter.Content = null;
        presenter.DataContext = null;
        presenter.Opacity = 1.0;
        presenter.IsHitTestVisible = true;
    }

    private void CancelLayoutTransitions(bool removeRemovalPresenters)
    {
        _hasPendingConnectedAnimationPost = false;
        _connectedStartBounds.Clear();
        ClearScopedLayoutAction();

        var controls = new HashSet<Control>(ReferenceEqualityComparer.Instance);
        foreach (var control in _pendingConnectedAnimations.Keys)
        {
            controls.Add(control);
        }

        foreach (var control in _activeConnectedControls)
        {
            controls.Add(control);
        }

        foreach (var splitter in _splitters.Values)
        {
            controls.Add(splitter);
        }

        _pendingConnectedAnimations.Clear();
        _pendingLayoutAnimations.Clear();
        _pendingInsertAnimations.Clear();

        var presenters = new HashSet<ContentPresenter>(ReferenceEqualityComparer.Instance);

        foreach (var presenter in _presenters.Values)
        {
            presenters.Add(presenter);
        }

        foreach (var presenter in _activeInsertPresenters)
        {
            presenters.Add(presenter);
        }

        foreach (var presenter in _activeLayoutPresenters)
        {
            presenters.Add(presenter);
        }

        foreach (var presenter in _removalPresenterBounds.Keys)
        {
            presenters.Add(presenter);
        }

        foreach (var presenter in _pendingRemovalPresenters)
        {
            presenters.Add(presenter);
        }

        foreach (var presenter in _activeRemovalPresenters)
        {
            presenters.Add(presenter);
        }

        foreach (var presenter in presenters)
        {
            CompletePresenterTransitionState(presenter);
        }

        foreach (var control in controls)
        {
            CompleteConnectedControlState(control);
        }

        if (removeRemovalPresenters)
        {
            RemoveRemovalPresenters();
        }
    }

    private void CompletePresenterTransitionState(ContentPresenter presenter)
    {
        _enteringPresenters.Remove(presenter);
        _pendingInsertAnimations.Remove(presenter);
        _activeInsertPresenters.Remove(presenter);
        _insertAnimationVersions.Remove(presenter);
        _pendingLayoutAnimations.Remove(presenter);
        _layoutAnimationVersions.Remove(presenter);
        _activeLayoutPresenters.Remove(presenter);

        ResetPresenterComposition(presenter);

        if (Children.Contains(presenter))
        {
            presenter.Opacity = 1.0;
            presenter.IsHitTestVisible = true;
        }
    }

    private void CompleteConnectedControlState(Control control)
    {
        ClearConnectedControlState(control);
        ResetControlComposition(control);
    }

    private void TrackLayoutActionDockables(NotifyCollectionChangedEventArgs e)
    {
        if (!UseLayoutTransitions || !_hasCompletedArrange)
        {
            return;
        }

        var previousCount = _layoutActionDockables.Count;
        TrackLayoutActionDockables(e.NewItems);
        TrackLayoutActionDockables(e.OldItems);

        if (_layoutActionDockables.Count > previousCount)
        {
            _hasScopedLayoutAction = true;
        }
    }

    private void TrackStructuralLayoutActionDockables()
    {
        if (!UseLayoutTransitions
            || !_hasCompletedArrange
            || IsScopedLayoutActionActive()
            || Dock is not { } dock)
        {
            return;
        }

        EnsureConnectedStartBounds();

        var previousDockables = new List<IDockable>();
        CaptureCurrentPresenterDockables(previousDockables);
        if (previousDockables.Count == 0)
        {
            return;
        }

        var nextDockables = new List<IDockable>();
        CaptureLeafDockables(dock, nextDockables);

        var previousCount = _layoutActionDockables.Count;
        TrackChangedDockables(previousDockables, nextDockables);

        if (_layoutActionDockables.Count > previousCount)
        {
            _hasScopedLayoutAction = true;
        }
    }

    private void CaptureCurrentPresenterDockables(ICollection<IDockable> dockables)
    {
        foreach (var child in Children)
        {
            if (child is not ContentPresenter presenter
                || presenter.Content is not IDockable dockable
                || !_presenters.TryGetValue(dockable, out var activePresenter)
                || !ReferenceEquals(activePresenter, presenter))
            {
                continue;
            }

            dockables.Add(dockable);
        }
    }

    private void CaptureLeafDockables(IDockable dockable, ICollection<IDockable> dockables)
    {
        switch (dockable)
        {
            case IProportionalDock proportionalDock:
            {
                if (proportionalDock.VisibleDockables is null)
                {
                    return;
                }

                foreach (var child in proportionalDock.VisibleDockables)
                {
                    CaptureLeafDockables(child, dockables);
                }

                return;
            }
            case IProportionalDockSplitter:
                return;
            default:
                dockables.Add(dockable);
                return;
        }
    }

    private void TrackChangedDockables(IList<IDockable> previousDockables, IList<IDockable> nextDockables)
    {
        if (TrackSingleMovedDockable(previousDockables, nextDockables))
        {
            return;
        }

        for (var index = 0; index < previousDockables.Count; index++)
        {
            var dockable = previousDockables[index];
            var nextIndex = IndexOfReference(nextDockables, dockable);
            if (nextIndex < 0 || nextIndex != index)
            {
                _layoutActionDockables.Add(dockable);
            }
        }

        for (var index = 0; index < nextDockables.Count; index++)
        {
            var dockable = nextDockables[index];
            var previousIndex = IndexOfReference(previousDockables, dockable);
            if (previousIndex < 0 || previousIndex != index)
            {
                _layoutActionDockables.Add(dockable);
            }
        }
    }

    private bool TrackSingleMovedDockable(IList<IDockable> previousDockables, IList<IDockable> nextDockables)
    {
        if (previousDockables.Count != nextDockables.Count || previousDockables.Count < 3)
        {
            return false;
        }

        IDockable? movedDockable = null;
        for (var previousIndex = 0; previousIndex < previousDockables.Count; previousIndex++)
        {
            var dockable = previousDockables[previousIndex];
            var nextIndex = IndexOfReference(nextDockables, dockable);
            if (nextIndex < 0)
            {
                return false;
            }

            if (nextIndex == previousIndex)
            {
                continue;
            }

            if (!MatchesAfterMove(previousDockables, nextDockables, previousIndex, nextIndex))
            {
                continue;
            }

            if (movedDockable is not null)
            {
                return false;
            }

            movedDockable = dockable;
        }

        if (movedDockable is null)
        {
            return false;
        }

        _layoutActionDockables.Add(movedDockable);
        return true;
    }

    private static bool MatchesAfterMove(
        IList<IDockable> previousDockables,
        IList<IDockable> nextDockables,
        int previousIndex,
        int nextIndex)
    {
        for (var index = 0; index < nextDockables.Count; index++)
        {
            var candidateIndex = GetMovedSequenceSourceIndex(index, previousIndex, nextIndex);
            if (!ReferenceEquals(previousDockables[candidateIndex], nextDockables[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static int GetMovedSequenceSourceIndex(int nextIndex, int previousIndex, int movedNextIndex)
    {
        if (nextIndex == movedNextIndex)
        {
            return previousIndex;
        }

        if (previousIndex < movedNextIndex && nextIndex >= previousIndex && nextIndex < movedNextIndex)
        {
            return nextIndex + 1;
        }

        if (previousIndex > movedNextIndex && nextIndex > movedNextIndex && nextIndex <= previousIndex)
        {
            return nextIndex - 1;
        }

        return nextIndex;
    }

    private static int IndexOfReference(IList<IDockable> dockables, IDockable dockable)
    {
        for (var index = 0; index < dockables.Count; index++)
        {
            if (ReferenceEquals(dockables[index], dockable))
            {
                return index;
            }
        }

        return -1;
    }

    private void TrackLayoutActionDockables(System.Collections.IList? dockables)
    {
        if (dockables is null)
        {
            return;
        }

        foreach (var item in dockables)
        {
            if (item is IDockable dockable)
            {
                TrackLayoutActionDockable(dockable);
            }
        }
    }

    private void TrackLayoutActionDockable(IDockable dockable)
    {
        switch (dockable)
        {
            case IProportionalDock proportionalDock:
            {
                if (proportionalDock.VisibleDockables is null)
                {
                    return;
                }

                foreach (var child in proportionalDock.VisibleDockables)
                {
                    TrackLayoutActionDockable(child);
                }

                return;
            }
            case IProportionalDockSplitter:
                return;
            default:
                _layoutActionDockables.Add(dockable);
                return;
        }
    }

    private void ClearScopedLayoutAction()
    {
        _layoutActionDockables.Clear();
        _hasScopedLayoutAction = false;
    }

    private void EnsureConnectedStartBounds()
    {
        if (_connectedStartBounds.Count == 0)
        {
            CaptureConnectedStartBounds();
        }
    }

    private void ClearConnectedControlState(Control control)
    {
        _pendingConnectedAnimations.Remove(control);
        _activeConnectedControls.Remove(control);
        _connectedAnimationVersions.Remove(control);
    }

    private static void ResetPresenterComposition(ContentPresenter presenter)
    {
        ResetControlComposition(presenter, resetOpacity: true);
    }

    private static void ResetPresenterOpacity(ContentPresenter presenter)
    {
        var visual = ElementComposition.GetElementVisual(presenter);
        if (visual is null)
        {
            return;
        }

        visual.StopAnimation(nameof(CompositionVisual.Opacity));
        visual.Opacity = 1.0f;
    }

    private static void ResetControlComposition(Control control)
    {
        ResetControlComposition(control, resetOpacity: false);
    }

    private static void ResetControlComposition(Control control, bool resetOpacity)
    {
        var visual = ElementComposition.GetElementVisual(control);
        if (visual is null)
        {
            return;
        }

        if (resetOpacity)
        {
            visual.StopAnimation(nameof(CompositionVisual.Opacity));
            visual.Opacity = 1.0f;
        }

        visual.StopAnimation(nameof(CompositionVisual.Offset));
        visual.StopAnimation(nameof(CompositionVisual.Scale));
        visual.StopAnimation(nameof(CompositionVisual.Size));
        visual.CenterPoint = new Vector3D(0.0, 0.0, 0.0);
        visual.Scale = new Vector3D(1.0, 1.0, 1.0);

        var bounds = control.Bounds;
        if (HasVisibleSize(bounds))
        {
            visual.Offset = new Vector3D(bounds.X, bounds.Y, visual.Offset.Z);
            visual.Size = new Vector(bounds.Width, bounds.Height);
        }
    }

    private void CaptureConnectedStartBounds()
    {
        _connectedStartBounds.Clear();

        if (!UseLayoutTransitions || !_hasCompletedArrange)
        {
            return;
        }

        foreach (var presenter in _presenters.Values)
        {
            CaptureConnectedStartBounds(presenter);
        }

        foreach (var splitter in _splitters.Values)
        {
            CaptureConnectedStartBounds(splitter);
        }

        foreach (var kvp in _removalPresenterBounds)
        {
            if (HasVisibleSize(kvp.Value))
            {
                _connectedStartBounds[kvp.Key] = kvp.Value;
            }
        }
    }

    private void CaptureConnectedStartBounds(Control control)
    {
        var bounds = control.Bounds;
        if (HasVisibleSize(bounds))
        {
            _connectedStartBounds[control] = bounds;
        }
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

    private static bool AreClose(Rect left, Rect right)
    {
        return AreClose(left.X, right.X)
               && AreClose(left.Y, right.Y)
               && AreClose(left.Width, right.Width)
               && AreClose(left.Height, right.Height);
    }

    private static bool HasVisibleSize(Rect bounds)
    {
        return bounds.Width > 0
               && bounds.Height > 0
               && !double.IsNaN(bounds.Width)
               && !double.IsNaN(bounds.Height)
               && !double.IsInfinity(bounds.Width)
               && !double.IsInfinity(bounds.Height);
    }

    private TimeSpan GetTransitionCompletionDelay()
    {
        return LayoutTransitionDuration + s_transitionCompletionSlack;
    }

    private readonly struct ConnectedAnimation
    {
        public ConnectedAnimation(Rect from, Rect to)
        {
            From = from;
            To = to;
        }

        public Rect From { get; }

        public Rect To { get; }
    }

    private static AvaloniaOrientation ToAvaloniaOrientation(DockOrientation orientation)
    {
        return orientation == DockOrientation.Vertical ? AvaloniaOrientation.Vertical : AvaloniaOrientation.Horizontal;
    }
}
