using System;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Manager-dispatched model operations require a live root and manager before
// touching native layout state. Other model methods have their own contracts.
internal static class LayoutOperationContext
{
    internal static DockingManager RequireManager(ILayoutElement element) =>
        element.Root!.Manager ?? throw new NullReferenceException();

    internal static void SelectAndActivate(LayoutContent content)
    {
        content.IsSelected = true;
        // Public collections move the model passively. Select the new native
        // pane before projecting active state so its previous focus cannot win.
        if (content.DockModel.Owner is IDock pane && content.DockModel.Factory is { } factory)
            factory.SetFocusedDockable(pane, content.DockModel);
        content.IsActive = true;
    }
}
