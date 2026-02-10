// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using ProtoBuf.Meta;

namespace Dock.Serializer.Protobuf;

/// <summary>
/// A serializer implementation using protobuf-net.
/// </summary>
public sealed class ProtobufDockSerializer : IDockSerializer
{
    private readonly Type _listType;
    private readonly object _modelLock = new();
    private RuntimeTypeModel? _model;
    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufDockSerializer"/> class with the specified list type.
    /// </summary>
    /// <param name="listType">The generic list type to instantiate.</param>
    public ProtobufDockSerializer(Type listType)
    {
        _listType = listType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufDockSerializer"/> class.
    /// </summary>
    public ProtobufDockSerializer() : this(typeof(ObservableCollection<>))
    {
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        Model.Serialize(stream, value!);
        return Convert.ToBase64String(stream.ToArray());
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        var bytes = Convert.FromBase64String(text);
        using var stream = new MemoryStream(bytes);
        var result = (T?)Model.Deserialize(stream, null, typeof(T));
        ListTypeConverter.Convert(result, _listType);
        return result;
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        var result = (T?)Model.Deserialize(stream, null, typeof(T));
        ListTypeConverter.Convert(result, _listType);
        return result;
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        Model.Serialize(stream, value!);
    }

    private RuntimeTypeModel Model
    {
        get
        {
            if (_model is not null)
            {
                return _model;
            }

            lock (_modelLock)
            {
                _model ??= BuildModel();
            }

            return _model;
        }
    }

    private static RuntimeTypeModel BuildModel()
    {
        var model = RuntimeTypeModel.Create();
        model.AutoAddMissingTypes = true;
        model.InferTagFromNameDefault = true;

        var dockInterfaces = GetDockInterfaceTypes();
        var dockClasses = GetDockClassTypes(dockInterfaces);
        foreach (var dockInterface in dockInterfaces)
        {
            model.Add(dockInterface, false);
        }

        foreach (var dockClass in dockClasses)
        {
            ConfigureClass(model, dockClass);
        }

        var interfaceHierarchy = BuildInterfaceHierarchy(dockInterfaces);
        var classHierarchy = BuildClassHierarchy(dockInterfaces, dockClasses);

        foreach (var baseType in dockInterfaces)
        {
            var derivedTypes = new List<Type>();
            if (interfaceHierarchy.TryGetValue(baseType, out var interfaceTypes))
            {
                derivedTypes.AddRange(interfaceTypes);
            }

            if (classHierarchy.TryGetValue(baseType, out var classTypes))
            {
                derivedTypes.AddRange(classTypes);
            }

            AddSubTypes(model, baseType, derivedTypes);
        }

        return model;
    }

    private static void AddSubTypes(RuntimeTypeModel model, Type baseType, IReadOnlyList<Type> derivedTypes)
    {
        if (derivedTypes.Count == 0)
        {
            return;
        }

        var metaType = model[baseType];
        var fieldNumber = 1000;
        foreach (var derivedType in derivedTypes
            .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            metaType.AddSubType(fieldNumber++, derivedType);
        }
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

    private static IReadOnlyList<Type> GetDockClassTypes(IReadOnlyCollection<Type> dockInterfaces)
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

                if (!IsPublicType(type) || !ImplementsDockInterfaces(type, dockInterfaces))
                {
                    continue;
                }

                types.Add(type);
            }
        }

        return types
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool ImplementsDockInterfaces(Type type, IReadOnlyCollection<Type> dockInterfaces)
    {
        foreach (var dockInterface in dockInterfaces)
        {
            if (dockInterface.IsAssignableFrom(type))
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<Type, List<Type>> BuildInterfaceHierarchy(IReadOnlyCollection<Type> dockInterfaces)
    {
        var map = new Dictionary<Type, List<Type>>();
        foreach (var dockInterface in dockInterfaces)
        {
            var baseInterface = GetClosestDockInterface(dockInterface, dockInterfaces);
            if (baseInterface is null)
            {
                continue;
            }

            AddToTypeMap(map, baseInterface, dockInterface);
        }

        return map;
    }

    private static Dictionary<Type, List<Type>> BuildClassHierarchy(
        IReadOnlyCollection<Type> dockInterfaces,
        IReadOnlyCollection<Type> dockClasses)
    {
        var map = new Dictionary<Type, List<Type>>();
        foreach (var dockClass in dockClasses)
        {
            var baseInterface = GetClosestDockInterface(dockClass, dockInterfaces);
            if (baseInterface is null)
            {
                continue;
            }

            AddToTypeMap(map, baseInterface, dockClass);
        }

        return map;
    }

    private static Type? GetClosestDockInterface(Type type, IReadOnlyCollection<Type> dockInterfaces)
    {
        var candidates = type.GetInterfaces()
            .Where(dockInterfaces.Contains)
            .ToArray();

        if (candidates.Length == 0)
        {
            return null;
        }

        var bestCandidates = new List<Type>();
        foreach (var candidate in candidates)
        {
            var isMostDerived = true;
            foreach (var other in candidates)
            {
                if (candidate == other)
                {
                    continue;
                }

                if (candidate.IsAssignableFrom(other))
                {
                    isMostDerived = false;
                    break;
                }
            }

            if (isMostDerived)
            {
                bestCandidates.Add(candidate);
            }
        }

        if (bestCandidates.Count == 0)
        {
            bestCandidates.AddRange(candidates);
        }

        return bestCandidates
            .OrderBy(candidate => candidate.FullName, StringComparer.Ordinal)
            .First();
    }

    private static void AddToTypeMap(Dictionary<Type, List<Type>> map, Type baseType, Type derivedType)
    {
        if (!map.TryGetValue(baseType, out var types))
        {
            types = new List<Type>();
            map[baseType] = types;
        }

        if (!types.Contains(derivedType))
        {
            types.Add(derivedType);
        }
    }

    private static IReadOnlyList<Type> GetDockInterfaceTypes()
    {
        var types = new HashSet<Type>
        {
            typeof(IDockable),
            typeof(IDockWindow),
            typeof(IDocumentTemplate),
            typeof(IToolTemplate)
        };

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            foreach (var type in GetAssemblyTypesSafely(assembly))
            {
                if (type is null || !type.IsInterface)
                {
                    continue;
                }

                if (!IsPublicType(type))
                {
                    continue;
                }

                if (typeof(IDockable).IsAssignableFrom(type)
                    || typeof(IDockWindow).IsAssignableFrom(type)
                    || typeof(IDocumentTemplate).IsAssignableFrom(type)
                    || typeof(IToolTemplate).IsAssignableFrom(type))
                {
                    types.Add(type);
                }
            }
        }

        return types
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ConfigureClass(RuntimeTypeModel model, Type type)
    {
        var metaType = model.Add(type, false);
        var fieldNumber = 1;
        foreach (var property in GetSerializableProperties(type))
        {
            metaType.Add(fieldNumber++, property.Name);
        }
    }

    private static IReadOnlyList<PropertyInfo> GetSerializableProperties(Type type)
    {
        var properties = new List<PropertyInfo>();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }

            if (property.IsDefined(typeof(IgnoreDataMemberAttribute), true))
            {
                continue;
            }

            if (!property.IsDefined(typeof(DataMemberAttribute), true))
            {
                continue;
            }

            properties.Add(property);
        }

        return properties
            .GroupBy(property => property.Name, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
    }
}
