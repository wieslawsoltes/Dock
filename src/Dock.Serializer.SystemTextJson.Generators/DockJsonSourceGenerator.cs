// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Dock.Serializer.SystemTextJson.Generators;

[Generator]
public sealed class DockJsonSourceGenerator : IIncrementalGenerator
{
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

        public static readonly DiagnosticDescriptor MissingActivation =
            new(
                id: "DSTJ002",
                title: "Dock JSON source generation is not enabled",
                messageFormat: "Add [assembly: DockJsonSourceGeneration] before using DockJsonSerializableAttribute",
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
        string ContextSource,
        ImmutableArray<GeneratedSourceArtifact> AdditionalSources,
        ImmutableArray<string> ContextTypes,
        ImmutableArray<PolymorphismModel> Polymorphisms,
        ImmutableArray<IgnoredMembersModel> IgnoredMembers)
    {
        public static GenerationModel Empty(ImmutableArray<Diagnostic> diagnostics)
        {
            return new GenerationModel(
                ShouldGenerate: false,
                Diagnostics: diagnostics,
                ContextSource: string.Empty,
                AdditionalSources: ImmutableArray<GeneratedSourceArtifact>.Empty,
                ContextTypes: ImmutableArray<string>.Empty,
                Polymorphisms: ImmutableArray<PolymorphismModel>.Empty,
                IgnoredMembers: ImmutableArray<IgnoredMembersModel>.Empty);
        }
    }

    private static class GenerationModelBuilder
    {
        private static readonly SymbolDisplayFormat s_fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

        public static GenerationModel Build(Compilation compilation, System.Threading.CancellationToken cancellationToken)
        {
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
            ImmutableArray<RegistrationCandidate> registeredTypes = GetRegisteredTypes(compilation, diagnostics, cancellationToken);
            bool isActivated = HasAssemblyAttribute(compilation, MetadataNames.SourceGenerationAttribute);

            if (!isActivated)
            {
                foreach (RegistrationCandidate registeredType in registeredTypes)
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MissingActivation,
                            registeredType.Location ?? Location.None));
                }

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

            ImmutableArray<SerializableTypeModel> serializableTypes =
                GetSerializableTypes(compilation, dockSymbols!, registeredTypes, cancellationToken);

            ImmutableArray<IgnoredMembersModel> ignoredMembers =
                BuildIgnoredMembers(serializableTypes, dockSymbols!);

            ImmutableArray<PolymorphismModel> polymorphisms =
                BuildPolymorphisms(serializableTypes, diagnostics);

            ImmutableArray<string> contextTypes =
                BuildContextTypes(serializableTypes, dockSymbols!);
            string contextSource = SourceEmitter.EmitContext(contextTypes);
            ImmutableArray<GeneratedSourceArtifact> additionalSources =
                SystemTextJsonContextGenerator.Generate(compilation, contextSource, cancellationToken);

            return new GenerationModel(
                ShouldGenerate: true,
                Diagnostics: diagnostics.ToImmutable(),
                ContextSource: contextSource,
                AdditionalSources: additionalSources,
                ContextTypes: contextTypes,
                Polymorphisms: polymorphisms,
                IgnoredMembers: ignoredMembers);
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

            foreach (RegistrationCandidate registration in registeredTypes)
            {
                if (seen.Add(registration.Type))
                {
                    bool isObjectPayload = !IsAssignableToAnyDockContract(registration.Type, symbols);
                    result.Add(CreateSerializableType(registration.Type, registration.Location, isObjectPayload, symbols));
                }
            }

