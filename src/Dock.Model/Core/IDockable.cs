// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model
{
    /// <summary>
    /// Dockable contract.
    /// </summary>
    public interface IDockable
    {
        /// <summary>
        /// Gets or sets dockable id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets dockable title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets dockable context.
        /// </summary>
        object? Context { get; set; }

        /// <summary>
        /// Gets or sets dockable owner.
        /// </summary>
        /// <remarks>If owner is null than dockable is root.</remarks>
        IDockable? Owner { get; set; }

        /// <summary>
        /// Gets or sets dockable factory.
        /// </summary>
        IFactory? Factory { get; set; }

        /// <summary>
        /// Called when the dockable is closed.
        /// </summary>
        /// <returns>true to accept the close, and false to cancel the close.</returns>
        bool OnClose();

        /// <summary>
        /// Called when the dockable becomes the selected dockable.
        /// </summary>
        void OnSelected();

        /// <summary>
        /// Clones <see cref="IDockable"/> object.
        /// </summary>
        /// <returns>The new instance or reference of the <see cref="IDockable"/> class.</returns>
        IDockable Clone();
    }
}
