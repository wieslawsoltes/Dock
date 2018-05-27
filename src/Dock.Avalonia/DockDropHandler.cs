// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Diagnostics;
using Avalonia.Input;
using Dock.Model;

namespace Dock.Avalonia
{
    public class DockDropHandler : IDropHandler
    {
        public static IDropHandler Instance = new DockDropHandler();

        private bool ValidateDockStrip(IDock sourceLayout, IDock targetLayout, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            if (sourceLayout.Parent is IDockStrip sourceStrip 
                && targetLayout.Parent is IDockStrip targetStrip)
            {
                if (sourceStrip == targetStrip)
                {
                    int sourceIndex = sourceStrip.Views.IndexOf(sourceLayout);
                    int targetIndex = sourceStrip.Views.IndexOf(targetLayout);

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
                    int sourceIndex = sourceStrip.Views.IndexOf(sourceLayout);
                    int targetIndex = targetStrip.Views.IndexOf(targetLayout);

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

        private bool ValidateDockLayout(IDock sourceLayout, IDock targetLayout, DragEventArgs e, bool bExecute, DockOperation operation)
        {
            if (sourceLayout.Parent is IDockStrip sourceStrip 
                && targetLayout is IDockStrip targetStrip 
                && sourceLayout != targetLayout 
                && sourceStrip != targetStrip)
            {
                int sourceIndex = sourceStrip.Views.IndexOf(sourceLayout);
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

        private bool Validate(IDock sourceLayout, IDock targetLayout, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            if (sourceLayout.Parent is IDockStrip sourceStrip)
            {
                if (targetLayout is IDockStrip targetStrip)
                {
                    return ValidateDockLayout(sourceLayout, targetLayout, e, bExecute, operation);
                }
                else
                {
                    return ValidateDockStrip(sourceLayout, targetLayout, e, bExecute, operation);
                }
            }

            if (sourceLayout is IDockView item && sourceLayout.Parent is IDockStrip container)
            {
                if (bExecute)
                {
                    switch(operation)
                    {
                        case DockOperation.Fill:
                            {
                                int itemIndex = sourceLayout.Parent.Views.IndexOf(sourceLayout);
                                var position = DropHelper.GetPositionScreen(sender, e);

                                // WIP: This is work in progress.
                                //var splitLayout = container.SplitLayout(context, SplitDirection.Left);
                                //layout.ReplaceView(container, splitLayout);

                                if (container.FindRootLayout() is IDock rootLayout)
                                {
                                    var window = rootLayout.CurrentView.CreateWindow(container, itemIndex, sourceLayout.Context);
                                    window.X = position.X;
                                    window.Y = position.Y;
                                    window.Width = 300;
                                    window.Height = 400;
                                    window.Id = "Dock";
                                    window.Title = "Dock";
                                    window.Layout.Title = "Dock";
                                    window.Present(false);

                                    return true;
                                }
                            }
                            return false;
                        default:
                            System.Console.WriteLine($"DockSplit: {operation}");
                            break;
                    }                        
                }
                return true;
            }

            return false;
        }

        public bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceLayout && targetContext is IDock targetLayout)
            {
                Debug.WriteLine($"Validate: {sourceLayout.Title} -> {targetLayout.Title}");
                return Validate(sourceLayout, targetLayout, sender, e,  operation, false);
            }
            return false;
        }

        public bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceLayout && targetContext is IDock targetLayout)
            {
                Debug.WriteLine($"Execute: {sourceLayout.Title} -> {targetLayout.Title}");
                return Validate(sourceLayout, targetLayout, sender, e, operation, true);
            }
            return false;
        }
    }
}
