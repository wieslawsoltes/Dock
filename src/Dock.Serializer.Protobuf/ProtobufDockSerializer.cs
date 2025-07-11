using System;
using System.Collections.ObjectModel;
using System.IO;
using Dock.Model.Core;
using ProtoBuf;

namespace Dock.Serializer.Protobuf;

/// <summary>
/// A serializer implementation using protobuf-net.
/// </summary>
public sealed class ProtobufDockSerializer : IDockSerializer
{
    private readonly Type _listType;
    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufDockSerializer"/> class with the specified list type.
    /// </summary>
    /// <param name="listType">The generic list type to instantiate.</param>
    public ProtobufDockSerializer(Type listType)
    {
        _listType = listType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufDockSerializer"/> class.
    /// </summary>
    public ProtobufDockSerializer() : this(typeof(ObservableCollection<>))
    {
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        global::ProtoBuf.Serializer.Serialize(stream, value!);
        return Convert.ToBase64String(stream.ToArray());
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        var bytes = Convert.FromBase64String(text);
        using var stream = new MemoryStream(bytes);
        var result = global::ProtoBuf.Serializer.Deserialize<T>(stream);
        ListTypeConverter.Convert(result, _listType);
        return result;
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        var result = global::ProtoBuf.Serializer.Deserialize<T>(stream);
        ListTypeConverter.Convert(result, _listType);
        return result;
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        global::ProtoBuf.Serializer.Serialize(stream, value!);
    }
}
