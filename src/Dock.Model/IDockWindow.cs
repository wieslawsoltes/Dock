// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        /// Gets or sets window context.
        /// </summary>
        object Context { get; set; }

        /// <summary>
        /// Gets or sets window owner view.
        /// </summary>
        IView Owner { get; set; }

        /// <summary>
        /// Gets or sets dock factory.
        /// </summary>
        IDockFactory Factory { get; set; }

        /// <summary>
        /// Gets or sets views layout.
        /// </summary>
        IDock Layout { get; set; }

        /// <summary>
        /// Gets or sets dock window.
        /// </summary>
        IDockHost Host { get; set; }

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
    }
}
