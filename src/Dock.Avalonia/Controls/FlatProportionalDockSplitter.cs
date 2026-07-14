// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using Avalonia;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;
using Dock.Controls.Flat;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Splitter used by <see cref="FlatProportionalDockPanel"/> to resize flattened proportional dock regions.
/// </summary>
public class FlatProportionalDockSplitter : FlatProportionalSplitter
{
    private IProportionalDockSplitter? _dataContextSplitter;
    private INotifyPropertyChanged? _propertyChanged;

    /// <summary>
    /// Gets the Dock model splitter represented by this control.
    /// </summary>
    public new IProportionalDockSplitter? Splitter =>
        base.Splitter is DockFlatProportionalAdapter.DockFlatSplitterAdapter adapter ? adapter.Splitter : null;

    /// <summary>
    /// Gets the Dock model proportional dock that owns <see cref="Splitter"/>.
    /// </summary>
    public new IProportionalDock? OwnerDock =>
        base.OwnerDock is DockFlatProportionalAdapter.DockFlatDockAdapter adapter ? adapter.Dock : null;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataContextProperty)
        {
            SetDataContextSplitter(change.NewValue as IProportionalDockSplitter);
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SubscribeToDataContextSplitter();
        base.OnAttachedToVisualTree(e);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromDataContextSplitter();
        base.OnDetachedFromVisualTree(e);
    }

    private void SetDataContextSplitter(IProportionalDockSplitter? splitter)
    {
        UnsubscribeFromDataContextSplitter();
        _dataContextSplitter = splitter;

        if (VisualRoot is not null)
        {
            SubscribeToDataContextSplitter();
        }

        UpdateFromDataContextSplitter();
    }

    private void SubscribeToDataContextSplitter()
    {
        if (_propertyChanged is not null
            || _dataContextSplitter is not INotifyPropertyChanged propertyChanged)
        {
            return;
        }

        _propertyChanged = propertyChanged;
        _propertyChanged.PropertyChanged += OnDataContextSplitterPropertyChanged;
    }

    private void UnsubscribeFromDataContextSplitter()
    {
        if (_propertyChanged is null)
        {
            return;
        }

        _propertyChanged.PropertyChanged -= OnDataContextSplitterPropertyChanged;
        _propertyChanged = null;
    }

    private void OnDataContextSplitterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName == nameof(IProportionalDockSplitter.CanResize)
            || e.PropertyName == nameof(IProportionalDockSplitter.ResizePreview))
        {
            UpdateFromDataContextSplitter();
        }
    }

    private void UpdateFromDataContextSplitter()
    {
        if (_dataContextSplitter is null)
        {
            return;
        }

        IsResizingEnabled = _dataContextSplitter.CanResize;
        PreviewResize = _dataContextSplitter.ResizePreview;
    }
}
