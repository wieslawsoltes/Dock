using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;

namespace AvalonDock.Infrastructure;

// Native pinned windows are created outside the layout's floating-window factory.
// Apply the manager's presentation and input routes before their first layout.
internal sealed class LayoutPinnedWindowBinding
{
    private static readonly AttachedProperty<LayoutPinnedWindowBinding?> BindingProperty =
        AvaloniaProperty.RegisterAttached<LayoutPinnedWindowBinding, PinnedDockWindow, LayoutPinnedWindowBinding?>("Binding");
    private readonly WeakReference<DockingManager> _manager;

    private LayoutPinnedWindowBinding(DockingManager manager) => _manager = new(manager);

    internal static IDisposable Observe(DockingManager manager)
    {
        WeakReference<DockingManager> reference = new(manager);
        return Window.WindowOpenedEvent.AddClassHandler<PinnedDockWindow>((window, _) =>
        {
            if (!reference.TryGetTarget(out DockingManager? owner) || window.GetValue(BindingProperty) is not null ||
                window.Content is not ToolDockControl { DataContext: IToolDock preview } ||
                !ReferenceEquals(preview, owner.DockLayout.PinnedDock)) return;

            LayoutPinnedWindowBinding binding = new(owner);
            window.SetValue(BindingProperty, binding);
            window.Closed += binding.Closed;
            window.AddHandler(InputElement.KeyDownEvent, binding.PreviewKeyDown, RoutingStrategies.Tunnel);
            window.Classes.Add("avalondock");
            window.Styles.Add(new AvalonDockTheme());
            LayoutCommandRouting.SetIsEnabled(window, true);
            LayoutContextMenuRouting.SetIsEnabled(window, true);
            owner.AttachThemeResources(window);
        });
    }

    private void PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab && (e.KeyModifiers & KeyModifiers.Control) != 0 &&
            _manager.TryGetTarget(out DockingManager? manager) && !manager.IsNavigatorWindowActive)
        {
            manager.ShowNavigatorWindow();
            e.Handled = true;
        }
    }

    private void Closed(object? sender, EventArgs e)
    {
        if (sender is not PinnedDockWindow window) return;
        window.Closed -= Closed;
        window.RemoveHandler(InputElement.KeyDownEvent, PreviewKeyDown);
        LayoutCommandRouting.SetIsEnabled(window, false);
        LayoutContextMenuRouting.SetIsEnabled(window, false);
        window.ClearValue(BindingProperty);
    }
}
