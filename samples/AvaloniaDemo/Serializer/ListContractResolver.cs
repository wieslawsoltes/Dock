// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AvaloniaDemo.Serializer
{
    /// <inheritdoc/>
    public class ListContractResolver : DefaultContractResolver
    {
        private readonly Type _type;

        /// <summary>
        /// Initializes new instance of the <see cref="ListContractResolver"/> class.
        /// </summary>
        /// <param name="type">The resolved list type.</param>
        public ListContractResolver(Type type)
        {
            _type = type;
        }

        /// <inheritdoc/>
        public override JsonContract ResolveContract(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return base.ResolveContract(_type.MakeGenericType(type.GenericTypeArguments[0]));
            }
            return base.ResolveContract(type);
        }

        /// <inheritdoc/>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToList();
        }
    }
}
