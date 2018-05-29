// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;
using Dock.Model.Controls;

namespace Dock.Avalonia
{
    public class DockDropHandler : IDropHandler
    {
        public int Id { get; set; }

        private bool ValidateMoveViewsBetweenTabs(IViewDock sourceView, IViewDock targetView, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            Console.WriteLine($"ValidateMoveViewsBetweenTabs: {sourceView.Title} -> {targetView.Title}");

            if (sourceView.Parent is ITabDock sourceTab && targetView.Parent is ITabDock targetTab)
            {
                if (sourceTab == targetTab)
                {
                    int sourceIndex = sourceTab.Views.IndexOf(sourceView);
                    int targetIndex = sourceTab.Views.IndexOf(targetView);

                    if (sourceIndex >= 0 && targetIndex >= 0)
                    {
                        if (e.DragEffects == DragDropEffects.Copy)
                        {
                            if (bExecute)
                            {
                                // TODO: Clone item.
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Move)
                        {
                            if (bExecute)
                            {
                                sourceTab.MoveView(sourceIndex, targetIndex);
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Link)
                        {
                            if (bExecute)
                            {
                                sourceTab.SwapView(sourceIndex, targetIndex);
                            }
                            return true;
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
                        if (e.DragEffects == DragDropEffects.Copy)
                        {
                            if (bExecute)
                            {
                                // TODO: Clone item.
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Move)
                        {
                            if (bExecute)
                            {
                                sourceTab.MoveView(targetTab, sourceIndex, targetIndex);
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Link)
                        {
                            if (bExecute)
                            {
                                sourceTab.SwapView(targetTab, sourceIndex, targetIndex);
                            }
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        private bool ValidateMoveViewToTab(IViewDock sourceView, ITabDock targetTab, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            Console.WriteLine($"ValidateMoveViewToTab: {sourceView.Title} -> {targetTab.Title}");

            if (sourceView.Parent is ITabDock sourceTab && sourceTab != targetTab)
            {
                int sourceIndex = sourceTab.Views.IndexOf(sourceView);
                int targetIndex = targetTab.Views.Count;

                if (sourceIndex >= 0 && targetIndex >= 0)
                {
                    if (e.DragEffects == DragDropEffects.Copy)
                    {
                        if (bExecute)
                        {
                            // TODO: Clone item.
                        }
                        return true;
                    }
                    else if (e.DragEffects == DragDropEffects.Move)
                    {
                        if (bExecute)
                        {
                            switch (operation)
                            {
                                case DockOperation.Fill:
                                    {
                                        sourceTab.MoveView(targetTab, sourceIndex, targetIndex);
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
                                    // TODO:
                                    break;
                            }
                        }
                        return true;
                    }
                    else if (e.DragEffects == DragDropEffects.Link)
                    {
                        if (bExecute)
                        {
                            sourceTab.SwapView(targetTab, sourceIndex, targetIndex);
                        }
                        return true;
                    }
                }

                return false;
            }
            return false;
        }

        private bool ValidateMoveDockToWindow(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            Console.WriteLine($"ValidateMoveDockToWindow: {sourceDock.Title} -> {targetDock.Title}");

            switch (operation)
            {
                case DockOperation.Fill:
                    {
                        var position = DropHelper.GetPositionScreen(sender, e);
                        int sourceIndex = sourceDock.Parent.Views.IndexOf(sourceDock);
                        if (sourceDock != targetDock 
                            && sourceDock.Parent != targetDock 
                            && sourceIndex >= 0 
                            && sourceDock.FindRootLayout() is IDock rootLayout 
                            && rootLayout.CurrentView != null
                            && rootLayout.Factory is IDockFactory factory)
                        {
                            if (bExecute)
                            {
                                sourceDock.Parent.RemoveView(sourceIndex);

                                var window = factory.CreateWindowFrom(sourceDock);
                                if (window != null)
                                {
                                    factory.AddWindow(rootLayout.CurrentView, window, sourceDock.Context);

                                    window.X = position.X;
                                    window.Y = position.Y;
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

        private bool ValidateRootDock(IRootDock sourceRoot, IDock targetDock, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        // TODO:
                    }
                    break;
                case IViewDock targetView:
                    {
                        // TODO:
                    }
                    break;
                case ILayoutDock targetLayout:
                    {
                        // TODO:
                    }
                    break;
                case ITabDock targetTab:
                    {
                        // TODO:
                    }
                    break;
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(IRootDock)} dock target: {sourceRoot} -> {targetDock}");
                        return false;
                    }
            }
            return false;
        }

        private bool ValidateViewDock(IViewDock sourceView, IDock targetDock, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        // TODO:
                        return ValidateMoveDockToWindow(sourceView, targetDock, sender, e, bExecute, operation);
                    }
                case IViewDock targetView:
                    {
                        // TODO:
                        return ValidateMoveViewsBetweenTabs(sourceView, targetView, e, bExecute, operation);
                    }
                case ILayoutDock targetLayout:
                    {
                        // TODO:
                        return false;
                    }
                case ITabDock targetTab:
                    {
                        // TODO:
                        return ValidateMoveViewToTab(sourceView, targetTab, e, bExecute, operation);
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(IViewDock)} dock target: {sourceView} -> {targetDock}");
                        return false;
                    }
            } 
        }

        private bool ValidateLayoutDock(ILayoutDock sourceLayout, IDock targetDock, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        // TODO:
                        return false;
                    }
                case IViewDock targetView:
                    {
                        // TODO:
                        return false;
                    }
                case ILayoutDock targetLayout:
                    {
                        // TODO:
                        return false;
                    }
                case ITabDock targetTab:
                    {
                        // TODO:
                        return false;
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(ILayoutDock)} dock target: {sourceLayout} -> {targetDock}");
                        return false;
                    }
            }
        }

        private bool ValidateTabDock(ITabDock sourceTab, IDock targetDock, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            switch (targetDock)
            {
                case IRootDock targetRoot:
                    {
                        // TODO:
                        return ValidateMoveDockToWindow(sourceTab, targetDock, sender, e, bExecute, operation);
                    }
                case IViewDock targetView:
                    {
                        // TODO:
                        return false;
                    }
                case ILayoutDock targetLayout:
                    {
                        // TODO:
                        return false;
                    }
                case ITabDock targetTab:
                    {
                        // TODO:
                        return false;
                    }
                default:
                    {
                        Console.WriteLine($"Not supported {nameof(ITabDock)} dock operation: {sourceTab} -> {targetDock}");
                        return false;
                    }
            }
        }

        private bool Validate(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            switch (sourceDock)
            {
                case IRootDock sourceRoot:
                    {
                        return ValidateRootDock(sourceRoot, targetDock, sender, e, operation, bExecute);
                    }
                case IViewDock sourceView:
                    {
                        return ValidateViewDock(sourceView, targetDock, sender, e, operation, bExecute);
                    }
                case ILayoutDock sourceLayout:
                    {
                        return ValidateLayoutDock(sourceLayout, targetDock, sender, e, operation, bExecute);
                    }
                case ITabDock sourceTab:
                    {
                        return ValidateTabDock(sourceTab, targetDock, sender, e, operation, bExecute);
                    }
                default:
                    {
                        Console.WriteLine($"Not supported dock source: {sourceDock}");
                        return false;
                    }
            }
        }

        public bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                Point point = DropHelper.GetPosition(sender, e);
                Console.WriteLine($"Validate [{Id}]: {sourceDock.Title} -> {targetDock.Title} [{operation}] [{point}]");
                return Validate(sourceDock, targetDock, sender, e, operation, false);
            }
            return false;
        }

        private bool bExecuted = false;

        public bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (bExecuted == false && sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                Point point = DropHelper.GetPosition(sender, e);
                Console.WriteLine($"Execute [{Id}]: {sourceDock.Title} -> {targetDock.Title} [{operation}] [{point}]");
                bool bResult = Validate(sourceDock, targetDock, sender, e, operation, true);
                if (bResult == true)
                {
                    Console.WriteLine($"Executed [{Id}]: {sourceDock.Title} -> {targetDock.Title} [{operation}] [{point}]");
                    bExecuted = true;
                    return true;
                }
                return false;
            }
            return false;
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
            bExecuted = false;
        }
    }
}
