// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Dock.Avalonia.Internal;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Reactive;
using Avalonia.Styling;
using Dock.Avalonia.Automation.Peers;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStrip custom control.
/// </summary>
[PseudoClasses(":create", ":active")]
public class DocumentTabStrip : TabStrip
{
    private HostWindow? _attachedWindow;
    private Control? _grip;
    private Control? _panel;
    private Control? _leadingSpacer;
    private Control? _createButtonHost;
    private ScrollViewer? _scrollViewer;
    private readonly List<IDisposable> _templateObservables = new();
    private IDisposable? _scrollViewerWheelSubscription;
    private IDisposable? _doubleTappedSubscription;
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
    /// Define the <see cref="EnableWindowStateToggleOnDoubleTap"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableWindowStateToggleOnDoubleTapProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(EnableWindowStateToggleOnDoubleTap));

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
    /// Defines the <see cref="IconTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, object?>(nameof(IconTemplate));

    /// <summary>
    /// Defines the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Defines the <see cref="ModifiedTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ModifiedTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, IDataTemplate?>(nameof(ModifiedTemplate));

    /// <summary>
    /// Defines the <see cref="CloseTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CloseTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, IDataTemplate?>(nameof(CloseTemplate));

    /// <summary>
    /// Defines the <see cref="CreateButtonTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CreateButtonTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, IDataTemplate?>(nameof(CreateButtonTemplate));

    /// <summary>
    /// Defines the <see cref="LeftContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftContentProperty =
        AvaloniaProperty.Register<DocumentTabStrip, object?>(nameof(LeftContent));

    /// <summary>
    /// Defines the <see cref="TopContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> TopContentProperty =
        AvaloniaProperty.Register<DocumentTabStrip, object?>(nameof(TopContent));

    /// <summary>
    /// Defines the <see cref="RightContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<DocumentTabStrip, object?>(nameof(RightContent));

    /// <summary>
    /// Defines the <see cref="BottomContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> BottomContentProperty =
        AvaloniaProperty.Register<DocumentTabStrip, object?>(nameof(BottomContent));

    /// <summary>
    /// Defines the <see cref="LeftContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftContentTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, IDataTemplate?>(nameof(LeftContentTemplate));

    /// <summary>
    /// Defines the <see cref="TopContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TopContentTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, IDataTemplate?>(nameof(TopContentTemplate));

    /// <summary>
    /// Defines the <see cref="RightContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightContentTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, IDataTemplate?>(nameof(RightContentTemplate));

    /// <summary>
    /// Defines the <see cref="BottomContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> BottomContentTemplateProperty =
        AvaloniaProperty.Register<DocumentTabStrip, IDataTemplate?>(nameof(BottomContentTemplate));

