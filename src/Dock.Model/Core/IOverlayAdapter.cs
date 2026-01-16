// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Overlay adapter contract for overlay dock bookkeeping.
/// </summary>
public interface IOverlayAdapter
{
    /// <summary>
    /// Gets the optional background dockable from the visible list.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <returns>Background dockable or null.</returns>
    IDockable? GetBackground(IOverlayDock overlayDock);

    /// <summary>
    /// Gets overlay panels from the dock by skipping the optional background item.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <returns>Panels list or null if none.</returns>
    IList<IOverlayPanel>? GetOverlayPanels(IOverlayDock overlayDock);

    /// <summary>
    /// Sets the background dockable and keeps existing panels.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <param name="background">The background dockable.</param>
    /// <param name="listFactory">Factory used to create the visible list.</param>
    void SetBackground(IOverlayDock overlayDock, IDockable? background, Func<IList<IDockable>> listFactory);

    /// <summary>
    /// Sets overlay panels and keeps the existing background item.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <param name="panels">The panels to set.</param>
    /// <param name="listFactory">Factory used to create the visible list.</param>
    void SetPanels(IOverlayDock overlayDock, IList<IOverlayPanel>? panels, Func<IList<IDockable>> listFactory);

    /// <summary>
    /// Assigns the overlay dock as the owner of each splitter group.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <param name="splitterGroups">The splitter groups to update.</param>
    void SetSplitterGroupsOwner(IOverlayDock overlayDock, IList<IOverlaySplitterGroup>? splitterGroups);

    /// <summary>
    /// Sets overlay panel content and keeps ownership/visible list consistent.
    /// </summary>
    /// <param name="panel">The overlay panel instance.</param>
    /// <param name="content">The dockable content to set.</param>
    /// <param name="listFactory">Factory used to create the visible list.</param>
    void SetOverlayPanelContent(IOverlayPanel panel, IDockable? content, Func<IList<IDockable>> listFactory);

    /// <summary>
    /// Updates panel back-references when the panel collection changes.
    /// </summary>
    /// <param name="splitterGroup">The owning splitter group.</param>
    /// <param name="previous">Panels previously attached to the group.</param>
    /// <param name="next">Panels being assigned to the group.</param>
    void UpdateGroupPanels(IOverlaySplitterGroup splitterGroup, IList<IOverlayPanel>? previous, IList<IOverlayPanel>? next);

    /// <summary>
    /// Assigns the splitter group owner to each splitter.
    /// </summary>
    /// <param name="splitterGroup">The owning splitter group.</param>
    /// <param name="splitters">Splitters to update.</param>
    void UpdateGroupSplitters(IOverlaySplitterGroup splitterGroup, IList<IOverlaySplitter>? splitters);
}
