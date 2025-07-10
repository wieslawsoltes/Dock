using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Dock.Model.Core;

namespace Dock.Serializer.Xml;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using XML serialization.
/// </summary>
public sealed class DockXmlSerializer : IDockSerializer
{
    private readonly DataContractSerializerSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockXmlSerializer"/> class with optional known types.
    /// </summary>
    /// <param name="knownTypes">Additional types used during serialization.</param>
    public DockXmlSerializer(params Type[] knownTypes)
    {
        _settings = new DataContractSerializerSettings
        {
            PreserveObjectReferences = true,
            KnownTypes = knownTypes
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockXmlSerializer"/> class.
    /// </summary>
    public DockXmlSerializer() : this(Array.Empty<Type>())
    {
    }

    private DataContractSerializer CreateSerializer(Type type)
    {
        return new DataContractSerializer(type, _settings);
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        var serializer = CreateSerializer(typeof(T));
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
        {
            serializer.WriteObject(writer, value);
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        var serializer = CreateSerializer(typeof(T));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        using var reader = XmlReader.Create(stream);
        return (T?)serializer.ReadObject(reader);
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
