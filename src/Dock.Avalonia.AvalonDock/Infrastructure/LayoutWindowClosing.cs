using System.Collections.Generic;
using AvalonDock.Controls;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutWindowClosing
{
    internal static bool TryClose(LayoutRoot root, IDockWindow window)
    {
        // Host detachment, docking, and layout replacement preserve content.
        if (window.Layout is null || window.Host?.IsTracked != true) return true;
        LayoutFloatingWindow? floating = null;
        foreach (LayoutFloatingWindow candidate in root.FloatingWindows)
            if (ReferenceEquals(candidate.DockWindow, window)) { floating = candidate; break; }
        if (floating is null) return true;

        List<LayoutContent> contents = new();
        bool canClose = true;
        bool canHide = floating is LayoutAnchorableFloatingWindow;
        foreach (ILayoutElement element in floating.Descendents())
        {
            if (element is not LayoutContent content) continue;
            contents.Add(content);
            canClose &= content.CanClose;
            canHide &= content is LayoutAnchorable { CanHide: true };
        }
        if (contents.Count == 0) return true;
        if (window.Host is LayoutFloatingWindowControl control)
        {
            control.CloseOrHideContents();
        }
        else if (canClose)
        {
            foreach (LayoutContent content in contents) content.Close();
        }
        else if (canHide)
        {
            foreach (LayoutContent content in contents) ((LayoutAnchorable)content).Hide();
        }
        // Hiding and content cancellation cancel the native close. The hidden
        // host stays tracked for restoration; closing all content detaches it.
        return window.Host?.IsTracked != true;
    }
}
