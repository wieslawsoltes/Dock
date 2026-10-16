// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Control used to track associated to <see cref="IDockable"/> control state.
/// </summary>
public class DockableControl : Panel, IDockableControl
{
    private DockableControlTracker? _tracker;

    /// <summary>
    /// Defines the <see cref="TrackingMode"/> property.
    /// </summary>
    public static readonly StyledProperty<TrackingMode> TrackingModeProperty = 
        AvaloniaProperty.Register<DockableControl, TrackingMode>(nameof(TrackingMode));

    /// <summary>
    /// Gets or sets dockable tracking mode.
    /// </summary>
    public TrackingMode TrackingMode
    {
        get => GetValue(TrackingModeProperty);
        set => SetValue(TrackingModeProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tracker?.Dispose();
        _tracker = new DockableControlTracker(this, TrackingMode);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _tracker?.Dispose();
        _tracker = null;
        base.OnDetachedFromVisualTree(e);
    }
}
