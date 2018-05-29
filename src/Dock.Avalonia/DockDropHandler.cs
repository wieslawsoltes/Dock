// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;

namespace Dock.Avalonia
{
    public class DockDropHandler : IDropHandler
    {
        private DockManager _manager = new DockManager();

        private bool _executed = false;

        public int Id { get; set; }

        private DragAction ToDragAction(DragEventArgs e)
        {
            if (e.DragEffects == DragDropEffects.Copy)
            {
                return DragAction.Copy;
            }
            else if (e.DragEffects == DragDropEffects.Move)
            {
                return DragAction.Move;
            }
            else if (e.DragEffects == DragDropEffects.Link)
            {
                return DragAction.Link;
            }
            return DragAction.None;
        }

        private DockPoint ToDockPoint(Point point)
        {
            return new DockPoint(point.X, point.Y);
        }

        public bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                _manager.Position = ToDockPoint(DropHelper.GetPosition(sender, e));
                _manager.ScreenPosition = ToDockPoint(DropHelper.GetPositionScreen(sender, e));
                Console.WriteLine($"Validate [{Id}]: {sourceDock.Title} -> {targetDock.Title} [{operation}] [{_manager.Position}] [{_manager.ScreenPosition}]");
                return _manager.ValidateDock(sourceDock, targetDock, ToDragAction(e), operation, false);
            }
            return false;
        }

        public bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e)
        {
            if (_executed == false && sourceContext is IDock sourceDock && targetContext is IDock targetDock)
            {
                _manager.Position = ToDockPoint(DropHelper.GetPosition(sender, e));
                _manager.ScreenPosition = ToDockPoint(DropHelper.GetPositionScreen(sender, e));
                Console.WriteLine($"Execute [{Id}]: {sourceDock.Title} -> {targetDock.Title} [{operation}] [{_manager.Position}] [{_manager.ScreenPosition}]");
                bool bResult = _manager.ValidateDock(sourceDock, targetDock, ToDragAction(e), operation, true);
                if (bResult == true)
                {
                    Console.WriteLine($"Executed [{Id}]: {sourceDock.Title} -> {targetDock.Title} [{operation}] [{_manager.Position}] [{_manager.ScreenPosition}]");
                    _executed = true;
                    return true;
                }
                return false;
            }
            return false;
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
            _executed = false;
        }
    }
}
