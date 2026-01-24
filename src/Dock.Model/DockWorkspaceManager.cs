// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Manages named workspace snapshots for docking layouts.
/// </summary>
public sealed class DockWorkspaceManager
{
    private readonly IDockSerializer _serializer;
    private readonly Dictionary<string, DockWorkspace> _workspaces;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockWorkspaceManager"/> class.
    /// </summary>
    /// <param name="serializer">The dock serializer.</param>
    public DockWorkspaceManager(IDockSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _workspaces = new Dictionary<string, DockWorkspace>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the known workspaces.
    /// </summary>
    public IReadOnlyCollection<DockWorkspace> Workspaces => _workspaces.Values;

    /// <summary>
    /// Gets the most recently captured or restored workspace.
    /// </summary>
    public DockWorkspace? ActiveWorkspace { get; private set; }

    /// <summary>
    /// Captures a workspace snapshot.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <param name="layout">Layout to serialize.</param>
    /// <param name="includeState">Whether to capture dock state.</param>
    /// <param name="name">Optional workspace name.</param>
    /// <param name="description">Optional workspace description.</param>
    /// <returns>The captured workspace.</returns>
    public DockWorkspace Capture(string id, IDock layout, bool includeState = true, string? name = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Workspace id cannot be null or whitespace.", nameof(id));
        }

        if (layout is null)
        {
            throw new ArgumentNullException(nameof(layout));
        }

        DockState? state = null;
        if (includeState)
        {
            state = new DockState();
            state.Save(layout);
        }

        var payload = _serializer.Serialize(layout);

        if (_workspaces.TryGetValue(id, out var existing))
        {
            existing.Update(payload, state, name, description);
            ActiveWorkspace = existing;
            return existing;
        }

        var workspace = new DockWorkspace(id, payload, state)
        {
            Name = string.IsNullOrWhiteSpace(name) ? id : name,
            Description = description
        };

        _workspaces[id] = workspace;
        ActiveWorkspace = workspace;
        return workspace;
    }

    /// <summary>
    /// Gets a workspace by id.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <returns>The workspace or null.</returns>
    public DockWorkspace? Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        _workspaces.TryGetValue(id, out var workspace);
        return workspace;
    }

    /// <summary>
    /// Removes a workspace by id.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <returns>True when removed.</returns>
    public bool Remove(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        var removed = _workspaces.Remove(id);
        if (removed && ActiveWorkspace?.Id == id)
        {
            ActiveWorkspace = null;
        }

        return removed;
    }

    /// <summary>
    /// Clears all workspaces.
    /// </summary>
    public void Clear()
    {
        _workspaces.Clear();
        ActiveWorkspace = null;
    }

    /// <summary>
    /// Restores a layout from a workspace.
    /// </summary>
    /// <param name="workspace">Workspace to restore.</param>
    /// <returns>The restored layout, or null if deserialization fails.</returns>
    public IDock? Restore(DockWorkspace workspace)
    {
        if (workspace is null)
        {
            throw new ArgumentNullException(nameof(workspace));
        }

        var layout = _serializer.Deserialize<IDock>(workspace.Layout);
        if (layout is null)
        {
            return null;
        }

        workspace.State?.Restore(layout);
        ActiveWorkspace = workspace;
        return layout;
    }

    /// <summary>
    /// Restores a layout by workspace id.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <param name="layout">The restored layout, when successful.</param>
    /// <returns>True when the restore succeeded.</returns>
    public bool TryRestore(string id, out IDock? layout)
    {
        layout = null;

        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        if (!_workspaces.TryGetValue(id, out var workspace))
        {
            return false;
        }

        layout = Restore(workspace);
        return layout is not null;
    }
}
