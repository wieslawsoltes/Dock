using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal static class Extensions
{
    public static IEnumerable<DockControl> GetZOrderedDockControls(this IList<IDockControl> dockControls)
    {
        // Note: we should traverse the dock controls in their windows' z-order.
        // However there is no way to get the z-order of a window in Avalonia.
        // Uncomment once this PR is merged and a new Avalonia version is released
        // https://github.com/AvaloniaUI/Avalonia/pull/14909
        // return dockControls
        //     .OfType<DockControl>()
        //     .Select(dock => (dock, order: (dock.GetVisualRoot() as Window)?.WindowZOrder ?? IntPtr.Zero))
        //     .OrderByDescending(x => x.order)
        //     .Select(pair => pair.dock);

        // For now, as a workaround, iterating in the reverse order of the dock controls is better then the regular order,
        // because the main window dock control is always at index 0 and all the other windows are always
        // on top of the main window.
        return dockControls
            .OfType<DockControl>()
            .Reverse();
    }
}
