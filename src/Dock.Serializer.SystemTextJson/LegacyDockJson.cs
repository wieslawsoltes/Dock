// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Dock.Serializer.SystemTextJson;

/// <summary>Translates legacy object type metadata and plain arrays using generated contracts.</summary>
internal sealed class LegacyDockJson
{
    private readonly DockJsonMetadata.MetadataResolver _resolver;
    private readonly JsonSerializerOptions _options;

    internal LegacyDockJson(DockJsonMetadata.MetadataResolver resolver, JsonSerializerOptions options)
    {
        _resolver = resolver;
        _options = options;
    }

    internal byte[] Read(JsonElement element, Type type)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            ReadElement(writer, element, type);
        }
        return stream.ToArray();
    }

    internal Type GetRootType(JsonElement element, Type declaredType) => element.ValueKind == JsonValueKind.Object
        ? ResolveConcreteType(element, declaredType)
        : declaredType;

    private void ReadElement(Utf8JsonWriter writer, JsonElement element, Type type)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            Type elementType = _resolver.GetContract(type)?.ElementType
                ?? throw new NotSupportedException($"No generated element contract exists for '{type}'.");
            writer.WriteStartArray();
            foreach (JsonElement item in element.EnumerateArray())
            {
                ReadElement(writer, item, elementType);
            }
            writer.WriteEndArray();
            return;
        }
        if (element.ValueKind != JsonValueKind.Object || element.TryGetProperty("$ref", out _))
        {
            element.WriteTo(writer);
            return;
        }

        Type concreteType = ResolveConcreteType(element, type);
        JsonTypeInfo declaredInfo = _options.GetTypeInfo(type);
        JsonTypeInfo info = _options.GetTypeInfo(concreteType);
        writer.WriteStartObject();
        if (element.TryGetProperty("$id", out JsonElement id))
        {
            writer.WriteString("$id", id.GetString());
        }
        if (element.TryGetProperty("$type", out _) && concreteType != type
            && (declaredInfo.PolymorphismOptions is not null || type == typeof(object)))
        {
            writer.WriteString("$type", concreteType.FullName ?? concreteType.Name);
        }
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (property.NameEquals("$id") || property.NameEquals("$type"))
            {
                continue;
            }
            JsonPropertyInfo? member = GetProperty(info, property.Name);
            if (info.Kind == JsonTypeInfoKind.Object && member is null)
            {
                // Newtonsoft skips unknown members without inspecting their values.
                // A removed property's array or obsolete $type is not part of the
                // current contract, so it must not require generated metadata.
                foreach (JsonPropertyInfo candidate in info.Properties)
                {
                    if (candidate.IsExtensionData)
                    {
                        property.WriteTo(writer);
                        break;
                    }
                }
                continue;
            }
            writer.WritePropertyName(property.Name);
            if (info.Kind == JsonTypeInfoKind.Enumerable && property.NameEquals("$values"))
            {
                ReadElement(writer, property.Value, concreteType);
            }
            else
            {
                ReadElement(writer, property.Value, info.Kind == JsonTypeInfoKind.Dictionary
                    ? _resolver.GetContract(concreteType)?.DictionaryValueType ?? typeof(object)
                    : member?.PropertyType ?? typeof(object));
            }
        }
        writer.WriteEndObject();
    }

    private Type ResolveConcreteType(JsonElement element, Type declaredType)
    {
        if (!element.TryGetProperty("$type", out JsonElement discriminator))
        {
            return declaredType;
        }
        string name = discriminator.GetString() ?? throw new JsonException("The Dock type discriminator must be a string.");
        Type concreteType = _resolver.ResolveName(name) ?? throw new JsonException($"Dock JSON type '{name}' was not generated.");
        if (_resolver.GetContract(concreteType)?.CanAssignTo(declaredType) != true)
        {
            throw new JsonException($"Dock JSON type '{name}' is not compatible with '{declaredType}'.");
        }
        return concreteType;
    }

    private static JsonPropertyInfo? GetProperty(JsonTypeInfo info, string name)
    {
        foreach (JsonPropertyInfo property in info.Properties)
        {
            if (property.Name == name)
            {
                return property;
            }
        }
        return null;
    }

    internal static string NormalizeTypeName(string name)
    {
        var result = new StringBuilder(name.Length);
        for (int i = 0; i < name.Length; i++)
        {
            if (name[i] == ',' && (name.AsSpan(i).StartsWith(", Version=", StringComparison.Ordinal)
                || name.AsSpan(i).StartsWith(", Culture=", StringComparison.Ordinal)
                || name.AsSpan(i).StartsWith(", PublicKeyToken=", StringComparison.Ordinal)))
            {
                i++;
                while (i < name.Length && name[i] != ',' && name[i] != ']')
                {
                    i++;
                }
                i--;
                continue;
            }
            result.Append(name[i]);
        }
        return result.ToString();
    }
}
