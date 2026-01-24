// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model;

/// <summary>
/// Represents a named layout workspace snapshot.
/// </summary>
public sealed class DockWorkspace
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockWorkspace"/> class.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <param name="layout">Serialized layout string.</param>
    /// <param name="state">Optional state snapshot.</param>
    public DockWorkspace(string id, string layout, DockState? state)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Workspace id cannot be null or whitespace.", nameof(id));
        }

        Id = id;
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        State = state;
    }

    /// <summary>
    /// Gets the workspace identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets the workspace display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the workspace description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the serialized layout payload.
    /// </summary>
    public string Layout { get; private set; }

    /// <summary>
    /// Gets whether the workspace has unsaved layout changes.
    /// </summary>
    public bool IsDirty { get; private set; }

    /// <summary>
    /// Gets the optional dock state snapshot used for in-memory restores.
    /// </summary>
    public DockState? State { get; private set; }

    internal void Update(string layout, DockState? state, string? name, string? description)
    {
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        State = state;
        IsDirty = false;

        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            Description = description;
        }
    }

    internal bool SetDirty(bool value)
    {
        if (IsDirty == value)
        {
            return false;
        }

        IsDirty = value;
        return true;
    }
}
