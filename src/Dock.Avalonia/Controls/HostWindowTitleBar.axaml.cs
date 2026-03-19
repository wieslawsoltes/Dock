// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dock.Avalonia.Automation.Peers;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="HostWindowTitleBar"/> xaml.
/// </summary>
[PseudoClasses(":fullscreen", ":maximized")]
[TemplatePart("PART_Background", typeof(Control))]
[TemplatePart("PART_Container", typeof(Control))]
[TemplatePart("PART_MouseTracker", typeof(Control))]
[TemplatePart("PART_CaptionButtons", typeof(Panel))]
[TemplatePart("PART_CloseButton", typeof(Button))]
[TemplatePart("PART_MinimizeButton", typeof(Button))]
[TemplatePart("PART_MaximizeRestoreButton", typeof(Button))]
public class HostWindowTitleBar : TemplatedControl
{
    private HostWindow? _hostWindow;
    private Control? _dragSurface;
    private Control? _mouseTracker;
    private Panel? _captionButtons;
    private Button? _closeButton;
    private Button? _minimizeButton;
    private Button? _maximizeRestoreButton;

    internal Control? BackgroundControl { get; private set; }
    internal Control? DragSurface => _dragSurface;
    internal Control? MouseTracker => _mouseTracker;
    internal HostWindow? HostWindow => _hostWindow;
    internal Button? CloseButton => _closeButton;
    internal Button? MinimizeButton => _minimizeButton;
    internal Button? MaximizeRestoreButton => _maximizeRestoreButton;

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(HostWindowTitleBar);

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new HostWindowTitleBarAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachButtonHandlers();

        BackgroundControl = e.NameScope.Find<Control>("PART_Background");
        _dragSurface = e.NameScope.Find<Control>("PART_Container");
        _mouseTracker = e.NameScope.Find<Control>("PART_MouseTracker");
        _captionButtons = e.NameScope.Find<Panel>("PART_CaptionButtons");
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton");
        _maximizeRestoreButton = e.NameScope.Find<Button>("PART_MaximizeRestoreButton");

        ArrangeCaptionButtons();
        AttachButtonHandlers();
        AttachToHostWindow(TopLevel.GetTopLevel(this) as HostWindow);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachToHostWindow(TopLevel.GetTopLevel(this) as HostWindow);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachFromHostWindow();
        DetachButtonHandlers();
    }

    private void AttachToHostWindow(HostWindow? hostWindow)
    {
        if (ReferenceEquals(_hostWindow, hostWindow))
        {
            UpdateWindowState();
            UpdateCaptionButtons();
            return;
        }

        DetachFromHostWindow();

        _hostWindow = hostWindow;

        if (_hostWindow is not null)
        {
            _hostWindow.PropertyChanged += HostWindowOnPropertyChanged;
        }

        UpdateWindowState();
        UpdateCaptionButtons();
    }

    private void DetachFromHostWindow()
    {
        if (_hostWindow is not null)
        {
            _hostWindow.PropertyChanged -= HostWindowOnPropertyChanged;
            _hostWindow = null;
        }

        UpdateWindowState();
        UpdateCaptionButtons();
    }

    private void AttachButtonHandlers()
    {
        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseButtonClick;
        }

        if (_minimizeButton is not null)
        {
            _minimizeButton.Click += OnMinimizeButtonClick;
        }

        if (_maximizeRestoreButton is not null)
        {
            _maximizeRestoreButton.Click += OnMaximizeRestoreButtonClick;
        }
    }

    private void DetachButtonHandlers()
    {
        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseButtonClick;
        }

        if (_minimizeButton is not null)
        {
            _minimizeButton.Click -= OnMinimizeButtonClick;
        }

        if (_maximizeRestoreButton is not null)
        {
            _maximizeRestoreButton.Click -= OnMaximizeRestoreButtonClick;
        }
    }

    private void HostWindowOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == Window.WindowStateProperty)
        {
            UpdateWindowState();
        }
        else if (change.Property == Window.CanMinimizeProperty ||
                 change.Property == Window.CanMaximizeProperty ||
                 change.Property == Window.CanResizeProperty)
        {
            UpdateCaptionButtons();
        }
    }

    private void UpdateWindowState()
    {
        var state = _hostWindow?.WindowState ?? WindowState.Normal;
        PseudoClasses.Set(":fullscreen", state == WindowState.FullScreen);
        PseudoClasses.Set(":maximized", state == WindowState.Maximized);
    }

    private void UpdateCaptionButtons()
    {
        if (_minimizeButton is not null)
        {
            _minimizeButton.IsVisible = _hostWindow?.CanMinimize ?? true;
        }

        if (_maximizeRestoreButton is not null)
        {
            _maximizeRestoreButton.IsVisible =
                (_hostWindow?.CanResize ?? true) &&
                (_hostWindow?.CanMaximize ?? true);
        }
    }

    private void ArrangeCaptionButtons()
    {
        if (_captionButtons is null)
        {
            return;
        }

        _captionButtons.HorizontalAlignment = OperatingSystem.IsMacOS()
            ? global::Avalonia.Layout.HorizontalAlignment.Left
            : global::Avalonia.Layout.HorizontalAlignment.Right;

        if (_closeButton is null || _minimizeButton is null || _maximizeRestoreButton is null)
        {
            return;
        }

        _captionButtons.Children.Clear();

        if (OperatingSystem.IsMacOS())
        {
            _captionButtons.Children.Add(_closeButton);
            _captionButtons.Children.Add(_minimizeButton);
            _captionButtons.Children.Add(_maximizeRestoreButton);
            return;
        }

        _captionButtons.Children.Add(_minimizeButton);
        _captionButtons.Children.Add(_maximizeRestoreButton);
        _captionButtons.Children.Add(_closeButton);
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows() || _hostWindow is null)
        {
            return;
        }

        _hostWindow.Exit();
        e.Handled = true;
    }

    private void OnMinimizeButtonClick(object? sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows() || _hostWindow is not { CanMinimize: true })
        {
            return;
        }

        _hostWindow.WindowState = WindowState.Minimized;
        e.Handled = true;
    }

    private void OnMaximizeRestoreButtonClick(object? sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows() || _hostWindow is not { CanResize: true, CanMaximize: true })
        {
            return;
        }

        _hostWindow.WindowState = _hostWindow.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        e.Handled = true;
    }
}
