// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStripItem custom control.
/// </summary>
[PseudoClasses(":active", ":flash")]
public class DocumentTabStripItem : TabStripItem
{
    private readonly DispatcherTimer _flashTimer;
    private bool _flashState;
    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<DocumentTabStripItem, bool>(nameof(IsActive));

    /// <summary>
    /// Define the <see cref="IsFlashing"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFlashingProperty =
        AvaloniaProperty.Register<DocumentTabStripItem, bool>(nameof(IsFlashing));

    /// <summary>
    /// Gets or sets if this is the currently active dockable.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets if this tab item should flash.
    /// </summary>
    public bool IsFlashing
    {
        get => GetValue(IsFlashingProperty);
        set => SetValue(IsFlashingProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DocumentTabStripItem);
        
    /// <summary>
    /// Initializes new instance of the <see cref="DocumentTabStripItem"/> class.
    /// </summary>
    public DocumentTabStripItem()
    {
        _flashTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _flashTimer.Tick += (_, _) =>
        {
            _flashState = !_flashState;
            PseudoClasses.Set(":flash", _flashState);
        };

        UpdatePseudoClasses(IsActive);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        RemoveHandler(PointerPressedEvent, PressedHandler);
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            if (DataContext is IDockable { Owner: IDock { Factory: { } factory }, CanClose: true } dockable)
            {
                factory.CloseDockable(dockable);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<bool>());
            if (IsActive)
            {
                IsFlashing = false;
            }
        }
        else if (change.Property == IsFlashingProperty)
        {
            if (IsFlashing)
            {
                _flashTimer.Start();
            }
            else
            {
                _flashTimer.Stop();
                PseudoClasses.Set(":flash", false);
                _flashState = false;
            }
        }
    }

    private void UpdatePseudoClasses(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }
}
