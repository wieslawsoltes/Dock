// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal static class WindowActivationHelper
{
    public static void ActivateAllWindows(IFactory factory, Visual? initiator)
    {
        var windows = new HashSet<Window>();

        foreach (var host in factory.HostWindows.OfType<Window>())
        {
            windows.Add(host);
        }

        foreach (var dockControl in factory.DockControls.OfType<Control>())
        {
            if (dockControl.GetVisualRoot() is Window window)
            {
                windows.Add(window);
            }
        }

        Window? rootWindow = null;
        if (initiator?.GetVisualRoot() is Window root)
        {
            rootWindow = root;
            windows.Remove(rootWindow);
        }

        var sortedWindows = windows.ToArray();

        Window.SortWindowsByZOrder(sortedWindows);

        foreach (var window in sortedWindows)
        {
            window.Activate();
        }

        rootWindow?.Activate();
    }
}
