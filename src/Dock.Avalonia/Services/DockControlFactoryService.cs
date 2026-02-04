// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Recycling.Model;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Services;

internal sealed class DockControlFactoryService : IDockControlFactoryService
{
    private static readonly ConditionalWeakTable<IFactory, IControlRecycling> s_controlRecycling = new();

    public void InitializeControlRecycling(DockControl control)
    {
        if (control.Layout?.Factory is not { } factory)
        {
            return;
        }

        var controlRecycling = ControlRecyclingDataTemplate.GetControlRecycling(control);
        if (controlRecycling is null)
        {
            return;
        }

        if (s_controlRecycling.TryGetValue(factory, out var shared))
        {
            if (ReferenceEquals(shared, controlRecycling))
            {
                return;
            }

            if (shared is ControlRecycling sharedRecycling && controlRecycling is ControlRecycling localRecycling)
            {
                if (sharedRecycling.TryToUseIdAsKey != localRecycling.TryToUseIdAsKey)
                {
                    sharedRecycling.TryToUseIdAsKey = localRecycling.TryToUseIdAsKey;
                }

                ControlRecyclingDataTemplate.SetControlRecycling(control, sharedRecycling);
                return;
            }

            if (controlRecycling is ControlRecycling)
            {
                ControlRecyclingDataTemplate.SetControlRecycling(control, shared);
            }

            return;
        }

        if (controlRecycling is ControlRecycling defaultRecycling)
        {
            controlRecycling = new ControlRecycling
            {
                TryToUseIdAsKey = defaultRecycling.TryToUseIdAsKey
            };
            ControlRecyclingDataTemplate.SetControlRecycling(control, controlRecycling);
        }

        s_controlRecycling.Add(factory, controlRecycling);
    }

    public void CleanupFactory(DockControl control, IDock layout)
    {
        if (layout.Factory is not { } factory)
        {
            return;
        }

        if (!HasOtherDockControlForLayout(factory, layout))
        {
            PruneFactoryCaches(factory, layout);
        }

        ReleaseFactoryDefaults(factory, control);
    }

    private static bool HasOtherDockControlForLayout(IFactory factory, IDock layout)
    {
        foreach (var control in factory.DockControls.OfType<DockControl>())
        {
            if (!ReferenceEquals(control.Layout, layout))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static void PruneFactoryCaches(IFactory factory, IDock layout)
    {
        var dockables = new HashSet<IDockable>();
        CollectAllDockables(layout, dockables);

        PruneDictionary(factory.VisibleDockableControls, dockables);
        PruneDictionary(factory.PinnedDockableControls, dockables);
        PruneDictionary(factory.TabDockableControls, dockables);
        PruneDictionary(factory.VisibleRootControls, dockables);
        PruneDictionary(factory.PinnedRootControls, dockables);
        PruneDictionary(factory.TabRootControls, dockables);
        PruneDictionary(factory.ToolControls, dockables);
        PruneDictionary(factory.DocumentControls, dockables);
        PruneControlRecycling(factory, dockables);
    }

    private static void PruneDictionary<TValue>(IDictionary<IDockable, TValue> dictionary, HashSet<IDockable> dockables)
    {
        if (dictionary.Count == 0)
        {
            return;
        }

        var keys = dictionary.Keys.Where(dockables.Contains).ToList();
        foreach (var key in keys)
        {
            dictionary.Remove(key);
        }
    }

    private static void PruneControlRecycling(IFactory factory, HashSet<IDockable> dockables)
    {
        if (!s_controlRecycling.TryGetValue(factory, out var recycling))
        {
            return;
        }

        if (recycling is not ControlRecycling controlRecycling)
        {
            return;
        }

        foreach (var dockable in dockables)
        {
            controlRecycling.Remove(dockable);
        }
    }

    private static void CollectAllDockables(IDockable dockable, HashSet<IDockable> dockables)
    {
        if (!dockables.Add(dockable))
        {
            return;
        }

        if (dockable is IRootDock root)
        {
            AddDockablesAll(root.HiddenDockables, dockables);
            AddDockablesAll(root.LeftPinnedDockables, dockables);
            AddDockablesAll(root.RightPinnedDockables, dockables);
            AddDockablesAll(root.TopPinnedDockables, dockables);
            AddDockablesAll(root.BottomPinnedDockables, dockables);

            if (root.PinnedDock is { } pinnedDock)
            {
                CollectAllDockables(pinnedDock, dockables);
            }

            if (root.Windows is { })
            {
                foreach (var window in root.Windows)
                {
                    if (window.Layout is { } layout)
                    {
                        CollectAllDockables(layout, dockables);
                    }
                }
            }
        }

        if (dockable is IDock dock && dock.VisibleDockables is { })
        {
            foreach (var child in dock.VisibleDockables)
            {
                CollectAllDockables(child, dockables);
            }
        }

        if (dockable is ISplitViewDock splitViewDock)
        {
            if (splitViewDock.PaneDockable is { } paneDockable)
            {
                CollectAllDockables(paneDockable, dockables);
            }

            if (splitViewDock.ContentDockable is { } contentDockable
                && !ReferenceEquals(contentDockable, splitViewDock.PaneDockable))
            {
                CollectAllDockables(contentDockable, dockables);
            }
        }

        if (dockable is IDockWindow dockWindow && dockWindow.Layout is { } dockLayout)
        {
            CollectAllDockables(dockLayout, dockables);
        }
    }

    private static void AddDockablesAll(IList<IDockable>? source, HashSet<IDockable> dockables)
    {
        if (source is null)
        {
            return;
        }

        foreach (var dockable in source)
        {
            CollectAllDockables(dockable, dockables);
        }
    }

    private static void ReleaseFactoryDefaults(IFactory factory, DockControl owner)
    {
        var remaining = factory.DockControls
            .OfType<DockControl>()
            .FirstOrDefault(control => !ReferenceEquals(control, owner));

        if (ReferenceEquals(factory.DefaultContextLocator?.Target, owner))
        {
            factory.DefaultContextLocator = remaining is null ? null : remaining.ResolveDefaultContext;
        }

        if (ReferenceEquals(factory.DefaultHostWindowLocator?.Target, owner))
        {
            factory.DefaultHostWindowLocator = remaining is null ? null : remaining.ResolveDefaultHostWindow;
        }

        var hostWindowLocator = factory.HostWindowLocator;
        if (hostWindowLocator is null)
        {
            return;
        }

        if (hostWindowLocator.TryGetValue(nameof(IDockWindow), out var locator)
            && ReferenceEquals(locator?.Target, owner))
        {
            if (remaining is null)
            {
                hostWindowLocator.Remove(nameof(IDockWindow));
                if (hostWindowLocator.Count == 0 && ReferenceEquals(factory.HostWindowLocator, hostWindowLocator))
                {
                    factory.HostWindowLocator = null;
                }
            }
            else
            {
                hostWindowLocator[nameof(IDockWindow)] = remaining.ResolveDefaultHostWindow;
            }
        }
    }
}
