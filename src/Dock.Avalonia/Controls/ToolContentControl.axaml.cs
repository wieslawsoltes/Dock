// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Reactive;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="ToolContentControl"/> xaml.
/// </summary>
public class ToolContentControl : TemplatedControl
{
    private IDisposable? _dataContextDisposable;
    private IDockable? _currentDockable;
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _dataContextDisposable = this.GetObservable(DataContextProperty).Subscribe(
            new AnonymousObserver<object?>(DataContextTracking));

        DataContextTracking(DataContext);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_currentDockable is { Factory: { } factory })
        {
            factory.ToolControls.Remove(_currentDockable);
            _currentDockable = null;
        }

        _dataContextDisposable?.Dispose();
    }

    private void DataContextTracking(object? dataContext)
    {
        if (_currentDockable is { Factory: { } factory })
        {
            factory.ToolControls.Remove(_currentDockable);
            _currentDockable = null;
        }

        if (dataContext is IDockable { Factory: { } factory } dockable)
        {
            factory.ToolControls[dockable] = this;
            _currentDockable = dockable;
        }
    }
}
