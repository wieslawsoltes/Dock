// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Dock.Serializer.SystemTextJson;

/// <summary>Uses the composed generated contracts for object properties, roots and collection values.</summary>
internal sealed class DockObjectJsonConverter(DockJsonMetadata.MetadataResolver resolver) : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null: return null;
            case JsonTokenType.String:
                string? text = reader.GetString();
                return text is { Length: >= 19 } && reader.TryGetDateTime(out DateTime date) ? date : text;
            case JsonTokenType.True: return true;
            case JsonTokenType.False: return false;
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out long integer)) return integer;
                using (JsonDocument number = JsonDocument.ParseValue(ref reader))
                {
                    string value = number.RootElement.GetRawText();
                    return HasFraction(value) ? number.RootElement.GetDouble() : BigInteger.Parse(value, CultureInfo.InvariantCulture);
                }
        }

        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) return root.Clone();
        if (root.TryGetProperty("$ref", out JsonElement reference))
            return options.ReferenceHandler!.CreateResolver().ResolveReference(reference.GetString()!);
        if (!root.TryGetProperty("$type", out JsonElement discriminator)) return root.Clone();

        Type concrete = resolver.ResolveName(discriminator.GetString()!)
            ?? throw new JsonException($"Dock JSON type '{discriminator.GetString()}' was not generated.");
        if (concrete == typeof(object)) return root.Clone();
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (JsonProperty property in root.EnumerateObject())
                if (!property.NameEquals("$type")) property.WriteTo(writer);
            writer.WriteEndObject();
        }
        return JsonSerializer.Deserialize(stream.ToArray(), options.GetTypeInfo(concrete));
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case JsonElement element: element.WriteTo(writer); return;
            case JsonDocument document: document.RootElement.WriteTo(writer); return;
            case string scalar: writer.WriteStringValue(scalar); return;
            case bool scalar: writer.WriteBooleanValue(scalar); return;
            case char scalar: writer.WriteStringValue(scalar.ToString()); return;
            case byte scalar: writer.WriteNumberValue(scalar); return;
            case sbyte scalar: writer.WriteNumberValue(scalar); return;
            case short scalar: writer.WriteNumberValue(scalar); return;
            case ushort scalar: writer.WriteNumberValue(scalar); return;
            case int scalar: writer.WriteNumberValue(scalar); return;
            case uint scalar: writer.WriteNumberValue(scalar); return;
            case long scalar: writer.WriteNumberValue(scalar); return;
            case ulong scalar: writer.WriteNumberValue(scalar); return;
            case decimal scalar: WriteFloatingPoint(writer, scalar.ToString(CultureInfo.InvariantCulture)); return;
            case float scalar:
                if (float.IsFinite(scalar)) WriteFloatingPoint(writer, scalar.ToString("R", CultureInfo.InvariantCulture));
                else writer.WriteStringValue(scalar.ToString(CultureInfo.InvariantCulture));
                return;
            case double scalar:
                if (double.IsFinite(scalar)) WriteFloatingPoint(writer, scalar.ToString("R", CultureInfo.InvariantCulture));
                else writer.WriteStringValue(scalar.ToString(CultureInfo.InvariantCulture));
                return;
            case DateTime scalar: writer.WriteStringValue(scalar); return;
            case DateTimeOffset scalar: writer.WriteStringValue(scalar); return;
            case Guid scalar: writer.WriteStringValue(scalar); return;
            case TimeSpan scalar: writer.WriteStringValue(scalar.ToString("c", CultureInfo.InvariantCulture)); return;
            case Uri scalar: writer.WriteStringValue(scalar.OriginalString); return;
            case BigInteger scalar: writer.WriteRawValue(scalar.ToString(CultureInfo.InvariantCulture)); return;
            case byte[] scalar: writer.WriteBase64StringValue(scalar); return;
        }

        Type runtimeType = value.GetType();
        if (runtimeType == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }
        JsonTypeInfo info = options.GetTypeInfo(runtimeType);
        JsonElement serialized = JsonSerializer.SerializeToElement(value, info);
        if (serialized.ValueKind != JsonValueKind.Object || serialized.TryGetProperty("$ref", out _))
        {
            serialized.WriteTo(writer);
            return;
        }
        if (resolver.GetContract(runtimeType) is null)
            throw new NotSupportedException($"Dock JSON type '{runtimeType}' was not generated.");
        writer.WriteStartObject();
        writer.WriteString("$type", runtimeType.FullName ?? runtimeType.Name);
        foreach (JsonProperty property in serialized.EnumerateObject()) property.WriteTo(writer);
        writer.WriteEndObject();
    }

    private static bool HasFraction(string text) => text.IndexOf('.') >= 0 || text.IndexOf('e') >= 0 || text.IndexOf('E') >= 0;

    private static void WriteFloatingPoint(Utf8JsonWriter writer, string text) => writer.WriteRawValue(HasFraction(text) ? text : text + ".0");
}
