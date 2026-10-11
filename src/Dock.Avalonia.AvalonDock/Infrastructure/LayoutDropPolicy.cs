using Dock.Model.Controls;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// AvalonDock document floating windows are not overlay hosts. Tool floating
// windows accept tools from the same manager, retaining their typed hierarchy.
internal sealed class LayoutDropPolicy(LayoutRoot layout) : IDockDropPolicy
{
    public bool CanDrop(IDockable source, IDockable target, DockOperation operation)
    {
        if (operation == DockOperation.Window) return true;
        if (!ReferenceEquals(source.Factory, target.Factory)) return false;
        if (!AllowsDocumentSplit(target, operation)) return false;
        for (IDockable? current = target; current is not null; current = current.Owner)
        {
            if (current is not IRootDock { Window: not null } root) continue;
            if (ReferenceEquals(root, layout.DockModel)) return true;
            if (root.VisibleDockables is { Count: > 0 } children && children[0] is IDocumentDock) return false;
            return !ContainsDocument(source);
        }
        return true;
    }

    private bool AllowsDocumentSplit(IDockable target, DockOperation operation)
    {
        if (operation is not (DockOperation.Left or DockOperation.Right or DockOperation.Top or DockOperation.Bottom) ||
            layout.Manager?.AllowMixedOrientation == true) return true;
        IDocumentDock? nativePane = target as IDocumentDock ?? target.Owner as IDocumentDock;
        if (nativePane is null || layout.Context.FindExisting(nativePane) is not LayoutDocumentPane
            { Parent: LayoutDocumentPaneGroup group }) return true;
        bool horizontal = operation is DockOperation.Left or DockOperation.Right;
        if (horizontal == (group.Orientation == global::Avalonia.Layout.Orientation.Horizontal)) return true;
        int visible = 0;
        foreach (ILayoutDocumentPane child in group.Children)
            if (child.IsVisible && ++visible > 1) return false;
        return true;
    }

    private static bool ContainsDocument(IDockable source)
    {
        // Native Tool also implements IDocument for Dock's document-tab support.
        if (source is IDocument and not ITool || source is IDocumentDock) return true;
        if (source is IDock { VisibleDockables: { } children })
            foreach (IDockable child in children) if (ContainsDocument(child)) return true;
        return false;
    }
}
