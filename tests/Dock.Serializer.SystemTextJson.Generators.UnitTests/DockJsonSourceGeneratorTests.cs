using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Dock.Model.Inpc.Controls;
using Dock.Serializer.SystemTextJson;
using Dock.Serializer.SystemTextJson.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Xunit;

namespace Dock.Serializer.SystemTextJson.Generators.UnitTests;

public class DockJsonSourceGeneratorTests
{
    [Fact]
    public void ActivationAttribute_ProducesGeneratedSources()
    {
        const string source = """
            using Dock.Model.Inpc.Controls;
            using Dock.Serializer.SystemTextJson;

            [assembly: DockJsonSourceGeneration]

            namespace Example;

            public class CustomDocument : Document
            {
                public string? Category { get; set; }
            }
            """;

        CompilationRun run = Run(source);
        GeneratorRunResult result = Assert.Single(run.RunResult.Results);

        Assert.Empty(result.Diagnostics);
        Assert.Contains(result.GeneratedSources, x => x.HintName == "DockSystemTextJsonContext.g.cs");
        Assert.Contains(result.GeneratedSources, x => x.HintName == "DockSystemTextJsonGenerated.g.cs");
    }

    [Fact]
    public void AutoDiscovery_IncludesCustomDockTypes()
    {
        const string source = """
            using Dock.Model.Inpc.Controls;
            using Dock.Model.Inpc.Core;
            using Dock.Serializer.SystemTextJson;

            [assembly: DockJsonSourceGeneration]

            namespace Example;

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
            """;

        CompilationRun run = Run(source);
        string contextSource = GetGeneratedSource(run, "DockSystemTextJsonContext.g.cs");
        string generatedSource = GetGeneratedSource(run, "DockSystemTextJsonGenerated.g.cs");

        Assert.Contains("global::Example.CustomRootDock", contextSource);
        Assert.Contains("global::Example.CustomDocumentDock", contextSource);
        Assert.Contains("global::Example.CustomToolDock", contextSource);
        Assert.Contains("global::Example.CustomDocument", generatedSource);
        Assert.Contains("global::Example.CustomTool", generatedSource);
        Assert.Contains("global::Example.CustomDockWindow", generatedSource);
    }

    [Fact]
    public void ExplicitRegistration_IncludesObjectPayloadType()
    {
        const string source = """
            using Dock.Model.Inpc.Controls;
            using Dock.Serializer.SystemTextJson;

            [assembly: DockJsonSourceGeneration]
            [assembly: DockJsonSerializable(typeof(Example.CustomPayload))]

            namespace Example;

            public class CustomDocument : Document
            {
            }

            public sealed class CustomPayload
            {
                public string? Name { get; set; }
            }
            """;

        CompilationRun run = Run(source);
        string contextSource = GetGeneratedSource(run, "DockSystemTextJsonContext.g.cs");
        string generatedSource = GetGeneratedSource(run, "DockSystemTextJsonGenerated.g.cs");

        Assert.Contains("global::Example.CustomPayload", contextSource);
        Assert.Contains("ObjectPayloadConverter", generatedSource);
        Assert.Contains("CreateObjectPayloadDiscriminators", generatedSource);
        Assert.Contains("typeof(global::Example.CustomPayload)", generatedSource);
    }

    [Fact]
    public void ClosedGenericRegistrations_DoNotCollide()
    {
        const string source = """
            using Dock.Serializer.SystemTextJson;

            [assembly: DockJsonSourceGeneration]
            [assembly: DockJsonSerializable(typeof(Example.Payload<int>))]
            [assembly: DockJsonSerializable(typeof(Example.Payload<string>))]

            namespace Example;

            public sealed class Payload<T>
            {
                public T Value { get; set; } = default!;
            }
            """;

        CompilationRun run = Run(source);
        GeneratorRunResult result = Assert.Single(run.RunResult.Results);
        string contextSource = GetGeneratedSource(run, "DockSystemTextJsonContext.g.cs");
        string generatedSource = GetGeneratedSource(run, "DockSystemTextJsonGenerated.g.cs");

        Assert.DoesNotContain(result.Diagnostics, x => x.Id == "DSTJ003");
        Assert.Contains("Payload<int>", contextSource);
        Assert.Contains("Payload<string>", contextSource);
        Assert.Contains("Payload<int>", generatedSource);
        Assert.Contains("Payload<string>", generatedSource);
    }

