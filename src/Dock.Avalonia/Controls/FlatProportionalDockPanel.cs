// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;
using Avalonia.Controls;
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

        var surface = new FlatProportionalDockSurface
        {
            TrackingMode = TrackingMode.Visible,
            Background = Brushes.Transparent,
            [DockProperties.IsDropAreaProperty] = true,
            [DockProperties.IsDockTargetProperty] = true
        };

        DockProperties.SetDockAdornerHost(surface, surface);
        return surface;
    }

    /// <inheritdoc/>
    protected override void UpdateDockSurface(Control surface, IFlatProportionalDock dock)
    {
        if (surface is FlatProportionalDockSurface dockSurface
            && dock is DockFlatProportionalAdapter.DockFlatDockAdapter adapter)
        {
            dockSurface.SetDock(adapter.Dock);
            return;
        }

        base.UpdateDockSurface(surface, dock);
    }

    /// <inheritdoc/>
    protected override void ClearDockSurface(Control surface)
    {
        if (surface is FlatProportionalDockSurface dockSurface)
        {
            dockSurface.SetDock(null);
            return;
        }

        base.ClearDockSurface(surface);
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
