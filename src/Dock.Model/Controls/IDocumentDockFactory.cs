// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Document dock factory contract.
/// </summary>
public interface IDocumentDockFactory
{
    /// <summary>
    /// Gets or sets factory method used to create new documents.
    /// </summary>
    Func<IDockable>? DocumentFactory { get; set; }
}
