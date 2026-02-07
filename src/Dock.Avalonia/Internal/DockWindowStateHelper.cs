// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal static class DockWindowStateHelper
{
    public static DockWindowState ToDockWindowState(WindowState windowState)
    {
        return windowState switch
        {
            WindowState.Minimized => DockWindowState.Minimized,
            WindowState.Maximized => DockWindowState.Maximized,
            WindowState.FullScreen => DockWindowState.FullScreen,
            _ => DockWindowState.Normal
        };
    }

    public static WindowState ToAvaloniaWindowState(DockWindowState windowState)
    {
        return windowState switch
        {
            DockWindowState.Minimized => WindowState.Minimized,
            DockWindowState.Maximized => WindowState.Maximized,
            DockWindowState.FullScreen => WindowState.FullScreen,
            _ => WindowState.Normal
        };
    }

    public static DockWindowState ToDockWindowState(MdiWindowState mdiWindowState)
    {
        return mdiWindowState switch
        {
            MdiWindowState.Minimized => DockWindowState.Minimized,
            MdiWindowState.Maximized => DockWindowState.Maximized,
            _ => DockWindowState.Normal
        };
    }

    public static MdiWindowState ToMdiWindowState(DockWindowState windowState)
    {
        return windowState switch
        {
            DockWindowState.Minimized => MdiWindowState.Minimized,
            DockWindowState.Maximized => MdiWindowState.Maximized,
            _ => MdiWindowState.Normal
        };
    }
}
