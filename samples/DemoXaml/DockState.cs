using System.Collections.Generic;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvaloniaDemo.Xaml;

public class DockState
{
    private readonly Dictionary<string, object?> _toolContents;
    private readonly Dictionary<string, object?> _documentContents;
    private readonly Dictionary<string, DocumentTemplate?> _documentTemplates;

    public DockState()
    {
        _toolContents = new Dictionary<string, object?>();
        _documentContents = new Dictionary<string, object?>();
        _documentTemplates = new Dictionary<string, DocumentTemplate?>();
    }

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
            if (dockable is Tool tool)
            {
                var id = tool.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    _toolContents[id] = tool.Content;
                }
            }

            if (dockable is Document document)
            {
                var id = document.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    _documentContents[id] = document.Content;
                }
            }

            if (dockable is DocumentDock documentDock)
            {
                var id = documentDock.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    _documentTemplates[id] = documentDock.DocumentTemplate;
                }
            }

            if (dockable is IDock childDock)
            {
                Save(childDock);
            }
        }
    }

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
            if (dockable is Tool tool)
            {
                var id = tool.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    if (_toolContents.TryGetValue(id, out var content) && content is { })
                    {
                        tool.Content = content;
                    }
                }
            }

            if (dockable is Document document)
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
            }

            if (dockable is DocumentDock documentDock)
            {
                var id = documentDock.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    if (_documentTemplates.TryGetValue(id, out var content) && content is { })
                    {
                        documentDock.DocumentTemplate = content;
                    }
                }
            }

            if (dockable is IDock childDock)
            {
                Restore(childDock);
            }
        }
    }

    public void Reset()
    {
        _toolContents.Clear();
        _documentContents.Clear();
        _documentTemplates.Clear();
    }
}
