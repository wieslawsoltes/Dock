// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Input;
using Dock.Model;

namespace Dock.Avalonia
{
    public class DebugTreeViewDropHandler : IDropHandler
    {
        public static IDropHandler Instance = new DebugTreeViewDropHandler();

        private bool ValidateTreeView(IDock layout, DragEventArgs e, bool bExecute, TreeView tree)
        {
            var sourceItem = e.Data.Get(DragDataFormats.Parent);
            var targetItem = (e.Source as IControl)?.Parent;

            if (sourceItem is TreeViewItem source && targetItem is TreeViewItem target)
            {
                var sourceData = source.DataContext;
                var targetData = target.DataContext;

                switch (sourceData)
                {
                    case IDock sourceLayout:
                        {
                            switch (targetData)
                            {
                                case IDock targetLayout:
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
                            }
                            return false;
                        }
                }
            }

            return false;
        }

        private bool Validate(IDock layout, object sender, DragEventArgs e, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            switch (sender)
            {
                case TreeView tree:
                    return ValidateTreeView(layout, e, bExecute, tree);
            }

            return false;
        }

        public bool Validate(object context, object sender, DockOperation operation, DragEventArgs e)
        {
            if (context is IDock layout)
            {
                return Validate(layout, sender, e, false);
            }
            return false;
        }

        public bool Execute(object context, object sender, DockOperation operation, DragEventArgs e)
        {
            if (context is IDock layout)
            {
                return Validate(layout, sender, e, true);
            }
            return false;
        }
    }
}
