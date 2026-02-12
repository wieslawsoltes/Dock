// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dock.Avalonia.Automation.Peers;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="HostWindow"/> xaml.
/// </summary>
[PseudoClasses(":toolwindow", ":dragging", ":toolchromecontrolswindow", ":documentchromecontrolswindow")]
[TemplatePart("PART_TitleBar", typeof(HostWindowTitleBar))]
public class HostWindow : Window, IHostWindow
{
    private readonly HostWindowState _hostWindowState;
    private List<Control> _chromeGrips = new();
    private HostWindowTitleBar? _hostWindowTitleBar;
    private bool _mouseDown, _draggingWindow;
    private double _normalX = double.NaN;
    private double _normalY = double.NaN;
    private double _normalWidth = double.NaN;
    private double _normalHeight = double.NaN;

    /// <summary>
    /// Define <see cref="IsToolWindow"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsToolWindowProperty =
        AvaloniaProperty.Register<HostWindow, bool>(nameof(IsToolWindow));

    /// <summary>
    /// Define <see cref="ToolChromeControlsWholeWindow"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ToolChromeControlsWholeWindowProperty =
        AvaloniaProperty.Register<HostWindow, bool>(nameof(ToolChromeControlsWholeWindow));

    /// <summary>
    /// Define <see cref="DocumentChromeControlsWholeWindowProperty"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> DocumentChromeControlsWholeWindowProperty =
        AvaloniaProperty.Register<HostWindow, bool>(nameof(DocumentChromeControlsWholeWindow));

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(HostWindow);

    /// <summary>
    /// Gets or sets if this is the tool window.
    /// </summary>
    public bool IsToolWindow
    {
        get => GetValue(IsToolWindowProperty);
        set => SetValue(IsToolWindowProperty, value);
    }

    /// <summary>
    /// Gets or sets if the tool chrome controls the whole window.
    /// </summary>
    public bool ToolChromeControlsWholeWindow
    {
        get => GetValue(ToolChromeControlsWholeWindowProperty);
        set => SetValue(ToolChromeControlsWholeWindowProperty, value);
    }

    /// <summary>
    /// Gets or sets if the document chrome controls the whole window.
    /// </summary>
    public bool DocumentChromeControlsWholeWindow
    {
        get => GetValue(DocumentChromeControlsWholeWindowProperty);
        set => SetValue(DocumentChromeControlsWholeWindowProperty, value);
    }

    /// <inheritdoc/>
    public IHostWindowState HostWindowState => _hostWindowState;

    /// <inheritdoc/>
    public bool IsTracked { get; set; }

    /// <inheritdoc/>
    public IDockWindow? Window { get; set; }

    /// <summary>
    /// Initializes new instance of the <see cref="HostWindow"/> class.
    /// </summary>
    public HostWindow()
        : this(new DockManagerOptions())
    {
    }

    /// <summary>
    /// Initializes new instance of the <see cref="HostWindow"/> class with shared dock manager options.
    /// </summary>
    /// <param name="options">Dock manager options to share across windows.</param>
    public HostWindow(DockManagerOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        PositionChanged += HostWindow_PositionChanged;
        LayoutUpdated += HostWindow_LayoutUpdated;
        Activated += HostWindow_Activated;
        Deactivated += HostWindow_Deactivated;
        _hostWindowState = new HostWindowState(new DockManager(new DockService(), options), this);
        UpdatePseudoClasses(IsToolWindow, ToolChromeControlsWholeWindow, DocumentChromeControlsWholeWindow);
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new HostWindowAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _hostWindowTitleBar = e.NameScope.Find<HostWindowTitleBar>("PART_TitleBar");
        if (_hostWindowTitleBar is { })
        {
            _hostWindowTitleBar.ApplyTemplate();

            if (_hostWindowTitleBar.BackgroundControl is { })
            {
                _hostWindowTitleBar.BackgroundControl.PointerPressed += (_, args) =>
                {
                    MoveDrag(args);
                };
            }
        }
    }

    private PixelPoint ClientPointToScreenRelativeToWindow(Point clientPoint)
    {
        var absScreenPoint = this.PointToScreen(clientPoint);
        var absScreenWindowPoint = this.PointToScreen(new Point(0, 0));
        var relativeScreenDiff = absScreenPoint - absScreenWindowPoint;
        return relativeScreenDiff;
    }

