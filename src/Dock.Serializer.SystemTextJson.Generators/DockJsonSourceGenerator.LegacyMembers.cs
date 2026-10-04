// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Dock.Serializer.SystemTextJson.Generators;

public sealed partial class DockJsonSourceGenerator
{
    // STJ does not interpret DataMember or generate access to its nonpublic members.
    // Emit these contracts separately so the compatibility facade can opt into them.
    private static class LegacyMemberEmitter
    {
        internal static IEnumerable<ISymbol> GetDataMembers(INamedTypeSymbol type)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
            {
                foreach (ISymbol member in current.GetMembers())
                {
                    if (member.IsStatic || member is not (IFieldSymbol or IPropertySymbol)
                        || member is IPropertySymbol { IsIndexer: true }
                        || !names.Add(member.Name)
                        || !member.GetAttributes().Any(static a => a.AttributeClass?.ToDisplayString() == "System.Runtime.Serialization.DataMemberAttribute"))
                    {
                        continue;
                    }
                    yield return member;
                }
            }
        }

        internal static void Emit(StringBuilder builder, StringBuilder accessors, INamedTypeSymbol type, GenerationModel model, ref int index)
        {
            foreach (ISymbol member in GetDataMembers(type))
            {
                IPropertySymbol? property = member as IPropertySymbol;
                IFieldSymbol? field = member as IFieldSymbol;
                if (property is not null && (property.GetMethod is null || property.SetMethod is null))
                {
                    continue;
                }
                bool directGet = IsAccessible(property?.GetMethod ?? member, model.GeneratedAssembly!);
                bool directSet = IsAccessible(property?.SetMethod ?? member, model.GeneratedAssembly!)
                    && property?.SetMethod?.IsInitOnly != true && field?.IsReadOnly != true;
                if (field is null && property!.DeclaredAccessibility == Accessibility.Public && directGet && directSet)
                {
                    continue;
                }

                ITypeSymbol valueType = field?.Type ?? property!.Type;
                string valueExpression = valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                string ownerExpression = member.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                string instance = member.ContainingType.IsValueType
                    ? "global::System.Runtime.CompilerServices.Unsafe.Unbox<" + ownerExpression + ">(obj)"
                    : "((" + ownerExpression + ")obj)";
                bool needsAccessor = !directGet || !directSet;
                string helper = "LegacyMember" + index++;
                var declaringTypes = new Stack<INamedTypeSymbol>();
                for (INamedTypeSymbol? owner = member.ContainingType; owner is not null; owner = owner.ContainingType)
                {
                    declaringTypes.Push(owner);
                }
                ITypeSymbol[] arguments = declaringTypes.SelectMany(static t => t.TypeArguments).ToArray();
                string call = helper + (arguments.Length == 0 ? "" : "<" + string.Join(", ", arguments.Select(static t => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))) + ">");

                if (needsAccessor && (!model.SupportsUnsafeAccessors || arguments.Length > 0 && !model.SupportsGenericUnsafeAccessors))
                {
                    // A generated extern accessor cannot be implemented on .NET 6. A
                    // descriptive failure is preferable to silently discarding saved data.
                    builder.Append("                    throw new global::System.NotSupportedException(")
                        .Append(Literal("Legacy DataMember '" + member.ToDisplayString() + "' requires an accessible getter/setter or .NET 8 or later (.NET 9 for generic declaring types)."))
                        .AppendLine(");");
                    continue;
                }

                string argument = (member.ContainingType.IsValueType ? "ref " : "") + instance;
                string get = directGet ? instance + ".@" + member.Name : call + ".Get(" + argument + ")";
                string set = directSet ? instance + ".@" + member.Name + " = (" + valueExpression + ")value!"
                    : field is not null ? call + ".Get(" + argument + ") = (" + valueExpression + ")value!"
                    : call + ".Set(" + argument + ", (" + valueExpression + ")value!)";
                if (needsAccessor)
                {
                    INamedTypeSymbol definition = member.ContainingType.OriginalDefinition;
                    ISymbol original = member.OriginalDefinition;
                    ITypeSymbol originalValueType = original is IFieldSymbol originalField ? originalField.Type : ((IPropertySymbol)original).Type;
                    string originalValue = originalValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    ITypeParameterSymbol[] parameters = declaringTypes.SelectMany(static t => t.OriginalDefinition.TypeParameters).ToArray();
                    accessors.Append("    private static class ").Append(helper);
                    if (parameters.Length > 0)
                    {
                        accessors.Append('<').Append(string.Join(", ", parameters.Select(static p => "@" + p.Name))).Append('>');
                    }
                    accessors.AppendLine();
                    foreach (ITypeParameterSymbol parameter in parameters)
                    {
                        EmitConstraints(accessors, parameter);
                    }
                    accessors.AppendLine("    {");
                    string receiver = (definition.IsValueType ? "ref " : "") + definition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " instance";
                    if (field is not null)
                    {
                        EmitAccessor(accessors, "Field", member.MetadataName, "ref " + originalValue, "Get", receiver);
                    }
                    else
                    {
                        if (!directGet)
                        {
                            EmitAccessor(accessors, "Method", property!.GetMethod!.MetadataName, originalValue, "Get", receiver);
                        }
                        if (!directSet)
                        {
                            EmitAccessor(accessors, "Method", property!.SetMethod!.MetadataName, "void", "Set", receiver + ", " + originalValue + " value");
                        }
                    }
                    accessors.AppendLine("    }");
                }

                AttributeData dataMember = member.GetAttributes().First(static a => a.AttributeClass?.ToDisplayString() == "System.Runtime.Serialization.DataMemberAttribute");
                string name = dataMember.NamedArguments.FirstOrDefault(static a => a.Key == "Name").Value.Value as string ?? member.Name;
                builder.AppendLine("                    {");
                builder.AppendLine("                        global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo? member = null;");
                builder.AppendLine("                        foreach (var candidate in info.Properties)");
                builder.AppendLine("                        {");
                builder.Append("                            if (candidate.Name == ").Append(Literal(name)).AppendLine(") { member = candidate; break; }");
                builder.AppendLine("                        }");
                builder.AppendLine("                        if (member is null)");
                builder.AppendLine("                        {");
                builder.Append("                            member = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<")
                    .Append(valueExpression).AppendLine(">(info.Options,");
                builder.Append("                                new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<").Append(valueExpression).AppendLine(">");
                builder.AppendLine("                                {");
                builder.Append("                                    IsProperty = ").Append(field is null ? "true" : "false").AppendLine(",");
                builder.AppendLine("                                    IsPublic = true, HasJsonInclude = true,");
                builder.Append("                                    DeclaringType = typeof(").Append(ownerExpression).AppendLine("),");
                builder.Append("                                    PropertyName = ").Append(Literal(name)).AppendLine(",");
                builder.AppendLine("                                });");
                builder.AppendLine("                            info.Properties.Add(member);");
                builder.AppendLine("                        }");
                builder.Append("                        member.Get = static obj => ").Append(get).AppendLine(";");
                builder.Append("                        member.Set = static (obj, value) => ").Append(set).AppendLine(";");
                if (valueType.SpecialType == SpecialType.System_Object)
                {
                    builder.AppendLine("                        member.CustomConverter ??= DockSystemTextJsonResolver.GetObjectPayloadConverter();");
                }
                if (dataMember.NamedArguments.FirstOrDefault(static a => a.Key == "EmitDefaultValue").Value.Value is false)
                {
                    builder.Append("                        member.ShouldSerialize = static (_, value) => !global::System.Collections.Generic.EqualityComparer<")
                        .Append(valueExpression).Append(">.Default.Equals((").Append(valueExpression).AppendLine(")value!, default!);");
                }
                if (dataMember.NamedArguments.FirstOrDefault(static a => a.Key == "Order").Value.Value is int order)
                {
                    builder.Append("                        member.Order = ").Append(order).AppendLine(";");
                }
                builder.AppendLine("                    }");
            }
        }

        private static bool IsAccessible(ISymbol symbol, IAssemblySymbol assembly) => symbol.DeclaredAccessibility == Accessibility.Public
            || (symbol.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal
                && (SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, assembly) || symbol.ContainingAssembly.GivesAccessTo(assembly)));

        private static void EmitAccessor(StringBuilder builder, string kind, string name, string result, string method, string parameters)
        {
            builder.Append("    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.")
                .Append(kind).Append(", Name = ").Append(Literal(name)).AppendLine(")]");
            builder.Append("    internal static extern ").Append(result).Append(' ').Append(method).Append('(').Append(parameters).AppendLine(");");
        }

        private static void EmitConstraints(StringBuilder builder, ITypeParameterSymbol parameter)
        {
            var constraints = new List<string>();
            if (parameter.HasUnmanagedTypeConstraint) constraints.Add("unmanaged");
            else if (parameter.HasValueTypeConstraint) constraints.Add("struct");
            else if (parameter.HasReferenceTypeConstraint) constraints.Add("class");
            else if (parameter.HasNotNullConstraint) constraints.Add("notnull");
            constraints.AddRange(parameter.ConstraintTypes.Select(static t => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            if (parameter.HasConstructorConstraint) constraints.Add("new()");
            if (constraints.Count > 0)
            {
                builder.Append("        where @").Append(parameter.Name).Append(" : ").AppendLine(string.Join(", ", constraints));
            }
        }

        private static string Literal(string value) => Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(value, quote: true);
    }
}
