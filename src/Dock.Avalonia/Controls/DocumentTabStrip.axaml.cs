// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Interactivity;

using System.Runtime.InteropServices;
using Dock.Avalonia.Internal;
using Avalonia.Layout;
using Avalonia.Media;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStrip custom control.
/// </summary>
[PseudoClasses(":create", ":active")]
public class DocumentTabStrip : TabStrip
{
    private HostWindow? _attachedWindow;
    private Control? _grip;
    private WindowDragHelper? _windowDragHelper;
    private bool _isDragging;
    private DocumentTabStripItem? _dragItem;
    private Point _dragStart;
    private int _dragIndex;

    /// <summary>
    /// Defines the <see cref="DockAdornerHost"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> DockAdornerHostProperty =
        AvaloniaProperty.Register<DocumentTabStrip, Control?>(nameof(DockAdornerHost));
    
    /// <summary>
    /// Defines the <see cref="CanCreateItem"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanCreateItemProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(CanCreateItem));

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(IsActive));
    
    /// <summary>
    /// Define the <see cref="EnableWindowDrag"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableWindowDragProperty = 
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(EnableWindowDrag));

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<DocumentTabStrip, Orientation>(nameof(Orientation));

    /// <summary>
    /// Gets or sets if tab strop dock can create new items.
    /// </summary>
    public bool CanCreateItem
    {
        get => GetValue(CanCreateItemProperty);
        set => SetValue(CanCreateItemProperty, value);
    }

    /// <summary>
    /// Gets or sets if this is the currently active dockable.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
    
    /// <summary>
    /// Gets or sets if the window can be dragged by clicking on the tab strip.
    /// </summary>
    public bool EnableWindowDrag
    {
        get => GetValue(EnableWindowDragProperty);
        set => SetValue(EnableWindowDragProperty, value);
    }

    /// <summary>
    /// Gets or sets orientation of the tab strip.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the control that should host the dock adorner.
    /// </summary>
    public Control? DockAdornerHost
    {
        get => GetValue(DockAdornerHostProperty);
        set => SetValue(DockAdornerHostProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DocumentTabStrip);

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentTabStrip"/> class.
    /// </summary>
    public DocumentTabStrip()
    {
        UpdatePseudoClassesCreate(CanCreateItem);
        UpdatePseudoClassesActive(IsActive);
        AddHandler(PointerPressedEvent, PointerPressedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, PointerReleasedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, PointerMovedHandler, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _grip = e.NameScope.Find<Control>("PART_BorderFill");
        AttachToWindow();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AttachToWindow();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachFromWindow();
        RemoveHandler(PointerPressedEvent, PointerPressedHandler);
        RemoveHandler(PointerReleasedEvent, PointerReleasedHandler);
        RemoveHandler(PointerMovedEvent, PointerMovedHandler);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new DocumentTabStripItem();
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<DocumentTabStripItem>(item, out recycleKey);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanCreateItemProperty)
        {
            UpdatePseudoClassesCreate(change.GetNewValue<bool>());
        }

        if (change.Property == IsActiveProperty)
        {
            UpdatePseudoClassesActive(change.GetNewValue<bool>());
        }

        if (change.Property == EnableWindowDragProperty)
        {
            if (change.GetNewValue<bool>())
            {
                AttachToWindow();
            }
            else
            {
                DetachFromWindow();
            }
        }
    }

    private void UpdatePseudoClassesCreate(bool canCreate)
    {
        PseudoClasses.Set(":create", canCreate);
    }

    private void UpdatePseudoClassesActive(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }

    private WindowDragHelper CreateDragHelper()
    {
        return new WindowDragHelper(
            this,
            () => EnableWindowDrag,
            source =>
            {
                if (source == this)
                    return true;

                var allow = source is { } s &&
                            !(s is DocumentTabStripItem) &&
                            !(s is Button) &&
                            !WindowDragHelper.IsChildOfType<DocumentTabStripItem>(this, s) &&
                            !WindowDragHelper.IsChildOfType<Button>(this, s);

                if (!allow &&
                    Items is { } items && items.Count == 1 &&
                    DataContext is Dock.Model.Core.IDock { CanCloseLastDockable: false })
                {
                    allow = true;
                }

                return allow;
            });
    }

    private void AttachToWindow()
    {
        if (!EnableWindowDrag)
        {
            return;
        }

        if (VisualRoot is Window window &&
            window is HostWindow hostWindow &&
            _grip is { } &&
            (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
        {
            hostWindow.AttachGrip(_grip, ":documentwindow");
            _attachedWindow = hostWindow;
        }

        _windowDragHelper = CreateDragHelper();
        _windowDragHelper.Attach();
    }

    private void DetachFromWindow()
    {
        if (_attachedWindow is { } && _grip is { })
        {
            _attachedWindow.DetachGrip(_grip, ":documentwindow");
            _attachedWindow = null;
        }

        if (_windowDragHelper != null)
        {
            _windowDragHelper.Detach();
            _windowDragHelper = null;
        }
    }

    private void PointerPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is DocumentTabStripItem item && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _dragItem = item;
            _dragStart = e.GetPosition(this);
            _dragIndex = ItemContainerGenerator.IndexFromContainer(item);
            _isDragging = true;
            e.Pointer.Capture(this);
        }
    }

    private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            if (_dragItem is { } item && DataContext is Dock.Model.Controls.IDocumentDock dock && item.DataContext is Dock.Model.Core.IDockable dockable)
            {
                var pos = e.GetPosition(this);
                var rect = Bounds.Inflate(20);
                if (dock.RemoveTabOnDragOut && !rect.Contains(pos))
                {
                    dock.Factory?.RemoveDockable(dockable, true);
                }
            }

            _dragItem?.ClearValue(RenderTransformProperty);
            e.Pointer.Capture(null);
            _isDragging = false;
            _dragItem = null;
        }
    }

    private void PointerMovedHandler(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _dragItem is null)
            return;

        var pos = e.GetPosition(this);
        var delta = pos - _dragStart;
        if (Orientation == Orientation.Horizontal)
        {
            _dragItem.RenderTransform = new TranslateTransform(delta.X, 0);
        }
        else
        {
            _dragItem.RenderTransform = new TranslateTransform(0, delta.Y);
        }

        if (DataContext is not Dock.Model.Controls.IDocumentDock dock || dock.VisibleDockables is null)
            return;

        if (_dragItem.DataContext is not Dock.Model.Core.IDockable dragDockable)
            return;

        dragDockable.SetPointerPosition(pos.X, pos.Y);

        for (var i = 0; i < dock.VisibleDockables.Count; i++)
        {
            if (i == _dragIndex)
                continue;

            if (ItemContainerGenerator.ContainerFromIndex(i) is DocumentTabStripItem other)
            {
                var bounds = other.Bounds;
                var center = Orientation == Orientation.Horizontal ? bounds.Center.X : bounds.Center.Y;
                var pointer = Orientation == Orientation.Horizontal ? pos.X : pos.Y;

                if ((_dragIndex < i && pointer > center) || (_dragIndex > i && pointer < center))
                {
                    dragDockable.OnTabIndexChanging(i);
                    dock.Factory?.MoveDockable(dock, dragDockable, other.DataContext as Dock.Model.Core.IDockable);
                    dragDockable.TabIndex = i;
                    dragDockable.OnTabIndexChanged(_dragIndex, i);
                    _dragIndex = i;
                    break;
                }
            }
        }
    }
}
