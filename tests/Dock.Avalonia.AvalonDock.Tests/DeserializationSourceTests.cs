using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using Avalonia.Headless.XUnit;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DeserializationSourceTests
{
    [AvaloniaFact]
    public void Content_Callbacks_Can_Populate_Sources_Without_Modifying_The_Previous_Layout()
    {
        ObservableCollection<object> documents = new() { new object() };
        ObservableCollection<object> tools = new() { new object() };
        DockingManager manager = new() { DocumentsSource = documents, AnchorablesSource = tools };
        LayoutRoot previous = manager.Layout;
        XmlLayoutSerializer serializer = new(manager);
        string xml = Save(serializer);
        bool added = false;
        serializer.LayoutSerializationCallback += (_, _) =>
        {
            if (added) return;
            added = true;
            documents.Add(new object());
            tools.Add(new object());
            Assert.Single(previous.Descendents().OfType<LayoutDocument>());
            Assert.Single(previous.Descendents().OfType<LayoutAnchorable>());
        };

        serializer.Deserialize(new StringReader(xml));

        Assert.NotSame(previous, manager.Layout);
        Assert.Equal(documents, manager.Layout.Descendents().OfType<LayoutDocument>().Select(x => x.Content));
        Assert.Equal(tools, manager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => x.Content));
        documents.Add(new object());
        tools.Add(new object());
        Assert.Equal(3, manager.Layout.Descendents().OfType<LayoutDocument>().Count());
        Assert.Equal(3, manager.Layout.Descendents().OfType<LayoutAnchorable>().Count());
    }

    [AvaloniaFact]
    public void Resetting_Sources_In_A_Callback_Does_Not_Clear_The_Previous_Contents()
    {
        ObservableCollection<object> documents = new() { new object() };
        ObservableCollection<object> tools = new() { new object() };
        DockingManager manager = new() { DocumentsSource = documents, AnchorablesSource = tools };
        LayoutDocument oldDocument = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
        LayoutAnchorable oldTool = Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());
        XmlLayoutSerializer serializer = new(manager);
        string xml = Save(serializer);
        serializer.LayoutSerializationCallback += (_, _) =>
        {
            documents.Clear();
            tools.Clear();
            Assert.NotNull(oldDocument.Content);
            Assert.NotNull(oldTool.Parent);
        };

        serializer.Deserialize(new StringReader(xml));

        Assert.Same(oldDocument.Content, Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>()).Content);
        Assert.Same(oldTool.Content, Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>()).Content);
        documents.Clear();
        tools.Clear();
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutContent>());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Failed_Deserialization_Resumes_Both_Source_Subscriptions(bool callbackFailure)
    {
        ObservableCollection<object> documents = new() { new object() };
        ObservableCollection<object> tools = new() { new object() };
        DockingManager manager = new() { DocumentsSource = documents, AnchorablesSource = tools };
        LayoutRoot previous = manager.Layout;
        XmlLayoutSerializer serializer = new(manager);
        string xml = Save(serializer);
        if (callbackFailure)
        {
            serializer.LayoutSerializationCallback += (_, _) => throw new InvalidOperationException("Content restore failed.");
            Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(new StringReader(xml)));
        }
        else Assert.Throws<XmlException>(() => serializer.Deserialize(new StringReader("<LayoutRoot>")));

        Assert.Same(previous, manager.Layout);
        documents.Add(new object());
        tools.Add(new object());
        Assert.Equal(documents, manager.Layout.Descendents().OfType<LayoutDocument>().Select(x => x.Content));
        Assert.Equal(tools, manager.Layout.Descendents().OfType<LayoutAnchorable>().Select(x => x.Content));
    }

    [AvaloniaFact]
    public void Nested_Serializers_Keep_Source_Notifications_Suspended_Until_The_Outer_Scope_Ends()
    {
        ObservableCollection<object> source = new();
        DockingManager manager = new() { DocumentsSource = source };
        ScopedSerializer outer = new(manager);
        ScopedSerializer inner = new(manager);
        outer.Start();
        inner.Start();
        inner.End();
        // An unmatched End must not release another serializer's suspension.
        inner.End();
        source.Add(new object());
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutDocument>());
        outer.End();
        object content = new();
        source.Add(content);
        Assert.Same(content, Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>()).Content);
    }

    private static string Save(XmlLayoutSerializer serializer)
    {
        using StringWriter writer = new();
        serializer.Serialize(writer);
        return writer.ToString();
    }

    private sealed class ScopedSerializer(DockingManager manager) : LayoutSerializer(manager)
    {
        internal void Start() => StartDeserialization();
        internal void End() => EndDeserialization();
    }
}
