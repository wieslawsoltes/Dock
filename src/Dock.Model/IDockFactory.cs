// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model
{
    /// <summary>
    /// Dock factory contract.
    /// </summary>
    public interface IDockFactory
    {
        /// <summary>
        /// Gets or sets <see cref="IView.Context"/> locator registry.
        /// </summary>
        IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IDockHost"/> locator registry.
        /// </summary>
        IDictionary<string, Func<IDockHost>> HostLocator { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IView"/> locator registry.
        /// </summary>
        IDictionary<string, Func<IView>> ViewLocator { get; set; }

        /// <summary>
        /// Creates list of type <see cref="IList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The list item type.</typeparam>
        /// <param name="items">The initial list items.</param>
        /// <returns>The new instance of <see cref="IList{T}"/>.</returns>
        IList<T> CreateList<T>(params T[] items);

        /// <summary>
        /// Creates <see cref="IRootDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IRootDock"/> class.</returns>
        IRootDock CreateRootDock();

        /// <summary>
        /// Creates <see cref="IPinDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IPinDock"/> class.</returns>
        IPinDock CreatePinDock();

        /// <summary>
        /// Creates <see cref="ILayoutDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="ILayoutDock"/> class.</returns>
        ILayoutDock CreateLayoutDock();

        /// <summary>
        /// Creates <see cref="ISplitterDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="ISplitterDock"/> class.</returns>
        ISplitterDock CreateSplitterDock();

        /// <summary>
        /// Creates <see cref="IToolDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IToolDock"/> class.</returns>
        IToolDock CreateToolDock();

        /// <summary>
        /// Creates <see cref="IDocumentDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IDocumentDock"/> class.</returns>
        IDocumentDock CreateDocumentDock();

        /// <summary>
        /// Creates <see cref="IDockWindow"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IDockWindow"/> class.</returns>
        IDockWindow CreateDockWindow();

        /// <summary>
        /// Creates <see cref="IToolTab"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IToolTab"/> class.</returns>
        IToolTab CreateToolTab();

        /// <summary>
        /// Creates <see cref="IDocumentTab"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IDocumentTab"/> class.</returns>
        IDocumentTab CreateDocumentTab();

        /// <summary>
        /// Creates <see cref="IView"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IView"/> class.</returns>
        IView CreateView();

        /// <summary>
        /// Creates <see cref="IDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateDock();

        /// <summary>
        /// Creates layout.
        /// </summary>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateLayout();

        /// <summary>
        /// Initialize layout.
        /// </summary>
        /// <param name="layout">The layout to initialize.</param>
        void InitLayout(IView layout);

        /// <summary>
        /// Gets context.
        /// </summary>
        /// <param name="id">The object id.</param>
        /// <returns>The located context.</returns>
        object GetContext(string id);

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
        /// <param name="owner">The window owner view.</param>
        void Update(IDockWindow window, IView owner);

        /// <summary>
        /// Update view.
        /// </summary>
        /// <param name="view">The view to update.</param>
        /// <param name="parent">The parent view.</param>
        void Update(IView view, IView parent);

        /// <summary>
        /// Adds <see cref="IView"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The parent dock.</param>
        /// <param name="view">The view to add.</param>
        void AddView(IDock dock, IView view);

        /// <summary>
        /// Inserts <see cref="IView"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The parent dock.</param>
        /// <param name="view">The view to add.</param>
        /// <param name="index">The view index.</param>
        void InsertView(IDock dock, IView view, int index);

        /// <summary>
        /// Adds window into dock windows list.
        /// </summary>
        /// <param name="dock">The window dock.</param>
        /// <param name="window">The window to add.</param>
        void AddWindow(IDock dock, IDockWindow window);

        /// <summary>
        /// Removes window from owner windows list.
        /// </summary>
        /// <param name="window">The window to remove.</param>
        void RemoveWindow(IDockWindow window);

        /// <summary>
        /// Sets a currently selected view. If the view is contained inside an dock it
        /// will become the selected tab.
        /// </summary>
        /// <param name="view">The view to select.</param>
        void SetCurrentView(IView view);

        /// <summary>
        /// Sets the currently focused view updating IsActive flags.
        /// </summary>
        /// <param name="dock">The dock to set the focused view on.</param>
        /// <param name="view">The view to set.</param>
        void SetFocusedView(IDock dock, IView view);

        /// <summary>
        /// Searches for root view.
        /// </summary>
        /// <param name="view">The view to find root for.</param>
        /// <returns>The root view instance or null if root view was not found.</returns>
        IView FindRoot(IView view);

        /// <summary>
        /// Searches dock for view.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="predicate">The predicate to filter views.</param>
        /// <returns>The view instance or null if view was not found.</returns>
        IView FindView(IDock dock, Func<IView, bool> predicate);

        /// <summary>
        /// Pins views.
        /// </summary>
        /// <param name="view">The view to pin.</param>
        void PinView(IView view);

        /// <summary>
        /// Removes view from parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="view">The view to remove.</param>
        void RemoveView(IView view);

        /// <summary>
        /// Removes view from parent <see cref="IDock.Views"/> collection, and call IView.OnClose.
        /// </summary>
        /// <param name="view">The view to remove.</param>
        void CloseView(IView view);

        /// <summary>
        /// Moves view inside <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="sourceView">The source view.</param>
        /// <param name="targetView">The target view.</param>
        void MoveView(IDock dock, IView sourceView, IView targetView);

        /// <summary>
        /// Moves view into another <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="sourceDock">The source views dock.</param>
        /// <param name="targetDock">The target views dock.</param>
        /// <param name="sourceView">The source view.</param>
        /// <param name="targetView">The target view.</param>
        void MoveView(IDock sourceDock, IDock targetDock, IView sourceView, IView targetView);

        /// <summary>
        /// Moves view into another <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="first">The first view.</param>
        /// <param name="second">The second view.</param>
        void Move(IView first, IView second);

        /// <summary>
        /// Swaps view in parents <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="first">The first view.</param>
        /// <param name="second">The second view.</param>
        void Swap(IView first, IView second);

        /// <summary>
        /// Swaps view in inside <see cref="IDock.Views"/> collections.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="sourceView">The source view.</param>
        /// <param name="targetView">The target view.</param>
        void SwapView(IDock dock, IView sourceView, IView targetView);

        /// <summary>
        /// Swaps view into between <see cref="IDock.Views"/> collections.
        /// </summary>
        /// <param name="sourceDock">The source views dock.</param>
        /// <param name="targetDock">The target views dock.</param>
        /// <param name="sourceView">The source view.</param>
        /// <param name="targetView">The target view.</param>
        void SwapView(IDock sourceDock, IDock targetDock, IView sourceView, IView targetView);

        /// <summary>
        /// Replaces source view with destination view in source view parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="source">The source view.</param>
        /// <param name="destination">The destination view.</param>
        void Replace(IView source, IView destination);

        /// <summary>
        /// Creates a new split layout from source view.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        /// <param name="view">The optional view to add to splitted side.</param>
        /// <param name="operation">The dock operation.</param>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateSplitLayout(IDock dock, IView view, DockOperation operation);

        /// <summary>
        /// Splits dock and updates parent layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        /// <param name="view">The optional view to add to splitted side.</param>
        /// <param name="operation"> The dock operation to perform.</param>
        void Split(IDock dock, IView view, DockOperation operation);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Fill"/> and updates <see cref="IView.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToFill(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Left"/> and updates <see cref="IView.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToLeft(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Right"/> and updates <see cref="IView.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToRight(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Top"/> and updates <see cref="IView.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToTop(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Bottom"/> and updates <see cref="IView.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToBottom(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Window"/> and updates <see cref="IView.Parent"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToWindow(IDock dock);

        /// <summary>
        /// Creates dock window from source view.
        /// </summary>
        /// <param name="view">The view to embed into window.</param>
        /// <returns>The new instance of the <see cref="IDockWindow"/> class.</returns>
        IDockWindow CreateWindowFrom(IView view);
    }
}
