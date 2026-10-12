// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>Optionally reports whether a host has completed closing.</summary>
/// <remarks>Hosts that can cancel or defer <see cref="IHostWindow.Exit"/> implement
/// this contract so their model retains the live host until closing completes.
/// Hosts without this contract retain the synchronous, unconditional exit behavior.</remarks>
public interface IHostWindowCloseState
{
    /// <summary>Gets whether the host has closed. Hidden and canceled hosts return false.</summary>
    bool IsClosed { get; }
}
