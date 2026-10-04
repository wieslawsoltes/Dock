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
    public void PackageGenerator_IsolatedFromSameNameGeneratorInCompilerHost()
    {
        _ = new global::System.Text.Json.SourceGeneration.JsonSourceGenerator();
        string package = Path.Combine(Path.GetTempPath(), "dock-generator-" + Guid.NewGuid().ToString("N"));
        string runtime = Path.Combine(package, "lib", "net10.0", "System.Text.Json.dll");
        string analyzer = Path.Combine(package, "analyzers", "dotnet", "roslyn4.4", "cs", "System.Text.Json.SourceGeneration.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(runtime)!);
        Directory.CreateDirectory(Path.GetDirectoryName(analyzer)!);
        try
        {
            File.Copy(typeof(global::System.Text.Json.JsonSerializer).Assembly.Location, runtime);
            // A different package implementation with the same simple assembly name
            // must be selected even when the host already loaded its own generator.
            CSharpCompilation packageGenerator = CreateCompilation("System.Text.Json.SourceGeneration", """"
                using Microsoft.CodeAnalysis;
                [assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
                public sealed class JsonSourceGenerator : ISourceGenerator
                {
                    public void Initialize(GeneratorInitializationContext context) { }
                    public void Execute(GeneratorExecutionContext context) => context.AddSource("PackageContext.g.cs", """
                        #nullable enable
                        namespace Dock.Serializer.SystemTextJson;
                        internal partial class DockSerializerGeneratedJsonContext
                        {
                            public DockSerializerGeneratedJsonContext() : base(null) { }
                            public const string SelectedPackage = "isolated package";
                            public static DockSerializerGeneratedJsonContext Default { get; } = new();
                            protected override global::System.Text.Json.JsonSerializerOptions? GeneratedSerializerOptions => null;
                            public override global::System.Text.Json.Serialization.Metadata.JsonTypeInfo? GetTypeInfo(global::System.Type type) => null;
                        }
                        """);
                }
                """");
            EmitResult emitted = packageGenerator.Emit(analyzer, cancellationToken: TestContext.Current.CancellationToken);
            Assert.True(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));

            CSharpCompilation compilation = CreateCompilation("PackageConsumer", """
                namespace Example;
                public sealed class CustomDocument : Dock.Model.Inpc.Controls.Document { }
                """);
            MetadataReference original = Assert.Single(compilation.References, r => r.Display?.EndsWith("System.Text.Json.dll", StringComparison.Ordinal) == true);
            compilation = compilation.ReplaceReference(original, MetadataReference.CreateFromFile(runtime));
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new DockJsonSourceGenerator().AsSourceGenerator() },
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees[0].Options);
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _, TestContext.Current.CancellationToken);
            var run = new CompilationRun(driver.GetRunResult(), output);
            Assert.Contains("isolated package", GetGeneratedSource(run, "SystemTextJson.PackageContext.g.cs"));
            using var stream = new MemoryStream();
            EmitResult result = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        }
        finally
        {
            // Windows may retain the loaded analyzer until the test host exits.
            try { Directory.Delete(package, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    [Fact]
    public void BrokenPackageGenerator_ProducesBuildErrorInsteadOfMissingMetadata()
    {
        string package = Path.Combine(Path.GetTempPath(), "dock-broken-generator-" + Guid.NewGuid().ToString("N"));
        string runtime = Path.Combine(package, "lib", "net10.0", "System.Text.Json.dll");
        string analyzer = Path.Combine(package, "analyzers", "dotnet", "roslyn4.4", "cs", "System.Text.Json.SourceGeneration.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(runtime)!);
        Directory.CreateDirectory(Path.GetDirectoryName(analyzer)!);
        try
        {
            File.Copy(typeof(global::System.Text.Json.JsonSerializer).Assembly.Location, runtime);
            File.WriteAllText(analyzer, "not a managed assembly");
            CSharpCompilation compilation = CreateCompilation("BrokenPackageConsumer", """
                namespace Example;
                public sealed class CustomDocument : Dock.Model.Inpc.Controls.Document { }
                """);
            MetadataReference original = Assert.Single(compilation.References, r => r.Display?.EndsWith("System.Text.Json.dll", StringComparison.Ordinal) == true);
            compilation = compilation.ReplaceReference(original, MetadataReference.CreateFromFile(runtime));
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new DockJsonSourceGenerator().AsSourceGenerator() },
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees[0].Options);
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _, TestContext.Current.CancellationToken);
            Diagnostic diagnostic = Assert.Single(driver.GetRunResult().Diagnostics, d => d.Id == "DSTJ005");
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        }
        finally
        {
            Directory.Delete(package, recursive: true);
        }
    }

    [Fact]
    public void OpenNestedSerializerTypeArguments_DoNotLeakIntoGeneratedCode()
    {
        CompilationRun run = Run("""
            using System.Collections.Generic;
            using Dock.Serializer.SystemTextJson;
            using Dock.Model.Inpc.Controls;
            namespace Example;
            public sealed class CustomDocument : Document { }
            public static class Usage
            {
                public static string Save<T>(List<List<T>> values) => new DockSerializer().Serialize(values);
            }
            """);
        using var stream = new MemoryStream();
        EmitResult result = run.OutputCompilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [Fact]
    public void PublicFieldsAndCollectionInterfaces_ProduceCompilableDescriptors()
    {
        CompilationRun run = Run("""
            using System;
            using System.Collections.Generic;
            using System.Runtime.Serialization;
            using Dock.Serializer.SystemTextJson;
            using Dock.Model.Inpc.Controls;
            namespace Example;
            public sealed class Payload { public string? Value; }
            public sealed class PayloadList : List<Payload> { }
            public sealed class CustomDocument : Document
            {
                [DataMember] public PayloadList? Items { get; set; }
                [DataMember] public HashSet<Payload>? Set { get; set; }
            }
            public static class Usage
            {
                public static Guid[]? Load(string text) => new DockSerializer().Deserialize<Guid[]>(text);
            }
            """);
        using var stream = new MemoryStream();
        EmitResult result = run.OutputCompilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        string generated = GetGeneratedSource(run, "DockSystemTextJsonGenerated.g.cs");
        Assert.Contains("elementType: typeof(global::Example.Payload)", generated);
        Assert.Contains("elementType: typeof(global::System.Guid)", generated);
    }

    [Fact]
    public void ReferencedTypeWithInternalInterfaces_ProducesCompilableMetadata()
    {
        MetadataReference reference = CreateAliasedReference("""
            using Dock.Model.Inpc.Controls;
            namespace External;
            internal interface IHidden { }
            internal interface IHiddenGeneric<T> { }
            internal sealed class HiddenType { }
            public interface IPublic<T> { }
            public class ExternalDocument : Document, IHidden, IHiddenGeneric<string>, IPublic<HiddenType> { }
            """, "ExternalModels", "global");
        CompilationRun run = Run("""
            namespace Example;
            public sealed class CustomDocument : External.ExternalDocument { }
            """, reference);

        Assert.DoesNotContain(run.RunResult.Diagnostics, x => x.Severity == DiagnosticSeverity.Error);
        string generated = GetGeneratedSource(run, "DockSystemTextJsonGenerated.g.cs");
        Assert.DoesNotContain("typeof(global::External.IHidden", generated);
        Assert.DoesNotContain("typeof(global::External.IPublic<global::External.HiddenType>)", generated);
        using var stream = new MemoryStream();
        EmitResult result = run.OutputCompilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [Fact]
    public void DockKeyedDictionary_UsesCompatibleSystemTextJsonGenerator()
    {
        CompilationRun run = Run("""
            using System.Collections.Generic;
            using System.Runtime.Serialization;
            using Dock.Model.Core;
            using Dock.Model.Inpc.Controls;
            namespace Example;
            public sealed class CustomDocument : Document
            {
                [IgnoreDataMember]
                public IDictionary<IDockable, object>? Index { get; set; }
            }
            """);

        using var stream = new MemoryStream();
        EmitResult result = run.OutputCompilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [Fact]
    public void DefaultSerializer_GeneratesAndRegistersMetadataWithoutActivation()
    {
        const string source = """
            using Dock.Serializer.SystemTextJson;
            namespace Example;
            internal sealed class Payload { public string? Name { get; set; } }
            public static class Usage
            {
                public static string Save() => new DockSerializer().Serialize(new Payload { Name = "Saved" });
            }
            """;

        CompilationRun run = Run(source);
        Assert.DoesNotContain(run.RunResult.Diagnostics, x => x.Severity == DiagnosticSeverity.Error);
        Assert.Contains("global::Example.Payload", GetGeneratedSource(run, "DockSystemTextJsonContext.g.cs"));
        string generated = GetGeneratedSource(run, "DockSystemTextJsonGenerated.g.cs");
        Assert.Contains("ModuleInitializer", generated);
        Assert.Contains("DockJsonMetadata.Register", generated);
        Assert.Contains("new global::System.Collections.ObjectModel.ObservableCollection<", generated);
        Assert.DoesNotContain("Activator", generated);
        Assert.DoesNotContain("MakeGenericType", generated);
    }

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
    public void ApplicationContextWithGeneratedSimpleName_DoesNotCollideWithDockContext()
    {
        const string source = """
            using Dock.Serializer.SystemTextJson;
            using System.Text.Json.Serialization;

            [assembly: DockJsonSourceGeneration]

            namespace Example;

            [JsonSerializable(typeof(string))]
            internal sealed partial class DockSerializerGeneratedJsonContext : JsonSerializerContext
            {
            }
            """;

        CompilationRun run = Run(source);
        GeneratorRunResult result = Assert.Single(run.RunResult.Results);
        Assert.DoesNotContain(run.RunResult.Diagnostics, x => x.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.Diagnostics, x => x.Severity == DiagnosticSeverity.Error);
        string contextSource = GetGeneratedSource(run, "DockSystemTextJsonContext.g.cs");
        string generatedSource = GetGeneratedSource(run, "DockSystemTextJsonGenerated.g.cs");

        Assert.Contains("DockSerializerGeneratedJsonContext_1", contextSource);
        Assert.Contains("DockSerializerGeneratedJsonContext_1.Default", generatedSource);
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
        // Roslyn's compiler host normally loads this analyzer. The in-process test
        // host must also load it so emitted contexts can be compiled, not just inspected.
        _ = new global::System.Text.Json.SourceGeneration.JsonSourceGenerator();
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
            // This is a generator implementation, not a consumer reference. It
            // embeds STJ helper types that would conflict with the runtime types.
            if (Path.GetFileName(path) == "System.Text.Json.SourceGeneration.dll")
            {
                continue;
            }
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
