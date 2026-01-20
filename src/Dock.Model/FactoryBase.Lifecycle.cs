// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Dock.Model.Core;
using Dock.Model.Services;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <summary>
    /// Gets the window lifecycle services invoked on window events.
    /// </summary>
    public IList<IWindowLifecycleService> WindowLifecycleServices { get; } = new List<IWindowLifecycleService>();

    /// <summary>
    /// Invokes lifecycle services when a window is closed.
    /// </summary>
    /// <param name="window">The closed window.</param>
    protected virtual void NotifyWindowClosed(IDockWindow? window)
    {
        if (WindowLifecycleServices.Count == 0)
        {
            return;
        }

        foreach (var service in WindowLifecycleServices)
        {
            service?.OnWindowClosed(window);
        }
    }

    /// <summary>
    /// Invokes lifecycle services when a window is removed.
    /// </summary>
    /// <param name="window">The removed window.</param>
    protected virtual void NotifyWindowRemoved(IDockWindow? window)
    {
        if (WindowLifecycleServices.Count == 0)
        {
            return;
        }

        foreach (var service in WindowLifecycleServices)
        {
            service?.OnWindowRemoved(window);
        }
    }
}
