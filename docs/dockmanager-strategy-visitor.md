# DockManager strategy and visitor patterns

`DockManager` relies on two extension points to keep the drag and drop logic decoupled from the dockable types:
`IDockOperationStrategy` and `IDockableVisitor`. Understanding these abstractions
helps when implementing custom operations or extending the model with new
classes.

## IDockOperationStrategy

An `IDockOperationStrategy` handles a particular dock operation such as
`Fill`, `Left` or `Window`. `DockManager` holds a list of strategies and
selects one based on the current `DockOperation` value. Each strategy can
validate and execute the operation.

```csharp
public interface IDockOperationStrategy
{
    bool CanExecute(DockOperation operation);
    bool Validate(IDockable source, IDockable target, DragAction action);
    void Execute(IDockable source, IDockable target, DragAction action);
}
```

To add a new operation:

1. Define a constant in `DockOperation` for the new action.
2. Implement `IDockOperationStrategy` and register the instance with the
   `DockManager` before a drag begins.
3. Provide validation logic in `Validate` and modify the layout in `Execute`.

## IDockableVisitor

The visitor pattern keeps algorithms independent from the dockable hierarchy.
`IDockableVisitor` exposes one method per dockable type. `DockManager` calls the
appropriate method based on the runtime type of the dragged or target item.

```csharp
public interface IDockableVisitor
{
    void Visit(IRootDock root);
    void Visit(IToolDock dock);
    void Visit(IDocumentDock dock);
    void Visit(ITool tool);
    void Visit(IDocument document);
    // implement methods for custom types here
}
```

When introducing a new dockable type:

1. Add an overload to `IDockableVisitor`.
2. Implement `Accept` on the dockable so it invokes the visitor method.
3. Update visitor implementations to handle the new overload.

This approach lets strategies operate on arbitrary hierarchies without large
`switch` or `if` blocks.

## Putting it together

`DockManager` first asks each strategy whether it can handle the requested
operation. The selected strategy then visits the source and target dockables to
execute the layout changes. By adding strategies and visitor methods you can
introduce entirely new behaviours while keeping the core manager unchanged.

For more information about `DockManager` see the
[DockManager guide](dock-manager-guide.md).
