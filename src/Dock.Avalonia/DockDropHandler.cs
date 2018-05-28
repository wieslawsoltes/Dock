// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;

namespace Dock.Avalonia
{
    public class DockDropHandler : IDropHandler
    {
        public static IDropHandler Instance = new DockDropHandler();

        private bool ValidateMoveViewsBetweenStrips(IDockView sourceView, IDockView targetView, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            if (sourceView.Parent is IDockStrip sourceStrip && targetView.Parent is IDockStrip targetStrip)
            {
                if (sourceStrip == targetStrip)
                {
                    int sourceIndex = sourceStrip.Views.IndexOf(sourceView);
                    int targetIndex = sourceStrip.Views.IndexOf(targetView);

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
                                sourceStrip.MoveView(sourceIndex, targetIndex);
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Link)
                        {
                            if (bExecute)
                            {
                                sourceStrip.SwapView(sourceIndex, targetIndex);
                            }
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    int sourceIndex = sourceStrip.Views.IndexOf(sourceView);
                    int targetIndex = targetStrip.Views.IndexOf(targetView);

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
                                sourceStrip.MoveView(targetStrip, sourceIndex, targetIndex);
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Link)
                        {
                            if (bExecute)
                            {
                                sourceStrip.SwapView(targetStrip, sourceIndex, targetIndex);
                            }
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        private bool ValidateMoveViewToStrip(IDockView sourceView, IDockStrip targetStrip, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            if (sourceView.Parent is IDockStrip sourceStrip && sourceStrip != targetStrip)
            {
                int sourceIndex = sourceStrip.Views.IndexOf(sourceView);
                int targetIndex = targetStrip.Views.Count;

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
                                    sourceStrip.MoveView(targetStrip, sourceIndex, targetIndex);
                                    break;
                                case DockOperation.Left:
                                    // TODO:
                                    break;
                                case DockOperation.Bottom:
                                    // TODO:
                                    break;
                                case DockOperation.Right:
                                    // TODO:
                                    break;
                                case DockOperation.Top:
                                    // TODO:
                                    break;
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
                            sourceStrip.SwapView(targetStrip, sourceIndex, targetIndex);
                        }
                        return true;
                    }
                }

                return false;
            }
            return false;
        }

        private bool ValidateMoveViewToWindow(IDockView sourceView, object sender, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            if (sourceView.Parent is IDockStrip sourceStrip)
            {
                if (bExecute)
                {
                    switch(operation)
                    {
                        case DockOperation.Fill:
                            {
                                int sourceIndex = sourceStrip.Views.IndexOf(sourceView);
                                var position = DropHelper.GetPositionScreen(sender, e);

                                if (sourceStrip.FindRootLayout() is IDock rootLayout)
                                {
                                    if (rootLayout.Factory is IDockFactory factory)
                                    {
                                        sourceStrip.RemoveView(sourceIndex);

                                        var window = factory.CreateWindow(rootLayout.CurrentView, sourceView, sourceView.Context);

                                        window.X = position.X;
                                        window.Y = position.Y;
                                        window.Width = 300;
                                        window.Height = 400;
                                        window.Present(false);

                                        return true;
                                    }
                                    return false;
                                }
                            }
                            return false;
                    }                        
                }
                return true;
            }
            return false;
        }

        private bool Validate(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            switch (sourceDock)
            {
                case IDockRoot sourceRoot:
                    {
                        switch (targetDock)
                        {
                            case IDockRoot targetRoot:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockView targetView:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockLayout targetLayout:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockStrip targetStrip:
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
                case IDockView sourceView:
                    {
                        switch (targetDock)
                        {
                            case IDockRoot targetRoot:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockView targetView:
                                {
                                    // TODO:
                                    return ValidateMoveViewsBetweenStrips(sourceView, targetView, e, bExecute, operation);
                                }
                            case IDockLayout targetLayout:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockStrip targetStrip:
                                {
                                    // TODO:
                                    return ValidateMoveViewToStrip(sourceView, targetStrip, e, bExecute, operation);
                                }
                            default:
                                {
                                    Console.WriteLine($"Not supported dock target: {sourceDock} -> {targetDock}");
                                }
                                break;
                        }

                        return ValidateMoveViewToWindow(sourceView, sender, e, bExecute, operation);
                    }
                case IDockLayout sourceLayout:
                    {
                        switch (targetDock)
                        {
                            case IDockRoot targetRoot:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockView targetView:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockLayout targetLayout:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockStrip targetStrip:
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
                case IDockStrip sourceStrip:
                    {
                        switch (targetDock)
                        {
                            case IDockRoot targetRoot:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockView targetView:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockLayout targetLayout:
                                {
                                    // TODO:
                                }
                                break;
                            case IDockStrip targetStrip:
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
                    }
                    break;
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
                Console.WriteLine($"Validate: {sourceDock.Title} -> {targetDock.Title}");
                return Validate(sourceDock, targetDock, sender, e,  operation, false);
            }
            return false;
        }

        public bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                Console.WriteLine($"Execute: {sourceDock.Title} -> {targetDock.Title}");
                return Validate(sourceDock, targetDock, sender, e, operation, true);
            }
            return false;
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
        }
    }
}
