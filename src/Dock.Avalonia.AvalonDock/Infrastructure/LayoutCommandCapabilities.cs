using AvalonDock.Layout;
using Dock.Model;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Explicit layout methods follow AvalonDock's model contract. UI command
// availability also honors the native factory's restrictions.
internal static class LayoutCommandCapabilities
{
    internal static bool IsEnabled(LayoutContent content, DockCapability capability) =>
        DockCapabilityResolver.IsEnabled(content.DockModel, capability,
            DockCapabilityResolver.ResolveOperationDock(content.DockModel));

    // A replacement command owns AvalonDock's capability decision. Explicit
    // Dock policies remain authoritative for native integration gestures.
    internal static bool IsAllowedByPolicy(IDockable dockable, DockCapability capability)
    {
        DockCapabilityEvaluation evaluation = DockCapabilityResolver.Evaluate(dockable, capability);
        return evaluation.EffectiveSource == DockCapabilityValueSource.Dockable || evaluation.EffectiveValue;
    }

    internal static bool CanClose(LayoutContent content) =>
        content.CanClose && IsEnabled(content, DockCapability.Close) &&
        (content.DockModel.Owner is not IDock { CanCloseLastDockable: false } owner ||
            owner.VisibleDockables is not { Count: <= 1 });
}
