using System.Collections.Generic;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Core;

/// <summary>
/// 
/// </summary>
public class DockState : IDockState
{
    private readonly Dictionary<string, object?> _toolContents;
    private readonly Dictionary<string, object?> _documentContents;
    private readonly Dictionary<string, DocumentTemplate?> _documentTemplates;

    /// <summary>
    /// 
    /// </summary>
    public DockState()
    {
        _toolContents = new Dictionary<string, object?>();
        _documentContents = new Dictionary<string, object?>();
        _documentTemplates = new Dictionary<string, DocumentTemplate?>();
    }

    /// <inheritdoc/>
    public void Save(IDock dock)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        if (dock is IRootDock rootDock)
        {
            if (rootDock.Windows is { })
            {
                foreach (var window in rootDock.Windows)
                {
                    if (window.Layout is { })
                    {
                        Save(window.Layout);
                    }
                }
            }
        }

        foreach (var dockable in dock.VisibleDockables)
        {
            SaveDockable(dockable);

            if (dockable is IDock childDock)
            {
                Save(childDock);
            }
        }
    }

    private void SaveDockable(IDockable dockable)
    {
        switch (dockable)
        {
            case IToolContent tool:
            {
                var id = tool.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    _toolContents[id] = tool.Content;
                }

                break;
            }
            case IDocumentContent document:
            {
                var id = document.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    _documentContents[id] = document.Content;
                }

                break;
            }
            // TODO: Add IIDocumentDockContent interface?
            case DocumentDock documentDock:
            {
                var id = documentDock.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    _documentTemplates[id] = documentDock.DocumentTemplate;
                }

                break;
            }
        }
    }

    /// <inheritdoc/>
    public void Restore(IDock dock)
    {
        if (dock.VisibleDockables is null)
        {
            return;
        }

        if (dock is IRootDock rootDock)
        {
            if (rootDock.Windows is { })
            {
                foreach (var window in rootDock.Windows)
                {
                    if (window.Layout is { })
                    {
                        Restore(window.Layout);
                    }
                }
            }
        }

        foreach (var dockable in dock.VisibleDockables)
        {
            RestoreDockable(dockable);

            if (dockable is IDock childDock)
            {
                Restore(childDock);
            }
        }
    }

    private void RestoreDockable(IDockable dockable)
    {
        switch (dockable)
        {
            case IToolContent tool:
            {
                var id = tool.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    if (_toolContents.TryGetValue(id, out var content) && content is { })
                    {
                        tool.Content = content;
                    }
                }

                break;
            }
            case IDocumentContent document:
            {
                var id = document.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    if (_documentContents.TryGetValue(id, out var content) && content is { })
                    {
                        document.Content = content;
                    }
                }
                else
                {
                    // TODO: Use DocumentTemplate to recreate documents without Id.
                }

                break;
            }
            // TODO: Add IIDocumentDockContent interface?
            case DocumentDock documentDock:
            {
                var id = documentDock.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    if (_documentTemplates.TryGetValue(id, out var content) && content is { })
                    {
                        documentDock.DocumentTemplate = content;
                    }
                }

                break;
            }
        }
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _toolContents.Clear();
        _documentContents.Clear();
        _documentTemplates.Clear();
    }
}
