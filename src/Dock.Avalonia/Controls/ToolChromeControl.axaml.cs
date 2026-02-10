// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dock.Avalonia.Automation.Peers;
using Dock.Model.Core;
using Dock.Avalonia.Internal;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Dock tool chrome content control.
/// </summary>
[PseudoClasses(":floating", ":active", ":pinned", ":maximized")]
[TemplatePart("PART_Grip", typeof(Control))]
[TemplatePart("PART_CloseButton", typeof(Button))]
[TemplatePart("PART_MaximizeRestoreButton", typeof(Button))]
public class ToolChromeControl : ContentControl
{
    private HostWindow? _attachedWindow;
    private WindowDragHelper? _windowDragHelper;
    private Button? _maximizeRestoreButton;

    /// <summary>
    /// Define <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProprty =
        AvaloniaProperty.Register<ToolChromeControl, string>(nameof(Title));

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ToolChromeControl, bool>(nameof(IsActive));

    /// <summary>
    /// Define the <see cref="IsPinned"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPinnedProperty =
        AvaloniaProperty.Register<ToolChromeControl, bool>(nameof(IsPinned));

    /// <summary>
    /// Define the <see cref="IsFloating"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFloatingProperty =
        AvaloniaProperty.Register<ToolChromeControl, bool>(nameof(IsFloating));

    /// <summary>
    /// Define the <see cref="IsMaximized"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaximizedProperty =
        AvaloniaProperty.Register<ToolChromeControl, bool>(nameof(IsMaximized));

    /// <summary>
    /// Define the <see cref="ToolFlyout"/> property.
    /// </summary>
    public static readonly StyledProperty<FlyoutBase?> ToolFlyoutProperty =
        AvaloniaProperty.Register<ToolChromeControl, FlyoutBase?>(nameof(ToolFlyout));

    /// <summary>
    /// Define the <see cref="CloseButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> CloseButtonThemeProperty =
        AvaloniaProperty.Register<ToolChromeControl, ControlTheme?>(nameof(CloseButtonTheme));

    /// <summary>
    /// Define the <see cref="MaximizeButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> MaximizeButtonThemeProperty =
        AvaloniaProperty.Register<ToolChromeControl, ControlTheme?>(nameof(MaximizeButtonTheme));

    /// <summary>
    /// Define the <see cref="PinButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> PinButtonThemeProperty =
        AvaloniaProperty.Register<ToolChromeControl, ControlTheme?>(nameof(PinButtonTheme));

    /// <summary>
    /// Define the <see cref="MenuButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> MenuButtonThemeProperty =
        AvaloniaProperty.Register<ToolChromeControl, ControlTheme?>(nameof(MenuButtonTheme));

    /// <summary>
    /// Gets or sets is pinned
    /// </summary>
    public bool IsPinned
    {
        get => GetValue(IsPinnedProperty);
        set => SetValue(IsPinnedProperty, value);
    }

    /// <summary>
    /// Gets or sets is floating
    /// </summary>
    public bool IsFloating
    {
        get => GetValue(IsFloatingProperty);
        set => SetValue(IsFloatingProperty, value);
    }

    /// <summary>
    /// Gets or sets is maximized
    /// </summary>
    public bool IsMaximized
    {
        get => GetValue(IsMaximizedProperty);
        set => SetValue(IsMaximizedProperty, value);
    }

