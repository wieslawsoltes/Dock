using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Avalonia.Headless;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Mvvm.Core;
using Dock.Serializer.SystemTextJson;

[assembly: DockJsonSourceGeneration]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.RegisteredPayload))]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.CollectionPayload))]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.FieldPayload))]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.PolymorphicCollectionPayload))]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.RegisteredList<int>))]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.RegisteredList<string>))]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.RegisteredList<long>))]

namespace DockSystemTextJsonSourceGenAotSample;

internal static class Program
{
    private static int Main()
    {
        Avalonia.AppBuilder.Configure<BindingSmokeApp>()
            .UseHeadless(new Avalonia.Headless.AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();
        BindingSmokeChecks.ValidateProportionalSplitter(new ProportionalDockSplitter(), new ProportionalDockSplitter());
        BindingSmokeChecks.ValidateGridSplitter(new GridDockSplitter(), new GridDockSplitter());
        BindingSmokeChecks.ValidateMdiCommands();
        BindingSmokeChecks.ValidateOverlayServices();
        BindingSmokeChecks.ValidateCommandBarKinds();
        ValidateRoundTrip();
        ValidateCollectionRoundTrip<ObservableCollection<int>, ObservableCollection<string>, ObservableCollection<long>>(typeof(ObservableCollection<>));
        ValidateCollectionRoundTrip<List<int>, List<string>, List<long>>(typeof(List<>));
        ValidateCollectionRoundTrip<RegisteredList<int>, RegisteredList<string>, RegisteredList<long>>(typeof(RegisteredList<>));
        ValidateSuppliedCollectionFactories();
        ValidateIncludedMembers();
        ValidatePolymorphicCollections();
        Console.WriteLine("Dock Fluent bindings and source-generated AOT serialization round trip succeeded.");
        return 0;
    }

    private static void ValidateRoundTrip()
    {
        DockSerializer serializer = DockSystemTextJsonGenerated.CreateSerializer();
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

    private static void ValidateCollectionRoundTrip<TNumbers, TNames, TValues>(Type listType)
        where TNumbers : IList<int>
        where TNames : IList<string>
        where TValues : IList<long>
    {
        DockSerializer serializer = DockSystemTextJsonGenerated.CreateSerializer(listType);
        var source = new CollectionPayload
        {
            Numbers = new List<int> { 1, 2, 3 },
            Names = new List<string> { "first", "second" },
            Children = new Dictionary<string, CollectionChild>
            {
                ["child"] = new CollectionChild { Values = new List<long> { 42, 84 } }
            }
        };
        CollectionPayload restored = Require(serializer.Deserialize<CollectionPayload>(serializer.Serialize(source)),
            "Collection payload deserialization returned null.");
        Ensure(restored.Numbers is TNumbers && restored.Names is TNames && restored.Children["child"].Values is TValues,
            "Collection payload did not preserve the configured concrete list types.");
        Ensure(restored.Numbers.Count == 3 && restored.Numbers[0] == 1 && restored.Numbers[2] == 3,
            "Integer list values were not preserved.");
        Ensure(restored.Names.Count == 2 && restored.Names[0] == "first" && restored.Names[1] == "second",
            "String list values were not preserved.");
        Ensure(restored.Children["child"].Values.Count == 2 && restored.Children["child"].Values[0] == 42
            && restored.Children["child"].Values[1] == 84, "Nested list values were not preserved.");
    }

    private static void ValidateSuppliedCollectionFactories()
    {
        var resolver = new DockSystemTextJsonResolver().WithAddedModifier(info =>
        {
            if (info.Type == typeof(IList<int>))
                info.CreateObject = static () => new RegisteredList<int> { CreatedByResolver = true };
            else if (info.Type == typeof(IList<string>))
                info.CreateObject = static () => new RegisteredList<string> { CreatedByResolver = true };
            else if (info.Type == typeof(IList<long>))
                info.CreateObject = static () => new RegisteredList<long> { CreatedByResolver = true };
        });
        var serializer = new DockSerializer(typeof(RegisteredList<>), resolver);
        var source = new CollectionPayload
        {
            Numbers = new List<int> { 7 },
            Names = new List<string> { "custom" },
            Children = new Dictionary<string, CollectionChild>
            {
                ["child"] = new CollectionChild { Values = new List<long> { 123 } }
            }
        };
        CollectionPayload restored = Require(serializer.Deserialize<CollectionPayload>(serializer.Serialize(source)),
            "Custom collection factory deserialization returned null.");
        Ensure(restored.Numbers is RegisteredList<int> { CreatedByResolver: true }
            && restored.Names is RegisteredList<string> { CreatedByResolver: true }
            && restored.Children["child"].Values is RegisteredList<long> { CreatedByResolver: true },
            "Resolver-supplied collection factories were replaced.");
        Ensure(restored.Numbers[0] == 7 && restored.Names[0] == "custom" && restored.Children["child"].Values[0] == 123,
            "Resolver-created collection values were not preserved.");
    }

    private static void ValidateIncludedMembers()
    {
        DockSerializer serializer = DockSystemTextJsonGenerated.CreateSerializer();
        var source = new FieldPayload
        {
            Numbers = new List<double> { 1.5 },
            BaseNumbers = new List<float> { 2.5f },
            Values = new List<decimal> { 3.75m },
            BaseValues = new List<TimeSpan> { TimeSpan.FromMinutes(4) }
        };
        FieldPayload restored = Require(serializer.Deserialize<FieldPayload>(serializer.Serialize(source)),
            "Included member deserialization returned null.");
        Ensure(restored.Numbers is ObservableCollection<double> && restored.Numbers[0] == 1.5
            && restored.BaseNumbers is ObservableCollection<float> && restored.BaseNumbers[0] == 2.5f,
            "Included fields did not preserve values and configured collection types.");
        Ensure(restored.Values is ObservableCollection<decimal> && restored.Values[0] == 3.75m
            && restored.BaseValues is ObservableCollection<TimeSpan> && restored.BaseValues[0] == TimeSpan.FromMinutes(4),
            "Included non-public properties did not preserve values and configured collection types.");
    }

    private static void ValidatePolymorphicCollections()
    {
        DockSerializer serializer = DockSystemTextJsonGenerated.CreateSerializer();
        PolymorphicCollectionPayload source = new DerivedCollectionPayload { Values = new List<short> { 3, 7 } };
        DerivedCollectionPayload restored = RequireType<DerivedCollectionPayload>(
            serializer.Deserialize<PolymorphicCollectionPayload>(serializer.Serialize(source)),
            "Polymorphic collection payload did not preserve its derived type.");
        Ensure(restored.Values is ObservableCollection<short> && restored.Values.Count == 2
            && restored.Values[0] == 3 && restored.Values[1] == 7,
            "Polymorphic collection payload did not preserve values and configured collection types.");
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
    public string? RootTag { get; set; }
}

public class SampleDocumentDock : DocumentDock
{
    public string? DockTag { get; set; }
}

public class SampleToolDock : ToolDock
{
    public string? DockTag { get; set; }
}

public class SampleDocument : Document
{
    public string? DocumentTag { get; set; }
}

public class SampleTool : Tool
{
    public string? ToolTag { get; set; }
}

public class SampleDockWindow : DockWindow
{
    public string? WindowTag { get; set; }
}

public sealed class CollectionPayload
{
    public IList<int> Numbers { get; set; } = new List<int>();
    public IList<string> Names { get; set; } = new List<string>();
    public Dictionary<string, CollectionChild> Children { get; set; } = new();
}

public sealed class CollectionChild
{
    public IList<long> Values { get; set; } = new List<long>();
}

public sealed class RegisteredList<T> : Collection<T>
{
    public bool CreatedByResolver { get; set; }
}

public class FieldPayloadBase
{
    [JsonInclude] public IList<float> BaseNumbers = new List<float>();
    [JsonInclude] internal IList<TimeSpan> BaseValues { get; set; } = new List<TimeSpan>();
}

public sealed class FieldPayload : FieldPayloadBase
{
    [JsonInclude] public IList<double> Numbers = new List<double>();
    [JsonInclude] internal IList<decimal> Values { get; set; } = new List<decimal>();
}

[JsonDerivedType(typeof(DerivedCollectionPayload), "derived")]
public class PolymorphicCollectionPayload
{
}

public sealed class DerivedCollectionPayload : PolymorphicCollectionPayload
{
    public IList<short> Values { get; set; } = new List<short>();
}
