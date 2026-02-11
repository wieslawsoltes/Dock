// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Represents a logical dock capability that can be controlled by policy.
/// </summary>
public enum DockCapability
{
    /// <summary>
    /// Controls close operations.
    /// </summary>
    Close,

    /// <summary>
    /// Controls pin and unpin operations.
    /// </summary>
    Pin,

    /// <summary>
    /// Controls floating operations.
    /// </summary>
    Float,

    /// <summary>
    /// Controls drag source behavior.
    /// </summary>
    Drag,

    /// <summary>
    /// Controls drop target behavior.
    /// </summary>
    Drop,

    /// <summary>
    /// Controls docking a window as a document.
    /// </summary>
    DockAsDocument
}
