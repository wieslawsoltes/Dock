// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;

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
            },
            FloatActiveDocument);
    }

    private static PixelPoint ApplyMagnetism(HostWindow window, PixelPoint position)
    {
        if (!Dock.Settings.DockSettings.EnableWindowMagnetism || window.Window?.Factory is not { HostWindows: var windows })
        {
            return position;
        }

        var snap = Dock.Settings.DockSettings.WindowMagnetDistance;
        var x = position.X;
        var y = position.Y;
        var rect = new Rect(position.X, position.Y, window.Width, window.Height);

        foreach (var host in windows.OfType<HostWindow>())
        {
            if (host == window || !host.IsVisible)
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

        return new PixelPoint(x, y);
    }

    private void FloatActiveDocument(PointerPressedEventArgs args)
    {
        if (DataContext is Dock.Model.Core.IDock { ActiveDockable: { } dockable, Factory: { } factory } dock)
        {
            var before = factory.HostWindows.Count;
            var screen = this.PointToScreen(args.GetPosition(this));
            dockable.SetPointerScreenPosition(screen.X, screen.Y);
            factory.FloatDockable(dockable);

            if (factory.HostWindows.Count > before && factory.HostWindows[^1] is HostWindow host)
            {
                var snapped = ApplyMagnetism(host, host.Position);
                if (snapped != host.Position)
                {
                    host.Position = snapped;
                }
            }
        }
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
}
