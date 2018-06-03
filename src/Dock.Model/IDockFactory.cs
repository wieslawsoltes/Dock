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
        /// Gets or sets <see cref="IDock.Context"/> locator registry.
        /// </summary>
        IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IDockHost"/> locator registry.
        /// </summary>
        IDictionary<string, Func<IDockHost>> HostLocator { get; set; }

        /// <summary>
        /// Creates <see cref="IRootDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IRootDock"/> class.</returns>
        IRootDock CreateRootDock();

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
        /// <param name="context">The context object.</param>
        void InitLayout(IView layout, object context);

        /// <summary>
        /// Closes layout.
        /// </summary>
        /// <param name="layout">The layout to close.</param>
        void CloseLayout(IView layout);

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
        /// <param name="owner">The window owner view.</param>
        void Update(IDockWindow window, object context, IView owner);

        /// <summary>
        /// Update view.
        /// </summary>
        /// <param name="view">The view to update.</param>
        /// <param name="context">The context object.</param>
        /// <param name="parent">The parent view.</param>
        void Update(IView view, object context, IView parent);

        /// <summary>
        /// Selects a view. If the view is contained inside an IViewHost it
        /// will become the selected tab.
        /// </summary>
        /// <param name="view">The view to select.</param>
        void Select(IView view);

        /// <summary>
        /// Searches for root view.
        /// </summary>
        /// <param name="view">The view to find root for.</param>
        /// <returns>The root view instance or null if root view was not found.</returns>
        IView FindRoot(IView view);

        /// <summary>
        /// Removes view from parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="view">The view to remove.</param>
        void Remove(IView view);

        /// <summary>
        /// Removes view from the <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="host">The views host.</param>
        /// <param name="index">The source view index.</param>
        void RemoveView(IViewsHost host, int index);

        /// <summary>
        /// Moves view inside <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="host">The views host.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        void MoveView(IViewsHost host, int sourceIndex, int targetIndex);

        /// <summary>
        /// Moves view into another <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="sourceHost">The source views dock.</param>
        /// <param name="targetHost">The target views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        void MoveView(IViewsHost sourceHost, IViewsHost targetHost, int sourceIndex, int targetIndex);

        /// <summary>
        /// Moves view into another <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="view">The view to move.</param>
        /// <param name="targetHost">The target host.</param>
        void MoveTo(IView view, IViewsHost targetHost);

        /// <summary>
        /// Swaps view in parents <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="first">The first view.</param>
        /// <param name="parent">The second view.</param>
        void Swap(IView first, IView second);

        /// <summary>
        /// Swaps view in inside <see cref="IViewsHost.Views"/> collections.
        /// </summary>
        /// <param name="host">The views host.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        void SwapView(IViewsHost host, int sourceIndex, int targetIndex);

        /// <summary>
        /// Swaps view into between <see cref="IViewsHost.Views"/> collections.
        /// </summary>
        /// <param name="sourceHost">The source views host.</param>
        /// <param name="targetHost">The target views host.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        void SwapView(IViewsHost sourceHost, IViewsHost targetHost, int sourceIndex, int targetIndex);

        /// <summary>
        /// Replaces source view with destination view in source view parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="source">The source view.</param>
        /// <param name="destination">The destination view.</param>
        void Replace(IView source, IView destination);

        /// <summary>
        /// Creates a new split layout from source view.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        /// <param name="view">The optional view to add to splitted side.</param>
        /// <param name="context">The context object.</param>
        /// <param name="operation">The dock operation.</param>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateSplitLayout(IDock dock, IView view, object context, DockOperation operation);

        /// <summary>
        /// Splits dock and updates parent layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        /// <param name="view">The optional view to add to splitted side.</param>
        /// <param name="operation"> The dock operation to perform.</param>
        void Split(IDock dock, IView view, DockOperation operation);

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
        /// Adds <see cref="Controls.ILayoutDock"/> to dock <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddLayout(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IRootDock"/> to dock <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddRoot(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.ISplitterDock"/> to dock <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddSplitter(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IDocumentDock"/> to dock <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddDocument(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IToolDock"/> to dock <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddTool(IDock dock);

        /// <summary>
        /// Adds <see cref="IView"/> to dock <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddView(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IToolTab"/> to dock <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddToolTab(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IDocumentTab"/> to dock <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddDocumentTab(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ILayoutDock"/> before dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertLayoutBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IRootDock"/> before dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertRootBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ISplitterDock"/> before dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertSplitterBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentDock"/> before dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolDock"/> before dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="IView"/> before dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertViewBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolTab"/> before dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolTabBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentTab"/> before dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentTabBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ILayoutDock"/> after dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertLayoutAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IRootDock"/> after dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertRootAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ISplitterDock"/> after dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertSplitterAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentDock"/> after dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolDock"/> after dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="IView"/> after dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertViewAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolTab"/> after dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolTabAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentTab"/> after dock in parent <see cref="IViewsHost.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentTabAfter(IDock dock);

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
        /// Converts dock to <see cref="IView"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToView(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IToolTab"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToToolTab(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IDocumentTab"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToDocumentTab(IDock dock);

        /// <summary>
        /// Creates dock window from source view.
        /// </summary>
        /// <param name="view">The view to embed into window.</param>
        /// <returns>The new instance of the <see cref="IDockWindow"/> class.</returns>
        IDockWindow CreateWindowFrom(IView view);

        /// <summary>
        /// Adds window to host windows list.
        /// </summary>
        /// <param name="host">The window host.</param>
        /// <param name="window">The window to add.</param>
        /// <param name="context">The context for dock window.</param>
        void AddWindow(IWindowsHost host, IDockWindow window, object context);

        /// <summary>
        /// Removes window from owner windows list.
        /// </summary>
        /// <param name="window">The window to remove.</param>
        void RemoveWindow(IDockWindow window);
    }
}
