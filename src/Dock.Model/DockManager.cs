// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Linq;
using Dock.Model.Controls;

namespace Dock.Model
{
    /// <summary>
    /// Docking manager.
    /// </summary>
    public class DockManager : IDockManager
    {
        /// <inheritdoc/>
        public DockPoint Position { get; set; }

        /// <inheritdoc/>
        public DockPoint ScreenPosition { get; set; }

        internal bool DockIntoDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(DockIntoDockable)}: {sourceDockable.Title} -> {targetDockable.Title}");
            if (sourceDockable.Owner is IDock sourceDockableOwner && targetDockable.Owner is IDock targetDockableOwner)
            {
                if (sourceDockable == targetDockable && sourceDockableOwner == targetDockableOwner)
                {
                    return false;
                }

                if (sourceDockableOwner == targetDockableOwner)
                {
                    if (sourceDockableOwner.Visible.Count == 1)
                    {
                        return false;
                    }

                    switch (action)
                    {
                        case DragAction.Copy:
                            {
                                if (bExecute)
                                {
                                    // TODO: Clone item.
                                }
                                return true;
                            }
                        case DragAction.Move:
                            {
                                if (bExecute)
                                {
                                    if (sourceDockableOwner.Factory is IFactory factory)
                                    {
                                        factory.MoveDockable(sourceDockableOwner, sourceDockable, targetDockable);
                                    }
                                }
                                return true;
                            }
                        case DragAction.Link:
                            {
                                if (bExecute)
                                {
                                    if (sourceDockableOwner.Factory is IFactory factory)
                                    {
                                        factory.SwapDockable(sourceDockableOwner, sourceDockable, targetDockable);
                                    }
                                }
                                return true;
                            }
                    }

                    return false;
                }
                else
                {
                    switch (action)
                    {
                        case DragAction.Copy:
                            {
                                if (bExecute)
                                {
                                    // TODO: Clone item.
                                }
                                return true;
                            }
                        case DragAction.Move:
                            {
                                if (bExecute)
                                {
                                    if (sourceDockableOwner.Factory is IFactory factory)
                                    {
                                        factory.MoveDockable(sourceDockableOwner, targetDockableOwner, sourceDockable, targetDockable);
                                    }
                                }
                                return true;
                            }
                        case DragAction.Link:
                            {
                                if (bExecute)
                                {
                                    if (sourceDockableOwner.Factory is IFactory factory)
                                    {
                                        factory.SwapDockable(sourceDockableOwner, targetDockableOwner, sourceDockable, targetDockable);
                                    }
                                }
                                return true;
                            }
                    }
                    return false;
                }
            }

