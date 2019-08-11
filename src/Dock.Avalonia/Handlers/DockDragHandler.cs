// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Dock drag handler.
    /// </summary>
    public class DockDragHandler : IDragHandler
    {
        /// <inheritdoc/>
        public void BeforeDragDrop(object sender, PointerEventArgs e, object context)
        {
            if (context is IDockable dockable)
            {
                if (dockable.Owner is IDock dock
                    && dock.Factory is IFactory factory
                    && factory.FindRoot(dock) is IDock root
                    && root.CurrentDockable is IDock currentRootDockable)
                {
                    currentRootDockable.ShowWindows();
                }
            }
        }

        /// <inheritdoc/>
        public void AfterDragDrop(object sender, PointerEventArgs e, object context)
        {
        }
    }
}
