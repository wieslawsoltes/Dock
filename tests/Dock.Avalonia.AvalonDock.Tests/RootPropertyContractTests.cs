using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class RootPropertyContractTests
{
    [AvaloniaTheory]
    [InlineData("set")]
    [InlineData("remove")]
    [InlineData("replace")]
    public void Clearing_The_Root_Panel_Installs_A_Default_Document_Pane_Between_Notifications(string operation)
    {
        LayoutRoot root = new();
        LayoutPanel previous = root.RootPanel;
        object model = root.DockModel;
        List<(string Phase, LayoutPanel Panel)> events = new();
        root.PropertyChanging += (_, e) => { if (e.PropertyName == nameof(root.RootPanel)) events.Add(("before", root.RootPanel)); };
        root.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(root.RootPanel)) events.Add(("after", root.RootPanel)); };

        if (operation == "set") root.RootPanel = null;
        else if (operation == "remove") root.RemoveChild(previous);
        else root.ReplaceChild(previous, null!);

        LayoutPanel replacement = root.RootPanel;
        Assert.NotSame(previous, replacement);
        Assert.Null(previous.Parent);
        Assert.Same(root, replacement.Parent);
        Assert.Empty(Assert.IsType<LayoutDocumentPane>(Assert.Single(replacement.Children)).Children);
        Assert.Equal(new[] { ("before", previous), ("after", replacement) }, events);
        Assert.Same(model, root.DockModel);
        Assert.Same(replacement.DockModel, Assert.Single(((RootDock)root.DockModel).VisibleDockables!));
        Assert.Same(root.DockFactory, replacement.DockModel.Factory);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Root_Panel_Observer_Errors_Stop_At_Their_Phase_And_Later_Replacements_Remain_Usable(bool before)
    {
        LayoutRoot root = new();
        LayoutPanel previous = root.RootPanel;
        LayoutPanel replacement = new(new LayoutDocumentPane());
        InvalidOperationException error = new("root observer");
        bool fail = true;
        root.PropertyChanging += (_, e) => { if (fail && before && e.PropertyName == nameof(root.RootPanel)) throw error; };
        root.PropertyChanged += (_, e) => { if (fail && !before && e.PropertyName == nameof(root.RootPanel)) throw error; };

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => root.RootPanel = replacement));

        Assert.Same(before ? previous : replacement, root.RootPanel);
        Assert.Same(before ? root : null, previous.Parent);
        Assert.Same(before ? null : root, replacement.Parent);
        fail = false;
        root.RootPanel = null;
        Assert.Same(root, root.RootPanel.Parent);
        Assert.IsType<LayoutDocumentPane>(Assert.Single(root.RootPanel.Children));
    }

    [AvaloniaFact]
    public void A_Reentrant_Root_Panel_Changing_Callback_Does_Not_Leave_The_Interim_Panel_Attached()
    {
        LayoutRoot root = new();
        LayoutPanel original = root.RootPanel;
        LayoutPanel interim = new(new LayoutDocumentPane());
        LayoutPanel final = new(new LayoutDocumentPane());
        List<(string Phase, LayoutPanel Panel)> events = new();
        bool nested = false;
        root.PropertyChanging += (_, e) =>
        {
            if (e.PropertyName != nameof(root.RootPanel)) return;
            events.Add(("before", root.RootPanel));
            if (nested) return;
            nested = true;
            root.RootPanel = interim;
        };
        root.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(root.RootPanel)) events.Add(("after", root.RootPanel)); };

        root.RootPanel = final;

        Assert.Equal(new[] { ("before", original), ("before", original), ("after", interim), ("after", final) }, events);
        Assert.Null(original.Parent);
        Assert.Null(interim.Parent);
        Assert.Same(root, final.Parent);
        Assert.Same(final.DockModel, Assert.Single(((RootDock)root.DockModel).VisibleDockables!));
    }

    [AvaloniaFact]
    public void Derived_Roots_Observe_Upstream_Constructor_Property_Order()
    {
        ConstructorRoot root = new();
        Assert.Equal(new[] { "before:RightSide", "after:RightSide", "before:LeftSide", "after:LeftSide",
            "before:TopSide", "after:TopSide", "before:BottomSide", "after:BottomSide", "before:RootPanel", "after:RootPanel" }, root.Calls);
        Assert.Null(root.InitialPanel);
        Assert.Equal(5, root.InitialCount);
    }

    public static IEnumerable<object[]> SideOperations()
    {
        foreach (string side in new[] { "LeftSide", "TopSide", "RightSide", "BottomSide" })
            foreach (string operation in new[] { "set", "remove", "replace" })
                yield return new object[] { side, operation };
    }

    [AvaloniaTheory]
    [MemberData(nameof(SideOperations))]
    public void Clearing_A_Side_Removes_Its_Projection_And_Publishes_Null(string property, string operation)
    {
        LayoutRoot root = new();
        SideSlot slot = Slot(root, property);
        LayoutAnchorSide previous = slot.Read()!;
        LayoutAnchorGroup group = new();
        LayoutAnchorable tool = new();
        group.Children.Add(tool);
        previous.Children.Add(group);
        List<(string Phase, LayoutAnchorSide? Side)> events = new();
        root.PropertyChanging += (_, e) => { if (e.PropertyName == property) events.Add(("before", slot.Read())); };
        root.PropertyChanged += (_, e) => { if (e.PropertyName == property) events.Add(("after", slot.Read())); };

        if (operation == "set") slot.Write(null);
        else if (operation == "remove") root.RemoveChild(previous);
        else root.ReplaceChild(previous, null!);

        Assert.Null(slot.Read());
        Assert.Same(root, previous.Parent);
        Assert.Same(root, tool.Root);
        Assert.Same(group, tool.Parent);
        Assert.DoesNotContain(previous, root.Children);
        Assert.Equal(4, root.Children.Count());
        Assert.Equal(5, root.ChildrenCount); // The upstream count includes the five fixed root slots.
        Assert.Equal(new (string, LayoutAnchorSide?)[] { ("before", previous), ("after", null) }, events);
        Assert.Empty(Pinned(root, property));
        slot.Write(previous);
        Assert.Same(root, tool.Root);
        Assert.Same(tool.DockModel, Assert.Single(Pinned(root, property)));
    }

    [AvaloniaTheory]
    [InlineData("LeftSide", false)]
    [InlineData("LeftSide", true)]
    [InlineData("TopSide", false)]
    [InlineData("TopSide", true)]
    [InlineData("RightSide", false)]
    [InlineData("RightSide", true)]
    [InlineData("BottomSide", false)]
    [InlineData("BottomSide", true)]
    public void Side_Notification_Errors_Preserve_The_Completed_Replacement_Phase(string property, bool before)
    {
        LayoutRoot root = new();
        SideSlot slot = Slot(root, property);
        LayoutAnchorSide previous = slot.Read()!;
        LayoutAnchorSide replacement = new();
        InvalidOperationException error = new("side observer");
        bool fail = true;
        root.PropertyChanging += (_, e) => { if (fail && before && e.PropertyName == property) throw error; };
        root.PropertyChanged += (_, e) => { if (fail && !before && e.PropertyName == property) throw error; };

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => slot.Write(replacement)));

        Assert.Same(before ? previous : replacement, slot.Read());
        Assert.Same(root, previous.Parent);
        Assert.Same(before ? null : root, replacement.Parent);
        fail = false;
        slot.Write(null);
        Assert.Null(slot.Read());
    }

    [AvaloniaFact]
    public void Manager_Attachment_And_Detachment_Publish_Both_Property_Phases()
    {
        LayoutRoot root = new();
        DockingManager first = new();
        DockingManager second = new();
        List<(string Phase, DockingManager? Manager)> events = new();
        root.PropertyChanging += (_, e) => { if (e.PropertyName == nameof(root.Manager)) events.Add(("before", root.Manager)); };
        root.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(root.Manager)) events.Add(("after", root.Manager)); };

        first.Layout = root;
        first.Layout = root;
        first.Layout = new LayoutRoot();
        second.Layout = root;

        Assert.Equal(new (string, DockingManager?)[] { ("before", null), ("after", first),
            ("before", first), ("after", null), ("before", null), ("after", second) }, events);
    }

    [AvaloniaFact]
    public void Children_Enumerate_Panel_Windows_Sides_Then_Hidden_Content_And_Read_Sides_Lazily()
    {
        LayoutRoot root = new();
        LayoutDocumentFloatingWindow window = new() { RootDocument = new LayoutDocument() };
        root.FloatingWindows.Add(window);
        LayoutAnchorable hidden = new();
        root.Hidden.Add(hidden);
        Assert.Equal(new ILayoutElement[] { root.RootPanel, window, root.TopSide!, root.RightSide!, root.BottomSide!, root.LeftSide!, hidden }, root.Children);
        Assert.Equal(7, root.ChildrenCount);
        using IEnumerator<ILayoutElement> iterator = root.Children.GetEnumerator();
        Assert.True(iterator.MoveNext());
        Assert.Same(root.RootPanel, iterator.Current);
        root.TopSide = null;
        List<ILayoutElement> remainder = new();
        while (iterator.MoveNext()) remainder.Add(iterator.Current);
        Assert.Equal(new ILayoutElement[] { window, root.RightSide!, root.BottomSide!, root.LeftSide!, hidden }, remainder);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReplaceChild_Removes_Then_Inserts_A_Hidden_Item_Or_Floating_Window(bool floating)
    {
        LayoutRoot root = new();
        ILayoutElement previous = floating ? new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() } : new LayoutAnchorable();
        ILayoutElement replacement = floating ? new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() } : new LayoutAnchorable();
        INotifyCollectionChanged collection;
        if (floating) { root.FloatingWindows.Add((LayoutFloatingWindow)previous); collection = root.FloatingWindows; }
        else { root.Hidden.Add((LayoutAnchorable)previous); collection = root.Hidden; }
        List<NotifyCollectionChangedAction> events = new();
        collection.CollectionChanged += (_, e) =>
        {
            events.Add(e.Action);
            Assert.Null(previous.Parent);
            Assert.Same(e.Action == NotifyCollectionChangedAction.Remove ? null : root, replacement.Parent);
        };

        root.ReplaceChild(previous, replacement);

        Assert.Equal(new[] { NotifyCollectionChangedAction.Remove, NotifyCollectionChangedAction.Add }, events);
        Assert.Same(root, replacement.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_ReplaceChild_Removal_Observer_Can_Stop_Before_The_New_Item_Is_Inserted(bool floating)
    {
        LayoutRoot root = new();
        ILayoutElement previous = floating ? new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() } : new LayoutAnchorable();
        ILayoutElement replacement = floating ? new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() } : new LayoutAnchorable();
        INotifyCollectionChanged collection;
        if (floating) { root.FloatingWindows.Add((LayoutFloatingWindow)previous); collection = root.FloatingWindows; }
        else { root.Hidden.Add((LayoutAnchorable)previous); collection = root.Hidden; }
        InvalidOperationException error = new("remove observer");
        collection.CollectionChanged += (_, _) => throw error;

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => root.ReplaceChild(previous, replacement)));

        Assert.Null(previous.Parent);
        Assert.Null(replacement.Parent);
        Assert.DoesNotContain(previous, root.Children);
        Assert.DoesNotContain(replacement, root.Children);
    }

    [AvaloniaFact]
    public void Null_Sides_Write_Empty_Xml_Sections_And_Restore_Default_Sides()
    {
        LayoutRoot root = new() { TopSide = null, RightSide = null, BottomSide = null, LeftSide = null };
        using StringWriter text = new();
        using (XmlWriter writer = XmlWriter.Create(text, new() { OmitXmlDeclaration = true }))
        {
            writer.WriteStartElement("LayoutRoot");
            root.WriteXml(writer);
            writer.WriteEndElement();
        }
        XElement xml = XElement.Parse(text.ToString());
        Assert.Equal(new[] { "RootPanel", "TopSide", "RightSide", "LeftSide", "BottomSide", "FloatingWindows", "Hidden" }, xml.Elements().Select(e => e.Name.LocalName));
        foreach (string side in new[] { "TopSide", "RightSide", "BottomSide", "LeftSide" }) Assert.True(xml.Element(side)!.IsEmpty);
        using XmlReader reader = xml.CreateReader();
        root.ReadXml(reader);
        Assert.Equal(5, root.Children.Count());
        Assert.All(new[] { root.TopSide, root.RightSide, root.BottomSide, root.LeftSide }, side => Assert.Same(root, side!.Parent));
    }

    [AvaloniaFact]
    public void Compiled_Xaml_Can_Clear_The_Root_Panel_And_All_Sides_In_Both_Xml_Namespaces()
    {
        RootPropertiesFixture fixture = new();
        Window window = new() { Content = fixture, Width = 900, Height = 600 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            foreach (string name in new[] { "DefaultManager", "PrefixedManager" })
            {
                DockingManager manager = fixture.FindControl<DockingManager>(name)!;
                Assert.IsType<LayoutDocumentPane>(Assert.Single(manager.Layout.RootPanel.Children));
                Assert.Null(manager.LeftSidePanel);
                Assert.Null(manager.TopSidePanel);
                Assert.Null(manager.RightSidePanel);
                Assert.Null(manager.BottomSidePanel);
                manager.Layout.LeftSide = new LayoutAnchorSide();
                Assert.Same(manager.Layout.LeftSide, manager.LeftSidePanel!.Model);
                manager.Layout.LeftSide = null;
                Assert.Null(manager.LeftSidePanel);
            }
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Creating_A_Docking_Pane_Publishes_The_Wrapped_Panel_Before_Adding_The_New_Pane(bool tool)
    {
        DockingManager manager = new();
        LayoutContent content;
        if (tool)
        {
            manager.Layout.RootPanel = new LayoutPanel();
            LayoutAnchorable anchorable = new();
            manager.Layout.FloatingWindows.Add(new LayoutAnchorableFloatingWindow { RootPanel = new(new LayoutAnchorablePane(anchorable)) });
            content = anchorable;
        }
        else
        {
            LayoutDocument document = new();
            manager.Layout.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = document });
            document.IsActive = true;
            content = document;
        }
        LayoutPanel original = manager.Layout.RootPanel;
        List<bool> wrapsOriginal = new();
        manager.Layout.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(LayoutRoot.RootPanel)) return;
            Assert.Single(manager.Layout.RootPanel.Children);
            wrapsOriginal.Add(ReferenceEquals(manager.Layout.RootPanel.Children[0], original));
        };

        if (tool) content.Dock();
        else content.DockAsDocument();

        Assert.Equal(new[] { false, true }, wrapsOriginal);
        Assert.Same(manager.Layout, content.Root);
        Assert.False(content.IsFloating);
        manager.Layout.FloatingWindows.Clear();
    }

    private static SideSlot Slot(LayoutRoot root, string property) => property switch
    {
        "LeftSide" => new(() => root.LeftSide, value => root.LeftSide = value),
        "TopSide" => new(() => root.TopSide, value => root.TopSide = value),
        "RightSide" => new(() => root.RightSide, value => root.RightSide = value),
        _ => new(() => root.BottomSide, value => root.BottomSide = value)
    };

    private static IEnumerable<Dock.Model.Core.IDockable> Pinned(LayoutRoot root, string property)
    {
        RootDock model = (RootDock)root.DockModel;
        return (property switch
        {
            "LeftSide" => model.LeftPinnedDockables,
            "TopSide" => model.TopPinnedDockables,
            "RightSide" => model.RightPinnedDockables,
            _ => model.BottomPinnedDockables
        })!;
    }

    private sealed record SideSlot(Func<LayoutAnchorSide?> Read, Action<LayoutAnchorSide?> Write);

    private sealed class ConstructorRoot : LayoutRoot
    {
        private List<string>? _calls;
        internal IReadOnlyList<string> Calls => _calls!;
        internal LayoutPanel? InitialPanel { get; private set; }
        internal int InitialCount { get; private set; }
        protected override void RaisePropertyChanging(string? propertyName = null)
        {
            if (propertyName == "RightSide" && _calls is null) { InitialPanel = RootPanel; InitialCount = ChildrenCount; }
            if (IsRootProperty(propertyName)) (_calls ??= new()).Add("before:" + propertyName);
            base.RaisePropertyChanging(propertyName);
        }
        protected override void RaisePropertyChanged(string? propertyName = null)
        {
            if (IsRootProperty(propertyName)) (_calls ??= new()).Add("after:" + propertyName);
            base.RaisePropertyChanged(propertyName);
        }
        private static bool IsRootProperty(string? name) => name is "RightSide" or "LeftSide" or "TopSide" or "BottomSide" or "RootPanel";
    }
}
