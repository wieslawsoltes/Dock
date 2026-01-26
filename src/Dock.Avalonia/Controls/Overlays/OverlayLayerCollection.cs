using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Defines an ordered collection of overlay layers.
/// </summary>
public sealed class OverlayLayerCollection : AvaloniaList<IOverlayLayer>
{
    /// <summary>
    /// Returns the layers ordered by z-index and insertion order.
    /// </summary>
    public IEnumerable<IOverlayLayer> GetOrderedLayers()
    {
        return this
            .Select((layer, index) => (Layer: layer, Index: index))
            .OrderBy(entry => entry.Layer.ZIndex)
            .ThenBy(entry => entry.Index)
            .Select(entry => entry.Layer);
    }
}
