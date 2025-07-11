// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// JSON converter factory for <see cref="IList{T}"/> using a custom list type.
/// </summary>
public class JsonConverterFactoryList : JsonConverterFactory
{
    private readonly Type _listType;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConverterFactoryList"/> class.
    /// </summary>
    /// <param name="listType">The generic list type to instantiate.</param>
    public JsonConverterFactoryList(Type listType)
    {
        _listType = listType;
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IList<>);

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IList<>));

        var elementType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(JsonConverterList<>).MakeGenericType(elementType);
        return (JsonConverter)Activator.CreateInstance(
            converterType,
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: new object[] { _listType },
            culture: null)!;
    }
}
