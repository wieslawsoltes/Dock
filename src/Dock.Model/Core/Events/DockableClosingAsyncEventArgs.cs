// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Threading.Tasks;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable closing async event args.
/// </summary>
public class DockableClosingAsyncEventArgs : EventArgs
{
    /// <summary>
    /// Gets closing dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets or sets flag indicating whether dockable closing should be canceled.
    /// </summary>
    public bool Cancel { get; set; }

    private Func<Task<bool>>? _asyncCancelHandler;

    /// <summary>
    /// Initializes new instance of the <see cref="DockableClosingAsyncEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The closing dockable.</param>
    public DockableClosingAsyncEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }

    /// <summary>
    /// Sets an async handler that will determine whether closing should be canceled.
    /// The handler should return true to cancel the close operation, false to allow it.
    /// </summary>
    /// <param name="handler">The async handler function.</param>
    public void SetAsyncCancelHandler(Func<Task<bool>> handler)
    {
        _asyncCancelHandler = handler;
    }

    /// <summary>
    /// Gets whether an async cancel handler has been set.
    /// </summary>
    public bool HasAsyncCancelHandler => _asyncCancelHandler != null;

    /// <summary>
    /// Executes the async cancel handler if one has been set.
    /// </summary>
    /// <returns>A task that represents the async operation. The task result contains true if closing should be canceled, false otherwise.</returns>
    public async Task<bool> ExecuteAsyncCancelHandlerAsync()
    {
        if (_asyncCancelHandler != null)
        {
            return await _asyncCancelHandler();
        }
        return false;
    }
}
