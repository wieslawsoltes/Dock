using System;
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Dock factory contract.
/// </summary>
public partial interface IFactory
{
    /// <summary>
    /// Gets visible dockable controls.
    /// </summary>
    IDictionary<IDockable, IDockableControl> VisibleDockableControls { get; }

    /// <summary>
    /// Gets pinned dockable controls.
    /// </summary>
    IDictionary<IDockable, IDockableControl> PinnedDockableControls { get; }

    /// <summary>
    /// Gets tab dockable controls.
    /// </summary>
    IDictionary<IDockable, IDockableControl> TabDockableControls { get; }

    /// <summary>
    /// Gets dock controls.
    /// </summary>
    IList<IDockControl> DockControls { get; }

    /// <summary>
    /// Gets host windows.
    /// </summary>
    IList<IHostWindow> HostWindows { get; }

    /// <summary>
    /// Gets or sets <see cref="IDockable.Context"/> default locator.
    /// </summary>
    Func<object?>? DefaultContextLocator { get; set; }

    /// <summary>
    /// Gets or sets <see cref="IHostWindow"/> default locator.
    /// </summary>
    Func<IHostWindow?>? DefaultHostWindowLocator { get; set; }

    /// <summary>
    /// Gets or sets <see cref="IDockable.Context"/> locator registry.
    /// </summary>
    Dictionary<string, Func<object?>>? ContextLocator { get; set; }

    /// <summary>
    /// Gets or sets <see cref="IHostWindow"/> locator registry.
    /// </summary>
    Dictionary<string, Func<IHostWindow?>>? HostWindowLocator { get; set; }

    /// <summary>
    /// Gets or sets <see cref="IDockable"/> locator registry.
    /// </summary>
    IDictionary<string, Func<IDockable?>>? DockableLocator { get; set; }

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
    /// Creates <see cref="IProportionalDock"/>.
    /// </summary>
    /// <returns>The new instance of the <see cref="IProportionalDock"/> class.</returns>
    IProportionalDock CreateProportionalDock();

    /// <summary>
    /// Creates <see cref="IDockDock"/>.
    /// </summary>
    /// <returns>The new instance of the <see cref="IDockDock"/> class.</returns>
    IDockDock CreateDockDock();

    /// <summary>
    /// Creates <see cref="IProportionalDockSplitter"/>.
    /// </summary>
    /// <returns>The new instance of the <see cref="IProportionalDockSplitter"/> class.</returns>
    IProportionalDockSplitter CreateProportionalDockSplitter();

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
    /// <returns>The new instance of the <see cref="IRootDock"/> class.</returns>
    IRootDock? CreateLayout();

    /// <summary>
    /// Gets registered context in <see cref="ContextLocator"/>.
    /// </summary>
    /// <param name="id">The object id.</param>
    /// <returns>The located context.</returns>
    object? GetContext(string id);

    /// <summary>
    /// Gets registered host window.
    /// </summary>
    /// <param name="id">The host id.</param>
    /// <returns>The located host.</returns>
    IHostWindow? GetHostWindow(string id);

    /// <summary>
    /// Gets registered dockable in <see cref="DockableLocator"/>.
    /// </summary>
    /// <param name="id">The dockable id.</param>
    /// <typeparam name="T">The dockable return type.</typeparam>
    /// <returns>The located dockable.</returns>
    T? GetDockable<T>(string id) where T: class, IDockable;

    /// <summary>
    /// Initialize layout.
    /// </summary>
    /// <param name="layout">The layout to initialize.</param>
    void InitLayout(IDockable layout);

    /// <summary>
    /// Initialize dockable.
    /// </summary>
    /// <param name="dockable">The dockable to update.</param>
    /// <param name="owner">The owner dockable.</param>
    void InitDockable(IDockable dockable, IDockable? owner);

