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

        internal bool MoveDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
        {
            if (sourceDockableOwner == targetDock)
            {
                if (targetDock.VisibleDockables.Count == 1)
                {
                    return false;
                }
            }
            var targetDockable = targetDock.ActiveDockable;
            if (targetDockable == null)
            {
                targetDockable = targetDock.VisibleDockables.LastOrDefault();
                if (targetDockable == null)
                {
                    if (bExecute)
                    {
                        if (sourceDockableOwner.Factory is IFactory factory)
                        {
                            factory.MoveDockable(sourceDockableOwner, targetDock, sourceDockable, null);
                        }
                    }
                    return true;
                }
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

        internal bool SwapDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
        {
            var targetDockable = targetDock.ActiveDockable;
            if (targetDockable == null)
            {
                targetDockable = targetDock.VisibleDockables.LastOrDefault();
                if (targetDockable == null)
                {
                    return false;
                }
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

        internal void SplitRemoveDockable(IDockable sourceDockable, IDock targetDock, DockOperation operation)
        {
            if (targetDock.Factory is IFactory factory)
            {
                factory.RemoveDockable(sourceDockable);
                IDock toolDock = factory.CreateToolDock();
                toolDock.Id = nameof(IToolDock);
                toolDock.Title = nameof(IToolDock);
                toolDock.ActiveDockable = sourceDockable;
                toolDock.VisibleDockables = factory.CreateList<IDockable>();
                toolDock.VisibleDockables.Add(sourceDockable);
                factory.Split(targetDock, toolDock, operation);
            }
        }

        internal void SplitMoveDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation)
        {
            if (targetDock.Factory is IFactory factory)
            {
                IDock documentDock = factory.CreateDocumentDock();
                documentDock.Id = nameof(IDocumentDock);
                documentDock.Title = nameof(IDocumentDock);
                documentDock.ActiveDockable = sourceDockable;
                documentDock.VisibleDockables = factory.CreateList<IDockable>();
                factory.MoveDockable(sourceDockableOwner, documentDock, sourceDockable, sourceDockable);
                factory.Split(targetDock, documentDock, operation);
            }
        }

        internal bool SplitDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
        {
            switch (sourceDockable)
            {
                case ITool _:
                    if (sourceDockableOwner == targetDock)
                    {
                        if (targetDock.VisibleDockables.Count == 1)
                        {
                            return false;
                        }
                    }
                    if (bExecute)
                    {
                        SplitRemoveDockable(sourceDockable, targetDock, operation);
                    }
                    return true;
                case IDocument _:
                    if (sourceDockableOwner == targetDock)
                    {
                        if (targetDock.VisibleDockables.Count == 1)
                        {
                            return false;
                        }
                    }
                    if (bExecute)
                    {
                        SplitMoveDockable(sourceDockable, sourceDockableOwner, targetDock, operation);
                    }
                    return true;
                default:
                    return false;
            }
        }

        internal bool DockDockableIntoWindow(IDockable sourceDockable, IDockable targetDockable, DockOperation operation, bool bExecute)
        {
            switch (operation)
            {
                case DockOperation.Window:
                    if (sourceDockable == targetDockable)
                    {
                        return false;
                    }
                    if (sourceDockable.Owner is IDock sourceDockableOwner)
                    {
                        //if (sourceDockableOwner is IDocumentDock)
                        //{
                        //    if (sourceDockableOwner.VisibleDockables.Count == 1)
                        //    {
                        //        return false;
                        //    }
                        //}
                        if (sourceDockableOwner.Factory is IFactory factory)
                        {
                            if (factory.FindRoot(sourceDockable) is IDock root && root.ActiveDockable is IDock targetWindowOwner)
                            {
                                if (bExecute)
                                {
                                    DockDockableIntoWindow(sourceDockable, targetWindowOwner);
                                }
                                return true;
                            }
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }

        internal bool DockDockableIntoDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, bool bExecute)
        {
            if (sourceDockable.Owner is IDock sourceDockableOwner && targetDockable.Owner is IDock targetDockableOwner)
            {
                if (sourceDockableOwner == targetDockableOwner)
                {
                    return DockDockable(sourceDockable, sourceDockableOwner, targetDockable, action, bExecute);
                }
                else
                {
                    return DockDockable(sourceDockable, sourceDockableOwner, targetDockable, targetDockableOwner, action, bExecute);
                }
            }
            return false;
        }

        internal void DockDockableIntoWindow(IDockable sourceDockable, IDock targetWindowOwner)
        {
            if (targetWindowOwner.Factory is IFactory factory)
            {
                factory.RemoveDockable(sourceDockable);
                var window = factory.CreateWindowFrom(sourceDockable);
                if (window != null)
                {
                    factory.AddWindow(targetWindowOwner, window);

                    window.X = ScreenPosition.X;
                    window.Y = ScreenPosition.Y;
                    window.Width = 300;
                    window.Height = 400;
                    window.Present(false);
                }
            }
        }

        internal bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDockable targetDockable, DragAction action, bool bExecute)
        {
            switch (action)
            {
                case DragAction.Copy:
                    return false;
                case DragAction.Move:
                    if (bExecute)
                    {
                        if (sourceDockableOwner.Factory is IFactory factory)
                        {
                            factory.MoveDockable(sourceDockableOwner, sourceDockable, targetDockable);
                        }
                    }
                    return true;
                case DragAction.Link:
                    if (bExecute)
                    {
                        if (sourceDockableOwner.Factory is IFactory factory)
                        {
                            factory.SwapDockable(sourceDockableOwner, sourceDockable, targetDockable);
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }

        internal bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDockable targetDockable, IDock targetDockableOwner, DragAction action, bool bExecute)
        {
            switch (action)
            {
                case DragAction.Copy:
                    return false;
                case DragAction.Move:
                    if (bExecute)
                    {
                        if (sourceDockableOwner.Factory is IFactory factory)
                        {
                            factory.MoveDockable(sourceDockableOwner, targetDockableOwner, sourceDockable, targetDockable);
                        }
                    }
                    return true;
                case DragAction.Link:
                    if (bExecute)
                    {
                        if (sourceDockableOwner.Factory is IFactory factory)
                        {
                            factory.SwapDockable(sourceDockableOwner, targetDockableOwner, sourceDockable, targetDockable);
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }

        internal bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
        {
            switch (operation)
            {
                case DockOperation.Fill:
                    return MoveDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute);
                case DockOperation.Left:
                case DockOperation.Right:
                case DockOperation.Top:
                case DockOperation.Bottom:
                    return SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute);
                case DockOperation.Window:
                    return DockDockableIntoWindow(sourceDockable, targetDock, operation, bExecute);
                default:
                    return false;
            }
        }

        internal bool DockDockableIntoDock(IDockable sourceDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            if (sourceDockable.Owner is IDock sourceDockableOwner)
            {
                return DockDockableIntoDock(sourceDockable, sourceDockableOwner, targetDock, action, operation, bExecute);
            }
            return false;
        }

        internal bool DockDockableIntoDock(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (action)
            {
                case DragAction.Copy:
                    return false;
                case DragAction.Move:
                    return DockDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute);
                case DragAction.Link:
                    return SwapDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute);
                default:
                    return false;
            }
        }

        internal bool DockDockableIntoDockVisible(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            var visible = sourceDock.VisibleDockables.ToList();
            foreach (var dockable in visible)
            {
                if (DockDockableIntoDock(dockable, targetDock, action, operation, bExecute) == false)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool DockDockIntoDock(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            var visible = sourceDock.VisibleDockables.ToList();
            if (visible.Count == 1)
            {
                if (DockDockableIntoDock(visible.First(), targetDock, action, operation, bExecute) == false)
                {
                    return false;
                }
            }
            else
            {
                if (DockDockableIntoDock(visible.First(), targetDock, action, operation, bExecute) == false)
                {
                    return false;
                }
                foreach (var dockable in visible.Skip(1))
                {
                    if (DockDockableIntoDockable(dockable, visible.First(), action, bExecute) == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool DockDockable(IDock sourceDock, IDockable targetDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (operation)
            {
                case DockOperation.Fill:
                    return DockDockableIntoDockVisible(sourceDock, targetDock, action, operation, bExecute);
                case DockOperation.Window:
                    return DockDockableIntoWindow(sourceDock, targetDockable, operation, bExecute);
                default:
                    return DockDockIntoDock(sourceDock, targetDock, action, operation, bExecute);
            }
        }

        /// <inheritdoc/>
        public bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDockable)
            {
                case IRootDock _:
                    return DockDockableIntoWindow(sourceTool, targetDockable, operation, bExecute);
                case IToolDock toolDock:
                    return DockDockableIntoDock(sourceTool, toolDock, action, operation, bExecute);
                case IDocumentDock documentDock:
                    return DockDockableIntoDock(sourceTool, documentDock, action, operation, bExecute);
                case IPinDock pinDock:
                    return DockDockableIntoDock(sourceTool, pinDock, action, operation, bExecute);
                case ITool tool:
                    return DockDockableIntoDockable(sourceTool, tool, action, bExecute);
                case IDocument document:
                    return DockDockableIntoDockable(sourceTool, document, action, bExecute);
                default:
                    return false;
            }
        }

        /// <inheritdoc/>
        public bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDockable)
            {
                case IRootDock _:
                    return DockDockableIntoWindow(sourceDocument, targetDockable, operation, bExecute);
                case IDocumentDock documentDock:
                    return DockDockableIntoDock(sourceDocument, documentDock, action, operation, bExecute);
                case IDocument document:
                    return DockDockableIntoDockable(sourceDocument, document, action, bExecute);
                default:
                    return false;
            }
        }

        /// <inheritdoc/>
        public bool ValidateDock(IDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDockable)
            {
                case IRootDock _:
                    return DockDockableIntoWindow(sourceDock, targetDockable, operation, bExecute);
                case IToolDock toolDock:
                    if (sourceDock == toolDock)
                    {
                        return false;
                    }
                    return DockDockable(sourceDock, targetDockable, toolDock, action, operation, bExecute);
                case IDocumentDock documentDock:
                    if (sourceDock == documentDock)
                    {
                        return false;
                    }
                    return DockDockable(sourceDock, targetDockable, documentDock, action, operation, bExecute);
                case IPinDock pinDock:
                    if (sourceDock == pinDock)
                    {
                        return false;
                    }
                    return DockDockable(sourceDock, targetDockable, pinDock, action, operation, bExecute);
                default:
                    return false;
            }
        }

        /// <inheritdoc/>
        public bool ValidateDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (sourceDockable)
            {
                case IToolDock toolDock:
                    return ValidateDock(toolDock, targetDockable, action, operation, bExecute);
                case IDocumentDock documentDock:
                    return ValidateDock(documentDock, targetDockable, action, operation, bExecute);
                case IPinDock pinDock:
                    return ValidateDock(pinDock, targetDockable, action, operation, bExecute);
                case ITool tool:
                    return ValidateTool(tool, targetDockable, action, operation, bExecute);
                case IDocument document:
                    return ValidateDocument(document, targetDockable, action, operation, bExecute);
                default:
                    return false;
            }
        }
    }
}