    [Fact]
    public void AutoDiscovery_IncludesProtectedInternalNestedDockTypes()
    {
        const string source = """
            using Dock.Model.Inpc.Controls;
            using Dock.Serializer.SystemTextJson;

            [assembly: DockJsonSourceGeneration]

            namespace Example;

            public static class Container
            {
                protected internal sealed class NestedDocument : Document
                {
                    public string? NestedTag { get; set; }
                }
            }
            """;

        CompilationRun run = Run(source);
        string contextSource = GetGeneratedSource(run, "DockSystemTextJsonContext.g.cs");
        string generatedSource = GetGeneratedSource(run, "DockSystemTextJsonGenerated.g.cs");

        Assert.Contains("global::Example.Container.NestedDocument", contextSource);
        Assert.Contains("global::Example.Container.NestedDocument", generatedSource);
    }

    [Fact]
    public void DuplicateDiscriminator_ReportsDiagnostic()
    {
        PortableExecutableReference duplicateReference = CreateAliasedReference(
            """
            using Dock.Model.Inpc.Controls;

            namespace Example;

            public class DuplicateDocument : Document
            {
            }
            """,
            "DuplicateReference",
            "RefA");

        const string source = """
            extern alias RefA;
            using Dock.Model.Inpc.Controls;
            using Dock.Serializer.SystemTextJson;

            [assembly: DockJsonSourceGeneration]
            [assembly: DockJsonSerializable(typeof(RefA::Example.DuplicateDocument))]

            namespace Example;

            public class DuplicateDocument : Document
            {
            }
            """;

        CompilationRun run = Run(source, duplicateReference);
        GeneratorRunResult result = Assert.Single(run.RunResult.Results);
        Diagnostic diagnostic = Assert.Single(result.Diagnostics.Where(x => x.Id == "DSTJ003"));

        Assert.Contains("Example.DuplicateDocument", diagnostic.GetMessage());
    }

    [Fact]
    public void OpenGenericRegistration_ReportsDiagnostic()
    {
        const string source = """
            using Dock.Serializer.SystemTextJson;

            [assembly: DockJsonSourceGeneration]
            [assembly: DockJsonSerializable(typeof(Example.Payload<>))]

            namespace Example;

            public sealed class Payload<T>
            {
            }
            """;

        CompilationRun run = Run(source);
        GeneratorRunResult result = Assert.Single(run.RunResult.Results);

        Assert.Contains(result.Diagnostics, x => x.Id == "DSTJ001");
    }

    [Fact]
    public void ScalarPayloadRegistration_ReportsDiagnostic()
    {
        const string source = """
            using Dock.Serializer.SystemTextJson;

            [assembly: DockJsonSourceGeneration]
            [assembly: DockJsonSerializable(typeof(int))]

            namespace Example;
            """;

        CompilationRun run = Run(source);
        GeneratorRunResult result = Assert.Single(run.RunResult.Results);

        Diagnostic diagnostic = Assert.Single(result.Diagnostics.Where(x => x.Id == "DSTJ001"));
        Assert.Contains("int", diagnostic.GetMessage(), StringComparison.Ordinal);
    }

    private static string GetGeneratedSource(CompilationRun run, string hintName)
    {
        GeneratorRunResult result = Assert.Single(run.RunResult.Results);
        return Assert.Single(result.GeneratedSources.Where(x => x.HintName == hintName)).SourceText.ToString();
    }

    private static CompilationRun Run(string source, params MetadataReference[] additionalReferences)
    {
        CSharpCompilation compilation = CreateCompilation(
            assemblyName: "DockGeneratorConsumer",
            source: source,
            additionalReferences: additionalReferences);

        var parseOptions = (CSharpParseOptions)compilation.SyntaxTrees[0].Options;
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { new DockJsonSourceGenerator().AsSourceGenerator() },
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);

        return new CompilationRun(driver.GetRunResult(), outputCompilation);
    }

    private static PortableExecutableReference CreateAliasedReference(string source, string assemblyName, string alias)
    {
        CSharpCompilation compilation = CreateCompilation(assemblyName, source);
        using var stream = new MemoryStream();
        EmitResult emitResult = compilation.Emit(stream);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));

        return MetadataReference.CreateFromImage(
            stream.ToArray(),
            MetadataReferenceProperties.Assembly.WithAliases(ImmutableArray.Create(alias)));
    }

    private static CSharpCompilation CreateCompilation(
        string assemblyName,
        string source,
        params MetadataReference[] additionalReferences)
    {
        CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        var references = new List<MetadataReference>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string? trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        Assert.False(string.IsNullOrWhiteSpace(trustedPlatformAssemblies));

        foreach (string path in trustedPlatformAssemblies!.Split(Path.PathSeparator))
        {
            if (seenPaths.Add(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        foreach (Assembly assembly in new[]
                 {
                     typeof(DockJsonSourceGenerationAttribute).Assembly,
                     typeof(Document).Assembly
                 })
        {
            if (seenPaths.Add(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        foreach (MetadataReference reference in additionalReferences)
        {
            references.Add(reference);
        }

        return CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private sealed record CompilationRun(GeneratorDriverRunResult RunResult, Compilation OutputCompilation);
}
