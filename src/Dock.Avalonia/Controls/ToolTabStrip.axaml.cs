// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;
using System.Runtime.InteropServices;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStrip custom control.
/// </summary>
[PseudoClasses(":create")]
public class ToolTabStrip : TabStrip
{
    private HostWindow? _attachedWindow;
    private Control? _grip;
    private WindowDragHelper? _windowDragHelper;
    /// <summary>
    /// Defines the <see cref="DockAdornerHost"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> DockAdornerHostProperty =
        AvaloniaProperty.Register<ToolTabStrip, Control?>(nameof(DockAdornerHost));

    /// <summary>
    /// Defines the <see cref="CanCreateItem"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanCreateItemProperty =
        AvaloniaProperty.Register<ToolTabStrip, bool>(nameof(CanCreateItem));

    /// <summary>
    /// Defines the <see cref="EnableWindowDrag"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> EnableWindowDragProperty =
        AvaloniaProperty.Register<ToolTabStrip, bool>(nameof(EnableWindowDrag), true);

    /// <summary>
    /// Gets or sets if tab strop dock can create new items.
    /// </summary>
    public bool CanCreateItem
    {
        get => GetValue(CanCreateItemProperty);
        set => SetValue(CanCreateItemProperty, value);
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
    /// Gets or sets the control that should host the dock adorner.
    /// </summary>
    public Control? DockAdornerHost
    {
        get => GetValue(DockAdornerHostProperty);
        set => SetValue(DockAdornerHostProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ToolTabStrip);

    /// <summary>
    /// Initializes new instance of the <see cref="ToolTabStrip"/> class.
    /// </summary>
    public ToolTabStrip()
    {
        UpdatePseudoClasses(CanCreateItem);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new ToolTabStripItem();
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ToolTabStripItem>(item, out recycleKey);
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
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanCreateItemProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<bool>());
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

    private void UpdatePseudoClasses(bool canCreate)
    {
        PseudoClasses.Set(":create", canCreate);
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
                       !(s is ToolTabStripItem) &&
                       !(s is Button) &&
                       !WindowDragHelper.IsChildOfType<ToolTabStripItem>(this, s) &&
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
                    hostWindow.AttachGrip(_grip, ":toolwindow");
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
            _attachedWindow.DetachGrip(_grip, ":toolwindow");
            _attachedWindow = null;
        }

        if (_windowDragHelper != null)
        {
            _windowDragHelper.Detach();
            _windowDragHelper = null;
        }
    }
}
