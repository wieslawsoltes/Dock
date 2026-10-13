using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Adapts Thumb input to the existing native pinned-content bounds. Resizing is
// transactional: only completion changes the model; cancellation removes the ghost.
internal sealed class LayoutAutoHideResizeBehavior : IWeakEventSubscriber<PropertyChangedEventArgs>
{
    private readonly LayoutGridResizerControl _splitter;
    private PinnedDockControl? _host;
    private LayoutAnchorable? _model;
    private ContentControl? _content;
    private Border? _preview;
    private IPointer? _pointer;
    private TopLevel? _inputRoot;
    private bool _dragging;
    private bool _horizontal;
    private double _direction;
    private double _startSize;
    private double _targetPosition;
    private double _minimum;
    private double _maximum;
    private double _originalContentWidth;
    private double _originalContentHeight;
    private Point _origin;
    private Point _areaOrigin;

    internal bool IsResizing => _dragging;

    internal LayoutAutoHideResizeBehavior(LayoutGridResizerControl splitter)
    {
        _splitter = splitter;
        splitter.AttachedToVisualTree += Attached;
        splitter.DetachedFromVisualTree += Detached;
        splitter.PropertyChanged += SplitterChanged;
        splitter.DragStarted += Started;
        splitter.DragDelta += Moved;
        splitter.DragCompleted += Completed;
        splitter.AddHandler(InputElement.PointerPressedEvent, Pressed, handledEventsToo: true);
        Connect();
    }

    internal void Cancel()
    {
        _dragging = false;
        if (_preview is not null) _preview.IsVisible = false;
        _inputRoot?.RemoveHandler(InputElement.KeyDownEvent, KeyDown);
        _inputRoot = null;
        IPointer? pointer = _pointer;
        _pointer = null;
        if (ReferenceEquals(pointer?.Captured, _splitter)) pointer.Capture(null);
    }

    private void Attached(object? sender, VisualTreeAttachmentEventArgs args) => Connect();
    private void Detached(object? sender, VisualTreeAttachmentEventArgs args) => Disconnect();

    private void Connect()
    {
        Disconnect();
        if (TopLevel.GetTopLevel(_splitter) is null || _splitter.FindAncestorOfType<PinnedDockControl>() is not { } host)
            return;
        _host = host;
        _host.PropertyChanged += HostChanged;
        _preview = LayoutAutoHideResize.GetPreview(_splitter);
        _model = LayoutAutoHideResize.GetModel(_splitter);
        if (_model is not null) WeakEvents.ThreadSafePropertyChanged.Subscribe(_model, this);
        if (_splitter.GetVisualParent() is Grid grid)
        {
            foreach (Control child in grid.Children)
                if (child is ContentControl { Name: "PART_PinnedDock" } content) { _content = content; break; }
        }
        if (_content is not null)
        {
            _originalContentWidth = _content.Width;
            _originalContentHeight = _content.Height;
        }
        UpdateSizing(updateContentSize: true);
    }

    private void Disconnect()
    {
        Cancel();
        if (_model is not null) WeakEvents.ThreadSafePropertyChanged.Unsubscribe(_model, this);
        if (_host is not null) _host.PropertyChanged -= HostChanged;
        if (_content is not null)
        {
            _content.SetCurrentValue(Control.WidthProperty, _originalContentWidth);
            _content.SetCurrentValue(Control.HeightProperty, _originalContentHeight);
        }
        _model = null;
        _host = null;
        _preview = null;
        _content = null;
    }

