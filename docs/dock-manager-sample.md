# DockManagerRulesSample

This sample shows how to subclass `DockManager` to enforce custom docking rules.
It builds a small layout in code and assigns the custom manager to
`DockControl`.

## RulesDockManager

`RulesDockManager` derives from `DockManager` and overrides the validation
methods for tools and documents. Tools are prevented from floating into
separate windows while documents cannot be split vertically.

```csharp
public class RulesDockManager : DockManager
{
    public override bool ValidateTool(ITool sourceTool, IDockable target, DragAction action, DockOperation operation, bool execute)
    {
        if (operation == DockOperation.Window)
            return false;
        return base.ValidateTool(sourceTool, target, action, operation, execute);
    }

    public override bool ValidateDocument(IDocument sourceDocument, IDockable target, DragAction action, DockOperation operation, bool execute)
    {
        if (operation == DockOperation.Top || operation == DockOperation.Bottom)
            return false;
        return base.ValidateDocument(sourceDocument, target, action, operation, execute);
    }
}
```

Assign the manager before showing the window:

```csharp
var dockManager = new RulesDockManager();
var dockControl = new DockControl
{
    DockManager = dockManager
};
```

The remaining layout code matches the `DockCodeOnlySample` and demonstrates the
restricted behaviour when dragging tabs.

## Running the sample

1. Navigate to `samples/DockManagerRulesSample`.
2. Restore and build:
   ```bash
   dotnet build
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

Try dragging a tool to float it or splitting a document vertically. The drag
indicators will not appear because the manager disallows these operations.
