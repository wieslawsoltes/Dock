using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Inpc.Controls;
using Dock.Model.Inpc.Core;
using Dock.Serializer.SystemTextJson;
using Newtonsoft.Json;
using Xunit;
using LegacySerializer = Dock.Serializer.DockSerializer;
using JsonSerializer = Dock.Serializer.SystemTextJson.DockSerializer;

[assembly: DockJsonSerializable(typeof(Dock.Serializer.UnitTests.CompatibilityPayload))]

namespace Dock.Serializer.UnitTests;

public class LegacyJsonCompatibilityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreviouslySavedNewtonsoftLayout_RestoresTypesValuesAndReferences(bool legacyApi)
    {
        var document = new CompatibilityDocument { Id = "Doc", Title = "Document", Category = "Code" };
        var documents = new DocumentDock
        {
            Id = "Documents",
            VisibleDockables = new List<IDockable> { document },
            ActiveDockable = document,
            DefaultDockable = document
        };
        var root = new RootDock
        {
            Id = "Root",
            VisibleDockables = new List<IDockable> { documents },
            ActiveDockable = documents,
            LeftPinnedDockables = new List<IDockable> { new Tool { Id = "Pinned" } },
            Windows = new List<IDockWindow> { new DockWindow { X = 10, Y = 20, Width = 800, Height = 600 } }
        };
        documents.Owner = root;
        document.Owner = documents;
        string previousJson = WritePreviousJson(root);

        IDockSerializer serializer = legacyApi ? new LegacySerializer() : new JsonSerializer();
        IRootDock? restored = serializer.Deserialize<IRootDock>(previousJson);

        Assert.NotNull(restored);
        var dock = Assert.IsType<DocumentDock>(restored.VisibleDockables![0]);
        var restoredDocument = Assert.IsType<CompatibilityDocument>(dock.VisibleDockables![0]);
        Assert.Equal("Code", restoredDocument.Category);
        Assert.Same(dock, restored.ActiveDockable);
        Assert.Same(restoredDocument, dock.ActiveDockable);
        Assert.Same(restoredDocument, dock.DefaultDockable);
        Assert.Same(restored, dock.Owner);
        Assert.Same(dock, restoredDocument.Owner);
        Assert.Equal("Pinned", restored.LeftPinnedDockables![0].Id);
        Assert.Equal(800, restored.Windows![0].Width);
        Assert.True(double.IsNaN(restoredDocument.Proportion));
        Assert.IsType<ObservableCollection<IDockable>>(restored.VisibleDockables);

        string newJson = serializer.Serialize(restored);
        IRootDock? replay = serializer.Deserialize<IRootDock>(newJson);
        Assert.NotNull(replay);
        Assert.Same(replay.VisibleDockables![0], replay.ActiveDockable);
    }

    [Fact]
    public void FrozenLegacyJson_ConcreteRootAndOldAssemblyVersion_Loads()
    {
        const string json = """
            {
              "$id": "1",
              "$type": "Dock.Model.Inpc.Controls.RootDock, Dock.Model.Inpc, Version=11.0.0.0, Culture=neutral, PublicKeyToken=null",
              "Id": "Root",
              "VisibleDockables": [
                {
                  "$id": "2",
                  "$type": "Dock.Model.Inpc.Controls.Document, Dock.Model.Inpc",
                  "Id": "Doc",
                  "Owner": { "$ref": "1" },
                  "Proportion": "NaN"
                }
              ],
              "ActiveDockable": { "$ref": "2" }
            }
            """;

        var serializer = new LegacySerializer();
        RootDock? restored = serializer.Deserialize<RootDock>(json);

        Assert.NotNull(restored);
        Assert.IsType<Document>(restored.VisibleDockables![0]);
        Assert.Same(restored.VisibleDockables[0], restored.ActiveDockable);
        Assert.Same(restored, restored.ActiveDockable!.Owner);
    }

    [Fact]
    public void LegacyPayloadAndDataMemberName_AreRestored()
    {
        var template = new CompatibilityTemplate
        {
            Caption = "Template",
            Content = new CompatibilityPayload { Name = "Payload" }
        };
        string previousJson = WritePreviousJson(template);
        var serializer = new LegacySerializer();

        var restored = serializer.Deserialize<CompatibilityTemplate>(previousJson);

        Assert.NotNull(restored);
        Assert.Equal("Template", restored.Caption);
        Assert.Equal("Payload", Assert.IsType<CompatibilityPayload>(restored.Content).Name);
    }

    [Fact]
    public void LegacyConcreteCollectionsAndDictionaryPayloads_LoadAndReplay()
    {
        var source = new CollectionPayload
        {
            Values = new List<int> { 1, 2 },
            Names = new Dictionary<string, string> { ["first"] = "Saved" },
            Array = new[] { 3, 4 }
        };
        var serializer = new LegacySerializer();
        string oldJson = WritePreviousJson(source);

        CollectionPayload? restored = serializer.Deserialize<CollectionPayload>(oldJson);

        Assert.NotNull(restored);
        Assert.Equal(source.Values, restored.Values);
        Assert.Equal("Saved", restored.Names!["first"]);
        Assert.Equal(source.Array, restored.Array);
        Assert.NotNull(serializer.Deserialize<CollectionPayload>(serializer.Serialize(restored)));
    }

    [Fact]
    public void SharedObjectPayload_ReferenceIdentitySurvivesOldAndNewJson()
    {
        var payload = new CompatibilityPayload { Name = "Shared" };
        var source = new SharedPayload { First = payload, Second = payload };
        var serializer = new LegacySerializer();

        SharedPayload? old = serializer.Deserialize<SharedPayload>(WritePreviousJson(source));
        SharedPayload? current = serializer.Deserialize<SharedPayload>(serializer.Serialize(source));

        Assert.NotNull(old);
        Assert.NotNull(current);
        Assert.IsType<CompatibilityPayload>(old.First);
        Assert.Same(old.First, old.Second);
        Assert.IsType<CompatibilityPayload>(current.First);
        Assert.Same(current.First, current.Second);
    }

    [Fact]
    public void ExistingCustomListConstructor_UsesCompiledCollectionCreator()
    {
        var serializer = new LegacySerializer(typeof(CompatibilityList<>));
        var source = new RootDock { VisibleDockables = new List<IDockable> { new Document { Id = "Saved" } } };

        RootDock? restored = serializer.Deserialize<RootDock>(WritePreviousJson(source));

        Assert.NotNull(restored);
        Assert.IsType<CompatibilityList<IDockable>>(restored.VisibleDockables);
        Assert.Equal("Saved", restored.VisibleDockables![0].Id);
    }

    [Fact]
    public void ExistingServiceProviderConstructor_UsesRegisteredDockableInstance()
    {
        var document = new CompatibilityDocument { Id = "Old", Category = "Old" };
        var provider = new DocumentProvider(document);
        var serializer = new LegacySerializer(typeof(List<>), provider);
        string json = WritePreviousJson(new CompatibilityDocument { Id = "Saved", Category = "Saved" });

        var restored = serializer.Deserialize<CompatibilityDocument>(json);

        Assert.Same(document, restored);
        Assert.Equal("Saved", restored!.Category);
    }

    [Fact]
    public void ServiceProviderConstructor_SupportsDockablesWithoutDefaultConstructors()
    {
        var dependency = new DocumentDependency();
        var document = new InjectedDocument(dependency);
        var serializer = new LegacySerializer(new InjectedDocumentProvider(document));
        string json = WritePreviousJson(new InjectedDocument(dependency) { Id = "Saved" });

        InjectedDocument? restored = serializer.Deserialize<InjectedDocument>(json);

        Assert.Same(document, restored);
        Assert.Same(dependency, restored!.Dependency);
        Assert.Equal("Saved", restored.Id);
    }

    [Fact]
    public void UnknownOrIncompatibleLegacyType_IsRejectedWithoutLoadingAssemblies()
    {
        var serializer = new LegacySerializer();
        Assert.Throws<System.Text.Json.JsonException>(() => serializer.Deserialize<RootDock>("""
            {"$type":"Missing.Root, Missing"}
            """));
        Assert.Throws<System.Text.Json.JsonException>(() => serializer.Deserialize<RootDock>(WritePreviousJson(new Document())));
    }

    private static string WritePreviousJson(object value) => JsonConvert.SerializeObject(value, new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.Objects,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        ContractResolver = new ListContractResolver(typeof(ObservableCollection<>)),
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new Newtonsoft.Json.Converters.KeyValuePairConverter() }
    });

    private sealed class DocumentProvider(CompatibilityDocument document) : IServiceProvider
    {
        public object? GetService(Type serviceType) => serviceType == typeof(CompatibilityDocument) ? document : null;
    }

    private sealed class InjectedDocumentProvider(InjectedDocument document) : IServiceProvider
    {
        public object? GetService(Type serviceType) => serviceType == typeof(InjectedDocument) ? document : null;
    }
}

[DataContract(IsReference = true)]
public sealed class CompatibilityDocument : Document
{
    [DataMember]
    public string? Category { get; set; }
}

[DataContract]
public sealed class CompatibilityTemplate : IDocumentTemplate
{
    [DataMember(Name = "caption")]
    public string? Caption { get; set; }
    [DataMember]
    public object? Content { get; set; }
}

public sealed class CompatibilityPayload
{
    public string? Name { get; set; }
}

public sealed class CollectionPayload
{
    public List<int>? Values { get; set; }
    public Dictionary<string, string>? Names { get; set; }
    public int[]? Array { get; set; }
}

public sealed class SharedPayload
{
    public object? First { get; set; }
    public object? Second { get; set; }
}

public sealed class CompatibilityList<T> : List<T>
{
}

public sealed class DocumentDependency;

[DataContract(IsReference = true)]
public sealed class InjectedDocument(DocumentDependency dependency) : Document
{
    [IgnoreDataMember]
    public DocumentDependency Dependency { get; } = dependency;
}
