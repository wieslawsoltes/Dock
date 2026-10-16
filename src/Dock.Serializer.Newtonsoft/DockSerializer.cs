// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.ObjectModel;
using System.IO;
using Dock.Model.Core;

namespace Dock.Serializer;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using JSON serialization.
/// </summary>
public sealed class DockSerializer : IDockSerializer
{
    private readonly IDockSerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class with the specified list type.
    /// </summary>
    /// <param name="listType">The type of list to use in the serialization process.</param>
    public DockSerializer(Type listType)
        : this(listType, provider: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class with the specified list type and service provider.
    /// </summary>
    /// <param name="listType">The type of list to use in the serialization process.</param>
    /// <param name="provider">The service provider.</param>
    public DockSerializer(Type listType, IServiceProvider? provider)
    {
        _serializer = SystemTextJson.DockSerializer.CreateCompatible(listType, provider);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class using <see cref="ObservableCollection{T}"/> as the list type and a service provider.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    public DockSerializer(IServiceProvider provider)
        : this(typeof(ObservableCollection<>), provider)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class using <see cref="ObservableCollection{T}"/> as the list type.
    /// </summary>
    public DockSerializer() : this(typeof(ObservableCollection<>), provider: null)
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
        return _serializer.Deserialize<T>(text);
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        return _serializer.Load<T>(stream);
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        _serializer.Save(stream, value);
    }
}
