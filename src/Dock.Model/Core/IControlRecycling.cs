namespace Dock.Model.Core;

/// <summary>
/// 
/// </summary>
public interface IControlRecycling
{
    /// <summary>
    /// 
    /// </summary>
    public bool TryToUseIdAsKey { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="control"></param>
    /// <returns></returns>
    bool TryGetValue(object? data, out object? control);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="control"></param>
    void Add(object data, object control);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="existing"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    object? Build(object? data, object? existing, object? parent);

    /// <summary>
    /// 
    /// </summary>
    void Clear();
}
