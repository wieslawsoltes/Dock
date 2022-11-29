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
    public virtual void RemoveWindow(IDockWindow window)
    {
        if (window.Owner is IRootDock rootDock)
        {
            window.Exit();
            rootDock.Windows?.Remove(window);
            OnWindowRemoved(window);
        }
    }

    /// <inheritdoc/>
    public virtual IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        IDockable? target;
        bool topmost;

        switch (dockable)
        {
            case ITool:
            {
                target = CreateToolDock();
                target.Id = nameof(IToolDock);
                target.Title = nameof(IToolDock);
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dock.VisibleDockables is not null)
                    {
                        dock.VisibleDockables.Add(dockable);
                        OnDockableAdded(dockable);
                        dock.ActiveDockable = dockable;
                    }
                }
                topmost = true;
                break;
            }
            case IDocument:
            {
                target = CreateDocumentDock();
                target.Id = nameof(IDocumentDock);
                target.Title = nameof(IDocumentDock);
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dockable.Owner is IDocumentDock sourceDocumentDock)
                    {
                        if (target is IDocumentDock documentDock)
                        {
                            documentDock.CanCreateDocument = sourceDocumentDock.CanCreateDocument;

                            if (sourceDocumentDock is IDocumentDockContent sourceDocumentDockContent
                                && documentDock is IDocumentDockContent documentDockContent)
                            {
                                documentDockContent.DocumentTemplate = sourceDocumentDockContent.DocumentTemplate;
                            }
                        }
                    }
                    if (dock.VisibleDockables is not null)
                    {
                        dock.VisibleDockables.Add(dockable);
                        OnDockableAdded(dockable);
                        dock.ActiveDockable = dockable;
                    }
                }
                topmost = false;
                break;
            }
            case IToolDock:
            {
                target = dockable;
                topmost = true;
                break;
            }
            case IDocumentDock:
            {
                target = dockable;
                topmost = false;
                break;
            }
            case IProportionalDock proportionalDock:
            {
                target = proportionalDock;
                topmost = false;
                break;
            }
            case IDockDock dockDock:
            {
                target = dockDock;
                topmost = false;
                break;
            }
            case IRootDock rootDock:
            {
                target = rootDock.ActiveDockable;
                topmost = false;
                break;
            }
            default:
            {
                return null;
            }
        }

        var root = CreateRootDock();
        root.Id = nameof(IRootDock);
        root.Title = nameof(IRootDock);
        root.VisibleDockables = CreateList<IDockable>();
        if (root.VisibleDockables is not null && target is not null)
        {
            root.VisibleDockables.Add(target);
            OnDockableAdded(target);
            root.ActiveDockable = target;
            root.DefaultDockable = target;
        }
        root.Owner = null;

        var window = CreateDockWindow();
        window.Id = nameof(IDockWindow);
        window.Title = "";
        window.Width = double.NaN;
        window.Height = double.NaN;
        window.Topmost = topmost;
        window.Layout = root;

        root.Window = window;

        return window;
    }
}
