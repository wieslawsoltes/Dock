using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Adapters;

/// <summary>
/// Provides logic for adjusting proportions around a splitter.
/// </summary>
public class ProportionalDockSplitterAdapter : IProportionalDockSplitterAdapter
{
    private static IDockable? GetSibling(IList<IDockable> items, int index, int direction)
    {
        var i = index + direction;
        return i >= 0 && i < items.Count ? items[i] : null;
    }

    /// <inheritdoc/>
    public void ResetProportion(IProportionalDockSplitter splitter)
    {
        if (splitter.Owner is IProportionalDock dock && dock.VisibleDockables is { } children)
        {
            var index = children.IndexOf(splitter);
            var prev = GetSibling(children, index, -1);
            var next = GetSibling(children, index, 1);
            if (prev is not null)
            {
                prev.Proportion = double.NaN;
            }
            if (next is not null)
            {
                next.Proportion = double.NaN;
            }
        }
    }

    /// <inheritdoc/>
    public void SetProportion(IProportionalDockSplitter splitter, double proportion)
    {
        if (splitter.Owner is IProportionalDock dock && dock.VisibleDockables is { } children)
        {
            var index = children.IndexOf(splitter);
            var prev = GetSibling(children, index, -1);
            var next = GetSibling(children, index, 1);
            if (prev is not null && next is not null)
            {
                prev.Proportion = proportion;
            }
        }
    }
}
