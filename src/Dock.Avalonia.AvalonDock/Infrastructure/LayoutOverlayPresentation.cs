using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Media;
using AvalonDock.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Adapts window template parts to Dock's presentation contract. All decisions
// about whether an operation is allowed and what it changes stay in Dock.
internal sealed class LayoutOverlayPresentation : IDockTargetPresentation, IDisposable
{
    private readonly OverlayWindow _window;
    private readonly LayoutOverlayProjection _projection;
    private readonly DockTargetBase _target;
    private readonly Canvas _container;
    private readonly Control? _nativePresenter;
    private readonly Path? _preview;
    private readonly RectangleGeometry _previewGeometry = new();
    private readonly Grid?[] _groups;
    private readonly Control?[] _selectors = new Control?[9];
    private readonly Control?[] _fullSelectors = new Control?[5];
    private readonly Grid? _normalGroup;
    private readonly Grid? _fullGroup;
    private uint _visible;
    private DockOperation _active = DockOperation.None;
    private bool _useFull;
    private bool _enabled;
    private bool _disposed;

    private LayoutOverlayPresentation(OverlayWindow window, LayoutOverlayProjection projection,
        DockTargetBase target, INameScope scope, Canvas container)
    {
        _window = window;
        _projection = projection;
        _target = target;
        _container = container;
        _preview = scope.Find<Path>("PART_PreviewBox");
        _nativePresenter = scope.Find<Control>("PART_NativeTargetPresenter") ?? scope.Find<Control>("PART_ContentPresenter");
        _groups = [scope.Find<Grid>("PART_DockingManagerDropTargets"),
            scope.Find<Grid>("PART_AnchorablePaneDropTargets"),
            scope.Find<Grid>("PART_DocumentPaneDropTargets"),
            scope.Find<Grid>("PART_DocumentPaneFullDropTargets")];
        int groupIndex = projection.Type switch
        {
            DropAreaType.DockingManager => 0, DropAreaType.AnchorablePane => 1, _ => 2
        };
        _normalGroup = _groups[groupIndex];
        _fullGroup = projection.Type == DropAreaType.DocumentPane ? _groups[3] : null;
        _useFull = _normalGroup is null && _fullGroup is not null;
        string prefix = groupIndex switch
        {
            0 => "PART_DockingManagerDropTarget", 1 => "PART_AnchorablePaneDropTarget", _ => "PART_DocumentPaneDropTarget"
        };
        for (int index = 0; index < 5; index++)
        {
            string direction = index switch { 0 => "Left", 1 => "Top", 2 => "Right", 3 => "Bottom", _ => "Into" };
            if (index < 4 || groupIndex != 0) _selectors[index] = scope.Find<Control>(prefix + direction);
            _fullSelectors[index] = scope.Find<Control>("PART_DocumentPaneFullDropTarget" + direction);
            if (index < 4 && projection.Type == DropAreaType.DocumentPane)
                _selectors[index + 5] = scope.Find<Control>("PART_DocumentPaneDropTarget" + direction + "AsAnchorablePane");
        }
        foreach (Grid? group in _groups)
            group?.SetCurrentValue(Control.IsVisibleProperty, ReferenceEquals(group, _normalGroup) || ReferenceEquals(group, _fullGroup));
        if (_preview is not null)
        {
            _preview.SetCurrentValue(Path.DataProperty, _previewGeometry);
            _preview.SetCurrentValue(Control.IsVisibleProperty, false);
        }
        window.LayoutUpdated += OnLayoutUpdated;
        target.PropertyChanged += OnTargetPropertyChanged;
        UpdatePresentation();
    }

    internal static LayoutOverlayPresentation? Create(OverlayWindow window, LayoutOverlayProjection projection, INameScope scope)
    {
        if (projection.Adorner is not DockTargetBase target ||
            scope.Find<Canvas>("PART_DropTargetsContainer") is not { } container) return null;
        return new LayoutOverlayPresentation(window, projection, target, scope, container);
    }

    public Control? GetSelector(DockOperation operation)
    {
        int index = Index(operation);
        if (index < 0 || _projection.Type == DropAreaType.DocumentPaneGroup && operation != DockOperation.Fill) return null;
        return index < 5 && _useFull ? _fullSelectors[index] : _selectors[index];
    }

    public void SetOperationState(DockOperation operation, bool visible, bool active)
    {
        int index = Index(operation);
        if (index < 0) return;
        if (visible) _visible |= 1u << index;
        else _visible &= ~(1u << index);
        if (active) _active = operation;
        else if (_active == operation) _active = DockOperation.None;
        _selectors[index]?.SetCurrentValue(Visual.OpacityProperty, visible ? 1d : 0d);
        if (index < 5) _fullSelectors[index]?.SetCurrentValue(Visual.OpacityProperty, visible ? 1d : 0d);
        _useFull = _fullGroup is not null && (_normalGroup is null || (_visible & 0x1e0u) != 0);
        UpdateGroups();
        UpdatePreview();
    }

