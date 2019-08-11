namespace AvaloniaDemo.Serializer
{
    public interface IDockJsonSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string text);
        T Load<T>(string path);
        void Save<T>(string path, T value);
    }
}
