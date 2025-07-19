// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Dock.Model.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dock.Serializer;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using JSON serialization.
/// </summary>
public sealed class DockSerializer : IDockSerializer
{
    private readonly JsonSerializerSettings _settings;

    private static JsonSerializerSettings CreateSettings(Type listType, IServiceProvider? provider)
    {
        var resolver = provider is null
            ? new ListContractResolver(listType)
            : new ServiceProviderContractResolver(listType, provider);

        return new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = resolver,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new KeyValuePairConverter() }
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class with the specified list type.
    /// </summary>
    /// <param name="listType">The type of list to use in the serialization process.</param>
    public DockSerializer(Type listType)
        : this(listType, provider: null)
    {
    }

    public DockSerializer(Type listType, IServiceProvider? provider)
    {
        _settings = CreateSettings(listType, provider);
    }

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
        return JsonConvert.SerializeObject(value, _settings);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        return JsonConvert.DeserializeObject<T>(text, _settings);
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

    /// <inheritdoc/>
    public async Task<T?> LoadAsync<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var text = await streamReader.ReadToEndAsync().ConfigureAwait(false);
        return Deserialize<T>(text);
    }

    /// <inheritdoc/>
    public async Task SaveAsync<T>(Stream stream, T value)
    {
        var text = Serialize(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        await streamWriter.WriteAsync(text).ConfigureAwait(false);
    }
}