            return result
                .OrderBy(static x => x.TypeExpression, StringComparer.Ordinal)
                .ToImmutableArray();
        }

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
                    serializableTypes.Where(static x => x.IsObjectPayload).Select(ToDerivedModel),
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

        private static IEnumerable<IPropertySymbol> GetAllPublicInstanceProperties(INamedTypeSymbol typeSymbol)
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
            return IsCommandType(propertySymbol.Type, symbols.ICommand)
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

            if (typeSymbol.TypeArguments.Any(static x => x.TypeKind == TypeKind.TypeParameter))
            {
                return true;
            }

            return typeSymbol.ContainingType is not null && HasOpenTypeParameters(typeSymbol.ContainingType);
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

        private static bool IsAccessibleFromGeneratedCode(INamedTypeSymbol typeSymbol, IAssemblySymbol generatedAssembly)
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

            return true;
        }

        private static bool IsDirectlyAccessibleFromGeneratedCode(INamedTypeSymbol typeSymbol, IAssemblySymbol generatedAssembly)
        {
            return typeSymbol.DeclaredAccessibility switch
            {
                Accessibility.Public => true,
                Accessibility.Internal => typeSymbol.ContainingAssembly.GivesAccessTo(generatedAssembly),
                Accessibility.ProtectedOrInternal => typeSymbol.ContainingAssembly.GivesAccessTo(generatedAssembly),
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
            if (typeSymbol.ContainingType is null)
            {
                string namespacePrefix = typeSymbol.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : typeSymbol.ContainingNamespace.ToDisplayString() + ".";
                return namespacePrefix + typeSymbol.MetadataName;
            }

            var containingTypes = new Stack<string>();
            INamedTypeSymbol? current = typeSymbol;
            while (current is not null)
            {
                containingTypes.Push(current.MetadataName);
                current = current.ContainingType;
            }

            string namespacePrefixNested = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : typeSymbol.ContainingNamespace.ToDisplayString() + ".";
            return namespacePrefixNested + string.Join("+", containingTypes);
        }
    }

    private static class SystemTextJsonContextGenerator
    {
        public static ImmutableArray<GeneratedSourceArtifact> Generate(
            Compilation compilation,
            string contextSource,
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
            driver = driver.RunGenerators(augmentedCompilation, cancellationToken);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            if (runResult.Results.Length == 0)
            {
                return ImmutableArray<GeneratedSourceArtifact>.Empty;
            }

            return runResult.Results[0].GeneratedSources
                .Select(static x => new GeneratedSourceArtifact("SystemTextJson." + x.HintName, x.SourceText.ToString()))
                .ToImmutableArray();
        }

        private static ISourceGenerator? CreateGenerator(Compilation compilation)
        {
            Assembly? assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(static x => string.Equals(x.GetName().Name, "System.Text.Json.SourceGeneration", StringComparison.Ordinal));

            if (assembly is null)
            {
                string? assemblyPath = TryGetAssemblyPath(compilation);
                if (assemblyPath is null)
                {
                    return null;
                }

                assembly = Assembly.LoadFrom(assemblyPath);
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

            return Directory.EnumerateFiles(
                    analyzersDirectory,
                    "System.Text.Json.SourceGeneration.dll",
                    SearchOption.AllDirectories)
                .OrderBy(static x => x, StringComparer.Ordinal)
                .FirstOrDefault();
        }
    }

    private static class SourceEmitter
    {
        public static string EmitContext(ImmutableArray<string> contextTypes)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            builder.AppendLine("namespace Dock.Serializer.SystemTextJson;");
            builder.AppendLine();
            builder.AppendLine("[global::System.Text.Json.Serialization.JsonSourceGenerationOptions(GenerationMode = global::System.Text.Json.Serialization.JsonSourceGenerationMode.Metadata)]");

            foreach (string contextType in contextTypes)
            {
                builder.Append("[global::System.Text.Json.Serialization.JsonSerializable(typeof(");
                builder.Append(contextType);
                builder.AppendLine("))]");
            }

            builder.AppendLine("internal sealed partial class DockSystemTextJsonContext : global::System.Text.Json.Serialization.JsonSerializerContext");
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
            builder.AppendLine("    private static readonly global::System.Text.Json.Serialization.Metadata.IJsonTypeInfoResolver s_resolver = global::System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver.WithAddedModifier(DockSystemTextJsonContext.Default, ModifyTypeInfo);");
            builder.AppendLine("    private static readonly global::System.Collections.Generic.IReadOnlyDictionary<global::System.Type, global::System.Collections.Generic.HashSet<string>> s_ignoredMembers = CreateIgnoredMembers();");
            builder.AppendLine("    private static readonly global::System.Collections.Generic.IReadOnlyDictionary<global::System.Type, string> s_objectPayloadDiscriminators = CreateObjectPayloadDiscriminators();");
            builder.AppendLine("    private static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::System.Type> s_objectPayloadTypes = CreateObjectPayloadTypes();");
            builder.AppendLine("    private static readonly global::System.Text.Json.Serialization.JsonConverter<object?> s_objectPayloadConverter = new ObjectPayloadConverter();");
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
            builder.AppendLine("        if (jsonTypeInfo.Kind != global::System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)");
            builder.AppendLine("        {");
            builder.AppendLine("            return;");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        for (int i = 0; i < jsonTypeInfo.Properties.Count; i++)");
            builder.AppendLine("        {");
            builder.AppendLine("            if (jsonTypeInfo.Properties[i].PropertyType == typeof(global::System.Object))");
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

            return builder.ToString();
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
            builder.AppendLine("            using global::System.Text.Json.JsonDocument document = global::System.Text.Json.JsonDocument.ParseValue(ref reader);");
            builder.AppendLine("            if (document.RootElement.ValueKind != global::System.Text.Json.JsonValueKind.Object)");
            builder.AppendLine("            {");
            builder.AppendLine("                throw new global::System.NotSupportedException(\"Dock source-generated object payloads must serialize as JSON objects.\");");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            if (!document.RootElement.TryGetProperty(\"$type\", out global::System.Text.Json.JsonElement typeElement))");
            builder.AppendLine("            {");
            builder.AppendLine("                throw new global::System.Text.Json.JsonException(\"Missing $type discriminator for Dock object payload.\");");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            string? discriminator = typeElement.GetString();");
            builder.AppendLine("            if (string.IsNullOrWhiteSpace(discriminator) || !s_objectPayloadTypes.TryGetValue(discriminator, out global::System.Type? payloadType))");
            builder.AppendLine("            {");
            builder.AppendLine("                throw new global::System.NotSupportedException($\"Dock source-generated object payload '{discriminator}' is not registered.\");");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            using var stream = new global::System.IO.MemoryStream();");
            builder.AppendLine("            using (var writer = new global::System.Text.Json.Utf8JsonWriter(stream))");
            builder.AppendLine("            {");
            builder.AppendLine("                writer.WriteStartObject();");
            builder.AppendLine("                foreach (global::System.Text.Json.JsonProperty property in document.RootElement.EnumerateObject())");
            builder.AppendLine("                {");
            builder.AppendLine("                    if (!property.NameEquals(\"$type\"))");
            builder.AppendLine("                    {");
            builder.AppendLine("                        property.WriteTo(writer);");
            builder.AppendLine("                    }");
            builder.AppendLine("                }");
            builder.AppendLine("                writer.WriteEndObject();");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            return global::System.Text.Json.JsonSerializer.Deserialize(stream.ToArray(), payloadType, options);");
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
            builder.AppendLine("            global::System.Type runtimeType = value.GetType();");
            builder.AppendLine("            if (!s_objectPayloadDiscriminators.TryGetValue(runtimeType, out string? discriminator))");
            builder.AppendLine("            {");
            builder.AppendLine("                throw new global::System.NotSupportedException($\"Dock source-generated object payload '{runtimeType.FullName ?? runtimeType.Name}' is not registered.\");");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            global::System.Text.Json.JsonElement element = global::System.Text.Json.JsonSerializer.SerializeToElement(value, runtimeType, options);");
            builder.AppendLine("            if (element.ValueKind != global::System.Text.Json.JsonValueKind.Object)");
            builder.AppendLine("            {");
            builder.AppendLine("                throw new global::System.NotSupportedException(\"Dock source-generated object payloads must serialize as JSON objects.\");");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            writer.WriteStartObject();");
            builder.AppendLine("            writer.WriteString(\"$type\", discriminator);");
            builder.AppendLine("            foreach (global::System.Text.Json.JsonProperty property in element.EnumerateObject())");
            builder.AppendLine("            {");
            builder.AppendLine("                if (!property.NameEquals(\"$type\"))");
            builder.AppendLine("                {");
            builder.AppendLine("                    property.WriteTo(writer);");
            builder.AppendLine("                }");
            builder.AppendLine("            }");
            builder.AppendLine("            writer.WriteEndObject();");
            builder.AppendLine("        }");
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
