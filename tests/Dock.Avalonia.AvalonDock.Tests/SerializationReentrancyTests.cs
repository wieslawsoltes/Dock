using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SerializationReentrancyTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Nested_Restoration_Keeps_Both_Sources_Suspended_Until_The_Outer_Callback_Returns(
        bool sameSerializer, bool innerFailure)
    {
        ObservableCollection<object> documents = new() { new object() };
        ObservableCollection<object> tools = new() { new object() };
        DockingManager manager = new() { DocumentsSource = documents, AnchorablesSource = tools };
        LayoutRoot previous = manager.Layout;
        XmlLayoutSerializer outer = new(manager);
        XmlLayoutSerializer inner = sameSerializer ? outer : new(manager);
        string xml = Save(outer);
        bool nesting = false;
        bool nested = false;
        LayoutRoot? outerRoot = null;
        LayoutRoot? innerRoot = null;

        void Restore(object? sender, LayoutSerializationCallbackEventArgs args)
        {
            if (nesting)
            {
                innerRoot ??= Assert.IsType<LayoutRoot>(args.Model.Root);
                if (innerFailure) throw new InvalidOperationException("Inner restoration failed.");
                return;
            }
            if (nested) return;
            nested = true;
            outerRoot = Assert.IsType<LayoutRoot>(args.Model.Root);
            nesting = true;
            try
            {
                if (innerFailure)
                    Assert.Throws<InvalidOperationException>(() => inner.Deserialize(new StringReader(xml)));
                else
                    inner.Deserialize(new StringReader(xml));
            }
            finally { nesting = false; }

            Assert.Same(innerFailure ? previous : innerRoot, manager.Layout);
            documents.Add(new object());
            tools.Add(new object());
            Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
            Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());
        }

        outer.LayoutSerializationCallback += Restore;
        if (!sameSerializer) inner.LayoutSerializationCallback += Restore;

        outer.Deserialize(new StringReader(xml));

        Assert.True(nested);
        Assert.NotNull(innerRoot);
        Assert.Same(outerRoot, manager.Layout);
        Assert.Same(manager, outerRoot!.Manager);
        Assert.Null(previous.Manager);
        Assert.Null(innerRoot.Manager);
        AssertSources(manager, documents, tools);
        documents.Add(new object());
        tools.Add(new object());
        AssertSources(manager, documents, tools);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Replacing_Sources_In_A_Content_Callback_Observes_Only_The_New_Collections(bool callbackFailure)
    {
        ObservableCollection<object> oldDocuments = new() { new object() };
        ObservableCollection<object> oldTools = new() { new object() };
        ObservableCollection<object> documents = new() { new object() };
        ObservableCollection<object> tools = new() { new object() };
        DockingManager manager = new() { DocumentsSource = oldDocuments, AnchorablesSource = oldTools };
        LayoutRoot previous = manager.Layout;
        XmlLayoutSerializer serializer = new(manager);
        string xml = Save(serializer);
        bool replaced = false;
        serializer.LayoutSerializationCallback += (_, _) =>
        {
            if (replaced) return;
            replaced = true;
            manager.DocumentsSource = documents;
            manager.AnchorablesSource = tools;
            AssertSources(manager, documents, tools);
            if (callbackFailure) throw new InvalidOperationException("Restoration failed after source replacement.");
        };

        if (callbackFailure)
        {
            Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(new StringReader(xml)));
            Assert.Same(previous, manager.Layout);
        }
        else
        {
            serializer.Deserialize(new StringReader(xml));
            Assert.NotSame(previous, manager.Layout);
            // Saved content identities are independent of the current sources.
            Assert.Contains(manager.Layout.Descendents().OfType<LayoutDocument>(), content => ReferenceEquals(content.Content, oldDocuments[0]));
            Assert.Contains(manager.Layout.Descendents().OfType<LayoutAnchorable>(), content => ReferenceEquals(content.Content, oldTools[0]));
        }

        int documentCount = manager.Layout.Descendents().OfType<LayoutDocument>().Count();
        int toolCount = manager.Layout.Descendents().OfType<LayoutAnchorable>().Count();
        oldDocuments.Add(new object());
        oldTools.Add(new object());
        Assert.Equal(documentCount, manager.Layout.Descendents().OfType<LayoutDocument>().Count());
        Assert.Equal(toolCount, manager.Layout.Descendents().OfType<LayoutAnchorable>().Count());
        documents.Add(new object());
        tools.Add(new object());
        Assert.Equal(documentCount + 1, manager.Layout.Descendents().OfType<LayoutDocument>().Count());
        Assert.Equal(toolCount + 1, manager.Layout.Descendents().OfType<LayoutAnchorable>().Count());
        Assert.Contains(manager.Layout.Descendents().OfType<LayoutDocument>(), content => ReferenceEquals(content.Content, documents[1]));
        Assert.Contains(manager.Layout.Descendents().OfType<LayoutAnchorable>(), content => ReferenceEquals(content.Content, tools[1]));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_LayoutChanged_Callback_Can_Restore_Another_Layout_And_Source_Notifications_Resume(bool callbackFailure)
    {
        ObservableCollection<object> documents = new() { new object() };
        ObservableCollection<object> tools = new() { new object() };
        DockingManager manager = new() { DocumentsSource = documents, AnchorablesSource = tools };
        XmlLayoutSerializer serializer = new(manager);
        string xml = Save(serializer);
        LayoutRoot? outerRoot = null;
        LayoutRoot? finalRoot = null;
        int changes = 0;
        manager.LayoutChanged += (_, _) =>
        {
            changes++;
            if (changes != 1) return;
            outerRoot = manager.Layout;
            serializer.Deserialize(new StringReader(xml));
            finalRoot = manager.Layout;
            if (callbackFailure) throw new InvalidOperationException("Layout notification failed.");
        };

        if (callbackFailure)
            Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(new StringReader(xml)));
        else
            serializer.Deserialize(new StringReader(xml));

        Assert.Equal(2, changes);
        Assert.NotSame(outerRoot, finalRoot);
        Assert.Null(outerRoot!.Manager);
        Assert.Same(finalRoot, manager.Layout);
        Assert.Same(manager, finalRoot!.Manager);
        AssertSources(manager, documents, tools);
        documents.Add(new object());
        tools.Add(new object());
        AssertSources(manager, documents, tools);
    }

    private static void AssertSources(DockingManager manager, ObservableCollection<object> documents, ObservableCollection<object> tools)
    {
        Assert.Equal(documents, manager.Layout.Descendents().OfType<LayoutDocument>().Select(content => content.Content));
        Assert.Equal(tools, manager.Layout.Descendents().OfType<LayoutAnchorable>().Select(content => content.Content));
    }

    private static string Save(XmlLayoutSerializer serializer)
    {
        using StringWriter writer = new();
        serializer.Serialize(writer);
        return writer.ToString();
    }
}
