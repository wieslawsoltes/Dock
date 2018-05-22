// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Text;

namespace Dock.Serializer
{
    /// <summary>
    /// Dock serializer.
    /// </summary>
    public static class DockSerializer
    {
        private static NewtonsoftJsonSerializer Serializer = new NewtonsoftJsonSerializer();

        /// <summary>
        /// Gets the base directory path.
        /// </summary>
        /// <returns>The base directory path.</returns>
        public static string GetBaseDirectory()
        {
            return AppContext.BaseDirectory;
        }

        /// <summary>
        /// Gets the base file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The base file path.</returns>
        public static string GetBasePath(string path)
        {
            return System.IO.Path.Combine(GetBaseDirectory(), path);
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>True if path contains the name of an existing file; otherwise, false.</returns>
        public static bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        /// <summary>
        /// Reads UTF8 encoded text from stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read string.</returns>
        public static string ReadUtf8Text(System.IO.Stream stream)
        {
            using (var sr = new System.IO.StreamReader(stream, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Reads UTF8 encoded text from a file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The read string.</returns>
        public static string ReadUtf8Text(string path)
        {
            using (var fs = System.IO.File.OpenRead(path))
            {
                return ReadUtf8Text(fs);
            }
        }

        /// <summary>
        /// Writes UTF8 encoded text to stream.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="text">The text to write.</param>
        public static void WriteUtf8Text(System.IO.Stream stream, string text)
        {
            using (var sw = new System.IO.StreamWriter(stream, Encoding.UTF8))
            {
                sw.Write(text);
            }
        }

        /// <summary>
        /// Writes UTF8 encoded text to a file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="text">The text to writee.</param>
        public static void WriteUtf8Text(string path, string text)
        {
            using (var fs = System.IO.File.Create(path))
            {
                WriteUtf8Text(fs, text);
            }
        }

        /// <summary>
        /// Loads json string from file and deserializes it to specified object type.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="path">The file path.</param>
        /// <returns>The new instance of object of type <typeparamref name="T"/>.</returns>
        public static T Load<T>(string path)
        {
            var json = ReadUtf8Text(path);
            return Serializer.Deserialize<T>(json);
        }
        
        /// <summary>
        /// Save the object value to json and writes it to file path.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="path">The file path.</param>
        /// <param name="value">The object instance.</param>
        public static void Save<T>(string path, T value)
        {
            var json = Serializer.Serialize<T>(value);
            WriteUtf8Text(path, json);
        }
    }
}
