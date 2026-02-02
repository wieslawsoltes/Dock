// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Settings;

/// <summary>
/// Defines the global owner policy for floating windows.
/// </summary>
public enum DockFloatingWindowOwnerPolicy
{
    /// <summary>
    /// Use <see cref="DockSettings.UseOwnerForFloatingWindows"/> to decide ownership.
    /// </summary>
    Default,

    /// <summary>
    /// Always assign an owner for floating windows when possible.
    /// </summary>
    AlwaysOwned,

    /// <summary>
    /// Never assign an owner for floating windows.
    /// </summary>
    NeverOwned
}
