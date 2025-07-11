// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Dock.Model.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Dock.Serializer.Yaml;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using YAML serialization.
/// </summary>
public sealed class DockYamlSerializer : IDockSerializer
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    private readonly Type _listType;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockYamlSerializer"/> class
    /// with the specified list type.
    /// </summary>
    /// <param name="listType">The generic list type to instantiate.</param>
    public DockYamlSerializer(Type listType)
    {
        _listType = listType;
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
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
        return _serializer.Serialize(value);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        var result = _deserializer.Deserialize<T>(text);
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
