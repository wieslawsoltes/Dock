using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Utilities;
using AvalonDock;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class SourceCollectionProbe
{
    internal static void Verify()
    {
        ObservableCollection<object> source = new();
        SourceManager manager = new() { DocumentsSource = source, Suppress = true };
        source.Add("handled by application");
        if (manager.Calls != 1 || manager.Layout.Descendents().OfType<LayoutDocument>().Any())
            throw new Exception("The packaged source hook did not intercept the live collection notification.");
        manager.Suppress = false;
        source.Add("imported");
        if (manager.Calls != 2 || manager.Layout.Descendents().OfType<LayoutDocument>().Single().Content as string != "imported")
            throw new Exception("The packaged source hook did not forward to the base collection handler.");
        manager.DocumentsSource = null;
        source.Add("detached");
        if (manager.Calls != 2)
            throw new Exception("The packaged manager retained a replaced source subscription.");
    }

    private sealed class SourceManager : DockingManager
    {
        internal int Calls { get; private set; }
        internal bool Suppress { get; set; }

        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(WeakEvent<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>) ||
                !ReferenceEquals(sender, DocumentsSource) || e is not NotifyCollectionChangedEventArgs)
                throw new Exception("The packaged source hook received an unexpected framework mapping.");
            Calls++;
            return Suppress || base.OnReceiveWeakEvent(managerType, sender, e);
        }
    }
}
