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

        public bool MoveBetweenTabs(ITab sourceView, ITab targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            Console.WriteLine($"{nameof(MoveBetweenTabs)}: {sourceView.Title} -> {targetView.Title}");

            if (sourceView.Parent is ITabDock sourceTab && targetView.Parent is ITabDock targetTab)
            {
                if (sourceTab == targetTab)
                {
                    int sourceIndex = sourceTab.Views.IndexOf(sourceView);
                    int targetIndex = sourceTab.Views.IndexOf(targetView);
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
                                        if (sourceTab.Factory is IDockFactory factory)
                                        {
                                            factory.MoveView(sourceTab, sourceIndex, targetIndex);
                                        }
                                    }
                                    return true;
                                }
                            case DragAction.Link:
                                {
                                    if (bExecute)
                                    {
                                        if (sourceTab.Factory is IDockFactory factory)
                                        {
                                            factory.SwapView(sourceTab, sourceIndex, targetIndex);
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
                    int sourceIndex = sourceTab.Views.IndexOf(sourceView);
                    int targetIndex = targetTab.Views.IndexOf(targetView);
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
                                        if (sourceTab.Factory is IDockFactory factory)
                                        {
                                            factory.MoveView(sourceTab, targetTab, sourceIndex, targetIndex);
                                        }
                                    }
                                    return true;
                                }
                            case DragAction.Link:
                                {
                                    if (bExecute)
                                    {
                                        if (sourceTab.Factory is IDockFactory factory)
                                        {
                                            factory.SwapView(sourceTab, targetTab, sourceIndex, targetIndex);
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

        public bool MoveIntoTab(ITab sourceView, ITabDock targetTab, DragAction action, DockOperation operation, bool bExecute)
        {
            Console.WriteLine($"{nameof(MoveIntoTab)}: {sourceView.Title} -> {targetTab.Title}");

            if (sourceView.Parent is ITabDock sourceTab && sourceTab != targetTab)
            {
                int sourceIndex = sourceTab.Views.IndexOf(sourceView);
                int targetIndex = targetTab.Views.Count;
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
                                                if (sourceTab.Factory is IDockFactory factory)
                                                {
                                                    factory.MoveView(sourceTab, targetTab, sourceIndex, targetIndex);
                                                }
                                                return true;
                                            }
                                        case DockOperation.Left:
                                        case DockOperation.Right:
                                        case DockOperation.Top:
                                        case DockOperation.Bottom:
                                            {
                                                if (targetTab.Factory is IDockFactory factory)
                                                {
                                                    factory.Remove(sourceView);

                                                    IDock tool = new ToolDock
                                                    {
                                                        Id = nameof(ToolDock),
                                                        Title = nameof(ToolDock),
                                                        CurrentView = sourceView,
                                                        Views = new ObservableCollection<IDock> { sourceView }
                                                    };

                                                    factory.Split(targetTab, tool, operation);
                                                }
                                                return true;
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
                                    if (sourceTab.Factory is IDockFactory factory)
                                    {
                                        factory.SwapView(sourceTab, targetTab, sourceIndex, targetIndex);
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
                        return MoveBetweenTabs(toolTab, sourceToolTab, action, operation, bExecute);
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return MoveBetweenTabs(toolTab, sourceDocumentTab, action, operation, bExecute);
                    }
                case ILayoutDock targetLayout:
                    {
                        return false;
                    }
                case IToolDock targetTool:
                    {
                        return MoveIntoTab(toolTab, targetTool, action, operation, bExecute);
                    }
                case IDocumentDock targetTab:
                    {
                        return MoveIntoTab(toolTab, targetTab, action, operation, bExecute);
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
                        return MoveBetweenTabs(documentTab, targetDocumentTab, action, operation, bExecute);
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
                        return MoveIntoTab(documentTab, targetTab, action, operation, bExecute);
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
