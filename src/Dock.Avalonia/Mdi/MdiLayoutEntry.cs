// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Dock.Model.Controls;

namespace Dock.Avalonia.Mdi;

/// <summary>
/// Represents a control and its MDI document for layout calculations.
/// </summary>
public readonly struct MdiLayoutEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MdiLayoutEntry"/> struct.
    /// </summary>
    /// <param name="control">The document control.</param>
    /// <param name="document">The document model.</param>
    public MdiLayoutEntry(Control control, IMdiDocument document)
    {
        Control = control ?? throw new ArgumentNullException(nameof(control));
        Document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <summary>
    /// Gets the document control.
    /// </summary>
    public Control Control { get; }

    /// <summary>
    /// Gets the document model.
    /// </summary>
    public IMdiDocument Document { get; }
}
