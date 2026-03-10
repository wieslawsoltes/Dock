using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Inpc.Controls;
using Dock.Model.Inpc.Core;
using Dock.Serializer.SystemTextJson;

[assembly: DockJsonSourceGeneration]
[assembly: DockJsonSerializable(typeof(DockSystemTextJsonSourceGenAotSample.RegisteredPayload))]

namespace DockSystemTextJsonSourceGenAotSample;

internal static class Program
{
    private static int Main()
    {
        ValidateRoundTrip();
        Console.WriteLine("Dock source-generated AOT serialization round trip succeeded.");
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
