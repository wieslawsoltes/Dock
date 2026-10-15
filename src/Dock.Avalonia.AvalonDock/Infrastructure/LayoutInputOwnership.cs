using Avalonia;
using Avalonia.VisualTree;

namespace AvalonDock.Infrastructure;

internal static class LayoutInputOwnership
{
    internal static bool IsWithinManager(object? source, DockingManager? owner)
    {
        if (source is not Visual visual || owner is null) return false;
        for (Visual? parent = visual.GetVisualParent(); parent is not null; parent = parent.GetVisualParent())
            if (parent is DockingManager manager) return ReferenceEquals(manager, owner);
        return false;
    }
}
