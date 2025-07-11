using System.IO;
using System.Text;
using Dock.Model.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Dock.Serializer.Yaml;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using YAML serialization.
/// </summary>
public sealed class DockYamlSerializer : IDockSerializer
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockYamlSerializer"/> class.
    /// </summary>
    public DockYamlSerializer()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        return _serializer.Serialize(value);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        return _deserializer.Deserialize<T>(text);
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = reader.ReadToEnd();
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
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(text);
    }
}
