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
        /// Gets or sets <see cref="IDock.Context"/> locator registry.
        /// </summary>
        IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IDockHost"/> locator registry.
        /// </summary>
        IDictionary<string, Func<IDockHost>> HostLocator { get; set; }

        /// <summary>
        /// Gets context.
        /// </summary>
        /// <param name="id">The object id.</param>
        /// <param name="context">The default context.</param>
        /// <returns>The located context or default context.</returns>
        object GetContext(string id, object context);

        /// <summary>
        /// Gets host.
        /// </summary>
        /// <param name="id">The host id.</param>
        /// <returns>The located host.</returns>
        IDockHost GetHost(string id);

        /// <summary>
        /// Updates window.
        /// </summary>
        /// <param name="window">The window to update.</param>
        /// <param name="context">The context object.</param>
        /// <param name="owner">The window owner dock.</param>
        void Update(IDockWindow window, object context, IDock owner);

        /// <summary>
        /// Updates windows.
        /// </summary>
        /// <param name="windows">The windows to update.</param>
        /// <param name="context">The context object.</param>
        /// <param name="owner">The window owner dock.</param>
        void Update(IList<IDockWindow> windows, object context, IDock owner);

        /// <summary>
        /// Update view.
        /// </summary>
        /// <param name="view">The view to update.</param>
        /// <param name="context">The context object.</param>
        /// <param name="parent">The view parent dock.</param>
        void Update(IDock view, object context, IDock parent);

        /// <summary>
        /// Updates views.
        /// </summary>
        /// <param name="views">The views to update.</param>
        /// <param name="context">The context object.</param>
        /// <param name="parent">The view parent dock.</param>
        void Update(IList<IDock> views, object context, IDock parent);

        /// <summary>
        /// Searches for root layout.
        /// </summary>
        /// <param name="dock">The dock to find root for.</param>
        /// <returns>The root layout instance or null if root layout was not found.</returns>
        IDock FindRootLayout(IDock dock);

        /// <summary>
        /// Removes dock from parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock to remove.</param>
        void Remove(IDock dock);

        /// <summary>
        /// Removes view from the dock.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="index">The source view index.</param>
        void RemoveView(IDock dock, int index);

        /// <summary>
        /// Moves view in the dock.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        void MoveView(IDock dock, int sourceIndex, int targetIndex);

        /// <summary>
        /// Moves view into another dock.
        /// </summary>
        /// <param name="sourceDock">The source views dock.</param>
        /// <param name="targetDock">The target views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        void MoveView(IDock sourceDock, IDock targetDock, int sourceIndex, int targetIndex);

        /// <summary>
        /// Moves dock to the destination parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock to move.</param>
        /// <param name="parent">The destination parent.</param>
        void MoveTo(IDock dock, IDock parent);

        /// <summary>
        /// Swaps docks in parents <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The first dock.</param>
        /// <param name="parent">The second dock.</param>
        void Swap(IDock first, IDock second);

        /// <summary>
        /// Swaps view in the dock.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        void SwapView(IDock dock, int sourceIndex, int targetIndex);

        /// <summary>
        /// Swaps view into another dock.
        /// </summary>
        /// <param name="sourceDock">The source views dock.</param>
        /// <param name="targetDock">The target views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        void SwapView(IDock sourceDock, IDock targetDock, int sourceIndex, int targetIndex);

        /// <summary>
        /// Replaces source dock with destination dock in source dock parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="source">The source dock.</param>
        /// <param name="destination">The destination dock.</param>
        void Replace(IDock source, IDock destination);

        /// <summary>
        /// Creates a new split layout from source dock.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        /// <param name="view">The optional view dock to add to splitted side.</param>
        /// <param name="context">The context object.</param>
        /// <param name="operation">The dock operation.</param>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateSplitLayout(IDock dock, IDock view, object context, DockOperation operation);

        /// <summary>
        /// Splits dock and updates parent layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        /// <param name="view">The optional view dock to add to splitted side.</param>
        /// <param name="operation"> The dock operation to perform.</param>
        void Split(IDock dock, IDock view, DockOperation operation);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Fill"/> and updates <see cref="IDock.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToFill(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Left"/> and updates <see cref="IDock.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToLeft(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Right"/> and updates <see cref="IDock.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToRight(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Top"/> and updates <see cref="IDock.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToTop(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Bottom"/> and updates <see cref="IDock.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToBottom(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Window"/> and updates <see cref="IDock.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToWindow(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.ILayoutDock"/> to dock <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddLayout(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IRootDock"/> to dock <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddRoot(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.ISplitterDock"/> to dock <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddSplitter(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IDocumentDock"/> to dock <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddDocument(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IToolDock"/> to dock <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddTool(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IViewDock"/> to dock <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddView(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ILayoutDock"/> before dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertLayoutBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IRootDock"/> before dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertRootBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ISplitterDock"/> before dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertSplitterBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentDock"/> before dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolDock"/> before dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IViewDock"/> before dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertViewBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ILayoutDock"/> after dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertLayoutAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IRootDock"/> after dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertRootAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ISplitterDock"/> after dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertSplitterAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentDock"/> after dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolDock"/> after dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IViewDock"/> after dock in parent <see cref="IDockNavigation.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertViewAfter(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.ILayoutDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToLayout(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IRootDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToRoot(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.ISplitterDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToSplitter(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IDocumentDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToDocument(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IToolDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToTool(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IViewDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToView(IDock dock);

        /// <summary>
        /// Creates dock window from source view.
        /// </summary>
        /// <param name="source">The source dock to embed into window.</param>
        /// <returns>The new instance of the <see cref="IDockWindow"/> class.</returns>
        IDockWindow CreateWindowFrom(IDock source);

        /// <summary>
        /// Adds window to owner windows list.
        /// </summary>
        /// <param name="owner">The window owner.</param>
        /// <param name="window">The window to add.</param>
        /// <param name="context">The context for dock window.</param>
        void AddWindow(IDock owner, IDockWindow window, object context);

        /// <summary>
        /// Removes window from owner windows list.
        /// </summary>
        /// <param name="window">The window to remove.</param>
        void RemoveWindow(IDockWindow window);

        /// <summary>
        /// Creates layout.
        /// </summary>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateLayout();

        /// <summary>
        /// Initialize layout.
        /// </summary>
        /// <param name="layout">The layout to initialize.</param>
        /// <param name="context">The context object.</param>
        void InitLayout(IDock layout, object context);
    }
}
