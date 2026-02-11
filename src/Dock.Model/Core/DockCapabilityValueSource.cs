// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Describes which layer resolved an effective capability value.
/// </summary>
public enum DockCapabilityValueSource
{
    /// <summary>
    /// The value came from dockable base flags (<c>Can*</c> properties).
    /// </summary>
    Dockable,

    /// <summary>
    /// The value came from the root policy.
    /// </summary>
    RootPolicy,

    /// <summary>
    /// The value came from the dock policy.
    /// </summary>
    DockPolicy,

    /// <summary>
    /// The value came from dockable overrides.
    /// </summary>
    DockableOverride
}
