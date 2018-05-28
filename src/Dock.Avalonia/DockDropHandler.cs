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

        private bool ValidateDockStrip(IDock sourceDock, IDock targetDock, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            if (sourceDock.Parent is IDockStrip sourceStrip 
                && targetDock.Parent is IDockStrip targetStrip)
            {
                if (sourceStrip == targetStrip)
                {
                    int sourceIndex = sourceStrip.Views.IndexOf(sourceDock);
                    int targetIndex = sourceStrip.Views.IndexOf(targetDock);

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
                    int sourceIndex = sourceStrip.Views.IndexOf(sourceDock);
                    int targetIndex = targetStrip.Views.IndexOf(targetDock);

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

        private bool ValidateDockLayout(IDock sourceDock, IDock targetDock, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            if (sourceDock.Parent is IDockStrip sourceStrip 
                && targetDock is IDockStrip targetStrip 
                && sourceDock != targetDock 
                && sourceStrip != targetStrip)
            {
                int sourceIndex = sourceStrip.Views.IndexOf(sourceDock);
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
                                    // WIP: This is work in progress.
                                    //var splitLayout = container.SplitLayout(context, SplitDirection.Left);
                                    //layout.ReplaceView(container, splitLayout);
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

        private bool Validate(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            if (sourceDock.Parent is IDockStrip sourceStrip)
            {
                if (targetDock is IDockStrip targetStrip)
                {
                    return ValidateDockLayout(sourceDock, targetDock, e, bExecute, operation);
                }
                else
                {
                    return ValidateDockStrip(sourceDock, targetDock, e, bExecute, operation);
                }
            }

            if (sourceDock is IDockView item && sourceDock.Parent is IDockStrip container)
            {
                if (bExecute)
                {
                    switch(operation)
                    {
                        case DockOperation.Fill:
                            {
                                int itemIndex = sourceDock.Parent.Views.IndexOf(sourceDock);
                                var position = DropHelper.GetPositionScreen(sender, e);

                                // WIP: This is work in progress.
                                //var splitLayout = container.SplitLayout(context, SplitDirection.Left);
                                //layout.ReplaceView(container, splitLayout);

                                if (container.FindRootLayout() is IDock rootLayout)
                                {
                                    if (rootLayout.Factory is IDockFactory factory)
                                    {
                                        var window = factory.CreateWindow(rootLayout.CurrentView, container, itemIndex, sourceDock.Context);

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
                        default:
                            Console.WriteLine($"DockSplit: {operation}");
                            break;
                    }                        
                }
                return true;
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
