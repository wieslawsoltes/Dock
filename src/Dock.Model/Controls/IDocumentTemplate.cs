// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Controls;

/// <summary>
/// Document template contract.
/// </summary>
public interface IDocumentTemplate
{
    /// <summary>
    /// Gets or sets document content.
    /// </summary>
    object? Content { get; set; }
}
