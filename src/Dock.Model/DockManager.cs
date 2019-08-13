﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
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

        internal bool DockDockableIntoDock(IDockable sourceDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
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
                                                    return false;
                                                }
                                        }
                                    }
                                case DockOperation.Window:
                                    {
                                        return DockDockableIntoWindow(sourceDockable, targetDock, action, operation, bExecute);
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

        internal bool DockDockableIntoWindow(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
        {
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

                        switch (operation)
                        {
                            case DockOperation.Fill:
                                {
                                    foreach (var dockable in sourceDock.Visible.ToList())
                                    {
                                        if (DockDockableIntoDock(dockable, toolDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                            case DockOperation.Window:
                                {
                                    return DockDockableIntoWindow(sourceDock, targetDockable, action, operation, bExecute);
                                }
                            default:
                                {
                                    return DockDockIntoDock(sourceDock, toolDock, action, operation, bExecute);
                                }
                        }
                    }
                case IDocumentDock documentDock:
                    {
                        if (sourceDock == documentDock)
                        {
                            return false;
                        }

                        switch (operation)
                        {
                            case DockOperation.Fill:
                                {
                                    foreach (var dockable in sourceDock.Visible.ToList())
                                    {
                                        if (DockDockableIntoDock(dockable, documentDock, action, operation, bExecute) == false)
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                            case DockOperation.Window:
                                {
                                    return DockDockableIntoWindow(sourceDock, targetDockable, action, operation, bExecute);
                                }
                            default:
                                {
                                    return DockDockIntoDock(sourceDock, documentDock, action, operation, bExecute);
                                }
                        }
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
