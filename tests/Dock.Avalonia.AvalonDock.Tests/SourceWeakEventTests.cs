using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Headless.XUnit;
using Avalonia.Utilities;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SourceWeakEventTests
{
    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Collection_Changes_Pass_Through_The_Override_Before_Updating_Models(bool documents)
    {
        ObservableCollection<object> source = new() { new object() };
        RecordingManager manager = new();
        SetSource(manager, source, documents);
        List<NotifyCollectionChangedEventArgs> notifications = new();
        source.CollectionChanged += (_, e) => notifications.Add(e);

        source.Add(new object());
        source[1] = new object();
        source.RemoveAt(0);
        source.Clear();

        Assert.Equal(4, manager.Calls.Count);
        Assert.Equal(notifications, manager.Calls.Select(call => call.Args));
        Assert.Equal(new[] { 1, 2, 2, 1 }, manager.Calls.Select(call => call.Before));
        Assert.Equal(new[] { 2, 2, 1, 0 }, manager.Calls.Select(call => call.After));
        Assert.All(manager.Calls, call =>
        {
            Assert.Same(source, call.Sender);
            Assert.Equal(typeof(WeakEvent<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>), call.ManagerType);
            Assert.True(call.Handled);
        });
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void An_Override_Can_Handle_A_Change_Without_The_Default_Import(bool documents)
    {
        ObservableCollection<object> source = new();
        RecordingManager manager = new() { Suppress = true };
        SetSource(manager, source, documents);
        source.Add(new object());
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutContent>());

        manager.Suppress = false;
        object imported = new();
        source.Add(imported);
        Assert.Same(imported, Assert.Single(manager.Layout.Descendents().OfType<LayoutContent>()).Content);
        Assert.Equal(2, manager.Calls.Count);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Replacing_The_Source_In_An_Override_Ignores_The_Old_Notification(bool documents)
    {
        ObservableCollection<object> old = new() { new object() };
        ObservableCollection<object> replacement = new() { new object() };
        RecordingManager manager = new();
        SetSource(manager, old, documents);
        manager.BeforeReceive = () =>
        {
            manager.BeforeReceive = null;
            SetSource(manager, replacement, documents);
        };

        old.Add(new object());
        Assert.Same(replacement[0], Assert.Single(manager.Layout.Descendents().OfType<LayoutContent>()).Content);
        old.Add(new object());
        Assert.Single(manager.Calls);
        replacement.Add(new object());
        Assert.Equal(2, manager.Calls.Count);
        Assert.Equal(replacement, manager.Layout.Descendents().OfType<LayoutContent>().Select(content => content.Content));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Shared_Sources_Deliver_Once_And_Keep_The_Remaining_Subscription(bool detachDocuments)
    {
        ObservableCollection<object> source = new() { new object() };
        RecordingManager manager = new() { DocumentsSource = source, AnchorablesSource = source };

        source.Add(new object());
        Assert.Single(manager.Calls);
        Assert.Equal(2, manager.Layout.Descendents().OfType<LayoutDocument>().Count());
        Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());

        SetSource(manager, null, detachDocuments);
        object remaining = new();
        source.Add(remaining);
        Assert.Equal(2, manager.Calls.Count);
        Assert.Contains(manager.Layout.Descendents().OfType<LayoutContent>(), content => ReferenceEquals(content.Content, remaining));
        Assert.All(manager.Layout.Descendents().OfType<LayoutContent>(), content => Assert.Equal(!detachDocuments, content is LayoutDocument));

        SetSource(manager, null, !detachDocuments);
        source.Add(new object());
        Assert.Equal(2, manager.Calls.Count);
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutContent>());
    }

    [AvaloniaFact]
    public void The_Base_Hook_Reports_Recognized_Managers_And_Only_Updates_Current_Sources()
    {
        RecordingManager manager = new();
        ObservableCollection<object> source = new();
        NotifyCollectionChangedEventArgs change = new(NotifyCollectionChangedAction.Add, new object());
        Type collectionManager = typeof(WeakEvent<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>);
        Assert.False(manager.Receive(typeof(object), source, change));
        Assert.True(manager.Receive(collectionManager, source, change));
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutContent>());

        manager.DocumentsSource = source;
        Assert.True(manager.Receive(collectionManager, source, change));
        Assert.Same(change.NewItems![0], Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>()).Content);
        Assert.Throws<InvalidCastException>(() => manager.Receive(collectionManager, source, EventArgs.Empty));
    }

    [AvaloniaFact]
    public void Native_Weak_Subscription_Rejects_An_Unhandled_Source_Notification()
    {
        ObservableCollection<object> source = new();
        RejectingManager manager = new() { DocumentsSource = source };
        Assert.Throws<InvalidOperationException>(() => source.Add(new object()));
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutContent>());
    }

    [AvaloniaFact]
    public void Native_Weak_Subscriber_Ignores_Another_Event_Registration_And_A_Null_Sender()
    {
        RecordingManager manager = new();
        IWeakEventSubscriber<NotifyCollectionChangedEventArgs> subscriber = manager;
        ObservableCollection<object> source = new();
        WeakEvent<INotifyCollectionChanged, NotifyCollectionChangedEventArgs> other = WeakEvent.Register<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>(
            (_, _) => { }, (_, _) => { });
        NotifyCollectionChangedEventArgs change = new(NotifyCollectionChangedAction.Reset);
        subscriber.OnEvent(source, other, change);
        subscriber.OnEvent(null, WeakEvents.CollectionChanged, change);
        Assert.Empty(manager.Calls);
    }

    private static void SetSource(DockingManager manager, ObservableCollection<object>? source, bool documents)
    {
        if (documents) manager.DocumentsSource = source;
        else manager.AnchorablesSource = source;
    }

    private sealed class RecordingManager : DockingManager
    {
        internal List<Call> Calls { get; } = new();
        internal bool Suppress { get; set; }
        internal Action? BeforeReceive { get; set; }
        internal bool Receive(Type managerType, object sender, EventArgs e) => base.OnReceiveWeakEvent(managerType, sender, e);

        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            int before = Layout.Descendents().OfType<LayoutContent>().Count();
            BeforeReceive?.Invoke();
            bool handled = Suppress || base.OnReceiveWeakEvent(managerType, sender, e);
            Calls.Add(new(managerType, sender, e, before, Layout.Descendents().OfType<LayoutContent>().Count(), handled));
            return handled;
        }
    }

    private sealed class RejectingManager : DockingManager
    {
        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e) => false;
    }

    private sealed record Call(Type ManagerType, object Sender, EventArgs Args, int Before, int After, bool Handled);
}
