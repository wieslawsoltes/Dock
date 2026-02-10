// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Controls;

/// <summary>
/// Tool template contract.
/// </summary>
public interface IToolTemplate
{
    /// <summary>
    /// Gets or sets tool content.
    /// </summary>
    object? Content { get; set; }
}
