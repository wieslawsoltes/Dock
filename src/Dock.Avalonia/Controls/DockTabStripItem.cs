using System;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Dock TabStripItem custom control.
    /// </summary>
    public class DockTabStripItem : TabStripItem, IStyleable
    {
        Type IStyleable.StyleKey => typeof(TabStripItem);
    }
}
