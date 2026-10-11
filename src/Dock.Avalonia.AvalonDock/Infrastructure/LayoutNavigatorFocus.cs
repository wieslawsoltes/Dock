using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

internal static class LayoutNavigatorFocus
{
    internal static void Restore(LayoutItem item)
    {
        if (item.LayoutElement?.Content is not Control content) return;
        WeakReference<Control> reference = new(content);
        Dispatcher.UIThread.Post(() =>
        {
            if (!reference.TryGetTarget(out Control? target) || !target.IsEffectivelyVisible ||
                TopLevel.GetTopLevel(target)?.FocusManager is not { } manager) return;
            IInputElement? focus = target.Focusable ? target : manager.FindNextElement(NavigationDirection.Next,
                new FindNextElementOptions { FocusedElement = target });
            focus?.Focus();
        // Dock materializes selected content in its Background queue. Restore
        // focus after that work, when the activated content is in the visual tree.
        }, DispatcherPriority.ContextIdle);
    }
}
