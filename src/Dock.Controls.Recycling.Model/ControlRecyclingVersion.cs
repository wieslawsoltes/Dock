// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Avalonia.Controls.Recycling.Model;

/// <summary>
/// Identifies a revision of the content used to create a recycled control.
/// </summary>
public readonly struct ControlRecyclingVersion
{
    /// <summary>
    /// Creates a content revision. The source is an opaque identity token which
    /// must remain stable across revisions and must not reference the content or its owner.
    /// </summary>
    /// <param name="source">The identity of the source of content revisions.</param>
    /// <param name="revision">The current content revision.</param>
    public ControlRecyclingVersion(object source, long revision)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Revision = revision;
    }

    /// <summary>
    /// Gets the opaque identity of the source of content revisions.
    /// </summary>
    public object Source { get; }

    /// <summary>
    /// Gets the content revision within the source.
    /// </summary>
    public long Revision { get; }
}
