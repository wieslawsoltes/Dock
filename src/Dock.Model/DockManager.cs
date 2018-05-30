// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.ObjectModel;
using Dock.Model.Controls;

namespace Dock.Model
{
    public class DockManager
    {
        public DockPoint Position { get; set; }

        public DockPoint ScreenPosition { get; set; }

        public bool DockBetweenTabs(ITab sourceTab, ITab targetTab, DragAction action, DockOperation operation, bool bExecute)
        {
            Console.WriteLine($"{nameof(DockBetweenTabs)}: {sourceTab.Title} -> {targetTab.Title}");

            if (sourceTab.Parent is ITabDock sourceTabParent && targetTab.Parent is ITabDock targetTabParent)
            {
                if (sourceTabParent == targetTabParent)
                {
                    int sourceIndex = sourceTabParent.Views.IndexOf(sourceTab);
                    int targetIndex = sourceTabParent.Views.IndexOf(targetTab);
                    if (sourceIndex >= 0 && targetIndex >= 0)
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
                                        if (sourceTabParent.Factory is IDockFactory factory)
                                        {
                                            factory.MoveView(sourceTabParent, sourceIndex, targetIndex);
                                        }
                                    }
                                    return true;
                                }
                            case DragAction.Link:
                                {
                                    if (bExecute)
                                    {
                                        if (sourceTabParent.Factory is IDockFactory factory)
                                        {
                                            factory.SwapView(sourceTabParent, sourceIndex, targetIndex);
                                        }
                                    }
                                    return true;
                                }
                        }
                    }
                    return false;
                }
                else
                {
                    int sourceIndex = sourceTabParent.Views.IndexOf(sourceTab);
                    int targetIndex = targetTabParent.Views.IndexOf(targetTab);
                    if (sourceIndex >= 0 && targetIndex >= 0)
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
                                        if (sourceTabParent.Factory is IDockFactory factory)
                                        {
                                            factory.MoveView(sourceTabParent, targetTabParent, sourceIndex, targetIndex);
                                        }
                                    }
                                    return true;
                                }
                            case DragAction.Link:
                                {
                                    if (bExecute)
                                    {
                                        if (sourceTabParent.Factory is IDockFactory factory)
                                        {
                                            factory.SwapView(sourceTabParent, targetTabParent, sourceIndex, targetIndex);
                                        }
                                    }
                                    return true;
                                }
                        }
                    }
                    return false;
                }
            }

            return false;
        }

        public bool DockIntoTab(ITab sourceTab, ITabDock targetTabParent, DragAction action, DockOperation operation, bool bExecute)
        {
            Console.WriteLine($"{nameof(DockIntoTab)}: {sourceTab.Title} -> {targetTabParent.Title}");

            if (sourceTab.Parent is ITabDock sourceTabParent /* && sourceTabParent != targetTabParent */)
            {
                bool isSameParent = sourceTabParent == targetTabParent;
                int sourceIndex = sourceTabParent.Views.IndexOf(sourceTab);
                int targetIndex = isSameParent ? targetTabParent.Views.Count - 1 : targetTabParent.Views.Count;

                if (sourceIndex >= 0 && targetIndex >= 0)
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
                                    switch (operation)
                                    {
                                        case DockOperation.Fill:
                                            {
                                                if (!(isSameParent && sourceIndex == targetIndex))
                                                {
                                                    if (sourceTabParent.Factory is IDockFactory factory)
                                                    {
                                                        factory.MoveView(sourceTabParent, targetTabParent, sourceIndex, targetIndex);
                                                    }
                                                }
                                                return true;
                                            }
                                        case DockOperation.Left:
                                        case DockOperation.Right:
                                        case DockOperation.Top:
                                        case DockOperation.Bottom:
                                            {
                                                if (targetTabParent.Factory is IDockFactory factory)
                                                {
                                                    factory.Remove(sourceTab);

                                                    switch (sourceTab)
                                                    {
                                                        case IToolTab toolTab:
                                                            {
                                                                IDock tool = new ToolDock
                                                                {
                                                                    Id = nameof(ToolDock),
                                                                    Title = nameof(ToolDock),
                                                                    CurrentView = sourceTab,
                                                                    Views = new ObservableCollection<IDock> { sourceTab }
                                                                };
                                                                factory.Split(targetTabParent, tool, operation);
                                                                return true;
                                                            }

                                                        case IDocumentTab documentTab:
                                                            {
                                                                IDock tool = new DocumentDock
                                                                {
                                                                    Id = nameof(DocumentDock),
                                                                    Title = nameof(DocumentDock),
                                                                    CurrentView = sourceTab,
                                                                    Views = new ObservableCollection<IDock> { sourceTab }
                                                                };
                                                                factory.Split(targetTabParent, tool, operation);
                                                                return true;
                                                            }
                                                        default:
                                                            {
                                                                Console.WriteLine($"Not supported tab type {sourceTab.GetType().Name} to splitting : {sourceTab} -> {targetTabParent}");
                                                                return false;
                                                            }
                                                    }
                                                }
                                                return false;
                                            }
                                        case DockOperation.Window:
                                            {
                                                // TODO:
                                            }
                                            break;
                                    }
                                }
                                return true;
                            }
                        case DragAction.Link:
                            {
                                if (bExecute)
                                {
                                    if (sourceTabParent.Factory is IDockFactory factory)
                                    {
                                        factory.SwapView(sourceTabParent, targetTabParent, sourceIndex, targetIndex);
                                    }
                                }
                                return true;
                            }
                    }
                }

                return false;
            }

            return false;
        }

        public bool MoveIntoWindow(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            Console.WriteLine($"{nameof(MoveIntoWindow)}: {sourceDock.Title} -> {targetDock.Title}");

            switch (operation)
            {
                case DockOperation.Fill:
                    {
                        int sourceIndex = sourceDock.Parent.Views.IndexOf(sourceDock);
                        if (sourceIndex >= 0
                            && sourceDock != targetDock && sourceDock.Parent != targetDock
                            && sourceDock.Factory is IDockFactory factory
                            && factory.FindRootLayout(sourceDock) is IDock rootLayout && rootLayout.CurrentView != null)
                        {
                            if (bExecute)
                            {
                                factory.RemoveView(sourceDock.Parent, sourceIndex);

                                var window = factory.CreateWindowFrom(sourceDock);
                                if (window != null)
                                {
                                    factory.AddWindow(rootLayout.CurrentView, window, sourceDock.Context);

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

        public bool ValidateRoot(IRootDock sourceRoot, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        return false;
                    }
                case IViewDock targetView:
                    {
                        return false;
                    }
                case IToolTab sourceToolTab:
                    {
                        return false;
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return false;
                    }
                case ILayoutDock targetLayout:
                    {
                        return false;
                    }
                case ITabDock targetTab:
                    {
                        return false;
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(IRootDock)} dock target: {sourceRoot} -> {targetDock}");
                        return false;
                    }
            }
        }

        public bool ValidateToolTab(IToolTab toolTab, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        return MoveIntoWindow(toolTab, targetDock, action, operation, bExecute);
                    }
                case IViewDock targetView:
                    {
                        return false;
                    }
                case IToolTab sourceToolTab:
                    {
                        return DockBetweenTabs(toolTab, sourceToolTab, action, operation, bExecute);
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return DockBetweenTabs(toolTab, sourceDocumentTab, action, operation, bExecute);
                    }
                case ILayoutDock targetLayout:
                    {
                        return false;
                    }
                case IToolDock targetTool:
                    {
                        return DockIntoTab(toolTab, targetTool, action, operation, bExecute);
                    }
                case IDocumentDock targetTab:
                    {
                        return DockIntoTab(toolTab, targetTab, action, operation, bExecute);
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(IToolTab)} dock target: {toolTab} -> {targetDock}");
                        return false;
                    }
            }
        }

        public bool ValidateDocumentTab(IDocumentTab documentTab, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        return MoveIntoWindow(documentTab, targetDock, action, operation, bExecute);
                    }
                case IViewDock targetView:
                    {
                        return false;
                    }
                case IToolTab targetToolTab:
                    {
                        return false;
                    }
                case IDocumentTab targetDocumentTab:
                    {
                        return DockBetweenTabs(documentTab, targetDocumentTab, action, operation, bExecute);
                    }
                case ILayoutDock targetLayout:
                    {
                        return false;
                    }
                case IToolDock targetTool:
                    {
                        return false;
                    }
                case IDocumentDock targetTab:
                    {
                        return DockIntoTab(documentTab, targetTab, action, operation, bExecute);
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(IDocumentTab)} dock target: {documentTab} -> {targetDock}");
                        return false;
                    }
            }
        }

        public bool ValidateView(IViewDock sourceView, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        return false;
                    }
                case IViewDock targetView:
                    {
                        return false;
                    }
                case IToolTab sourceToolTab:
                    {
                        return false;
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return false;
                    }
                case ILayoutDock targetLayout:
                    {
                        return false;
                    }
                case ITabDock targetTab:
                    {
                        return false;
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(IViewDock)} dock target: {sourceView} -> {targetDock}");
                        return false;
                    }
            }
        }

        public bool ValidateLayout(ILayoutDock sourceLayout, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        return false;
                    }
                case IViewDock targetView:
                    {
                        return false;
                    }
                case IToolTab sourceToolTab:
                    {
                        return false;
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return false;
                    }
                case ILayoutDock targetLayout:
                    {
                        return false;
                    }
                case ITabDock targetTab:
                    {
                        return false;
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(ILayoutDock)} dock target: {sourceLayout} -> {targetDock}");
                        return false;
                    }
            }
        }

        public bool ValidateTab(ITabDock sourceTab, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        return MoveIntoWindow(sourceTab, targetDock, action, operation, bExecute);
                    }
                case IViewDock targetView:
                    {
                        return false;
                    }
                case IToolTab sourceToolTab:
                    {
                        return false;
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return false;
                    }
                case ILayoutDock targetLayout:
                    {
                        return false;
                    }
                case ITabDock targetTab:
                    {
                        return false;
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(ITabDock)} dock operation: {sourceTab} -> {targetDock}");
                        return false;
                    }
            }
        }

        public bool ValidateDock(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (sourceDock)
            {
                case IRootDock sourceRoot:
                    {
                        return ValidateRoot(sourceRoot, targetDock, action, operation, bExecute);
                    }
                case IViewDock sourceView:
                    {
                        return ValidateView(sourceView, targetDock, action, operation, bExecute);
                    }
                case IToolTab sourceToolTab:
                    {
                        return ValidateToolTab(sourceToolTab, targetDock, action, operation, bExecute);
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return ValidateDocumentTab(sourceDocumentTab, targetDock, action, operation, bExecute);
                    }
                case ILayoutDock sourceLayout:
                    {
                        return ValidateLayout(sourceLayout, targetDock, action, operation, bExecute);
                    }
                case ITabDock sourceTab:
                    {
                        return ValidateTab(sourceTab, targetDock, action, operation, bExecute);
                    }
                default:
                    {
                        Console.WriteLine($"Not supported dock source: {sourceDock}");
                        return false;
                    }
            }
        }
    }
}
