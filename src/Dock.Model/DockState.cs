// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// 
/// </summary>
public class DockState : IDockState
{
    private readonly Dictionary<string, object?> _toolContents;
    private readonly Dictionary<string, object?> _documentContents;
    private readonly Dictionary<string, IDocumentTemplate?> _documentTemplates;

    /// <summary>
    /// 
    /// </summary>
    public DockState()
    {
        _toolContents = new Dictionary<string, object?>();
        _documentContents = new Dictionary<string, object?>();
        _documentTemplates = new Dictionary<string, IDocumentTemplate?>();
    }

    /// <inheritdoc/>
    public void Save(IDock dock)
    {
        if (dock is IRootDock rootDock)
        {
            if (rootDock.HiddenDockables is { })
            {
                SaveDockables(rootDock.HiddenDockables);
            }

            if (rootDock.LeftPinnedDockables is { })
            {
                SaveDockables(rootDock.LeftPinnedDockables);
            }

            if (rootDock.RightPinnedDockables is { })
            {
                SaveDockables(rootDock.RightPinnedDockables);
            }

            if (rootDock.TopPinnedDockables is { })
            {
                SaveDockables(rootDock.TopPinnedDockables);
            }

            if (rootDock.BottomPinnedDockables is { })
            {
                SaveDockables(rootDock.BottomPinnedDockables);
            }

            if (rootDock.Windows is { })
            {
                SaveWindows(rootDock.Windows);
            }
        }

        if (dock.VisibleDockables is { })
        {
            SaveDockables(dock.VisibleDockables);
        }
    }

    /// <inheritdoc/>
    public void Restore(IDock dock)
    {
        if (dock is IRootDock rootDock)
        {
            if (rootDock.HiddenDockables is { })
            {
                RestoreDockables(rootDock.HiddenDockables);
            }

            if (rootDock.LeftPinnedDockables is { })
            {
                RestoreDockables(rootDock.LeftPinnedDockables);
            }

            if (rootDock.RightPinnedDockables is { })
            {
                RestoreDockables(rootDock.RightPinnedDockables);
            }

            if (rootDock.TopPinnedDockables is { })
            {
                RestoreDockables(rootDock.TopPinnedDockables);
            }

            if (rootDock.BottomPinnedDockables is { })
            {
                RestoreDockables(rootDock.BottomPinnedDockables);
            }

            if (rootDock.Windows is { })
            {
                RestoreWindows(rootDock.Windows);
            }
        }

        if (dock.VisibleDockables is { })
        {
            RestoreDockables(dock.VisibleDockables);
        }
    }

    private void SaveWindows(IList<IDockWindow> windows)
    {
        foreach (var window in windows)
        {
            if (window.Layout is { })
            {
                Save(window.Layout);
            }
        }
    }

    private void RestoreWindows(IList<IDockWindow> windows)
    {
        foreach (var window in windows)
        {
            if (window.Layout is { })
            {
                Restore(window.Layout);
            }
        }
    }

    private void SaveDockables(IList<IDockable> dockables)
    {
        foreach (var dockable in dockables)
        {
            SaveDockable(dockable);

            if (dockable is IDock childDock)
            {
                Save(childDock);
            }
        }
    }

    private void RestoreDockables(IList<IDockable> dockables)
    {
        foreach (var dockable in dockables)
        {
            RestoreDockable(dockable);

            if (dockable is IDock childDock)
            {
                Restore(childDock);
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
            case IDocumentDockContent documentDock:
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

    private void RestoreDockable(IDockable dockable)
    {
        switch (dockable)
        {
            case IToolContent tool:
            {
                var id = tool.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    if (_toolContents.TryGetValue(id, out var content))
                    {
                        tool.Content = content;
                    }
                }

                break;
            }
            case IDocumentContent document:
            {
                var haveContent = false;
                var id = document.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    if (_documentContents.TryGetValue(id, out var content))
                    {
                        document.Content = content;
                        haveContent = true;
                    }
                }

                if (haveContent == false)
                {
                    if (document.Owner is IDocumentDockContent documentDock)
                    {
                        if (documentDock.DocumentTemplate is { })
                        {
                           document.Content = documentDock.DocumentTemplate.Content;
                        }
                    }
                }

                break;
            }
            case IDocumentDockContent documentDock:
            {
                var id = documentDock.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    if (_documentTemplates.TryGetValue(id, out var content))
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