            return false;
        }

        internal bool DockIntoDock(IDockable sourceDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(DockIntoDock)}: {sourceDockable.Title} -> {targetDock.Title}");
            if (sourceDockable.Owner is IDock sourceDockableOwner)
            {
                if (sourceDockableOwner == targetDock)
                {
                    if (sourceDockableOwner.Visible.Count == 1)
                    {
                        return false;
                    }
                }

                switch (action)
                {
                    case DragAction.Copy:
                        {
                            if (bExecute)
                            {
                                // TODO: Clone item.
                            }
                            return true;
                        }
                    case DragAction.Move:
                        {
                            switch (operation)
                            {
                                case DockOperation.Fill:
                                    {
                                        var targetDockable = targetDock.CurrentDockable;
                                        if (targetDockable == null)
                                        {
                                            targetDockable = targetDock.Visible.LastOrDefault();
                                        }

                                        if (targetDockable == null || sourceDockable == targetDockable)
                                        {
                                            return false;
                                        }

                                        if (sourceDockable == targetDockable && sourceDockableOwner == targetDock)
                                        {
                                            return false;
                                        }

                                        if (bExecute)
                                        {
                                            if (sourceDockableOwner.Factory is IFactory factory)
                                            {
                                                factory.MoveDockable(sourceDockableOwner, targetDock, sourceDockable, targetDockable);
                                            }
                                        }

                                        return true;
                                    }
                                case DockOperation.Left:
                                case DockOperation.Right:
                                case DockOperation.Top:
                                case DockOperation.Bottom:
                                    {
                                        switch (sourceDockable)
                                        {
                                            case ITool tool:
                                                {
                                                    if (bExecute)
                                                    {
                                                        if (targetDock.Factory is IFactory factory)
                                                        {
                                                            factory.RemoveDockable(sourceDockable);

                                                            IDock toolDock = factory.CreateToolDock();
                                                            toolDock.Id = nameof(IToolDock);
                                                            toolDock.Title = nameof(IToolDock);
                                                            toolDock.CurrentDockable = sourceDockable;
                                                            toolDock.Visible = factory.CreateList<IDockable>();
                                                            toolDock.Visible.Add(sourceDockable);

                                                            factory.Split(targetDock, toolDock, operation);
                                                        }
                                                    }
                                                    return true;
                                                }
                                            case IDocument document:
                                                {
                                                    if (bExecute)
                                                    {
                                                        if (targetDock.Factory is IFactory factory)
                                                        {
                                                            IDock documentDock = factory.CreateDocumentDock();
                                                            documentDock.Id = nameof(IDocumentDock);
                                                            documentDock.Title = nameof(IDocumentDock);
                                                            documentDock.CurrentDockable = sourceDockable;
                                                            documentDock.Visible = factory.CreateList<IDockable>();

                                                            factory.MoveDockable(sourceDockableOwner, documentDock, sourceDockable, sourceDockable);

                                                            factory.Split(targetDock, documentDock, operation);
                                                        }
                                                    }
                                                    return true;
                                                }
                                            default:
                                                {
                                                    Logger.Log($"Not supported dockable type {sourceDockable?.GetType().Name} to splitting : {sourceDockable} -> {targetDock}");
                                                    return false;
                                                }
                                        }
                                    }
                                case DockOperation.Window:
                                    {
                                        return DockIntoWindow(sourceDockable, targetDock, action, operation, bExecute);
                                    }
                            }

                            return false;
                        }
                    case DragAction.Link:
                        {
                            var targetDockable = targetDock.CurrentDockable;
                            if (targetDockable == null)
                            {
                                targetDockable = targetDock.Visible.LastOrDefault();
                            }

                            if (targetDockable == null || sourceDockable == targetDockable)
                            {
                                return false;
                            }

                            if (sourceDockable == targetDockable && sourceDockableOwner == targetDock)
                            {
                                return false;
                            }

                            if (bExecute)
                            {
                                if (sourceDockableOwner.Factory is IFactory factory)
                                {
                                    factory.SwapDockable(sourceDockableOwner, targetDock, sourceDockable, targetDockable);
                                }
                            }

                            return true;
                        }
                }
                return false;
            }

            return false;
        }

        internal bool DockIntoWindow(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(DockIntoWindow)}: {sourceDockable.Title} -> {targetDockable.Title}");
            switch (operation)
            {
                case DockOperation.Fill:
                    {
                        if (sourceDockable != targetDockable
                            && sourceDockable.Owner != targetDockable
                            && sourceDockable.Owner is IDock sourceDockableOwner
                            && sourceDockableOwner.Factory is IFactory factory
                            && factory.FindRoot(sourceDockable) is IDock root
                            && root.CurrentDockable is IDock currentDockableRoot)
                        {
                            if (bExecute)
                            {
                                factory.RemoveDockable(sourceDockable);

                                var window = factory.CreateWindowFrom(sourceDockable);
                                if (window != null)
                                {
                                    factory.AddWindow(currentDockableRoot, window);

                                    window.X = ScreenPosition.X;
                                    window.Y = ScreenPosition.Y;
                                    window.Width = 300;
                                    window.Height = 400;
                                    window.Present(false);
                                }
                            }
                            return true;
                        }
                        break;
                    }
            }
            return false;
        }

        internal bool ValidateDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(ValidateDockable)}: {sourceDockable.Title} -> {targetDockable.Title}");
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return false;
                    }
                case IDocument document:
                    {
                        return false;
                    }
                case IToolDock toolDock:
                    {
                        return false;
                    }
                case IDocumentDock documentDock:
                    {
                        return false;
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return false;
                    }
                default:
                    {
                        Logger.Log($"Not supported type {targetDockable?.GetType()}: {sourceDockable} -> {targetDockable}");
                        return false;
                    }
            }
        }

        internal bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(ValidateTool)}: {sourceTool.Title} -> {targetDockable.Title}");
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return DockIntoDockable(sourceTool, tool, action, operation, bExecute);
                    }
                case IDocument document:
                    {
                        return DockIntoDockable(sourceTool, document, action, operation, bExecute);
                    }
                case IToolDock toolDock:
                    {
                        return DockIntoDock(sourceTool, toolDock, action, operation, bExecute);
                    }
                case IDocumentDock documentDock:
                    {
                        return DockIntoDock(sourceTool, documentDock, action, operation, bExecute);
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return DockIntoWindow(sourceTool, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
                        Logger.Log($"Not supported type {targetDockable?.GetType()}: {sourceTool} -> {targetDockable}");
                        return false;
                    }
            }
        }

        internal bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(ValidateDocument)}: {sourceDocument.Title} -> {targetDockable.Title}");
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return false;
                    }
                case IDocument document:
                    {
                        return DockIntoDockable(sourceDocument, document, action, operation, bExecute);
                    }
                case IToolDock toolDock:
                    {
                        return false;
                    }
                case IDocumentDock documentDock:
                    {
                        return DockIntoDock(sourceDocument, documentDock, action, operation, bExecute);
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return DockIntoWindow(sourceDocument, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
                        Logger.Log($"Not supported type {targetDockable?.GetType()}: {sourceDocument} -> {targetDockable}");
                        return false;
                    }
            }
        }

        internal bool ValidateToolDock(IToolDock sourceToolDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(ValidateToolDock)}: {sourceToolDock.Title} -> {targetDockable.Title}");
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return false;
                    }
                case IDocument document:
                    {
                        return false;
                    }
                case IToolDock toolDock:
                    {
                        if (sourceToolDock == toolDock)
                        {
                            Logger.Log($"{nameof(ValidateToolDock)}: {nameof(sourceToolDock)} == {toolDock}");
                            return false;
                        }

                        switch (operation)
                        {
                            case DockOperation.Fill:
                                {
                                    foreach (var dockable in sourceToolDock.Visible.ToList())
                                    {
                                        if (DockIntoDock(dockable, toolDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                            case DockOperation.Window:
                                {
                                    return DockIntoWindow(sourceToolDock, targetDockable, action, operation, bExecute);
                                }
                            default:
                                {
                                    var toMove = sourceToolDock.Visible.ToList();

                                    if (toMove.Count == 1)
                                    {
                                        if (DockIntoDock(toMove.First(), toolDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (DockIntoDock(toMove.First(), toolDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }

                                        foreach (var dockable in toMove.Skip(1))
                                        {
                                            if (DockIntoDockable(dockable, toMove.First(), action, DockOperation.Fill, bExecute) == false)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    return true;
                                }
                        }
                    }
                case IDocumentDock documentDock:
                    {
                        if (sourceToolDock == documentDock)
                        {
                            Logger.Log($"{nameof(ValidateToolDock)}: {nameof(sourceToolDock)} == {documentDock}");
                            return false;
                        }

                        switch (operation)
                        {
                            case DockOperation.Fill:
                                {
                                    foreach (var dockable in sourceToolDock.Visible.ToList())
                                    {
                                        if (DockIntoDock(dockable, documentDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                            case DockOperation.Window:
                                {
                                    return DockIntoWindow(sourceToolDock, targetDockable, action, operation, bExecute);
                                }
                            default:
                                {
                                    var toMove = sourceToolDock.Visible.ToList();

                                    if (toMove.Count == 1)
                                    {
                                        if (DockIntoDock(toMove.First(), documentDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (DockIntoDock(toMove.First(), documentDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }

                                        foreach (var dockable in toMove.Skip(1))
                                        {
                                            if (DockIntoDockable(dockable, toMove.First(), action, DockOperation.Fill, bExecute) == false)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    return true;
                                }
                        }
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return DockIntoWindow(sourceToolDock, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
                        Logger.Log($"Not supported type {targetDockable?.GetType()}: {sourceToolDock} -> {targetDockable}");
                        return false;
                    }
            }
        }

        internal bool ValidateDocumentDock(IDocumentDock sourceDocumentDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(ValidateToolDock)}: {sourceDocumentDock.Title} -> {targetDockable.Title}");
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return false;
                    }
                case IDocument document:
                    {
                        return false;
                    }
                case IToolDock toolDock:
                    {
                        if (sourceDocumentDock == toolDock)
                        {
                            Logger.Log($"{nameof(ValidateDocumentDock)}: {nameof(sourceDocumentDock)} == {toolDock}");
                            return false;
                        }

                        switch (operation)
                        {
                            case DockOperation.Fill:
                                {
                                    foreach (var dockable in sourceDocumentDock.Visible.ToList())
                                    {
                                        if (DockIntoDock(dockable, toolDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                            case DockOperation.Window:
                                {
                                    return DockIntoWindow(sourceDocumentDock, targetDockable, action, operation, bExecute);
                                }
                            default:
                                {
                                    var toMove = sourceDocumentDock.Visible.ToList();

                                    if (toMove.Count == 1)
                                    {
                                        if (DockIntoDock(toMove.First(), toolDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (DockIntoDock(toMove.First(), toolDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }

                                        foreach (var dockable in toMove.Skip(1))
                                        {
                                            if (DockIntoDockable(dockable, toMove.First(), action, DockOperation.Fill, bExecute) == false)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    return true;
                                }
                        }
                    }
                case IDocumentDock documentDock:
                    {
                        if (sourceDocumentDock == documentDock)
                        {
                            Logger.Log($"{nameof(ValidateDocumentDock)}: {nameof(sourceDocumentDock)} == {documentDock}");
                            return false;
                        }

                        switch (operation)
                        {
                            case DockOperation.Fill:
                                {
                                    foreach (var dockable in sourceDocumentDock.Visible.ToList())
                                    {
                                        if (DockIntoDock(dockable, documentDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                            case DockOperation.Window:
                                {
                                    return DockIntoWindow(sourceDocumentDock, targetDockable, action, operation, bExecute);
                                }
                            default:
                                {
                                    var toMove = sourceDocumentDock.Visible.ToList();

                                    if (toMove.Count == 1)
                                    {
                                        if (DockIntoDock(toMove.First(), documentDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (DockIntoDock(toMove.First(), documentDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }

                                        foreach (var dockable in toMove.Skip(1))
                                        {
                                            if (DockIntoDockable(dockable, toMove.First(), action, DockOperation.Fill, bExecute) == false)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    return true;
                                }
                        }
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return DockIntoWindow(sourceDocumentDock, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
                        Logger.Log($"Not supported type {targetDockable?.GetType()}: {sourceDocumentDock} -> {targetDockable}");
                        return false;
                    }
            }
        }

        internal bool ValidateProportionalDock(IProportionalDock sourceProportionalDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(ValidateProportionalDock)}: {sourceProportionalDock.Title} -> {targetDockable.Title}");
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return false;
                    }
                case IDocument document:
                    {
                        return false;
                    }
                case IToolDock toolDock:
                    {
                        return false;
                    }
                case IDocumentDock documentDock:
                    {
                        return false;
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return false;
                    }
                default:
                    {
                        Logger.Log($"Not supported type {targetDockable?.GetType()}: {sourceProportionalDock} -> {targetDockable}");
                        return false;
                    }
            }
        }

        internal bool ValidateRootDock(IRootDock sourceRootDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            Logger.Log($"{nameof(ValidateRootDock)}: {sourceRootDock.Title} -> {targetDockable.Title}");
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return false;
                    }
                case IDocument document:
                    {
                        return false;
                    }
                case IToolDock toolDock:
                    {
                        return false;
                    }
                case IDocumentDock documentDock:
                    {
                        return false;
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return false;
                    }
                default:
                    {
                        Logger.Log($"Not supported type {targetDockable?.GetType()}: {sourceRootDock} -> {targetDockable}");
                        return false;
                    }
            }
        }

        /// <inheritdoc/>
        public bool Validate(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (sourceDockable)
            {
                case ITool tool:
                    {
                        return ValidateTool(tool, targetDockable, action, operation, bExecute);
                    }
                case IDocument document:
                    {
                        return ValidateDocument(document, targetDockable, action, operation, bExecute);
                    }
                case IToolDock toolDock:
                    {
                        return ValidateToolDock(toolDock, targetDockable, action, operation, bExecute);
                    }
                case IDocumentDock documentDock:
                    {
                        return ValidateDocumentDock(documentDock, targetDockable, action, operation, bExecute);
                    }
                case IProportionalDock proportionalDock:
                    {
                        return ValidateProportionalDock(proportionalDock, targetDockable, action, operation, bExecute);
                    }
                case IRootDock rootDock:
                    {
                        return ValidateRootDock(rootDock, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
                        Logger.Log($"Not supported type {sourceDockable?.GetType()}: {sourceDockable} -> {targetDockable}");
                        return false;
                    }
            }
        }
    }
}
