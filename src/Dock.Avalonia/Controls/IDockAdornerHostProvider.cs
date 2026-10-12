// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;
using Avalonia.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>Selects an optional enclosing control for a local docking preview.</summary>
public interface IDockAdornerHostProvider
{
    /// <summary>Resolves the control over which the local adorner is arranged.</summary>
    /// <param name="surface">The hovered docking surface.</param>
    /// <returns>The surface control or a control among its visual ancestors, or null to use the surface.</returns>
    /// <remarks>This does not change the docking destination. An expanded target theme uses
    /// <see cref="DockTarget.TargetMargin"/> to keep its selectors over the original surface.
    /// Resolution must not mutate the visual or model hierarchy.</remarks>
    Control? GetAdornerHost(Visual surface);
}
