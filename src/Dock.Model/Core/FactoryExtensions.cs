// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Linq;
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Helper methods for <see cref="IFactory"/>.
/// </summary>
internal static class FactoryExtensions
{
    /// <summary>
    /// Finds the root dock that currently has focus.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <returns>The active <see cref="IRootDock"/> if found, otherwise <c>null</c>.</returns>
    public static IRootDock? GetActiveRoot(this IFactory factory)
    {
        return factory
            .Find(d => d is IRootDock root && root.IsActive)
            .OfType<IRootDock>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the document that is currently focused across all docks and windows.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <returns>The active <see cref="IDocument"/> or <c>null</c> if no document is focused.</returns>
    public static IDocument? GetCurrentDocument(this IFactory factory)
    {
        return factory.GetActiveRoot()?.FocusedDockable as IDocument;
    }
}
