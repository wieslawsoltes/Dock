// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace Dock.Model
{
    /// <summary>
    /// Dock contract.
    /// </summary>
    public interface IDock
    {
        /// <summary>
        /// Gets or sets dock.
        /// </summary>
        string Dock { get; set; }

        /// <summary>
        /// Gets or sets width.
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Gets or sets height.
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Gets view title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets view context.
        /// </summary>
        object Context { get; set; }

        /// <summary>
        /// Gets or sets views.
        /// </summary>
        IList<IDock> Views { get; set; }

        /// <summary>
        /// Gets or sets current view.
        /// </summary>
        IDock CurrentView { get; set; }

        /// <summary>
        /// Gets or sets windows.
        /// </summary>
        IList<IDockWindow> Windows { get; set; }

        /// <summary>
        /// Gets or sets dock factory.
        /// </summary>
        IDockFactory Factory { get; set; }

        /// <summary>
        /// Change current view.
        /// </summary>
        /// <param name="view">The view instance.</param>
        void OnChangeCurrentView(IDock view);

        /// <summary>
        /// Change current view.
        /// </summary>
        /// <param name="title">The view title.</param>
        void OnChangeCurrentView(string title);

        /// <summary>
        /// Show windows.
        /// </summary>
        void ShowWindows();

        /// <summary>
        /// Close windows.
        /// </summary>
        void CloseWindows();

        /// <summary>
        /// Adds window.
        /// </summary>
        /// <param name="window">The window to add.</param>
        void AddWindow(IDockWindow window);

        /// <summary>
        /// Removes window.
        /// </summary>
        /// <param name="window">The window to remove.</param>
        void RemoveWindow(IDockWindow window);
    }
}
