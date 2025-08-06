// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Grid dock contract.
/// </summary>
[RequiresDataTemplate]
public interface IGridDock : IDock
{
    /// <summary>
    /// Gets or sets column definitions string.
    /// </summary>
    string? ColumnDefinitions { get; set; }

    /// <summary>
    /// Gets or sets row definitions string.
    /// </summary>
    string? RowDefinitions { get; set; }
}
