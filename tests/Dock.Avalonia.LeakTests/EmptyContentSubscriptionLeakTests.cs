using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class EmptyContentSubscriptionLeakTests
{
    [ReleaseFact]
    public void DocumentControl_Subscriptions_Are_Released_On_Swap_Replace_And_Detach()
    {
        var result = RunInSession(() =>
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
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var initialFirst = firstCollection.CollectionChangedSubscriberCount;
            var initialSecond = secondCollection.CollectionChangedSubscriberCount;

            control.DataContext = secondDock;
            DrainDispatcher();

            var afterSwapFirst = firstCollection.CollectionChangedSubscriberCount;
            var afterSwapSecond = secondCollection.CollectionChangedSubscriberCount;

            secondDock.VisibleDockables = replacementCollection;
            DrainDispatcher();

            var afterReplaceSecond = secondCollection.CollectionChangedSubscriberCount;
            var afterReplaceNew = replacementCollection.CollectionChangedSubscriberCount;

            window.Content = null;
            DrainDispatcher();

            var afterDetach = replacementCollection.CollectionChangedSubscriberCount;
            var hasVisibleDockables = control.HasVisibleDockables;

            CleanupWindow(window);

            return new SubscriptionResult(
                initialFirst,
                initialSecond,
                afterSwapFirst,
                afterSwapSecond,
                afterReplaceSecond,
                afterReplaceNew,
                afterDetach,
                hasVisibleDockables);
        });

        Assert.Equal(1, result.InitialFirstSubscribers);
        Assert.Equal(0, result.InitialSecondSubscribers);
        Assert.Equal(0, result.AfterSwapFirstSubscribers);
        Assert.Equal(1, result.AfterSwapSecondSubscribers);
        Assert.Equal(0, result.AfterReplaceOldSubscribers);
        Assert.Equal(1, result.AfterReplaceNewSubscribers);
        Assert.Equal(0, result.AfterDetachSubscribers);
        Assert.False(result.HasVisibleItemsAfterDetach);
    }

    [ReleaseFact]
    public void MdiDocumentControl_Subscriptions_Are_Released_On_Swap_Replace_And_Detach()
    {
        var result = RunInSession(() =>
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
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var initialFirst = firstCollection.CollectionChangedSubscriberCount;
            var initialSecond = secondCollection.CollectionChangedSubscriberCount;

            control.DataContext = secondDock;
            DrainDispatcher();

            var afterSwapFirst = firstCollection.CollectionChangedSubscriberCount;
            var afterSwapSecond = secondCollection.CollectionChangedSubscriberCount;

            secondDock.VisibleDockables = replacementCollection;
            DrainDispatcher();

            var afterReplaceSecond = secondCollection.CollectionChangedSubscriberCount;
            var afterReplaceNew = replacementCollection.CollectionChangedSubscriberCount;

            window.Content = null;
            DrainDispatcher();

            var afterDetach = replacementCollection.CollectionChangedSubscriberCount;
            var hasVisibleDocuments = control.HasVisibleDocuments;

            CleanupWindow(window);

            return new SubscriptionResult(
                initialFirst,
                initialSecond,
                afterSwapFirst,
                afterSwapSecond,
                afterReplaceSecond,
                afterReplaceNew,
                afterDetach,
                hasVisibleDocuments);
        });

        Assert.Equal(1, result.InitialFirstSubscribers);
        Assert.Equal(0, result.InitialSecondSubscribers);
        Assert.Equal(0, result.AfterSwapFirstSubscribers);
        Assert.Equal(1, result.AfterSwapSecondSubscribers);
        Assert.Equal(0, result.AfterReplaceOldSubscribers);
        Assert.Equal(1, result.AfterReplaceNewSubscribers);
        Assert.Equal(0, result.AfterDetachSubscribers);
        Assert.False(result.HasVisibleItemsAfterDetach);
    }

    private static DocumentDock CreateDocumentDock(DocumentLayoutMode layoutMode, TrackingDockableCollection visibleDockables)
    {
        return new DocumentDock
        {
            Factory = new Factory(),
            LayoutMode = layoutMode,
            VisibleDockables = visibleDockables,
            EmptyContent = "No documents are open."
        };
    }

    private sealed record SubscriptionResult(
        int InitialFirstSubscribers,
        int InitialSecondSubscribers,
        int AfterSwapFirstSubscribers,
        int AfterSwapSecondSubscribers,
        int AfterReplaceOldSubscribers,
        int AfterReplaceNewSubscribers,
        int AfterDetachSubscribers,
        bool HasVisibleItemsAfterDetach);

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
