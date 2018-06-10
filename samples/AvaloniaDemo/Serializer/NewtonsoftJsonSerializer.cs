// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AvaloniaDemo.Serializer
{
    /// <summary>
    /// Json string serializer.
    /// </summary>
    public sealed class NewtonsoftJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initializes new instance of the <see cref="NewtonsoftJsonSerializer"/> class.
        /// </summary>
        /// <param name="type">The resolved list type.</param>
        public NewtonsoftJsonSerializer(Type type)
        {
            _settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ListContractResolver(type),
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
            return JsonConvert.SerializeObject(value, _settings);
        }

        /// <summary>
        /// Deserialize the json string to object.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="json">The json string.</param>
        /// <returns>The new instance of object of type <typeparamref name="T"/>.</returns>
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}