    private static int Index(DockOperation operation) => operation switch
    {
        DockOperation.Left => 0, DockOperation.Top => 1, DockOperation.Right => 2,
        DockOperation.Bottom => 3, DockOperation.Fill => 4, DockOperation.OuterLeft => 5,
        DockOperation.OuterTop => 6, DockOperation.OuterRight => 7, DockOperation.OuterBottom => 8, _ => -1
    };

    private void UpdatePresentation()
    {
        // Explicit native themes/templates remain an escape hatch. The adapter's
        // ordinary target theme is supplied by its inherited TargetTheme setter.
        bool customTheme = _target.Theme is { } theme &&
            (!_projection.Surface.TryFindResource("AvalonDockLocalTargetTheme", out object? standard) || !ReferenceEquals(theme, standard));
        bool customTemplate = _target.GetDiagnostic(TemplatedControl.TemplateProperty).Priority <= BindingPriority.LocalValue;
        bool enabled = !customTheme && !customTemplate;
        if (_target.Presentation is { } other && !ReferenceEquals(other, this)) enabled = false;
        _enabled = enabled;
        _container.SetCurrentValue(Visual.OpacityProperty, enabled ? 1d : 0d);
        _nativePresenter?.SetCurrentValue(Visual.OpacityProperty, enabled ? 0d : 1d);
        if (enabled) _target.Presentation = this;
        else if (ReferenceEquals(_target.Presentation, this)) _target.Presentation = null;
        UpdateLayout();
    }

    private void OnTargetPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TemplatedControl.TemplateProperty || e.Property == TemplatedControl.ThemeProperty) UpdatePresentation();
        else if (e.Property == DockTargetBase.PreviewXProperty || e.Property == DockTargetBase.PreviewYProperty ||
                 e.Property == DockTargetBase.PreviewWidthProperty || e.Property == DockTargetBase.PreviewHeightProperty)
            UpdatePreview();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e) => UpdateLayout();

    private void UpdateLayout()
    {
        if (_disposed || !_enabled) return;
        Rect bounds = LayoutOverlayGeometry.GetBounds(_projection.Surface, new Rect(_projection.Surface.Bounds.Size), _container);
        foreach (Grid? group in _groups)
        {
            if (group is null || !group.IsVisible) continue;
            group.SetCurrentValue(Canvas.LeftProperty, bounds.X);
            group.SetCurrentValue(Canvas.TopProperty, bounds.Y);
            group.SetCurrentValue(Control.WidthProperty, bounds.Width);
            group.SetCurrentValue(Control.HeightProperty, bounds.Height);
        }
        UpdateGroups();
        UpdatePreview();
    }

    private void UpdateGroups()
    {
        _normalGroup?.SetCurrentValue(Visual.OpacityProperty, _useFull ? 0d : 1d);
        _fullGroup?.SetCurrentValue(Visual.OpacityProperty, _useFull ? 1d : 0d);
    }

    private void UpdatePreview()
    {
        if (_preview is null) return;
        bool active = _enabled && _active != DockOperation.None;
        _preview.SetCurrentValue(Control.IsVisibleProperty, active);
        if (!active) return;
        Rect bounds;
        if (_target.PreviewWidth > 0 && _target.PreviewHeight > 0)
        {
            Control coordinates = TopLevel.GetTopLevel(_target) is not null ? _target
                : AdornerLayer.GetAdornedElement(_target) as Control ?? _projection.Surface;
            bounds = LayoutOverlayGeometry.GetBounds(coordinates,
                new Rect(_target.PreviewX, _target.PreviewY, _target.PreviewWidth, _target.PreviewHeight), _container);
        }
        else
        {
            Control source = Index(_active) >= 5
                ? AdornerLayer.GetAdornedElement(_target) as Control ?? _projection.Surface : _projection.Surface;
            bounds = LayoutOverlayGeometry.GetBounds(source, new Rect(source.Bounds.Size), _container);
            double fraction = Index(_active) >= 5 ? 1d / 3 : 0.5;
            bounds = _active switch
            {
                DockOperation.Left or DockOperation.OuterLeft => new Rect(bounds.X, bounds.Y, bounds.Width * fraction, bounds.Height),
                DockOperation.Right or DockOperation.OuterRight => new Rect(bounds.Right - bounds.Width * fraction, bounds.Y, bounds.Width * fraction, bounds.Height),
                DockOperation.Top or DockOperation.OuterTop => new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height * fraction),
                DockOperation.Bottom or DockOperation.OuterBottom => new Rect(bounds.X, bounds.Bottom - bounds.Height * fraction, bounds.Width, bounds.Height * fraction),
                _ => bounds
            };
        }
        _previewGeometry.Rect = bounds;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _window.LayoutUpdated -= OnLayoutUpdated;
        _target.PropertyChanged -= OnTargetPropertyChanged;
        if (ReferenceEquals(_target.Presentation, this)) _target.Presentation = null;
        if (_preview is not null)
        {
            _preview.SetCurrentValue(Control.IsVisibleProperty, false);
            _preview.SetCurrentValue(Path.DataProperty, null);
        }
    }
}
