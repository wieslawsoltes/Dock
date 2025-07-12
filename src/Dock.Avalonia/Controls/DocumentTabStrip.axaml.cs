﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

using System.Runtime.InteropServices;
using Dock.Avalonia.Internal;
using Avalonia.Layout;

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
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(EnableWindowDrag), global::Dock.Settings.DockSettings.EnableWindowDrag);

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

    private WindowDragHelper CreateDragHelper(Control grip)
    {
        return new WindowDragHelper(
            grip,
            () => EnableWindowDrag,
            source =>
            {
                if (source == this)
                    return true;

                return source is { } s &&
                       !(s is DocumentTabStripItem) &&
                       !(s is Button) &&
                       !WindowDragHelper.IsChildOfType<DocumentTabStripItem>(this, s) &&
                       !WindowDragHelper.IsChildOfType<Button>(this, s);
            });
    }

    private void AttachToWindow()
    {
        if (!EnableWindowDrag || _grip == null)
        {
            return;
        }

        if (VisualRoot is Window window)
        {
            if (window is HostWindow hostWindow)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    hostWindow.AttachGrip(_grip, ":documentwindow");
                    _attachedWindow = hostWindow;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _windowDragHelper = CreateDragHelper(_grip);
                    _windowDragHelper.Attach();
                }
            }
            else
            {
                _windowDragHelper = CreateDragHelper(_grip);
                _windowDragHelper.Attach();
            }
        }
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
}
