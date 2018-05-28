// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.ObjectModel;
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
            Console.WriteLine($"ValidateMoveViewsBetweenStrips: {sourceView.Title} -> {targetView.Title}");

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
            Console.WriteLine($"ValidateMoveViewToStrip: {sourceView.Title} -> {targetStrip.Title}");

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
                                    {
                                        sourceStrip.MoveView(targetStrip, sourceIndex, targetIndex);
                                        return true;
                                    }
                                case DockOperation.Left:
                                case DockOperation.Right:
                                case DockOperation.Top:
                                case DockOperation.Bottom:
                                    {
                                        if (targetStrip.Factory is IDockFactory factory)
                                        {
                                            factory.Remove(sourceView);

                                            IDock strip = new DockStrip
                                            {
                                                Id = nameof(DockStrip),
                                                Title = nameof(DockStrip),
                                                CurrentView = sourceView,
                                                Views = new ObservableCollection<IDock> { sourceView }
                                            };

                                            factory.Split(targetStrip, strip, operation);
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
                            sourceStrip.SwapView(targetStrip, sourceIndex, targetIndex);
                        }
                        return true;
                    }
                }

                return false;
            }
            return false;
        }

        private bool ValidateMoveViewToWindow(IDockView sourceView, IDock targetDock, object sender, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            Console.WriteLine($"ValidateMoveViewToWindow: {sourceView.Title} -> {targetDock.Title}");

            if (bExecute)
            {
                switch(operation)
                {
                    case DockOperation.Fill:
                        {
                            var position = DropHelper.GetPositionScreen(sender, e);
                            int sourceIndex = sourceView.Parent.Views.IndexOf(sourceView);
                            if (sourceIndex >= 0)
                            {
                                if (sourceView.FindRootLayout() is IDock rootLayout)
                                {
                                    if (rootLayout.Factory is IDockFactory factory)
                                    {
                                        sourceView.Parent.RemoveView(sourceIndex);

                                        var window = factory.CreateWindowFrom(sourceView);
                                        window.X = position.X;
                                        window.Y = position.Y;
                                        window.Width = 300;
                                        window.Height = 400;

                                        factory.AddWindow(rootLayout.CurrentView, window, sourceView.Context);

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

                        return ValidateMoveViewToWindow(sourceView, targetDock, sender, e, bExecute, operation);
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
