// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model
{
    /// <summary>
    /// Dock host contract.
    /// </summary>
    public interface IDockHost
    {
        /// <summary>
        /// Presents host.
        /// </summary>
        /// <param name="isDialog">The value that indicates whether window is dialog.</param>
        void Present(bool isDialog);

        /// <summary>
        /// Destroys host.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Exits host.
        /// </summary>
        void Exit();

        /// <summary>
        /// Sets host position.
        /// </summary>
        /// <param name="x">The X coordinate of host.</param>
        /// <param name="y">The Y coordinate of host.</param>
        void SetPosition(double x, double y);

        /// <summary>
        /// Gets host position.
        /// </summary>
        /// <param name="x">The X coordinate of host.</param>
        /// <param name="y">The Y coordinate of host.</param>
        void GetPosition(out double x, out double y);

        /// <summary>
        /// Sets host size.
        /// </summary>
        /// <param name="width">The host width.</param>
        /// <param name="height">The host height.</param>
        void SetSize(double width, double height);

        /// <summary>
        /// Gets host size.
        /// </summary>
        /// <param name="width">The host width.</param>
        /// <param name="height">The host height.</param>
        void GetSize(out double width, out double height);

        /// <summary>
        /// Sets host title.
        /// </summary>
        /// <param name="title">The host title.</param>
        void SetTitle(string title);

        /// <summary>
        /// Sets host context.
        /// </summary>
        /// <param name="context">The host context.</param>
        void SetContext(object context);

        /// <summary>
        /// Sets host layout.
        /// </summary>
        /// <param name="layout">The host layout.</param>
        void SetLayout(IDock layout);
    }
}
