// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls.Primitives;
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
