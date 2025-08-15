// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model.Core;
using System;
using Avalonia.Reactive;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Chrome for MDI document window: header with title and close button.
/// Also activates and brings the document to front on click.
/// </summary>
[PseudoClasses(":active", ":maximized")]
[TemplatePart("PART_CloseButton", typeof(Button))]
[TemplatePart("PART_MaximizeRestoreButton", typeof(Button))]
[TemplatePart("PART_MinimizeButton", typeof(Button))]
public class DocumentChromeControl : ContentControl
{
    private static int s_nextZIndex;
    private Button? _closeButton;
    private Button? _maximizeRestoreButton;
    private Button? _minimizeButton;
    private MdiDocumentItem? _mdiItem;
    private IDisposable? _maximizedSubscription;

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<DocumentChromeControl, bool>(nameof(IsActive));

    /// <summary>
    /// Define the <see cref="DocumentFlyout"/> property.
    /// </summary>
    public static readonly StyledProperty<FlyoutBase?> DocumentFlyoutProperty =
        AvaloniaProperty.Register<DocumentChromeControl, FlyoutBase?>(nameof(DocumentFlyout));

    /// <summary>
    /// Define the <see cref="CloseButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> CloseButtonThemeProperty =
        AvaloniaProperty.Register<DocumentChromeControl, ControlTheme?>(nameof(CloseButtonTheme));

    /// <summary>
    /// Define the <see cref="MaximizeButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> MaximizeButtonThemeProperty =
        AvaloniaProperty.Register<DocumentChromeControl, ControlTheme?>(nameof(MaximizeButtonTheme));

    /// <summary>
    /// Define the <see cref="MinimizeButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> MinimizeButtonThemeProperty =
        AvaloniaProperty.Register<DocumentChromeControl, ControlTheme?>(nameof(MinimizeButtonTheme));

    /// <summary>
    /// Gets or sets if this is the currently active document.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets the document context flyout.
    /// </summary>
    public FlyoutBase? DocumentFlyout
    {
        get => GetValue(DocumentFlyoutProperty);
        set => SetValue(DocumentFlyoutProperty, value);
    }

    public ControlTheme? CloseButtonTheme
    {
        get => GetValue(CloseButtonThemeProperty);
        set => SetValue(CloseButtonThemeProperty, value);
    }

    public ControlTheme? MaximizeButtonTheme
    {
        get => GetValue(MaximizeButtonThemeProperty);
        set => SetValue(MaximizeButtonThemeProperty, value);
    }

    public ControlTheme? MinimizeButtonTheme
    {
        get => GetValue(MinimizeButtonThemeProperty);
        set => SetValue(MinimizeButtonThemeProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentChromeControl"/> class.
    /// </summary>
    public DocumentChromeControl()
    {
        UpdatePseudoClasses();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);

        // Track ancestor MDI item to reflect its maximized state via pseudo-class
        _mdiItem = this.FindAncestorOfType<MdiDocumentItem>();
        if (_mdiItem is not null)
        {
            PseudoClasses.Set(":maximized", _mdiItem.IsMaximized);
            _maximizedSubscription = _mdiItem
                .GetObservable(MdiDocumentItem.IsMaximizedProperty)
                .Subscribe(new AnonymousObserver<bool>(v => PseudoClasses.Set(":maximized", v)));
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _maximizeRestoreButton = e.NameScope.Find<Button>("PART_MaximizeRestoreButton");
        _minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton");

        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseClicked;
        }
        if (_maximizeRestoreButton is not null)
        {
            _maximizeRestoreButton.Click += OnMaximizeRestoreClicked;
        }
        if (_minimizeButton is not null)
        {
            _minimizeButton.Click += OnMinimizeClicked;
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _maximizedSubscription?.Dispose();
        _maximizedSubscription = null;
        _mdiItem = null;
        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseClicked;
        }
        if (_maximizeRestoreButton is not null)
        {
            _maximizeRestoreButton.Click -= OnMaximizeRestoreClicked;
        }
        if (_minimizeButton is not null)
        {
            _minimizeButton.Click -= OnMinimizeClicked;
        }
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            UpdatePseudoClasses();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":active", IsActive);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Double-click toggles maximize/restore when clicking on chrome background (not on buttons)
        if (e.ClickCount >= 2)
        {
            if (e.Source is Visual visual && visual.FindAncestorOfType<Button>() is null)
            {
                var mdi = _mdiItem ?? this.FindAncestorOfType<MdiDocumentItem>();
                if (mdi is not null)
                {
                    mdi.IsMaximized = !mdi.IsMaximized;
                    e.Handled = true;
                    return;
                }
            }
        }

        if (DataContext is IDockable dockable && dockable.Owner is IDock owner && owner.Factory is { } factory)
        {
            factory.SetActiveDockable(dockable);
            factory.SetFocusedDockable(owner, dockable);
        }

        // Bring the entire MDI window (item container) to front
        Control? container = this.Parent as Control;
        while (container is not null && container is not MdiDocumentItem)
        {
            container = container.Parent as Control;
        }
        if (container is not null)
        {
            container.ZIndex = ++s_nextZIndex;
        }

        // Do NOT mark handled here; allow underlying controls (e.g., buttons) to receive their clicks
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is IDockable dockable && dockable.Owner is IDock { Factory: { } factory })
        {
            factory.CloseDockable(dockable);
        }
        e.Handled = true;
    }

    private void OnMaximizeRestoreClicked(object? sender, RoutedEventArgs e)
    {
        MdiDocumentItem? mdiItem = null;
        Control? current = this.Parent as Control;
        while (current is not null)
        {
            if (current is MdiDocumentItem found)
            {
                mdiItem = found;
                break;
            }
            current = current.Parent as Control;
        }
        if (mdiItem is not null)
        {
            mdiItem.IsMaximized = !mdiItem.IsMaximized;
        }
        e.Handled = true;
    }

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        MdiDocumentItem? mdiItem = null;
        Control? current = this.Parent as Control;
        while (current is not null)
        {
            if (current is MdiDocumentItem found)
            {
                mdiItem = found;
                break;
            }
            current = current.Parent as Control;
        }
        if (mdiItem is not null)
        {
            mdiItem.Minimize();
        }
        e.Handled = true;
    }
}


