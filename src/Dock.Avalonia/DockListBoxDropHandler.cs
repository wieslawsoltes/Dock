// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;

namespace Dock.Avalonia
{
    public class DockListBoxDropHandler : IDropHandler
    {
        public static IDropHandler Instance = new DockListBoxDropHandler();

        private bool Validate(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            if (sourceDock != targetDock && sourceDock.Factory is IDockFactory factory)
            {
                if (sourceDock.Parent is IDock sourceParent && targetDock.Parent is IDock targetParent)
                {
                    if (sourceParent == targetParent)
                    {
                        int sourceIndex = sourceParent.Views.IndexOf(sourceDock);
                        int targetIndex = sourceParent.Views.IndexOf(targetDock);

                        if (sourceIndex >= 0 && targetIndex >= 0)
                        {
                            if (e.DragEffects == DragDropEffects.Copy)
                            {
                                if (bExecute)
                                {
                                    // TODO: Clone layout and insert into Views collection.
                                }
                                return true;
                            }
                            else if (e.DragEffects == DragDropEffects.Move)
                            {
                                if (bExecute)
                                {
                                    sourceParent.MoveView(sourceIndex, targetIndex);
                                }
                                return true;
                            }
                            else if (e.DragEffects == DragDropEffects.Link)
                            {
                                if (bExecute)
                                {
                                    sourceParent.SwapView(sourceIndex, targetIndex);
                                }
                                return true;
                            }
                        }
                    }
                    else
                    {
                        int sourceIndex = sourceParent.Views.IndexOf(sourceDock);
                        int targetIndex = targetParent.Views.IndexOf(targetDock);

                        if (sourceIndex >= 0 && targetIndex >= 0)
                        {
                            if (e.DragEffects == DragDropEffects.Copy)
                            {
                                if (bExecute)
                                {
                                    // TODO: Clone layout and insert into Views collection.
                                }
                                return true;
                            }
                            else if (e.DragEffects == DragDropEffects.Move)
                            {
                                if (bExecute)
                                {
                                    sourceParent.MoveView(targetParent, sourceIndex, targetIndex);
                                }
                                return true;
                            }
                            else if (e.DragEffects == DragDropEffects.Link)
                            {
                                if (bExecute)
                                {
                                    sourceParent.SwapView(targetParent, sourceIndex, targetIndex);
                                }
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                return Validate(sourceDock, targetDock, sender, e, false);
            }
            return false;
        }

        public bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                return Validate(sourceDock, targetDock, sender, e, true);
            }
            return false;
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
        }
    }
}
