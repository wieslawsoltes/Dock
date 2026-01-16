// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Avalonia.Selectors;

/// <summary>
/// Represents an entry in the dock selector overlay.
/// </summary>
public sealed class DockSelectorItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockSelectorItem"/> class.
    /// </summary>
    /// <param name="dockable">The dockable represented by this item.</param>
    /// <param name="activationOrder">The activation order used for sorting.</param>
    /// <param name="isDocument">Whether the item is a document.</param>
    /// <param name="isTool">Whether the item is a tool.</param>
    /// <param name="isPinned">Whether the item is pinned.</param>
    /// <param name="isHidden">Whether the item is hidden.</param>
    /// <param name="isFloating">Whether the item is in a floating window.</param>
    public DockSelectorItem(
        IDockable dockable,
        long activationOrder,
        bool isDocument,
        bool isTool,
        bool isPinned,
        bool isHidden,
        bool isFloating)
    {
        Dockable = dockable;
        ActivationOrder = activationOrder;
        IsDocument = isDocument;
        IsTool = isTool;
        IsPinned = isPinned;
        IsHidden = isHidden;
        IsFloating = isFloating;
        var selectorTitle = dockable is IDockSelectorInfo selectorInfo ? selectorInfo.SelectorTitle : null;
        Title = string.IsNullOrWhiteSpace(selectorTitle)
            ? dockable.Title ?? string.Empty
            : selectorTitle ?? string.Empty;
    }

    /// <summary>
    /// Gets the dockable represented by the item.
    /// </summary>
    public IDockable Dockable { get; }

    /// <summary>
    /// Gets the title used in the selector list.
    /// </summary>
    public string Title { get; } = string.Empty;

    /// <summary>
    /// Gets the activation order used for sorting.
    /// </summary>
    public long ActivationOrder { get; }

    /// <summary>
    /// Gets a value indicating whether this item is a document.
    /// </summary>
    public bool IsDocument { get; }

    /// <summary>
    /// Gets a value indicating whether this item is a tool.
    /// </summary>
    public bool IsTool { get; }

    /// <summary>
    /// Gets a value indicating whether this item is pinned.
    /// </summary>
    public bool IsPinned { get; }

    /// <summary>
    /// Gets a value indicating whether this item is hidden.
    /// </summary>
    public bool IsHidden { get; }

    /// <summary>
    /// Gets a value indicating whether this item is in a floating window.
    /// </summary>
    public bool IsFloating { get; }
}
