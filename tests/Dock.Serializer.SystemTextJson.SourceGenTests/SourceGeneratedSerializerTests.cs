using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Inpc.Controls;
using Dock.Model.Inpc.Core;
using Dock.Serializer.SystemTextJson.SourceGenSharedTypes;
using Dock.Serializer.SystemTextJson;
using Xunit;

[assembly: DockJsonSourceGeneration]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.RegisteredPayload))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.GenericRegisteredPayload<int>))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.GenericRegisteredPayload<string>))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenTests.TypeNamedPayload))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenSharedTypes.LibraryRootDock))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenSharedTypes.LibraryDocumentDock))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenSharedTypes.LibraryToolDock))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenSharedTypes.LibraryDocument))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenSharedTypes.LibraryTool))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenSharedTypes.LibraryDockWindow))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenSharedTypes.LibraryDocumentTemplate))]
[assembly: DockJsonSerializable(typeof(Dock.Serializer.SystemTextJson.SourceGenSharedTypes.LibraryRegisteredPayload))]

namespace Dock.Serializer.SystemTextJson.SourceGenTests;

public class SourceGeneratedSerializerTests
{
    [Fact]
    public void GeneratedSerializer_Roundtrip_CustomDockTypes_Works()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer();
        var source = CreateLayout();

        var json = serializer.Serialize(source);
        var restored = serializer.Deserialize<CustomRootDock>(json);

        Assert.NotNull(restored);
        Assert.Equal("RootTag", restored!.RootTag);
        Assert.Equal("WindowRootTag", restored.Windows?[0].Layout is CustomRootDock windowRoot ? windowRoot.RootTag : null);

        var documentDock = Assert.IsType<CustomDocumentDock>(restored.VisibleDockables![0]);
        var toolDock = Assert.IsType<CustomToolDock>(restored.VisibleDockables![1]);
        var document = Assert.IsType<CustomDocument>(documentDock.VisibleDockables![0]);
        var tool = Assert.IsType<CustomTool>(toolDock.VisibleDockables![0]);
        var window = Assert.IsType<CustomDockWindow>(restored.Windows![0]);

