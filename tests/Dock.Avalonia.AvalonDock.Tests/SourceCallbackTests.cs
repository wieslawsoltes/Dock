using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SourceCallbackTests
{
    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Redirecting_A_Source_Preserves_The_Base_Call_Order(bool documents, bool beforeBase)
    {
        SourceManager manager = new();
        ObservableCollection<object> original = new() { new object() };
        ObservableCollection<object> requested = new() { new object() };
        ObservableCollection<object> redirected = new() { new object() };
        SetSource(manager, original, documents);
        void Redirect(AvaloniaPropertyChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewValue, requested)) SetSource(manager, redirected, documents);
        }
        if (beforeBase) manager.BeforeSource = Redirect;
        else manager.AfterSource = Redirect;

        SetSource(manager, requested, documents);
        Assert.Same(redirected, GetSource(manager, documents));
        Assert.Equal(beforeBase ? new[] { redirected[0], requested[0] } : new[] { redirected[0] }, Values(manager.Layout));
        original.Add(new object());
        Assert.Empty(manager.Senders);
        requested.Add(new object());
        Assert.Equal(beforeBase ? 1 : 0, manager.Senders.Count);
        Assert.DoesNotContain(requested[1], Values(manager.Layout));
        redirected.Add(new object());
        Assert.Same(redirected, manager.Senders.Last());
        Assert.Contains(redirected[1], Values(manager.Layout));
    }

    [AvaloniaTheory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void A_Skipped_Base_Call_Does_Not_Become_The_Next_Callbacks_Old_Source(bool documents, bool fail)
    {
        SourceManager manager = new();
        ObservableCollection<object> original = new() { new object() };
        ObservableCollection<object> skipped = new() { new object() };
        ObservableCollection<object> next = new() { new object() };
        SetSource(manager, original, documents);
        manager.Suppress = !fail;
        manager.Fail = fail;
        if (fail) Assert.Throws<InvalidOperationException>(() => SetSource(manager, skipped, documents));
        else SetSource(manager, skipped, documents);
        Assert.Same(skipped, GetSource(manager, documents));
        manager.Suppress = manager.Fail = false;

        SetSource(manager, next, documents);
        Assert.Equal(new[] { original[0], next[0] }, Values(manager.Layout));
        original.Add(new object());
        Assert.Same(original, Assert.Single(manager.Senders));
        Assert.DoesNotContain(original[1], Values(manager.Layout));
        skipped.Add(new object());
        Assert.Single(manager.Senders);
        next.Add(new object());
        Assert.Contains(next[1], Values(manager.Layout));
    }

    [AvaloniaTheory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void Replayed_Arguments_Detach_Their_Old_Value_And_Import_Their_New_Value(bool documents, bool clear)
    {
        SourceManager manager = new();
        ObservableCollection<object> original = new() { new object() };
        ObservableCollection<object> requested = new() { new object() };
        ObservableCollection<object> current = new() { new object() };
        SetSource(manager, original, documents);
        manager.Suppress = true;
        SetSource(manager, clear ? null : requested, documents);
        AvaloniaPropertyChangedEventArgs captured = manager.LastChange!;
        SetSource(manager, current, documents);
        manager.Suppress = false;

        manager.Apply(captured, documents);
        Assert.Same(current, GetSource(manager, documents));
        Assert.Equal(clear ? Array.Empty<object>() : new[] { requested[0] }, Values(manager.Layout));
        original.Add(new object());
        current.Add(new object());
        Assert.Empty(manager.Senders);
        requested.Add(new object());
        Assert.Equal(clear ? 0 : 1, manager.Senders.Count);
        Assert.DoesNotContain(requested[1], Values(manager.Layout));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Layout_Replacement_Uses_The_Current_Source_After_A_Suppressed_Source_Callback(bool documents)
    {
        SourceManager manager = new();
        ObservableCollection<object> original = new() { new object() };
        ObservableCollection<object> current = new() { new object() };
        SetSource(manager, original, documents);
        LayoutRoot oldLayout = manager.Layout;
        manager.Suppress = true;
        SetSource(manager, current, documents);
        manager.Suppress = false;

        manager.Layout = NewRoot();
        Assert.Equal(new[] { original[0] }, Values(oldLayout));
        Assert.Null(oldLayout.Manager);
        Assert.Equal(new[] { current[0] }, Values(manager.Layout));
        original.Add(new object());
        Assert.Same(original, Assert.Single(manager.Senders));
        current.Add(new object());
        Assert.Equal(current, Values(manager.Layout));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Layout_Replacement_Unsubscribes_Old_Root_Notifications_Before_Detaching_Its_Source(bool documents)
    {
        SourceManager manager = new();
        ObservableCollection<object> source = new() { new object() };
        SetSource(manager, source, documents);
        LayoutRoot oldLayout = manager.Layout;
        manager.ActiveContent = source[0];
        List<object?> valuesDuringRemoval = new();
        oldLayout.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LayoutRoot.ActiveContent) && oldLayout.ActiveContent is null)
                valuesDuringRemoval.Add(manager.ActiveContent);
        };

        manager.Layout = NewRoot();
        Assert.Same(source[0], Assert.Single(valuesDuringRemoval));
        Assert.Empty(Values(oldLayout));
        Assert.Equal(source, Values(manager.Layout));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Live_Notifications_Use_The_Current_Layout_Even_When_Its_Transition_Was_Suppressed(bool documents)
    {
        SourceManager manager = new();
        ObservableCollection<object> source = new() { new object() };
        SetSource(manager, source, documents);
        LayoutRoot oldLayout = manager.Layout;
        manager.SuppressLayout = true;
        manager.Layout = NewRoot();

        source.Add(new object());
        Assert.Equal(new[] { source[0] }, Values(oldLayout));
        Assert.Equal(new[] { source[1] }, Values(manager.Layout));
        Assert.Same(manager, oldLayout.Manager);
        Assert.Null(manager.Layout.Manager);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_Failed_Initial_Import_Does_Not_Subscribe_Until_A_Successful_Attachment(bool documents)
    {
        Strategy strategy = new() { After = _ => throw new InvalidOperationException("Import failed") };
        SourceManager manager = new() { LayoutUpdateStrategy = strategy };
        ObservableCollection<object> source = new() { new object() };
        Assert.Throws<InvalidOperationException>(() => SetSource(manager, source, documents));
        AvaloniaPropertyChangedEventArgs captured = manager.LastChange!;
        strategy.After = null;
        source.Add(new object());
        Assert.Empty(manager.Senders);
        Assert.Equal(new[] { source[0] }, Values(manager.Layout));

        manager.Apply(captured, documents);
        Assert.Equal(source, Values(manager.Layout));
        source.Add(new object());
        Assert.Same(source, Assert.Single(manager.Senders));
        Assert.Equal(source, Values(manager.Layout));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void An_Initial_Batch_Attaches_Its_Supplied_Source_After_A_Strategy_Replaces_It(bool documents)
    {
        SourceManager manager = new();
        object manual = new();
        if (documents) manager.Layout.Descendents().OfType<LayoutDocumentPane>().First().Children.Add(new LayoutDocument { Content = manual });
        else manager.Layout.RootPanel.Children.Add(new LayoutAnchorablePane(new LayoutAnchorable { Content = manual }));
        ObservableCollection<object> requested = new() { new object(), new object() };
        ObservableCollection<object> redirected = new() { new object() };
        Strategy strategy = new();
        strategy.After = _ =>
        {
            strategy.After = null;
            SetSource(manager, redirected, documents);
        };
        manager.LayoutUpdateStrategy = strategy;

        SetSource(manager, requested, documents);
        Assert.Same(redirected, GetSource(manager, documents));
        Assert.Contains(redirected[0], Values(manager.Layout));
        Assert.DoesNotContain(requested[0], Values(manager.Layout));
        Assert.Contains(requested[1], Values(manager.Layout));
        requested.Add(new object());
        Assert.Same(requested, Assert.Single(manager.Senders));
        Assert.DoesNotContain(requested[2], Values(manager.Layout));
        redirected.Add(new object());
        Assert.Contains(redirected[1], Values(manager.Layout));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Repeated_Attachments_Deliver_Independently_And_Detach_One_Registration_At_A_Time(bool documents)
    {
        SourceManager manager = new();
        ObservableCollection<object> source = new() { new object() };
        SetSource(manager, source, documents);
        AvaloniaPropertyChangedEventArgs captured = manager.LastChange!;
        for (int index = 0; index < 10; index++) manager.Apply(captured, documents);
        Assert.Single(Values(manager.Layout));

        source.Add(new object());
        Assert.Equal(11, manager.Senders.Count);
        Assert.Equal(11, Values(manager.Layout).Count(value => ReferenceEquals(value, source[1])));
        SetSource(manager, null, documents);
        manager.Senders.Clear();
        source.Add(new object());
        Assert.Equal(10, manager.Senders.Count);
        Assert.Empty(Values(manager.Layout));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_Manager_Does_Not_Retain_An_Obsolete_But_Still_Subscribed_Collection(bool documents)
    {
        SourceManager manager = new();
        WeakReference reference = AttachObsoleteSource(manager, documents);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(reference.IsAlive);
        GC.KeepAlive(manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachRepeatedManager(ObservableCollection<object> source, bool documents)
    {
        SourceManager manager = new();
        SetSource(manager, source, documents);
        AvaloniaPropertyChangedEventArgs captured = manager.LastChange!;
        for (int index = 0; index < 10; index++) manager.Apply(captured, documents);
        return new(manager);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Repeated_Registrations_Do_Not_Retain_An_Unused_Manager(bool documents)
    {
        ObservableCollection<object> source = new() { new object() };
        WeakReference reference = AttachRepeatedManager(source, documents);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(reference.IsAlive);
        source.Add(new object());
        GC.KeepAlive(source);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachObsoleteSource(SourceManager manager, bool documents)
    {
        ObservableCollection<object> source = new();
        SetSource(manager, source, documents);
        manager.Suppress = true;
        SetSource(manager, null, documents);
        manager.LastChange = null;
        return new(source);
    }

    private static object?[] Values(LayoutRoot layout)
        => layout.Descendents().OfType<LayoutContent>().Select(content => content.Content).ToArray();

    private static LayoutRoot NewRoot() => new() { RootPanel = new LayoutPanel(new LayoutDocumentPane()) };

    private static IEnumerable? GetSource(DockingManager manager, bool documents)
        => documents ? manager.DocumentsSource : manager.AnchorablesSource;

    private static void SetSource(DockingManager manager, IEnumerable? source, bool documents)
    {
        if (documents) manager.DocumentsSource = source;
        else manager.AnchorablesSource = source;
    }

    private sealed class SourceManager : DockingManager
    {
        internal bool Suppress { get; set; }
        internal bool Fail { get; set; }
        internal bool SuppressLayout { get; set; }
        internal Action<AvaloniaPropertyChangedEventArgs>? BeforeSource { get; set; }
        internal Action<AvaloniaPropertyChangedEventArgs>? AfterSource { get; set; }
        internal AvaloniaPropertyChangedEventArgs? LastChange { get; set; }
        internal List<object> Senders { get; } = new();

        internal void Apply(AvaloniaPropertyChangedEventArgs e, bool documents)
        {
            if (documents) base.OnDocumentsSourceChanged(e);
            else base.OnAnchorablesSourceChanged(e);
        }

        protected override void OnDocumentsSourceChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, true);
        protected override void OnAnchorablesSourceChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, false);

        private void Change(AvaloniaPropertyChangedEventArgs e, bool documents)
        {
            LastChange = e;
            if (Fail) throw new InvalidOperationException("Source callback failed");
            if (Suppress) return;
            BeforeSource?.Invoke(e);
            Apply(e, documents);
            AfterSource?.Invoke(e);
        }

        protected override void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
        {
            if (!SuppressLayout) base.OnLayoutChanged(oldLayout, newLayout);
        }

        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            Senders.Add(sender);
            return base.OnReceiveWeakEvent(managerType, sender, e);
        }
    }

    private sealed class Strategy : ILayoutUpdateStrategy
    {
        internal Action<LayoutContent>? After { get; set; }
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument document, ILayoutContainer? destination) => false;
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool, ILayoutContainer? destination) => false;
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument document) => After?.Invoke(document);
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool) => After?.Invoke(tool);
    }
}
