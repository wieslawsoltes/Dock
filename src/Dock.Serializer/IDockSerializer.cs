
namespace Dock.Serializer;

public interface IDockSerializer
{
    string Serialize<T>(T value);
    T Deserialize<T>(string text);
    T Load<T>(string path);
    void Save<T>(string path, T value);
}