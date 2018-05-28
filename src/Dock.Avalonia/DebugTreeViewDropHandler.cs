// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;

namespace Dock.Avalonia
{
    public class DebugTreeViewDropHandler : IDropHandler
    {
        public static IDropHandler Instance = new DebugTreeViewDropHandler();

        private bool Validate(IDock sourceDock, IDock targetDock, object sender, DragEventArgs e, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

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
                    if (sourceDock.Factory is IDockFactory factory)
                    {
                        factory.Move(sourceDock, targetDock);
                    }
                }
                return true;
            }
            else if (e.DragEffects == DragDropEffects.Link)
            {
                if (bExecute)
                {
                    if (sourceDock.Factory is IDockFactory factory)
                    {
                        factory.Swap(sourceDock, targetDock);
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
