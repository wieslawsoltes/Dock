// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>Optional factory policy selecting the pane used when splitting tool content.</summary>
/// <remarks>The native dock service retains ownership of pane creation and content movement.
/// Factories without this policy create tool panes for tool splits.</remarks>
public interface IDockSplitPolicy
{
    /// <summary>Determines whether a tool split should create a document pane.</summary>
    /// <param name="source">The tool being moved into the new pane.</param>
    /// <param name="target">The pane being split.</param>
    /// <param name="operation">The requested split direction.</param>
    /// <returns>True to create a document pane; false to create a tool pane.</returns>
    bool UseDocumentDock(IDockable source, IDock target, DockOperation operation);
}
