// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Tree view drop handler.
    /// </summary>
    public class DockTreeViewDropHandler : IDropHandler
    {
        private bool Validate(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            if (sourceDock != targetDock && sourceDock.Factory is IDockFactory factory)
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
                        factory.MoveTo(sourceDock, targetDock);
                    }
                    return true;
                }
                else if (e.DragEffects == DragDropEffects.Link)
                {
                    if (bExecute)
                    {
                        factory.Swap(sourceDock, targetDock);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                return Validate(sourceDock, targetDock, sender, e, false);
            }
            return false;
        }

        /// <inheritdoc/>
        public bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                return Validate(sourceDock, targetDock, sender, e, true);
            }
            return false;
        }

        /// <inheritdoc/>
        public void Cancel(object sender, RoutedEventArgs e)
        {
        }
    }
}