    /// <summary>
    /// Gets or sets the tool flyout.
    /// </summary>
    public FlyoutBase? ToolFlyout
    {
        get => GetValue(ToolFlyoutProperty);
        set => SetValue(ToolFlyoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the close button theme.
    /// </summary>
    public ControlTheme? CloseButtonTheme
    {
        get => GetValue(CloseButtonThemeProperty);
        set => SetValue(CloseButtonThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximize button theme.
    /// </summary>
    public ControlTheme? MaximizeButtonTheme
    {
        get => GetValue(MaximizeButtonThemeProperty);
        set => SetValue(MaximizeButtonThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the pin button theme.
    /// </summary>
    public ControlTheme? PinButtonTheme
    {
        get => GetValue(PinButtonThemeProperty);
        set => SetValue(PinButtonThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the menu button theme.
    /// </summary>
    public ControlTheme? MenuButtonTheme
    {
        get => GetValue(MenuButtonThemeProperty);
        set => SetValue(MenuButtonThemeProperty, value);
    }

    /// <summary>
    /// Initialize the new instance of the <see cref="ToolChromeControl"/>.
    /// </summary>
    public ToolChromeControl()
    {
        UpdatePseudoClasses();
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ToolChromeControlAutomationPeer(this);
    }

    /// <summary>
    /// Gets or sets chrome tool title.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProprty);
        set => SetValue(TitleProprty, value);
    }

    /// <summary>
    /// Gets or sets if this is the currently active Tool.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    internal Control? Grip { get; private set; }

    internal Button? CloseButton { get; private set; }

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
        RemoveHandler(PointerPressedEvent, PressedHandler);

        if (_maximizeRestoreButton is not null)
        {
            _maximizeRestoreButton.Click -= OnMaximizeRestoreButtonClicked;
            _maximizeRestoreButton = null;
        }

        DetachFromWindow();
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is IDock {Factory: { } factory} dock && dock.ActiveDockable is { })
        {
            if (factory.FindRoot(dock.ActiveDockable, _ => true) is { } root)
            {
                factory.SetFocusedDockable(root, dock.ActiveDockable);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachFromWindow();
        RemoveHandler(PointerPressedEvent, PressedHandler);

        if (_maximizeRestoreButton is not null)
        {
            _maximizeRestoreButton.Click -= OnMaximizeRestoreButtonClicked;
            _maximizeRestoreButton = null;
        }

        Grip = e.NameScope.Find<Control>("PART_Grip");
        CloseButton = e.NameScope.Find<Button>("PART_CloseButton");
        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);

        AttachToWindow();

        _maximizeRestoreButton = e.NameScope.Find<Button>("PART_MaximizeRestoreButton");
        if (_maximizeRestoreButton is not null)
        {
            _maximizeRestoreButton.Click += OnMaximizeRestoreButtonClicked;
        }
    }

    private static bool IsFocusableChild(Control owner, Control source)
    {
        var current = source;
        while (current != null && current != owner)
        {
            if (current.Focusable)
            {
                return true;
            }

            current = current.Parent as Control;
        }

        return false;
    }

    private WindowDragHelper CreateDragHelper(Control owner, bool ignoreFocusable = false, bool handlePointerPressed = true)
    {
        return new WindowDragHelper(
            owner,
            () => true,
            source =>
            {
                if (source is null)
                    return false;

                if (ignoreFocusable && IsFocusableChild(owner, source))
                {
                    return false;
                }

                return !(source is Button) &&
                       !WindowDragHelper.IsChildOfType<Button>(owner, source);
            },
            handlePointerPressed);
    }

    private void AttachToWindow()
    {
        if (Grip == null)
        {
            return;
        }

        // On linux we use WindowDragHelper because of inconsistent drag behaviour with BeginMoveDrag.
        if (VisualRoot is Window window)
        {
            if (window is HostWindow hostWindow)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    hostWindow.AttachGrip(this);
                    _attachedWindow = hostWindow;

                    SetCurrentValue(IsFloatingProperty, true);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _windowDragHelper = CreateDragHelper(hostWindow, ignoreFocusable: true, handlePointerPressed: false);
                    _windowDragHelper.Attach();
                }
            }
#if false
            else
            {
                _windowDragHelper = CreateDragHelper(Grip);
                _windowDragHelper.Attach();
            }
#endif
        }
    }

    private void DetachFromWindow()
    {
        if (_attachedWindow != null)
        {
            _attachedWindow.DetachGrip(this);
            _attachedWindow = null;
        }

        if (_windowDragHelper != null)
        {
            _windowDragHelper.Detach();
            _windowDragHelper = null;
        }
    }

    private void OnMaximizeRestoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is HostWindow window)
        {
            if (window.WindowState == WindowState.Maximized)
                window.WindowState = WindowState.Normal;
            else
                window.WindowState = WindowState.Maximized;
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty ||
            change.Property == IsPinnedProperty ||
            change.Property == IsFloatingProperty ||
            change.Property == IsMaximizedProperty)
        {
            UpdatePseudoClasses();
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":active", IsActive);
        PseudoClasses.Set(":pinned", IsPinned);
        PseudoClasses.Set(":floating", IsFloating);
        PseudoClasses.Set(":maximized", IsMaximized);
    }
}
