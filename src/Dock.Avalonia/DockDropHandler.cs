// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;
using Dock.Model.Controls;

namespace Dock.Avalonia
{
    public class DockDropHandler : IDropHandler
    {
        public static IDropHandler Instance = new DockDropHandler();

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

            if (bExecute)
            {
                switch(operation)
                {
                    case DockOperation.Fill:
                        {
                            var position = DropHelper.GetPositionScreen(sender, e);
                            int sourceIndex = sourceDock.Parent.Views.IndexOf(sourceDock);
                            if (sourceIndex >= 0)
                            {
                                if (sourceDock.FindRootLayout() is IDock rootLayout && rootLayout.CurrentView != null)
                                {
                                    if (rootLayout.Factory is IDockFactory factory)
                                    {
                                        sourceDock.Parent.RemoveView(sourceIndex);

                                        var window = factory.CreateWindowFrom(sourceDock);
                                        window.X = position.X;
                                        window.Y = position.Y;
                                        window.Width = 300;
                                        window.Height = 400;

                                        factory.AddWindow(rootLayout.CurrentView, window, sourceDock.Context);

                                        window.Present(false);

                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                } 
                            }
                        }
                        break;
                }
            }
            return true;
        }

        private bool Validate(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            switch (sourceDock)
            {
                case IRootDock sourceRoot:
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
                                    Console.WriteLine($"Not supported dock target: {sourceDock} -> {targetDock}");
                                }
                                break;
                        }
                    }
                    break;
                case IViewDock sourceView:
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
                                    return ValidateMoveViewsBetweenTabs(sourceView, targetView, e, bExecute, operation);
                                }
                            case ILayoutDock targetLayout:
                                {
                                    // TODO:
                                }
                                break;
                            case ITabDock targetTab:
                                {
                                    // TODO:
                                    return ValidateMoveViewToTab(sourceView, targetTab, e, bExecute, operation);
                                }
                            default:
                                {
                                    Console.WriteLine($"Not supported dock target: {sourceDock} -> {targetDock}");
                                }
                                break;
                        }

                        return ValidateMoveDockToWindow(sourceView, targetDock, sender, e, bExecute, operation);
                    }
                case ILayoutDock sourceLayout:
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
                                    Console.WriteLine($"Not supported dock target: {sourceDock} -> {targetDock}");
                                }
                                break;
                        }
                    }
                    break;
                case ITabDock sourceTab:
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
                                    Console.WriteLine($"Not supported dock operation: {sourceDock} -> {targetDock}");
                                }
                                break;
                        }

                        return ValidateMoveDockToWindow(sourceTab, targetDock, sender, e, bExecute, operation);
                    }
                default:
                    {
                        Console.WriteLine($"Not supported dock source: {sourceDock}");
                    }
                    break;
            }
            return false;
        }

        public bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                Console.WriteLine($"Validate: {sourceDock.Title} -> {targetDock.Title} [{operation}]");
                return Validate(sourceDock, targetDock, sender, e,  operation, false);
            }
            return false;
        }

        public bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                Console.WriteLine($"Execute: {sourceDock.Title} -> {targetDock.Title} [{operation}]");
                return Validate(sourceDock, targetDock, sender, e, operation, true);
            }
            return false;
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
        }
    }
}