    private void MoveDrag(PointerPressedEventArgs e)
    {
        if (!ToolChromeControlsWholeWindow)
            return;

        if (Window?.Factory?.OnWindowMoveDragBegin(Window) != true)
        {
            return;
        }

        if (DockSettings.BringWindowsToFrontOnDrag && Window?.Factory is { } factory)
        {
            WindowActivationHelper.ActivateAllWindows(factory, this);
        }

        _mouseDown = true;
        _hostWindowState.Process(ClientPointToScreenRelativeToWindow(e.GetPosition(this)), EventType.Pressed);

        PseudoClasses.Set(":dragging", true);
        _draggingWindow = true;
        BeginMoveDrag(e);
    }

    private void EndDrag(PointerEventArgs e)
    {
        PseudoClasses.Set(":dragging", false);

        Window?.Factory?.OnWindowMoveDragEnd(Window);
        _hostWindowState.Process(ClientPointToScreenRelativeToWindow(e.GetPosition(this)), EventType.Released);
        _mouseDown = false;
        _draggingWindow = false;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.Handled)
        {
            return;
        }

        if (_chromeGrips.Any(grip => grip.IsPointerOver))
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                MoveDrag(e);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.Handled)
        {
            return;
        }

        if (_draggingWindow)
        {
            EndDrag(e);
        }
    }

    private void HostWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (Window is { } && IsTracked)
        {
            CaptureNormalBounds();
            Window.Save();

            if (_mouseDown)
            {
                Window.Factory?.OnWindowMoveDrag(Window);
                _hostWindowState.Process(Position, EventType.Moved);

                if (DockSettings.EnableWindowMagnetism && Window?.Factory is { HostWindows: var windows })
                {
                    var snap = DockSettings.WindowMagnetDistance;
                    var x = Position.X;
                    var y = Position.Y;
                    var rect = new Rect(Position.X, Position.Y, Width, Height);

                    foreach (var host in windows.OfType<HostWindow>())
                    {
                        if (host == this || !host.IsVisible)
                            continue;

                        var other = new Rect(host.Position.X, host.Position.Y, host.Width, host.Height);
                        var verticalOverlap = rect.Top < other.Bottom && rect.Bottom > other.Top;
                        var horizontalOverlap = rect.Left < other.Right && rect.Right > other.Left;

                        if (verticalOverlap)
                        {
                            if (Math.Abs(rect.Left - other.Right) <= snap)
                                x = (int)other.Right;
                            else if (Math.Abs(rect.Right - other.Left) <= snap)
                                x = (int)(other.Left - rect.Width);
                        }

                        if (horizontalOverlap)
                        {
                            if (Math.Abs(rect.Top - other.Bottom) <= snap)
                                y = (int)other.Bottom;
                            else if (Math.Abs(rect.Bottom - other.Top) <= snap)
                                y = (int)(other.Top - rect.Height);
                        }
                    }

                    if (x != Position.X || y != Position.Y)
                    {
                        Position = new PixelPoint(x, y);
                    }
                }
            }
        }
    }

    private void HostWindow_LayoutUpdated(object? sender, EventArgs e)
    {
        if (Window is { } && IsTracked)
        {
            CaptureNormalBounds();
            Window.Save();
        }
    }

    private void HostWindow_Activated(object? sender, EventArgs e)
    {
        if (Window is { })
        {
            Window.Factory?.OnWindowActivated(Window);
            
            // Also activate the active dockable in this window
            if (Window.Layout?.ActiveDockable is { } activeDockable)
            {
                Window.Factory?.OnDockableActivated(activeDockable);
            }
        }
    }

    private void HostWindow_Deactivated(object? sender, EventArgs e)
    {
        if (Window is { })
        {
            Window.Factory?.OnWindowDeactivated(Window);
            
            // Also deactivate the active dockable in this window
            if (Window.Layout?.ActiveDockable is { } activeDockable)
            {
                Window.Factory?.OnDockableDeactivated(activeDockable);
            }
        }
    }

    /// <summary>
    /// Attaches grip to chrome.
    /// </summary>
    /// <param name="chromeControl">The chrome control.</param>
    public void AttachGrip(ToolChromeControl chromeControl)
    {
        if (chromeControl.CloseButton is not null)
        {
            chromeControl.CloseButton.Click += ChromeCloseClick;
        }

        if (chromeControl.Grip is { } grip)
        {
            _chromeGrips.Add(grip);
        }

        ((IPseudoClasses)chromeControl.Classes).Add(":floating");
        IsToolWindow = true;
    }

    /// <summary>
    /// Attaches a grip control to enable window dragging.
    /// </summary>
    /// <param name="grip">The grip control.</param>
    /// <param name="pseudoClass">The grip pseudo class.</param>
    public void AttachGrip(Control grip, string pseudoClass)
    {
        PseudoClasses.Set(pseudoClass, true);
        _chromeGrips.Add(grip);
    }

    /// <summary>
    /// Detaches grip to chrome.
    /// </summary>
    /// <param name="chromeControl">The chrome control.</param>
    public void DetachGrip(ToolChromeControl chromeControl)
    {
        if (chromeControl.Grip is { } grip)
        {
            _chromeGrips.Remove(grip);
        }

        if (chromeControl.CloseButton is not null)
        {
            chromeControl.CloseButton.Click -= ChromeCloseClick;
        }
    }

    /// <summary>
    /// Detaches a grip control from window dragging.
    /// </summary>
    /// <param name="grip">The grip control.</param>
    /// <param name="pseudoClass">The grip pseudo class.</param>
    public void DetachGrip(Control grip, string pseudoClass)
    {
        PseudoClasses.Set(pseudoClass, false);
        _chromeGrips.Remove(grip);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsToolWindowProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<bool>(), ToolChromeControlsWholeWindow, DocumentChromeControlsWholeWindow);
        }
        else if (change.Property == ToolChromeControlsWholeWindowProperty)
        {
            UpdatePseudoClasses(IsToolWindow, change.GetNewValue<bool>(), DocumentChromeControlsWholeWindow);
        }
        else if (change.Property == DocumentChromeControlsWholeWindowProperty)
        {
            UpdatePseudoClasses(IsToolWindow, ToolChromeControlsWholeWindow, change.GetNewValue<bool>());
        }
        else if (change.Property == WindowStateProperty && Window is { } && IsTracked)
        {
            Window.Save();
        }
    }

    private void UpdatePseudoClasses(bool isToolWindow, bool toolChromeControlsWholeWindow, bool documentChromeControlsWholeWindow)
    {
        PseudoClasses.Set(":toolwindow", isToolWindow);
        PseudoClasses.Set(":toolchromecontrolswindow", toolChromeControlsWholeWindow);
        PseudoClasses.Set(":documentchromecontrolswindow", documentChromeControlsWholeWindow);
    }

    private int CountVisibleToolsAndDocuments(IDockable? dockable)
    {
        switch (dockable)
        {
            case ITool:
                return 1;
            case IDocument:
                return 1;
            case IDock dock:
                return dock.VisibleDockables?.Sum(CountVisibleToolsAndDocuments) ?? 0;
            default:
                return 0;
        }
    }

    private void ChromeCloseClick(object? sender, RoutedEventArgs e)
    {
        if (CountVisibleToolsAndDocuments(DataContext as IRootDock) <= 1)
            Exit();
    }

    /// <inheritdoc/>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        Window?.Factory?.HostWindows.Add(this);
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        Window?.Factory?.HostWindows.Remove(this);

        if (Window is { })
        {
            Window.Factory?.CloseWindow(Window);
            Window.Factory?.OnWindowClosed(Window);

            if (IsTracked)
            {
                Window?.Factory?.RemoveWindow(Window);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (Window is { })
        {
            if (Window.Factory?.OnWindowClosing(Window) == false)
            {
                e.Cancel = true;
                return;
            }
        }

        if (Window is { } && IsTracked)
        {
            Window.Save();

            if (Window.Layout is IDock root)
            {
                if (root.Close.CanExecute(null))
                {
                    root.Close.Execute(null);
                }
            }
        }
    }

    private Window? ResolveOwnerWindow(IDockWindow windowModel, out bool copyOwnerChrome)
    {
        copyOwnerChrome = false;
        var parentWindow = windowModel.ParentWindow;
        var usesGlobalDefaultMode = windowModel.OwnerMode == DockWindowOwnerMode.Default;
        var ownerMode = windowModel.OwnerMode == DockWindowOwnerMode.Default
            ? DockSettings.DefaultFloatingWindowOwnerMode
            : windowModel.OwnerMode;

        if (ReferenceEquals(parentWindow, windowModel))
        {
            parentWindow = null;
        }

        if (ownerMode == DockWindowOwnerMode.None)
        {
            return null;
        }

        if (ownerMode == DockWindowOwnerMode.ParentWindow
            || ownerMode == DockWindowOwnerMode.DockableWindow)
        {
            if (parentWindow?.Host is Window parentOwnerWindow)
            {
                return parentOwnerWindow;
            }

            if (usesGlobalDefaultMode)
            {
                return ResolveFallbackOwnerWindow(windowModel);
            }

            return null;
        }

        if (ownerMode == DockWindowOwnerMode.RootWindow)
        {
            if (windowModel.Owner is IRootDock root && root.Window is { } rootWindow)
            {
                if (!ReferenceEquals(rootWindow, windowModel))
                {
                    return rootWindow.Host as Window;
                }
            }

            if (windowModel.Factory is { } factory && windowModel.Owner is { } owner)
            {
                var ownerRoot = factory.FindRoot(owner);
                if (ownerRoot?.Window is { } rootDockWindow && !ReferenceEquals(rootDockWindow, windowModel))
                {
                    return rootDockWindow.Host as Window;
                }
            }

            return null;
        }

        if (ownerMode == DockWindowOwnerMode.Default && parentWindow?.Host is Window explicitOwner)
        {
            return explicitOwner;
        }

        if (ownerMode == DockWindowOwnerMode.Default && DockSettings.ShouldUseOwnerForFloatingWindows())
        {
            var ownerDockControl = windowModel.Layout?.Factory?.DockControls.FirstOrDefault();
            if (ownerDockControl is Control control && control.GetVisualRoot() is Window visualOwnerWindow)
            {
                copyOwnerChrome = true;
                return visualOwnerWindow;
            }
        }

        return null;
    }

    private Window? ResolveFallbackOwnerWindow(IDockWindow windowModel)
    {
        if (windowModel.ParentWindow?.Host is Window explicitOwner)
        {
            return explicitOwner;
        }

        if (windowModel.Owner is IRootDock root && root.Window is { } rootWindow)
        {
            if (!ReferenceEquals(rootWindow, windowModel))
            {
                return rootWindow.Host as Window;
            }
        }

        if (windowModel.Factory is { } factory && windowModel.Owner is { } owner)
        {
            var ownerRoot = factory.FindRoot(owner);
            if (ownerRoot?.Window is { } rootDockWindow && !ReferenceEquals(rootDockWindow, windowModel))
            {
                return rootDockWindow.Host as Window;
            }
        }

        var ownerDockControl = windowModel.Layout?.Factory?.DockControls.FirstOrDefault();
        if (ownerDockControl is Control control && control.GetVisualRoot() is Window parentWindow)
        {
            return parentWindow;
        }

        return null;
    }

    private void ApplyPresentationOptions(IDockWindow windowModel)
    {
        if (windowModel.ShowInTaskbar is { } showInTaskbar)
        {
            ShowInTaskbar = showInTaskbar;
        }
    }

    /// <inheritdoc/>
    public void Present(bool isDialog)
    {
        if (isDialog)
        {
            if (!IsVisible)
            {
                if (Window is { } windowModel)
                {
                    windowModel.Factory?.OnWindowOpened(windowModel);
                    ApplyPresentationOptions(windowModel);
                }

                var ownerWindow = Window is { } modalWindowModel
                    ? ResolveOwnerWindow(modalWindowModel, out _)
                    : null;

                if (ownerWindow is null && Window is { } fallbackWindowModel)
                {
                    var fallbackOwnerMode = fallbackWindowModel.OwnerMode == DockWindowOwnerMode.Default
                        ? DockSettings.DefaultFloatingWindowOwnerMode
                        : fallbackWindowModel.OwnerMode;
                    var policyDisallowsOwner = fallbackOwnerMode == DockWindowOwnerMode.None
                                               || DockSettings.FloatingWindowOwnerPolicy == DockFloatingWindowOwnerPolicy.NeverOwned;
                    if (!policyDisallowsOwner)
                    {
                        ownerWindow = ResolveFallbackOwnerWindow(fallbackWindowModel);
                    }
                }

                if (ownerWindow is not null)
                {
                    ShowDialog(ownerWindow);
                }
                else
                {
                    DockLogger.LogDebug("Windowing", "Modal window has no owner; presenting non-modally.");
                    Show();
                }
            }
        }
        else
        {
            if (!IsVisible)
            {
                if (Window is { } windowModel)
                {
                    windowModel.Factory?.OnWindowOpened(windowModel);
                    ApplyPresentationOptions(windowModel);
                }

                var copyOwnerChrome = false;
                var ownerWindow = Window is { } ownerWindowModel
                    ? ResolveOwnerWindow(ownerWindowModel, out copyOwnerChrome)
                    : null;

                if (ownerWindow is not null)
                {
                    if (copyOwnerChrome)
                    {
                        Title = ownerWindow.Title;
                        Icon = ownerWindow.Icon;
                    }

                    Show(ownerWindow);
                }
                else
                {
                    Show();
                }
            }
        }
    }

    /// <inheritdoc/>
    public void Exit()
    {
        if (Window is { })
        {
            if (Window.OnClose())
            {
                Close();
            }
        }
        else
        {
            Close();
        }
    }

    /// <inheritdoc/>
    public void SetPosition(double x, double y)
    {
        if (!double.IsNaN(x) && !double.IsNaN(y))
        {
            Position = new PixelPoint((int)x, (int)y);
            _normalX = x;
            _normalY = y;
        }
    }

    /// <inheritdoc/>
    public void GetPosition(out double x, out double y)
    {
        if (WindowState != WindowState.Normal && TryGetNormalBounds(out var normalX, out var normalY, out _, out _))
        {
            x = normalX;
            y = normalY;
            return;
        }

        x = Position.X;
        y = Position.Y;
    }

    /// <inheritdoc/>
    public void SetSize(double width, double height)
    {
        if (!double.IsNaN(width))
        {
            Width = width;
            _normalWidth = width;
        }

        if (!double.IsNaN(height))
        {
            Height = height;
            _normalHeight = height;
        }
    }

    /// <inheritdoc/>
    public void GetSize(out double width, out double height)
    {
        if (WindowState != WindowState.Normal && TryGetNormalBounds(out _, out _, out var normalWidth, out var normalHeight))
        {
            width = normalWidth;
            height = normalHeight;
            return;
        }

        width = Width;
        height = Height;
    }

    /// <inheritdoc/>
    public void SetWindowState(DockWindowState windowState)
    {
        WindowState = DockWindowStateHelper.ToAvaloniaWindowState(windowState);
    }

    /// <inheritdoc/>
    public DockWindowState GetWindowState()
    {
        return DockWindowStateHelper.ToDockWindowState(WindowState);
    }

    /// <inheritdoc/>
    public void SetTitle(string? title)
    {
        if (!string.IsNullOrEmpty(title))
        {
            // Only do this if the user intended to manually set the title.
            // Otherwise binding to the active document title will no longer work due to local values taking priority.
            Title = title;
        }
    }

    /// <inheritdoc/>
    public void SetLayout(IDock layout)
    {
        DataContext = layout;
    }

    private void CaptureNormalBounds()
    {
        if (WindowState != WindowState.Normal)
        {
            return;
        }

        if (double.IsNaN(Width) || double.IsNaN(Height))
        {
            return;
        }

        _normalX = Position.X;
        _normalY = Position.Y;
        _normalWidth = Width;
        _normalHeight = Height;
    }

    private bool TryGetNormalBounds(out double x, out double y, out double width, out double height)
    {
        x = _normalX;
        y = _normalY;
        width = _normalWidth;
        height = _normalHeight;

        return !double.IsNaN(x)
               && !double.IsNaN(y)
               && !double.IsNaN(width)
               && !double.IsNaN(height)
               && width > 0
               && height > 0;
    }

    void IHostWindow.SetActive()
    {
        if(WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        
        Activate();
    }
}
