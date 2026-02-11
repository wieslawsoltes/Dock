// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Docking manager contract.
/// </summary>
public interface IDockManager
{
    /// <summary>
    /// Gets or sets pointer position relative to event source.
    /// </summary>
    DockPoint Position { get; set; }

    /// <summary>
    /// Gets or sets pointer position relative to event source and translated to screen coordinates.
    /// </summary>
    DockPoint ScreenPosition { get; set; }

    /// <summary>
    /// Gets or sets flag that prevents docking when fixed sizes conflict.
    /// </summary>
    bool PreventSizeConflicts { get; set; }

    /// <summary>
    /// Gets or sets whether docking interactions are enabled.
    /// </summary>
    bool IsDockingEnabled { get; set; }

    /// <summary>
    /// Gets the last capability evaluation that blocked a docking-related action.
    /// Returns null when the last validation did not fail due to capability policy.
    /// </summary>
    DockCapabilityEvaluation? LastCapabilityEvaluation { get; }

    /// <summary>
    /// Validates tool docking operation.
    /// </summary>
    /// <param name="sourceTool">The source tool.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">The flag indicating whether to execute.</param>
    /// <returns>True if docking operation can be executed otherwise false.</returns>
    bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

    /// <summary>
    /// Validates document docking operation.
    /// </summary>
    /// <param name="sourceDocument">The source document.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">The flag indicating whether to execute.</param>
    /// <returns>True if docking operation can be executed otherwise false.</returns>
    bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

    /// <summary>
    /// Validates dock docking operation.
    /// </summary>
    /// <param name="sourceDock">The source dock.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">The flag indicating whether to execute.</param>
    /// <returns>True if docking operation can be executed otherwise false.</returns>
    bool ValidateDock(IDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

    /// <summary>
    /// Validates dockable docking operation.
    /// </summary>
    /// <param name="sourceDockable">The source dockable.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">The flag indicating whether to execute.</param>
    /// <returns>True if docking operation can be executed otherwise false.</returns>
    bool ValidateDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

    /// <summary>
    /// Determines if a dock target indicator should be visible.
    /// </summary>
    /// <param name="sourceDockable">The source dockable being dragged.</param>
    /// <param name="targetDockable">The dockable under the pointer.</param>
    /// <param name="operation">The dock operation represented by the indicator.</param>
    /// <returns>True to show the indicator, false to hide it.</returns>
    bool IsDockTargetVisible(IDockable sourceDockable, IDockable targetDockable, DockOperation operation);
}
