// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Avalonia.Selectors;

/// <summary>
/// Provides control over the dock selector overlay.
/// </summary>
public interface IDockSelectorService
{
    /// <summary>
    /// Shows the selector overlay for the specified mode.
    /// </summary>
    /// <param name="mode">The selector mode.</param>
    void ShowSelector(DockSelectorMode mode);

    /// <summary>
    /// Hides the selector overlay.
    /// </summary>
    void HideSelector();

    /// <summary>
    /// Gets a value indicating whether the selector is currently open.
    /// </summary>
    bool IsOpen { get; }
}
