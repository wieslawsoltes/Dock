// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// List box drop handler.
    /// </summary>
    public class DockListBoxDropHandler : DefaultDropHandler
    {
        private bool Validate(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            if (sourceDock != targetDock)
            {
                if (sourceDock.Parent is IDock sourceParent && targetDock.Parent is IDock targetParent)
                {
                    if (sourceParent == targetParent)
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
                                if (sourceParent.Factory is IDockFactory factory)
                                {
                                    factory.MoveView(sourceParent, sourceDock, targetDock);
                                }
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Link)
                        {
                            if (bExecute)
                            {
                                if (sourceParent.Factory is IDockFactory factory)
                                {
                                    factory.SwapView(sourceParent, sourceDock, targetDock);
                                }
                            }
                            return true;
                        }
                    }
                    else
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
                                if (sourceParent.Factory is IDockFactory factory)
                                {
                                    factory.MoveView(sourceParent, targetParent, sourceDock, targetDock);
                                }
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Link)
                        {
                            if (bExecute)
                            {
                                if (sourceParent.Factory is IDockFactory factory)
                                {
                                    factory.SwapView(sourceParent, targetParent, sourceDock, targetDock);
                                }
                            }
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool Validate(object sender, DragEventArgs e, object sourceContext, object targetContext, object state)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                return Validate(sourceDock, targetDock, sender, e, false);
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool Execute(object sender, DragEventArgs e, object targetContext, object sourceContext, object state)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                return Validate(sourceDock, targetDock, sender, e, true);
            }
            return false;
        }
    }
}
