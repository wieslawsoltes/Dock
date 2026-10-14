using System;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>Lets a factory preview source removal in a custom pane layout.</summary>
/// <remarks>
/// Implement this on the source factory when its views do not use Dock's proportional
/// panels. Dock still handles individual tabs in panes containing multiple items.
/// </remarks>
public interface IDockDragSourcePreviewProvider
{
    /// <summary>Temporarily presents the layout with the dragged pane or last item absent.</summary>
    /// <param name="source">The resolved pane or its last item.</param>
    /// <param name="host">The attached Dock control containing the source visuals.</param>
    /// <returns>A scope that restores presentation when disposed, or null to leave it unchanged.</returns>
    /// <remarks>
    /// Change presentation only; retain live ownership, collections and saved dimensions.
    /// Dock disposes the scope before a drop commits, on cancellation, on host detachment,
    /// or before recreating it after layout changes. Disposal must be idempotent and must
    /// preserve intervening application changes. A null result opts out of the stock pane
    /// projection. This hook also supports panes whose models are not collapsible.
    /// </remarks>
    IDisposable? CreateDragSourcePreview(IDockable source, DockControl host);
}
