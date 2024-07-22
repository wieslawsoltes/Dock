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
    private static int IndexOf(this Window[] windowsArray, Window? windowToFind)
    {
        if (windowToFind == null)
            return -1;

        for (var i = 0; i < windowsArray.Length; i++)
        {
            if (ReferenceEquals(windowsArray[i], windowToFind))
                return i;
        }

        return -1;
    }

    public static IEnumerable<DockControl> GetZOrderedDockControls(this IList<IDockControl> dockControls)
    {
        var windows = dockControls
            .OfType<DockControl>()
            .Select(dock => dock.GetVisualRoot() as Window)
            .OfType<Window>()
            .Distinct()
            .ToArray();

        Window.SortWindowsByZOrder(windows);

        return dockControls
            .OfType<DockControl>()
            .Select(dock => (dock, order: windows.IndexOf(dock.GetVisualRoot() as Window)))
            .OrderByDescending(x => x.order)
            .Select(pair => pair.dock);
    }
}
