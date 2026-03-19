// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;

namespace Avalonia.VisualTree;

/// <summary>
/// Provides a compatibility shim for root lookup APIs removed in Avalonia 12.
/// </summary>
public static class AvaloniaVisualTreeExtensions
{
    /// <summary>
    /// Gets the top-level visual hosting the specified visual.
    /// </summary>
    /// <param name="visual">The visual to inspect.</param>
    /// <returns>
    /// The host <see cref="TopLevel"/>, or <see langword="null"/> when the visual is not attached.
    /// </returns>
    public static TopLevel? GetVisualRoot(this Visual visual)
    {
        return TopLevel.GetTopLevel(visual);
    }
}
