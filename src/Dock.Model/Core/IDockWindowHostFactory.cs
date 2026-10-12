// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>Optionally creates native window hosts with access to their layout model.</summary>
/// <remarks>
/// A factory implementing this interface is called when a window is initialized
/// and when an existing window is presented again without a host. Factories that
/// do not implement it continue to use <see cref="IFactory.GetHostWindow"/>.
/// </remarks>
public interface IDockWindowHostFactory
{
    /// <summary>Creates a host for the supplied window, or returns null to leave it unhosted.</summary>
    /// <param name="window">The native window whose layout will be presented.</param>
    /// <returns>The host to associate with the window.</returns>
    IHostWindow? CreateHostWindow(IDockWindow window);
}
