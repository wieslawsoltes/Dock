using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dock.Serializer;

/// <summary>
/// 
/// </summary>
public class ListContractResolver : DefaultContractResolver
{
    private readonly Type _type;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
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
