using System;
using System.Text;

namespace AvaloniaDemo.Serializer
{
    public static class ModelSerializer
    {
        public static NewtonsoftJsonSerializer Serializer { get; set; }

        public static string GetBaseDirectory()
        {
            return AppContext.BaseDirectory;
        }

        public static string GetBasePath(string path)
        {
            return System.IO.Path.Combine(GetBaseDirectory(), path);
        }

        public static bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public static string ReadUtf8Text(string path)
        {
            using (var stream = System.IO.File.OpenRead(path))
            using (var sr = new System.IO.StreamReader(stream, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        public static void WriteUtf8Text(string path, string text)
        {
            using (var stream = System.IO.File.Create(path))
            using (var sw = new System.IO.StreamWriter(stream, Encoding.UTF8))
            {
                sw.Write(text);
            }
        }

        public static T Load<T>(string path)
        {
            var json = ReadUtf8Text(path);
            return Serializer.Deserialize<T>(json);
        }

        public static void Save<T>(string path, T value)
        {
            var json = Serializer.Serialize<T>(value);
            WriteUtf8Text(path, json);
        }
    }
}
