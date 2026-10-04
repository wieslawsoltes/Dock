using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Mvvm.Core;
using Dock.Serializer.SystemTextJson;

[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.RegisteredPayload))]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.DerivedMetadata))]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.LegacyFieldPayload))]

namespace DockSystemTextJsonSourceGenAotSample;

internal static class Program
{
    private static int Main()
    {
        ValidateRoundTrip();
        ValidateLegacyJson();
        ValidateLegacyValuesAndPolymorphism();
        ValidateLegacyMembersAndCollections();
        Console.WriteLine("Dock source-generated AOT serialization round trip succeeded.");
        return 0;
    }

    private static void ValidateRoundTrip()
    {
        DockSerializer serializer = new();
        SampleRootDock source = CreateLayout();

        string json = serializer.Serialize(source);
        SampleRootDock restored = Require(serializer.Deserialize<SampleRootDock>(json), "Round-trip layout deserialization returned null.");
        VerifyLayout(restored);

        DockSerializer resolverSerializer = new(new DockSystemTextJsonResolver());
        string resolverJson = resolverSerializer.Serialize(source);
        SampleRootDock resolverRestored = Require(
            resolverSerializer.Deserialize<SampleRootDock>(resolverJson),
            "Resolver constructor deserialization returned null.");
        VerifyLayout(resolverRestored);

        IDocumentTemplate template = new SampleDocumentTemplate
        {
            TemplateTag = "TemplateTag",
            Content = new RegisteredPayload { Name = "TemplatePayload" }
        };

        string templateJson = serializer.Serialize(template);
        IDocumentTemplate restoredTemplate = Require(
            serializer.Deserialize<IDocumentTemplate>(templateJson),
            "Template round-trip deserialization returned null.");
        SampleDocumentTemplate sampleTemplate = RequireType<SampleDocumentTemplate>(
            restoredTemplate,
            "Generated serializer did not restore the custom template type.");
        RegisteredPayload payload = RequireType<RegisteredPayload>(
            sampleTemplate.Content,
            "Generated serializer did not restore the registered payload type.");

        Ensure(sampleTemplate.TemplateTag == "TemplateTag", "Template tag was not preserved.");
        Ensure(payload.Name == "TemplatePayload", "Template payload value was not preserved.");
    }

    private static void ValidateLegacyJson()
    {
        const string json = """
            {
              "$id": "1",
              "$type": "DockSystemTextJsonSourceGenAotSample.SampleRootDock, DockSystemTextJsonSourceGenAotSample",
              "Id": "LegacyRoot",
              "VisibleDockables": [
                {
                  "$id": "2",
                  "$type": "DockSystemTextJsonSourceGenAotSample.SampleDocument, DockSystemTextJsonSourceGenAotSample",
                  "Id": "LegacyDocument",
                  "Owner": { "$ref": "1" },
                  "DocumentTag": "LegacyTag",
                  "private_tag": "PrivateTag",
                  "GenericTag": "GenericTag",
                  "secret": "Secret",
                  "LegacyItems": [
                    {
                      "$type": "DockSystemTextJsonSourceGenAotSample.LegacyItem, DockSystemTextJsonSourceGenAotSample",
                      "Value": "CollectionValue"
                    }
                  ]
                }
              ],
              "ActiveDockable": { "$ref": "2" }
            }
            """;
        var serializer = new Dock.Serializer.DockSerializer();
        SampleRootDock restored = Require(serializer.Deserialize<SampleRootDock>(json), "Legacy layout returned null.");
        SampleDocument document = RequireType<SampleDocument>(restored.VisibleDockables?[0], "Legacy document type was lost.");
        Ensure(document.DocumentTag == "LegacyTag", "Legacy document properties were lost.");
        VerifyLegacyMembers(document);
        Ensure(ReferenceEquals(document, restored.ActiveDockable), "Legacy active document identity was lost.");
        Ensure(ReferenceEquals(restored, document.Owner), "Legacy owner identity was lost.");

        string currentJson = serializer.Serialize(restored);
        SampleRootDock replay = Require(serializer.Deserialize<SampleRootDock>(currentJson), "New layout returned null.");
        Ensure(ReferenceEquals(replay.VisibleDockables?[0], replay.ActiveDockable), "New layout references were lost.");
        VerifyLegacyMembers(RequireType<SampleDocument>(replay.ActiveDockable, "Replayed document type was lost."));
    }

    private static void VerifyLegacyMembers(SampleDocument document)
    {
        Ensure(document.PrivateTag == "PrivateTag", "Legacy private setter value was lost.");
        Ensure(document.GenericTag == "GenericTag", "Legacy generic private setter value was lost.");
        Ensure(document.ReadSecret() == "Secret", "Legacy private field value was lost.");
        Ensure(document.LegacyItems?[0].Value == "CollectionValue", "Legacy collection item was lost.");
    }

