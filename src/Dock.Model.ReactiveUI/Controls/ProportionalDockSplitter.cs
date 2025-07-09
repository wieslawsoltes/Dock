// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public partial class ProportionalDockSplitter : DockableBase, IProportionalDockSplitter
{
    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
    /// </summary>
    public ProportionalDockSplitter()
    {
        _canResize = true;
    }
    
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanResize { get; set; }

    private IDockable? GetSibling(int direction)
    {
        if (Owner is IDock dock && dock.VisibleDockables is { } children)
        {
            var index = children.IndexOf(this) + direction;
            while (index >= 0 && index < children.Count)
            {
                var candidate = children[index];
                if (candidate is not IProportionalDockSplitter)
                {
                    return candidate;
                }
                index += direction;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public void ResetProportion()
    {
        var prev = GetSibling(-1);
        var next = GetSibling(1);
        if (prev is not null)
        {
            prev.Proportion = double.NaN;
        }
        if (next is not null)
        {
            next.Proportion = double.NaN;
        }
    }

    /// <inheritdoc/>
    public void SetProportion(double proportion)
    {
        var target = GetSibling(-1);
        var neighbour = GetSibling(1);
        if (target is null || neighbour is null)
        {
            return;
        }

        var delta = proportion - target.Proportion;
        target.Proportion = proportion;
        neighbour.Proportion = neighbour.Proportion - delta;
    }
}
