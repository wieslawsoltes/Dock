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

        internal bool DockDockIntoDock(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            var visible = sourceDock.Visible.ToList();
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
                    if (DockDockableIntoDockable(dockable, visible.First(), action, DockOperation.Fill, bExecute) == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool DockDockableIntoDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            if (sourceDockable.Owner is IDock sourceDockableOwner && targetDockable.Owner is IDock targetDockableOwner)
            {
                //if (sourceDockable == targetDockable && sourceDockableOwner == targetDockableOwner)
                //{
                //    return false;
                //}

                if (sourceDockableOwner == targetDockableOwner)
                {
                    //if (sourceDockableOwner.Visible.Count == 1)
                    //{
                    //    return false;
                    //}

                    return DockDockable(sourceDockable, sourceDockableOwner, targetDockable, action, bExecute);
                }
                else
                {
                    return DockDockable(sourceDockable, sourceDockableOwner, targetDockable, targetDockableOwner, action, bExecute);
                }
            }

            return false;
        }

        internal bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDockable targetDockable, DragAction action, bool bExecute)
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

        internal bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDockable targetDockable, IDock targetDockableOwner, DragAction action, bool bExecute)
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

        internal bool DockDockableIntoDock(IDockable sourceDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            if (sourceDockable.Owner is IDock sourceDockableOwner)
            {
                //if (sourceDockableOwner == targetDock)
                //{
                //    if (sourceDockableOwner.Visible.Count == 1)
                //    {
                //        return false;
                //    }
                //}

                return DockDockableIntoDock(sourceDockable, sourceDockableOwner, targetDock, action, operation, bExecute);
            }

            return false;
        }

        internal bool DockDockableIntoDock(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
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
                        return DockDockable(sourceDockable, sourceDockableOwner, targetDock, action, operation, bExecute);
                    }
                case DragAction.Link:
                    {
                        return SwapDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute);
                    }
            }
            return false;
        }

        internal bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (operation)
            {
                case DockOperation.Fill:
                    {
                        return MoveDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute);
                    }
                case DockOperation.Left:
                case DockOperation.Right:
                case DockOperation.Top:
                case DockOperation.Bottom:
                    {
                        return SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute);
                    }
                case DockOperation.Window:
                    {
                        return DockDockableIntoWindow(sourceDockable, targetDock, action, operation, bExecute);
                    }
            }

            return false;
        }

        internal bool SplitDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
        {
            switch (sourceDockable)
            {
                case ITool tool:
                    {
                        if (bExecute)
                        {
                            SplitRemoveDockable(sourceDockable, targetDock, operation);
                        }
                        return true;
                    }
                case IDocument document:
                    {
                        if (bExecute)
                        {
                            SplitMoveDockable(sourceDockable, sourceDockableOwner, targetDock, operation);
                        }
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        internal void SplitRemoveDockable(IDockable sourceDockable, IDock targetDock, DockOperation operation)
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

        internal void SplitMoveDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation)
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

        internal bool MoveDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
        {
            var targetDockable = targetDock.CurrentDockable;
            if (targetDockable == null)
            {
                targetDockable = targetDock.Visible.LastOrDefault();
            }

            if (targetDockable == null)
            {
                return false;
            }

            //if (sourceDockable == targetDockable)
            //{
            //    return false;
            //}

            //if (sourceDockableOwner == targetDock)
            //{
            //    return false;
            //}

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
            var targetDockable = targetDock.CurrentDockable;
            if (targetDockable == null)
            {
                targetDockable = targetDock.Visible.LastOrDefault();
            }

            if (targetDockable == null)
            {
                return false;
            }

            //if (sourceDockable == targetDockable)
            //{
            //    return false;
            //}

            //if (sourceDockableOwner == targetDock)
            //{
            //    return false;
            //}

            if (bExecute)
            {
                if (sourceDockableOwner.Factory is IFactory factory)
                {
                    factory.SwapDockable(sourceDockableOwner, targetDock, sourceDockable, targetDockable);
                }
            }

            return true;
        }

        internal bool DockDockableIntoWindow(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (operation)
            {
                case DockOperation.Fill:
                    {
                        if (sourceDockable == targetDockable)
                        {
                            return false;
                        }

                        if (sourceDockable.Owner != targetDockable
                            && sourceDockable.Owner is IDock sourceDockableOwner
                            && sourceDockableOwner.Factory is IFactory factory
                            && factory.FindRoot(sourceDockable) is IDock root
                            && root.CurrentDockable is IDock targetWindowOwner)
                        {
                            if (bExecute)
                            {
                                DockDockableIntoWindow(sourceDockable, targetWindowOwner);
                            }
                            return true;
                        }
                        break;
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

        internal bool DockDockable(IDock sourceDock, IDockable targetDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (operation)
            {
                case DockOperation.Fill:
                    {
                        return DockDockableIntoDockVisible(sourceDock, targetDock, action, operation, bExecute);
                    }
                case DockOperation.Window:
                    {
                        return DockDockableIntoWindow(sourceDock, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
                        return DockDockIntoDock(sourceDock, targetDock, action, operation, bExecute);
                    }
            }
        }

        internal bool DockDockableIntoDockVisible(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            foreach (var dockable in sourceDock.Visible.ToList())
            {
                if (DockDockableIntoDock(dockable, targetDock, action, operation, bExecute) == false)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return DockDockableIntoDockable(sourceTool, tool, action, operation, bExecute);
                    }
                case IDocument document:
                    {
                        return DockDockableIntoDockable(sourceTool, document, action, operation, bExecute);
                    }
                case IToolDock toolDock:
                    {
                        return DockDockableIntoDock(sourceTool, toolDock, action, operation, bExecute);
                    }
                case IDocumentDock documentDock:
                    {
                        return DockDockableIntoDock(sourceTool, documentDock, action, operation, bExecute);
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return DockDockableIntoWindow(sourceTool, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        internal bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDockable)
            {
                case ITool tool:
                    {
                        return false;
                    }
                case IDocument document:
                    {
                        return DockDockableIntoDockable(sourceDocument, document, action, operation, bExecute);
                    }
                case IToolDock toolDock:
                    {
                        return false;
                    }
                case IDocumentDock documentDock:
                    {
                        return DockDockableIntoDock(sourceDocument, documentDock, action, operation, bExecute);
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return DockDockableIntoWindow(sourceDocument, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        internal bool ValidateDock(IDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
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
                        if (sourceDock == toolDock)
                        {
                            return false;
                        }
                        return DockDockable(sourceDock, targetDockable, toolDock, action, operation, bExecute);
                    }
                case IDocumentDock documentDock:
                    {
                        if (sourceDock == documentDock)
                        {
                            return false;
                        }
                        return DockDockable(sourceDock, targetDockable, documentDock, action, operation, bExecute);
                    }
                case IProportionalDock proportionalDock:
                    {
                        return false;
                    }
                case IRootDock rootDock:
                    {
                        return DockDockableIntoWindow(sourceDock, targetDockable, action, operation, bExecute);
                    }
                default:
                    {
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
                        return ValidateDock(toolDock, targetDockable, action, operation, bExecute);
                    }
                case IDocumentDock documentDock:
                    {
                        return ValidateDock(documentDock, targetDockable, action, operation, bExecute);
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
                        return false;
                    }
            }
        }
    }
}
