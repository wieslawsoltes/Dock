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

namespace Dock.Controls.Flat;

/// <summary>
/// Presents a proportional dock tree as a flat set of direct child visuals.
/// </summary>
public class FlatProportionalPanel : Panel
{
    private static readonly CubicEaseOut s_layoutTransitionEasing = new();
    private static readonly TimeSpan s_transitionCompletionSlack = TimeSpan.FromMilliseconds(16);

    private readonly Dictionary<object, Control> _dockSurfaces = new();
    private readonly Dictionary<object, ContentPresenter> _presenters = new();
    private readonly Dictionary<ContentPresenter, IFlatProportionalItem> _presenterItems = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, FlatProportionalSplitter> _splitters = new();
    private readonly Dictionary<object, Rect> _dockBounds = new();
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
    private readonly Dictionary<object, ContentPresenter> _removalPresentersByDockable = new();
    private readonly HashSet<ContentPresenter> _pendingRemovalPresenters = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ContentPresenter> _activeRemovalPresenters = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<object> _layoutActionDockables = new();
    private readonly HashSet<Control> _arrangedChildren = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<INotifyPropertyChanged> _propertySubscriptions = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<INotifyCollectionChanged> _collectionSubscriptions = new(ReferenceEqualityComparer.Instance);
    private Dictionary<object, Control>? _reusableDockSurfaces;
    private Dictionary<object, ContentPresenter>? _reusablePresenters;
    private Dictionary<object, FlatProportionalSplitter>? _reusableSplitters;
    private bool _isRebuilding;
    private bool _isAssigningProportions;
    private bool _hasCompletedArrange;
    private bool _hasPendingConnectedAnimationPost;
    private bool _hasPendingRebuild;
    private bool _suppressLayoutTransitions;
    private bool _hasScopedLayoutAction;

    /// <summary>
    /// Defines the <see cref="Root"/> property.
    /// </summary>
    public static readonly StyledProperty<IFlatProportionalDock?> RootProperty =
        AvaloniaProperty.Register<FlatProportionalPanel, IFlatProportionalDock?>(nameof(Root));

    /// <summary>
    /// Defines the <see cref="SplitterThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SplitterThicknessProperty =
        AvaloniaProperty.Register<FlatProportionalPanel, double>(nameof(SplitterThickness), 4.0);

    /// <summary>
    /// Defines the <see cref="MinimumProportionSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProportionSizeProperty =
        AvaloniaProperty.Register<FlatProportionalPanel, double>(nameof(MinimumProportionSize), 75.0);

    /// <summary>
    /// Defines the <see cref="UseLayoutTransitions"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> UseLayoutTransitionsProperty =
        AvaloniaProperty.Register<FlatProportionalPanel, bool>(nameof(UseLayoutTransitions), true);

    /// <summary>
    /// Defines the <see cref="LayoutTransitionDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> LayoutTransitionDurationProperty =
        AvaloniaProperty.Register<FlatProportionalPanel, TimeSpan>(
            nameof(LayoutTransitionDuration),
            TimeSpan.FromMilliseconds(240));

