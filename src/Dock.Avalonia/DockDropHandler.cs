// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Dock.Model;

namespace Dock.Avalonia
{
    public class DockDropHandler : IDropHandler
    {
        public static IDropHandler Instance = new DockDropHandler();

        private bool ValidateTabStrip(IDock layout, DragEventArgs e, bool bExecute, DockOperation operation, TabStrip strip)
        {
            var sourceItem = e.Data.Get(DragDataFormats.Parent);
            var targetItem = (e.Source as IControl)?.Parent?.Parent;

            if (sourceItem is TabStripItem source && targetItem is TabStripItem target)
            {
                if (source.Parent == target.Parent)
                {
                    int sourceIndex = strip.ItemContainerGenerator.IndexFromContainer(source);
                    int targetIndex = strip.ItemContainerGenerator.IndexFromContainer(target);

                    if (strip.DataContext is IDock container)
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
                                container.MoveView(sourceIndex, targetIndex);
                            }
                            return true;
                        }
                        else if (e.DragEffects == DragDropEffects.Link)
                        {
                            if (bExecute)
                            {
                                container.SwapView(sourceIndex, targetIndex);
                            }
                            return true;
                        }
                        return false;
                    }

                    return false;
                }
                else if (source.Parent is TabStrip sourceStrip
                    && target.Parent is TabStrip targetStrip
                    && sourceStrip.DataContext is IDock sourceLayout
                    && targetStrip.DataContext is IDock targetLayout)
                {
                    int sourceIndex = sourceStrip.ItemContainerGenerator.IndexFromContainer(source);
                    int targetIndex = targetStrip.ItemContainerGenerator.IndexFromContainer(target);

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
                        if (sourceLayout.Views.Count > 1)
                        {
                            if (bExecute)
                            {
                                sourceLayout.MoveView(targetLayout, sourceIndex, targetIndex);
                            }
                            return true;
                        }
                        return false;
                    }
                    else if (e.DragEffects == DragDropEffects.Link)
                    {
                        if (bExecute)
                        {
                            sourceLayout.SwapView(targetLayout, sourceIndex, targetIndex);
                        }
                        return true;
                    }

                    return false;
                }

                return false;
            }

            return false;
        }

        private bool ValidateDockPanel(IDock layout, DragEventArgs e, bool bExecute, DockOperation operation, DockPanel panel)
        {
            var sourceItem = e.Data.Get(DragDataFormats.Parent);

            if (sourceItem is TabStripItem source
                && source.Parent is TabStrip sourceStrip
                && sourceStrip.DataContext is IDock sourceLayout
                && panel.DataContext is IDock targetLayout
                && sourceLayout != targetLayout)
            {
                int sourceIndex = sourceStrip.ItemContainerGenerator.IndexFromContainer(source);
                int targetIndex = targetLayout.Views.Count;

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
                                sourceLayout.MoveView(targetLayout, sourceIndex, targetIndex);
                                break;

                            case DockOperation.Left:
                                sourceLayout.SplitLayout(source.DataContext, operation);
                                break;
                        }
                    }
                    return true;
                }
                else if (e.DragEffects == DragDropEffects.Link)
                {
                    if (bExecute)
                    {
                        sourceLayout.SwapView(targetLayout, sourceIndex, targetIndex);
                    }
                    return true;
                }

                return false;
            }

            return false;
        }

        private bool Validate(IDock layout, object context, object sender, DragEventArgs e, DockOperation operation, bool bExecute)
        {
            var point = DropHelper.GetPosition(sender, e);

            switch (sender)
            {
                case TabStrip strip:
                    return ValidateTabStrip(layout, e, bExecute, operation, strip);
                case DockPanel panel:
                    return ValidateDockPanel(layout, e, bExecute, operation, panel);
            }

            if (e.Data.Get(DragDataFormats.Parent) is TabStripItem item)
            {
                var strip = item.Parent as TabStrip;
                if (strip.DataContext is IDock container)
                {
                    if (bExecute)
                    {
                        switch(operation)
                        {
                            case DockOperation.Fill:
                                int itemIndex = strip.ItemContainerGenerator.IndexFromContainer(item);
                                var position = DropHelper.GetPositionScreen(sender, e);

                                // WIP: This is work in progress.
                                //var splitLayout = container.SplitLayout(context, SplitDirection.Left);
                                //layout.ReplaceView(container, splitLayout);

                                var window = layout.CurrentView.CreateWindow(container, itemIndex, context);
                                window.X = position.X;
                                window.Y = position.Y;
                                window.Width = 300;
                                window.Height = 400;
                                window.Id = "Dock";
                                window.Title = "Dock";
                                window.Layout.Title = "Dock";
                                window.Present(false);

                                return true;

                            default:
                                System.Console.WriteLine($"DockSplit: {operation}");
                                break;
                        }                        
                    }
                    return true;
                }
            }

            return false;
        }

        public bool Validate(object context, object sender, DockOperation split, DragEventArgs e)
        {
            if (context is IDock layout)
            {
                return Validate(layout, layout.Context, sender, e,  split, false);
            }
            return false;
        }

        public bool Execute(object context, object sender, DockOperation split, DragEventArgs e)
        {
            if (context is IDock layout)
            {
                return Validate(layout, layout.Context, sender, e, split, true);
            }
            return false;
        }
    }
}
