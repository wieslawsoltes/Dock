using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AvaloniaDemo.Serializer
{
    public sealed class DockJsonSerializer
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

        public DockJsonSerializer(Type listType)
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

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        public string GetBaseDirectory()
        {
            return AppContext.BaseDirectory;
        }

        public string GetBasePath(string path)
        {
            return System.IO.Path.Combine(GetBaseDirectory(), path);
        }

        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public string ReadText(string path)
        {
            using (var stream = System.IO.File.OpenRead(path))
            using (var sr = new System.IO.StreamReader(stream, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        public void WriteText(string path, string text)
        {
            using (var stream = System.IO.File.Create(path))
            using (var sw = new System.IO.StreamWriter(stream, Encoding.UTF8))
            {
                sw.Write(text);
            }
        }

        public T Load<T>(string path)
        {
            var json = ReadText(path);
            return Deserialize<T>(json);
        }

        public void Save<T>(string path, T value)
        {
            var json = Serialize<T>(value);
            WriteText(path, json);
        }
    }
}
