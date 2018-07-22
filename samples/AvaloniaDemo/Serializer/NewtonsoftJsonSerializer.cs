using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AvaloniaDemo.Serializer
{
    public sealed class NewtonsoftJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public NewtonsoftJsonSerializer(Type listType)
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
    }
}
