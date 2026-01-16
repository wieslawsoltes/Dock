// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dock.Model.Core;

namespace Dock.Serializer.Xml;

internal static class KnownTypeHelper
{
    public static IEnumerable<Type> CollectKnownTypes(IEnumerable<Type> additional)
    {
        var result = new List<Type>();

        // include caller-provided types first
        result.AddRange(additional.Where(t => t is not null));

        foreach (var type in DiscoverDockTypes())
        {
            if (!result.Contains(type))
            {
                result.Add(type);
            }
        }

        return result;
    }

    private static IEnumerable<Type> DiscoverDockTypes()
    {
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a =>
            {
                var name = a.GetName().Name;
                return name is not null && (name.StartsWith("Dock", StringComparison.Ordinal) || name.StartsWith("Dock.", StringComparison.Ordinal));
            });

        return assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t is not null)!.Cast<Type>();
                }
            })
            .Where(t => t is not null
                && typeof(IDockable).IsAssignableFrom(t)
                && !t.IsInterface
                && !t.IsAbstract
                && t.GetConstructor(Type.EmptyTypes) is not null)
            .Distinct();
    }
}