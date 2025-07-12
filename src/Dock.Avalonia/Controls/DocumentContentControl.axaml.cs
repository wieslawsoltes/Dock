// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using System;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DocumentContentControl"/> xaml.
/// </summary>
public class DocumentContentControl : TemplatedControl
{
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is IDockable { Factory: { } factory } dockable)
        {
            factory.DocumentControls[dockable] = this;
        }

        // Fade the content in when it is attached to the visual tree
        Opacity = 0;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(OpacityProperty, 0d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(OpacityProperty, 1d) }
                }
            }
        };
        _ = animation.RunAsync(this, null);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (DataContext is IDockable { Factory: { } factory } dockable)
        {
            factory.DocumentControls.Remove(dockable);
        }
    }
}
