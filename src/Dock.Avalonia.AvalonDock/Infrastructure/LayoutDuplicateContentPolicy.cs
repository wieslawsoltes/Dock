using AvalonDock.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Duplicate restrictions apply to dropping into a pane, not to split targets
// or application-driven collection edits. Compare the public identifiers:
// Dock's generated native IDs are not implicit AvalonDock ContentIds.
internal sealed class LayoutDuplicateContentPolicy(LayoutModelContext context) : IDockDropPolicy
{
    public bool CanDrop(IDockable source, IDockable target, DockOperation operation)
    {
        if (operation != DockOperation.Fill) return true;
        IDockable? pane = target is IDocumentDock or IToolDock ? target : target.Owner;
        if (pane is not (IDocumentDock or IToolDock) || pane is not IDock { VisibleDockables: { } contents } ||
            context.FindExisting(pane) is not ILayoutPositionableGroup { AllowDuplicateContent: false }) return true;
        for (int index = 0; index < contents.Count; index++)
        {
            if (context.FindExisting(contents[index]) is LayoutContent existing && ContainsDuplicate(source, existing))
                return false;
        }
        return true;
    }

    private bool ContainsDuplicate(IDockable source, LayoutContent existing)
    {
        if (context.FindExisting(source) is LayoutContent incoming)
            return !ReferenceEquals(incoming, existing) && incoming.Title == existing.Title && incoming.ContentId == existing.ContentId;
        if (source is IDock { VisibleDockables: { } children })
            for (int index = 0; index < children.Count; index++)
                if (ContainsDuplicate(children[index], existing)) return true;
        return false;
    }
}
