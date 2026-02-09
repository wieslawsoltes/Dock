using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ControlSubscriptionLeakTests
{
    [AvaloniaFact]
    public void DocumentControl_Subscriptions_Are_Released_On_Switch_Collection_Replace_And_Detach()
    {
        var firstCollection = new TrackingDockableCollection();
        var secondCollection = new TrackingDockableCollection();
        var replacementCollection = new TrackingDockableCollection();
        var firstDock = CreateDocumentDock(DocumentLayoutMode.Tabbed, firstCollection);
        var secondDock = CreateDocumentDock(DocumentLayoutMode.Tabbed, secondCollection);
        var control = new DocumentControl { DataContext = firstDock };
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        try
        {
            Assert.Equal(1, firstCollection.CollectionChangedSubscriberCount);
            Assert.Equal(0, secondCollection.CollectionChangedSubscriberCount);

            control.DataContext = secondDock;
            window.UpdateLayout();
            control.UpdateLayout();

            Assert.Equal(0, firstCollection.CollectionChangedSubscriberCount);
            Assert.Equal(1, secondCollection.CollectionChangedSubscriberCount);

            secondDock.VisibleDockables = replacementCollection;
            window.UpdateLayout();
            control.UpdateLayout();

            Assert.Equal(0, secondCollection.CollectionChangedSubscriberCount);
            Assert.Equal(1, replacementCollection.CollectionChangedSubscriberCount);

            window.Content = null;
            window.UpdateLayout();

            Assert.Equal(0, replacementCollection.CollectionChangedSubscriberCount);
            Assert.False(control.HasVisibleDockables);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MdiDocumentControl_Subscriptions_Are_Released_On_Switch_Collection_Replace_And_Detach()
    {
        var firstCollection = new TrackingDockableCollection();
        var secondCollection = new TrackingDockableCollection();
        var replacementCollection = new TrackingDockableCollection();
        var firstDock = CreateDocumentDock(DocumentLayoutMode.Mdi, firstCollection);
        var secondDock = CreateDocumentDock(DocumentLayoutMode.Mdi, secondCollection);
        var control = new MdiDocumentControl { DataContext = firstDock };
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        try
        {
            Assert.Equal(1, firstCollection.CollectionChangedSubscriberCount);
            Assert.Equal(0, secondCollection.CollectionChangedSubscriberCount);

            control.DataContext = secondDock;
            window.UpdateLayout();
            control.UpdateLayout();

            Assert.Equal(0, firstCollection.CollectionChangedSubscriberCount);
            Assert.Equal(1, secondCollection.CollectionChangedSubscriberCount);

            secondDock.VisibleDockables = replacementCollection;
            window.UpdateLayout();
            control.UpdateLayout();

            Assert.Equal(0, secondCollection.CollectionChangedSubscriberCount);
            Assert.Equal(1, replacementCollection.CollectionChangedSubscriberCount);

            window.Content = null;
            window.UpdateLayout();

            Assert.Equal(0, replacementCollection.CollectionChangedSubscriberCount);
            Assert.False(control.HasVisibleDocuments);
        }
        finally
        {
            window.Close();
        }
    }

    private static DocumentDock CreateDocumentDock(DocumentLayoutMode layoutMode, TrackingDockableCollection visibleDockables)
    {
        var dock = new DocumentDock
        {
            Factory = new Factory(),
            LayoutMode = layoutMode,
            VisibleDockables = visibleDockables,
            EmptyContent = "No documents are open."
        };
        return dock;
    }

    private sealed class TrackingDockableCollection : IList<IDockable>, INotifyCollectionChanged
    {
        private readonly List<IDockable> _items = new();
        private NotifyCollectionChangedEventHandler? _collectionChanged;

        public int CollectionChangedSubscriberCount { get; private set; }

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                _collectionChanged += value;
                CollectionChangedSubscriberCount++;
            }
            remove
            {
                _collectionChanged -= value;
                CollectionChangedSubscriberCount--;
            }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public IDockable this[int index]
        {
            get => _items[index];
            set
            {
                var oldItem = _items[index];
                _items[index] = value;
                RaiseCollectionChanged(value, oldItem, index);
            }
        }

        public void Add(IDockable item)
        {
            _items.Add(item);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item, _items.Count - 1);
        }

        public void Clear()
        {
            _items.Clear();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public bool Contains(IDockable item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(IDockable[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IDockable> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(IDockable item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, IDockable item)
        {
            _items.Insert(index, item);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public bool Remove(IDockable item)
        {
            var index = _items.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            _items.RemoveAt(index);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            return true;
        }

        public void RemoveAt(int index)
        {
            var oldItem = _items[index];
            _items.RemoveAt(index);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedAction action)
        {
            _collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedAction action, IDockable item, int index)
        {
            _collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void RaiseCollectionChanged(IDockable newItem, IDockable oldItem, int index)
        {
            _collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }
    }
}
