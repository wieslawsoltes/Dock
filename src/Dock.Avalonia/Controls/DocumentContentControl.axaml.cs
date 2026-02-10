// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Reactive;
using Avalonia.Styling;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DocumentContentControl"/> xaml.
/// </summary>
public class DocumentContentControl : TemplatedControl
{
    private IDisposable? _dataContextSubscription;

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _dataContextSubscription = this.GetObservable(DataContextProperty)
            .Subscribe(new AnonymousObserver<object?>(ApplyGeneratedTheme));

        ApplyGeneratedTheme(DataContext);

        if (DataContext is IDockable { Factory: { } factory } dockable)
        {
            factory.DocumentControls[dockable] = this;
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _dataContextSubscription?.Dispose();
        _dataContextSubscription = null;
        ClearValue(ThemeProperty);

        if (DataContext is IDockable { Factory: { } factory } dockable)
        {
            factory.DocumentControls.Remove(dockable);
        }
    }

    private void ApplyGeneratedTheme(object? dataContext)
    {
        if (dataContext is not IDockItemContainerMetadata { ItemContainerTheme: { } themeMetadata })
        {
            ClearValue(ThemeProperty);
            return;
        }

        if (themeMetadata is ControlTheme directTheme)
        {
            Theme = directTheme;
            return;
        }

        if (this.TryFindResource(themeMetadata, out var resource)
            && resource is ControlTheme resolvedTheme)
        {
            Theme = resolvedTheme;
            return;
        }

        ClearValue(ThemeProperty);
    }
}
