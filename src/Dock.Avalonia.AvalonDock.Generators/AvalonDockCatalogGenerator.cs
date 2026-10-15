using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dock.Avalonia.AvalonDock.Generators;

/// <summary>Generates typed custom-layout and compiled-resource catalogs.</summary>
[Generator]
public sealed class AvalonDockCatalogGenerator : IIncrementalGenerator
{
    private const string CatalogNamespace = "AvalonDock.Generated";
    private static readonly DiagnosticDescriptor InvalidType = new("ADG001", "Layout factory cannot be generated",
        "Layout type '{0}' needs an accessible non-generic declaration, IXmlSerializable, and a public parameterless constructor; register a typed factory explicitly when supported", "AvalonDock", DiagnosticSeverity.Warning, true);
    private static readonly DiagnosticDescriptor DuplicateName = new("ADG002", "Ambiguous layout XML name",
        "XML name '{0}' is used by multiple layout types; rename the types or set AvalonDockGenerateCatalog=false and use explicit registrations with unique names", "AvalonDock", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor InvalidResource = new("ADG003", "Invalid compiled resource input",
        "Resource '{0}' cannot be cataloged: {1}", "AvalonDock", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor InvalidApplication = new("ADG004", "Application catalog cannot be attached",
        "Application '{0}' must be a non-nested, non-generic partial class to receive its generated AvalonDock catalog", "AvalonDock", DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor ReservedName = new("ADG005", "Reserved layout XML name",
        "Layout type '{0}' uses reserved XML name '{1}'; register its factory explicitly under a unique name", "AvalonDock", DiagnosticSeverity.Warning, true);

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol?> types = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
            static (syntax, cancellation) => syntax.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)syntax.Node, cancellation));
        IncrementalValuesProvider<ResourceInput> resources = context.AdditionalTextsProvider
            .Where(static text => text.Path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase) || text.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (input, cancellation) =>
            {
                input.Right.GetOptions(input.Left).TryGetValue("build_metadata.AdditionalFiles.AvalonDockResourcePath", out string? logicalPath);
                input.Right.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out string? projectDirectory);
                return new ResourceInput(input.Left.Path, logicalPath, projectDirectory, input.Left.GetText(cancellation)?.ToString());
            })
            .Where(static input => !string.IsNullOrEmpty(input.LogicalPath));
        IncrementalValueProvider<bool> enabled = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
            !options.GlobalOptions.TryGetValue("build_property.AvalonDockGenerateCatalog", out string? value) ||
            !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase));
        context.RegisterSourceOutput(context.CompilationProvider.Combine(types.Collect()).Combine(resources.Collect()).Combine(enabled),
            static (production, input) =>
            {
                if (input.Right) Generate(production, input.Left.Left.Left, input.Left.Left.Right, input.Left.Right);
            });
    }

    private static void Generate(SourceProductionContext context, Compilation compilation,
        ImmutableArray<INamedTypeSymbol?> declarations, ImmutableArray<ResourceInput> inputs)
    {
        if (compilation.GetTypeByMetadataName("AvalonDock.Layout.LayoutElement") is null ||
            compilation.AssemblyName == "Dock.Avalonia.AvalonDock") return;
        List<INamedTypeSymbol> layouts = new();
        List<INamedTypeSymbol> applications = new();
        HashSet<ISymbol> visited = new(SymbolEqualityComparer.Default);
        Dictionary<string, INamedTypeSymbol> names = new(StringComparer.Ordinal);
        foreach (INamedTypeSymbol? type in declarations)
        {
            if (type is null || !visited.Add(type)) continue;
            if (DerivesFrom(type, "Avalonia.Application") && !type.IsAbstract)
                applications.Add(type);
            if (!DerivesFrom(type, "AvalonDock.Layout.LayoutElement") || type.IsAbstract) continue;
            if (!CanConstruct(type) || DerivesFrom(type, "AvalonDock.Layout.LayoutRoot") ||
                !type.AllInterfaces.Any(static i => i.ToDisplayString() == "System.Xml.Serialization.IXmlSerializable"))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidType, type.Locations.FirstOrDefault(), type.ToDisplayString()));
                continue;
            }
            if (IsReserved(type.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(ReservedName, type.Locations.FirstOrDefault(), type.ToDisplayString(), type.Name));
                continue;
            }
            if (names.ContainsKey(type.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(DuplicateName, type.Locations.FirstOrDefault(), type.Name));
                continue;
            }
            names.Add(type.Name, type);
            layouts.Add(type);
        }
        layouts.Sort(static (a, b) => string.CompareOrdinal(a.ToDisplayString(), b.ToDisplayString()));
        List<string> resourceUris = ReadResources(context, compilation.AssemblyName!, inputs);
        string catalogName = "Catalog_" + Identifier(compilation.AssemblyName!);
        string catalogType = "global::" + CatalogNamespace + "." + catalogName;
        StringBuilder source = new("// <auto-generated/>\n#nullable enable\n");
        source.Append("[assembly: global::AvalonDock.Generated.AvalonDockGeneratedCatalogAttribute(typeof(").Append(catalogType).Append("))]\n");
        foreach (INamedTypeSymbol type in layouts)
            source.Append("[assembly: global::AvalonDock.Generated.AvalonDockGeneratedLayoutAttribute(").Append(Literal(type.Name)).Append(", typeof(").Append(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Append("))]\n");
        source.Append("namespace ").Append(CatalogNamespace).Append(" {\n");
        source.Append("/// <summary>Compile-time factories for this assembly.</summary>\n[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]\npublic sealed class ").Append(catalogName).Append(" : global::AvalonDock.Generated.IAvalonDockGeneratedCatalog {\n");
        source.Append("/// <inheritdoc/>\npublic void RegisterLayoutTypes(global::AvalonDock.Layout.Serialization.LayoutSerializationTypes types) {\n");
        foreach (INamedTypeSymbol type in layouts)
            source.Append("types.Register(").Append(Literal(type.Name)).Append(", static () => new ").Append(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Append("());\n");
        source.Append("}\n/// <inheritdoc/>\npublic void RegisterThemeResources(global::AvalonDock.Themes.ThemeResourceRegistry resources) {\n");
        for (int i = 0; i < resourceUris.Count; i++)
            source.Append("resources.Register(new global::System.Uri(").Append(Literal(resourceUris[i])).Append("), static () => (global::Avalonia.Controls.ResourceDictionary)CreateResource").Append(i).Append("());\n");
        source.Append("}\n");
        for (int i = 0; i < resourceUris.Count; i++)
            source.Append("[global::Avalonia.Markup.Xaml.CompiledResourceFactoryAttribute(").Append(Literal(resourceUris[i])).Append(")]\nprivate static object CreateResource").Append(i).Append("() => throw new global::System.InvalidOperationException(\"Avalonia XAML compilation is required for generated resource factories.\");\n");
        source.Append("}\n}\n");
        List<string> referencedCatalogs = FindReferencedCatalogs(compilation);
        if (applications.Count != 0)
            ValidateReferencedNames(context, compilation, names);
        foreach (INamedTypeSymbol application in applications)
        {
            if (application.AllInterfaces.Any(static i => i.ToDisplayString() == "AvalonDock.Generated.IAvalonDockGeneratedCatalogProvider")) continue;
            if (application.ContainingType is not null || application.IsGenericType ||
                application.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is ClassDeclarationSyntax c && !c.Modifiers.Any(SyntaxKind.PartialKeyword)))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidApplication, application.Locations.FirstOrDefault(), application.Name));
                continue;
            }
            bool hasNamespace = !application.ContainingNamespace.IsGlobalNamespace;
            if (hasNamespace) source.Append("namespace ").Append(application.ContainingNamespace.ToDisplayString()).Append(" {\n");
            source.Append("partial class @").Append(application.Name).Append(" : global::AvalonDock.Generated.IAvalonDockGeneratedCatalogProvider {\n");
            source.Append("global::AvalonDock.Generated.IAvalonDockGeneratedCatalog global::AvalonDock.Generated.IAvalonDockGeneratedCatalogProvider.AvalonDockCatalog { get; } = new __AvalonDockApplicationCatalog();\n");
            source.Append("private sealed class __AvalonDockApplicationCatalog : global::AvalonDock.Generated.IAvalonDockGeneratedCatalog {\n");
            foreach (string method in new[] { "RegisterLayoutTypes", "RegisterThemeResources" })
            {
                string registry = method == "RegisterLayoutTypes" ? "global::AvalonDock.Layout.Serialization.LayoutSerializationTypes" : "global::AvalonDock.Themes.ThemeResourceRegistry";
                source.Append("public void ").Append(method).Append('(').Append(registry).Append(" registry) {\n");
                foreach (string referenced in referencedCatalogs)
                    source.Append("new ").Append(referenced).Append("().").Append(method).Append("(registry);\n");
                source.Append("new ").Append(catalogType).Append("().").Append(method).Append("(registry);\n}\n");
            }
            source.Append("}\n}\n");
            if (hasNamespace) source.Append("}\n");
        }
        context.AddSource("AvalonDockCatalog.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private static List<string> FindReferencedCatalogs(Compilation compilation)
    {
        SortedSet<string> result = new(StringComparer.Ordinal);
        foreach (IAssemblySymbol assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        foreach (AttributeData attribute in assembly.GetAttributes())
            if (attribute.AttributeClass?.ToDisplayString() == "AvalonDock.Generated.AvalonDockGeneratedCatalogAttribute" &&
                attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Value is INamedTypeSymbol type)
                result.Add(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        return result.ToList();
    }

    private static void ValidateReferencedNames(SourceProductionContext context, Compilation compilation, Dictionary<string, INamedTypeSymbol> names)
    {
        foreach (IAssemblySymbol assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        foreach (AttributeData attribute in assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != "AvalonDock.Generated.AvalonDockGeneratedLayoutAttribute" ||
                attribute.ConstructorArguments.Length != 2 || attribute.ConstructorArguments[0].Value is not string name ||
                attribute.ConstructorArguments[1].Value is not INamedTypeSymbol type) continue;
            if (names.TryGetValue(name, out INamedTypeSymbol? existing) && !SymbolEqualityComparer.Default.Equals(type, existing))
                context.ReportDiagnostic(Diagnostic.Create(DuplicateName, Location.None, name));
            else names[name] = type;
        }
    }

    private static List<string> ReadResources(SourceProductionContext context, string assemblyName, ImmutableArray<ResourceInput> inputs)
    {
        SortedSet<string> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (ResourceInput input in inputs)
        {
            if (input.Text is null) continue;
            try
            {
                using StringReader text = new(input.Text);
                using XmlReader xml = XmlReader.Create(text, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
                XElement? root = XDocument.Load(xml).Root;
                if (root?.Name.LocalName != "ResourceDictionary" || root.Name.NamespaceName != "https://github.com/avaloniaui") continue;
                if (string.Equals(root.Attribute(XName.Get("Precompile", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value,
                    "false", StringComparison.OrdinalIgnoreCase)) continue;
                string path = string.IsNullOrEmpty(input.LogicalPath) ? input.Path : input.LogicalPath!;
                if (Path.IsPathRooted(path))
                {
                    if (string.IsNullOrEmpty(input.ProjectDirectory)) throw new InvalidOperationException("MSBuildProjectDirectory is required for absolute resource paths");
                    Uri project = new(Path.GetFullPath(input.ProjectDirectory!).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
                    path = Uri.UnescapeDataString(project.MakeRelativeUri(new Uri(Path.GetFullPath(path))).ToString());
                }
                path = path.Replace('\\', '/').TrimStart('/');
                if (path.StartsWith("../", StringComparison.Ordinal)) throw new InvalidOperationException("linked resources outside the project require Link metadata");
                string uri = "avares://" + assemblyName + "/" + path;
                if (!result.Add(uri)) throw new InvalidOperationException("duplicate resource URI");
            }
            catch (Exception e) when (e is XmlException or ArgumentException or InvalidOperationException)
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidResource, Location.None, input.Path, e.Message));
            }
        }
        return result.ToList();
    }

    private static bool DerivesFrom(INamedTypeSymbol type, string metadataName)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
            if (current.ToDisplayString() == metadataName) return true;
        return false;
    }

    private static bool CanConstruct(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
            if (current.IsFileLocal || current.IsGenericType || current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)) return false;
        return type.InstanceConstructors.Any(static c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);
    }

    private static bool IsReserved(string name) => name is
        "LayoutRoot" or "RootPanel" or "TopSide" or "RightSide" or "LeftSide" or "BottomSide" or "Hidden" or "FloatingWindows" or
        "LayoutPanel" or "LayoutDocumentPaneGroup" or "LayoutAnchorablePaneGroup" or "LayoutDocumentPane" or
        "LayoutAnchorablePane" or "LayoutDocument" or "LayoutAnchorable" or "LayoutAnchorGroup" or "LayoutAnchorSide" or
        "LayoutDocumentFloatingWindow" or "LayoutAnchorableFloatingWindow" or "LayoutGroupFloatingWindow";

    private static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);
    private static string Identifier(string name)
    {
        StringBuilder result = new();
        foreach (char c in name) result.Append(((int)c).ToString("x4"));
        return result.ToString();
    }

    private sealed class ResourceInput
    {
        public ResourceInput(string path, string? logicalPath, string? projectDirectory, string? text)
        { Path = path; LogicalPath = logicalPath; ProjectDirectory = projectDirectory; Text = text; }
        public string Path { get; }
        public string? LogicalPath { get; }
        public string? ProjectDirectory { get; }
        public string? Text { get; }
    }
}
