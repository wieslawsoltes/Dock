// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dock.Serializer.SystemTextJson.Generators;

[Generator]
public sealed partial class DockJsonSourceGenerator : IIncrementalGenerator
{
    private const string GeneratedContextTypeNameBase = "DockSerializerGeneratedJsonContext";
    private const string GeneratedContextNamespace = "Dock.Serializer.SystemTextJson";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<GenerationModel> modelProvider =
            context.CompilationProvider.Select(static (compilation, cancellationToken) =>
                GenerationModelBuilder.Build(compilation, cancellationToken));

        context.RegisterSourceOutput(modelProvider, static (productionContext, model) =>
        {
            foreach (Diagnostic diagnostic in model.Diagnostics)
            {
                productionContext.ReportDiagnostic(diagnostic);
            }

            if (!model.ShouldGenerate)
            {
                return;
            }

            productionContext.AddSource(
                "DockSystemTextJsonContext.g.cs",
                SourceText.From(model.ContextSource, Encoding.UTF8));

            foreach (GeneratedSourceArtifact additionalSource in model.AdditionalSources)
            {
                productionContext.AddSource(
                    additionalSource.HintName,
                    SourceText.From(additionalSource.SourceText, Encoding.UTF8));
            }

            productionContext.AddSource(
                "DockSystemTextJsonGenerated.g.cs",
                SourceText.From(SourceEmitter.EmitGenerated(model), Encoding.UTF8));
        });
    }

    private static class MetadataNames
    {
        public const string SourceGenerationAttribute = "Dock.Serializer.SystemTextJson.DockJsonSourceGenerationAttribute";
        public const string SerializableAttribute = "Dock.Serializer.SystemTextJson.DockJsonSerializableAttribute";
        public const string IDockable = "Dock.Model.Core.IDockable";
        public const string IDock = "Dock.Model.Core.IDock";
        public const string IRootDock = "Dock.Model.Controls.IRootDock";
        public const string IDockWindow = "Dock.Model.Core.IDockWindow";
        public const string IDocumentTemplate = "Dock.Model.Controls.IDocumentTemplate";
        public const string IToolTemplate = "Dock.Model.Controls.IToolTemplate";
        public const string ICommand = "System.Windows.Input.ICommand";
        public const string IgnoreDataMemberAttribute = "System.Runtime.Serialization.IgnoreDataMemberAttribute";
    }

    private static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor InvalidRegisteredType =
            new(
                id: "DSTJ001",
                title: "Invalid Dock JSON registration",
                messageFormat: "Type '{0}' is not a supported Dock JSON source generation registration. Register a closed, non-abstract named type.",
                category: "Dock.Serializer.SystemTextJson",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateDiscriminator =
            new(
                id: "DSTJ003",
                title: "Duplicate Dock JSON discriminator",
                messageFormat: "Types '{0}' share the discriminator '{1}' for '{2}'",
                category: "Dock.Serializer.SystemTextJson",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingDockSymbols =
            new(
                id: "DSTJ004",
                title: "Missing Dock model contracts",
                messageFormat: "Dock JSON source generation requires references to Dock model contracts. Missing: {0}.",
                category: "Dock.Serializer.SystemTextJson",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ContextGenerationFailed =
            new(
                id: "DSTJ005",
                title: "Dock JSON metadata generation failed",
                messageFormat: "Dock cannot generate AOT serialization metadata: {0}",
                category: "Dock.Serializer.SystemTextJson",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);
    }

    private enum DockBaseKind
    {
        Dockable,
        Dock,
        RootDock,
        DockWindow,
        DocumentTemplate,
        ToolTemplate,
        Object
    }

    private sealed record RegistrationCandidate(INamedTypeSymbol Type, Location? Location);

    private sealed record SerializableTypeModel(
        INamedTypeSymbol Type,
        string TypeExpression,
        string DisplayName,
        string DiscriminatorKey,
        ImmutableArray<string> IgnoredMembers,
        Location? DiagnosticLocation,
        bool IsDockable,
        bool IsDock,
        bool IsRootDock,
        bool IsDockWindow,
        bool IsDocumentTemplate,
        bool IsToolTemplate,
        bool IsObjectPayload);

    private sealed record DerivedTypeModel(
        string TypeExpression,
        string DisplayName,
        string DiscriminatorKey,
        Location? DiagnosticLocation);

    private sealed record PolymorphismModel(
        DockBaseKind Kind,
        string BaseTypeExpression,
        string BaseDisplayName,
        string UnknownDerivedTypeHandling,
        bool IgnoreUnrecognizedTypeDiscriminators,
        ImmutableArray<DerivedTypeModel> DerivedTypes);

    private sealed record IgnoredMembersModel(
        string TypeExpression,
        ImmutableArray<string> MemberNames);

    private sealed record DockSymbols(
        INamedTypeSymbol IDockable,
        INamedTypeSymbol IDock,
        INamedTypeSymbol IRootDock,
        INamedTypeSymbol IDockWindow,
        INamedTypeSymbol IDocumentTemplate,
        INamedTypeSymbol IToolTemplate,
        INamedTypeSymbol ICommand,
        INamedTypeSymbol IgnoreDataMemberAttribute);

    private sealed record GeneratedSourceArtifact(string HintName, string SourceText);

    private sealed record GenerationModel(
        bool ShouldGenerate,
        ImmutableArray<Diagnostic> Diagnostics,
        string ContextTypeName,
        string ContextSource,
        ImmutableArray<GeneratedSourceArtifact> AdditionalSources,
        ImmutableArray<string> ContextTypes,
        ImmutableArray<PolymorphismModel> Polymorphisms,
        ImmutableArray<IgnoredMembersModel> IgnoredMembers,
        ImmutableArray<SerializableTypeModel> SerializableTypes,
        ImmutableArray<ITypeSymbol> CollectionTypes,
        ImmutableArray<INamedTypeSymbol> ListTypes,
        IAssemblySymbol? GeneratedAssembly,
        bool SupportsUnsafeAccessors,
        bool SupportsGenericUnsafeAccessors)
    {
        public static GenerationModel Empty(ImmutableArray<Diagnostic> diagnostics)
        {
            return new GenerationModel(
                ShouldGenerate: false,
                Diagnostics: diagnostics,
                ContextTypeName: string.Empty,
                ContextSource: string.Empty,
                AdditionalSources: ImmutableArray<GeneratedSourceArtifact>.Empty,
                ContextTypes: ImmutableArray<string>.Empty,
                Polymorphisms: ImmutableArray<PolymorphismModel>.Empty,
                IgnoredMembers: ImmutableArray<IgnoredMembersModel>.Empty,
                SerializableTypes: ImmutableArray<SerializableTypeModel>.Empty,
                CollectionTypes: ImmutableArray<ITypeSymbol>.Empty,
                ListTypes: ImmutableArray<INamedTypeSymbol>.Empty,
                GeneratedAssembly: null,
                SupportsUnsafeAccessors: false,
                SupportsGenericUnsafeAccessors: false);
        }
    }

    private static class GenerationModelBuilder
    {
        private static readonly SymbolDisplayFormat s_fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

        public static GenerationModel Build(Compilation compilation, System.Threading.CancellationToken cancellationToken)
        {
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
            ImmutableArray<RegistrationCandidate> registeredTypes = GetRegisteredTypes(compilation, diagnostics, cancellationToken);
            // Metadata generation is automatic for serializer consumers. The activation
            // attribute remains accepted for source compatibility.
            if (compilation.GetTypeByMetadataName(MetadataNames.SourceGenerationAttribute) is null)
            {
                return GenerationModel.Empty(diagnostics.ToImmutable());
            }

            if (!TryGetDockSymbols(compilation, out DockSymbols? dockSymbols, out ImmutableArray<string> missingSymbols))
            {
                diagnostics.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.MissingDockSymbols,
                        Location.None,
                        string.Join(", ", missingSymbols)));

                return GenerationModel.Empty(diagnostics.ToImmutable());
            }

            registeredTypes = FilterRegisteredTypes(registeredTypes, dockSymbols!, diagnostics);

            ImmutableArray<SerializableTypeModel> serializableTypes =
                GetSerializableTypes(compilation, dockSymbols!, registeredTypes, cancellationToken);

            if (serializableTypes.IsEmpty && !HasAssemblyAttribute(compilation, MetadataNames.SourceGenerationAttribute)
                && !GetSerializerCallTypes(compilation).Any())
            {
                return GenerationModel.Empty(diagnostics.ToImmutable());
            }

            ImmutableArray<ITypeSymbol> collectionTypes = GetCollectionTypes(serializableTypes, dockSymbols!, compilation);
            ImmutableArray<IgnoredMembersModel> ignoredMembers =
                BuildIgnoredMembers(serializableTypes, dockSymbols!);

            ImmutableArray<PolymorphismModel> polymorphisms =
                BuildPolymorphisms(serializableTypes, diagnostics);

            ImmutableArray<string> contextTypes =
                BuildContextTypes(serializableTypes, dockSymbols!)
                    .AddRange(collectionTypes.Select(static t => t.ToDisplayString(s_fullyQualifiedFormat)))
                    .AddRange(serializableTypes.SelectMany(static t => LegacyMemberEmitter.GetDataMembers(t.Type))
                        .Select(static member => member is IFieldSymbol field ? field.Type : ((IPropertySymbol)member).Type)
                        .Select(static t => t.ToDisplayString(s_fullyQualifiedFormat)));
            string contextTypeName = GetUniqueContextTypeName(compilation, cancellationToken);
            string contextSource = SourceEmitter.EmitContext(contextTypes, contextTypeName);
            ImmutableArray<GeneratedSourceArtifact> additionalSources;
            try
            {
                additionalSources = SystemTextJsonContextGenerator.Generate(compilation, contextSource, contextTypeName, cancellationToken);
            }
            catch (Exception exception) when (exception is FileLoadException or FileNotFoundException or BadImageFormatException
                or TypeLoadException or ReflectionTypeLoadException or MissingMethodException or TargetInvocationException)
            {
                diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.ContextGenerationFailed, Location.None, exception.Message));
                return GenerationModel.Empty(diagnostics.ToImmutable());
            }
            if (additionalSources.IsDefaultOrEmpty)
            {
                diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.ContextGenerationFailed, Location.None,
                    "The System.Text.Json generator did not produce a serialization context."));
                return GenerationModel.Empty(diagnostics.ToImmutable());
            }

            return new GenerationModel(
                ShouldGenerate: true,
                Diagnostics: diagnostics.ToImmutable(),
                ContextTypeName: contextTypeName,
                ContextSource: contextSource,
                AdditionalSources: additionalSources,
                ContextTypes: contextTypes,
                Polymorphisms: polymorphisms,
                IgnoredMembers: ignoredMembers,
                SerializableTypes: serializableTypes,
                CollectionTypes: collectionTypes,
                ListTypes: GetConfiguredListTypes(compilation, cancellationToken),
                GeneratedAssembly: compilation.Assembly,
                SupportsUnsafeAccessors: compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.UnsafeAccessorAttribute") is not null,
                SupportsGenericUnsafeAccessors: compilation.GetSpecialType(SpecialType.System_Object).ContainingAssembly.Identity.Version.Major >= 9);
        }

        private static string GetUniqueContextTypeName(
            Compilation compilation,
            System.Threading.CancellationToken cancellationToken)
        {
            var suffix = 0;
            var candidate = GeneratedContextTypeNameBase;

            while (compilation.GetSymbolsWithName(candidate, SymbolFilter.Type, cancellationToken).Any())
            {
                suffix++;
                candidate = GeneratedContextTypeNameBase + "_" + suffix.ToString(CultureInfo.InvariantCulture);
            }

            return candidate;
        }

        private static ImmutableArray<RegistrationCandidate> GetRegisteredTypes(
            Compilation compilation,
            ImmutableArray<Diagnostic>.Builder diagnostics,
            System.Threading.CancellationToken cancellationToken)
        {
            INamedTypeSymbol? attributeSymbol =
                compilation.GetTypeByMetadataName(MetadataNames.SerializableAttribute);

            if (attributeSymbol is null)
            {
                return ImmutableArray<RegistrationCandidate>.Empty;
            }

            var results = ImmutableArray.CreateBuilder<RegistrationCandidate>();

            foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!MatchesAttribute(attribute, attributeSymbol, MetadataNames.SerializableAttribute))
                {
                    continue;
                }

                Location? location = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation();
                if (attribute.ConstructorArguments.Length != 1
                    || attribute.ConstructorArguments[0].Kind != TypedConstantKind.Type
                    || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol typeSymbol
                    || !IsValidRegisteredType(typeSymbol, compilation.Assembly))
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidRegisteredType,
                            location ?? Location.None,
                            GetRegisteredTypeDisplay(attribute)));
                    continue;
                }

                results.Add(new RegistrationCandidate(typeSymbol, location));
            }

            return results.ToImmutable();
        }

        private static string GetRegisteredTypeDisplay(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length == 1)
            {
                TypedConstant constant = attribute.ConstructorArguments[0];
                if (constant.Value is ITypeSymbol typeSymbol)
                {
                    return typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                }
            }

            return "<unknown>";
        }

        private static bool IsValidRegisteredType(INamedTypeSymbol typeSymbol, IAssemblySymbol generatedAssembly)
        {
            return !typeSymbol.IsAbstract
                   && !HasOpenTypeParameters(typeSymbol)
                   && typeSymbol.TypeKind != TypeKind.Interface
                   && typeSymbol.TypeKind != TypeKind.Delegate
                   && IsAccessibleFromGeneratedCode(typeSymbol, generatedAssembly);
        }

        private static ImmutableArray<RegistrationCandidate> FilterRegisteredTypes(
            ImmutableArray<RegistrationCandidate> registeredTypes,
            DockSymbols symbols,
            ImmutableArray<Diagnostic>.Builder diagnostics)
        {
            var valid = ImmutableArray.CreateBuilder<RegistrationCandidate>(registeredTypes.Length);

            foreach (RegistrationCandidate registration in registeredTypes)
            {
                if (!IsAssignableToAnyDockContract(registration.Type, symbols)
                    && !IsSupportedObjectPayloadType(registration.Type))
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidRegisteredType,
                            registration.Location ?? Location.None,
                            registration.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                    continue;
                }

                valid.Add(registration);
            }

            return valid.ToImmutable();
        }

        private static bool TryGetDockSymbols(
            Compilation compilation,
            out DockSymbols? symbols,
            out ImmutableArray<string> missingSymbols)
        {
            var missing = ImmutableArray.CreateBuilder<string>();
            INamedTypeSymbol? iDockable = compilation.GetTypeByMetadataName(MetadataNames.IDockable);
            INamedTypeSymbol? iDock = compilation.GetTypeByMetadataName(MetadataNames.IDock);
            INamedTypeSymbol? iRootDock = compilation.GetTypeByMetadataName(MetadataNames.IRootDock);
            INamedTypeSymbol? iDockWindow = compilation.GetTypeByMetadataName(MetadataNames.IDockWindow);
            INamedTypeSymbol? iDocumentTemplate = compilation.GetTypeByMetadataName(MetadataNames.IDocumentTemplate);
            INamedTypeSymbol? iToolTemplate = compilation.GetTypeByMetadataName(MetadataNames.IToolTemplate);
            INamedTypeSymbol? iCommand = compilation.GetTypeByMetadataName(MetadataNames.ICommand);
            INamedTypeSymbol? ignoreDataMemberAttribute =
                compilation.GetTypeByMetadataName(MetadataNames.IgnoreDataMemberAttribute);

            if (iDockable is null)
            {
                missing.Add(MetadataNames.IDockable);
            }

            if (iDock is null)
            {
                missing.Add(MetadataNames.IDock);
            }

            if (iRootDock is null)
            {
                missing.Add(MetadataNames.IRootDock);
            }

            if (iDockWindow is null)
            {
                missing.Add(MetadataNames.IDockWindow);
            }

            if (iDocumentTemplate is null)
            {
                missing.Add(MetadataNames.IDocumentTemplate);
            }

            if (iToolTemplate is null)
            {
                missing.Add(MetadataNames.IToolTemplate);
            }

            if (iCommand is null)
            {
                missing.Add(MetadataNames.ICommand);
            }

            if (ignoreDataMemberAttribute is null)
            {
                missing.Add(MetadataNames.IgnoreDataMemberAttribute);
            }

            if (missing.Count > 0)
            {
                symbols = null;
                missingSymbols = missing.ToImmutable();
                return false;
            }

            symbols = new DockSymbols(
                iDockable!,
                iDock!,
                iRootDock!,
                iDockWindow!,
                iDocumentTemplate!,
                iToolTemplate!,
                iCommand!,
                ignoreDataMemberAttribute!);
            missingSymbols = ImmutableArray<string>.Empty;
            return true;
        }

        private static ImmutableArray<SerializableTypeModel> GetSerializableTypes(
            Compilation compilation,
            DockSymbols symbols,
            ImmutableArray<RegistrationCandidate> registeredTypes,
            System.Threading.CancellationToken cancellationToken)
        {
            var result = new List<SerializableTypeModel>();
            var seen = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            foreach (INamedTypeSymbol candidate in EnumerateNamedTypes(compilation.Assembly.GlobalNamespace, cancellationToken))
            {
                if (!IsDiscoverableDockType(candidate, symbols, compilation.Assembly))
                {
                    continue;
                }

                if (seen.Add(candidate))
                {
                    result.Add(CreateSerializableType(candidate, candidate.Locations.FirstOrDefault(), false, symbols));
                }
            }

            foreach (IAssemblySymbol assembly in compilation.SourceModule.ReferencedAssemblySymbols)
            {
                if (!assembly.Name.StartsWith("Dock.Model.", StringComparison.Ordinal)
                    && !assembly.Modules.Any(static module => module.ReferencedAssemblySymbols.Any(static reference => reference.Name == "Dock.Model")))
                {
                    continue;
                }
                foreach (INamedTypeSymbol candidate in EnumerateNamedTypes(assembly.GlobalNamespace, cancellationToken))
                {
                    if (IsDiscoverableDockType(candidate, symbols, compilation.Assembly) && seen.Add(candidate))
                    {
                        result.Add(CreateSerializableType(candidate, null, false, symbols));
                    }
                }
            }

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);
                foreach (InvocationExpressionSyntax invocation in tree.GetRoot(cancellationToken).DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol method
                        || method.TypeArguments.Length != 1
                        || method.Name is not ("Serialize" or "Deserialize" or "Save" or "Load")
                        || !method.ContainingType.ToDisplayString().StartsWith("Dock.", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    if (method.TypeArguments[0] is INamedTypeSymbol candidate
                        && !HasOpenTypeParameters(candidate)
                        && IsAccessibleFromGeneratedCode(candidate, compilation.Assembly)
                        && candidate.TypeKind is not (TypeKind.Interface or TypeKind.Delegate)
                        && seen.Add(candidate))
                    {
                        result.Add(CreateSerializableType(candidate, invocation.GetLocation(), false, symbols));
                    }
                }
            }

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);
                foreach (AssignmentExpressionSyntax assignment in tree.GetRoot(cancellationToken).DescendantNodes().OfType<AssignmentExpressionSyntax>())
                {
                    if (semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol is IPropertySymbol member
                        && member.Type.SpecialType == SpecialType.System_Object
                        && (seen.Contains(member.ContainingType) || IsAssignableToAnyDockContract(member.ContainingType, symbols))
                        && semanticModel.GetTypeInfo(assignment.Right, cancellationToken).Type is INamedTypeSymbol payload
                        && payload.SpecialType == SpecialType.None
                        && IsValidRegisteredType(payload, compilation.Assembly) && seen.Add(payload))
                    {
                        result.Add(CreateSerializableType(payload, assignment.GetLocation(), true, symbols));
                    }
                }
            }

            foreach (RegistrationCandidate registration in registeredTypes)
            {
                if (seen.Add(registration.Type))
                {
                    bool isObjectPayload = !IsAssignableToAnyDockContract(registration.Type, symbols);
                    result.Add(CreateSerializableType(registration.Type, registration.Location, isObjectPayload, symbols));
                }
            }

            // Follow collection elements and dictionary values as well as direct members.
            // Every reachable legacy $type needs a registered descriptor, even when STJ
            // already includes that type transitively in its own context.
            var pending = new Queue<ITypeSymbol>(result.Select(static t => (ITypeSymbol)t.Type).Concat(GetSerializerCallTypes(compilation)));
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            while (pending.Count > 0)
            {
                ITypeSymbol value = pending.Dequeue();
                if (!visited.Add(value))
                {
                    continue;
                }
                if (value is IArrayTypeSymbol array)
                {
                    pending.Enqueue(array.ElementType);
                    continue;
                }
                if (value is not INamedTypeSymbol candidate || HasOpenTypeParameters(candidate))
                {
                    continue;
                }
                foreach (ITypeSymbol argument in candidate.TypeArguments)
                {
                    pending.Enqueue(argument);
                }
                if (GetCollectionValueType(candidate, out _) is { } collectionValue) pending.Enqueue(collectionValue);
                if (candidate.SpecialType != SpecialType.None || candidate.TypeKind == TypeKind.Enum
                    || candidate.ContainingNamespace.ToDisplayString().StartsWith("System", StringComparison.Ordinal)
                    || !IsAccessibleFromGeneratedCode(candidate, compilation.Assembly))
                {
                    continue;
                }
                if (!candidate.IsAbstract && candidate.TypeKind is TypeKind.Class or TypeKind.Struct && seen.Add(candidate))
                {
                    result.Add(CreateSerializableType(candidate, null, false, symbols));
                }
                foreach (ITypeSymbol memberType in GetSerializableMemberTypes(candidate, symbols))
                {
                    pending.Enqueue(memberType);
                }
            }

            return result
                .OrderBy(static x => x.TypeExpression, StringComparer.Ordinal)
                .ToImmutableArray();
        }

        private static IEnumerable<ITypeSymbol> GetSerializableMemberTypes(INamedTypeSymbol type, DockSymbols symbols)
        {
            foreach (IPropertySymbol property in GetAllPublicInstanceProperties(type))
            {
                if (!ShouldIgnoreProperty(property, symbols))
                {
                    yield return property.Type;
                }
            }
            foreach (ISymbol member in LegacyMemberEmitter.GetDataMembers(type))
            {
                if (member is IFieldSymbol field)
                {
                    yield return field.Type;
                }
                else if (member is IPropertySymbol property && !ShouldIgnoreProperty(property, symbols))
                {
                    yield return property.Type;
                }
            }
        }

        private static ImmutableArray<INamedTypeSymbol> GetConfiguredListTypes(
            Compilation compilation, System.Threading.CancellationToken cancellationToken)
        {
            var results = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel model = compilation.GetSemanticModel(tree);
                foreach (TypeOfExpressionSyntax expression in tree.GetRoot(cancellationToken).DescendantNodes().OfType<TypeOfExpressionSyntax>())
                {
                    if (model.GetTypeInfo(expression.Type, cancellationToken).Type is INamedTypeSymbol type
                        && type.IsUnboundGenericType && type.Arity == 1 && !type.IsAbstract
                        && type.ContainingNamespace.ToDisplayString() != "System.Collections.Generic"
                        && type.ContainingNamespace.ToDisplayString() != "System.Collections.ObjectModel"
                        && IsAccessibleFromGeneratedCode(type, compilation.Assembly)
                        && type.OriginalDefinition.InstanceConstructors.Any(static c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public)
                        && type.OriginalDefinition.AllInterfaces.Any(static i => i.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IList<T>"))
                    {
                        results.Add(type.OriginalDefinition);
                    }
                }
            }
            return results.OrderBy(static t => t.ToDisplayString(), StringComparer.Ordinal).ToImmutableArray();
        }

        private static ImmutableArray<ITypeSymbol> GetCollectionTypes(
            ImmutableArray<SerializableTypeModel> types, DockSymbols symbols, Compilation compilation)
        {
            var collections = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var pending = new Queue<ITypeSymbol>();
            foreach (SerializableTypeModel type in types)
            {
                pending.Enqueue(type.Type);
            }
            foreach (ITypeSymbol type in GetSerializerCallTypes(compilation)) pending.Enqueue(type);
            INamedTypeSymbol? list = compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");
            if (list is not null)
            {
                pending.Enqueue(list.Construct(symbols.IDockable));
                pending.Enqueue(list.Construct(symbols.IDockWindow));
            }
            while (pending.Count > 0)
            {
                ITypeSymbol candidate = pending.Dequeue();
                if (!visited.Add(candidate))
                {
                    continue;
                }
                if (candidate is IArrayTypeSymbol array)
                {
                    collections.Add(array);
                    pending.Enqueue(array.ElementType);
                    continue;
                }
                if (candidate is not INamedTypeSymbol named || HasOpenTypeParameters(named))
                {
                    continue;
                }
                ITypeSymbol? element = GetCollectionValueType(named, out bool dictionary);
                if (element is not null)
                {
                    collections.Add(named);
                    pending.Enqueue(element);
                    string definition = named.OriginalDefinition.ToDisplayString();
                    if (definition == "System.Collections.Generic.IList<T>")
                    {
                        pending.Enqueue(compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!.Construct(element));
                        pending.Enqueue(compilation.GetTypeByMetadataName("System.Collections.ObjectModel.ObservableCollection`1")!.Construct(element));
                    }
                    if (dictionary && definition == "System.Collections.Generic.IDictionary<TKey, TValue>")
                    {
                        pending.Enqueue(compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2")!.Construct(named.TypeArguments[0], element));
                    }
                    continue;
                }
                if (named.SpecialType != SpecialType.None || named.TypeKind == TypeKind.Enum
                    || named.ContainingNamespace.ToDisplayString().StartsWith("System", StringComparison.Ordinal))
                {
                    continue;
                }
                foreach (ITypeSymbol memberType in GetSerializableMemberTypes(named, symbols))
                {
                    pending.Enqueue(memberType);
                }
            }
            return collections.OrderBy(static t => t.ToDisplayString(), StringComparer.Ordinal).ToImmutableArray();
        }

        internal static ITypeSymbol? GetCollectionValueType(ITypeSymbol type, out bool dictionary)
        {
            dictionary = false;
            if (type is IArrayTypeSymbol array) return array.ElementType;
            if (type is not INamedTypeSymbol named || named.SpecialType == SpecialType.System_String) return null;
            IEnumerable<INamedTypeSymbol> contracts = named.AllInterfaces.Prepend(named);
            foreach (INamedTypeSymbol contract in contracts)
            {
                if (contract.OriginalDefinition.ToDisplayString() is "System.Collections.Generic.IDictionary<TKey, TValue>" or "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>")
                {
                    dictionary = true;
                    return contract.TypeArguments[1];
                }
            }
            foreach (INamedTypeSymbol contract in contracts)
                if (contract.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>") return contract.TypeArguments[0];
            return null;
        }

        private static IEnumerable<ITypeSymbol> GetSerializerCallTypes(Compilation compilation)
        {
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);
                foreach (InvocationExpressionSyntax invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
                        && method.TypeArguments.Length == 1
                        && method.Name is "Serialize" or "Deserialize" or "Save" or "Load"
                        && method.ContainingType.ToDisplayString().StartsWith("Dock.", StringComparison.Ordinal)
                        && IsClosedAccessibleType(method.TypeArguments[0], compilation.Assembly))
                        yield return method.TypeArguments[0];
                }
            }
        }

        private static bool IsClosedAccessibleType(ITypeSymbol type, IAssemblySymbol assembly) => type switch
        {
            IArrayTypeSymbol array => IsClosedAccessibleType(array.ElementType, assembly),
            INamedTypeSymbol named => !HasOpenTypeParameters(named) && IsAccessibleFromGeneratedCode(named, assembly)
                && named.TypeArguments.All(t => IsClosedAccessibleType(t, assembly)),
            _ => false
        };

        private static SerializableTypeModel CreateSerializableType(
            INamedTypeSymbol typeSymbol,
            Location? location,
            bool isObjectPayload,
            DockSymbols symbols)
        {
            ImmutableArray<string> ignoredMembers = GetIgnoredMembers(typeSymbol, symbols);
            return new SerializableTypeModel(
                Type: typeSymbol,
                TypeExpression: typeSymbol.ToDisplayString(s_fullyQualifiedFormat),
                DisplayName: typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                DiscriminatorKey: GetDiscriminatorKey(typeSymbol),
                IgnoredMembers: ignoredMembers,
                DiagnosticLocation: location,
                IsDockable: IsAssignableTo(typeSymbol, symbols.IDockable),
                IsDock: IsAssignableTo(typeSymbol, symbols.IDock),
                IsRootDock: IsAssignableTo(typeSymbol, symbols.IRootDock),
                IsDockWindow: IsAssignableTo(typeSymbol, symbols.IDockWindow),
                IsDocumentTemplate: IsAssignableTo(typeSymbol, symbols.IDocumentTemplate),
                IsToolTemplate: IsAssignableTo(typeSymbol, symbols.IToolTemplate),
                IsObjectPayload: isObjectPayload);
        }

        private static ImmutableArray<IgnoredMembersModel> BuildIgnoredMembers(
            ImmutableArray<SerializableTypeModel> serializableTypes,
            DockSymbols symbols)
        {
            var results = new List<IgnoredMembersModel>();

            foreach (SerializableTypeModel serializableType in serializableTypes)
            {
                if (serializableType.IgnoredMembers.Length > 0)
                {
                    results.Add(new IgnoredMembersModel(serializableType.TypeExpression, serializableType.IgnoredMembers));
                }
            }

            AddInterfaceIgnoredMembers(results, symbols.IDockable, serializableTypes.Where(static x => x.IsDockable), symbols);
            AddInterfaceIgnoredMembers(results, symbols.IDock, serializableTypes.Where(static x => x.IsDock), symbols);
            AddInterfaceIgnoredMembers(results, symbols.IRootDock, serializableTypes.Where(static x => x.IsRootDock), symbols);
            AddInterfaceIgnoredMembers(results, symbols.IDockWindow, serializableTypes.Where(static x => x.IsDockWindow), symbols);
            AddInterfaceIgnoredMembers(results, symbols.IDocumentTemplate, serializableTypes.Where(static x => x.IsDocumentTemplate), symbols);
            AddInterfaceIgnoredMembers(results, symbols.IToolTemplate, serializableTypes.Where(static x => x.IsToolTemplate), symbols);

            return results
                .OrderBy(static x => x.TypeExpression, StringComparer.Ordinal)
                .ToImmutableArray();
        }

        private static void AddInterfaceIgnoredMembers(
            ICollection<IgnoredMembersModel> results,
            INamedTypeSymbol interfaceSymbol,
            IEnumerable<SerializableTypeModel> matchingTypes,
            DockSymbols symbols)
        {
            var ignoredMembers = new HashSet<string>(StringComparer.Ordinal);

            foreach (IPropertySymbol propertySymbol in GetAllPublicInstanceProperties(interfaceSymbol))
            {
                if (ShouldIgnoreProperty(propertySymbol, symbols))
                {
                    ignoredMembers.Add(propertySymbol.Name);
                }
            }

            foreach (SerializableTypeModel matchingType in matchingTypes)
            {
                foreach (string memberName in matchingType.IgnoredMembers)
                {
                    ignoredMembers.Add(memberName);
                }
            }

            if (ignoredMembers.Count == 0)
            {
                return;
            }

            results.Add(
                new IgnoredMembersModel(
                    interfaceSymbol.ToDisplayString(s_fullyQualifiedFormat),
                    ignoredMembers.OrderBy(static x => x, StringComparer.Ordinal).ToImmutableArray()));
        }

        private static ImmutableArray<PolymorphismModel> BuildPolymorphisms(
            ImmutableArray<SerializableTypeModel> serializableTypes,
            ImmutableArray<Diagnostic>.Builder diagnostics)
        {
            return
            [
                BuildPolymorphism(
                    DockBaseKind.Dockable,
                    "global::Dock.Model.Core.IDockable",
                    "IDockable",
                    "global::System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToBaseType",
                    true,
                    serializableTypes.Where(static x => x.IsDockable).Select(ToDerivedModel),
                    diagnostics),
                BuildPolymorphism(
                    DockBaseKind.Dock,
                    "global::Dock.Model.Core.IDock",
                    "IDock",
                    "global::System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToBaseType",
                    true,
                    serializableTypes.Where(static x => x.IsDock).Select(ToDerivedModel),
                    diagnostics),
                BuildPolymorphism(
                    DockBaseKind.RootDock,
                    "global::Dock.Model.Controls.IRootDock",
                    "IRootDock",
                    "global::System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor",
                    true,
                    serializableTypes.Where(static x => x.IsRootDock).Select(ToDerivedModel),
                    diagnostics),
                BuildPolymorphism(
                    DockBaseKind.DockWindow,
                    "global::Dock.Model.Core.IDockWindow",
                    "IDockWindow",
                    "global::System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToBaseType",
                    true,
                    serializableTypes.Where(static x => x.IsDockWindow).Select(ToDerivedModel),
                    diagnostics),
                BuildPolymorphism(
                    DockBaseKind.DocumentTemplate,
                    "global::Dock.Model.Controls.IDocumentTemplate",
                    "IDocumentTemplate",
                    "global::System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor",
                    true,
                    serializableTypes.Where(static x => x.IsDocumentTemplate).Select(ToDerivedModel),
                    diagnostics),
                BuildPolymorphism(
                    DockBaseKind.ToolTemplate,
                    "global::Dock.Model.Controls.IToolTemplate",
                    "IToolTemplate",
                    "global::System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor",
                    true,
                    serializableTypes.Where(static x => x.IsToolTemplate).Select(ToDerivedModel),
                    diagnostics),
                BuildPolymorphism(
                    DockBaseKind.Object,
                    "global::System.Object",
                    "object",
                    "global::System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FailSerialization",
                    false,
                    serializableTypes.Where(static x => x.IsObjectPayload
                        || x.Type.SpecialType == SpecialType.None && !x.IsDockable && !x.IsDockWindow && !x.IsDocumentTemplate && !x.IsToolTemplate
                        && !x.Type.ContainingNamespace.ToDisplayString().StartsWith("System", StringComparison.Ordinal)).Select(ToDerivedModel),
                    diagnostics)
            ];
        }

        private static DerivedTypeModel ToDerivedModel(SerializableTypeModel serializableType)
        {
            return new DerivedTypeModel(
                serializableType.TypeExpression,
                serializableType.DisplayName,
                serializableType.DiscriminatorKey,
                serializableType.DiagnosticLocation);
        }

        private static PolymorphismModel BuildPolymorphism(
            DockBaseKind kind,
            string baseTypeExpression,
            string baseDisplayName,
            string unknownDerivedTypeHandling,
            bool ignoreUnrecognizedTypeDiscriminators,
            IEnumerable<DerivedTypeModel> derivedTypes,
            ImmutableArray<Diagnostic>.Builder diagnostics)
        {
            List<DerivedTypeModel> sorted = derivedTypes
                .OrderBy(static x => x.DiscriminatorKey, StringComparer.Ordinal)
                .ThenBy(static x => x.TypeExpression, StringComparer.Ordinal)
                .ToList();

            var collisions = new HashSet<string>(StringComparer.Ordinal);
            foreach (IGrouping<string, DerivedTypeModel> group in sorted.GroupBy(static x => x.DiscriminatorKey, StringComparer.Ordinal))
            {
                if (group.Count() < 2)
                {
                    continue;
                }

                collisions.Add(group.Key);
                string typeNames = string.Join(", ", group.Select(static x => x.DisplayName));
                Location location = group.Select(static x => x.DiagnosticLocation).FirstOrDefault(static x => x is not null) ?? Location.None;
                diagnostics.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.DuplicateDiscriminator,
                        location,
                        typeNames,
                        group.Key,
                        baseDisplayName));
            }

            ImmutableArray<DerivedTypeModel> filtered = sorted
                .Where(x => !collisions.Contains(x.DiscriminatorKey))
                .ToImmutableArray();

            return new PolymorphismModel(
                kind,
                baseTypeExpression,
                baseDisplayName,
                unknownDerivedTypeHandling,
                ignoreUnrecognizedTypeDiscriminators,
                filtered);
        }

        private static ImmutableArray<string> BuildContextTypes(
            ImmutableArray<SerializableTypeModel> serializableTypes,
            DockSymbols symbols)
        {
            var typeExpressions = new HashSet<string>(StringComparer.Ordinal)
            {
                "global::System.Object",
                symbols.IDockable.ToDisplayString(s_fullyQualifiedFormat),
                symbols.IDock.ToDisplayString(s_fullyQualifiedFormat),
                symbols.IRootDock.ToDisplayString(s_fullyQualifiedFormat),
                symbols.IDockWindow.ToDisplayString(s_fullyQualifiedFormat),
                symbols.IDocumentTemplate.ToDisplayString(s_fullyQualifiedFormat),
                symbols.IToolTemplate.ToDisplayString(s_fullyQualifiedFormat),
                "global::System.Collections.Generic.IList<global::Dock.Model.Core.IDockable>",
                "global::System.Collections.Generic.IList<global::Dock.Model.Core.IDockWindow>"
            };

            foreach (SerializableTypeModel serializableType in serializableTypes)
            {
                typeExpressions.Add(serializableType.TypeExpression);
            }

            return typeExpressions
                .OrderBy(static x => x, StringComparer.Ordinal)
                .ToImmutableArray();
        }

        private static ImmutableArray<string> GetIgnoredMembers(INamedTypeSymbol typeSymbol, DockSymbols symbols)
        {
            var results = new HashSet<string>(StringComparer.Ordinal);

            foreach (IPropertySymbol propertySymbol in GetAllPublicInstanceProperties(typeSymbol))
            {
                if (ShouldIgnoreProperty(propertySymbol, symbols))
                {
                    results.Add(propertySymbol.Name);
                }
            }

            return results
                .OrderBy(static x => x, StringComparer.Ordinal)
                .ToImmutableArray();
        }

        public static IEnumerable<IPropertySymbol> GetAllPublicInstanceProperties(INamedTypeSymbol typeSymbol)
        {
            var results = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);

            if (typeSymbol.TypeKind == TypeKind.Interface)
            {
                AddInterfaceProperties(typeSymbol, results);
                return results.Values;
            }

            INamedTypeSymbol? current = typeSymbol;
            while (current is not null)
            {
                foreach (IPropertySymbol propertySymbol in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (IsPublicInstanceProperty(propertySymbol) && !results.ContainsKey(propertySymbol.Name))
                    {
                        results.Add(propertySymbol.Name, propertySymbol);
                    }
                }

                current = current.BaseType;
            }

            foreach (INamedTypeSymbol interfaceSymbol in typeSymbol.AllInterfaces)
            {
                AddInterfaceProperties(interfaceSymbol, results);
            }

            return results.Values;
        }

        private static void AddInterfaceProperties(
            INamedTypeSymbol interfaceSymbol,
            IDictionary<string, IPropertySymbol> results)
        {
            foreach (INamedTypeSymbol inheritedInterface in interfaceSymbol.AllInterfaces.Reverse().Concat([interfaceSymbol]))
            {
                foreach (IPropertySymbol propertySymbol in inheritedInterface.GetMembers().OfType<IPropertySymbol>())
                {
                    if (IsPublicInstanceProperty(propertySymbol) && !results.ContainsKey(propertySymbol.Name))
                    {
                        results.Add(propertySymbol.Name, propertySymbol);
                    }
                }
            }
        }

        private static bool IsPublicInstanceProperty(IPropertySymbol propertySymbol)
        {
            return propertySymbol.DeclaredAccessibility == Accessibility.Public && !propertySymbol.IsStatic;
        }

        private static bool ShouldIgnoreProperty(IPropertySymbol propertySymbol, DockSymbols symbols)
        {
            return propertySymbol.SetMethod is null
                   || IsCommandType(propertySymbol.Type, symbols.ICommand)
                   || HasIgnoreDataMemberAttribute(propertySymbol, symbols.IgnoreDataMemberAttribute);
        }

        private static bool HasIgnoreDataMemberAttribute(IPropertySymbol propertySymbol, INamedTypeSymbol ignoreDataMemberAttribute)
        {
            IPropertySymbol? current = propertySymbol;
            while (current is not null)
            {
                foreach (AttributeData attribute in current.GetAttributes())
                {
                    if (MatchesAttribute(attribute, ignoreDataMemberAttribute, MetadataNames.IgnoreDataMemberAttribute))
                    {
                        return true;
                    }
                }

                current = current.OverriddenProperty;
            }

            return false;
        }

        private static bool IsCommandType(ITypeSymbol typeSymbol, INamedTypeSymbol commandSymbol)
        {
            if (SymbolEqualityComparer.Default.Equals(typeSymbol, commandSymbol))
            {
                return true;
            }

            return typeSymbol is INamedTypeSymbol namedType
                   && namedType.AllInterfaces.Any(interfaceSymbol => SymbolEqualityComparer.Default.Equals(interfaceSymbol, commandSymbol));
        }

        private static bool IsDiscoverableDockType(
            INamedTypeSymbol typeSymbol,
            DockSymbols symbols,
            IAssemblySymbol generatedAssembly)
        {
            return typeSymbol.TypeKind == TypeKind.Class
                   && !typeSymbol.IsAbstract
                   && !HasOpenTypeParameters(typeSymbol)
                   && IsAccessibleFromGeneratedCode(typeSymbol, generatedAssembly)
                   && IsAssignableToAnyDockContract(typeSymbol, symbols);
        }

        private static bool HasOpenTypeParameters(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol.IsUnboundGenericType)
            {
                return true;
            }

            if (typeSymbol.TypeArguments.Any(ContainsOpenType))
            {
                return true;
            }

            return typeSymbol.ContainingType is not null && HasOpenTypeParameters(typeSymbol.ContainingType);
        }

        private static bool ContainsOpenType(ITypeSymbol type) => type switch
        {
            ITypeParameterSymbol => true,
            IArrayTypeSymbol array => ContainsOpenType(array.ElementType),
            INamedTypeSymbol named => HasOpenTypeParameters(named),
            _ => false
        };

        private static bool IsSupportedObjectPayloadType(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.TypeKind == TypeKind.Class
                   && typeSymbol.SpecialType == SpecialType.None;
        }

        private static bool IsAssignableToAnyDockContract(INamedTypeSymbol typeSymbol, DockSymbols symbols)
        {
            return IsAssignableTo(typeSymbol, symbols.IDockable)
                   || IsAssignableTo(typeSymbol, symbols.IDock)
                   || IsAssignableTo(typeSymbol, symbols.IRootDock)
                   || IsAssignableTo(typeSymbol, symbols.IDockWindow)
                   || IsAssignableTo(typeSymbol, symbols.IDocumentTemplate)
                   || IsAssignableTo(typeSymbol, symbols.IToolTemplate);
        }

        private static bool IsAssignableTo(ITypeSymbol typeSymbol, INamedTypeSymbol targetType)
        {
            if (SymbolEqualityComparer.Default.Equals(typeSymbol, targetType))
            {
                return true;
            }

            if (typeSymbol is not INamedTypeSymbol namedType)
            {
                return false;
            }

            if (targetType.TypeKind == TypeKind.Interface)
            {
                return namedType.AllInterfaces.Any(interfaceSymbol => SymbolEqualityComparer.Default.Equals(interfaceSymbol, targetType));
            }

            INamedTypeSymbol? current = namedType.BaseType;
            while (current is not null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, targetType))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        internal static bool IsAccessibleFromGeneratedCode(INamedTypeSymbol typeSymbol, IAssemblySymbol generatedAssembly)
        {
            INamedTypeSymbol? current = typeSymbol;
            while (current is not null)
            {
                if (!IsDirectlyAccessibleFromGeneratedCode(current, generatedAssembly))
                {
                    return false;
                }

                current = current.ContainingType;
            }

            return typeSymbol.IsUnboundGenericType || typeSymbol.TypeArguments.All(argument => argument switch
            {
                INamedTypeSymbol named => IsAccessibleFromGeneratedCode(named, generatedAssembly),
                IArrayTypeSymbol array when array.ElementType is INamedTypeSymbol element => IsAccessibleFromGeneratedCode(element, generatedAssembly),
                _ => true
            });
        }

        private static bool IsDirectlyAccessibleFromGeneratedCode(INamedTypeSymbol typeSymbol, IAssemblySymbol generatedAssembly)
        {
            return typeSymbol.DeclaredAccessibility switch
            {
                Accessibility.Public => true,
                Accessibility.Internal => SymbolEqualityComparer.Default.Equals(typeSymbol.ContainingAssembly, generatedAssembly) || typeSymbol.ContainingAssembly.GivesAccessTo(generatedAssembly),
                Accessibility.ProtectedOrInternal => SymbolEqualityComparer.Default.Equals(typeSymbol.ContainingAssembly, generatedAssembly) || typeSymbol.ContainingAssembly.GivesAccessTo(generatedAssembly),
                _ => false
            };
        }

        private static bool HasAssemblyAttribute(Compilation compilation, string metadataName)
        {
            return compilation.Assembly.GetAttributes().Any(attribute => MatchesAttribute(attribute, null, metadataName));
        }

        private static bool MatchesAttribute(AttributeData attribute, INamedTypeSymbol? symbol, string metadataName)
        {
            return attribute.AttributeClass is not null
                   && ((symbol is not null && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, symbol))
                       || string.Equals(attribute.AttributeClass.ToDisplayString(), metadataName, StringComparison.Ordinal));
        }

        private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypes(
            INamespaceSymbol namespaceSymbol,
            System.Threading.CancellationToken cancellationToken)
        {
            foreach (INamedTypeSymbol typeSymbol in namespaceSymbol.GetTypeMembers())
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (INamedTypeSymbol nested in EnumerateNamedTypes(typeSymbol, cancellationToken))
                {
                    yield return nested;
                }
            }

            foreach (INamespaceSymbol childNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (INamedTypeSymbol nested in EnumerateNamedTypes(childNamespace, cancellationToken))
                {
                    yield return nested;
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypes(
            INamedTypeSymbol typeSymbol,
            System.Threading.CancellationToken cancellationToken)
        {
            yield return typeSymbol;

            foreach (INamedTypeSymbol nestedType in typeSymbol.GetTypeMembers())
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (INamedTypeSymbol nested in EnumerateNamedTypes(nestedType, cancellationToken))
                {
                    yield return nested;
                }
            }
        }

        private static string GetDiscriminatorKey(INamedTypeSymbol typeSymbol)
        {
            return GetTypeFullName(typeSymbol);
        }

        public static string GetTypeFullName(ITypeSymbol typeSymbol)
        {
            return typeSymbol switch
            {
                IArrayTypeSymbol arrayTypeSymbol => GetArrayTypeFullName(arrayTypeSymbol),
                INamedTypeSymbol namedTypeSymbol => GetNamedTypeFullName(namedTypeSymbol),
                IPointerTypeSymbol pointerTypeSymbol => GetTypeFullName(pointerTypeSymbol.PointedAtType) + "*",
                _ => typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
            };
        }

        private static string GetArrayTypeFullName(IArrayTypeSymbol arrayTypeSymbol)
        {
            string rankSpecifier = arrayTypeSymbol.Rank == 1
                ? "[]"
                : "[" + new string(',', arrayTypeSymbol.Rank - 1) + "]";

            return GetTypeFullName(arrayTypeSymbol.ElementType) + rankSpecifier;
        }

        private static string GetNamedTypeFullName(INamedTypeSymbol typeSymbol)
        {
            INamedTypeSymbol effectiveType = typeSymbol.TupleUnderlyingType ?? typeSymbol;

            if (effectiveType.ContainingType is null)
            {
                string namespacePrefix = effectiveType.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : effectiveType.ContainingNamespace.ToDisplayString() + ".";
                return AppendGenericArguments(namespacePrefix + effectiveType.MetadataName, effectiveType);
            }

            var containingTypes = new Stack<string>();
            INamedTypeSymbol? current = effectiveType;
            while (current is not null)
            {
                containingTypes.Push(current.MetadataName);
                current = current.ContainingType;
            }

            string namespacePrefixNested = effectiveType.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : effectiveType.ContainingNamespace.ToDisplayString() + ".";
            return AppendGenericArguments(namespacePrefixNested + string.Join("+", containingTypes), effectiveType);
        }

        private static string AppendGenericArguments(string typeName, INamedTypeSymbol typeSymbol)
        {
            List<ITypeSymbol> typeArguments = GetFlattenedTypeArguments(typeSymbol);
            if (typeArguments.Count == 0)
            {
                return typeName;
            }

            return typeName + "[" + string.Join(",", typeArguments.Select(GetBracketedAssemblyQualifiedTypeName)) + "]";
        }

        private static List<ITypeSymbol> GetFlattenedTypeArguments(INamedTypeSymbol typeSymbol)
        {
            var containingTypes = new Stack<INamedTypeSymbol>();
            INamedTypeSymbol? current = typeSymbol;
            while (current is not null)
            {
                containingTypes.Push(current);
                current = current.ContainingType;
            }

            var flattened = new List<ITypeSymbol>();
            while (containingTypes.Count > 0)
            {
                INamedTypeSymbol segment = containingTypes.Pop();
                ImmutableArray<ITypeSymbol> segmentArguments = segment.TypeArguments;
                if (segmentArguments.Length == 0)
                {
                    continue;
                }

                int ownArgumentCount = segment.Arity;
                int startIndex = Math.Max(0, segmentArguments.Length - ownArgumentCount);
                for (int i = startIndex; i < segmentArguments.Length; i++)
                {
                    flattened.Add(segmentArguments[i]);
                }
            }

            return flattened;
        }

        private static string GetBracketedAssemblyQualifiedTypeName(ITypeSymbol typeSymbol)
        {
            return "[" + GetAssemblyQualifiedTypeName(typeSymbol) + "]";
        }

        private static string GetAssemblyQualifiedTypeName(ITypeSymbol typeSymbol)
        {
            string typeName = GetTypeFullName(typeSymbol);
            IAssemblySymbol? assemblySymbol = GetContainingAssembly(typeSymbol);
            if (assemblySymbol is null)
            {
                return typeName;
            }

            return typeName + ", " + GetAssemblyDisplayName(assemblySymbol);
        }

        private static IAssemblySymbol? GetContainingAssembly(ITypeSymbol typeSymbol)
        {
            return typeSymbol switch
            {
                IArrayTypeSymbol arrayTypeSymbol => GetContainingAssembly(arrayTypeSymbol.ElementType),
                IPointerTypeSymbol pointerTypeSymbol => GetContainingAssembly(pointerTypeSymbol.PointedAtType),
                _ => typeSymbol.ContainingAssembly
            };
        }

        private static string GetAssemblyDisplayName(IAssemblySymbol assemblySymbol)
        {
            AssemblyIdentity identity = assemblySymbol.Identity;
            string cultureName = string.IsNullOrEmpty(identity.CultureName)
                ? "neutral"
                : identity.CultureName;

            return identity.Name
                   + ", Version="
                   + identity.Version.ToString()
                   + ", Culture="
                   + cultureName
                   + ", PublicKeyToken="
                   + FormatPublicKeyToken(identity.PublicKeyToken);
        }

        private static string FormatPublicKeyToken(ImmutableArray<byte> publicKeyToken)
        {
            if (publicKeyToken.IsDefaultOrEmpty)
            {
                return "null";
            }

            var builder = new StringBuilder(publicKeyToken.Length * 2);
            foreach (byte value in publicKeyToken)
            {
                builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }

    private static class SystemTextJsonContextGenerator
    {
        public static ImmutableArray<GeneratedSourceArtifact> Generate(
            Compilation compilation,
            string contextSource,
            string contextTypeName,
            System.Threading.CancellationToken cancellationToken)
        {
            ISourceGenerator? generator = CreateGenerator(compilation);
            if (generator is null)
            {
                return ImmutableArray<GeneratedSourceArtifact>.Empty;
            }

            var parseOptions = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                               ?? CSharpParseOptions.Default;
            SyntaxTree contextTree = CSharpSyntaxTree.ParseText(contextSource, parseOptions, cancellationToken: cancellationToken);
            Compilation augmentedCompilation = compilation.AddSyntaxTrees(contextTree);

            GeneratorDriver driver = CSharpGeneratorDriver.Create(
                generators: new[] { generator },
                parseOptions: parseOptions);
            driver = driver.RunGeneratorsAndUpdateCompilation(
                augmentedCompilation,
                out Compilation outputCompilation,
                out _,
                cancellationToken);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            if (runResult.Results.Length == 0)
            {
                return ImmutableArray<GeneratedSourceArtifact>.Empty;
            }

            INamedTypeSymbol? contextSymbol = outputCompilation.GetTypeByMetadataName(
                GeneratedContextNamespace + "." + contextTypeName);
            if (contextSymbol is null)
            {
                return ImmutableArray<GeneratedSourceArtifact>.Empty;
            }

            return runResult.Results[0].GeneratedSources
                .Where(x => IsContextArtifact(x, outputCompilation, contextSymbol, cancellationToken))
                .Select(static x => new GeneratedSourceArtifact("SystemTextJson." + x.HintName, x.SourceText.ToString()))
                .ToImmutableArray();
        }

        private static bool IsContextArtifact(
            GeneratedSourceResult source,
            Compilation outputCompilation,
            INamedTypeSymbol contextSymbol,
            System.Threading.CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = outputCompilation.GetSemanticModel(source.SyntaxTree);
            SyntaxNode root = source.SyntaxTree.GetRoot(cancellationToken);

            foreach (ClassDeclarationSyntax declaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(declaration, cancellationToken) is INamedTypeSymbol declaredSymbol
                    && SymbolEqualityComparer.Default.Equals(declaredSymbol, contextSymbol))
                {
                    return true;
                }
            }

            return false;
        }

        private static ISourceGenerator? CreateGenerator(Compilation compilation)
        {
            // Compiler servers can host several STJ generator versions. Prefer the
            // generator shipped with this compilation's package, not one loaded by
            // an unrelated project (or the package's oldest Roslyn implementation).
            string? assemblyPath = TryGetAssemblyPath(compilation);
            Assembly? assembly = assemblyPath is not null
                // LoadFile isolates the selected package from same-name generators
                // already loaded by the shared compiler for another target/package.
                ? Assembly.LoadFile(assemblyPath)
                : AppDomain.CurrentDomain.GetAssemblies()
                    .Where(static x => string.Equals(x.GetName().Name, "System.Text.Json.SourceGeneration", StringComparison.Ordinal))
                    .OrderByDescending(static x => x.GetName().Version)
                    .FirstOrDefault();
            if (assembly is null)
            {
                return null;
            }

            Type? generatorType = assembly.GetTypes()
                .FirstOrDefault(static x => string.Equals(x.Name, "JsonSourceGenerator", StringComparison.Ordinal));

            if (generatorType is null)
            {
                return null;
            }

            object? instance = Activator.CreateInstance(generatorType, nonPublic: true);
            if (instance is ISourceGenerator sourceGenerator)
            {
                return sourceGenerator;
            }

            if (instance is IIncrementalGenerator incrementalGenerator)
            {
                return incrementalGenerator.AsSourceGenerator();
            }

            return null;
        }

        private static string? TryGetAssemblyPath(Compilation compilation)
        {
            string? runtimeAssemblyPath = compilation.References
                .Select(static x => x.Display)
                .FirstOrDefault(static x =>
                    x is not null
                    && x.EndsWith("System.Text.Json.dll", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(runtimeAssemblyPath))
            {
                return null;
            }

            string? runtimeDirectory = Path.GetDirectoryName(runtimeAssemblyPath);
            string? packageDirectory = runtimeDirectory is null ? null : Directory.GetParent(runtimeDirectory)?.Parent?.FullName;
            if (string.IsNullOrWhiteSpace(packageDirectory))
            {
                return null;
            }

            string analyzersDirectory = Path.Combine(packageDirectory, "analyzers", "dotnet");
            if (!Directory.Exists(analyzersDirectory))
            {
                return null;
            }

            Version compilerVersion = typeof(Compilation).Assembly.GetName().Version!;
            return Directory.EnumerateFiles(
                    analyzersDirectory,
                    "System.Text.Json.SourceGeneration.dll",
                    SearchOption.AllDirectories)
                .Select(static path => (Path: path, Version: GetRoslynVersion(path)))
                .Where(candidate => candidate.Version <= compilerVersion)
                .OrderByDescending(static candidate => candidate.Version)
                .Select(static candidate => candidate.Path)
                .FirstOrDefault();
        }

        private static Version GetRoslynVersion(string path)
        {
            string? directory = Directory.GetParent(path)?.Parent?.Name;
            return directory is not null && directory.StartsWith("roslyn", StringComparison.Ordinal)
                && Version.TryParse(directory.Substring("roslyn".Length), out Version? version)
                ? version : new Version(0, 0);
        }
    }

    private static class SourceEmitter
    {
        public static string EmitContext(ImmutableArray<string> contextTypes, string contextTypeName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine("namespace Dock.Serializer.SystemTextJson;");
            builder.AppendLine();
            builder.AppendLine("[global::System.Text.Json.Serialization.JsonSourceGenerationOptions(GenerationMode = global::System.Text.Json.Serialization.JsonSourceGenerationMode.Metadata)]");

            int typeIndex = 0;
            foreach (string contextType in contextTypes.Select(static t => t == "object" ? "global::System.Object" : t).Distinct(StringComparer.Ordinal))
            {
                builder.Append("[global::System.Text.Json.Serialization.JsonSerializable(typeof(");
                builder.Append(contextType);
                if (contextType == "global::System.Object" || contextType is "string" or "bool" or "int" or "long" or "double" or "float" or "decimal" or "byte" or "short" or "uint" or "ulong" or "ushort" or "sbyte" or "char")
                {
                    builder.AppendLine("))]");
                }
                else
                {
                    builder.Append("), TypeInfoPropertyName = \"DockType").Append(typeIndex++).AppendLine("\")]");
                }
            }

            builder.Append("internal sealed partial class ");
            builder.Append(contextTypeName);
            builder.AppendLine(" : global::System.Text.Json.Serialization.JsonSerializerContext");
            builder.AppendLine("{");
            builder.AppendLine("}");
            return builder.ToString();
        }

        public static string EmitGenerated(GenerationModel model)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine("namespace Dock.Serializer.SystemTextJson;");
            builder.AppendLine();
            builder.AppendLine("internal sealed class DockSystemTextJsonResolver : global::System.Text.Json.Serialization.Metadata.IJsonTypeInfoResolver");
            builder.AppendLine("{");
            builder.Append("    private static readonly global::System.Text.Json.Serialization.Metadata.IJsonTypeInfoResolver s_resolver = global::System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver.WithAddedModifier(");
            builder.Append(model.ContextTypeName);
            builder.AppendLine(".Default, ModifyTypeInfo);");
            builder.AppendLine("    private static readonly global::System.Collections.Generic.IReadOnlyDictionary<global::System.Type, global::System.Collections.Generic.HashSet<string>> s_ignoredMembers = CreateIgnoredMembers();");
            builder.AppendLine("    private static readonly global::System.Collections.Generic.IReadOnlyDictionary<global::System.Type, string> s_objectPayloadDiscriminators = CreateObjectPayloadDiscriminators();");
            builder.AppendLine("    private static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::System.Type> s_objectPayloadTypes = CreateObjectPayloadTypes();");
            builder.AppendLine("    private static readonly global::System.Text.Json.Serialization.JsonConverter<object?> s_objectPayloadConverter = new ObjectPayloadConverter();");
            builder.AppendLine("    internal static global::System.Text.Json.Serialization.JsonConverter<object?> GetObjectPayloadConverter() => s_objectPayloadConverter;");
            builder.AppendLine();
            builder.AppendLine("    public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo? GetTypeInfo(global::System.Type type, global::System.Text.Json.JsonSerializerOptions options)");
            builder.AppendLine("    {");
            builder.AppendLine("        return s_resolver.GetTypeInfo(type, options);");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    private static void ModifyTypeInfo(global::System.Text.Json.Serialization.Metadata.JsonTypeInfo jsonTypeInfo)");
            builder.AppendLine("    {");
            builder.AppendLine("        ApplyIgnoredMembers(jsonTypeInfo);");
            builder.AppendLine("        ApplyObjectPayloadConverters(jsonTypeInfo);");
            builder.AppendLine("        ApplyPolymorphism(jsonTypeInfo);");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    private static void ApplyIgnoredMembers(global::System.Text.Json.Serialization.Metadata.JsonTypeInfo jsonTypeInfo)");
            builder.AppendLine("    {");
            builder.AppendLine("        if (!s_ignoredMembers.TryGetValue(jsonTypeInfo.Type, out global::System.Collections.Generic.HashSet<string>? ignoredMembers))");
            builder.AppendLine("        {");
            builder.AppendLine("            return;");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        for (int i = jsonTypeInfo.Properties.Count - 1; i >= 0; i--)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (ignoredMembers.Contains(jsonTypeInfo.Properties[i].Name))");
            builder.AppendLine("            {");
            builder.AppendLine("                jsonTypeInfo.Properties.RemoveAt(i);");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    private static void ApplyObjectPayloadConverters(global::System.Text.Json.Serialization.Metadata.JsonTypeInfo jsonTypeInfo)");
            builder.AppendLine("    {");
            builder.AppendLine("        foreach (var converter in jsonTypeInfo.Options.Converters)");
            builder.AppendLine("            if (converter.CanConvert(typeof(object))) return;");
            builder.AppendLine("        if (jsonTypeInfo.Kind != global::System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)");
            builder.AppendLine("        {");
            builder.AppendLine("            return;");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        for (int i = 0; i < jsonTypeInfo.Properties.Count; i++)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (jsonTypeInfo.Properties[i].PropertyType == typeof(global::System.Object)");
            builder.AppendLine("                && jsonTypeInfo.Properties[i].CustomConverter is null)");
            builder.AppendLine("            {");
            builder.AppendLine("                jsonTypeInfo.Properties[i].CustomConverter = s_objectPayloadConverter;");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    private static void ApplyPolymorphism(global::System.Text.Json.Serialization.Metadata.JsonTypeInfo jsonTypeInfo)");
            builder.AppendLine("    {");
            builder.AppendLine("        if (jsonTypeInfo.Kind != global::System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)");
            builder.AppendLine("        {");
            builder.AppendLine("            return;");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        global::System.Type type = jsonTypeInfo.Type;");

            bool wroteIf = false;
            foreach (PolymorphismModel polymorphism in model.Polymorphisms.Where(static x => x.Kind != DockBaseKind.Object))
            {
                builder.Append(wroteIf ? "        else if (" : "        if (");
                builder.Append("type == typeof(");
                builder.Append(polymorphism.BaseTypeExpression);
                builder.AppendLine("))");
                builder.AppendLine("        {");
                builder.Append("            jsonTypeInfo.PolymorphismOptions = Create");
                builder.Append(ToMethodName(polymorphism.Kind));
                builder.AppendLine("Options();");
                builder.AppendLine("        }");
                wroteIf = true;
            }
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    private static global::System.Collections.Generic.IReadOnlyDictionary<global::System.Type, global::System.Collections.Generic.HashSet<string>> CreateIgnoredMembers()");
            builder.AppendLine("    {");
            builder.AppendLine("        var map = new global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Collections.Generic.HashSet<string>>();");

            foreach (IgnoredMembersModel ignoredMembers in model.IgnoredMembers)
            {
                builder.Append("        map[typeof(");
                builder.Append(ignoredMembers.TypeExpression);
                builder.AppendLine(")] = new global::System.Collections.Generic.HashSet<string>(global::System.StringComparer.Ordinal)");
                builder.AppendLine("        {");
                foreach (string memberName in ignoredMembers.MemberNames)
                {
                    builder.Append("            ");
                    builder.Append(EscapeString(memberName));
                    builder.AppendLine(",");
                }

                builder.AppendLine("        };");
            }

            builder.AppendLine("        return map;");
            builder.AppendLine("    }");
            builder.AppendLine();

            PolymorphismModel objectPolymorphism = model.Polymorphisms.Single(static x => x.Kind == DockBaseKind.Object);

            builder.AppendLine("    private static global::System.Collections.Generic.IReadOnlyDictionary<global::System.Type, string> CreateObjectPayloadDiscriminators()");
            builder.AppendLine("    {");
            builder.AppendLine("        var map = new global::System.Collections.Generic.Dictionary<global::System.Type, string>();");
            foreach (DerivedTypeModel derivedType in objectPolymorphism.DerivedTypes)
            {
                builder.Append("        map[typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.Append(")] = typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.Append(").FullName ?? typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.AppendLine(").Name;");
            }
            builder.AppendLine("        return map;");
            builder.AppendLine("    }");
            builder.AppendLine();

            builder.AppendLine("    private static global::System.Collections.Generic.IReadOnlyDictionary<string, global::System.Type> CreateObjectPayloadTypes()");
            builder.AppendLine("    {");
            builder.AppendLine("        var map = new global::System.Collections.Generic.Dictionary<string, global::System.Type>(global::System.StringComparer.Ordinal);");
            foreach (DerivedTypeModel derivedType in objectPolymorphism.DerivedTypes)
            {
                builder.Append("        map[typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.Append(").FullName ?? typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.Append(").Name] = typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.AppendLine(");");
            }
            builder.AppendLine("        return map;");
            builder.AppendLine("    }");
            builder.AppendLine();

            foreach (PolymorphismModel polymorphism in model.Polymorphisms.Where(static x => x.Kind != DockBaseKind.Object))
            {
                EmitOptionsFactory(builder, polymorphism, nullableReturn: false);
            }

            EmitObjectPayloadConverter(builder);

            builder.AppendLine("}");
            builder.AppendLine();
            builder.AppendLine("internal static class DockSystemTextJsonGenerated");
            builder.AppendLine("{");
            builder.AppendLine("    internal static DockSerializer CreateSerializer()");
            builder.AppendLine("    {");
            builder.AppendLine("        return new DockSerializer(new DockSystemTextJsonResolver());");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    internal static DockSerializer CreateSerializer(global::System.Type listType)");
            builder.AppendLine("    {");
            builder.AppendLine("        return new DockSerializer(listType, new DockSystemTextJsonResolver());");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            EmitRegistration(builder, model);

            return builder.ToString();
        }

        private static void EmitRegistration(StringBuilder builder, GenerationModel model)
        {
            builder.AppendLine("internal static class DockGeneratedMetadataRegistration");
            builder.AppendLine("{");
            builder.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
            builder.AppendLine("    internal static void Initialize()");
            builder.AppendLine("    {");
            builder.AppendLine("        global::Dock.Serializer.SystemTextJson.DockJsonMetadata.Register(");
            builder.AppendLine("            new DockSystemTextJsonResolver(),");
            builder.AppendLine("            new global::System.Collections.Generic.Dictionary<global::System.Type, global::Dock.Serializer.SystemTextJson.DockJsonType>");
            builder.AppendLine("            {");
            var accessors = new StringBuilder();
            int accessorIndex = 0;
            foreach (SerializableTypeModel type in model.SerializableTypes)
            {
                builder.Append("                [typeof(").Append(type.TypeExpression).Append(")] = new(");
                builder.Append(EscapeString(GenerationModelBuilder.GetTypeFullName(type.Type) + ", " + type.Type.ContainingAssembly.Name));
                builder.AppendLine(", static info =>");
                builder.AppendLine("                {");
                builder.AppendLine("                    for (int i = info.Properties.Count - 1; i >= 0; i--)");
                builder.AppendLine("                    {");
                builder.AppendLine("                        var property = info.Properties[i];");
                builder.AppendLine("                        switch (property.Name)");
                builder.AppendLine("                        {");
                bool dataContract = HasInheritedDataContract(type.Type);
                bool optIn = UsesOptInContract(type.Type);
                bool fieldsOnly = UsesFieldContract(type.Type);
                foreach (IPropertySymbol property in GenerationModelBuilder.GetAllPublicInstanceProperties(type.Type))
                {
                    AttributeData? dataMember = dataContract ? LegacyMemberEmitter.Attribute(property, "System.Runtime.Serialization.DataMemberAttribute") : null;
                    if (!LegacyMemberEmitter.IsIncluded(property, optIn, dataContract, fieldsOnly))
                    {
                        builder.Append("                            case ").Append(EscapeString(property.Name)).AppendLine(": info.Properties.RemoveAt(i); break;");
                        continue;
                    }
                    string name = LegacyMemberEmitter.Name(property, dataContract);
                    var emitDefault = dataMember?.NamedArguments.FirstOrDefault(static a => a.Key == "EmitDefaultValue") ?? default;
                    var order = dataMember?.NamedArguments.FirstOrDefault(static a => a.Key == "Order") ?? default;
                    if (name == property.Name && emitDefault.Key is null && order.Key is null)
                    {
                        continue;
                    }
                    builder.Append("                            case ").Append(EscapeString(property.Name)).AppendLine(":");
                    if (name is not null)
                    {
                        builder.Append("                                property.Name = ").Append(EscapeString(name)).AppendLine(";");
                    }
                    if (emitDefault.Value.Value is false)
                    {
                        builder.Append("                                property.ShouldSerialize = static (_, value) => !global::System.Collections.Generic.EqualityComparer<")
                            .Append(property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Append(">.Default.Equals((")
                            .Append(property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).AppendLine(")value!, default!);");
                    }
                    if (order.Value.Value is int propertyOrder)
                    {
                        builder.Append("                                property.Order = ").Append(propertyOrder).AppendLine(";");
                    }
                    builder.AppendLine("                                break;");
                }
                builder.AppendLine("                            default: break;");
                builder.AppendLine("                        }");
                builder.AppendLine("                    }");
                LegacyMemberEmitter.Emit(builder, accessors, type.Type, model, ref accessorIndex);
                builder.Append("                }");
                if (model.CollectionTypes.Contains(type.Type, SymbolEqualityComparer.Default))
                {
                    ITypeSymbol element = GenerationModelBuilder.GetCollectionValueType(type.Type, out bool dictionary)!;
                    builder.Append(dictionary ? ", dictionaryValueType: typeof(" : ", elementType: typeof(")
                        .Append(element.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Append(")");
                }
                builder.Append(", canAssignTo: static requested => requested == typeof(global::System.Object)");
                for (INamedTypeSymbol? current = type.Type; current is not null; current = current.BaseType)
                {
                    builder.Append(" || requested == typeof(").Append(current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Append(")");
                }
                foreach (INamedTypeSymbol contract in type.Type.AllInterfaces.Where(t => GenerationModelBuilder.IsAccessibleFromGeneratedCode(t, model.GeneratedAssembly!)))
                {
                    builder.Append(" || requested == typeof(").Append(contract.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Append(")");
                }
                builder.AppendLine("),");
            }
            foreach (ITypeSymbol collection in model.CollectionTypes)
            {
                string expression = collection.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                ITypeSymbol element = GenerationModelBuilder.GetCollectionValueType(collection, out bool dictionary)!;
                // A root collection may already be registered among directly serialized types.
                if (model.SerializableTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Type, collection)))
                {
                    continue;
                }
                builder.Append("                [typeof(").Append(expression).Append(")] = new(")
                    .Append(EscapeString(GenerationModelBuilder.GetTypeFullName(collection) + ", " + (collection.ContainingAssembly ?? element.ContainingAssembly).Name))
                    .Append(", static _ => { }, ").Append(dictionary ? "dictionaryValueType: typeof(" : "elementType: typeof(")
                    .Append(element.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Append("), canAssignTo: static requested => requested == typeof(")
                    .Append(expression).Append(") || requested == typeof(global::System.Object)");
                if (collection is INamedTypeSymbol namedCollection)
                {
                    foreach (INamedTypeSymbol contract in namedCollection.AllInterfaces.Where(t => GenerationModelBuilder.IsAccessibleFromGeneratedCode(t, model.GeneratedAssembly!)))
                    {
                        builder.Append(" || requested == typeof(").Append(contract.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Append(")");
                    }
                }
                builder.AppendLine("),");
            }
            builder.AppendLine("            },");
            builder.AppendLine("            new global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Collections.Generic.IReadOnlyDictionary<global::System.Type, global::System.Func<object>>>");
            builder.AppendLine("            {");
            foreach (INamedTypeSymbol collection in model.CollectionTypes.OfType<INamedTypeSymbol>().Where(static t => t.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IList<T>"))
            {
                string expression = collection.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                string element = collection.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                builder.Append("                [typeof(").Append(expression).AppendLine(")] = new global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<object>>");
                builder.AppendLine("                {");
                builder.Append("                    [typeof(global::System.Collections.Generic.List<>)] = static () => new global::System.Collections.Generic.List<").Append(element).AppendLine(">(),");
                builder.Append("                    [typeof(global::System.Collections.ObjectModel.ObservableCollection<>)] = static () => new global::System.Collections.ObjectModel.ObservableCollection<").Append(element).AppendLine(">(),");
                foreach (INamedTypeSymbol listType in model.ListTypes)
                {
                    // Emit only closed constructions whose constraints are satisfied by this element.
                    ITypeParameterSymbol parameter = listType.TypeParameters[0];
                    ITypeSymbol argument = collection.TypeArguments[0];
                    if (parameter.HasReferenceTypeConstraint && !argument.IsReferenceType
                        || parameter.HasValueTypeConstraint && !argument.IsValueType
                        || parameter.HasConstructorConstraint || !parameter.ConstraintTypes.IsEmpty)
                    {
                        continue;
                    }
                    string openType = listType.ConstructUnboundGenericType().ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    string closedType = listType.Construct(argument).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    builder.Append("                    [typeof(").Append(openType).Append(")] = static () => new ").Append(closedType).AppendLine("(),");
                }
                builder.AppendLine("                },");
            }
            builder.AppendLine("            });");
            builder.AppendLine("    }");
            builder.Append(accessors);
            builder.AppendLine("}");
        }

        internal static bool HasInheritedDataContract(INamedTypeSymbol type)
        {
            for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
                if (LegacyMemberEmitter.Attribute(current, "System.Runtime.Serialization.DataContractAttribute") is not null) return true;
            return false;
        }

        internal static bool UsesOptInContract(INamedTypeSymbol type) => GetMemberSerializationMode(type) == 1;

        internal static bool UsesFieldContract(INamedTypeSymbol type) => GetMemberSerializationMode(type) == 2;

        private static int GetMemberSerializationMode(INamedTypeSymbol type)
        {
            for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
            {
                AttributeData? jsonObject = LegacyMemberEmitter.Attribute(current, "Newtonsoft.Json.JsonObjectAttribute");
                if (jsonObject is not null)
                {
                    object? mode = jsonObject.NamedArguments.FirstOrDefault(a => a.Key == "MemberSerialization").Value.Value
                        ?? (jsonObject.ConstructorArguments.Length > 0 ? jsonObject.ConstructorArguments[0].Value : null);
                    return mode is int serialization ? serialization : 0;
                }
            }
            return HasInheritedDataContract(type) ? 1 : 0;
        }

        private static void EmitOptionsFactory(StringBuilder builder, PolymorphismModel polymorphism, bool nullableReturn)
        {
            builder.Append("    private static global::System.Text.Json.Serialization.Metadata.JsonPolymorphismOptions");
            if (nullableReturn)
            {
                builder.Append('?');
            }

            builder.Append(" Create");
            builder.Append(ToMethodName(polymorphism.Kind));
            builder.AppendLine("Options()");
            builder.AppendLine("    {");

            if (nullableReturn && polymorphism.DerivedTypes.Length == 0)
            {
                builder.AppendLine("        return null;");
                builder.AppendLine("    }");
                builder.AppendLine();
                return;
            }

            builder.AppendLine("        var options = new global::System.Text.Json.Serialization.Metadata.JsonPolymorphismOptions");
            builder.AppendLine("        {");
            builder.AppendLine("            TypeDiscriminatorPropertyName = \"$type\",");
            builder.Append("            UnknownDerivedTypeHandling = ");
            builder.Append(polymorphism.UnknownDerivedTypeHandling);
            builder.AppendLine(",");
            builder.Append("            IgnoreUnrecognizedTypeDiscriminators = ");
            builder.Append(polymorphism.IgnoreUnrecognizedTypeDiscriminators ? "true" : "false");
            builder.AppendLine(",");
            builder.AppendLine("        };");
            builder.AppendLine();

            foreach (DerivedTypeModel derivedType in polymorphism.DerivedTypes)
            {
                builder.Append("        options.DerivedTypes.Add(new global::System.Text.Json.Serialization.Metadata.JsonDerivedType(typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.Append("), typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.Append(").FullName ?? typeof(");
                builder.Append(derivedType.TypeExpression);
                builder.AppendLine(").Name));");
            }

            builder.AppendLine();
            builder.AppendLine("        return options;");
            builder.AppendLine("    }");
            builder.AppendLine();
        }

        private static void EmitObjectPayloadConverter(StringBuilder builder)
        {
            builder.AppendLine("    private sealed class ObjectPayloadConverter : global::System.Text.Json.Serialization.JsonConverter<object?>");
            builder.AppendLine("    {");
            builder.AppendLine("        public override object? Read(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (reader.TokenType == global::System.Text.Json.JsonTokenType.Null)");
            builder.AppendLine("            {");
            builder.AppendLine("                return null;");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("""
            switch (reader.TokenType)
            {
                case global::System.Text.Json.JsonTokenType.String:
                    string? scalarText = reader.GetString();
                    // Newtonsoft only infers ISO dates containing a time component.
                    return scalarText is { Length: >= 19 } && reader.TryGetDateTime(out var date) ? (object)date : scalarText;
                case global::System.Text.Json.JsonTokenType.True: return true;
                case global::System.Text.Json.JsonTokenType.False: return false;
                case global::System.Text.Json.JsonTokenType.Number:
                    if (reader.TryGetInt64(out long integer)) return integer;
                    using (var number = global::System.Text.Json.JsonDocument.ParseValue(ref reader))
                    {
                        string text = number.RootElement.GetRawText();
                        return text.IndexOf('.') >= 0 || text.IndexOf('e') >= 0 || text.IndexOf('E') >= 0
                            ? (object)number.RootElement.GetDouble()
                            : global::System.Numerics.BigInteger.Parse(text, global::System.Globalization.CultureInfo.InvariantCulture);
                    }
            }
            """);
            builder.AppendLine("            using global::System.Text.Json.JsonDocument document = global::System.Text.Json.JsonDocument.ParseValue(ref reader);");
            builder.AppendLine("            if (document.RootElement.ValueKind != global::System.Text.Json.JsonValueKind.Object)");
            builder.AppendLine("            {");
            builder.AppendLine("                return document.RootElement.Clone();");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            if (document.RootElement.TryGetProperty(\"$ref\", out global::System.Text.Json.JsonElement reference))");
            builder.AppendLine("            {");
            builder.AppendLine("                return options.ReferenceHandler!.CreateResolver().ResolveReference(reference.GetString()!);");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            int discriminatorIndex = -1;");
            builder.AppendLine("            string? discriminator = null;");
            builder.AppendLine("            global::System.Type? payloadType = null;");
            builder.AppendLine("            int propertyIndex = 0;");
            builder.AppendLine("            int typePropertyCount = 0;");
            builder.AppendLine("            foreach (global::System.Text.Json.JsonProperty property in document.RootElement.EnumerateObject())");
            builder.AppendLine("            {");
            builder.AppendLine("                if (property.NameEquals(\"$type\")");
            builder.AppendLine("                    && property.Value.ValueKind == global::System.Text.Json.JsonValueKind.String)");
            builder.AppendLine("                {");
            builder.AppendLine("                    typePropertyCount++;");
            builder.AppendLine("                    string? candidate = property.Value.GetString();");
            builder.AppendLine("                    if (!string.IsNullOrWhiteSpace(candidate)");
            builder.AppendLine("                        && discriminatorIndex < 0");
            builder.AppendLine("                        && s_objectPayloadTypes.TryGetValue(candidate, out payloadType))");
            builder.AppendLine("                    {");
            builder.AppendLine("                        discriminatorIndex = propertyIndex;");
            builder.AppendLine("                        discriminator = candidate;");
            builder.AppendLine("                    }");
            builder.AppendLine("                }");
            builder.AppendLine();
            builder.AppendLine("                propertyIndex++;");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            if (typePropertyCount > 1)");
            builder.AppendLine("            {");
            builder.AppendLine("                return document.RootElement.Clone();");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            if (discriminatorIndex < 0 || payloadType is null)");
            builder.AppendLine("            {");
            builder.AppendLine("                return document.RootElement.Clone();");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            using var stream = new global::System.IO.MemoryStream();");
            builder.AppendLine("            using (var writer = new global::System.Text.Json.Utf8JsonWriter(stream))");
            builder.AppendLine("            {");
            builder.AppendLine("                writer.WriteStartObject();");
            builder.AppendLine("                int propertyWriteIndex = 0;");
            builder.AppendLine("                foreach (global::System.Text.Json.JsonProperty property in document.RootElement.EnumerateObject())");
            builder.AppendLine("                {");
            builder.AppendLine("                    if (propertyWriteIndex != discriminatorIndex)");
            builder.AppendLine("                    {");
            builder.AppendLine("                        property.WriteTo(writer);");
            builder.AppendLine("                    }");
            builder.AppendLine();
            builder.AppendLine("                    propertyWriteIndex++;");
            builder.AppendLine("                }");
            builder.AppendLine("                writer.WriteEndObject();");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            return global::System.Text.Json.JsonSerializer.Deserialize(stream.ToArray(), options.GetTypeInfo(payloadType));");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        public override void Write(global::System.Text.Json.Utf8JsonWriter writer, object? value, global::System.Text.Json.JsonSerializerOptions options)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (value is null)");
            builder.AppendLine("            {");
            builder.AppendLine("                writer.WriteNullValue();");
            builder.AppendLine("                return;");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            if (value is global::System.Text.Json.JsonElement jsonElement)");
            builder.AppendLine("            {");
            builder.AppendLine("                jsonElement.WriteTo(writer);");
            builder.AppendLine("                return;");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            if (value is global::System.Text.Json.JsonDocument jsonDocument)");
            builder.AppendLine("            {");
            builder.AppendLine("                jsonDocument.RootElement.WriteTo(writer);");
            builder.AppendLine("                return;");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("""
            // JSON scalars do not carry a type discriminator in Newtonsoft layouts.
            // Write them directly, without reflection or payload registration.
            switch (value)
            {
                case string scalar: writer.WriteStringValue(scalar); return;
                case bool scalar: writer.WriteBooleanValue(scalar); return;
                case char scalar: writer.WriteStringValue(scalar.ToString()); return;
                case byte scalar: writer.WriteNumberValue(scalar); return;
                case sbyte scalar: writer.WriteNumberValue(scalar); return;
                case short scalar: writer.WriteNumberValue(scalar); return;
                case ushort scalar: writer.WriteNumberValue(scalar); return;
                case int scalar: writer.WriteNumberValue(scalar); return;
                case uint scalar: writer.WriteNumberValue(scalar); return;
                case long scalar: writer.WriteNumberValue(scalar); return;
                case ulong scalar: writer.WriteNumberValue(scalar); return;
                case decimal scalar: WriteFloatingPoint(writer, scalar.ToString(global::System.Globalization.CultureInfo.InvariantCulture)); return;
                case float scalar:
                    if (float.IsFinite(scalar)) WriteFloatingPoint(writer, scalar.ToString("R", global::System.Globalization.CultureInfo.InvariantCulture));
                    else writer.WriteStringValue(scalar.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
                    return;
                case double scalar:
                    if (double.IsFinite(scalar)) WriteFloatingPoint(writer, scalar.ToString("R", global::System.Globalization.CultureInfo.InvariantCulture));
                    else writer.WriteStringValue(scalar.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
                    return;
                case global::System.DateTime scalar: writer.WriteStringValue(scalar); return;
                case global::System.DateTimeOffset scalar: writer.WriteStringValue(scalar); return;
                case global::System.Guid scalar: writer.WriteStringValue(scalar); return;
                case global::System.TimeSpan scalar: writer.WriteStringValue(scalar.ToString("c", global::System.Globalization.CultureInfo.InvariantCulture)); return;
                case global::System.Uri scalar: writer.WriteStringValue(scalar.OriginalString); return;
                case global::System.Numerics.BigInteger scalar:
                    writer.WriteRawValue(scalar.ToString(global::System.Globalization.CultureInfo.InvariantCulture)); return;
            }
            """);
            builder.AppendLine("            global::System.Type runtimeType = value.GetType();");
            builder.AppendLine("            if (!s_objectPayloadDiscriminators.TryGetValue(runtimeType, out string? discriminator))");
            builder.AppendLine("            {");
            builder.AppendLine("                throw new global::System.NotSupportedException($\"Dock source-generated object payload '{runtimeType.FullName ?? runtimeType.Name}' is not registered.\");");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            global::System.Text.Json.JsonElement element = global::System.Text.Json.JsonSerializer.SerializeToElement(value, options.GetTypeInfo(runtimeType));");
            builder.AppendLine("            if (element.ValueKind != global::System.Text.Json.JsonValueKind.Object)");
            builder.AppendLine("            {");
            builder.AppendLine("                throw new global::System.NotSupportedException(\"Dock source-generated object payloads must serialize as JSON objects.\");");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            if (element.TryGetProperty(\"$ref\", out _))");
            builder.AppendLine("            {");
            builder.AppendLine("                element.WriteTo(writer);");
            builder.AppendLine("                return;");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            writer.WriteStartObject();");
            builder.AppendLine("            writer.WriteString(\"$type\", discriminator);");
            builder.AppendLine("            foreach (global::System.Text.Json.JsonProperty property in element.EnumerateObject())");
            builder.AppendLine("            {");
            builder.AppendLine("                property.WriteTo(writer);");
            builder.AppendLine("            }");
            builder.AppendLine("            writer.WriteEndObject();");
            builder.AppendLine("        }");
            builder.AppendLine("""
            private static void WriteFloatingPoint(global::System.Text.Json.Utf8JsonWriter writer, string text)
            {
                // Keep the floating-point token shape so an integral-valued float or
                // decimal still reads as double in an object slot, as with Newtonsoft.
                bool fractional = text.IndexOf('.') >= 0 || text.IndexOf('e') >= 0 || text.IndexOf('E') >= 0;
                writer.WriteRawValue(fractional ? text : text + ".0");
            }
            """);
            builder.AppendLine("    }");
            builder.AppendLine();
        }

        private static string ToFieldName(DockBaseKind kind)
        {
            return kind switch
            {
                DockBaseKind.Dockable => "dockable",
                DockBaseKind.Dock => "dock",
                DockBaseKind.RootDock => "rootDock",
                DockBaseKind.DockWindow => "dockWindow",
                DockBaseKind.DocumentTemplate => "documentTemplate",
                DockBaseKind.ToolTemplate => "toolTemplate",
                DockBaseKind.Object => "object",
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
        }

        private static string ToMethodName(DockBaseKind kind)
        {
            return kind switch
            {
                DockBaseKind.Dockable => "Dockable",
                DockBaseKind.Dock => "Dock",
                DockBaseKind.RootDock => "RootDock",
                DockBaseKind.DockWindow => "DockWindow",
                DockBaseKind.DocumentTemplate => "DocumentTemplate",
                DockBaseKind.ToolTemplate => "ToolTemplate",
                DockBaseKind.Object => "Object",
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
        }

        private static string EscapeString(string value)
        {
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }
}
