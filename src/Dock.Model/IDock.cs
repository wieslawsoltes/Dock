// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Dock.Model
{
    /// <summary>
    /// Dock contract.
    /// </summary>
    public interface IDock : IView
    {
        /// <summary>
        /// Gets or sets views.
        /// </summary>
        IList<IView> Views { get; set; }

        /// <summary>
        /// Gets or sets current view.
        /// </summary>
        IView CurrentView { get; set; }

        /// <summary>
        /// Gets or sets default view.
        /// </summary>
        IView DefaultView { get; set; }

        /// <summary>
        /// Gets or sets the focused view.
        /// </summary>
        IView FocusedView { get; set; }

        /// <summary>
        /// Gets or sets if the view is the currently active.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in back navigation history.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in forward navigation history.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// Gets or sets windows.
        /// </summary>
        IList<IDockWindow> Windows { get; set; }

        /// <summary>
        /// Gets or sets dock factory.
        /// </summary>
        IDockFactory Factory { get; set; }

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
        /// Show windows.
        /// </summary>
        void ShowWindows();

        /// <summary>
        /// Exit windows.
        /// </summary>
        void ExitWindows();

        /// <summary>
        /// Close layout.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets or sets if the Dock collapses when all its children are removed.
        /// </summary>
        bool IsCollapsable { get; set; }
    }
}