    /// <summary>
    /// Gets or sets the root proportional dock to present.
    /// </summary>
    public IFlatProportionalDock? Root
    {
        get => GetValue(RootProperty);
        set => SetValue(RootProperty, value);
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

        if (change.Property == RootProperty)
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
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (Root is not null)
        {
            RequestRebuildVisualTree();
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        ExecutePendingRebuild(invalidateLayout: false);

        if (Root is not { } dock)
        {
            return default;
        }

        MeasureDock(dock, availableSize);
        MeasureRemovalPresenters();
        return NormalizeDesiredSize(availableSize, dock);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        ExecutePendingRebuild(invalidateLayout: false);
        TrackStructuralLayoutActionDockables();

        _arrangedChildren.Clear();
        _dockBounds.Clear();

        if (Root is { } dock)
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

    /// <summary>
    /// Resizes the proportional items adjacent to a splitter control.
    /// </summary>
    /// <param name="splitterControl">The splitter control that initiated the resize.</param>
    /// <param name="dragDelta">The drag delta in the splitter orientation axis.</param>
    public void ResizeSplitter(FlatProportionalSplitter splitterControl, double dragDelta)
    {
        if (splitterControl.OwnerDock is not { } ownerDock
            || splitterControl.Splitter is not { } splitter
            || ownerDock.VisibleItems is not { } visibleDockables)
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

        if (!_dockBounds.TryGetValue(GetItemKey(ownerDock), out var ownerBounds))
        {
            ownerBounds = new Rect(Bounds.Size);
        }

        var availableSize = ownerDock.Orientation == Avalonia.Layout.Orientation.Vertical
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
            _reusableDockSurfaces = new Dictionary<object, Control>(_dockSurfaces);
            _reusablePresenters = new Dictionary<object, ContentPresenter>(_presenters);
            _reusableSplitters = new Dictionary<object, FlatProportionalSplitter>(_splitters);
            _dockSurfaces.Clear();
            _presenters.Clear();
            _splitters.Clear();
            _dockBounds.Clear();

            if (Root is { } dock)
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

    private void AddDockSurfaces(IFlatProportionalDock dock)
    {
        var key = GetItemKey(dock);
        var surface = CreateDockSurface(dock);
        _dockSurfaces[key] = surface;
        EnsureSurfaceChild(surface);

        if (dock.VisibleItems is null)
        {
            return;
        }

        foreach (var dockable in dock.VisibleItems)
        {
            if (dockable is IFlatProportionalDock childDock)
            {
                AddDockSurfaces(childDock);
            }
        }
    }

    /// <summary>
    /// Creates the transparent surface arranged over a proportional container.
    /// </summary>
    /// <param name="dock">The proportional container item.</param>
    /// <returns>The surface control.</returns>
    protected virtual Control CreateDockSurface(IFlatProportionalDock dock)
    {
        if (_reusableDockSurfaces?.Remove(GetItemKey(dock), out var reusableSurface) == true)
        {
            reusableSurface.DataContext = dock.Content ?? dock;
            return reusableSurface;
        }

        return new Border
        {
            Background = Brushes.Transparent,
            DataContext = dock.Content ?? dock
        };
    }

    private void AddDockVisuals(IFlatProportionalDock dock)
    {
        if (dock.VisibleItems is null)
        {
            return;
        }

        foreach (var dockable in dock.VisibleItems)
        {
            switch (dockable)
            {
                case IFlatProportionalSplitter splitter:
                    AddSplitter(dock, splitter);
                    break;
                case IFlatProportionalDock childDock:
                    AddDockVisuals(childDock);
                    break;
                default:
                    AddPresenter(dockable);
                    break;
            }
        }
    }

    private void AddSplitter(IFlatProportionalDock ownerDock, IFlatProportionalSplitter splitter)
    {
        var key = GetItemKey(splitter);
        FlatProportionalSplitter? reusableSplitter = null;
        var reused = _reusableSplitters is not null && _reusableSplitters.Remove(key, out reusableSplitter);
        var control = reused
            ? reusableSplitter!
            : CreateSplitter(ownerDock, splitter);

        control.OwnerDock = ownerDock;
        control.Splitter = splitter;
        control.Orientation = ownerDock.Orientation;
        control.Thickness = SplitterThickness;

        if (!ReferenceEquals(control.DataContext, splitter))
        {
            control.DataContext = splitter;
        }

        if (!reused)
        {
            control.Bind(FlatProportionalSplitter.IsResizingEnabledProperty, new Binding(nameof(IFlatProportionalSplitter.CanResize)));
            control.Bind(FlatProportionalSplitter.PreviewResizeProperty, new Binding(nameof(IFlatProportionalSplitter.ResizePreview)));
        }

        _splitters[key] = control;
        EnsureDockVisualChild(control);
    }

    /// <summary>
    /// Creates the control used for a proportional splitter item.
    /// </summary>
    /// <param name="ownerDock">The owner dock item.</param>
    /// <param name="splitter">The splitter item.</param>
    /// <returns>The splitter control.</returns>
    protected virtual FlatProportionalSplitter CreateSplitter(IFlatProportionalDock ownerDock, IFlatProportionalSplitter splitter)
    {
        return new FlatProportionalSplitter
        {
            DataContext = splitter
        };
    }

    private void AddPresenter(IFlatProportionalItem dockable)
    {
        var key = GetItemKey(dockable);
        ContentPresenter? reusablePresenter = null;
        var reused = _reusablePresenters is not null && _reusablePresenters.Remove(key, out reusablePresenter);
        if (!reused && _removalPresentersByDockable.Remove(key, out reusablePresenter))
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

        _presenterItems[presenter] = dockable;
        presenter.Content = dockable.Content;
        presenter.DataContext = dockable.Content ?? dockable;

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

        _presenters[key] = presenter;
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

        var removedPresenters = new List<KeyValuePair<object, ContentPresenter>>(_reusablePresenters);
        foreach (var kvp in removedPresenters)
        {
            var dockableKey = kvp.Key;
            var presenter = kvp.Value;
            if (!_connectedStartBounds.TryGetValue(presenter, out var bounds) || !HasVisibleSize(bounds))
            {
                continue;
            }

            _reusablePresenters.Remove(dockableKey);
            _pendingInsertAnimations.Remove(presenter);
            _activeInsertPresenters.Remove(presenter);
            _insertAnimationVersions.Remove(presenter);
            _pendingLayoutAnimations.Remove(presenter);
            _layoutAnimationVersions.Remove(presenter);
            _activeLayoutPresenters.Remove(presenter);

            _removalPresenterBounds[presenter] = bounds;
            _removalPresentersByDockable[dockableKey] = presenter;
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
            _presenterItems.Remove(presenter);
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

    private void EnsureSurfaceChild(Control surface)
    {
        if (Children.Contains(surface))
        {
            return;
        }

        var index = 0;
        while (index < Children.Count && IsDockSurface(Children[index]))
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

    private bool IsDockSurface(Control control)
    {
        return _dockSurfaces.ContainsValue(control);
    }

    private void SubscribeLayout(IFlatProportionalDock dock)
    {
        SubscribeDockable(dock);

        if (dock.VisibleItems is INotifyCollectionChanged collectionChanged
            && _collectionSubscriptions.Add(collectionChanged))
        {
            collectionChanged.CollectionChanged += VisibleItemsCollectionChanged;
        }

        if (dock.VisibleItems is null)
        {
            return;
        }

        foreach (var dockable in dock.VisibleItems)
        {
            SubscribeDockable(dockable);

            if (dockable is IFlatProportionalDock childDock)
            {
                SubscribeLayout(childDock);
            }
        }
    }

    private void SubscribeDockable(IFlatProportionalItem dockable)
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
            collectionChanged.CollectionChanged -= VisibleItemsCollectionChanged;
        }

        _propertySubscriptions.Clear();
        _collectionSubscriptions.Clear();
    }

    private void VisibleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
            || e.PropertyName == nameof(IFlatProportionalDock.VisibleItems)
            || e.PropertyName == nameof(IFlatProportionalDock.Orientation))
        {
            RequestRebuildVisualTree();
            return;
        }

        if (e.PropertyName == nameof(IFlatProportionalItem.Content)
            && sender is IFlatProportionalItem item)
        {
            UpdateItemContent(item);
            return;
        }

        CaptureConnectedStartBounds();
        InvalidateMeasure();
        InvalidateArrange();
    }

    private void UpdateItemContent(IFlatProportionalItem item)
    {
        if (_presenters.TryGetValue(GetItemKey(item), out var presenter))
        {
            presenter.Content = item.Content;
            presenter.DataContext = item.Content ?? item;
        }

        if (item is IFlatProportionalDock
            && _dockSurfaces.TryGetValue(GetItemKey(item), out var surface))
        {
            surface.DataContext = item.Content ?? item;
        }
    }

    private void UpdateSplitterThickness()
    {
        foreach (var splitter in _splitters.Values)
        {
            splitter.Thickness = SplitterThickness;
        }
    }

    private void MeasureDock(IFlatProportionalDock dock, Size availableSize)
    {
        if (_dockSurfaces.TryGetValue(GetItemKey(dock), out var surface))
        {
            surface.Measure(availableSize);
        }

        if (dock.VisibleItems is not { } visibleDockables || visibleDockables.Count == 0)
        {
            return;
        }

        var splitterThickness = GetTotalSplitterThickness(visibleDockables);
        var proportions = AssignProportions(dock, availableSize, splitterThickness);
        var availableLength = Math.Max(0, GetLength(availableSize, dock.Orientation) - splitterThickness);
        var sumOfFractions = 0.0;

        for (var i = 0; i < visibleDockables.Count; i++)
        {
            var dockable = visibleDockables[i];
            if (dockable is IFlatProportionalSplitter splitter)
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
                GetLayoutProportion(proportions, dockable),
                ref sumOfFractions);

            var childSize = CreateChildSize(availableSize, dock.Orientation, length);
            if (dockable is IFlatProportionalDock childDock)
            {
                MeasureDock(childDock, childSize);
            }
            else if (_presenters.TryGetValue(GetItemKey(dockable), out var presenter))
            {
                presenter.Measure(childSize);
            }
        }
    }

    private void MeasureSplitter(
        IFlatProportionalDock dock,
        IList<IFlatProportionalItem> visibleDockables,
        int index,
        IFlatProportionalSplitter splitter,
        Size availableSize)
    {
        if (!_splitters.TryGetValue(GetItemKey(splitter), out var splitterControl))
        {
            return;
        }

        splitterControl.Orientation = dock.Orientation;

        if (!ShouldUseSplitter(visibleDockables, index))
        {
            splitterControl.Measure(default);
            return;
        }

        var size = dock.Orientation == Avalonia.Layout.Orientation.Vertical
            ? new Size(availableSize.Width, splitterControl.Thickness)
            : new Size(splitterControl.Thickness, availableSize.Height);

        splitterControl.Measure(size);
    }

    private void MeasureCollapsed(IFlatProportionalItem dockable)
    {
        switch (dockable)
        {
            case IFlatProportionalDock dock:
                MeasureDock(dock, default);
                break;
            default:
                if (_presenters.TryGetValue(GetItemKey(dockable), out var presenter))
                {
                    presenter.Measure(default);
                }
                break;
        }
    }

    private void ArrangeDock(IFlatProportionalDock dock, Rect bounds)
    {
        _dockBounds[GetItemKey(dock)] = bounds;

        if (_dockSurfaces.TryGetValue(GetItemKey(dock), out var surface))
        {
            ArrangeChild(surface, bounds, useConnectedAnimation: false);
        }

        if (dock.VisibleItems is not { } visibleDockables || visibleDockables.Count == 0)
        {
            return;
        }

        var splitterThickness = GetTotalSplitterThickness(visibleDockables);
        var proportions = AssignProportions(dock, bounds.Size, splitterThickness);
        var availableLength = Math.Max(0, GetLength(bounds.Size, dock.Orientation) - splitterThickness);
        var offset = 0.0;
        var sumOfFractions = 0.0;

        for (var i = 0; i < visibleDockables.Count; i++)
        {
            var dockable = visibleDockables[i];

            if (dockable is IFlatProportionalSplitter splitter)
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
                GetLayoutProportion(proportions, dockable),
                ref sumOfFractions);

            var childBounds = CreateChildRect(bounds, dock.Orientation, offset, length);
            offset += length;

            if (dockable is IFlatProportionalDock childDock)
            {
                ArrangeDock(childDock, childBounds);
            }
            else if (_presenters.TryGetValue(GetItemKey(dockable), out var presenter))
            {
                ArrangeChild(presenter, childBounds, useConnectedAnimation: true);
            }
        }
    }

