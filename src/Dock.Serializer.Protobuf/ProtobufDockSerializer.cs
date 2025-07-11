using System;
using System.IO;
using Dock.Model.Core;
using ProtoBuf;

namespace Dock.Serializer.Protobuf;

/// <summary>
/// A serializer implementation using protobuf-net.
/// </summary>
public sealed class ProtobufDockSerializer : IDockSerializer
{
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
        return global::ProtoBuf.Serializer.Deserialize<T>(stream);
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        return global::ProtoBuf.Serializer.Deserialize<T>(stream);
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        global::ProtoBuf.Serializer.Serialize(stream, value!);
    }
}
