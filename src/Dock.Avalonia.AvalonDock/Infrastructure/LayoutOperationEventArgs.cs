using Avalonia.Interactivity;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Avalonia starts a route with the raising control as Source. Restore the
// upstream public-model source before application instance/ancestor handlers.
internal sealed class LayoutOperationEventArgs(RoutedEvent routedEvent, LayoutContent content)
    : RoutedEventArgs(routedEvent, content)
{
    internal static void RestoreSource(DockingManager manager, RoutedEventArgs args)
    {
        if (args is LayoutOperationEventArgs notification && ReferenceEquals(args.Source, manager))
            args.Source = notification.Content;
    }

    private LayoutContent Content => content;
}