    /// <summary>
    /// Initialize dock window.
    /// </summary>
    /// <param name="window">The window to update.</param>
    /// <param name="owner">The window owner dockable.</param>
    void InitDockWindow(IDockWindow window, IDockable? owner);

    /// <summary>
    /// Initialize active dockable.
    /// </summary>
    /// <param name="dockable">The dockable to update.</param>
    /// <param name="owner">The owner dockable.</param>
    void InitActiveDockable(IDockable? dockable, IDock owner);

    /// <summary>
    /// Sets an active dockable. If the dockable is contained inside an dock it
    /// will become the selected dockable.
    /// </summary>
    /// <param name="dockable">The dockable to select.</param>
    void SetActiveDockable(IDockable dockable);

    /// <summary>
    /// Sets the currently focused dockable updating IsActive flags.
    /// </summary>
    /// <param name="dock">The dock to set the focused dockable on.</param>
    /// <param name="dockable">The dockable to set.</param>
    void SetFocusedDockable(IDock dock, IDockable? dockable);

    /// <summary>
    /// Searches for root dockable.
    /// </summary>
    /// <param name="dockable">The dockable to find root for.</param>
    /// <param name="predicate">The predicate to filter root docks.</param>
    /// <returns>The root dockable instance or null if root dockable was not found.</returns>
    IRootDock? FindRoot(IDockable dockable, Func<IRootDock, bool>? predicate = null);

    /// <summary>
    /// Searches dock for dockable.
    /// </summary>
    /// <param name="dock">The dock.</param>
    /// <param name="predicate">The predicate to filter dockables.</param>
    /// <returns>The dockable instance or null if dockable was not found.</returns>
    IDockable? FindDockable(IDock dock, Func<IDockable, bool> predicate);

    /// <summary>
    /// Searches for dockables in all registered <see cref="IDockControl"/>.
    /// </summary>
    /// <param name="predicate">The predicate to filter dockables.></param>
    /// <returns>The dockables collection.</returns>
    IEnumerable<IDockable> Find(Func<IDockable, bool> predicate);

