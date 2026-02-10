// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace Dock.Serializer.Yaml;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using YAML serialization.
/// </summary>
public sealed class DockYamlSerializer : IDockSerializer
{
    private ISerializer? _serializer;
    private IDeserializer? _deserializer;
    private readonly Type _listType;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockYamlSerializer"/> class
    /// with the specified list type.
    /// </summary>
    /// <param name="listType">The generic list type to instantiate.</param>
    public DockYamlSerializer(Type listType)
    {
        _listType = listType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockYamlSerializer"/> class.
    /// </summary>
    public DockYamlSerializer() : this(typeof(ObservableCollection<>))
    {
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        return Serializer.Serialize(value);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        var result = Deserializer.Deserialize<T>(text);
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

    private ISerializer Serializer => _serializer ??= CreateSerializer();

    private IDeserializer Deserializer => _deserializer ??= CreateDeserializer();

    private ISerializer CreateSerializer()
    {
        var tagMappings = BuildTagMappings();
        var serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .EnsureRoundtrip()
            .WithTypeResolver(new DynamicTypeResolver())
            .WithTypeInspector(inner => new IgnoreDataMemberTypeInspector(inner));

        foreach (var (tag, type) in tagMappings)
        {
            serializerBuilder.WithTagMapping(tag, type);
        }

        return serializerBuilder.Build();
    }

    private IDeserializer CreateDeserializer()
    {
        var tagMappings = BuildTagMappings();
        var deserializerBuilder = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .WithTypeInspector(inner => new IgnoreDataMemberTypeInspector(inner));

        foreach (var (tag, type) in tagMappings)
        {
            deserializerBuilder.WithTagMapping(tag, type);
        }

        return deserializerBuilder.Build();
    }

    private static IReadOnlyList<(TagName Tag, Type Type)> BuildTagMappings()
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

                if (string.IsNullOrWhiteSpace(type.FullName))
                {
                    continue;
                }

                types.Add(type);
            }
        }

        return types
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(type => (Tag: new TagName($"!{type.FullName}"), Type: type))
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

    private static bool ImplementsDockContracts(Type type)
    {
        return typeof(IDockable).IsAssignableFrom(type)
            || typeof(IDockWindow).IsAssignableFrom(type)
            || typeof(IDocumentTemplate).IsAssignableFrom(type)
            || typeof(IToolTemplate).IsAssignableFrom(type);
    }

    private sealed class IgnoreDataMemberTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _inner;

        public IgnoreDataMemberTypeInspector(ITypeInspector inner)
        {
            _inner = inner;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            foreach (var property in _inner.GetProperties(type, container))
            {
                if (property.GetCustomAttribute<IgnoreDataMemberAttribute>() is not null)
                {
                    continue;
                }

                if (typeof(ICommand).IsAssignableFrom(property.Type))
                {
                    continue;
                }

                yield return property;
            }
        }

        public override string GetEnumName(Type enumType, string name)
        {
            return _inner.GetEnumName(enumType, name);
        }

        public override string GetEnumValue(object enumValue)
        {
            return _inner.GetEnumValue(enumValue);
        }
    }
}
