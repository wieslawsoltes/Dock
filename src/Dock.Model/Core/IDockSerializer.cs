/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System.IO;

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
}
