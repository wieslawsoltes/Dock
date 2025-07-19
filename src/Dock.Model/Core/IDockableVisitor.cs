namespace Dock.Model.Core;

public interface IDockableVisitor
{
    bool VisitTool(ITool tool, IDockable target, DragAction action, DockOperation op, bool execute);
    bool VisitDocument(IDocument document, IDockable target, DragAction action, DockOperation op, bool execute);
    bool VisitDock(IDock dock, IDockable target, DragAction action, DockOperation op, bool execute);
    // extend for other dockable types
}