    /// <summary>
    /// Gets or sets the create button theme.
    /// </summary>
    public ControlTheme? CreateButtonTheme
    {
        get => GetValue(CreateButtonThemeProperty);
        set => SetValue(CreateButtonThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets tab icon template.
    /// </summary>
    public object? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab header template.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab modified template.
    /// </summary>
    public IDataTemplate? ModifiedTemplate
    {
        get => GetValue(ModifiedTemplateProperty);
        set => SetValue(ModifiedTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab close template.
    /// </summary>
    public IDataTemplate? CloseTemplate
    {
        get => GetValue(CloseTemplateProperty);
        set => SetValue(CloseTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the create button content template.
    /// </summary>
    public IDataTemplate? CreateButtonTemplate
    {
        get => GetValue(CreateButtonTemplateProperty);
        set => SetValue(CreateButtonTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets content displayed to the left side of the tab strip.
    /// </summary>
    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    /// <summary>
    /// Gets or sets content displayed above the tab strip.
    /// </summary>
    public object? TopContent
    {
        get => GetValue(TopContentProperty);
        set => SetValue(TopContentProperty, value);
    }

    /// <summary>
    /// Gets or sets content displayed to the right side of the tab strip.
    /// </summary>
    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    /// <summary>
    /// Gets or sets content displayed below the tab strip.
    /// </summary>
    public object? BottomContent
    {
        get => GetValue(BottomContentProperty);
        set => SetValue(BottomContentProperty, value);
    }

    /// <summary>
    /// Gets or sets template for <see cref="LeftContent"/>.
    /// </summary>
    public IDataTemplate? LeftContentTemplate
    {
        get => GetValue(LeftContentTemplateProperty);
        set => SetValue(LeftContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets template for <see cref="TopContent"/>.
    /// </summary>
    public IDataTemplate? TopContentTemplate
    {
        get => GetValue(TopContentTemplateProperty);
        set => SetValue(TopContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets template for <see cref="RightContent"/>.
    /// </summary>
    public IDataTemplate? RightContentTemplate
    {
        get => GetValue(RightContentTemplateProperty);
        set => SetValue(RightContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets template for <see cref="BottomContent"/>.
    /// </summary>
    public IDataTemplate? BottomContentTemplate
    {
        get => GetValue(BottomContentTemplateProperty);
        set => SetValue(BottomContentTemplateProperty, value);
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
    /// Gets or sets if the hosting window state can be toggled by double tapping the tab strip fill area.
    /// </summary>
    public bool EnableWindowStateToggleOnDoubleTap
    {
        get => GetValue(EnableWindowStateToggleOnDoubleTapProperty);
        set => SetValue(EnableWindowStateToggleOnDoubleTapProperty, value);
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
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new DocumentTabStripAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachScrollViewerWheel();
        ClearTemplateObservables();

        _grip = e.NameScope.Find<Control>("PART_BorderFill");
        _panel = e.NameScope.Find<Control>("PART_Panel");
        _leadingSpacer = e.NameScope.Find<Control>("PART_LeadingSpacer");
        _createButtonHost = e.NameScope.Find<Control>("PART_CreateButtonHost");
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        AttachDoubleTapped();
        AttachScrollViewerWheel();
        AttachTemplateObservables();

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
        DetachDoubleTapped();
        DetachScrollViewerWheel();
        ClearTemplateObservables();
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

        if (change.Property == EnableWindowStateToggleOnDoubleTapProperty)
        {
            AttachDoubleTapped();
        }

        if (change.Property == MouseWheelScrollOrientationProperty)
        {
            AttachScrollViewerWheel();
        }

    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (!EnableWindowStateToggleOnDoubleTap)
        {
            return;
        }

        if (ShouldIgnoreWindowStateToggleSource(e.Source))
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        window.WindowState = window.WindowState switch
        {
            WindowState.Normal => WindowState.Maximized,
            WindowState.Maximized => WindowState.Normal,
            _ => window.WindowState
        };

        e.Handled = true;
    }

    internal bool ShouldIgnoreWindowStateToggleSource(object? source)
    {
        if (source is not Control control)
        {
            return false;
        }

        return control is Button || WindowDragHelper.IsChildOfType<Button>(this, control);
    }

    private void AttachTemplateObservables()
    {
        if (_panel?.Parent is not DockPanel dockPanel)
        {
            return;
        }

        _templateObservables.Add(dockPanel.GetObservable(BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(_ => UpdateTabHostMaxWidth())));

        _templateObservables.Add(this.GetObservable(BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(_ => UpdateTabHostMaxWidth())));

        if (_leadingSpacer is not null)
        {
            _templateObservables.Add(_leadingSpacer.GetObservable(BoundsProperty)
                .Subscribe(new AnonymousObserver<Rect>(_ => UpdateTabHostMaxWidth())));
        }

        if (_createButtonHost is not null)
        {
            _templateObservables.Add(_createButtonHost.GetObservable(BoundsProperty)
                .Subscribe(new AnonymousObserver<Rect>(_ => UpdateTabHostMaxWidth())));

            _templateObservables.Add(_createButtonHost.GetObservable(IsVisibleProperty)
                .Subscribe(new AnonymousObserver<bool>(_ => UpdateTabHostMaxWidth())));
        }

        UpdateTabHostMaxWidth();
    }

    private void ClearTemplateObservables()
    {
        foreach (var disposable in _templateObservables)
        {
            disposable.Dispose();
        }

        _templateObservables.Clear();
    }

    private void UpdateTabHostMaxWidth()
    {
        if (_panel is null || _panel.Parent is not DockPanel dockPanel)
        {
            return;
        }

        var hostWidth = dockPanel.Bounds.Width;
        if (double.IsNaN(hostWidth) || double.IsInfinity(hostWidth) || hostWidth <= 0d)
        {
            _panel.SetCurrentValue(Layoutable.MaxWidthProperty, double.PositiveInfinity);
            return;
        }

        var reservedWidth = 0d;

        if (_leadingSpacer is { IsVisible: true } leadingSpacer)
        {
            var spacerWidth = Math.Max(leadingSpacer.Bounds.Width, leadingSpacer.DesiredSize.Width);
            reservedWidth += spacerWidth + leadingSpacer.Margin.Left + leadingSpacer.Margin.Right;
        }

        if (_createButtonHost is { IsVisible: true } createButtonHost)
        {
            var createHostWidth = Math.Max(createButtonHost.Bounds.Width, createButtonHost.DesiredSize.Width);
            reservedWidth += createHostWidth + createButtonHost.Margin.Left + createButtonHost.Margin.Right;
        }

        var maxWidth = Math.Max(0d, hostWidth - reservedWidth);
        _panel.SetCurrentValue(Layoutable.MaxWidthProperty, maxWidth);
    }

    private void AttachDoubleTapped()
    {
        DetachDoubleTapped();

        if (!EnableWindowStateToggleOnDoubleTap)
        {
            return;
        }

        _doubleTappedSubscription = this.AddDisposableHandler(Gestures.DoubleTappedEvent, OnDoubleTapped);
    }

    private void DetachDoubleTapped()
    {
        _doubleTappedSubscription?.Dispose();
        _doubleTappedSubscription = null;
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
            DetachFromWindow();
            return;
        }

        if (_windowDragHelper is not null)
        {
            _windowDragHelper.Detach();
            _windowDragHelper = null;
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
