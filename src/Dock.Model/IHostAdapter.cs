// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model
{
    /// <summary>
    /// Host adapter contract for the <see cref="IDockWindow"/>.
    /// </summary>
    public interface IHostAdapter
    {
        /// <summary>
        /// Implementation of the <see cref="IDockWindow.Load()"/> method.
        /// </summary>
        void Load();

        /// <summary>
        /// Implementation of the <see cref="IDockWindow.Save()"/> method.
        /// </summary>
        void Save();

        /// <summary>
        /// Implementation of the <see cref="IDockWindow.Present(bool)"/> method.
        /// </summary>
        void Present(bool isDialog);

        /// <summary>
        /// Implementation of the <see cref="IDockWindow.Destroy()"/> method.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Implementation of the <see cref="IDockWindow.Exit()"/> method.
        /// </summary>
        void Exit();
    }
}
