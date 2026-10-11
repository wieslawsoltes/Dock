using AvalonDock.Layout;
using Dock.Model;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Explicit layout methods follow AvalonDock's model contract. UI commands also
// honor the restrictions of the native factory they previously called.
internal static class LayoutCommandCapabilities
{
    internal static bool IsEnabled(LayoutContent content, DockCapability capability) =>
        DockCapabilityResolver.IsEnabled(content.DockModel, capability,
            DockCapabilityResolver.ResolveOperationDock(content.DockModel));

    internal static bool CanClose(LayoutContent content) =>
        content.CanClose && IsEnabled(content, DockCapability.Close) &&
        (content.DockModel.Owner is not IDock { CanCloseLastDockable: false } owner ||
            owner.VisibleDockables is not { Count: <= 1 });
}
