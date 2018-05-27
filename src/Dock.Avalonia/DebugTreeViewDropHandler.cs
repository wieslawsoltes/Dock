// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Dock.Model;

namespace Dock.Avalonia
{
    public class DebugTreeViewDropHandler : IDropHandler
    {
        public static IDropHandler Instance = new DebugTreeViewDropHandler();

        private bool Validate(IDock sourceLayout, IDock targetLayout, object sender, DragEventArgs e, bool bExecute)
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
                    sourceLayout.Factory.Move(sourceLayout, targetLayout);
                }
                return true;
            }
            else if (e.DragEffects == DragDropEffects.Link)
            {
                return false;
            }

            return false;
        }

        public bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceLayout && targetContext is IDock targetLayout)
            {
                return Validate(sourceLayout, targetLayout, sender, e, false);
            }
            return false;
        }

        public bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceLayout && targetContext is IDock targetLayout)
            {
                return Validate(sourceLayout, targetLayout, sender, e, true);
            }
            return false;
        }
    }
}
