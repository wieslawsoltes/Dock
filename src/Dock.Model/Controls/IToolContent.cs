// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Tool content contract.
/// </summary>
public interface IToolContent : IDockable
{
    /// <summary>
    /// Gets or sets tool content.
    /// </summary>
    object? Content { get; set; }
}
