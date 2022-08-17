using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Dock.Serializer;

public sealed class DockSerializer : IDockSerializer
{
    private readonly JsonSerializerSettings _settings;

    private class ListContractResolver : DefaultContractResolver
    {
        private readonly Type _type;

        public ListContractResolver(Type type)
        {
            _type = type;
        }

        public override JsonContract ResolveContract(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return base.ResolveContract(_type.MakeGenericType(type.GenericTypeArguments[0]));
            }
            return base.ResolveContract(type);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToList();
        }
    }

    public DockSerializer(Type listType)
    {
        _settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ListContractResolver(listType),
            NullValueHandling = NullValueHandling.Ignore,
            Converters =
            {
                new KeyValuePairConverter()
            }
        };
    }

    public string Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value, _settings);
    }

    public T Deserialize<T>(string text)
    {
        return JsonConvert.DeserializeObject<T>(text, _settings)!;
    }

    public T Load<T>(string path)
    {
        using var stream = System.IO.File.OpenRead(path);
        using var streamReader = new System.IO.StreamReader(stream, Encoding.UTF8);
        var text = streamReader.ReadToEnd();
        return Deserialize<T>(text);
    }

    public void Save<T>(string path, T value)
    {
        var text = Serialize(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        using var stream = System.IO.File.Create(path);
        using var streamWriter = new System.IO.StreamWriter(stream, Encoding.UTF8);
        streamWriter.Write(text);
    }
}