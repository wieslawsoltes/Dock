// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.CommandBars;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using Dock.Settings;

namespace Dock.Avalonia.CommandBars;

/// <summary>
/// Manages command bar merging for a dock control.
/// </summary>
public sealed class DockCommandBarManager
{
    private readonly DockCommandBarHost _host;
    private readonly IDockCommandBarAdapter _adapter;
    private IDock? _layout;
    private IFactory? _factory;
    private IDockCommandBarProvider? _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockCommandBarManager"/> class.
    /// </summary>
    /// <param name="host">The command bar host control.</param>
    /// <param name="adapter">The adapter used to build bars.</param>
    public DockCommandBarManager(DockCommandBarHost host, IDockCommandBarAdapter? adapter = null)
    {
        _host = host;
        _adapter = adapter ?? new DefaultDockCommandBarAdapter();
    }

    /// <summary>
    /// Attaches to the provided layout.
    /// </summary>
    /// <param name="layout">The dock layout.</param>
    public void Attach(IDock layout)
    {
        if (ReferenceEquals(_layout, layout))
        {
            UpdateBars();
            return;
        }

        Detach();
        _layout = layout;
        _factory = layout.Factory;

        if (_factory is { })
        {
            _factory.ActiveDockableChanged += FactoryActiveDockableChanged;
        }

        UpdateBars();
    }

    /// <summary>
    /// Detaches from the current layout.
    /// </summary>
    public void Detach()
    {
        if (_factory is { })
        {
            _factory.ActiveDockableChanged -= FactoryActiveDockableChanged;
        }

        if (_provider is { })
        {
            _provider.CommandBarsChanged -= ProviderCommandBarsChanged;
            _provider = null;
        }

        _factory = null;
        _layout = null;
        ClearHost();
    }

    private void FactoryActiveDockableChanged(object? sender, ActiveDockableChangedEventArgs e)
    {
        UpdateBars();
    }

    private void ProviderCommandBarsChanged(object? sender, EventArgs e)
    {
        UpdateBars();
    }

    private void UpdateBars()
    {
        if (!DockSettings.CommandBarMergingEnabled || _layout?.Factory is null)
        {
            ClearHost();
            return;
        }

        var activeDockable = ResolveActiveDockable(_layout, DockSettings.CommandBarMergingScope);
        var provider = activeDockable is { } dockable ? FindProvider(dockable) : null;
        if (!ReferenceEquals(_provider, provider))
        {
            if (_provider is { })
            {
                _provider.CommandBarsChanged -= ProviderCommandBarsChanged;
            }

            _provider = provider;

            if (_provider is { })
            {
                _provider.CommandBarsChanged += ProviderCommandBarsChanged;
            }
        }

        var baseBars = _host.BaseCommandBars ?? Array.Empty<DockCommandBarDefinition>();
        var activeBars = _provider?.GetCommandBars() ?? Array.Empty<DockCommandBarDefinition>();
        var merged = MergeBars(baseBars, activeBars);

        var menuBars = _adapter.BuildBars(DockCommandBarKind.Menu, merged.Where(bar => bar.Kind == DockCommandBarKind.Menu).ToList());
        var toolBars = _adapter.BuildBars(DockCommandBarKind.ToolBar, merged.Where(bar => bar.Kind == DockCommandBarKind.ToolBar).ToList());
        var ribbonBars = _adapter.BuildBars(DockCommandBarKind.Ribbon, merged.Where(bar => bar.Kind == DockCommandBarKind.Ribbon).ToList());

        _host.MenuBars = menuBars.Count > 0 ? menuBars : null;
        _host.ToolBars = toolBars.Count > 0 ? toolBars : null;
        _host.RibbonBars = ribbonBars.Count > 0 ? ribbonBars : null;
        _host.IsVisible = menuBars.Count > 0 || toolBars.Count > 0 || ribbonBars.Count > 0;
    }

    private void ClearHost()
    {
        _host.MenuBars = null;
        _host.ToolBars = null;
        _host.RibbonBars = null;
        _host.IsVisible = false;
    }

    private static IDockCommandBarProvider? FindProvider(IDockable dockable)
    {
        if (dockable is IDockCommandBarProvider provider)
        {
            return provider;
        }

        return dockable.Context as IDockCommandBarProvider;
    }

    private static IDockable? ResolveActiveDockable(IDock layout, DockCommandBarMergingScope scope)
    {
        var activeDockable = layout.ActiveDockable;
        if (scope == DockCommandBarMergingScope.ActiveDockable)
        {
            if (activeDockable is IDock dock && dock.ActiveDockable is { } dockActive)
            {
                return dockActive;
            }

            return activeDockable;
        }

        if (layout is IDock rootDock)
        {
            return FindActiveDocument(rootDock);
        }

        return null;
    }

