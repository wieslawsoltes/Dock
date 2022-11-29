using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <inheritdoc/>
    public virtual void InitLayout(IDockable layout)
    {
        InitDockable(layout, null);

        if (layout is IDock dock)
        {
            if (dock.DefaultDockable is not null)
            {
                dock.ActiveDockable = dock.DefaultDockable;
            }
        }

        if (layout is IRootDock rootDock)
        {
            if (rootDock.ShowWindows.CanExecute(null))
            {
                rootDock.ShowWindows.Execute(null);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void InitDockable(IDockable dockable, IDockable? owner)
    {
        if (dockable.Context is null)
        {
            if (GetContext(dockable.Id) is { } context)
            {
                dockable.Context = context;
            }
        }

        dockable.Owner = owner;

        if (dockable is IDock dock)
        {
            dock.Factory = this;

            if (dock.VisibleDockables is not null)
            {
                foreach (var child in dock.VisibleDockables)
                {
                    InitDockable(child, dockable);
                }
            }
        }

        if (dockable is IRootDock rootDock)
        {
            if (rootDock.Windows is not null)
            {
                foreach (var child in rootDock.Windows)
                {
                    InitDockWindow(child, dockable);
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual void InitDockWindow(IDockWindow window, IDockable? owner)
    {
        window.Host = GetHostWindow(window.Id);
        if (window.Host is not null)
        {
            window.Host.Window = window;
        }

        window.Owner = owner;
        window.Factory = this;

        if (window.Layout is not null)
        {
            InitDockable(window.Layout, window.Layout.Owner);
        }
    }

    /// <inheritdoc/>
    public virtual void InitActiveDockable(IDockable? dockable, IDock owner)
    {
        OnActiveDockableChanged(dockable);

        if (dockable is { })
        {
            InitDockable(dockable, owner);
            dockable.OnSelected();
        }

        if (dockable is { })
        {
            SetFocusedDockable(owner, dockable);
        } 
    }
}
