// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>Optionally separates closing a host from closing its layout.</summary>
/// <remarks>A factory implements this policy when another component owns content
/// closure. Window cancellation and closed notifications still run. Factories
/// without this policy retain Dock's content-closing behavior.</remarks>
public interface IDockWindowClosePolicy
{
    /// <summary>Returns whether closing this host preserves its window model and content.</summary>
    /// <param name="window">The model whose host is closing.</param>
    /// <returns>True to release only the host; false to run normal layout cleanup.</returns>
    bool PreserveLayoutOnHostClose(IDockWindow window);
}
