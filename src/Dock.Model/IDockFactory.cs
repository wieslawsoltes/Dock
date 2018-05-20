// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;

namespace Dock.Model
{
    /// <summary>
    /// Dock factory contract.
    /// </summary>
    public interface IDockFactory
    {
        /// <summary>
        /// Gets or sets  <see cref="IDock.Context"/> locator registry.
        /// </summary>
        IDictionary<Type, Func<object>> ContextLocator { get; set; }

        /// <summary>
        /// Gets context.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="context">The default context.</param>
        /// <returns>The located context or default context.</returns>
        object GetContext(object source, object context);

        /// <summary>
        /// Updates window.
        /// </summary>
        /// <param name="window">The window to update.</param>
        /// <param name="context">The context object.</param>
        void Update(IDockWindow window, object context);

        /// <summary>
        /// Updates windows.
        /// </summary>
        /// <param name="windows">The windows to update.</param>
        /// <param name="context">The context object.</param>
        void Update(IList<IDockWindow> windows, object context);

        /// <summary>
        /// Update view.
        /// </summary>
        /// <param name="view">The view to update.</param>
        /// <param name="context">The context object.</param>
        void Update(IDock view, object context);

        /// <summary>
        /// Updates views.
        /// </summary>
        /// <param name="views">The views to update.</param>
        /// <param name="context">The context object.</param>
        void Update(IList<IDock> views, object context);

        /// <summary>
        /// Creates dock window from view.
        /// </summary>
        /// <param name="layout">The owner layout.</param>
        /// <param name="context">The context object.</param>
        /// <param name="source">The source layout.</param>
        /// <param name="viewIndex">The source view index.</param>
        /// <param name="x">The X coordinate of window.</param>
        /// <param name="y">The Y coordinate of window.</param>
        /// <returns>The new instance of the <see cref="IDockWindow"/> class.</returns>
        IDockWindow CreateWindow(IDock layout, object context, IDock source, int viewIndex, double x, double y);

        /// <summary>
        /// Creates default layout.
        /// </summary>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateDefaultLayout();

        /// <summary>
        /// Creates new or updates current layout.
        /// </summary>
        void CreateOrUpdateLayout();
    }
}
