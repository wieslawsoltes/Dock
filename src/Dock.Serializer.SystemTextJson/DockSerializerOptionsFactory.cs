// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Dock.Serializer.SystemTextJson;

internal static class DockSerializerOptionsFactory
{
    public static JsonSerializerOptions Create(Type listType)
    {
        if (listType is null)
        {
            throw new ArgumentNullException(nameof(listType));
        }

        return Create(listType, DockJsonMetadata.CreateResolver(listType));
    }

    public static JsonSerializerOptions Create(Type listType, IJsonTypeInfoResolver typeInfoResolver)
    {
        if (listType is null)
        {
            throw new ArgumentNullException(nameof(listType));
        }

        if (typeInfoResolver is null)
        {
            throw new ArgumentNullException(nameof(typeInfoResolver));
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = new DockReferenceHandler(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            TypeInfoResolver = typeInfoResolver is DockJsonMetadata.MetadataResolver
                ? typeInfoResolver
                : typeInfoResolver.WithAddedModifier(typeInfo => DockListTypeInfoModifier.Apply(typeInfo, listType))
        };

        return options;
    }
}
