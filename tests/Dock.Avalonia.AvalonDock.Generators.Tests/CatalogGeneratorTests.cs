using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Dock.Avalonia.AvalonDock.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Generators.Tests;

public class CatalogGeneratorTests
{
    [Fact]
    public void Generates_Layout_Factories_And_Instance_Application_Catalog()
    {
        RunResult result = Run("App", """
            namespace Example;
            public partial class App : Avalonia.Application { }
            public class Editor : AvalonDock.Layout.LayoutDocument { }
            internal class Tool : AvalonDock.Layout.LayoutAnchorable { }
            """);
        AssertNoErrors(result);
        Assert.Contains("new global::Example.Editor()", result.Source);
        Assert.Contains("new global::Example.Tool()", result.Source);
        Assert.Contains("AvalonDockCatalog { get; } = new __AvalonDockApplicationCatalog()", result.Source);
        Assert.DoesNotContain("Assembly.Load", result.Source);
        Assert.DoesNotContain("Activator", result.Source);
    }

    [Theory]
    [InlineData("public class Bad : AvalonDock.Layout.LayoutDocument { public Bad(int x) {} }", "ADG001")]
    [InlineData("public class Bad<T> : AvalonDock.Layout.LayoutDocument { }", "ADG001")]
    [InlineData("public class App : Avalonia.Application { }", "ADG004")]
    [InlineData("public class Unserialized : AvalonDock.Layout.LayoutElement { }", "ADG001")]
    [InlineData("file class LocalEditor : AvalonDock.Layout.LayoutDocument { }", "ADG001")]
    [InlineData("namespace Custom { public class LayoutDocument : AvalonDock.Layout.LayoutDocument { } }", "ADG005")]
    [InlineData("namespace A { public class Editor : AvalonDock.Layout.LayoutDocument {} } namespace B { public class Editor : AvalonDock.Layout.LayoutDocument {} }", "ADG002")]
    public void Reports_Actionable_Type_Diagnostics(string source, string id)
    {
        RunResult result = Run("App", source);
        Assert.Contains(result.Diagnostics, d => d.Id == id &&
            d.Location.SourceTree?.GetText(TestContext.Current.CancellationToken).ToString() == source);
        Assert.Empty(result.Output.GetDiagnostics(TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Abstract_Classes_Are_Not_Factories()
    {
        RunResult result = Run("App", "public abstract class Editor : AvalonDock.Layout.LayoutDocument { }");
        AssertNoErrors(result);
        Assert.DoesNotContain("new global::Editor()", result.Source);
    }

    [Fact]
    public void Resource_Dictionaries_Get_Direct_Compiled_Factory_Declarations()
    {
        RunResult result = Run("App", "public partial class App : Avalonia.Application {}",
            resources: [new TextFile("/project/Themes/Theme.axaml", "<ResourceDictionary xmlns='https://github.com/avaloniaui' />"),
                        new TextFile("/project/View.axaml", "<UserControl xmlns='https://github.com/avaloniaui' />")]);
        AssertNoErrors(result);
        Assert.Contains("CompiledResourceFactoryAttribute(\"avares://App/Themes/Theme.axaml\")", result.Source);
        Assert.DoesNotContain("avares://App/View.axaml", result.Source);
        Assert.DoesNotContain("AvaloniaXamlLoader.Load", result.Source);
    }

    [Fact]
    public void Linked_Resource_Uses_Logical_Path()
    {
        RunResult result = Run("App", "", resources: [new TextFile("/shared/Theme.axaml", "<ResourceDictionary xmlns='https://github.com/avaloniaui' />")],
            logicalPath: "Themes/Linked.axaml");
        AssertNoErrors(result);
        Assert.Contains("avares://App/Themes/Linked.axaml", result.Source);
    }

    [Fact]
    public void Malformed_Resource_Reports_Diagnostic()
    {
        RunResult result = Run("App", "", resources: [new TextFile("/project/Broken.axaml", "<ResourceDictionary")]);
        Assert.Contains(result.Diagnostics, d => d.Id == "ADG003");
    }

    [Fact]
    public void Referenced_Library_Catalogs_Are_Composed_Without_Runtime_Discovery()
    {
        RunResult library = Run("Library", "public class LibraryEditor : AvalonDock.Layout.LayoutDocument {}");
        MetadataReference reference = Emit(library.Output);
        RunResult app = Run("App", "public partial class App : Avalonia.Application {}", [reference], includeContracts: false);
        AssertNoErrors(app);
        Assert.Contains("new global::AvalonDock.Generated.Catalog_004c006900620072006100720079().RegisterLayoutTypes(registry)", app.Source);
    }

    [Fact]
    public void Cross_Assembly_Name_Collisions_Are_Diagnosed()
    {
        RunResult library = Run("Library", "namespace Library { public class Editor : AvalonDock.Layout.LayoutDocument {} }");
        RunResult app = Run("App", "public partial class App : Avalonia.Application {} public class Editor : AvalonDock.Layout.LayoutDocument {}",
            [Emit(library.Output)], includeContracts: false);
        Assert.Contains(app.Diagnostics, d => d.Id == "ADG002");
    }

    [Fact]
    public void Manual_Application_Provider_Is_Preserved()
    {
        RunResult result = Run("App", """
            public class App : Avalonia.Application, AvalonDock.Generated.IAvalonDockGeneratedCatalogProvider {
              public AvalonDock.Generated.IAvalonDockGeneratedCatalog AvalonDockCatalog => throw new System.NotImplementedException();
            }
            """);
        AssertNoErrors(result);
        Assert.DoesNotContain("__AvalonDockApplicationCatalog", result.Source);
    }

    [Fact]
    public void Nonresource_Additional_Files_Do_Not_Create_Compiled_Factories()
    {
        RunResult result = Run("App", "", markResources: false,
            resources: [new TextFile("/project/Design.axaml", "<ResourceDictionary xmlns='https://github.com/avaloniaui' />")]);
        AssertNoErrors(result);
        Assert.DoesNotContain("CompiledResourceFactoryAttribute", result.Source);
    }

    [Fact]
    public void Catalog_Optout_Leaves_Colliding_Types_And_Manual_Provider_Compilable()
    {
        RunResult result = Run("App", """
            public class App : Avalonia.Application, AvalonDock.Generated.IAvalonDockGeneratedCatalogProvider {
              public AvalonDock.Generated.IAvalonDockGeneratedCatalog AvalonDockCatalog => throw new System.NotImplementedException();
            }
            namespace A { public class Editor : AvalonDock.Layout.LayoutDocument {} }
            namespace B { public class Editor : AvalonDock.Layout.LayoutDocument {} }
            """, generateCatalog: false);
        AssertNoErrors(result);
        Assert.Empty(result.Source);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Case_Only_Resource_Differences_Are_Ambiguous_For_The_Compiled_Uri_Dispatcher()
    {
        RunResult result = Run("App", "", resources:
            [new TextFile("/project/Theme.axaml", "<ResourceDictionary xmlns='https://github.com/avaloniaui' />"),
             new TextFile("/project/theme.axaml", "<ResourceDictionary xmlns='https://github.com/avaloniaui' />")]);
        Assert.Contains(result.Diagnostics, d => d.Id == "ADG003");
    }

    private static void AssertNoErrors(RunResult result)
    {
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(result.Output.GetDiagnostics(TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    private static MetadataReference Emit(Compilation compilation)
    {
        using MemoryStream stream = new();
        var result = compilation.Emit(stream);
        Assert.True(result.Success, string.Join("\n", result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    private static RunResult Run(string name, string code, MetadataReference[]? extra = null,
        TextFile[]? resources = null, string? logicalPath = null, bool includeContracts = true,
        bool markResources = true, bool generateCatalog = true)
    {
        IEnumerable<MetadataReference> references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(static p => MetadataReference.CreateFromFile(p));
        if (extra is not null) references = references.Concat(extra);
        CSharpParseOptions parse = new(LanguageVersion.CSharp13);
        CSharpCompilation compilation = CSharpCompilation.Create(name,
            (includeContracts ? new[] { Contracts, code } : new[] { code }).Select(s => CSharpSyntaxTree.ParseText(s, parse)),
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new AvalonDockCatalogGenerator().AsSourceGenerator()],
            resources ?? [], parse, new OptionsProvider(logicalPath, markResources, generateCatalog));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> diagnostics);
        GeneratorRunResult result = Assert.Single(driver.GetRunResult().Results);
        Assert.Null(result.Exception);
        return new RunResult(output, string.Join("\n", result.GeneratedSources.Select(s => s.SourceText.ToString())), diagnostics);
    }

    private sealed record RunResult(Compilation Output, string Source, ImmutableArray<Diagnostic> Diagnostics);
    private sealed class TextFile(string path, string content) : AdditionalText
    {
        public override string Path => path;
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(content);
    }
    private sealed class OptionsProvider(string? logicalPath, bool markResources, bool generateCatalog) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(new()
        {
            ["build_property.MSBuildProjectDirectory"] = "/project",
            ["build_property.AvalonDockGenerateCatalog"] = generateCatalog ? "true" : "false"
        });
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new Options(new());
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new Options(!markResources ? new() : new() { ["build_metadata.AdditionalFiles.AvalonDockResourcePath"] = logicalPath ?? textFile.Path });
    }
    private sealed class Options(Dictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) => values.TryGetValue(key, out value!);
    }

    private const string Contracts = """
        namespace Avalonia { public class Application {} }
        namespace Avalonia.Controls { public class ResourceDictionary {} }
        namespace Avalonia.Markup.Xaml {
          [System.AttributeUsage(System.AttributeTargets.Method)]
          public class CompiledResourceFactoryAttribute(string uri) : System.Attribute {}
        }
        namespace AvalonDock.Layout {
          public class LayoutElement {}
          public class LayoutDocument : LayoutElement, System.Xml.Serialization.IXmlSerializable {
            public System.Xml.Schema.XmlSchema GetSchema() => null;
            public void ReadXml(System.Xml.XmlReader reader) {}
            public void WriteXml(System.Xml.XmlWriter writer) {}
          }
          public class LayoutAnchorable : LayoutDocument {}
        }
        namespace AvalonDock.Layout.Serialization {
          public class LayoutSerializationTypes { public void Register<T>(string name, System.Func<T> factory) where T : AvalonDock.Layout.LayoutElement, System.Xml.Serialization.IXmlSerializable {} }
        }
        namespace AvalonDock.Themes { public class ThemeResourceRegistry { public void Register(System.Uri uri, System.Func<Avalonia.Controls.ResourceDictionary> factory) {} } }
        namespace AvalonDock.Generated {
          public interface IAvalonDockGeneratedCatalog {
            void RegisterLayoutTypes(AvalonDock.Layout.Serialization.LayoutSerializationTypes types);
            void RegisterThemeResources(AvalonDock.Themes.ThemeResourceRegistry resources);
          }
          public interface IAvalonDockGeneratedCatalogProvider { IAvalonDockGeneratedCatalog AvalonDockCatalog { get; } }
          [System.AttributeUsage(System.AttributeTargets.Assembly)]
          public class AvalonDockGeneratedCatalogAttribute(System.Type type) : System.Attribute {}
          [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
          public class AvalonDockGeneratedLayoutAttribute(string name, System.Type type) : System.Attribute {}
        }
        """;
}
