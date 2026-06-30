// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Dock.Avalonia.Internal;
using Dock.Controls.Flat;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Presents a Dock proportional model tree through the reusable flat proportional panel.
/// </summary>
public class FlatProportionalDockPanel : FlatProportionalPanel
{
    private DockFlatProportionalAdapter? _adapter;

    /// <summary>
    /// Defines the <see cref="Dock"/> property.
    /// </summary>
    public static readonly StyledProperty<IProportionalDock?> DockProperty =
        AvaloniaProperty.Register<FlatProportionalDockPanel, IProportionalDock?>(nameof(Dock));

    /// <summary>
    /// Gets or sets the root proportional dock to present.
    /// </summary>
    public IProportionalDock? Dock
    {
        get => GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DockProperty)
        {
            SetDockRoot(Dock);
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (Root is null && Dock is not null)
        {
            SetDockRoot(Dock);
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        DisposeAdapter();
        Root = null;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        _adapter?.PruneUnreachable(Dock);
        return result;
    }

    private void SetDockRoot(IProportionalDock? dock)
    {
        DisposeAdapter();

        if (dock is null)
        {
            Root = null;
            return;
        }

        _adapter = new DockFlatProportionalAdapter();
        Root = _adapter.GetDock(dock);
    }

    private void DisposeAdapter()
    {
        _adapter?.Dispose();
        _adapter = null;
    }

    /// <inheritdoc/>
    protected override Control CreateDockSurface(IFlatProportionalDock dock)
    {
        if (dock is not DockFlatProportionalAdapter.DockFlatDockAdapter adapter)
        {
            return base.CreateDockSurface(dock);
        }

        var surface = new DockableControl
        {
            TrackingMode = TrackingMode.Visible,
            Background = Brushes.Transparent,
            DataContext = adapter.Dock,
            [DockProperties.IsDropAreaProperty] = true,
            [DockProperties.IsDockTargetProperty] = true
        };

        DockProperties.SetDockAdornerHost(surface, surface);

        surface.Bind(DockProperties.IsDropEnabledProperty, new Binding(nameof(IDockable.CanDrop)));
        surface.Bind(DockProperties.DockGroupProperty, new Binding(nameof(IDockable.DockGroup)));

        return surface;
    }

    /// <inheritdoc/>
    protected override FlatProportionalSplitter CreateSplitter(
        IFlatProportionalDock ownerDock,
        IFlatProportionalSplitter splitter)
    {
        return new FlatProportionalDockSplitter
        {
            DataContext = splitter
        };
    }

    internal void ResizeSplitter(FlatProportionalDockSplitter splitterControl, double dragDelta)
    {
        ResizeSplitter((FlatProportionalSplitter)splitterControl, dragDelta);
    }
}
