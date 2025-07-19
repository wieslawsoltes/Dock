// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.IO;
using System.Threading.Tasks;

namespace Dock.Model.Core;
/// <summary>
/// Docking serializer contract.
/// </summary>
public interface IDockSerializer
{
    /// <summary>
    /// Serializes the specified object to a string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <returns>A string representation of the serialized object.</returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes the specified string to an object.
    /// </summary>
    /// <param name="text">The string to deserialize.</param>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <returns>The deserialized object, or null if the deserialization fails.</returns>
    T? Deserialize<T>(string text);

    /// <summary>
    /// Loads an object from the specified stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <typeparam name="T">The type of the object to load.</typeparam>
    /// <returns>The loaded object, or null if the deserialization fails.</returns>
    T? Load<T>(Stream stream);

    /// <summary>
    /// Saves the specified object to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="value">The object to save.</param>
    /// <typeparam name="T">The type of the object to save.</typeparam>
    void Save<T>(Stream stream, T value);

    /// <summary>
    /// Asynchronously loads an object from the specified stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <typeparam name="T">The type of the object to load.</typeparam>
    /// <returns>A task returning the loaded object, or null if the deserialization fails.</returns>
    Task<T?> LoadAsync<T>(Stream stream);

    /// <summary>
    /// Asynchronously saves the specified object to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="value">The object to save.</param>
    /// <typeparam name="T">The type of the object to save.</typeparam>
    Task SaveAsync<T>(Stream stream, T value);
}