    private void ArrangeSplitter(
        IFlatProportionalDock dock,
        IList<IFlatProportionalItem> visibleDockables,
        int index,
        IFlatProportionalSplitter splitter,
        Rect bounds,
        ref double offset)
    {
        if (!_splitters.TryGetValue(GetItemKey(splitter), out var splitterControl))
        {
            return;
        }

        splitterControl.Orientation = dock.Orientation;

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
                    presenter.Opacity = 1.0;
                    presenter.IsHitTestVisible = false;
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
            return _presenterItems.TryGetValue(presenter, out var dockable)
                   && _layoutActionDockables.Contains(GetItemKey(dockable))
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
            CompleteInsertAnimationAfterDelay(presenter, version, GetTransitionCompletionDelay());
            return;
        }

        var compositor = visual.Compositor;
        if (compositor is null)
        {
            CompleteInsertAnimationAfterDelay(presenter, version, GetTransitionCompletionDelay());
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
            CompleteLayoutAnimationAfterDelay(presenter, version, GetTransitionCompletionDelay());
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

    private async void CompleteLayoutAnimationAfterDelay(ContentPresenter presenter, int version, TimeSpan delay)
    {
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

    private async void CompleteInsertAnimationAfterDelay(ContentPresenter presenter, int version, TimeSpan delay)
    {
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

        if (_presenterItems.TryGetValue(presenter, out var dockable)
            && _removalPresentersByDockable.TryGetValue(GetItemKey(dockable), out var removalPresenter)
            && ReferenceEquals(removalPresenter, presenter))
        {
            _removalPresentersByDockable.Remove(GetItemKey(dockable));
        }

        ResetPresenterComposition(presenter);
        Children.Remove(presenter);
        _presenterItems.Remove(presenter);
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
            || Root is not { } dock)
        {
            return;
        }

        EnsureConnectedStartBounds();

        var previousDockables = new List<IFlatProportionalItem>();
        CaptureCurrentPresenterDockables(previousDockables);
        if (previousDockables.Count == 0)
        {
            return;
        }

        var nextDockables = new List<IFlatProportionalItem>();
        CaptureLeafDockables(dock, nextDockables);

        var previousCount = _layoutActionDockables.Count;
        TrackChangedDockables(previousDockables, nextDockables);

        if (_layoutActionDockables.Count > previousCount)
        {
            _hasScopedLayoutAction = true;
        }
    }

    private void CaptureCurrentPresenterDockables(ICollection<IFlatProportionalItem> dockables)
    {
        foreach (var child in Children)
        {
            if (child is not ContentPresenter presenter
                || !_presenterItems.TryGetValue(presenter, out var dockable)
                || !_presenters.TryGetValue(GetItemKey(dockable), out var activePresenter)
                || !ReferenceEquals(activePresenter, presenter))
            {
                continue;
            }

            dockables.Add(dockable);
        }
    }

    private void CaptureLeafDockables(IFlatProportionalItem dockable, ICollection<IFlatProportionalItem> dockables)
    {
        switch (dockable)
        {
            case IFlatProportionalDock proportionalDock:
            {
                if (proportionalDock.VisibleItems is null)
                {
                    return;
                }

                foreach (var child in proportionalDock.VisibleItems)
                {
                    CaptureLeafDockables(child, dockables);
                }

                return;
            }
            case IFlatProportionalSplitter:
                return;
            default:
                dockables.Add(dockable);
                return;
        }
    }

    private void TrackChangedDockables(IList<IFlatProportionalItem> previousDockables, IList<IFlatProportionalItem> nextDockables)
    {
        if (TrackSingleMovedDockable(previousDockables, nextDockables))
        {
            return;
        }

        for (var index = 0; index < previousDockables.Count; index++)
        {
            var dockable = previousDockables[index];
            var nextIndex = IndexOfItemKey(nextDockables, dockable);
            if (nextIndex < 0 || nextIndex != index)
            {
                _layoutActionDockables.Add(GetItemKey(dockable));
            }
        }

        for (var index = 0; index < nextDockables.Count; index++)
        {
            var dockable = nextDockables[index];
            var previousIndex = IndexOfItemKey(previousDockables, dockable);
            if (previousIndex < 0 || previousIndex != index)
            {
                _layoutActionDockables.Add(GetItemKey(dockable));
            }
        }
    }

    private bool TrackSingleMovedDockable(IList<IFlatProportionalItem> previousDockables, IList<IFlatProportionalItem> nextDockables)
    {
        if (previousDockables.Count != nextDockables.Count || previousDockables.Count < 3)
        {
            return false;
        }

        IFlatProportionalItem? movedDockable = null;
        for (var previousIndex = 0; previousIndex < previousDockables.Count; previousIndex++)
        {
            var dockable = previousDockables[previousIndex];
            var nextIndex = IndexOfItemKey(nextDockables, dockable);
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

        _layoutActionDockables.Add(GetItemKey(movedDockable));
        return true;
    }

    private static bool MatchesAfterMove(
        IList<IFlatProportionalItem> previousDockables,
        IList<IFlatProportionalItem> nextDockables,
        int previousIndex,
        int nextIndex)
    {
        for (var index = 0; index < nextDockables.Count; index++)
        {
            var candidateIndex = GetMovedSequenceSourceIndex(index, previousIndex, nextIndex);
            if (!ItemKeysEqual(previousDockables[candidateIndex], nextDockables[index]))
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

    private static int IndexOfItemKey(IList<IFlatProportionalItem> dockables, IFlatProportionalItem dockable)
    {
        for (var index = 0; index < dockables.Count; index++)
        {
            if (ItemKeysEqual(dockables[index], dockable))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool ItemKeysEqual(IFlatProportionalItem left, IFlatProportionalItem right)
    {
        return Equals(GetItemKey(left), GetItemKey(right));
    }

    private static object GetItemKey(IFlatProportionalItem item)
    {
        return item.Key;
    }

    private void TrackLayoutActionDockables(System.Collections.IList? dockables)
    {
        if (dockables is null)
        {
            return;
        }

        foreach (var item in dockables)
        {
            if (item is IFlatProportionalItem dockable)
            {
                TrackLayoutActionDockable(dockable);
            }
        }
    }

    private void TrackLayoutActionDockable(IFlatProportionalItem dockable)
    {
        switch (dockable)
        {
            case IFlatProportionalDock proportionalDock:
            {
                if (proportionalDock.VisibleItems is null)
                {
                    return;
                }

                foreach (var child in proportionalDock.VisibleItems)
                {
                    TrackLayoutActionDockable(child);
                }

                return;
            }
            case IFlatProportionalSplitter:
                return;
            default:
                _layoutActionDockables.Add(GetItemKey(dockable));
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

    private double GetTotalSplitterThickness(IList<IFlatProportionalItem> visibleDockables)
    {
        var total = 0.0;

        for (var i = 0; i < visibleDockables.Count; i++)
        {
            if (visibleDockables[i] is IFlatProportionalSplitter splitter
                && ShouldUseSplitter(visibleDockables, i)
                && _splitters.TryGetValue(GetItemKey(splitter), out var splitterControl))
            {
                total += splitterControl.Thickness;
            }
        }

        return total;
    }

    private Dictionary<IFlatProportionalItem, double> AssignProportions(IFlatProportionalDock dock, Size size, double splitterThickness)
    {
        var targets = new Dictionary<IFlatProportionalItem, double>(ReferenceEqualityComparer.Instance);
        if (dock.VisibleItems is not { } visibleDockables)
        {
            return targets;
        }

        var dockables = new List<IFlatProportionalItem>();
        foreach (var dockable in visibleDockables)
        {
            if (dockable is not IFlatProportionalSplitter)
            {
                dockables.Add(dockable);
            }
        }

        if (dockables.Count == 0)
        {
            return targets;
        }

        _isAssigningProportions = true;
        try
        {
            var availableLength = GetLength(size, dock.Orientation) - splitterThickness;
            var canWriteProportions = CanAssignConstrainedProportions(dockables, dock.Orientation, availableLength);
            var hasCollapsed = false;
            var assignedTotal = 0.0;
            var unassignedCount = 0;
            var restoreCollapsedProportions = ShouldRestoreCollapsedProportions(dockables);

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

                var target = GetTargetProportion(dockable, restoreCollapsedProportions);

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

            if (!canWriteProportions)
            {
                return targets;
            }

            ApplyActiveProportionConstraints(dockables, targets, dock.Orientation, availableLength);

            foreach (var dockable in dockables)
            {
                var isCollapsed = IsCollapsed(dockable);
                SetDockableProportion(dockable, targets[dockable], !isCollapsed && !hasCollapsed);
            }

            return targets;
        }
        finally
        {
            _isAssigningProportions = false;
        }
    }

    private void ApplyActiveProportionConstraints(
        IList<IFlatProportionalItem> dockables,
        IDictionary<IFlatProportionalItem, double> targets,
        Avalonia.Layout.Orientation orientation,
        double availableLength)
    {
        if (!IsValidProportion(availableLength) || availableLength <= 0)
        {
            return;
        }

        var activeDockables = new List<IFlatProportionalItem>();
        foreach (var dockable in dockables)
        {
            if (!IsCollapsed(dockable))
            {
                activeDockables.Add(dockable);
            }
        }

        if (activeDockables.Count == 0)
        {
            return;
        }

        var values = new double[activeDockables.Count];
        var weights = new double[activeDockables.Count];
        var minimums = new double[activeDockables.Count];
        var maximums = new double[activeDockables.Count];
        var isFixed = new bool[activeDockables.Count];

        for (var i = 0; i < activeDockables.Count; i++)
        {
            var dockable = activeDockables[i];
            var target = targets[dockable];
            var constraints = GetProportionConstraints(dockable, orientation, availableLength);
            var minimum = ResolveValidProportion(constraints.Min, 0);
            var maximum = constraints.Max;

            if (double.IsNaN(maximum) || maximum < minimum)
            {
                maximum = minimum;
            }

            weights[i] = ResolveValidProportion(target, 0);
            minimums[i] = minimum;
            maximums[i] = maximum;
        }

        for (var iteration = 0; iteration < activeDockables.Count; iteration++)
        {
            var fixedTotal = 0.0;
            var flexibleCount = 0;
            var flexibleWeightTotal = 0.0;

            for (var i = 0; i < activeDockables.Count; i++)
            {
                if (isFixed[i])
                {
                    fixedTotal += values[i];
                    continue;
                }

                flexibleCount++;
                flexibleWeightTotal += weights[i];
            }

            if (flexibleCount == 0)
            {
                break;
            }

            var remainingTotal = Math.Max(0, 1.0 - fixedTotal);
            var useEqualWeights = flexibleWeightTotal <= 0;
            var changed = false;

            for (var i = 0; i < activeDockables.Count; i++)
            {
                if (isFixed[i])
                {
                    continue;
                }

                var candidate = useEqualWeights
                    ? remainingTotal / flexibleCount
                    : remainingTotal * weights[i] / flexibleWeightTotal;
                var minimum = minimums[i];
                var maximum = maximums[i];

                if (candidate < minimum && !AreClose(candidate, minimum))
                {
                    values[i] = minimum;
                    isFixed[i] = true;
                    changed = true;
                    continue;
                }

                if (!double.IsPositiveInfinity(maximum) && candidate > maximum && !AreClose(candidate, maximum))
                {
                    values[i] = maximum;
                    isFixed[i] = true;
                    changed = true;
                    continue;
                }

                values[i] = candidate;
            }

            if (!changed)
            {
                break;
            }
        }

        for (var i = 0; i < activeDockables.Count; i++)
        {
            targets[activeDockables[i]] = Math.Max(0, values[i]);
        }
    }

    private static void NormalizeActiveProportions(IList<IFlatProportionalItem> dockables, IDictionary<IFlatProportionalItem, double> targets)
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

    private bool CanAssignConstrainedProportions(
        IList<IFlatProportionalItem> dockables,
        Avalonia.Layout.Orientation orientation,
        double availableLength)
    {
        if (!IsValidProportion(availableLength) || availableLength <= 0)
        {
            return false;
        }

        var requiredLength = 0.0;
        for (var i = 0; i < dockables.Count; i++)
        {
            var dockable = dockables[i];
            if (IsCollapsed(dockable))
            {
                continue;
            }

            var minimum = GetMinimumLength(dockable, orientation);
            var minimumLength = MinimumProportionSize > 0 ? MinimumProportionSize : 0.0;
            if (!double.IsNaN(minimum) && minimum > 0)
            {
                minimumLength = Math.Max(minimumLength, minimum);
            }

            requiredLength += minimumLength;
        }

        return requiredLength <= 0 || availableLength >= requiredLength;
    }

    private static bool ShouldRestoreCollapsedProportions(IList<IFlatProportionalItem> dockables)
    {
        for (var i = 0; i < dockables.Count; i++)
        {
            var dockable = dockables[i];
            if (!IsCollapsed(dockable)
                && IsValidProportion(dockable.CollapsedProportion)
                && dockable.CollapsedProportion > 0
                && (!IsValidProportion(dockable.Proportion) || dockable.Proportion <= 0))
            {
                return true;
            }
        }

        return false;
    }

    private static double GetTargetProportion(IFlatProportionalItem dockable, bool restoreCollapsedProportions)
    {
        var collapsedProportion = dockable.CollapsedProportion;
        if (restoreCollapsedProportions && IsValidProportion(collapsedProportion) && collapsedProportion > 0)
        {
            return collapsedProportion;
        }

        return IsValidProportion(dockable.Proportion)
            ? dockable.Proportion
            : collapsedProportion;
    }

    private static double GetLayoutProportion(
        IReadOnlyDictionary<IFlatProportionalItem, double> proportions,
        IFlatProportionalItem dockable)
    {
        return proportions.TryGetValue(dockable, out var proportion) && IsValidProportion(proportion)
            ? proportion
            : ResolveValidProportion(dockable.Proportion, 0);
    }

    private void ApplyResizeConstraints(
        Avalonia.Layout.Orientation orientation,
        double availableSize,
        IFlatProportionalItem primary,
        IFlatProportionalItem secondary,
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

    private (double Min, double Max) GetProportionConstraints(IFlatProportionalItem dockable, Avalonia.Layout.Orientation orientation, double availableSize)
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

    private static IFlatProportionalItem? FindResizeSibling(IList<IFlatProportionalItem> dockables, int splitterIndex, int direction)
    {
        for (var index = splitterIndex + direction; index >= 0 && index < dockables.Count; index += direction)
        {
            var dockable = dockables[index];
            if (dockable is IFlatProportionalSplitter || IsCollapsed(dockable))
            {
                continue;
            }

            return dockable;
        }

        return null;
    }

    private static bool ShouldUseSplitter(IList<IFlatProportionalItem> dockables, int splitterIndex)
    {
        if (dockables[splitterIndex] is not IFlatProportionalSplitter)
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

    private static IFlatProportionalItem? FindAdjacentDockable(IList<IFlatProportionalItem> dockables, int splitterIndex, int direction)
    {
        var index = splitterIndex + direction;
        if (index < 0 || index >= dockables.Count)
        {
            return null;
        }

        var dockable = dockables[index];
        return dockable is IFlatProportionalSplitter ? null : dockable;
    }

    private Size NormalizeDesiredSize(Size availableSize, IFlatProportionalDock dock)
    {
        var desired = CalculateDesiredSize(dock);
        var width = double.IsInfinity(availableSize.Width) ? desired.Width : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? desired.Height : availableSize.Height;
        return new Size(width, height);
    }

    private Size CalculateDesiredSize(IFlatProportionalItem item)
    {
        return item is IFlatProportionalDock dock
            ? CalculateDockDesiredSize(dock)
            : CalculateItemDesiredSize(item);
    }

    private Size CalculateDockDesiredSize(IFlatProportionalDock dock)
    {
        var surfaceDesired = GetDockSurfaceDesiredSize(dock);
        if (dock.VisibleItems is not { } visibleDockables || visibleDockables.Count == 0)
        {
            return surfaceDesired;
        }

        var stackingLength = 0.0;
        var splitterLength = 0.0;
        var crossLength = 0.0;
        var restoreCollapsedProportions = ShouldRestoreVisibleCollapsedProportions(visibleDockables);
        var assignedTotal = 0.0;
        var unassignedCount = 0;

        foreach (var dockable in visibleDockables)
        {
            if (dockable is IFlatProportionalSplitter || IsCollapsed(dockable))
            {
                continue;
            }

            var target = GetTargetProportion(dockable, restoreCollapsedProportions);
            if (IsValidProportion(target))
            {
                assignedTotal += target;
            }
            else
            {
                unassignedCount++;
            }
        }

        var unassignedProportion = unassignedCount > 0 ? Math.Max(0, 1.0 - assignedTotal) / unassignedCount : 0.0;
        var activeTotal = assignedTotal + (unassignedProportion * unassignedCount);
        var scale = activeTotal > 0 && Math.Abs(activeTotal - 1.0) >= 1e-10 ? 1.0 / activeTotal : 1.0;

        for (var i = 0; i < visibleDockables.Count; i++)
        {
            var dockable = visibleDockables[i];
            Size childDesired;

            if (dockable is IFlatProportionalSplitter splitter)
            {
                if (!ShouldUseSplitter(visibleDockables, i))
                {
                    continue;
                }

                childDesired = CalculateSplitterDesiredSize(splitter, dock.Orientation);
                splitterLength += GetLength(childDesired, dock.Orientation);
            }
            else if (IsCollapsed(dockable))
            {
                childDesired = default;
            }
            else
            {
                childDesired = CalculateDesiredSize(dockable);
                var childStackingLength = GetLength(childDesired, dock.Orientation);
                var target = GetTargetProportion(dockable, restoreCollapsedProportions);
                var proportion = (IsValidProportion(target) ? target : unassignedProportion) * scale;
                var requiredStackingLength = proportion > 0 ? childStackingLength / proportion : childStackingLength;
                stackingLength = Math.Max(stackingLength, requiredStackingLength);
            }

            if (dock.Orientation == Avalonia.Layout.Orientation.Vertical)
            {
                crossLength = Math.Max(crossLength, childDesired.Width);
            }
            else
            {
                crossLength = Math.Max(crossLength, childDesired.Height);
            }
        }

        var width = dock.Orientation == Avalonia.Layout.Orientation.Vertical
            ? crossLength
            : stackingLength + splitterLength;
        var height = dock.Orientation == Avalonia.Layout.Orientation.Vertical
            ? stackingLength + splitterLength
            : crossLength;

        width = Math.Max(width, surfaceDesired.Width);
        height = Math.Max(height, surfaceDesired.Height);
        width = ApplyDesiredConstraints(width, dock.MinWidth, dock.MaxWidth);
        height = ApplyDesiredConstraints(height, dock.MinHeight, dock.MaxHeight);

        return new Size(width, height);
    }

    private static bool ShouldRestoreVisibleCollapsedProportions(IList<IFlatProportionalItem> visibleDockables)
    {
        for (var i = 0; i < visibleDockables.Count; i++)
        {
            var dockable = visibleDockables[i];
            if (dockable is IFlatProportionalSplitter)
            {
                continue;
            }

            if (!IsCollapsed(dockable)
                && IsValidProportion(dockable.CollapsedProportion)
                && dockable.CollapsedProportion > 0
                && (!IsValidProportion(dockable.Proportion) || dockable.Proportion <= 0))
            {
                return true;
            }
        }

        return false;
    }

    private Size CalculateItemDesiredSize(IFlatProportionalItem dockable)
    {
        var width = 0.0;
        var height = 0.0;

        if (_presenters.TryGetValue(GetItemKey(dockable), out var presenter))
        {
            width = GetFiniteDesiredDimension(presenter.DesiredSize.Width);
            height = GetFiniteDesiredDimension(presenter.DesiredSize.Height);
        }

        width = ApplyDesiredConstraints(width, dockable.MinWidth, dockable.MaxWidth);
        height = ApplyDesiredConstraints(height, dockable.MinHeight, dockable.MaxHeight);

        return new Size(width, height);
    }

    private Size GetDockSurfaceDesiredSize(IFlatProportionalDock dock)
    {
        if (!_dockSurfaces.TryGetValue(GetItemKey(dock), out var surface))
        {
            return default;
        }

        return new Size(
            GetFiniteDesiredDimension(surface.DesiredSize.Width),
            GetFiniteDesiredDimension(surface.DesiredSize.Height));
    }

    private Size CalculateSplitterDesiredSize(IFlatProportionalSplitter splitter, Avalonia.Layout.Orientation orientation)
    {
        var thickness = SplitterThickness;
        if (_splitters.TryGetValue(GetItemKey(splitter), out var splitterControl))
        {
            thickness = splitterControl.Thickness;
        }

        return orientation == Avalonia.Layout.Orientation.Vertical
            ? new Size(0, thickness)
            : new Size(thickness, 0);
    }

    private static double ApplyDesiredConstraints(double value, double minimum, double maximum)
    {
        var constrained = GetFiniteDesiredDimension(value);
        if (!double.IsNaN(minimum) && !double.IsInfinity(minimum) && minimum > constrained)
        {
            constrained = minimum;
        }

        if (!double.IsNaN(maximum) && !double.IsPositiveInfinity(maximum) && maximum < constrained)
        {
            constrained = Math.Max(0, maximum);
        }

        return constrained;
    }

    private static double GetFiniteDesiredDimension(double value)
    {
        return double.IsNaN(value) || double.IsInfinity(value) || value < 0 ? 0 : value;
    }

    private static Size CreateChildSize(Size availableSize, Avalonia.Layout.Orientation orientation, double length)
    {
        return orientation == Avalonia.Layout.Orientation.Vertical
            ? new Size(availableSize.Width, length)
            : new Size(length, availableSize.Height);
    }

    private static Rect CreateChildRect(Rect bounds, Avalonia.Layout.Orientation orientation, double offset, double length)
    {
        return orientation == Avalonia.Layout.Orientation.Vertical
            ? new Rect(bounds.X, bounds.Y + offset, bounds.Width, length)
            : new Rect(bounds.X + offset, bounds.Y, length, bounds.Height);
    }

    private static double CalculateDimensionWithConstraints(
        IFlatProportionalItem dockable,
        Avalonia.Layout.Orientation orientation,
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
        if (!IsValidProportion(proportion) || proportion <= 0 || double.IsNaN(dimension) || dimension <= 0)
        {
            return 0;
        }

        var childDimension = dimension * proportion;
        if (double.IsPositiveInfinity(childDimension))
        {
            return double.PositiveInfinity;
        }

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

    private static double GetLength(Size size, Avalonia.Layout.Orientation orientation)
    {
        return orientation == Avalonia.Layout.Orientation.Vertical ? size.Height : size.Width;
    }

    private static double GetMinimumLength(IFlatProportionalItem dockable, Avalonia.Layout.Orientation orientation)
    {
        return orientation == Avalonia.Layout.Orientation.Vertical ? dockable.MinHeight : dockable.MinWidth;
    }

    private static double GetMaximumLength(IFlatProportionalItem dockable, Avalonia.Layout.Orientation orientation)
    {
        return orientation == Avalonia.Layout.Orientation.Vertical ? dockable.MaxHeight : dockable.MaxWidth;
    }

    private static bool IsCollapsed(IFlatProportionalItem dockable)
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

    private static void SetDockableProportion(IFlatProportionalItem dockable, double value, bool updateCollapsedProportion)
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
}
