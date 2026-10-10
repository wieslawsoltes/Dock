using System;

namespace AvalonDock.Layout;

/// <summary>Chooses the edge and whether to create an outer pane when adding a tool.</summary>
[Flags]
public enum AnchorableShowStrategy : byte
{
    /// <summary>Creates a new pane at the outermost requested edge.</summary>
    Most = 0x01,
    /// <summary>Uses the left edge.</summary>
    Left = 0x02,
    /// <summary>Uses the right edge.</summary>
    Right = 0x04,
    /// <summary>Uses the top edge.</summary>
    Top = 0x10,
    /// <summary>Uses the bottom edge.</summary>
    Bottom = 0x20
}
