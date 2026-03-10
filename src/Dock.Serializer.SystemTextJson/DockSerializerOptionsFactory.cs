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

        return Create(listType, new DockModelPolymorphicTypeResolver());
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
            ReferenceHandler = ReferenceHandler.Preserve,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            TypeInfoResolver = typeInfoResolver
        };

        options.Converters.Add(new JsonConverterFactoryList(listType));
        return options;
    }
}
