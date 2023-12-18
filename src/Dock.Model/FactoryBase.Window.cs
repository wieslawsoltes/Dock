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
            rootDock.Windows?.Remove(window);
            OnWindowRemoved(window);
        }
    }
}
