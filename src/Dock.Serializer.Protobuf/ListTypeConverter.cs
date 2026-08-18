// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Serializer.Protobuf;

internal static class ListTypeConverter
{
    public static void Convert(object? obj, Type listType)
    {
        Convert(obj, listType, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    private static void Convert(object? obj, Type listType, HashSet<object> visited)
    {
        if (obj is null)
            return;
        if (!visited.Add(obj))
            return;
        if (obj is IEnumerable enumerable && obj is not string)
        {
            foreach (var item in enumerable)
            {
                Convert(item, listType, visited);
            }
        }
        var type = obj.GetType();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || !property.CanWrite)
                continue;
            var value = property.GetValue(obj);
            if (value is null)
                continue;
            var propType = property.PropertyType;
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var elementType = propType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(listType.MakeGenericType(elementType))!;
                foreach (var item in (IEnumerable)value)
                {
                    list.Add(item);
                }
                foreach (var item in list)
                {
                    Convert(item, listType, visited);
                }
                property.SetValue(obj, list);
            }
            else
            {
                Convert(value, listType, visited);
            }
        }

        // protobuf-net cannot preserve references: reconcile the dockable aliases that came back
        // as duplicates, and rebuild Owner (excluded from the wire) from tree containment.
        if (obj is IDock dock)
        {
            Reconcile(dock, dock.ActiveDockable, v => dock.ActiveDockable = v);
            Reconcile(dock, dock.DefaultDockable, v => dock.DefaultDockable = v);
            Reconcile(dock, dock.FocusedDockable, v => dock.FocusedDockable = v);

            SetOwner(dock.VisibleDockables, dock);

            if (dock is IRootDock rootDock)
            {
                SetOwner(rootDock.HiddenDockables, rootDock);
                SetOwner(rootDock.LeftPinnedDockables, rootDock);
                SetOwner(rootDock.RightPinnedDockables, rootDock);
                SetOwner(rootDock.TopPinnedDockables, rootDock);
                SetOwner(rootDock.BottomPinnedDockables, rootDock);

                if (rootDock.PinnedDock is { } pinnedDock)
                {
                    pinnedDock.Owner = rootDock;
                }

                if (rootDock.Windows is not null)
                {
                    foreach (var window in rootDock.Windows)
                    {
                        window.Owner = rootDock;
                    }
                }
            }

            if (dock is ISplitViewDock splitViewDock)
            {
                if (splitViewDock.PaneDockable is { } paneDockable)
                {
                    paneDockable.Owner = dock;
                }

                if (splitViewDock.ContentDockable is { } contentDockable)
                {
                    contentDockable.Owner = dock;
                }
            }
        }
    }

    private static void SetOwner(IList<IDockable>? dockables, IDockable owner)
    {
        if (dockables is null)
        {
            return;
        }

        foreach (var dockable in dockables)
        {
            dockable.Owner = owner;
        }
    }

    private static void Reconcile(IDock dock, IDockable? current, Action<IDockable> setter)
    {
        if (current is null || string.IsNullOrEmpty(current.Id))
        {
            return;
        }

        var canonical = FindById(dock.VisibleDockables, current.Id)
            ?? (dock is IRootDock rootDock ? FindById(rootDock.HiddenDockables, current.Id) : null);

        if (canonical is not null && !ReferenceEquals(canonical, current))
        {
            setter(canonical);
        }
    }

    private static IDockable? FindById(IList<IDockable>? dockables, string id)
    {
        if (dockables is null)
        {
            return null;
        }

        IDockable? match = null;
        foreach (var dockable in dockables)
        {
            if (dockable.Id != id)
            {
                continue;
            }

            if (match is not null)
            {
                // Ambiguous: more than one dockable shares this Id, so we cannot pick the alias safely.
                return null;
            }

            match = dockable;
        }

        return match;
    }
}
