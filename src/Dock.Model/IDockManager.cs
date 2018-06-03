// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model
{
    /// <summary>
    /// Docking manager contract.
    /// </summary>
    public interface IDockManager
    {
        /// <summary>
        /// Gets or sets pointer position relative to event source.
        /// </summary>
        DockPoint Position { get; set; }

        /// <summary>
        /// Gets or sets pointer position relative to event source and translated to screen coordinates.
        /// </summary>
        DockPoint ScreenPosition { get; set; }

        /// <summary>
        /// Validates docking operation.
        /// </summary>
        /// <param name="sourceView">The source view.</param>
        /// <param name="targetView">The target view.</param>
        /// <param name="action">The drag action.</param>
        /// <param name="operation">The dock operation.</param>
        /// <param name="bExecute">The flag indicating whether to execute.</param>
        /// <returns>True if docking operation can be executed otherwise false.</returns>
        bool Validate(IView sourceView, IView targetView, DragAction action, DockOperation operation, bool bExecute);
    }
}
