using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Collections;

namespace Dock.Model.Avalonia.Json;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class JsonConverterAvaloniaList<T> : JsonConverter<IList<T>>
{
    /// <inheritdoc/>
    public override IList<T> Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
        reader.Read();

        var elements = new AvaloniaList<T>();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            var item = JsonSerializer.Deserialize<T>(ref reader, options)!;
            elements.Add(item);

            reader.Read();
        }

        return elements;
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer, IList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (T item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }

        writer.WriteEndArray();
    }
}