    private static void ValidateLegacyValuesAndPolymorphism()
    {
        const string json = """
            {
              "$type":"DockSystemTextJsonSourceGenAotSample.SampleDocument, DockSystemTextJsonSourceGenAotSample",
              "ScalarValue":"saved",
              "Metadata":{
                "$id":"metadata",
                "$type":"DockSystemTextJsonSourceGenAotSample.DerivedMetadata, DockSystemTextJsonSourceGenAotSample",
                "Name":"base", "Extra":"derived"
              },
              "SharedMetadata":{"$ref":"metadata"},
              "RemovedItems":[{"$type":"Removed.Type, Removed.Assembly"}]
            }
            """;
        var serializer = new Dock.Serializer.DockSerializer();
        SampleDocument restored = Require(serializer.Deserialize<SampleDocument>(json), "Legacy values returned null.");
        Ensure(restored.ScalarValue is string text && text == "saved", "Legacy scalar CLR type was lost.");
        Ensure(RequireType<DerivedMetadata>(restored.Metadata, "Legacy derived metadata was lost.").Extra == "derived", "Legacy derived value was lost.");
        Ensure(ReferenceEquals(restored.Metadata, restored.SharedMetadata), "Legacy metadata reference was lost.");

        Document declaredBase = restored;
        SampleDocument replay = RequireType<SampleDocument>(serializer.Deserialize<Document>(serializer.Serialize(declaredBase)), "Concrete base serialization lost its runtime type.");
        Ensure(RequireType<DerivedMetadata>(replay.Metadata, "Derived metadata replay was lost.").Extra == "derived", "Derived value replay was lost.");
        Ensure(ReferenceEquals(replay.Metadata, replay.SharedMetadata), "Replayed metadata reference was lost.");

        foreach (object value in new object[] { "text", true, 42L, 1.5d })
        {
            var document = new SampleDocument { ScalarValue = value };
            var result = Require(serializer.Deserialize<SampleDocument>(serializer.Serialize(document)), "Scalar replay returned null.");
            Ensure(Equals(value, result.ScalarValue), "Scalar value or CLR type was lost.");
        }
    }

    private static void ValidateLegacyMembersAndCollections()
    {
        const string json = """
            {
              "Items":[
                {"$id":"payload","$type":"DockSystemTextJsonSourceGenAotSample.LegacyFieldPayload, DockSystemTextJsonSourceGenAotSample","Value":"field","saved_name":"name"},
                {"$ref":"payload"}, "text", 42
              ],
              "Map":{"same":{"$ref":"payload"}},
              "Tags":["saved"], "Untyped":[1,2]
            }
            """;
        var serializer = new Dock.Serializer.DockSerializer();
        string current = json;
        for (int i = 0; i < 2; i++)
        {
            var restored = Require(serializer.Deserialize<LegacyCollectionPayload>(current), "Collections returned null.");
            var payload = RequireType<LegacyFieldPayload>(restored.Items![0], "Object collection type was lost.");
            Ensure(payload.Value == "field" && payload.Name == "name", "Public field or Newtonsoft name was lost.");
            Ensure(ReferenceEquals(payload, restored.Items[1]) && ReferenceEquals(payload, restored.Map!["same"]), "Collection references were lost.");
            Ensure(restored.Items[2] is string && restored.Items[3] is long, "Collection scalars changed type.");
            Ensure(restored.Tags!.Contains("saved"), "HashSet values were lost.");
            Ensure(restored.Untyped is System.Text.Json.JsonElement { ValueKind: System.Text.Json.JsonValueKind.Array }, "Untyped array was lost.");
            current = serializer.Serialize(restored);
            Ensure(!current.Contains("excluded"), "JsonIgnore was not respected.");
        }
        var rootArray = new[] { Guid.Empty };
        Ensure(Require(serializer.Deserialize<Guid[]>(serializer.Serialize(rootArray)), "Root array returned null.").Length == 1, "Root array was lost.");
    }

    private static void VerifyLayout(SampleRootDock restored)
    {
        Ensure(restored.RootTag == "RootTag", "Root tag was not preserved.");

        SampleDocumentDock documentDock = RequireType<SampleDocumentDock>(
            restored.VisibleDockables?[0],
            "First dockable was not restored as SampleDocumentDock.");
        SampleToolDock toolDock = RequireType<SampleToolDock>(
            restored.VisibleDockables?[1],
            "Second dockable was not restored as SampleToolDock.");
        SampleDocument document = RequireType<SampleDocument>(
            documentDock.VisibleDockables?[0],
            "Document was not restored as SampleDocument.");
        SampleTool tool = RequireType<SampleTool>(
            toolDock.VisibleDockables?[0],
            "Tool was not restored as SampleTool.");
        SampleDockWindow window = RequireType<SampleDockWindow>(
            restored.Windows?[0],
            "Window was not restored as SampleDockWindow.");
        SampleRootDock floatingRoot = RequireType<SampleRootDock>(
            window.Layout,
            "Floating window layout was not restored as SampleRootDock.");

        Ensure(documentDock.DockTag == "Docs", "Document dock tag was not preserved.");
        Ensure(toolDock.DockTag == "Tools", "Tool dock tag was not preserved.");
        Ensure(document.DocumentTag == "DocumentTag", "Document tag was not preserved.");
        Ensure(tool.ToolTag == "ToolTag", "Tool tag was not preserved.");
        Ensure(window.WindowTag == "WindowTag", "Window tag was not preserved.");
        Ensure(floatingRoot.RootTag == "WindowRootTag", "Floating root tag was not preserved.");
    }

