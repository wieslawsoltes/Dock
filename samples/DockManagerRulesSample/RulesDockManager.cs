using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace DockManagerRulesSample;

public class RulesDockManager : DockManager
{
    public override bool ValidateTool(ITool sourceTool, IDockable target, DragAction action, DockOperation operation, bool execute)
    {
        if (operation == DockOperation.Window)
        {
            return false;
        }

        return base.ValidateTool(sourceTool, target, action, operation, execute);
    }

    public override bool ValidateDocument(IDocument sourceDocument, IDockable target, DragAction action, DockOperation operation, bool execute)
    {
        if (operation == DockOperation.Top || operation == DockOperation.Bottom)
        {
            return false;
        }

        return base.ValidateDocument(sourceDocument, target, action, operation, execute);
    }
}
