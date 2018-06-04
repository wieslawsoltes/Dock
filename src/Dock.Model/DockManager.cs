// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.ObjectModel;
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

        internal bool DockIntoTab(ITab sourceTab, ITab targetTab, DragAction action, DockOperation operation, bool bExecute)
        {
            Console.WriteLine($"{nameof(DockIntoTab)}: {sourceTab.Title} -> {targetTab.Title}");

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

        internal bool DockIntoTab(ITab sourceTab, ITabDock targetTabParent, DragAction action, DockOperation operation, bool bExecute)
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
                                switch (operation)
                                {
                                    case DockOperation.Fill:
                                        {
                                            if (sourceTabParent.Factory is IDockFactory factory)
                                            {
                                                if (bExecute)
                                                {
                                                    factory.MoveView(sourceTabParent, targetTabParent, sourceIndex, targetIndex);
                                                }
                                                return true;
                                            }
                                            return false;
                                        }
                                    case DockOperation.Left:
                                    case DockOperation.Right:
                                    case DockOperation.Top:
                                    case DockOperation.Bottom:
                                        {
                                            switch (sourceTab)
                                            {
                                                case IToolTab toolTab:
                                                    {
                                                        if (bExecute)
                                                        {
                                                            if (targetTabParent.Factory is IDockFactory factory)
                                                            {
                                                                factory.Remove(sourceTab);

                                                                IDock tool = factory.CreateToolDock();
                                                                tool.Id = nameof(IToolDock);
                                                                tool.Title = nameof(IToolDock);
                                                                tool.CurrentView = sourceTab;
                                                                tool.Views = new ObservableCollection<IView> { sourceTab };

                                                                factory.Split(targetTabParent, tool, operation);
                                                            }
                                                        }
                                                        return true;
                                                    }
                                                case IDocumentTab documentTab:
                                                    {
                                                        if (bExecute)
                                                        {
                                                            if (targetTabParent.Factory is IDockFactory factory)
                                                            {
                                                                factory.Remove(sourceTab);

                                                                IDock document = factory.CreateDocumentDock();
                                                                document.Id = nameof(IDocumentDock);
                                                                document.Title = nameof(IDocumentDock);
                                                                document.CurrentView = sourceTab;
                                                                document.Views = new ObservableCollection<IView> { sourceTab };

                                                                factory.Split(targetTabParent, document, operation);
                                                            }
                                                        }
                                                        return true;
                                                    }
                                                default:
                                                    {
                                                        Console.WriteLine($"Not supported tab type {sourceTab.GetType().Name} to splitting : {sourceTab} -> {targetTabParent}");
                                                        return false;
                                                    }
                                            }
                                        }
                                    case DockOperation.Window:
                                        {
                                            return false;
                                        }
                                }
                                return false;
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

        internal bool DockIntoWindow(IView sourceView, IView targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            Console.WriteLine($"{nameof(DockIntoWindow)}: {sourceView.Title} -> {targetView.Title}");

            switch (operation)
            {
                case DockOperation.Fill:
                    {
                        if (sourceView.Parent is IViewsHost sourceViewParentHost)
                        {
                            int sourceIndex = sourceViewParentHost.Views.IndexOf(sourceView);
                            if (sourceIndex >= 0
                                && sourceView != targetView && sourceView.Parent != targetView
                                && sourceView.Parent is IDock sourceViewParentDock
                                && sourceViewParentDock.Factory is IDockFactory factory
                                && factory.FindRoot(sourceView) is IDock rootLayout && rootLayout.CurrentView != null)
                            {
                                if (bExecute)
                                {
                                    factory.RemoveView(sourceViewParentHost, sourceIndex);

                                    var window = factory.CreateWindowFrom(sourceView);
                                    if (window != null)
                                    {
                                        if (rootLayout.CurrentView is IWindowsHost host)
                                        {
                                            factory.AddWindow(host, window, sourceView.Context);
                                        }

                                        window.X = ScreenPosition.X;
                                        window.Y = ScreenPosition.Y;
                                        window.Width = 300;
                                        window.Height = 400;
                                        window.Present(false);
                                    }
                                }
                                return true;
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        internal bool ValidateToolTab(IToolTab toolTab, IView targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetView)
            {
                case IRootDock targetRoot:
                    {
                        return DockIntoWindow(toolTab, targetView, action, operation, bExecute);
                    }
                case IToolTab sourceToolTab:
                    {
                        return DockIntoTab(toolTab, sourceToolTab, action, operation, bExecute);
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return DockIntoTab(toolTab, sourceDocumentTab, action, operation, bExecute);
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
                        Console.WriteLine($"Not supported type {targetView.GetType()}: {toolTab} -> {targetView}");
                        return false;
                    }
            }
        }

        internal bool ValidateDocumentTab(IDocumentTab documentTab, IView targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetView)
            {
                case IRootDock targetRoot:
                    {
                        return DockIntoWindow(documentTab, targetView, action, operation, bExecute);
                    }
                case IToolTab targetToolTab:
                    {
                        return false;
                    }
                case IDocumentTab targetDocumentTab:
                    {
                        return DockIntoTab(documentTab, targetDocumentTab, action, operation, bExecute);
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
                        Console.WriteLine($"Not supported type {targetView.GetType()}: {documentTab} -> {targetView}");
                        return false;
                    }
            }
        }

        internal bool ValidateRoot(IRootDock sourceRoot, IView targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetView)
            {
                case IRootDock targetRoot:
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
                        Console.WriteLine($"Not supported type {targetView.GetType()}: {sourceRoot} -> {targetView}");
                        return false;
                    }
            }
        }

        internal bool ValidateView(IView sourceView, IView targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetView)
            {
                case IRootDock targetRoot:
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
                        Console.WriteLine($"Not supported type {targetView.GetType()}: {sourceView} -> {targetView}");
                        return false;
                    }
            }
        }

        internal bool ValidateLayout(ILayoutDock sourceLayout, IView targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetView)
            {
                case IRootDock targetRoot:
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
                        Console.WriteLine($"Not supported type {targetView.GetType()}: {sourceLayout} -> {targetView}");
                        return false;
                    }
            }
        }

        internal bool ValidateTab(ITabDock sourceTab, IView targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (targetView)
            {
                case IRootDock targetRoot:
                    {
                        return DockIntoWindow(sourceTab, targetView, action, operation, bExecute);
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
                        Console.WriteLine($"Not supported type {targetView.GetType()}: {sourceTab} -> {targetView}");
                        return false;
                    }
            }
        }

        /// <inheritdoc/>
        public bool Validate(IView sourceView, IView targetView, DragAction action, DockOperation operation, bool bExecute)
        {
            switch (sourceView)
            {
                case IRootDock sourceRoot:
                    {
                        return ValidateRoot(sourceRoot, targetView, action, operation, bExecute);
                    }
                case IToolTab sourceToolTab:
                    {
                        return ValidateToolTab(sourceToolTab, targetView, action, operation, bExecute);
                    }
                case IDocumentTab sourceDocumentTab:
                    {
                        return ValidateDocumentTab(sourceDocumentTab, targetView, action, operation, bExecute);
                    }
                case ILayoutDock sourceLayout:
                    {
                        return ValidateLayout(sourceLayout, targetView, action, operation, bExecute);
                    }
                case ITabDock sourceTab:
                    {
                        return ValidateTab(sourceTab, targetView, action, operation, bExecute);
                    }
                default:
                    {
                        Console.WriteLine($"Not supported type {sourceView.GetType()}: {sourceView} -> {targetView}");
                        return false;
                    }
            }
        }
    }
}
