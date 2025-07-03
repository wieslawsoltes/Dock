// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStrip custom control.
/// </summary>
[PseudoClasses(":create", ":active")]
public class DocumentTabStrip : TabStrip
{
    // Minimum distance needed to initiate a drag operation
    private const double MinimumDragDistance = 3.0;
        
    private Point _dragStartPoint;
    private bool _pointerPressed;
    private bool _isDragging;
    private PointerPressedEventArgs _lastPointerPressedArgs;
    
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
    
    // Add a property to control whether window dragging is enabled
    public static readonly StyledProperty<bool> EnableWindowDragProperty = 
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(EnableWindowDrag));

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
    
    public bool EnableWindowDrag
    {
        get => GetValue(EnableWindowDragProperty);
        set => SetValue(EnableWindowDragProperty, value);
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
        
        // Subscribe to pointer events for window dragging
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
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
    }

    private void UpdatePseudoClassesCreate(bool canCreate)
    {
        PseudoClasses.Set(":create", canCreate);
    }

    private void UpdatePseudoClassesActive(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }
    
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!EnableWindowDrag)
                return;

            _lastPointerPressedArgs = e;
                
            // Only handle primary button clicks
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                // Check if we're clicking on an empty area of the tab strip
                // (not on a tab item or button)
                var source = e.Source as Control;
                if (source == this || (source != null && 
                                     !(source is DocumentTabStripItem) && 
                                     !(source is Button) &&
                                     !IsChildOfType<DocumentTabStripItem>(source) &&
                                     !IsChildOfType<Button>(source)))
                {
                    _dragStartPoint = e.GetPosition(this);
                    _pointerPressed = true;
                    e.Handled = true;
                }
            }
        }
        
        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_pointerPressed || _isDragging)
            {
                _pointerPressed = false;
                _isDragging = false;
                e.Handled = true;
            }
        }
        
        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_pointerPressed || _isDragging)
                return;
                
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _dragStartPoint;
            
            // Check if we've moved enough to consider it a drag
            if (Math.Abs(delta.X) > MinimumDragDistance || Math.Abs(delta.Y) > MinimumDragDistance)
            {
                _isDragging = true;
                _pointerPressed = false;
                
                // Find the window that contains this tab strip
                if (this.VisualRoot is Window hostWindow)
                {
                    // Call the MoveDrag method to start window dragging
                    hostWindow.BeginMoveDrag(_lastPointerPressedArgs);
                    e.Handled = true;
                }
            }
        }
        
        private bool IsChildOfType<T>(Control control) where T : Control
        {
            // Walk up the visual tree to find a parent of type T
            var parent = control;
            while (parent != null && parent != this)
            {
                if (parent is T)
                    return true;
                    
                parent = parent.Parent as Control;
            }
            return false;
        }
}
