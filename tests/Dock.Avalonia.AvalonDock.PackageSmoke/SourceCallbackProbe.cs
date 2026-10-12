using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using AvalonDock;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class SourceCallbackProbe
{
    internal static void Verify()
    {
        VerifyRedirect(true);
        VerifyRedirect(false);
        VerifyRepeatedAttachments(true);
        VerifyRepeatedAttachments(false);
        ObservableCollection<object> shared = new() { new object() };
        SourceManager manager = new() { DocumentsSource = shared, AnchorablesSource = shared };
        shared.Add(new object());
        if (manager.Calls != 2 || manager.Layout.Descendents().OfType<LayoutDocument>().Count() != 3 ||
            manager.Layout.Descendents().OfType<LayoutAnchorable>().Count() != 1)
            throw new Exception("Packaged shared sources did not preserve per-attachment delivery and document precedence.");
        manager.DocumentsSource = null;
        shared.Add(new object());
        if (manager.Calls != 3 || manager.Layout.Descendents().OfType<LayoutAnchorable>().Count() != 2)
            throw new Exception("Detaching a packaged shared source removed the remaining registration.");
        manager.AnchorablesSource = null;
    }

    private static void VerifyRedirect(bool documents)
    {
        SourceManager manager = new();
        ObservableCollection<object> requested = new() { new object() };
        ObservableCollection<object> redirected = new() { new object() };
        manager.BeforeSource = e =>
        {
            if (ReferenceEquals(e.NewValue, requested)) SetSource(manager, redirected, documents);
        };
        SetSource(manager, requested, documents);
        if (!manager.Layout.Descendents().OfType<LayoutContent>().Select(content => content.Content)
            .SequenceEqual(new[] { redirected[0], requested[0] }))
            throw new Exception("Packaged source callbacks ignored their arguments after reentrant replacement.");
        requested.Add(new object());
        redirected.Add(new object());
        if (manager.Calls != 2 || manager.Layout.Descendents().OfType<LayoutContent>().Count() != 3)
            throw new Exception("Packaged reentrant attachments lost their listener or updated a noncurrent source.");
        manager.BeforeSource = null;
        SetSource(manager, null, documents);
    }

    private static void VerifyRepeatedAttachments(bool documents)
    {
        SourceManager manager = new();
        ObservableCollection<object> source = new();
        SetSource(manager, source, documents);
        AvaloniaPropertyChangedEventArgs captured = manager.LastChange!;
        for (int index = 0; index < 10; index++) manager.Apply(captured, documents);
        source.Add(new object());
        if (manager.Calls != 11 || manager.Layout.Descendents().OfType<LayoutContent>().Count() != 11)
            throw new Exception("Packaged repeated source attachments were coalesced.");
        SetSource(manager, null, documents);
        source.Add(new object());
        if (manager.Calls != 21 || manager.Layout.Descendents().OfType<LayoutContent>().Any())
            throw new Exception("Packaged source detachment did not remove exactly one registration.");
    }

    private static void SetSource(DockingManager manager, IEnumerable? source, bool documents)
    {
        if (documents) manager.DocumentsSource = source;
        else manager.AnchorablesSource = source;
    }

    private sealed class SourceManager : DockingManager
    {
        internal int Calls { get; private set; }
        internal Action<AvaloniaPropertyChangedEventArgs>? BeforeSource { get; set; }
        internal AvaloniaPropertyChangedEventArgs? LastChange { get; private set; }

        internal void Apply(AvaloniaPropertyChangedEventArgs e, bool documents)
        {
            if (documents) base.OnDocumentsSourceChanged(e);
            else base.OnAnchorablesSourceChanged(e);
        }

        protected override void OnDocumentsSourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            LastChange = e;
            BeforeSource?.Invoke(e);
            base.OnDocumentsSourceChanged(e);
        }

        protected override void OnAnchorablesSourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            LastChange = e;
            BeforeSource?.Invoke(e);
            base.OnAnchorablesSourceChanged(e);
        }

        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            Calls++;
            return base.OnReceiveWeakEvent(managerType, sender, e);
        }
    }
}
