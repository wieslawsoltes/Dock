// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="PinnedDockControl"/> xaml.
/// </summary>
[TemplatePart("PART_PinnedDock", typeof(ContentControl)/*, IsRequired = true*/)]
[TemplatePart("PART_PinnedDockGrid", typeof(Grid)/*, IsRequired = true*/)]
[TemplatePart("PART_PinnedDockSplitter", typeof(GridSplitter))]
public class PinnedDockControl : TemplatedControl
{
    private const double BoundsEpsilon = 0.5;
    private const double InlineResizeMinSize = 50;

    /// <summary>
    /// Define the <see cref="PinnedDockAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<Alignment> PinnedDockAlignmentProperty = AvaloniaProperty.Register<PinnedDockControl, Alignment>(nameof(PinnedDockAlignment));

    /// <summary>
    /// Define the <see cref="PinnedDockDisplayMode"/> property.
    /// </summary>
    public static readonly StyledProperty<PinnedDockDisplayMode> PinnedDockDisplayModeProperty =
        AvaloniaProperty.Register<PinnedDockControl, PinnedDockDisplayMode>(nameof(PinnedDockDisplayMode), PinnedDockDisplayMode.Overlay);

    /// <summary>
    /// Gets or sets pinned dock alignment
    /// </summary>
    public Alignment PinnedDockAlignment
    {
        get => GetValue(PinnedDockAlignmentProperty);
        set => SetValue(PinnedDockAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets pinned dock display mode.
    /// </summary>
    public PinnedDockDisplayMode PinnedDockDisplayMode
    {
        get => GetValue(PinnedDockDisplayModeProperty);
        set => SetValue(PinnedDockDisplayModeProperty, value);
    }

    private Grid? _pinnedDockGrid;
    private ContentControl? _pinnedDock;
    private GridSplitter? _pinnedDockSplitter;
    private PinnedDockWindow? _window;
    private Window? _ownerWindow;
    private ManagedWindowLayer? _managedLayer;
    private Control? _managedPinnedHost;
    private IDockable? _lastPinnedDockable;
    private double _lastPinnedWidth = double.NaN;
    private double _lastPinnedHeight = double.NaN;
    private bool _isResizingPinnedDock;
    private PinnedDockDisplayMode _lastEffectiveDisplayMode = PinnedDockDisplayMode.Overlay;
    private IDockable? _trackedDockable;
    private AvaloniaObject? _trackedAvaloniaDockable;
    private INotifyPropertyChanged? _trackedNotifyDockable;
    private bool _isPinnedDockEmpty = true;
    private bool _skipPinnedBoundsUpdate;

    static PinnedDockControl()
    {
        PinnedDockAlignmentProperty.Changed.AddClassHandler<PinnedDockControl>((control, e) => control.UpdateGrid());
        PinnedDockDisplayModeProperty.Changed.AddClassHandler<PinnedDockControl>((control, e) => control.OnDisplayModeChanged());
    }

    private bool IsOverlayDisplayMode()
    {
        return GetEffectiveDisplayMode() == PinnedDockDisplayMode.Overlay;
    }

    private PinnedDockDisplayMode GetEffectiveDisplayMode()
    {
        if (DataContext is IRootDock rootDock)
        {
            var dockable = GetPinnedDockable(rootDock);
            if (dockable?.PinnedDockDisplayModeOverride is { } overrideMode)
            {
                return overrideMode;
            }
        }

        return PinnedDockDisplayMode;
    }

    private void OnDisplayModeChanged()
    {
        _lastEffectiveDisplayMode = GetEffectiveDisplayMode();
        UpdateGrid();
        if (IsOverlayDisplayMode())
        {
            if (DockSettings.UsePinnedDockWindow)
            {
                UpdateWindow();
            }
        }
        else
        {
            CloseWindow();
        }
        InvalidateMeasure();
    }

    private void UpdateGrid()
    {
        if (_pinnedDockGrid == null || _pinnedDock == null)
            return;

        _pinnedDockGrid.RowDefinitions.Clear();
        _pinnedDockGrid.ColumnDefinitions.Clear();
        var isOverlay = IsOverlayDisplayMode();
        switch (PinnedDockAlignment)
        {
            case Alignment.Unset:
            case Alignment.Left:
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { MinWidth = 50 });
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                if (isOverlay)
                {
                    _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star) { MinWidth = 50 });
                }
                Grid.SetColumn(_pinnedDock, 0);
                Grid.SetRow(_pinnedDock, 0);
                if (_pinnedDockSplitter != null)
                {
                    Grid.SetRow(_pinnedDockSplitter, 0);
                    Grid.SetColumn(_pinnedDockSplitter, 1);
                }
                break;
            case Alignment.Bottom:
                if (isOverlay)
                {
                    _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star) { MinHeight = 50 });
                    _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                }
                else
                {
                    _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                }
                Grid.SetColumn(_pinnedDock, 0);
                Grid.SetRow(_pinnedDock, isOverlay ? 2 : 1);
                if (_pinnedDockSplitter != null)
                {
                    Grid.SetRow(_pinnedDockSplitter, isOverlay ? 1 : 0);
                    Grid.SetColumn(_pinnedDockSplitter, 0);
                }
                break;
            case Alignment.Right:
                if (isOverlay)
                {
                    _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star) { MinWidth = 50 });
                    _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                    _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { MinWidth = 50 });
                }
                else
                {
                    _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                    _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { MinWidth = 50 });
                }
                Grid.SetColumn(_pinnedDock, isOverlay ? 2 : 1);
                Grid.SetRow(_pinnedDock, 0);
                if (_pinnedDockSplitter != null)
                {
                    Grid.SetRow(_pinnedDockSplitter, 0);
                    Grid.SetColumn(_pinnedDockSplitter, isOverlay ? 1 : 0);
                }
                break;
            case Alignment.Top:
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                if (isOverlay)
                {
                    _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star) { MinHeight = 50 });
                }
                Grid.SetColumn(_pinnedDock, 0);
                Grid.SetRow(_pinnedDock, 0);
                if (_pinnedDockSplitter != null)
                {
                    Grid.SetRow(_pinnedDockSplitter, 1);
                    Grid.SetColumn(_pinnedDockSplitter, 0);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ApplyPinnedDockSize();
        InvalidateMeasure();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        LayoutUpdated -= OnLayoutUpdated;
        this.AttachedToVisualTree -= OnAttached;
        this.DetachedFromVisualTree -= OnDetached;
        DetachSplitterHandlers();
        _pinnedDockGrid = e.NameScope.Get<Grid>("PART_PinnedDockGrid");
        _pinnedDock = e.NameScope.Get<ContentControl>("PART_PinnedDock");
        _pinnedDockSplitter = e.NameScope.Find<GridSplitter>("PART_PinnedDockSplitter");
        AttachSplitterHandlers();
        UpdateGrid();

        LayoutUpdated += OnLayoutUpdated;
        this.AttachedToVisualTree += OnAttached;
        this.DetachedFromVisualTree += OnDetached;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        ApplyPinnedDockSize();
        return base.MeasureOverride(availableSize);
    }

    private void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DockSettings.UsePinnedDockWindow && IsOverlayDisplayMode())
        {
            UpdateWindow();
        }
    }

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        CloseWindow();
        ClearPinnedDockableSubscription();
        LayoutUpdated -= OnLayoutUpdated;
        this.AttachedToVisualTree -= OnAttached;
        this.DetachedFromVisualTree -= OnDetached;
        DetachSplitterHandlers();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        UpdatePinnedDockableSubscription();
        UpdatePinnedDockEmptyState();
        var effectiveMode = GetEffectiveDisplayMode();
        if (effectiveMode != _lastEffectiveDisplayMode)
        {
            _lastEffectiveDisplayMode = effectiveMode;
            OnDisplayModeChanged();
        }

        UpdatePinnedDockableBounds();
        UpdateWindow();
    }

    private void UpdatePinnedDockEmptyState()
    {
        var isEmpty = true;
        if (DataContext is IRootDock rootDock)
        {
            var pinnedDock = rootDock.PinnedDock;
            var visibleDockables = pinnedDock?.VisibleDockables;
            isEmpty = visibleDockables is null || visibleDockables.Count == 0;
        }

        if (isEmpty == _isPinnedDockEmpty)
        {
            return;
        }

        _isPinnedDockEmpty = isEmpty;
        if (isEmpty)
        {
            _lastPinnedDockable = null;
            _lastPinnedWidth = double.NaN;
            _lastPinnedHeight = double.NaN;
            _skipPinnedBoundsUpdate = false;
        }
        InvalidateMeasure();
    }

    private void UpdatePinnedDockableSubscription()
    {
        if (DataContext is not IRootDock rootDock)
        {
            ClearPinnedDockableSubscription();
            return;
        }

        var dockable = GetPinnedDockable(rootDock);
        if (ReferenceEquals(dockable, _trackedDockable))
        {
            return;
        }

        _skipPinnedBoundsUpdate = true;
        ResetPinnedDockSize();
        InvalidateMeasure();
        ClearPinnedDockableSubscription();

        if (dockable is null)
        {
            return;
        }

        _trackedDockable = dockable;
        if (dockable is AvaloniaObject avaloniaDockable)
        {
            _trackedAvaloniaDockable = avaloniaDockable;
            _trackedAvaloniaDockable.PropertyChanged += OnDockableAvaloniaPropertyChanged;
        }
        else if (dockable is INotifyPropertyChanged notifyDockable)
        {
            _trackedNotifyDockable = notifyDockable;
            _trackedNotifyDockable.PropertyChanged += OnDockablePropertyChanged;
        }

        UpdateGrid();
    }

    private void ClearPinnedDockableSubscription()
    {
        if (_trackedAvaloniaDockable is not null)
        {
            _trackedAvaloniaDockable.PropertyChanged -= OnDockableAvaloniaPropertyChanged;
            _trackedAvaloniaDockable = null;
        }

        if (_trackedNotifyDockable is not null)
        {
            _trackedNotifyDockable.PropertyChanged -= OnDockablePropertyChanged;
            _trackedNotifyDockable = null;
        }

        _trackedDockable = null;
    }

    private void OnDockableAvaloniaPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(IDockable.PinnedDockDisplayModeOverride))
        {
            return;
        }

        if (GetEffectiveDisplayMode() == _lastEffectiveDisplayMode)
        {
            return;
        }

        OnDisplayModeChanged();
    }

    private void OnDockablePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IDockable.PinnedDockDisplayModeOverride))
        {
            return;
        }

        if (GetEffectiveDisplayMode() == _lastEffectiveDisplayMode)
        {
            return;
        }

        OnDisplayModeChanged();
    }

    private void UpdateWindow()
    {
        if (!DockSettings.UsePinnedDockWindow || !IsOverlayDisplayMode() || _pinnedDockGrid is null || _pinnedDock is null)
        {
            return;
        }

        if (DataContext is not IRootDock root || root.PinnedDock is null)
        {
            CloseWindow();
            return;
        }

        if (DockSettings.IsManagedWindowHostingEnabled(root))
        {
            UpdateManagedWindow(root);
            return;
        }

        if (!root.PinnedDock.IsEmpty)
        {
            if (_window is null)
            {
                _window = new PinnedDockWindow
                {
                    Content = new ToolDockControl
                    {
                        DataContext = root.PinnedDock
                    }
                };

                if (this.GetVisualRoot() is Window owner)
                {
                    _ownerWindow = owner;
                    _ownerWindow.PositionChanged += OwnerWindow_PositionChanged;
                    _ownerWindow.Resized += OwnerWindow_Resized;
                    _ownerWindow.AddHandler(PointerPressedEvent, OwnerWindow_PointerPressed, RoutingStrategies.Tunnel);
                    _ownerWindow.Deactivated += OwnerWindow_Deactivated;
                    _window.Show(owner);
                }
            }

            var point = _pinnedDock.PointToScreen(new Point());
            _window.Position = new PixelPoint(point.X, point.Y);
            _window.Width = _pinnedDock.Bounds.Width;
            _window.Height = _pinnedDock.Bounds.Height;

            if (_pinnedDock.Opacity != 0)
            {
                _pinnedDock.Opacity = 0;
                _pinnedDock.IsHitTestVisible = false;
            }

            // Keep the splitter visible so the user can resize the docked area
        }
        else
        {
            CloseWindow();
        }
    }

    private void CloseWindow()
    {
        if (_managedLayer is not null)
        {
            _managedLayer.HideOverlay("PinnedDock");
            _managedLayer = null;
        }

        _managedPinnedHost = null;

        if (_window is not null)
        {
            _window.Close();
            _window = null;
        }

        if (_ownerWindow is not null)
        {
            _ownerWindow.PositionChanged -= OwnerWindow_PositionChanged;
            _ownerWindow.Resized -= OwnerWindow_Resized;
            _ownerWindow.RemoveHandler(PointerPressedEvent, OwnerWindow_PointerPressed);
            _ownerWindow.Deactivated -= OwnerWindow_Deactivated;
            _ownerWindow = null;
        }

        if (_pinnedDock is { Opacity: 0 })
        {
            _pinnedDock.ClearValue(OpacityProperty);
            _pinnedDock.ClearValue(IsHitTestVisibleProperty);
        }

    }

    private void UpdateManagedWindow(IRootDock root)
    {
        if (root.PinnedDock is null || root.PinnedDock.IsEmpty)
        {
            CloseWindow();
            return;
        }

        _managedLayer = ManagedWindowLayer.TryGetLayer(this);
        if (_managedLayer is null)
        {
            return;
        }

        if (_managedPinnedHost is null)
        {
            _managedPinnedHost = new ToolDockControl
            {
                DataContext = root.PinnedDock
            };
        }

        var pinnedDock = _pinnedDock;
        if (pinnedDock is null)
        {
            return;
        }

        var point = pinnedDock.PointToScreen(new Point());
        var bounds = GetManagedBounds(_managedLayer, point, pinnedDock.Bounds.Size);
        _managedLayer.ShowOverlay("PinnedDock", _managedPinnedHost, bounds, true);

        if (pinnedDock.Opacity != 0)
        {
            pinnedDock.Opacity = 0;
            pinnedDock.IsHitTestVisible = false;
        }
    }

    private static Rect GetManagedBounds(ManagedWindowLayer layer, PixelPoint screenPoint, Size size)
    {
        if (layer.GetVisualRoot() is not TopLevel topLevel)
        {
            return new Rect(0, 0, size.Width, size.Height);
        }

        var clientPoint = topLevel.PointToClient(screenPoint);
        var layerOrigin = layer.TranslatePoint(new Point(0, 0), topLevel) ?? new Point(0, 0);
        var local = new Point(clientPoint.X - layerOrigin.X, clientPoint.Y - layerOrigin.Y);
        return new Rect(local, size);
    }

    private void ApplyPinnedDockSize()
    {
        if (_isResizingPinnedDock)
        {
            return;
        }

        if (_pinnedDockGrid is null || DataContext is not IRootDock rootDock)
        {
            return;
        }

        var dockable = GetPinnedDockable(rootDock);
        if (dockable is null)
        {
            return;
        }

        var dockableChanged = !ReferenceEquals(_lastPinnedDockable, dockable);
        if (dockableChanged)
        {
            ResetPinnedDockSize();
            _lastPinnedDockable = dockable;
            _lastPinnedWidth = double.NaN;
            _lastPinnedHeight = double.NaN;
            _skipPinnedBoundsUpdate = true;
        }

        dockable.GetPinnedBounds(out _, out _, out var width, out var height);
        var isOverlay = IsOverlayDisplayMode();

        switch (PinnedDockAlignment)
        {
            case Alignment.Unset:
            case Alignment.Left:
                if (!IsValidSize(width))
                {
                    return;
                }
                if (IsColumnSizeApplied(_pinnedDockGrid.ColumnDefinitions, 0, width))
                {
                    return;
                }
                SetColumnSize(_pinnedDockGrid.ColumnDefinitions, 0, width);
                _lastPinnedDockable = dockable;
                _lastPinnedWidth = width;
                InvalidateMeasure();
                break;
            case Alignment.Right:
                if (!IsValidSize(width))
                {
                    return;
                }
                var rightIndex = isOverlay ? 2 : 1;
                if (IsColumnSizeApplied(_pinnedDockGrid.ColumnDefinitions, rightIndex, width))
                {
                    return;
                }
                SetColumnSize(_pinnedDockGrid.ColumnDefinitions, rightIndex, width);
                _lastPinnedDockable = dockable;
                _lastPinnedWidth = width;
                InvalidateMeasure();
                break;
            case Alignment.Top:
                if (!IsValidSize(height))
                {
                    return;
                }
                if (IsRowSizeApplied(_pinnedDockGrid.RowDefinitions, 0, height))
                {
                    return;
                }
                SetRowSize(_pinnedDockGrid.RowDefinitions, 0, height);
                _lastPinnedDockable = dockable;
                _lastPinnedHeight = height;
                InvalidateMeasure();
                break;
            case Alignment.Bottom:
                if (!IsValidSize(height))
                {
                    return;
                }
                var bottomIndex = isOverlay ? 2 : 1;
                if (IsRowSizeApplied(_pinnedDockGrid.RowDefinitions, bottomIndex, height))
                {
                    return;
                }
                SetRowSize(_pinnedDockGrid.RowDefinitions, bottomIndex, height);
                _lastPinnedDockable = dockable;
                _lastPinnedHeight = height;
                InvalidateMeasure();
                break;
            default:
                break;
        }
    }

    private void ResetPinnedDockSize()
    {
        if (_pinnedDockGrid is null)
        {
            return;
        }

        var isOverlay = IsOverlayDisplayMode();

        switch (PinnedDockAlignment)
        {
            case Alignment.Unset:
            case Alignment.Left:
                ResetColumnSize(_pinnedDockGrid.ColumnDefinitions, 0);
                break;
            case Alignment.Right:
                ResetColumnSize(_pinnedDockGrid.ColumnDefinitions, isOverlay ? 2 : 1);
                break;
            case Alignment.Top:
                ResetRowSize(_pinnedDockGrid.RowDefinitions, 0);
                break;
            case Alignment.Bottom:
                ResetRowSize(_pinnedDockGrid.RowDefinitions, isOverlay ? 2 : 1);
                break;
            default:
                break;
        }

        InvalidateMeasure();
    }

    private void UpdatePinnedDockableBounds()
    {
        if (_pinnedDock is null || DataContext is not IRootDock rootDock)
        {
            return;
        }

        var dockable = GetPinnedDockable(rootDock);
        if (dockable is null)
        {
            return;
        }

        dockable.GetPinnedBounds(out _, out _, out var storedWidth, out var storedHeight);

        var hasStoredBounds = IsValidSize(storedWidth) && IsValidSize(storedHeight);
        if (_skipPinnedBoundsUpdate)
        {
            _skipPinnedBoundsUpdate = false;
            if (!_isResizingPinnedDock && !hasStoredBounds)
            {
                return;
            }
        }
        if (!_isResizingPinnedDock && hasStoredBounds)
        {
            return;
        }

        var width = _pinnedDock.Bounds.Width;
        var height = _pinnedDock.Bounds.Height;

        if (!IsValidSize(width) || !IsValidSize(height))
        {
            return;
        }

        if (AreClose(width, storedWidth) && AreClose(height, storedHeight))
        {
            return;
        }

        dockable.SetPinnedBounds(0, 0, width, height);
        _lastPinnedDockable = dockable;
        _lastPinnedWidth = width;
        _lastPinnedHeight = height;
    }

    private static IDockable? GetPinnedDockable(IRootDock rootDock)
    {
        var pinnedDock = rootDock.PinnedDock;
        if (pinnedDock is null)
        {
            return null;
        }

        if (pinnedDock.ActiveDockable is { } activeDockable
            && activeDockable is not ISplitter
            && pinnedDock.VisibleDockables?.Contains(activeDockable) == true)
        {
            return activeDockable;
        }

        return pinnedDock.VisibleDockables?.FirstOrDefault(dockable => dockable is not ISplitter);
    }

    private static void SetColumnSize(ColumnDefinitions definitions, int index, double size)
    {
        if (index < 0 || index >= definitions.Count)
        {
            return;
        }

        definitions[index].Width = new GridLength(size, GridUnitType.Pixel);
    }

    private static void ResetColumnSize(ColumnDefinitions definitions, int index)
    {
        if (index < 0 || index >= definitions.Count)
        {
            return;
        }

        definitions[index].Width = GridLength.Auto;
    }

    private static void SetRowSize(RowDefinitions definitions, int index, double size)
    {
        if (index < 0 || index >= definitions.Count)
        {
            return;
        }

        definitions[index].Height = new GridLength(size, GridUnitType.Pixel);
    }

    private static void ResetRowSize(RowDefinitions definitions, int index)
    {
        if (index < 0 || index >= definitions.Count)
        {
            return;
        }

        definitions[index].Height = GridLength.Auto;
    }

    private static bool IsColumnSizeApplied(ColumnDefinitions definitions, int index, double size)
    {
        if (index < 0 || index >= definitions.Count)
        {
            return false;
        }

        var length = definitions[index].Width;
        return length.IsAbsolute && AreClose(length.Value, size);
    }

    private static bool IsRowSizeApplied(RowDefinitions definitions, int index, double size)
    {
        if (index < 0 || index >= definitions.Count)
        {
            return false;
        }

        var length = definitions[index].Height;
        return length.IsAbsolute && AreClose(length.Value, size);
    }

    private static bool IsValidSize(double size)
    {
        return !double.IsNaN(size) && !double.IsInfinity(size) && size > 0;
    }

    private static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) <= BoundsEpsilon;
    }

    internal bool TryResizeInlinePinnedDock(Vector delta)
    {
        if (IsOverlayDisplayMode())
        {
            return false;
        }

        if (_pinnedDockGrid is null || _pinnedDock is null || DataContext is not IRootDock rootDock)
        {
            return false;
        }

        var dockable = GetPinnedDockable(rootDock);
        if (dockable is null)
        {
            return false;
        }

        dockable.GetPinnedBounds(out _, out _, out var width, out var height);
        if (!IsValidSize(width) || !IsValidSize(height))
        {
            width = _pinnedDock.Bounds.Width;
            height = _pinnedDock.Bounds.Height;
        }

        if (!IsValidSize(width) || !IsValidSize(height))
        {
            return false;
        }

        var newWidth = width;
        var newHeight = height;

        switch (PinnedDockAlignment)
        {
            case Alignment.Unset:
            case Alignment.Left:
                newWidth = Math.Max(InlineResizeMinSize, width + delta.X);
                SetColumnSize(_pinnedDockGrid.ColumnDefinitions, 0, newWidth);
                break;
            case Alignment.Right:
                newWidth = Math.Max(InlineResizeMinSize, width - delta.X);
                SetColumnSize(_pinnedDockGrid.ColumnDefinitions, 1, newWidth);
                break;
            case Alignment.Top:
                newHeight = Math.Max(InlineResizeMinSize, height + delta.Y);
                SetRowSize(_pinnedDockGrid.RowDefinitions, 0, newHeight);
                break;
            case Alignment.Bottom:
                newHeight = Math.Max(InlineResizeMinSize, height - delta.Y);
                SetRowSize(_pinnedDockGrid.RowDefinitions, 1, newHeight);
                break;
            default:
                return false;
        }

        dockable.SetPinnedBounds(0, 0, newWidth, newHeight);
        _lastPinnedDockable = dockable;
        _lastPinnedWidth = newWidth;
        _lastPinnedHeight = newHeight;
        InvalidateMeasure();
        return true;
    }

    private void AttachSplitterHandlers()
    {
        if (_pinnedDockSplitter is null)
        {
            return;
        }

        _pinnedDockSplitter.DragStarted += OnPinnedDockSplitterDragStarted;
        _pinnedDockSplitter.DragDelta += OnPinnedDockSplitterDragDelta;
        _pinnedDockSplitter.DragCompleted += OnPinnedDockSplitterDragCompleted;
    }

    private void DetachSplitterHandlers()
    {
        if (_pinnedDockSplitter is null)
        {
            return;
        }

        _pinnedDockSplitter.DragStarted -= OnPinnedDockSplitterDragStarted;
        _pinnedDockSplitter.DragDelta -= OnPinnedDockSplitterDragDelta;
        _pinnedDockSplitter.DragCompleted -= OnPinnedDockSplitterDragCompleted;
    }

    private void OnPinnedDockSplitterDragStarted(object? sender, VectorEventArgs e)
    {
        _isResizingPinnedDock = true;
        if (IsOverlayDisplayMode())
        {
            UpdatePinnedDockableBounds();
        }
    }

    private void OnPinnedDockSplitterDragDelta(object? sender, VectorEventArgs e)
    {
        if (IsOverlayDisplayMode())
        {
            UpdatePinnedDockableBounds();
            return;
        }

        TryResizeInlinePinnedDock(e?.Vector ?? default);
    }

    private void OnPinnedDockSplitterDragCompleted(object? sender, VectorEventArgs e)
    {
        if (IsOverlayDisplayMode())
        {
            UpdatePinnedDockableBounds();
        }
        else
        {
            UpdatePinnedDockableBounds();
        }
        _isResizingPinnedDock = false;
    }

    private void OwnerWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        CloseWindow();
        UpdateWindow();
    }

    private void OwnerWindow_Resized(object? sender, EventArgs e)
    {
        UpdateWindow();
    }

    private bool IsPointerInsidePinnedDock(PointerEventArgs e)
    {
        if (_pinnedDockGrid is null)
            return false;

        var point = e.GetPosition(_pinnedDockGrid);
        return point.X >= 0 && point.Y >= 0 &&
               point.X <= _pinnedDockGrid.Bounds.Width &&
               point.Y <= _pinnedDockGrid.Bounds.Height;
    }

    private bool ShouldKeepPinnedDockableVisible()
    {
        if (DataContext is not IRootDock rootDock)
        {
            return false;
        }

        if (rootDock.PinnedDock?.VisibleDockables is null)
        {
            return false;
        }

        foreach (var dockable in rootDock.PinnedDock.VisibleDockables)
        {
            if (dockable.KeepPinnedDockableVisible)
            {
                return true;
            }
        }

        return false;
    }

    private void OwnerWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ShouldKeepPinnedDockableVisible())
        {
            return;
        }

        if (!IsPointerInsidePinnedDock(e))
        {
            CloseWindow();
        }
    }

    private void OwnerWindow_Deactivated(object? sender, EventArgs e)
    {
        if (ShouldKeepPinnedDockableVisible())
        {
            return;
        }

        CloseWindow();
    }
}
