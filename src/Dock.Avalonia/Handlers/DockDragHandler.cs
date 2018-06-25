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
            if (context is IView view)
            {
                if (view.Parent is IDock dock
                    && dock.Factory is IDockFactory factory
                    && factory.FindRoot(dock) is IDock root
                    && root.CurrentView is IDock currentViewRoot)
                {
                    currentViewRoot.ShowWindows();
                }
            }
        }

        /// <inheritdoc/>
        public void AfterDragDrop(object sender, PointerEventArgs e, object context)
        {
        }
    }
}
