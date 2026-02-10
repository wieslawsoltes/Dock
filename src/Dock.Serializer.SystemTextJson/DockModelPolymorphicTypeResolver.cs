// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Serializer.SystemTextJson;

internal sealed class DockModelPolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    private static readonly Type[] s_polymorphicBaseTypes =
    [
        typeof(IDockable),
        typeof(IDock),
        typeof(IRootDock),
        typeof(IDockWindow),
        typeof(IDocumentTemplate),
        typeof(IToolTemplate)
    ];

    private static readonly Lazy<IReadOnlyDictionary<Type, HashSet<string>>> s_interfaceIgnoredMembers =
        new(BuildInterfaceIgnoredMembers);

    private static readonly JsonPolymorphismOptions s_dockableOptions =
        CreateOptions(typeof(IDockable), JsonUnknownDerivedTypeHandling.FallBackToBaseType);

    private static readonly JsonPolymorphismOptions s_dockOptions =
        CreateOptions(typeof(IDock), JsonUnknownDerivedTypeHandling.FallBackToBaseType);

    private static readonly JsonPolymorphismOptions s_rootDockOptions =
        CreateOptions(typeof(IRootDock), JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor);

    private static readonly JsonPolymorphismOptions s_windowOptions =
        CreateOptions(typeof(IDockWindow), JsonUnknownDerivedTypeHandling.FallBackToBaseType);

    private static readonly JsonPolymorphismOptions s_documentTemplateOptions =
        CreateOptions(typeof(IDocumentTemplate), JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor);

    private static readonly JsonPolymorphismOptions s_toolTemplateOptions =
        CreateOptions(typeof(IToolTemplate), JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor);

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);
        RemoveIgnoredMembers(jsonTypeInfo);

        if (type == typeof(IDockable))
        {
            jsonTypeInfo.PolymorphismOptions = s_dockableOptions;
        }
        else if (type == typeof(IDock))
        {
            jsonTypeInfo.PolymorphismOptions = s_dockOptions;
        }
        else if (type == typeof(IRootDock))
        {
            jsonTypeInfo.PolymorphismOptions = s_rootDockOptions;
        }
        else if (type == typeof(IDockWindow))
        {
            jsonTypeInfo.PolymorphismOptions = s_windowOptions;
        }
        else if (type == typeof(IDocumentTemplate))
        {
            jsonTypeInfo.PolymorphismOptions = s_documentTemplateOptions;
        }
        else if (type == typeof(IToolTemplate))
        {
            jsonTypeInfo.PolymorphismOptions = s_toolTemplateOptions;
        }

        return jsonTypeInfo;
    }

    private static void RemoveIgnoredMembers(JsonTypeInfo jsonTypeInfo)
    {
        for (var i = jsonTypeInfo.Properties.Count - 1; i >= 0; i--)
        {
            var property = jsonTypeInfo.Properties[i];
            if (property.AttributeProvider?.IsDefined(typeof(IgnoreDataMemberAttribute), true) == true
                || typeof(ICommand).IsAssignableFrom(property.PropertyType)
                || IsIgnoredInterfaceMember(jsonTypeInfo.Type, property.Name))
            {
                jsonTypeInfo.Properties.RemoveAt(i);
            }
        }
    }

    private static bool IsIgnoredInterfaceMember(Type type, string propertyName)
    {
        if (!type.IsInterface)
        {
            return false;
        }

        if (!s_interfaceIgnoredMembers.Value.TryGetValue(type, out var ignoredMembers))
        {
            return false;
        }

        return ignoredMembers.Contains(propertyName);
    }

    private static JsonPolymorphismOptions CreateOptions(Type baseType, JsonUnknownDerivedTypeHandling handling)
    {
        var options = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type",
            UnknownDerivedTypeHandling = handling,
            IgnoreUnrecognizedTypeDiscriminators = true,
        };

        foreach (var derivedType in GetDerivedTypes(baseType))
        {
            options.DerivedTypes.Add(new JsonDerivedType(derivedType, derivedType.FullName ?? derivedType.Name));
        }

        return options;
    }

    private static IReadOnlyList<Type> GetDerivedTypes(Type baseType)
    {
        var types = new HashSet<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            foreach (var type in GetAssemblyTypesSafely(assembly))
            {
                if (type is null || !type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
                {
                    continue;
                }

                if (!IsPublicType(type) || !baseType.IsAssignableFrom(type))
                {
                    continue;
                }

                types.Add(type);
            }
        }

        return types
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<Type?> GetAssemblyTypesSafely(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types;
        }
    }

    private static bool IsPublicType(Type type)
    {
        return type.IsPublic || type.IsNestedPublic;
    }

    private static IReadOnlyDictionary<Type, HashSet<string>> BuildInterfaceIgnoredMembers()
    {
        var map = new Dictionary<Type, HashSet<string>>();
        foreach (var baseType in s_polymorphicBaseTypes)
        {
            var ignoredMembers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var type in GetAssignableTypes(baseType))
            {
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.IsDefined(typeof(IgnoreDataMemberAttribute), true)
                        || typeof(ICommand).IsAssignableFrom(property.PropertyType))
                    {
                        ignoredMembers.Add(property.Name);
                    }
                }
            }

            if (ignoredMembers.Count > 0)
            {
                map[baseType] = ignoredMembers;
            }
        }

        return map;
    }

    private static IEnumerable<Type> GetAssignableTypes(Type baseType)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            foreach (var type in GetAssemblyTypesSafely(assembly))
            {
                if (type is null || !type.IsClass || type.ContainsGenericParameters)
                {
                    continue;
                }

                if (!IsPublicType(type) || !baseType.IsAssignableFrom(type))
                {
                    continue;
                }

                yield return type;
            }
        }
    }
}
