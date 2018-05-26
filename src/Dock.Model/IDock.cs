// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace Dock.Model
{
    /// <summary>
    /// Navigation contract.
    /// </summary>
    public interface IDockNavigation
    {
        /// <summary>
        /// Gets or sets views.
        /// </summary>
        IList<IDock> Views { get; set; }

        /// <summary>
        /// Gets or sets current view.
        /// </summary>
        IDock CurrentView { get; set; }

        /// <summary>
        /// Gets or sets default view.
        /// </summary>
        IDock DefaultView { get; set; }

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
    }

    /// <summary>
    /// Windows contract.
    /// </summary>
    public interface IDockWindows
    {
        /// <summary>
        /// Gets or sets windows.
        /// </summary>
        IList<IDockWindow> Windows { get; set; }

        /// <summary>
        /// Show windows.
        /// </summary>
        void ShowWindows();

        /// <summary>
        /// Hide windows.
        /// </summary>
        void HideWindows();
    }

    /// <summary>
    /// Dock contract.
    /// </summary>
    public interface IDock : IDockNavigation, IDockWindows
    {
        /// <summary>
        /// Gets or sets dock id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets dock position.
        /// </summary>
        string Dock { get; set; }

        /// <summary>
        /// Gets or sets dock width.
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Gets or sets dock height.
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Gets or sets dock title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets dock context.
        /// </summary>
        object Context { get; set; }

        /// <summary>
        /// Gets or sets dock parent.
        /// </summary>
        /// <remarks>If parrent is <see cref="null"/> than dock is root.</remarks>
        IDock Parent { get; set; }

        /// <summary>
        /// Gets or sets dock factory.
        /// </summary>
        IDockFactory Factory { get; set; }
    }
}
