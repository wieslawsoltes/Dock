using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>A detached layout obtained by executing the real docking operation on copied models.</summary>
public sealed class DockSplitPreview
{
    private DockSplitPreview(IDock layout, IDock insertedDock, Dictionary<IDockable, IDockable> copies)
    {
        Layout = layout;
        InsertedDock = insertedDock;
        Copies = copies;
    }

    /// <summary>Gets the projected target layout, including source removal and container collapse.</summary>
    public IDock Layout { get; }
    /// <summary>Gets the resulting pane containing the moved dockable.</summary>
    public IDock InsertedDock { get; }
    /// <summary>Gets mappings from live models to independent preview models.</summary>
    public IReadOnlyDictionary<IDockable, IDockable> Copies { get; }

    /// <summary>Projects a move without changing live models or invoking their lifecycle handlers.</summary>
    /// <param name="source">The resolved tool, document or tab group that execution will move.</param>
    /// <param name="target">The target passed to DockManager.</param>
    /// <param name="operation">The docking operation.</param>
    /// <param name="proportion">Optional global split proportion; NaN retains normal local split sizing.</param>
    /// <returns>A detached projection, or null for unsupported layouts or rejected operations.</returns>
    public static DockSplitPreview? Create(IDockable source, IDockable target, DockOperation operation, double proportion = double.NaN)
    {
        if (operation is DockOperation.None or DockOperation.Window
            || source is IDock and not (IToolDock or IDocumentDock))
            return null;

        var cloner = new DockPreviewCloner();
        var targetRoot = FindLayout(target);
        var sourceRoot = FindLayout(source);
        var layout = cloner.Copy(targetRoot) as IDock;
        if (layout is null || cloner.Copy(sourceRoot) is null
            || !cloner.Copies.TryGetValue(source, out var sourceCopy)
            || !cloner.Copies.TryGetValue(target, out var targetCopy))
            return null;

        // Whole-pane drags move every tab; track a child to locate the resulting pane.
        var movedItem = sourceCopy switch
        {
            IDock { VisibleDockables: { Count: > 0 } children } => children[0],
            IDock => null,
            _ => sourceCopy
        };
        if (movedItem is null) return null;
        var manager = new DockManager(new DockService());
        if (!manager.ValidateDockable(sourceCopy, targetCopy, DragAction.Move, operation, true)
            || movedItem.Owner is not IDock inserted)
            return null;
        if (!double.IsNaN(proportion) && operation != DockOperation.Fill)
            DockSplitProportion.Apply(inserted, proportion);
        return new DockSplitPreview(layout, inserted, cloner.Copies);
    }

    private static IDockable FindLayout(IDockable dockable)
    {
        while (dockable.Owner is IDock owner) dockable = owner;
        return dockable;
    }
}
