// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dock.Model.Avalonia.Json;

/// <summary>
/// 
/// </summary>
public class JsonConverterFactoryAvaloniaList : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType
           && typeToConvert.GetGenericTypeDefinition() == typeof(IList<>);

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(
        Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert.IsGenericType &&
                     typeToConvert.GetGenericTypeDefinition() == typeof(IList<>));

        Type elementType = typeToConvert.GetGenericArguments()[0];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(JsonConverterAvaloniaList<>)
                .MakeGenericType(new Type[] { elementType }),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: null,
            culture: null)!;

        return converter;
    }
}
