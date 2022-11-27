
using System.IO;

namespace Dock.Model.Core;

/// <summary>
/// Docking serializer contract.
/// </summary>
public interface IDockSerializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    string Serialize<T>(T value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Deserialize<T>(string text);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Load<T>(Stream stream);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    void Save<T>(Stream stream, T value);
}
