// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core;

/// <summary>
/// Dock control contract.
/// </summary>
public interface IDockControl
{
    /// <summary>
    /// Gets dock manager.
    /// </summary>
    IDockManager DockManager { get; }

    /// <summary>
    /// Gets dock control state.
    /// </summary>
    IDockControlState DockControlState { get; }

    /// <summary>
    /// Gets or sets the dock layout.
    /// </summary>
    IDock? Layout { get; set; }

    /// <summary>
    /// Gets or sets default context.
    /// </summary>
    object? DefaultContext { get; set; }

    /// <summary>
    /// Gets or sets the flag indicating whether to initialize layout.
    /// </summary>
    bool InitializeLayout { get; set; }

    /// <summary>
    /// Gets or sets the flag indicating whether to initialize factory.
    /// </summary>
    bool InitializeFactory { get; set; }

    /// <summary>
    /// Gets or sets the factory.
    /// </summary>
    IFactory? Factory { get; set; }

    /// <summary>
    /// Gets or sets group identifier used to link dock controls.
    /// Dock controls sharing the same group allow cross docking.
    /// </summary>
    string? DockGroup { get; set; }
}
