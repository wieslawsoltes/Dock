// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.CompilerServices;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal static class ManagedWindowRegistry
{
    private static readonly ConditionalWeakTable<IFactory, ManagedWindowDock> s_docks = new();
    private static readonly ConditionalWeakTable<IFactory, ManagedWindowLayer> s_layers = new();

    public static ManagedWindowDock GetOrCreateDock(IFactory factory)
    {
        if (!s_docks.TryGetValue(factory, out var dock))
        {
            dock = new ManagedWindowDock
            {
                Id = "ManagedWindowDock",
                Title = "Managed Windows",
                Factory = factory
            };
            s_docks.Add(factory, dock);
        }

        return dock;
    }

    public static ManagedWindowLayer? TryGetLayer(IFactory factory)
    {
        return s_layers.TryGetValue(factory, out var layer) ? layer : null;
    }

    public static void RegisterLayer(IFactory factory, ManagedWindowLayer layer)
    {
        if (s_layers.TryGetValue(factory, out var existing) && ReferenceEquals(existing, layer))
        {
            layer.Dock = GetOrCreateDock(factory);
            return;
        }

        if (s_layers.TryGetValue(factory, out existing))
        {
            existing.Dock = null;
            existing.IsVisible = false;
            s_layers.Remove(factory);
        }

        s_layers.Add(factory, layer);
        layer.Dock = GetOrCreateDock(factory);
    }

    public static void UnregisterLayer(IFactory factory, ManagedWindowLayer layer)
    {
        if (s_layers.TryGetValue(factory, out var existing) && ReferenceEquals(existing, layer))
        {
            s_layers.Remove(factory);
            layer.Dock = null;
            layer.IsVisible = false;
        }
    }
}
