// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dock.Model.Controls;

namespace Dock.Model
{
    /// <summary>
    /// Dock window contract.
    /// </summary>
    public interface IDockWindow
    {
        /// <summary>
        /// Gets or sets id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets window X coordinate.
        /// </summary>
        double X { get; set; }

        /// <summary>
        /// Gets or sets window X coordinate.
        /// </summary>
        double Y { get; set; }

        /// <summary>
        /// Gets or sets window width.
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Gets or sets window height.
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Gets or sets whether this window appears on top of all other windows.
        /// </summary>
        bool Topmost { get; set; }

        /// <summary>
        /// Gets or sets window title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets window owner dockable.
        /// </summary>
        IDockable? Owner { get; set; }

        /// <summary>
        /// Gets or sets dock factory.
        /// </summary>
        IFactory? Factory { get; set; }

        /// <summary>
        /// Gets or sets layout.
        /// </summary>
        IRootDock? Layout { get; set; }

        /// <summary>
        /// Gets or sets dock window.
        /// </summary>
        IHostWindow? Host { get; set; }

        /// <summary>
        /// Saves window properties.
        /// </summary>
        void Save();

        /// <summary>
        /// Presents window.
        /// </summary>
        /// <param name="isDialog">The value that indicates whether window is dialog.</param>
        void Present(bool isDialog);

        /// <summary>
        /// Exits window.
        /// </summary>
        void Exit();

        /// <summary>
        /// Clones <see cref="IDockWindow"/> object.
        /// </summary>
        /// <returns>The new instance or reference of the <see cref="IDockWindow"/> class.</returns>
        IDockWindow? Clone();
    }
}
