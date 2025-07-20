// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <inheritdoc/>
    public virtual void AddWindow(IRootDock rootDock, IDockWindow window)
    {
        rootDock.Windows ??= CreateList<IDockWindow>();
        rootDock.Windows.Add(window);
        OnWindowAdded(window);
        InitDockWindow(window, rootDock);
    }

    /// <inheritdoc/>
    public virtual void InsertWindow(IRootDock rootDock, IDockWindow window, int index)
    {
        if (index >= 0)
        {
            rootDock.Windows ??= CreateList<IDockWindow>();
            rootDock.Windows.Insert(index, window);
            OnWindowAdded(window);
            InitDockWindow(window, rootDock);
        }
    }

    /// <inheritdoc/>
    public virtual void RemoveWindow(IDockWindow window)
    {
        if (window.Owner is IRootDock rootDock)
        {
            window.Exit();
            if (window.Layout is { })
            {
                UnsubscribeDockable(window.Layout);
            }
            rootDock.Windows?.Remove(window);
            OnWindowRemoved(window);
        }
    }
}
