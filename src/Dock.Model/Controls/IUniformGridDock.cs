// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Uniform grid dock contract.
/// </summary>
[RequiresDataTemplate]
public interface IUniformGridDock : IDock
{
    /// <summary>
    /// Gets or sets number of rows.
    /// </summary>
    int Rows { get; set; }

    /// <summary>
    /// Gets or sets number of columns.
    /// </summary>
    int Columns { get; set; }
}
