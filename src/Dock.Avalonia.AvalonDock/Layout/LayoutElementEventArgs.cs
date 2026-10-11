using System;

namespace AvalonDock.Layout;

/// <summary>Identifies the element whose membership in a layout changed.</summary>
public class LayoutElementEventArgs : EventArgs
{
    /// <summary>Creates event data for a layout element.</summary>
    /// <param name="element">The element entering or leaving the layout.</param>
    public LayoutElementEventArgs(LayoutElement element) => Element = element;

    /// <summary>Gets the element entering or leaving the layout.</summary>
    public LayoutElement Element { get; private set; }
}
