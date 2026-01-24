// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal static class WindowActivationHelper
{
    public static void ActivateAllWindows(IFactory factory, Visual? initiator)
    {
        var windows = new HashSet<Window>();
        var managedHosts = new List<ManagedHostWindow>();

        foreach (var host in factory.HostWindows.OfType<Window>())
        {
            windows.Add(host);
        }

        foreach (var host in factory.HostWindows.OfType<ManagedHostWindow>())
        {
            managedHosts.Add(host);
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

        foreach (var managed in managedHosts
            .OrderBy(host => host.ManagedZIndex))
        {
            managed.SetActive();
        }

        rootWindow?.Activate();
    }
}
