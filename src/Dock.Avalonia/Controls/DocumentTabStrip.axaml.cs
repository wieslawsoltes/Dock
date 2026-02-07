// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Dock.Avalonia.Internal;
using Avalonia.Layout;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStrip custom control.
/// </summary>
[PseudoClasses(":create", ":active")]
public class DocumentTabStrip : TabStrip
{
    private HostWindow? _attachedWindow;
    private Control? _grip;
    private ScrollViewer? _scrollViewer;
    private IDisposable? _scrollViewerWheelSubscription;
    private WindowDragHelper? _windowDragHelper;
    
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
    /// Defines the <see cref="MouseWheelScrollOrientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> MouseWheelScrollOrientationProperty =
        AvaloniaProperty.Register<DocumentTabStrip, Orientation>(
            nameof(MouseWheelScrollOrientation),
            defaultValue: Orientation.Horizontal);

    /// <summary>
    /// Define the <see cref="CreateButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> CreateButtonThemeProperty =
        AvaloniaProperty.Register<DocumentTabStrip, ControlTheme?>(nameof(CreateButtonTheme));

    /// <summary>
    /// Gets or sets the create button theme.
    /// </summary>
    public ControlTheme? CreateButtonTheme
    {
        get => GetValue(CreateButtonThemeProperty);
        set => SetValue(CreateButtonThemeProperty, value);
    }

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
    /// Gets or sets orientation used for mouse wheel scrolling in the tab strip.
    /// </summary>
    public Orientation MouseWheelScrollOrientation
    {
        get => GetValue(MouseWheelScrollOrientationProperty);
        set => SetValue(MouseWheelScrollOrientationProperty, value);
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

        DetachScrollViewerWheel();

        _grip = e.NameScope.Find<Control>("PART_BorderFill");
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        AttachScrollViewerWheel();

        AttachToWindow();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AttachScrollViewerWheel();
        AttachToWindow();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachScrollViewerWheel();
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

        if (change.Property == MouseWheelScrollOrientationProperty)
        {
            AttachScrollViewerWheel();
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

                // Preserve original behavior when source is null by evaluating override based on layout state.
                var s = source;

                var isButtonRelated = s is not null && WindowDragHelper.IsChildOfType<Button>(this, s);
                var isTabItemRelated = s is DocumentTabStripItem || (s is not null && WindowDragHelper.IsChildOfType<DocumentTabStripItem>(this, s));

                // Base rule: allow drag when not interacting with a tab item or any button (including descendants).
                var baseAllow = s is not null && !isTabItemRelated && !isButtonRelated;

                // Override: if there is only one item and the last dockable cannot be closed, allow drag
                // BUT do not allow this override when the interaction is on/inside a Button.
                var overrideAllow =
                    !baseAllow &&
                    !isButtonRelated &&
                    Items is { } items && items.Count == 1 &&
                    DataContext is Dock.Model.Core.IDock { CanCloseLastDockable: false };

                return baseAllow || overrideAllow;
            });
    }

    private void AttachToWindow()
    {
        if (!EnableWindowDrag)
        {
            return;
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

    private void DetachScrollViewerWheel()
    {
        _scrollViewerWheelSubscription?.Dispose();
        _scrollViewerWheelSubscription = null;
    }

    private void AttachScrollViewerWheel()
    {
        _scrollViewerWheelSubscription?.Dispose();
        _scrollViewerWheelSubscription = ScrollViewerMouseWheelHookHelper.Attach(_scrollViewer, MouseWheelScrollOrientation);
    }
}
