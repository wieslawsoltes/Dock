// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Dock.Avalonia.Mdi;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Panel used to arrange MDI document windows.
/// </summary>
public class MdiLayoutPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="LayoutManager"/> property.
    /// </summary>
    public static readonly StyledProperty<IMdiLayoutManager?> LayoutManagerProperty =
        AvaloniaProperty.Register<MdiLayoutPanel, IMdiLayoutManager?>(nameof(LayoutManager), ClassicMdiLayoutManager.Instance);

    /// <summary>
    /// Gets or sets the layout manager.
    /// </summary>
    public IMdiLayoutManager? LayoutManager
    {
        get => GetValue(LayoutManagerProperty);
        set => SetValue(LayoutManagerProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LayoutManagerProperty)
        {
            InvalidateArrange();
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(availableSize);
        }

        return availableSize;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var documentEntries = new List<MdiLayoutEntry>();
        foreach (var child in Children)
        {
            if (child.DataContext is IMdiDocument document)
            {
                documentEntries.Add(new MdiLayoutEntry(child, document));
            }
        }

        var manager = LayoutManager ?? ClassicMdiLayoutManager.Instance;
        manager.Arrange(documentEntries, finalSize);

        return finalSize;
    }
}
