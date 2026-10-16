using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.VisualTree;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Dock owns this scope's lifetime, including transfer, cancellation and drop commit.
internal sealed class LayoutDragSourcePreview : IDisposable
{
    private readonly List<ILayoutGridView> _grids = new();
    private readonly List<IDisposable> _visibility = new();

    internal static LayoutDragSourcePreview Create(IDockable source, LayoutElement element, DockControl host)
    {
        LayoutDragSourcePreview preview = new();
        IDock? nativePane = source as IDock ?? source.Owner as IDock;
        ILayoutPanelElement? pane = element as ILayoutPanelElement ?? element.Parent as ILayoutPanelElement;
        foreach (Visual visual in host.GetVisualDescendants())
        {
            if (pane is not null && visual is ILayoutGridView grid)
            {
                grid.SetDragSourcePreview(pane);
                preview._grids.Add(grid);
            }
            // Retain an empty main document region as an available drop target.
            // This also handles the implicit pane in a document floating window.
            if (visual is Control control &&
                (control is DocumentTabStrip or ToolChromeControl or MdiDocumentControl
                    && ReferenceEquals(control.DataContext, nativePane)
                 || control is DockableControl && control.DataContext is IDockable content
                    && ReferenceEquals(content.Owner, nativePane)))
                preview._visibility.Add(control.SetValue(Visual.IsVisibleProperty, false, BindingPriority.Animation)!);
        }
        return preview;
    }

    public void Dispose()
    {
        foreach (ILayoutGridView grid in _grids) grid.SetDragSourcePreview(null);
        foreach (IDisposable visibility in _visibility) visibility.Dispose();
        _grids.Clear();
        _visibility.Clear();
    }
}