    private void SplitterChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == LayoutAutoHideResize.ModelProperty || args.Property == LayoutAutoHideResize.PreviewProperty)
            Connect();
        else if (args.Property == LayoutSplitterSizing.WidthProperty || args.Property == LayoutSplitterSizing.HeightProperty)
        {
            Cancel();
            UpdateSizing();
        }
        else if (args.Property == LayoutGridResizerControl.IsResizingEnabledProperty && !_splitter.IsResizingEnabled)
            Cancel();
    }

    private void HostChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == PinnedDockControl.PinnedDockAlignmentProperty)
        {
            Cancel();
            UpdateSizing(updateContentSize: true);
        }
        else if (args.Property == Visual.BoundsProperty || args.Property == PinnedDockControl.PinnedDockDisplayModeProperty)
            Cancel();
    }

    public void OnEvent(object? sender, WeakEvent ev, PropertyChangedEventArgs args)
    {
        if (!ReferenceEquals(sender, _model)) return;
        if (args.PropertyName is nameof(LayoutAnchorable.AutoHideMinWidth) or nameof(LayoutAnchorable.AutoHideMinHeight) or null or "")
        {
            Cancel();
            UpdateSizing();
        }
        else if (args.PropertyName is nameof(LayoutAnchorable.AutoHideWidth) or nameof(LayoutAnchorable.AutoHideHeight))
        {
            Cancel();
            UpdateSizing(updateContentSize: args.PropertyName == (_horizontal
                ? nameof(LayoutAnchorable.AutoHideWidth) : nameof(LayoutAnchorable.AutoHideHeight)));
            _host?.InvalidateMeasure();
        }
        else if (args.PropertyName == nameof(LayoutAnchorable.IsAutoHidden)) Cancel();
    }

    private static double Dimension(double value, double fallback) => double.IsFinite(value) && value >= 0 ? value : fallback;

    private void UpdateSizing(bool updateContentSize = false)
    {
        if (_host is null) return;
        _horizontal = _host.PinnedDockAlignment is Alignment.Left or Alignment.Right or Alignment.Unset;
        _direction = _host.PinnedDockAlignment is Alignment.Right or Alignment.Bottom ? -1 : 1;
        double thickness = Dimension(_horizontal ? LayoutSplitterSizing.GetWidth(_splitter) : LayoutSplitterSizing.GetHeight(_splitter), 6);
        _splitter.SetCurrentValue(Control.WidthProperty, _horizontal ? thickness : double.NaN);
        _splitter.SetCurrentValue(Control.HeightProperty, _horizontal ? double.NaN : thickness);
        _splitter.SetCurrentValue(InputElement.CursorProperty,
            new Cursor(_horizontal ? StandardCursorType.SizeWestEast : StandardCursorType.SizeNorthSouth));
        if (updateContentSize && _content is not null && _model is not null)
        {
            double width = _horizontal && _model.AutoHideWidth == 0
                ? Dimension(_model.AutoHideMinWidth, 100) : _originalContentWidth;
            double height = !_horizontal && _model.AutoHideHeight == 0
                ? Dimension(_model.AutoHideMinHeight, 100) : _originalContentHeight;
            _content.SetCurrentValue(Control.WidthProperty, width);
            _content.SetCurrentValue(Control.HeightProperty, height);
        }
    }

    private void Started(object? sender, VectorEventArgs args)
    {
        if (!_splitter.IsResizingEnabled || _model is not { IsAutoHidden: true } || _host is null
            || _content is null || _preview?.Parent is not Canvas canvas) return;
        Control area = _host.FindAncestorOfType<PinnedDockHostPanel>() ?? (Control)_host;
        Point first = area.TranslatePoint(default, canvas) ?? default;
        Point last = area.TranslatePoint(new Point(area.Bounds.Width, area.Bounds.Height), canvas) ?? first;
        _areaOrigin = new Point(Math.Min(first.X, last.X), Math.Min(first.Y, last.Y));
        double available = _horizontal ? Math.Abs(last.X - first.X) : Math.Abs(last.Y - first.Y);
        double thickness = _horizontal ? _splitter.Bounds.Width : _splitter.Bounds.Height;
        double areaStart = _horizontal ? _areaOrigin.X : _areaOrigin.Y;
        // The ghost has physical limits independent of the model's size minimum.
        _minimum = areaStart + 25;
        _maximum = areaStart + available - (_horizontal ? 0 : _model.AutoHideMinHeight);
        double stored = _horizontal ? _model.AutoHideWidth : _model.AutoHideHeight;
        _startSize = stored == 0 ? (_horizontal ? _content.Bounds.Width : _content.Bounds.Height) : stored;
        _origin = _splitter.TranslatePoint(default, canvas) ?? default;
        _targetPosition = _horizontal ? _origin.X : _origin.Y;
        if ((_model.DockModel.PinnedDockDisplayModeOverride ?? _host.PinnedDockDisplayMode) == PinnedDockDisplayMode.Inline)
        {
            // Native inline content has no WPF overlay equivalent. Preserve its
            // size-relative range when its leading splitter starts at zero.
            double minimumSize = Dimension(_horizontal ? _model.AutoHideMinWidth : _model.AutoHideMinHeight, 100);
            double maximumSize = Math.Max(minimumSize, available - thickness - 50);
            double firstDelta = (minimumSize - _startSize) * _direction;
            double lastDelta = (maximumSize - _startSize) * _direction;
            _minimum = _targetPosition + Math.Min(firstDelta, lastDelta);
            _maximum = _targetPosition + Math.Max(firstDelta, lastDelta);
        }
        _preview.Width = _horizontal ? thickness : Math.Abs(last.X - first.X);
        _preview.Height = _horizontal ? Math.Abs(last.Y - first.Y) : thickness;
        _dragging = true;
        _inputRoot = TopLevel.GetTopLevel(_splitter);
        _inputRoot?.AddHandler(InputElement.KeyDownEvent, KeyDown, RoutingStrategies.Tunnel);
        PositionPreview();
        _preview.IsVisible = true;
    }

    private void Pressed(object? sender, PointerPressedEventArgs args)
    {
        if (!_dragging) return;
        _pointer = args.Pointer;
        _pointer.Capture(_splitter);
    }

    private void Moved(object? sender, VectorEventArgs args)
    {
        if (!_dragging) return;
        double delta = _horizontal ? args.Vector.X : args.Vector.Y;
        _targetPosition = Math.Clamp((_horizontal ? _origin.X : _origin.Y) + delta, _minimum, _maximum);
        PositionPreview();
    }

    private void PositionPreview()
    {
        if (_preview is null) return;
        Canvas.SetLeft(_preview, _horizontal ? _targetPosition : _areaOrigin.X);
        Canvas.SetTop(_preview, _horizontal ? _areaOrigin.Y : _targetPosition);
    }

    private void Completed(object? sender, VectorEventArgs args)
    {
        if (!_dragging || _model is not { } model) return;
        double delta = _targetPosition - (_horizontal ? _origin.X : _origin.Y);
        double size = _startSize + _direction * delta;
        Cancel();
        if (_horizontal) model.AutoHideWidth = size;
        else model.AutoHideHeight = size;
        _host?.InvalidateMeasure();
    }

    private void KeyDown(object? sender, KeyEventArgs args)
    {
        if (args.Key != Key.Escape || !_dragging) return;
        Cancel();
        args.Handled = true;
    }
}
