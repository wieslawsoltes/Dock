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
        var activated = new HashSet<Window>();

        foreach (var host in factory.HostWindows.OfType<Window>())
        {
            if (activated.Add(host))
            {
                host.Activate();
            }
        }

        foreach (var dockControl in factory.DockControls.OfType<Control>())
        {
            if (dockControl.GetVisualRoot() is Window window && activated.Add(window))
            {
                window.Activate();
            }
        }

        if (initiator?.GetVisualRoot() is Window root)
        {
            activated.Add(root);
            root.Activate();
        }
    }
}