        Assert.Equal("Docs", documentDock.DockTag);
        Assert.Equal("Tools", toolDock.DockTag);
        Assert.Equal("DocumentTag", document.DocumentTag);
        Assert.Equal("ToolTag", tool.ToolTag);
        Assert.Equal("WindowTag", window.WindowTag);
        Assert.Null(document.Context);
    }

    [Fact]
    public void GeneratedSerializer_Roundtrip_TemplateContent_Works()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer();
        IDocumentTemplate source = new CustomDocumentTemplate
        {
            TemplateTag = "TemplateTag",
            Content = new RegisteredPayload { Name = "TemplatePayload" }
        };

        var json = serializer.Serialize(source);
        var restored = serializer.Deserialize<IDocumentTemplate>(json);

        var template = Assert.IsType<CustomDocumentTemplate>(restored);
        var payload = Assert.IsType<RegisteredPayload>(template.Content);
        Assert.Equal("TemplateTag", template.TemplateTag);
        Assert.Equal("TemplatePayload", payload.Name);
    }

    [Fact]
    public void GeneratedSerializer_Roundtrip_ClosedGenericPayloadRegistrations_Work()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer();

        IDocumentTemplate intSource = new CustomDocumentTemplate
        {
            TemplateTag = "IntTemplate",
            Content = new GenericRegisteredPayload<int> { Value = 42 }
        };

        string intJson = serializer.Serialize(intSource);
        IDocumentTemplate? intRestored = serializer.Deserialize<IDocumentTemplate>(intJson);

        var intTemplate = Assert.IsType<CustomDocumentTemplate>(intRestored);
        var intPayload = Assert.IsType<GenericRegisteredPayload<int>>(intTemplate.Content);
        Assert.Equal("IntTemplate", intTemplate.TemplateTag);
        Assert.Equal(42, intPayload.Value);

        IDocumentTemplate stringSource = new CustomDocumentTemplate
        {
            TemplateTag = "StringTemplate",
            Content = new GenericRegisteredPayload<string> { Value = "Payload" }
        };

        string stringJson = serializer.Serialize(stringSource);
        IDocumentTemplate? stringRestored = serializer.Deserialize<IDocumentTemplate>(stringJson);

        var stringTemplate = Assert.IsType<CustomDocumentTemplate>(stringRestored);
        var stringPayload = Assert.IsType<GenericRegisteredPayload<string>>(stringTemplate.Content);
        Assert.Equal("StringTemplate", stringTemplate.TemplateTag);
        Assert.Equal("Payload", stringPayload.Value);
    }

    [Fact]
    public void GeneratedSerializer_ReadsLegacyTemplateContentWithoutTypeDiscriminator()
    {
        var reflectionSerializer = new DockSerializer();
        var generatedSerializer = DockSystemTextJsonGenerated.CreateSerializer();
        var source = new CustomDocumentTemplate
        {
            TemplateTag = "LegacyTemplate",
            Content = new RegisteredPayload { Name = "LegacyPayload" }
        };

        string legacyJson = reflectionSerializer.Serialize(source);
        CustomDocumentTemplate? restored = generatedSerializer.Deserialize<CustomDocumentTemplate>(legacyJson);

        Assert.NotNull(restored);
        var content = Assert.IsType<JsonElement>(restored!.Content);
        Assert.Equal("LegacyTemplate", restored.TemplateTag);
        Assert.Equal(JsonValueKind.Object, content.ValueKind);
        Assert.Equal("LegacyPayload", content.GetProperty("Name").GetString());

        string replayJson = generatedSerializer.Serialize(restored);
        CustomDocumentTemplate? replayRestored = generatedSerializer.Deserialize<CustomDocumentTemplate>(replayJson);

        Assert.NotNull(replayRestored);
        var replayContent = Assert.IsType<JsonElement>(replayRestored!.Content);
        Assert.Equal("LegacyPayload", replayContent.GetProperty("Name").GetString());
    }

    [Fact]
    public void GeneratedSerializer_PreservesCustomObjectPropertyConverter()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer();
        var source = new CustomConvertedTemplate
        {
            TemplateTag = "ConvertedTemplate",
            Content = "ConvertedValue"
        };

        string json = serializer.Serialize(source);
        CustomConvertedTemplate? restored = serializer.Deserialize<CustomConvertedTemplate>(json);

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.NotNull(restored);
        Assert.Equal("ConvertedValue", document.RootElement.GetProperty("Content").GetString());
        Assert.Equal("ConvertedTemplate", restored!.TemplateTag);
        Assert.Equal("ConvertedValue", Assert.IsType<string>(restored.Content));
    }

    [Fact]
    public void GeneratedSerializer_PreservesPayloadWithTypeNamedMember()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer();
        var source = new CustomDocumentTemplate
        {
            TemplateTag = "TypeNamedTemplate",
            Content = new TypeNamedPayload
            {
                Kind = "payload-type",
                Name = "PayloadName"
            }
        };

        string json = serializer.Serialize(source);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement payload = document.RootElement.GetProperty("Content");
        int typePropertyCount = payload.EnumerateObject().Count(static x => x.NameEquals("$type"));

        CustomDocumentTemplate? restored = serializer.Deserialize<CustomDocumentTemplate>(json);

        Assert.Equal(2, typePropertyCount);
        Assert.NotNull(restored);
        JsonElement restoredPayload = Assert.IsType<JsonElement>(restored!.Content);
        Assert.Equal("payload-type", restoredPayload.EnumerateObject().Last(static x => x.NameEquals("$type")).Value.GetString());
        Assert.Equal("PayloadName", restoredPayload.GetProperty("Name").GetString());

        string replayJson = serializer.Serialize(restored);
        using JsonDocument replayDocument = JsonDocument.Parse(replayJson);
        int replayTypePropertyCount = replayDocument.RootElement.GetProperty("Content")
            .EnumerateObject()
            .Count(static x => x.NameEquals("$type"));

        Assert.Equal(2, replayTypePropertyCount);
    }

    [Fact]
    public void GeneratedSerializer_UnregisteredPayload_Throws()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer();
        IDocumentTemplate template = new CustomDocumentTemplate
        {
            TemplateTag = "TemplateTag",
            Content = new UnregisteredPayload { Name = "NotRegistered" }
        };

        Assert.Throws<NotSupportedException>(() => serializer.Serialize(template));
    }

    [Fact]
    public void GeneratedAndReflectionSerializers_AreWireCompatible()
    {
        var generatedSerializer = DockSystemTextJsonGenerated.CreateSerializer();
        var reflectionSerializer = new DockSerializer();
        var source = CreateLayout();

        var generatedJson = generatedSerializer.Serialize(source);
        var reflectionRoundtrip = reflectionSerializer.Deserialize<CustomRootDock>(generatedJson);

        Assert.NotNull(reflectionRoundtrip);
        Assert.Equal("RootTag", reflectionRoundtrip!.RootTag);
        Assert.IsType<CustomDocumentDock>(reflectionRoundtrip.VisibleDockables![0]);

        var reflectionJson = reflectionSerializer.Serialize(source);
        var generatedRoundtrip = generatedSerializer.Deserialize<CustomRootDock>(reflectionJson);

        Assert.NotNull(generatedRoundtrip);
        Assert.Equal("RootTag", generatedRoundtrip!.RootTag);
        Assert.IsType<CustomToolDock>(generatedRoundtrip.VisibleDockables![1]);
    }

    [Fact]
    public void GeneratedSerializer_ListTypeOverride_UsesRequestedConcreteListType()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer(typeof(List<>));
        var source = CreateLayout();

        var json = serializer.Serialize(source);
        var restored = serializer.Deserialize<CustomRootDock>(json);

        Assert.NotNull(restored);
        Assert.IsType<List<IDockable>>(restored!.VisibleDockables);
        Assert.IsType<List<IDockWindow>>(restored.Windows);

        var documentDock = Assert.IsType<CustomDocumentDock>(restored.VisibleDockables![0]);
        var toolDock = Assert.IsType<CustomToolDock>(restored.VisibleDockables![1]);

        Assert.IsType<List<IDockable>>(documentDock.VisibleDockables);
        Assert.IsType<List<IDockable>>(toolDock.VisibleDockables);
    }

    [Fact]
    public void GeneratedSerializer_Roundtrip_ReferencedAssemblyDockTypes_Works()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer();
        var source = CreateReferencedLayout();

        var json = serializer.Serialize(source);
        var restored = serializer.Deserialize<LibraryRootDock>(json);

        Assert.NotNull(restored);
        Assert.Equal("LibraryRootTag", restored!.RootTag);
        Assert.Equal("LibraryWindowRootTag", restored.Windows?[0].Layout is LibraryRootDock windowRoot ? windowRoot.RootTag : null);

        var documentDock = Assert.IsType<LibraryDocumentDock>(restored.VisibleDockables![0]);
        var toolDock = Assert.IsType<LibraryToolDock>(restored.VisibleDockables![1]);
        var document = Assert.IsType<LibraryDocument>(documentDock.VisibleDockables![0]);
        var tool = Assert.IsType<LibraryTool>(toolDock.VisibleDockables![0]);
        var window = Assert.IsType<LibraryDockWindow>(restored.Windows![0]);

        Assert.Equal("LibraryDocs", documentDock.DockTag);
        Assert.Equal("LibraryTools", toolDock.DockTag);
        Assert.Equal("LibraryDocumentTag", document.DocumentTag);
        Assert.Equal("LibraryToolTag", tool.ToolTag);
        Assert.Equal("LibraryWindowTag", window.WindowTag);
    }

    [Fact]
    public void GeneratedSerializer_Roundtrip_ReferencedAssemblyTemplateContent_Works()
    {
        var serializer = DockSystemTextJsonGenerated.CreateSerializer();
        IDocumentTemplate source = new LibraryDocumentTemplate
        {
            TemplateTag = "LibraryTemplateTag",
            Content = new LibraryRegisteredPayload { Name = "LibraryTemplatePayload" }
        };

        var json = serializer.Serialize(source);
        var restored = serializer.Deserialize<IDocumentTemplate>(json);

        var template = Assert.IsType<LibraryDocumentTemplate>(restored);
        var payload = Assert.IsType<LibraryRegisteredPayload>(template.Content);
        Assert.Equal("LibraryTemplateTag", template.TemplateTag);
        Assert.Equal("LibraryTemplatePayload", payload.Name);
    }

    [Fact]
    public void GeneratedResolver_PublicConstructor_Roundtrip_Works()
    {
        var serializer = new DockSerializer(new DockSystemTextJsonResolver());
        var source = CreateReferencedLayout();

        var json = serializer.Serialize(source);
        var restored = serializer.Deserialize<LibraryRootDock>(json);

        Assert.NotNull(restored);
        Assert.Equal("LibraryRootTag", restored!.RootTag);
        Assert.IsType<LibraryDocumentDock>(restored.VisibleDockables![0]);
        Assert.IsType<LibraryDockWindow>(restored.Windows![0]);
    }

    private static CustomRootDock CreateLayout()
    {
        var document = new CustomDocument
        {
            Id = "Doc1",
            Title = "Document 1",
            DocumentTag = "DocumentTag"
        };

        var tool = new CustomTool
        {
            Id = "Tool1",
            Title = "Tool 1",
            ToolTag = "ToolTag"
        };

        var documentDock = new CustomDocumentDock
        {
            Id = "DocumentDock",
            Title = "Documents",
            DockTag = "Docs",
            VisibleDockables = new List<IDockable> { document },
            ActiveDockable = document
        };

        var toolDock = new CustomToolDock
        {
            Id = "ToolDock",
            Title = "Tools",
            DockTag = "Tools",
            VisibleDockables = new List<IDockable> { tool },
            ActiveDockable = tool
        };

        var floatingRoot = new CustomRootDock
        {
            Id = "FloatingRoot",
            Title = "Floating Root",
            RootTag = "WindowRootTag",
            VisibleDockables = new List<IDockable>()
        };

        return new CustomRootDock
        {
            Id = "Root",
            Title = "Root",
            RootTag = "RootTag",
            VisibleDockables = new List<IDockable> { documentDock, toolDock },
            ActiveDockable = documentDock,
            Windows = new List<IDockWindow>
            {
                new CustomDockWindow
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

    private static LibraryRootDock CreateReferencedLayout()
    {
        var document = new LibraryDocument
        {
            Id = "LibraryDoc1",
            Title = "Library Document 1",
            DocumentTag = "LibraryDocumentTag"
        };

        var tool = new LibraryTool
        {
            Id = "LibraryTool1",
            Title = "Library Tool 1",
            ToolTag = "LibraryToolTag"
        };

        var documentDock = new LibraryDocumentDock
        {
            Id = "LibraryDocumentDock",
            Title = "Library Documents",
            DockTag = "LibraryDocs",
            VisibleDockables = new List<IDockable> { document },
            ActiveDockable = document
        };

        var toolDock = new LibraryToolDock
        {
            Id = "LibraryToolDock",
            Title = "Library Tools",
            DockTag = "LibraryTools",
            VisibleDockables = new List<IDockable> { tool },
            ActiveDockable = tool
        };

        var floatingRoot = new LibraryRootDock
        {
            Id = "LibraryFloatingRoot",
            Title = "Library Floating Root",
            RootTag = "LibraryWindowRootTag",
            VisibleDockables = new List<IDockable>()
        };

        return new LibraryRootDock
        {
            Id = "LibraryRoot",
            Title = "Library Root",
            RootTag = "LibraryRootTag",
            VisibleDockables = new List<IDockable> { documentDock, toolDock },
            ActiveDockable = documentDock,
            Windows = new List<IDockWindow>
            {
                new LibraryDockWindow
                {
                    Id = "LibraryWindow1",
                    Title = "Library Window 1",
                    WindowTag = "LibraryWindowTag",
                    X = 10,
                    Y = 20,
                    Width = 800,
                    Height = 600,
                    Layout = floatingRoot
                }
            }
        };
    }

}

public sealed class RegisteredPayload
{
    public string? Name { get; set; }
}

public sealed class GenericRegisteredPayload<T>
{
    public T Value { get; set; } = default!;
}

public sealed class TypeNamedPayload
{
    [JsonPropertyName("$type")]
    public string? Kind { get; set; }

    public string? Name { get; set; }
}

public sealed class UnregisteredPayload
{
    public string? Name { get; set; }
}

public class CustomRootDock : RootDock
{
    public string? RootTag { get; set; }
}

public class CustomDocumentDock : DocumentDock
{
    public string? DockTag { get; set; }
}

public class CustomToolDock : ToolDock
{
    public string? DockTag { get; set; }
}

public class CustomDocument : Document
{
    public string? DocumentTag { get; set; }
}

public class CustomTool : Tool
{
    public string? ToolTag { get; set; }
}

public class CustomDockWindow : DockWindow
{
    public string? WindowTag { get; set; }
}

public sealed class CustomDocumentTemplate : IDocumentTemplate
{
    public object? Content { get; set; }

    public string? TemplateTag { get; set; }
}

public sealed class CustomConvertedTemplate : IDocumentTemplate
{
    [JsonConverter(typeof(LiteralObjectConverter))]
    public object? Content { get; set; }

    public string? TemplateTag { get; set; }
}

public sealed class LiteralObjectConverter : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.String
            ? reader.GetString()
            : throw new JsonException("Expected string content.");
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}