    /// <summary>
    /// Searches dock for dockables in all registered <see cref="IDockControl"/>.
    /// </summary>
    /// <param name="dock"></param>
    /// <param name="predicate">The predicate to filter dockables.></param>
    /// <returns>The dockables collection.</returns>
    IEnumerable<IDockable> Find(IDock dock, Func<IDockable, bool> predicate);

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
    /// Removes dockable from owner <see cref="IDock.VisibleDockables"/> collection.
    /// </summary>
    /// <param name="dockable">The dockable to remove.</param>
    /// <param name="collapse">The flag indicating whether to collapse empty dock.</param>
    void RemoveDockable(IDockable dockable, bool collapse);

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
    void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable);

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
    /// Pins or unpins a dockable.
    /// </summary>
    /// <param name="dockable">The dockable to pin/unpin.</param>
    void PinDockable(IDockable dockable);

    /// <summary>
    /// Unpins a dockable.
    /// </summary>
    /// <param name="dockable">The dockable to unpin.</param>
    void UnpinDockable(IDockable dockable);

    /// <summary>
    /// Temporarily shows a pinned dockable.
    /// </summary>
    /// <param name="dockable">The dockable to show.</param>
    void PreviewPinnedDockable(IDockable dockable);

    /// <summary>
    /// Hides all temporarily shown pinned dockables.
    /// </summary>
    /// <param name="rootDock">The owner of the pinned dockables</param>
    void HidePreviewingDockables(IRootDock rootDock);

    /// <summary>
    /// Returns true if dockable is pinned.
    /// </summary>
    /// <param name="dockable">The dockable to check.</param>
    /// <param name="rootDock">The root dock. If null, the root will be automatically found.</param>
    bool IsDockablePinned(IDockable dockable, IRootDock? rootDock = null);

    /// <summary>
    /// Floats dockable.
    /// </summary>
    /// <param name="dockable">The dockable to float.</param>
    void FloatDockable(IDockable dockable);

    /// <summary>
    /// Removes dockable from owner <see cref="IDock.VisibleDockables"/> collection, and call IDockable.OnClose.
    /// </summary>
    /// <param name="dockable">The dockable to remove.</param>
    void CloseDockable(IDockable dockable);
        
    /// <summary>
    /// Calls <see cref="IFactory.CloseDockable"/> on all <see cref="IDock.VisibleDockables"/> of the dockable owner, excluding the dockable itself.
    /// </summary>
    /// <param name="dockable">The dockable owner source.</param>
    void CloseOtherDockables(IDockable dockable);

    /// <summary>
    /// Calls <see cref="IFactory.CloseDockable"/> on all <see cref="IDock.VisibleDockables"/> of the dockable owner.
    /// </summary>
    /// <param name="dockable">The dockable owner source.</param>
    void CloseAllDockables(IDockable dockable);

    /// <summary>
    /// Calls <see cref="IFactory.CloseDockable"/> on all tabs to the left of the dockable, from the <see cref="IDock.VisibleDockables"/> collection of the dockable owner.
    /// </summary>
    /// <param name="dockable">The dockable owner source.</param>
    void CloseLeftDockables(IDockable dockable);

    /// <summary>
    /// Calls <see cref="IFactory.CloseDockable"/> on all tabs to the right of the dockable, from the <see cref="IDock.VisibleDockables"/> collection of the dockable owner.
    /// </summary>
    /// <param name="dockable">The dockable owner source.</param>
    void CloseRightDockables(IDockable dockable);

    /// <summary>
    /// Adds window into dock windows list.
    /// </summary>
    /// <param name="rootDock">The root dock.</param>
    /// <param name="window">The window to add.</param>
    void AddWindow(IRootDock rootDock, IDockWindow window);

    /// <summary>
    /// Inserts window into dock windows list.
    /// </summary>
    /// <param name="rootDock">The root dock.</param>
    /// <param name="window">The window to add.</param>
    /// <param name="index">The window index.</param>
    void InsertWindow(IRootDock rootDock, IDockWindow window, int index);

    /// <summary>
    /// Removes window from owner windows list.
    /// </summary>
    /// <param name="window">The window to remove.</param>
    void RemoveWindow(IDockWindow window);

    /// <summary>
    /// Collapses dock.
    /// </summary>
    /// <param name="dock">The dock to collapse.</param>
    void CollapseDock(IDock dock);

    /// <summary>
    /// Creates a new split layout from source dockable.
    /// </summary>
    /// <param name="dock">The dock to perform operation on.</param>
    /// <param name="dockable">The optional dockable to add to a split side.</param>
    /// <param name="operation">The dock operation.</param>
    /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
    IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation);

    /// <summary>
    /// Splits dock and updates owner layout.
    /// </summary>
    /// <param name="dock">The dock to perform operation on.</param>
    /// <param name="dockable">The optional dockable to add to a split side.</param>
    /// <param name="operation"> The dock operation to perform.</param>
    void SplitToDock(IDock dock, IDockable dockable, DockOperation operation);

    /// <summary>
    /// Creates dock window from source dockable.
    /// </summary>
    /// <param name="dockable">The dockable to embed into window.</param>
    /// <returns>The new instance of the <see cref="IDockWindow"/> class.</returns>
    IDockWindow? CreateWindowFrom(IDockable dockable);

    /// <summary>
    /// Splits dock to the <see cref="DockOperation.Window"/> and updates <see cref="IDockable.Owner"/> layout.
    /// </summary>
    /// <param name="dock">The window owner.</param>
    /// <param name="dockable">The dockable to add to a split window.</param>
    /// <param name="x">The window X coordinate.</param>
    /// <param name="y">The window Y coordinate.</param>
    /// <param name="width">The window width.</param>
    /// <param name="height">The window height.</param>
    void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height);
}
