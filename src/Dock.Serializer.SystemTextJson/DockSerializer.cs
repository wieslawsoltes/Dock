// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Dock.Model.Core;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using JSON serialization.
/// </summary>
public sealed class DockSerializer : IDockSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly LegacyDockJson? _legacy;

    private DockSerializer(JsonSerializerOptions options)
    {
        _options = options;
        if (options.TypeInfoResolver is DockJsonMetadata.MetadataResolver resolver)
        {
            _legacy = new LegacyDockJson(resolver, options);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class with the specified list type.
    /// </summary>
    /// <param name="listType">The type of list to use in the serialization process.</param>
    public DockSerializer(Type listType)
        : this(DockSerializerOptionsFactory.Create(listType))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class with the specified list type and type info resolver.
    /// </summary>
    /// <param name="listType">The type of list to use in the serialization process.</param>
    /// <param name="typeInfoResolver">The resolver to use for generated metadata.</param>
    public DockSerializer(Type listType, IJsonTypeInfoResolver typeInfoResolver)
        : this(DockSerializerOptionsFactory.Create(listType, typeInfoResolver))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class using <see cref="ObservableCollection{T}"/> as the list type and the specified type info resolver.
    /// </summary>
    /// <param name="typeInfoResolver">The resolver to use for generated metadata.</param>
    public DockSerializer(IJsonTypeInfoResolver typeInfoResolver)
        : this(typeof(ObservableCollection<>), typeInfoResolver)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class using <see cref="ObservableCollection{T}"/> as the list type.
    /// </summary>
    public DockSerializer() : this(typeof(ObservableCollection<>))
    {
    }

    /// <summary>Creates the System.Text.Json implementation of the legacy Dock serializer API.</summary>
    /// <param name="listType">The collection type used for dockable lists.</param>
    /// <param name="provider">An optional service provider for constructing serialized objects.</param>
    /// <returns>A serializer backed exclusively by generated metadata.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static DockSerializer CreateCompatible(Type listType, IServiceProvider? provider)
    {
        if (listType is null)
        {
            throw new ArgumentNullException(nameof(listType));
        }
        return new DockSerializer(DockSerializerOptionsFactory.Create(listType, DockJsonMetadata.CreateResolver(listType, provider, legacy: true)));
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        using DockReferenceHandler.Scope operation = ((DockReferenceHandler)_options.ReferenceHandler!).BeginOperation();
        JsonTypeInfo<T> typeInfo = (JsonTypeInfo<T>)_options.GetTypeInfo(typeof(T));
        return JsonSerializer.Serialize(value, typeInfo);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }

        using DockReferenceHandler.Scope operation = ((DockReferenceHandler)_options.ReferenceHandler!).BeginOperation();

        if (_legacy is null)
        {
            return JsonSerializer.Deserialize(text, (JsonTypeInfo<T>)_options.GetTypeInfo(typeof(T)));
        }
        using JsonDocument document = JsonDocument.Parse(text);
        Type rootType = _legacy.GetRootType(document.RootElement, typeof(T));
        byte[] normalized = _legacy.Read(document.RootElement, rootType);
        object? value = JsonSerializer.Deserialize(normalized, _options.GetTypeInfo(rootType));
        return value is null ? default : (T)value;
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var text = streamReader.ReadToEnd();
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
        using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        streamWriter.Write(text);
    }
}