    private static IDocument? FindActiveDocument(IDock dock)
    {
        if (dock is IDocumentDock { ActiveDockable: IDocument activeDocument })
        {
            return activeDocument;
        }

        if (dock.VisibleDockables is { })
        {
            foreach (var child in dock.VisibleDockables)
            {
                if (child is IDocument activeChildDocument && ReferenceEquals(dock.ActiveDockable, activeChildDocument))
                {
                    return activeChildDocument;
                }

                if (child is IDock childDock)
                {
                    var nestedDocument = FindActiveDocument(childDock);
                    if (nestedDocument is { })
                    {
                        return nestedDocument;
                    }
                }
            }
        }

        if (dock is ISplitViewDock splitViewDock)
        {
            var pane = splitViewDock.PaneDockable as IDock;
            var content = splitViewDock.ContentDockable as IDock;

            if (pane is { })
            {
                var paneDocument = FindActiveDocument(pane);
                if (paneDocument is { })
                {
                    return paneDocument;
                }
            }

            if (content is { })
            {
                var contentDocument = FindActiveDocument(content);
                if (contentDocument is { })
                {
                    return contentDocument;
                }
            }
        }

        return null;
    }

    private static IReadOnlyList<DockCommandBarDefinition> MergeBars(
        IReadOnlyList<DockCommandBarDefinition> baseBars,
        IReadOnlyList<DockCommandBarDefinition> activeBars)
    {
        var merged = new List<DockCommandBarDefinition>();
        foreach (DockCommandBarKind kind in Enum.GetValues(typeof(DockCommandBarKind)))
        {
            merged.AddRange(MergeBarsForKind(kind, baseBars, activeBars));
        }

        return merged;
    }

    private static List<DockCommandBarDefinition> MergeBarsForKind(
        DockCommandBarKind kind,
        IReadOnlyList<DockCommandBarDefinition> baseBars,
        IReadOnlyList<DockCommandBarDefinition> activeBars)
    {
        var baseForKind = baseBars.Where(bar => bar.Kind == kind).OrderBy(bar => bar.Order).Select(CloneBar).ToList();
        var activeForKind = activeBars.Where(bar => bar.Kind == kind).OrderBy(bar => bar.Order).ToList();

        if (activeForKind.Count == 0)
        {
            return baseForKind;
        }

        var replaceBars = activeForKind.Where(bar => bar.MergeMode == DockCommandBarMergeMode.Replace).ToList();
        if (replaceBars.Count > 0)
        {
            return replaceBars.Select(CloneBar).OrderBy(bar => bar.Order).ToList();
        }

        var merged = baseForKind;
        foreach (var bar in activeForKind)
        {
            switch (bar.MergeMode)
            {
                case DockCommandBarMergeMode.Append:
                    AppendToBar(merged, bar);
                    break;
                case DockCommandBarMergeMode.MergeByGroup:
                    MergeByGroup(merged, bar);
                    break;
                default:
                    merged.Add(CloneBar(bar));
                    break;
            }
        }

        return merged.OrderBy(bar => bar.Order).ToList();
    }

    private static void AppendToBar(IList<DockCommandBarDefinition> merged, DockCommandBarDefinition bar)
    {
        if (bar.Items is null || bar.Items.Count == 0)
        {
            merged.Add(CloneBar(bar));
            return;
        }

        var target = merged.FirstOrDefault(candidate => candidate.Id == bar.Id)
                     ?? merged.LastOrDefault();

        if (target is null || target.Items is null || target.Items.Count == 0)
        {
            merged.Add(CloneBar(bar));
            return;
        }

        var targetItems = target.Items.ToList();
        var itemsToAdd = bar.Items.OrderBy(item => item.Order);
        targetItems.AddRange(itemsToAdd);
        target.Items = targetItems;
    }

    private static void MergeByGroup(IList<DockCommandBarDefinition> merged, DockCommandBarDefinition bar)
    {
        if (bar.Items is null || bar.Items.Count == 0 || string.IsNullOrWhiteSpace(bar.GroupId))
        {
            merged.Add(CloneBar(bar));
            return;
        }

        var target = merged.FirstOrDefault(candidate => candidate.Items?.Any(item => item.GroupId == bar.GroupId) == true);
        if (target is null)
        {
            merged.Add(CloneBar(bar));
            return;
        }

        var targetItems = target.Items?.ToList() ?? new List<DockCommandBarItem>();
        var insertIndex = FindGroupInsertIndex(targetItems, bar.GroupId!);
        var itemsToInsert = bar.Items.OrderBy(item => item.Order).ToList();
        targetItems.InsertRange(insertIndex, itemsToInsert);
        target.Items = targetItems;
    }

    private static int FindGroupInsertIndex(IReadOnlyList<DockCommandBarItem> items, string groupId)
    {
        var index = -1;
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].GroupId == groupId)
            {
                index = i;
            }
        }

        return index >= 0 ? index + 1 : items.Count;
    }

    private static DockCommandBarDefinition CloneBar(DockCommandBarDefinition bar)
    {
        return new DockCommandBarDefinition(bar.Id, bar.Kind)
        {
            MergeMode = bar.MergeMode,
            Order = bar.Order,
            GroupId = bar.GroupId,
            Content = bar.Content,
            Items = bar.Items?.ToList()
        };
    }
}
