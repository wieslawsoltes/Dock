using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Collections;

namespace Dock.Model.Avalonia.Json;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class AvaloniaListJsonConverter<T> : JsonConverter<AvaloniaList<T>>
{
    /// <inheritdoc/>
    public override AvaloniaList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        var list = new AvaloniaList<T>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            var element = JsonSerializer.Deserialize<T>(ref reader, options);
            if (element is { })
            {
                list.Add(element);
            }
        }

        return list;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, AvaloniaList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var element in value)
        {
            JsonSerializer.Serialize(writer, element, options);
        }
        writer.WriteEndArray();
    }
}
