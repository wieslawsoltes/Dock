// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Overlay dock control that hosts background content and overlay items.
/// </summary>
[TemplatePart("PART_BackgroundContent", typeof(ContentPresenter))]
[TemplatePart("PART_OverlayPanels", typeof(ItemsControl))]
[TemplatePart("PART_SplitterGroups", typeof(ItemsControl))]
[TemplatePart("PART_AlignmentGrid", typeof(Border))]
[TemplatePart("PART_OverlayCanvas", typeof(Canvas))]
public class OverlayDockControl : TemplatedControl
{
    private const double LayoutEpsilon = 0.5;
    private ItemsControl? _panelsControl;
    private ItemsControl? _splitterGroupsControl;
    private Border? _alignmentGrid;
    private Canvas? _overlayCanvas;
    private IOverlayDock? _overlayDock;
    private INotifyPropertyChanged? _overlayDockNotifier;
    private bool _isLayoutUpdating;

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_panelsControl != null)
        {
            _panelsControl.ContainerPrepared -= OnPanelContainerPrepared;
            _panelsControl = null;
        }

        if (_splitterGroupsControl != null)
        {
            _splitterGroupsControl.ContainerPrepared -= OnSplitterGroupContainerPrepared;
            _splitterGroupsControl = null;
        }

        _overlayCanvas = e.NameScope.Find<Canvas>("PART_OverlayCanvas");
        _alignmentGrid = e.NameScope.Find<Border>("PART_AlignmentGrid");

        _panelsControl = e.NameScope.Find<ItemsControl>("PART_OverlayPanels");
        if (_panelsControl != null)
        {
            _panelsControl.ContainerPrepared += OnPanelContainerPrepared;
        }

        _splitterGroupsControl = e.NameScope.Find<ItemsControl>("PART_SplitterGroups");
        if (_splitterGroupsControl != null)
        {
            _splitterGroupsControl.ContainerPrepared += OnSplitterGroupContainerPrepared;
        }

        UpdateAlignmentGrid();
        NormalizeZOrder();
        UpdateAnchoredPositions();
        LayoutUpdated -= OnLayoutUpdated;
        LayoutUpdated += OnLayoutUpdated;
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        DetachOverlayDockHandlers();

        _overlayDock = DataContext as IOverlayDock;
        _overlayDockNotifier = _overlayDock as INotifyPropertyChanged;
        if (_overlayDockNotifier != null)
        {
            _overlayDockNotifier.PropertyChanged += OnOverlayDockPropertyChanged;
        }

        UpdateAlignmentGrid();
        NormalizeZOrder();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        LayoutUpdated -= OnLayoutUpdated;
        DetachOverlayDockHandlers();
    }

    private void DetachOverlayDockHandlers()
    {
        if (_overlayDockNotifier != null)
        {
            _overlayDockNotifier.PropertyChanged -= OnOverlayDockPropertyChanged;
            _overlayDockNotifier = null;
        }
        _overlayDock = null;
    }

    private void OnOverlayDockPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IOverlayDock.AlignmentGridSize))
        {
            UpdateAlignmentGrid();
        }
        else if (e.PropertyName == nameof(IOverlayDock.OverlayPanels)
                 || e.PropertyName == nameof(IOverlayDock.SplitterGroups))
        {
            NormalizeZOrder();
            UpdateAnchoredPositions();
        }
    }

    private void OnPanelContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is Control container)
        {
            // Bind positioning to the overlay panel model.
            container[!Canvas.LeftProperty] = new Binding(nameof(IOverlayPanel.X)) { Mode = BindingMode.TwoWay };
            container[!Canvas.TopProperty] = new Binding(nameof(IOverlayPanel.Y)) { Mode = BindingMode.TwoWay };
            container[!Canvas.ZIndexProperty] = new Binding(nameof(IOverlayPanel.ZIndex)) { Mode = BindingMode.TwoWay };
            container[!Layoutable.WidthProperty] = new Binding(nameof(IOverlayPanel.Width)) { Mode = BindingMode.TwoWay };
            container[!Layoutable.HeightProperty] = new Binding(nameof(IOverlayPanel.Height)) { Mode = BindingMode.TwoWay };

            container.RemoveHandler(InputElement.PointerPressedEvent, OnPanelPointerPressed);
            container.AddHandler(InputElement.PointerPressedEvent, OnPanelPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    private void OnSplitterGroupContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is Control container)
        {
            // Bind positioning to the splitter group model.
            container[!Canvas.LeftProperty] = new Binding(nameof(IOverlaySplitterGroup.X)) { Mode = BindingMode.TwoWay };
            container[!Canvas.TopProperty] = new Binding(nameof(IOverlaySplitterGroup.Y)) { Mode = BindingMode.TwoWay };
            container[!Canvas.ZIndexProperty] = new Binding(nameof(IOverlaySplitterGroup.ZIndex)) { Mode = BindingMode.TwoWay };
            container[!Layoutable.WidthProperty] = new Binding(nameof(IOverlaySplitterGroup.Width)) { Mode = BindingMode.TwoWay };
            container[!Layoutable.HeightProperty] = new Binding(nameof(IOverlaySplitterGroup.Height)) { Mode = BindingMode.TwoWay };

            container.RemoveHandler(InputElement.PointerPressedEvent, OnGroupPointerPressed);
            container.AddHandler(InputElement.PointerPressedEvent, OnGroupPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    private void OnPanelPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        if (control.DataContext is not IOverlayPanel panel)
        {
            return;
        }

        var overlayDock = _overlayDock ?? panel.Owner as IOverlayDock;
        if (overlayDock is null)
        {
            return;
        }

        overlayDock.ActiveDockable = panel;
        BringPanelToFront(overlayDock, panel);
        control.Focus();
    }

    private void OnGroupPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        if (control.DataContext is not IOverlaySplitterGroup group)
        {
            return;
        }

        var overlayDock = _overlayDock ?? group.Owner as IOverlayDock;
        if (overlayDock is null)
        {
            return;
        }

        overlayDock.ActiveDockable = group;
        BringGroupToFront(overlayDock, group);
        control.Focus();
    }

    private static void BringPanelToFront(IOverlayDock overlayDock, IOverlayPanel panel)
    {
        if (overlayDock.OverlayPanels is null)
        {
            return;
        }

        var ordered = overlayDock.OverlayPanels.Where(p => p != null).OrderBy(p => p!.ZIndex).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i]!.ZIndex = i;
        }

        var maxZ = ordered.Count == 0 ? 0 : ordered.Max(p => p!.ZIndex);
        panel.ZIndex = maxZ + 1;
    }

    private static void BringGroupToFront(IOverlayDock overlayDock, IOverlaySplitterGroup group)
    {
        if (overlayDock.SplitterGroups is null)
        {
            return;
        }

        var ordered = overlayDock.SplitterGroups.Where(g => g != null).OrderBy(g => g!.ZIndex).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i]!.ZIndex = i;
        }

        var maxZ = ordered.Count == 0 ? 0 : ordered.Max(g => g!.ZIndex);
        group.ZIndex = maxZ + 1;
    }

    private void NormalizeZOrder()
    {
        var overlayDock = _overlayDock ?? DataContext as IOverlayDock;
        if (overlayDock is null)
        {
            return;
        }

        NormalizePanels(overlayDock);
        NormalizeGroups(overlayDock);
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (_isLayoutUpdating)
        {
            return;
        }

        _isLayoutUpdating = true;
        try
        {
            UpdateAnchoredPositions();
        }
        finally
        {
            _isLayoutUpdating = false;
        }
    }

    private static void NormalizePanels(IOverlayDock overlayDock)
    {
        if (overlayDock.OverlayPanels is null)
        {
            return;
        }

        var ordered = overlayDock.OverlayPanels.Where(p => p != null).OrderBy(p => p!.ZIndex).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i]!.ZIndex = i;
        }
    }

    private static void NormalizeGroups(IOverlayDock overlayDock)
    {
        if (overlayDock.SplitterGroups is null)
        {
            return;
        }

        var ordered = overlayDock.SplitterGroups.Where(g => g != null).OrderBy(g => g!.ZIndex).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i]!.ZIndex = i;
        }
    }

    private void UpdateAlignmentGrid()
    {
        if (_alignmentGrid is null)
        {
            return;
        }

        var overlayDock = _overlayDock ?? DataContext as IOverlayDock;
        if (overlayDock is null)
        {
            return;
        }

        var size = overlayDock.AlignmentGridSize;
        if (double.IsNaN(size) || size <= 0)
        {
            size = 16;
        }

        var rect = new Rect(0, 0, size, size);
        var geometryText = FormattableString.Invariant(
            $"M0,0 L{size},0 {size},{size} 0,{size} z M0,0 L0,{size} M0,0 L{size},0");
        var geometry = Geometry.Parse(geometryText);

        var pen = TryGetResource("OverlayAlignmentGridPen", out var penResource) ? penResource as Pen : null;
        if (pen is null)
        {
            Brush? brush = null;
            if (TryGetResource("DockThemeBorderLowBrush", out var brushResource))
            {
                brush = brushResource as Brush;
            }

            if (brush is null)
            {
                brush = new SolidColorBrush(Colors.Gray);
            }
            pen = new Pen(brush, 1);
        }

        var drawing = new GeometryDrawing
        {
            Brush = new SolidColorBrush(Color.Parse("#10FFFFFF")),
            Pen = pen,
            Geometry = geometry
        };

        var relativeRect = new RelativeRect(rect, RelativeUnit.Absolute);

        _alignmentGrid.Background = new DrawingBrush
        {
            Stretch = Stretch.None,
            TileMode = TileMode.Tile,
            SourceRect = relativeRect,
            DestinationRect = relativeRect,
            Drawing = drawing
        };
    }

    private void UpdateAnchoredPositions()
    {
        var overlayDock = _overlayDock ?? DataContext as IOverlayDock;
        if (overlayDock is null || _overlayCanvas is null)
        {
            return;
        }

        var bounds = _overlayCanvas.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var overlaySize = bounds.Size;

        UpdateAnchoredPanels(overlayDock, overlaySize);
        UpdateAnchoredGroups(overlayDock, overlaySize);
    }

    private void UpdateAnchoredPanels(IOverlayDock overlayDock, Size overlaySize)
    {
        if (_panelsControl is null || overlayDock.OverlayPanels is null)
        {
            return;
        }

        foreach (var panel in overlayDock.OverlayPanels)
        {
            if (panel is null || panel.Anchor == OverlayAnchor.None)
            {
                continue;
            }

            var width = panel.Width;
            var height = panel.Height;

            if (!IsValidSize(width) || !IsValidSize(height))
            {
                continue;
            }

            var (x, y) = GetAnchoredPosition(
                panel.Anchor,
                overlaySize,
                new Size(width, height),
                panel.AnchorOffsetX,
                panel.AnchorOffsetY);

            if (Math.Abs(panel.X - x) > LayoutEpsilon)
            {
                panel.X = x;
            }

            if (Math.Abs(panel.Y - y) > LayoutEpsilon)
            {
                panel.Y = y;
            }
        }
    }

    private void UpdateAnchoredGroups(IOverlayDock overlayDock, Size overlaySize)
    {
        if (_splitterGroupsControl is null || overlayDock.SplitterGroups is null)
        {
            return;
        }

        foreach (var group in overlayDock.SplitterGroups)
        {
            if (group is null)
            {
                continue;
            }

            var width = group.Width;
            var height = group.Height;

            if (group.EdgeDock != OverlayEdgeDock.None)
            {
                var (x, y, w, h) = GetEdgeDockedBounds(group.EdgeDock, overlaySize, width, height);
                UpdateGroupBounds(group, x, y, w, h);
                continue;
            }

            if (group.Anchor == OverlayAnchor.None || !IsValidSize(width) || !IsValidSize(height))
            {
                continue;
            }

            var (anchorX, anchorY) = GetAnchoredPosition(
                group.Anchor,
                overlaySize,
                new Size(width, height),
                0,
                0);

            if (Math.Abs(group.X - anchorX) > LayoutEpsilon)
            {
                group.X = anchorX;
            }

            if (Math.Abs(group.Y - anchorY) > LayoutEpsilon)
            {
                group.Y = anchorY;
            }
        }
    }

    private static void UpdateGroupBounds(IOverlaySplitterGroup group, double x, double y, double width, double height)
    {
        if (Math.Abs(group.X - x) > LayoutEpsilon)
        {
            group.X = x;
        }

        if (Math.Abs(group.Y - y) > LayoutEpsilon)
        {
            group.Y = y;
        }

        if (Math.Abs(group.Width - width) > LayoutEpsilon)
        {
            group.Width = width;
        }

        if (Math.Abs(group.Height - height) > LayoutEpsilon)
        {
            group.Height = height;
        }
    }

    private static bool IsValidSize(double size)
    {
        return !double.IsNaN(size) && size > 0;
    }

    private static (double X, double Y) GetAnchoredPosition(
        OverlayAnchor anchor,
        Size overlaySize,
        Size itemSize,
        double offsetX,
        double offsetY)
    {
        var x = offsetX;
        var y = offsetY;

        switch (anchor)
        {
            case OverlayAnchor.Top:
                x = (overlaySize.Width - itemSize.Width) / 2 + offsetX;
                break;
            case OverlayAnchor.TopRight:
                x = overlaySize.Width - itemSize.Width + offsetX;
                break;
            case OverlayAnchor.Left:
                y = (overlaySize.Height - itemSize.Height) / 2 + offsetY;
                break;
            case OverlayAnchor.Center:
                x = (overlaySize.Width - itemSize.Width) / 2 + offsetX;
                y = (overlaySize.Height - itemSize.Height) / 2 + offsetY;
                break;
            case OverlayAnchor.Right:
                x = overlaySize.Width - itemSize.Width + offsetX;
                y = (overlaySize.Height - itemSize.Height) / 2 + offsetY;
                break;
            case OverlayAnchor.BottomLeft:
                y = overlaySize.Height - itemSize.Height + offsetY;
                break;
            case OverlayAnchor.Bottom:
                x = (overlaySize.Width - itemSize.Width) / 2 + offsetX;
                y = overlaySize.Height - itemSize.Height + offsetY;
                break;
            case OverlayAnchor.BottomRight:
                x = overlaySize.Width - itemSize.Width + offsetX;
                y = overlaySize.Height - itemSize.Height + offsetY;
                break;
            case OverlayAnchor.TopLeft:
            case OverlayAnchor.None:
            default:
                break;
        }

        return (x, y);
    }

    private static (double X, double Y, double Width, double Height) GetEdgeDockedBounds(
        OverlayEdgeDock edgeDock,
        Size overlaySize,
        double width,
        double height)
    {
        var dockWidth = width > 0 ? width : overlaySize.Width;
        var dockHeight = height > 0 ? height : overlaySize.Height;

        switch (edgeDock)
        {
            case OverlayEdgeDock.Left:
                return (0, 0, dockWidth, overlaySize.Height);
            case OverlayEdgeDock.Right:
                return (overlaySize.Width - dockWidth, 0, dockWidth, overlaySize.Height);
            case OverlayEdgeDock.Top:
                return (0, 0, overlaySize.Width, dockHeight);
            case OverlayEdgeDock.Bottom:
                return (0, overlaySize.Height - dockHeight, overlaySize.Width, dockHeight);
            case OverlayEdgeDock.Fill:
                return (0, 0, overlaySize.Width, overlaySize.Height);
            case OverlayEdgeDock.None:
            default:
                return (0, 0, dockWidth, dockHeight);
        }
    }

    private bool TryGetResource(object key, out object? value)
    {
        if (Application.Current?.Resources is { } resources)
        {
            return resources.TryGetResource(key, ActualThemeVariant, out value);
        }

        value = null;
        return false;
    }
}
