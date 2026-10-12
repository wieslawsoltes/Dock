using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class DockableOwnerTests
{
    [AvaloniaFact]
    public void Owner_Changing_Precedes_The_Native_Property_Change_And_Ignores_Identical_Assignments()
    {
        Document content = new();
        DocumentDock first = new();
        DocumentDock second = new();
        List<(string Phase, IDockable? Old, IDockable? Next)> notifications = new();
        content.OwnerChanging += (_, args) =>
        {
            Assert.Same(args.OldOwner, content.Owner);
            notifications.Add(("changing", args.OldOwner, args.NewOwner));
        };
        content.PropertyChanged += (_, args) =>
        {
            if (args.Property != DockableBase.OwnerProperty) return;
            Assert.Same(args.NewValue, content.Owner);
            notifications.Add(("changed", (IDockable?)args.OldValue, (IDockable?)args.NewValue));
        };

        content.Owner = first;
        content.Owner = first;
        content.Owner = second;
        content.Owner = null;
        content.Owner = null;

        Assert.Equal(new (string, IDockable?, IDockable?)[]
        {
            ("changing", null, first), ("changed", null, first),
            ("changing", first, second), ("changed", first, second),
            ("changing", second, null), ("changed", second, null)
        }, notifications);
    }

    [AvaloniaFact]
    public void A_Throwing_Owner_Changing_Handler_Prevents_The_Assignment_And_Can_Be_Unsubscribed()
    {
        DocumentDock parent = new();
        Document content = new() { Owner = parent };
        EventHandler<DockableOwnerChangingEventArgs> handler = (_, _) => throw new InvalidOperationException("Rejected transition");
        content.OwnerChanging += handler;
        Assert.Throws<InvalidOperationException>(() => content.Owner = null);
        Assert.Same(parent, content.Owner);
        content.OwnerChanging -= handler;
        content.Owner = null;
        Assert.Null(content.Owner);
    }

    [AvaloniaFact]
    public async Task Invalid_Thread_Access_Is_Rejected_Before_Notifying_Owner_Observers()
    {
        DocumentDock parent = new();
        Document content = new() { Owner = parent };
        int changing = 0;
        content.OwnerChanging += (_, _) => changing++;

        Exception? exception = await Task.Run(() => Record.Exception(() => content.Owner = null));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Same(parent, content.Owner);
        Assert.Equal(0, changing);
    }
}
