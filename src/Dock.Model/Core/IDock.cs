// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Dock.Model
{
    /// <summary>
    /// Dock contract.
    /// </summary>
    public interface IDock : IDockable
    {
        /// <summary>
        /// Gets or sets visible dockables.
        /// </summary>
        IList<IDockable>? VisibleDockables { get; set; }

        /// <summary>
        /// Gets or sets hidden dockables.
        /// </summary>
        IList<IDockable>? HiddenDockables { get; set; }

        /// <summary>
        /// Gets or sets pinned dockables.
        /// </summary>
        IList<IDockable>? PinnedDockables { get; set; }

        /// <summary>
        /// Gets or sets avtive dockable.
        /// </summary>
        IDockable? ActiveDockable { get; set; }

        /// <summary>
        /// Gets or sets default dockable.
        /// </summary>
        IDockable? DefaultDockable { get; set; }

        /// <summary>
        /// Gets or sets the focused dockable.
        /// </summary>
        IDockable? FocusedDockable { get; set; }

        /// <summary> 
        /// Gets or sets splitter proportion. 
        /// </summary> 
        double Proportion { get; set; }

        /// <summary>
        /// Gets or sets if the dockable is the currently active.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets if the Dock collapses when all its children are removed.
        /// </summary>
        bool IsCollapsable { get; set; }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in back navigation history.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in forward navigation history.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// Navigates to the most recent entry in back navigation history, if there is one.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Navigate to the most recent entry in forward navigation history, if there is one.
        /// </summary>
        void GoForward();

        /// <summary>
        /// Navigate to content that is contained by an object.
        /// </summary>
        /// <param name="root">An object that contains the content to navigate to.</param>
        void Navigate(object root);

        /// <summary>
        /// Close layout.
        /// </summary>
        void Close();
    }
}
