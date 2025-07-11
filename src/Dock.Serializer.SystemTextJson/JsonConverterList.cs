// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// JSON converter for <see cref="IList{T}"/> using a custom list type.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class JsonConverterList<T> : JsonConverter<IList<T>>
{
    private readonly Type _listType;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConverterList{T}"/> class.
    /// </summary>
    /// <param name="listType">The generic list type to instantiate.</param>
    public JsonConverterList(Type listType)
    {
        _listType = listType;
    }

    /// <inheritdoc/>
    public override IList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
        reader.Read();

        var list = (IList<T>)Activator.CreateInstance(_listType.MakeGenericType(typeof(T)))!;

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            var item = JsonSerializer.Deserialize<T>(ref reader, options)!;
            list.Add(item);
            reader.Read();
        }

        return list;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }
        writer.WriteEndArray();
    }
}
