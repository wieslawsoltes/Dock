using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

// An outer split keeps a dragged pane/group intact instead of distributing its
// tabs among new panes. Mutations still pass through the destination factory.
internal static class DockOuterSplit
{
    internal static DockOperation Direction(DockOperation operation) => operation switch
    {
        DockOperation.OuterLeft => DockOperation.Left,
        DockOperation.OuterTop => DockOperation.Top,
        DockOperation.OuterRight => DockOperation.Right,
        DockOperation.OuterBottom => DockOperation.Bottom,
        _ => DockOperation.None
    };

    internal static void Execute(IDockable source, IDock target, DockOperation direction)
    {
        if (source.Owner is not IDock owner || target.Factory is not { } factory) return;
        IDockable? focused = factory.FindRoot(source, static root => root.IsFocusableRoot)?.FocusedDockable;
        if (!Contains(source, focused)) focused = SelectedContent(source);
        if (target.Owner is IProportionalDock parent && parent.VisibleDockables is { } siblings)
        {
            int count = 0;
            foreach (IDockable sibling in siblings) if (sibling is not ISplitter) count++;
            if (count == 1) parent.Orientation = direction is DockOperation.Left or DockOperation.Right
                ? Orientation.Horizontal : Orientation.Vertical;
        }

        if (source is IDock dock)
        {
            factory.RemoveDockable(dock, false);
            factory.SplitToDock(target, dock, direction);
            factory.CollapseDock(owner);
        }
        else
        {
            IDock created = source is ITool ? factory.CreateToolDock() : factory.CreateDocumentDock();
            if (created is IToolDock toolDock) toolDock.Alignment = direction.ToAlignment();
            created.VisibleDockables = factory.CreateList<IDockable>();
            created.DockGroup = DockGroupValidator.GetEffectiveDockGroup(source);
            factory.MoveDockable(owner, created, source, null);
            factory.SplitToDock(target, created, direction);
        }
        if (focused is not null && factory.FindRoot(focused, static root => root.IsFocusableRoot) is { } destinationRoot)
            factory.SetFocusedDockable(destinationRoot, focused);
    }

    internal static bool Contains(IDockable source, IDockable? content)
    {
        for (IDockable? current = content; current is not null; current = current.Owner)
            if (ReferenceEquals(source, current)) return true;
        return false;
    }

    internal static bool HasOtherContent(IDockable region, IDockable source)
    {
        if (ReferenceEquals(region, source) || region is ISplitter) return false;
        if (region is not IDock dock) return true;
        if (dock.VisibleDockables is not { } children) return false;
        for (int index = 0; index < children.Count; index++)
            if (HasOtherContent(children[index], source)) return true;
        return false;
    }

    private static IDockable? SelectedContent(IDockable source)
    {
        if (source is not IDock dock) return source;
        if (dock.VisibleDockables is not { } children) return null;
        if (dock.ActiveDockable is { } active && children.Contains(active)) return SelectedContent(active);
        foreach (IDockable child in children)
            if (child is not ISplitter && SelectedContent(child) is { } content) return content;
        return null;
    }
}
