// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
#if NET6_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// Configures polymorphic handling for Dock model interfaces so interface-typed
/// properties (e.g. <see cref="IOverlayPanel"/>) can be deserialized.
/// </summary>
internal sealed class DockModelTypeResolver : DefaultJsonTypeInfoResolver
{
    private static readonly ConcurrentDictionary<Type, JsonPolymorphismOptions> s_cache = new();

    private static readonly Type[] s_polymorphicBases =
    {
        typeof(IDockable),
        typeof(IDock),
        typeof(IRootDock),
        typeof(IDockWindow),
        typeof(IOverlayDock),
        typeof(IOverlayPanel),
        typeof(IOverlaySplitterGroup),
        typeof(IOverlaySplitter)
    };

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var info = base.GetTypeInfo(type, options);

        if (s_polymorphicBases.Any(baseType => baseType.IsAssignableFrom(info.Type)))
        {
            info.PolymorphismOptions = s_cache.GetOrAdd(info.Type, CreateOptions);
        }

        return info;
    }

    private static JsonPolymorphismOptions CreateOptions(Type baseType)
    {
        var derivedTypes = DiscoverDerivedTypes(baseType)
            .Select(t => new JsonDerivedType(t, GetTypeDiscriminator(t)));

        var options = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type",
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType,
            IgnoreUnrecognizedTypeDiscriminators = false
        };

        foreach (var derived in derivedTypes)
        {
            options.DerivedTypes.Add(derived);
        }

        return options;
    }

    private static string GetTypeDiscriminator(Type type)
    {
        return type.FullName ?? type.Name;
    }

    private static bool ShouldScanAssembly(Assembly assembly)
    {
        if (assembly.IsDynamic)
        {
            return false;
        }

        var name = assembly.GetName().Name;
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (name.StartsWith("Dock", StringComparison.Ordinal) || name.StartsWith("Dock.", StringComparison.Ordinal))
        {
            return true;
        }

        return assembly.GetReferencedAssemblies()
            .Any(reference =>
            {
                var referenceName = reference.Name;
                return referenceName is not null
                    && (referenceName.StartsWith("Dock", StringComparison.Ordinal)
                        || referenceName.StartsWith("Dock.", StringComparison.Ordinal));
            });
    }

    private static IEnumerable<Type> DiscoverDerivedTypes(Type baseType)
    {
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(ShouldScanAssembly);

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
                && baseType.IsAssignableFrom(t)
                && !t.IsInterface
                && !t.IsAbstract
                && t.GetConstructor(Type.EmptyTypes) is not null)
            .Distinct();
    }
}
#endif
