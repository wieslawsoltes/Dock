using System;
using System.IO;
using System.Text;
using Dock.Model.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dock.Serializer;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using JSON serialization.
/// </summary>
public sealed class DockSerializer : IDockSerializer
{
    private readonly JsonSerializerSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class with the specified list type.
    /// </summary>
    /// <param name="listType">The type of list to use in the serialization process.</param>
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

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value, _settings);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        return JsonConvert.DeserializeObject<T>(text, _settings);
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var text = streamReader.ReadToEnd();
        return Deserialize<T>(text);
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        var text = Serialize(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        streamWriter.Write(text);
    }
}
