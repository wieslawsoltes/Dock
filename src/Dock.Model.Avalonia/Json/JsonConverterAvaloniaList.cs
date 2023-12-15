/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
