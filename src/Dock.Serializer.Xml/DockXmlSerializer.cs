// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Serializer.Xml;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using XML serialization.
/// </summary>
public sealed class DockXmlSerializer : IDockSerializer
{
    private readonly Type _listType;
    private readonly Type[] _knownTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockXmlSerializer"/> class with optional known types and a list type.
    /// </summary>
    /// <param name="listType">The generic list type to instantiate.</param>
    /// <param name="knownTypes">Additional types used during serialization.</param>
    public DockXmlSerializer(Type listType, params Type[] knownTypes)
    {
        _listType = listType;
        _knownTypes = knownTypes ?? Array.Empty<Type>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockXmlSerializer"/> class.
    /// </summary>
    public DockXmlSerializer() : this(typeof(ObservableCollection<>), Array.Empty<Type>())
    {
    }

    private DataContractSerializer CreateSerializer(Type type)
    {
        var settings = new DataContractSerializerSettings
        {
            PreserveObjectReferences = true,
            KnownTypes = GetKnownTypes()
        };
        return new DataContractSerializer(type, settings);
    }

    private IEnumerable<Type> GetKnownTypes()
    {
        if (_knownTypes.Length == 0)
        {
            return BuildDefaultKnownTypes();
        }

        var types = new HashSet<Type>(_knownTypes);
        foreach (var type in BuildDefaultKnownTypes())
        {
            types.Add(type);
        }

        return types;
    }

    private static IEnumerable<Type> BuildDefaultKnownTypes()
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

                if (!IsPublicType(type) || !ImplementsDockContracts(type))
                {
                    continue;
                }

                if (!IsDataContractCompatible(type))
                {
                    continue;
                }

                types.Add(type);
            }
        }

        return types;
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

    private static bool ImplementsDockContracts(Type type)
    {
        return typeof(IDockable).IsAssignableFrom(type)
            || typeof(IDockWindow).IsAssignableFrom(type)
            || typeof(IDocumentTemplate).IsAssignableFrom(type)
            || typeof(IToolTemplate).IsAssignableFrom(type);
    }

    private static bool IsDataContractCompatible(Type type)
    {
        var current = type;
        while (current is not null && current != typeof(object))
        {
            if (HasDataContractOrSerializable(current))
            {
                var baseType = current.BaseType;
                if (baseType is not null && baseType != typeof(object) && !HasDataContractOrSerializable(baseType))
                {
                    return false;
                }
            }

            current = current.BaseType;
        }

        return true;
    }

    private static bool HasDataContractOrSerializable(Type type)
    {
        return type.IsDefined(typeof(SerializableAttribute), false)
            || type.IsDefined(typeof(DataContractAttribute), false);
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        var serializer = CreateSerializer(typeof(T));
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
        {
            serializer.WriteObject(writer, value);
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        var serializer = CreateSerializer(typeof(T));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        using var reader = XmlReader.Create(stream);
        var result = (T?)serializer.ReadObject(reader);
        ListTypeConverter.Convert(result, _listType);
        return result;
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = reader.ReadToEnd();
        return Deserialize<T>(text);
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        var text = Serialize(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(text);
    }
}
