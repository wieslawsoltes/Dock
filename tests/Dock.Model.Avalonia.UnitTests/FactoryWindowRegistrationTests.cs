using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class FactoryWindowRegistrationTests
{
    public static IEnumerable<object[]> CallbackChanges()
    {
        foreach (bool insert in new[] { false, true })
            foreach (bool addedCallback in new[] { false, true })
                for (int operation = 0; operation < 4; operation++)
                    yield return new object[] { insert, addedCallback, operation };
    }

    [AvaloniaTheory]
    [MemberData(nameof(CallbackChanges))]
    public void Callback_Removal_Or_Transfer_Does_Not_Initialize_A_Stale_Registration(
        bool insert, bool addedCallback, int operation)
    {
        RecordingFactory factory = new();
        RootDock root = new() { Windows = factory.CreateList<IDockWindow>() };
        RootDock destination = new() { Windows = factory.CreateList<IDockWindow>() };
        IDockWindow window = factory.CreateDockWindow();
        bool changed = false;
        int added = 0;
        void Change()
        {
            if (changed) return;
            changed = true;
            if (operation == 0) root.Windows.Remove(window);
            else if (operation == 1) root.Windows.Clear();
            else if (operation == 2) root.Windows = factory.CreateList<IDockWindow>();
            else
            {
                root.Windows.Remove(window);
                factory.AddWindow(destination, window);
            }
        }
        ((INotifyCollectionChanged)root.Windows).CollectionChanged += (_, args) =>
        {
            if (!addedCallback && args.Action == NotifyCollectionChangedAction.Add) Change();
        };
        factory.WindowAdded += (_, _) =>
        {
            added++;
            if (addedCallback) Change();
        };

        if (insert) factory.InsertWindow(root, window, 0);
        else factory.AddWindow(root, window);

        Assert.True(changed);
        Assert.Empty(root.Windows);
        Assert.Equal((addedCallback ? 1 : 0) + (operation == 3 ? 1 : 0), added);
        Assert.Equal(operation == 3 ? 1 : 0, factory.Initializations.Count);
        if (operation == 3)
        {
            Assert.Same(destination, Assert.Single(factory.Initializations));
            Assert.Same(destination, window.Owner);
            Assert.Same(factory, window.Factory);
            Assert.Same(window, Assert.Single(destination.Windows));
        }
        else
        {
            Assert.Null(window.Owner);
            Assert.Null(window.Factory);
            Assert.Null(window.Host);
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Normal_Registration_Preserves_Collection_Event_And_Initialization_Order(bool insert)
    {
        RecordingFactory factory = new();
        RootDock root = new() { Windows = factory.CreateList<IDockWindow>() };
        IDockWindow window = factory.CreateDockWindow();
        ((INotifyCollectionChanged)root.Windows).CollectionChanged += (_, _) =>
        {
            Assert.Null(window.Owner);
            factory.Events.Add("collection");
        };
        factory.WindowAdded += (_, _) =>
        {
            Assert.Null(window.Owner);
            factory.Events.Add("added");
        };

        if (insert) factory.InsertWindow(root, window, 0);
        else factory.AddWindow(root, window);

        Assert.Equal(new[] { "collection", "added", "initialize" }, factory.Events);
        Assert.Same(root, Assert.Single(factory.Initializations));
        Assert.Same(root, window.Owner);
        Assert.Same(factory, window.Factory);
        Assert.Same(window, Assert.Single(root.Windows));
    }

    private sealed class RecordingFactory : Factory
    {
        internal List<IDockable?> Initializations { get; } = new();
        internal List<string> Events { get; } = new();

        public override void InitDockWindow(IDockWindow window, IDockable? owner)
        {
            Initializations.Add(owner);
            Events.Add("initialize");
            base.InitDockWindow(window, owner);
        }
    }
}
