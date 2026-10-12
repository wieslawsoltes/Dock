// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>Supplies docking selectors and their visual state from an external template.</summary>
/// <remarks>Dock retains coordinate translation, direction limits, drop validation, and the
/// resulting operation. A presentation only supplies controls and renders the reported state.</remarks>
public interface IDockTargetPresentation
{
    /// <summary>Gets the selector for an operation, or null when the presentation omits it.</summary>
    /// <param name="operation">The native docking operation.</param>
    /// <returns>A selector in an attached visual tree, which may be in another window.</returns>
    Control? GetSelector(DockOperation operation);

    /// <summary>Updates the selector's visibility and the operation's preview state.</summary>
    /// <param name="operation">The native docking operation.</param>
    /// <param name="visible">Whether policy allows displaying the selector.</param>
    /// <param name="active">Whether the current input activates this operation's preview.</param>
    /// <remarks>Indicator-only mode can activate a preview while its selector is hidden.
    /// Reset and presentation replacement report both flags as false. Calls occur on the UI thread.</remarks>
    void SetOperationState(DockOperation operation, bool visible, bool active);
}
