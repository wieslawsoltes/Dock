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
    public interface IFactory
    {
        /// <summary>
        /// Gets or sets <see cref="IDockable.Context"/> locator registry.
        /// </summary>
        IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IHostWindow"/> locator registry.
        /// </summary>
        IDictionary<string, Func<IHostWindow>> HostLocator { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IDockable"/> locator registry.
        /// </summary>
        IDictionary<string, Func<IDockable>> DockableLocator { get; set; }

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
        /// Creates <see cref="IProportionalDock"/>.
        /// </summary>
        /// <returns>The new instance of the <see cref="IProportionalDock"/> class.</returns>
        IProportionalDock CreateProportionalDock();

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
        /// Creates layout.
        /// </summary>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateLayout();

        /// <summary>
        /// Initialize layout.
        /// </summary>
        /// <param name="layout">The layout to initialize.</param>
        void InitLayout(IDockable layout);

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
        IHostWindow GetHostWindow(string id);

        /// <summary>
        /// Updates window.
        /// </summary>
        /// <param name="window">The window to update.</param>
        /// <param name="owner">The window owner dockable.</param>
        void UpdateDockable(IDockWindow window, IDockable owner);

        /// <summary>
        /// Update dockable.
        /// </summary>
        /// <param name="dockable">The dockable to update.</param>
        /// <param name="owner">The owner dockable.</param>
        void UpdateDockable(IDockable dockable, IDockable owner);

        /// <summary>
        /// Adds <see cref="IDockable"/> into dock <see cref="IDock.VisibleDockables"/> collection.
        /// </summary>
        /// <param name="dock">The owner dock.</param>
        /// <param name="dockable">The dockable to add.</param>
        void AddDockable(IDock dock, IDockable dockable);

        /// <summary>
        /// Inserts <see cref="IDockable"/> into dock <see cref="IDock.VisibleDockables"/> collection.
        /// </summary>
        /// <param name="dock">The owner dock.</param>
        /// <param name="dockable">The dockable to add.</param>
        /// <param name="index">The dockable index.</param>
        void InsertDockable(IDock dock, IDockable dockable, int index);

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
        /// Sets an avtive dockable. If the dockable is contained inside an dock it
        /// will become the selected dockable.
        /// </summary>
        /// <param name="dockable">The dockable to select.</param>
        void SetAvtiveDockable(IDockable dockable);

        /// <summary>
        /// Sets the currently focused dockable updating IsActive flags.
        /// </summary>
        /// <param name="dock">The dock to set the focused dockable on.</param>
        /// <param name="dockable">The dockable to set.</param>
        void SetFocusedDockable(IDock dock, IDockable dockable);

        /// <summary>
        /// Searches for root dockable.
        /// </summary>
        /// <param name="dockable">The dockable to find root for.</param>
        /// <returns>The root dockable instance or null if root dockable was not found.</returns>
        IDockable FindRoot(IDockable dockable);

        /// <summary>
        /// Searches dock for dockable.
        /// </summary>
        /// <param name="dock">The dock.</param>
        /// <param name="predicate">The predicate to filter dockables.</param>
        /// <returns>The dockable instance or null if dockable was not found.</returns>
        IDockable FindDockable(IDock dock, Func<IDockable, bool> predicate);

        /// <summary>
        /// Pins dockable.
        /// </summary>
        /// <param name="dockable">The dockable to pin.</param>
        void PinDockable(IDockable dockable);

        /// <summary>
        /// Removes dockable from owner <see cref="IDock.VisibleDockables"/> collection.
        /// </summary>
        /// <param name="dockable">The dockable to remove.</param>
        void RemoveDockable(IDockable dockable);

        /// <summary>
        /// Removes dockable from owner <see cref="IDock.VisibleDockables"/> collection, and call IDockable.OnClose.
        /// </summary>
        /// <param name="dockable">The dockable to remove.</param>
        void CloseDockable(IDockable dockable);

        /// <summary>
        /// Moves dockable inside <see cref="IDock.VisibleDockables"/> collection.
        /// </summary>
        /// <param name="dock">The dock.</param>
        /// <param name="sourceDockable">The source dockable.</param>
        /// <param name="targetDockable">The target dockable.</param>
        void MoveDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable);

        /// <summary>
        /// Moves dockable into another <see cref="IDock.VisibleDockables"/> collection.
        /// </summary>
        /// <param name="sourceDock">The source dock.</param>
        /// <param name="targetDock">The target dock.</param>
        /// <param name="sourceDockable">The source dockable.</param>
        /// <param name="targetDockable">The target dockable.</param>
        void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable targetDockable);

        /// <summary>
        /// Moves dockable into another <see cref="IDock.VisibleDockables"/> collection.
        /// </summary>
        /// <param name="first">The first dockable.</param>
        /// <param name="second">The second dockable.</param>
        void Move(IDockable first, IDockable second);

        /// <summary>
        /// Swaps dockable in owners <see cref="IDock.VisibleDockables"/> collection.
        /// </summary>
        /// <param name="first">The first dockable.</param>
        /// <param name="second">The second dockable.</param>
        void Swap(IDockable first, IDockable second);

        /// <summary>
        /// Swaps dockable in inside <see cref="IDock.VisibleDockables"/> collections.
        /// </summary>
        /// <param name="dock">The dock.</param>
        /// <param name="sourceDockable">The source dockable.</param>
        /// <param name="targetDockable">The target dockable.</param>
        void SwapDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable);

        /// <summary>
        /// Swaps dockable into between <see cref="IDock.VisibleDockables"/> collections.
        /// </summary>
        /// <param name="sourceDock">The source dock.</param>
        /// <param name="targetDock">The target dock.</param>
        /// <param name="sourceDockable">The source dockable.</param>
        /// <param name="targetDockable">The target dockable.</param>
        void SwapDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable targetDockable);

        /// <summary>
        /// Replaces source dockable with destination dockable in source dockable owner <see cref="IDock.VisibleDockables"/> collection.
        /// </summary>
        /// <param name="source">The source dockable.</param>
        /// <param name="destination">The destination dockable.</param>
        void Replace(IDockable source, IDockable destination);

        /// <summary>
        /// Creates a new split layout from source dockable.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        /// <param name="dockable">The optional dockable to add to splitted side.</param>
        /// <param name="operation">The dock operation.</param>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation);

        /// <summary>
        /// Splits dock and updates owner layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        /// <param name="dockable">The optional dockable to add to splitted side.</param>
        /// <param name="operation"> The dock operation to perform.</param>
        void Split(IDock dock, IDockable dockable, DockOperation operation);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Fill"/> and updates <see cref="IDockable.Owner"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToFill(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Left"/> and updates <see cref="IDockable.Owner"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToLeft(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Right"/> and updates <see cref="IDockable.Owner"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToRight(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Top"/> and updates <see cref="IDockable.Owner"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToTop(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Bottom"/> and updates <see cref="IDockable.Owner"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToBottom(IDock dock);

        /// <summary>
        /// Splits dock to the <see cref="DockOperation.Window"/> and updates <see cref="IDockable.Owner"/> layout.
        /// </summary>
        /// <param name="dock">The dock to perform operation on.</param>
        void SplitToWindow(IDock dock);

        /// <summary>
        /// Creates dock window from source dockable.
        /// </summary>
        /// <param name="dockable">The dockable to embed into window.</param>
        /// <returns>The new instance of the <see cref="IDockWindow"/> class.</returns>
        IDockWindow CreateWindowFrom(IDockable dockable);
    }
}
