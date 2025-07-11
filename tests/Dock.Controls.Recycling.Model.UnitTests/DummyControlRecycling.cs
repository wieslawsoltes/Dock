using System.Collections.Generic;
using Avalonia.Controls.Recycling.Model;

namespace Dock.Controls.Recycling.Model.UnitTests;

internal class DummyControlRecycling : IControlRecycling
{
    public bool TryToUseIdAsKey { get; set; }

    private readonly Dictionary<object, object> _map = new();

    public bool TryGetValue(object? data, out object? control)
    {
        if (data is null)
        {
            control = null;
            return false;
        }
        return _map.TryGetValue(data, out control);
    }

    public void Add(object data, object control)
    {
        _map[data] = control;
    }

    public object? Build(object? data, object? existing, object? parent)
    {
        if (existing != null)
        {
            Add(data!, existing);
            return existing;
        }
        var obj = new object();
        if (data != null)
        {
            Add(data, obj);
        }
        return obj;
    }

    public void Clear()
    {
        _map.Clear();
    }
}
