// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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
    /// Initialize the new instance of the <see cref="ToolChromeControl"/>.
    /// </summary>
    public ToolChromeControl()
    {
        UpdatePseudoClasses();
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

        Grip = e.NameScope.Find<Control>("PART_Grip");
        CloseButton = e.NameScope.Find<Button>("PART_CloseButton");
        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);

        AttachToWindow();

        var maximizeRestoreButton = e.NameScope.Get<Button>("PART_MaximizeRestoreButton");
        maximizeRestoreButton.Click += OnMaximizeRestoreButtonClicked;
    }

    private WindowDragHelper CreateDragHelper(Control grip)
    {
        return new WindowDragHelper(
            grip,
            () => true,
            source =>
            {
                if (source is null)
                    return false;

                return !(source is Button) &&
                       !WindowDragHelper.IsChildOfType<Button>(grip, source);
            });
    }

    private void AttachToWindow()
    {
        if (Grip == null)
        {
            return;
        }

        // On linux we dont attach to the HostWindow because of inconsistent drag behaviour
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
                    _windowDragHelper = CreateDragHelper(Grip);
                    _windowDragHelper.Attach();
                }
            }
            else
            {
                _windowDragHelper = CreateDragHelper(Grip);
                _windowDragHelper.Attach();
            }
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
