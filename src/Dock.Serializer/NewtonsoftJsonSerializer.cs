// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Dock.Serializer
{
    /// <inheritdoc/>
    internal class ListContractResolver : DefaultContractResolver
    {
        /// <inheritdoc/>
        public override JsonContract ResolveContract(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return base
                    .ResolveContract(typeof(ObservableCollection<>)
                    .MakeGenericType(type.GenericTypeArguments[0]));
            }
            return base.ResolveContract(type);
        }

        /// <inheritdoc/>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToList();
        }
    }

    /// <summary>
    /// Json string serializer.
    /// </summary>
    public sealed class NewtonsoftJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings;

        static NewtonsoftJsonSerializer()
        {
            Settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ListContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Converters =
                {
                    new KeyValuePairConverter()
                }
            };
        }

        /// <summary>
        /// Serialize the object value to json string.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="value">The object instance.</param>
        /// <returns>The new instance of object of type <see cref="string"/>.</returns>
        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        /// <summary>
        /// Deserialize the json string to object.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="json">The json string.</param>
        /// <returns>The new instance of object of type <typeparamref name="T"/>.</returns>
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}