    private static SampleRootDock CreateLayout()
    {
        var document = new SampleDocument
        {
            Id = "Doc1",
            Title = "Document 1",
            DocumentTag = "DocumentTag"
        };

        var tool = new SampleTool
        {
            Id = "Tool1",
            Title = "Tool 1",
            ToolTag = "ToolTag"
        };

        var documentDock = new SampleDocumentDock
        {
            Id = "DocumentDock",
            Title = "Documents",
            DockTag = "Docs",
            VisibleDockables = new List<IDockable> { document },
            ActiveDockable = document
        };

        var toolDock = new SampleToolDock
        {
            Id = "ToolDock",
            Title = "Tools",
            DockTag = "Tools",
            VisibleDockables = new List<IDockable> { tool },
            ActiveDockable = tool
        };

        var floatingRoot = new SampleRootDock
        {
            Id = "FloatingRoot",
            Title = "Floating Root",
            RootTag = "WindowRootTag",
            VisibleDockables = new List<IDockable>()
        };

        return new SampleRootDock
        {
            Id = "Root",
            Title = "Root",
            RootTag = "RootTag",
            VisibleDockables = new List<IDockable> { documentDock, toolDock },
            ActiveDockable = documentDock,
            Windows = new List<IDockWindow>
            {
                new SampleDockWindow
                {
                    Id = "Window1",
                    Title = "Window 1",
                    WindowTag = "WindowTag",
                    X = 100,
                    Y = 200,
                    Width = 640,
                    Height = 480,
                    Layout = floatingRoot
                }
            }
        };
    }

    private static T Require<T>(T? value, string message)
        where T : class
    {
        return value ?? throw new InvalidOperationException(message);
    }

    private static T RequireType<T>(object? value, string message)
        where T : class
    {
        return value as T ?? throw new InvalidOperationException(message);
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

public sealed class RegisteredPayload
{
    public string? Name { get; set; }
}

public sealed class SampleDocumentTemplate : IDocumentTemplate
{
    public object? Content { get; set; }

    public string? TemplateTag { get; set; }
}

public class SampleRootDock : RootDock
{
    [DataMember]
    public string? RootTag { get; set; }
}

public class SampleDocumentDock : DocumentDock
{
    [DataMember]
    public string? DockTag { get; set; }
}

public class SampleToolDock : ToolDock
{
    [DataMember]
    public string? DockTag { get; set; }
}

public abstract class GenericSampleDocument<T> : Document where T : class
{
    [DataMember]
    public T? GenericTag { get; private set; }
}

[DataContract]
public class SampleDocument : GenericSampleDocument<string>
{
    [DataMember]
    public string? DocumentTag { get; set; }

    [DataMember(Name = "private_tag")]
    public string? PrivateTag { get; private set; }

    [DataMember(Name = "secret")]
    private string? _secret = null;

    public string? ReadSecret() => _secret;

    [DataMember]
    public List<LegacyItem>? LegacyItems;

    [DataMember]
    public object? ScalarValue { get; set; }

    [DataMember]
    public LayoutMetadata? Metadata { get; set; }

    [DataMember]
    public LayoutMetadata? SharedMetadata { get; set; }
}

public class LayoutMetadata
{
    public string? Name { get; set; }
}

public sealed class DerivedMetadata : LayoutMetadata
{
    public string? Extra { get; set; }
}

public sealed class LegacyItem
{
    public string? Value { get; set; }
}

public sealed class LegacyFieldPayload
{
    public string? Value;
    [Newtonsoft.Json.JsonProperty("saved_name")] public string? Name { get; private set; }
    [Newtonsoft.Json.JsonIgnore] public string Ignored { get; set; } = "excluded";
}

public sealed class LegacyCollectionPayload
{
    public List<object>? Items { get; set; }
    public Dictionary<string, object>? Map { get; set; }
    public HashSet<string>? Tags { get; set; }
    public object? Untyped { get; set; }
}

public class SampleTool : Tool
{
    [DataMember]
    public string? ToolTag { get; set; }
}

public class SampleDockWindow : DockWindow
{
    [DataMember]
    public string? WindowTag { get; set; }
}
