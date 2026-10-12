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
    private double _targetSize;
    private double _minimum;
    private double _maximum;
    private Point _origin;

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
        UpdateSizing();
    }

    private void Disconnect()
    {
        Cancel();
        if (_model is not null) WeakEvents.ThreadSafePropertyChanged.Unsubscribe(_model, this);
        if (_host is not null) _host.PropertyChanged -= HostChanged;
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
            UpdateSizing();
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
            _host?.InvalidateMeasure();
        }
        else if (args.PropertyName == nameof(LayoutAnchorable.IsAutoHidden)) Cancel();
    }

    private static double Dimension(double value, double fallback) => double.IsFinite(value) && value >= 0 ? value : fallback;

    private void UpdateSizing()
    {
        if (_host is null) return;
        _horizontal = _host.PinnedDockAlignment is Alignment.Left or Alignment.Right or Alignment.Unset;
        _direction = _host.PinnedDockAlignment is Alignment.Right or Alignment.Bottom ? -1 : 1;
        double thickness = Dimension(_horizontal ? LayoutSplitterSizing.GetWidth(_splitter) : LayoutSplitterSizing.GetHeight(_splitter), 6);
        _splitter.SetCurrentValue(Control.WidthProperty, _horizontal ? thickness : double.NaN);
        _splitter.SetCurrentValue(Control.HeightProperty, _horizontal ? double.NaN : thickness);
        _splitter.SetCurrentValue(InputElement.CursorProperty,
            new Cursor(_horizontal ? StandardCursorType.SizeWestEast : StandardCursorType.SizeNorthSouth));
        _host.SetCurrentValue(PinnedDockControl.MinimumPinnedWidthProperty, Dimension(_model?.AutoHideMinWidth ?? 100, 100));
        _host.SetCurrentValue(PinnedDockControl.MinimumPinnedHeightProperty, Dimension(_model?.AutoHideMinHeight ?? 100, 100));
    }

    private void Started(object? sender, VectorEventArgs args)
    {
        if (!_splitter.IsResizingEnabled || _model is not { IsAutoHidden: true } || _host is null
            || _content is null || _preview?.Parent is not Canvas canvas) return;
        _minimum = _horizontal ? _host.MinimumPinnedWidth : _host.MinimumPinnedHeight;
        Control area = _host.FindAncestorOfType<PinnedDockHostPanel>() ?? (Control)_host;
        double available = _horizontal ? area.Bounds.Width : area.Bounds.Height;
        double thickness = _horizontal ? _splitter.Bounds.Width : _splitter.Bounds.Height;
        // Match the native overlay grid's reserved space for the document area.
        _maximum = Math.Max(_minimum, available - thickness - 50);
        _startSize = _horizontal ? _content.Bounds.Width : _content.Bounds.Height;
        _targetSize = Math.Clamp(_startSize, _minimum, _maximum);
        _origin = _splitter.TranslatePoint(default, canvas) ?? default;
        _preview.Width = _horizontal ? thickness : _host.Bounds.Width;
        _preview.Height = _horizontal ? _host.Bounds.Height : thickness;
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
        _targetSize = Math.Clamp(_startSize + _direction * delta, _minimum, _maximum);
        PositionPreview();
    }

    private void PositionPreview()
    {
        if (_preview is null) return;
        double delta = (_targetSize - _startSize) * _direction;
        Canvas.SetLeft(_preview, _origin.X + (_horizontal ? delta : 0));
        Canvas.SetTop(_preview, _origin.Y + (_horizontal ? 0 : delta));
    }

    private void Completed(object? sender, VectorEventArgs args)
    {
        if (!_dragging || _model is not { } model) return;
        double size = _targetSize;
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
