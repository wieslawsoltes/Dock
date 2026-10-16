using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Collections;
using Avalonia.LogicalTree;

namespace AvalonDock.Infrastructure;

// Keep framework traversal on the same virtual enumeration as the public
// AvalonDock hook, while excluding objects that cannot be Avalonia tree nodes.
internal sealed class LogicalChildrenView(Func<IEnumerator?> enumerate) : IAvaloniaReadOnlyList<ILogical>
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Count
    {
        get
        {
            int count = 0;
            using IEnumerator<ILogical> children = GetEnumerator();
            while (children.MoveNext()) count++;
            return count;
        }
    }

    public ILogical this[int index]
    {
        get
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            using IEnumerator<ILogical> children = GetEnumerator();
            while (children.MoveNext())
                if (index-- == 0) return children.Current;
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public IEnumerator<ILogical> GetEnumerator()
    {
        IEnumerator? children = enumerate();
        if (children is null) yield break;
        try
        {
            while (children.MoveNext())
                if (children.Current is ILogical logical) yield return logical;
        }
        finally { (children as IDisposable)?.Dispose(); }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void NotifyChanged()
    {
        PropertyChanged?.Invoke(this, new(nameof(Count)));
        PropertyChanged?.Invoke(this, new("Item[]"));
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
    }
}
