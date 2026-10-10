using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Source-only inventory: no WPF binaries, runtime reflection, or upstream
// implementation code is needed. This measures declared API presence, not
// behavior, inheritance substitutability, or framework-signature compatibility.
if (args.Length != 3 || args[0] is not ("snapshot" or "compare"))
{
    Console.Error.WriteLine("Usage: AvalonDockApiAudit snapshot <source-directory> <output-file> | compare <baseline-file> <implementation-directory>");
    return 2;
}

if (args[0] == "snapshot")
{
    SortedSet<string> declarations = ReadInventory(args[1]);
    File.WriteAllLines(args[2], declarations);
    Console.WriteLine($"Recorded {declarations.Count} API declarations in {args[2]}.");
    return 0;
}

SortedSet<string> expected = new(File.ReadAllLines(args[1]), StringComparer.Ordinal);
SortedSet<string> actual = ReadInventory(args[2]);
HashSet<string> actualKeys = new(actual.Select(Key), StringComparer.Ordinal);
List<string> missing = expected.Where(line => !actualKeys.Contains(Key(line))).ToList();
foreach (string line in missing) Console.WriteLine("MISSING\t" + line);
Console.WriteLine($"Presence audit: {expected.Count - missing.Count}/{expected.Count} declarations have a matching type/member key; {missing.Count} missing.");
Console.WriteLine("A matching key is not proof of signature or behavioral compatibility. Verify framework types, inheritance, defaults, bindings, templates, and behavior separately.");
return missing.Count == 0 ? 0 : 1;

static string Key(string declaration)
{
    int separator = declaration.IndexOf('\t');
    return separator < 0 ? declaration : declaration[..separator];
}

static SortedSet<string> ReadInventory(string directory)
{
    SortedSet<string> result = new(StringComparer.Ordinal);
    foreach (string file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
    {
        string relative = Path.GetRelativePath(directory, file).Replace('\\', '/');
        if (relative.StartsWith("obj/", StringComparison.Ordinal) || relative.StartsWith("bin/", StringComparison.Ordinal)) continue;
        SyntaxNode root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
        foreach (BaseTypeDeclarationSyntax type in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            if (!IsExported(type)) continue;
            string ns = string.Join(".", type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(x => x.Name.ToString()));
            if (!ns.StartsWith("Xceed.Wpf.AvalonDock", StringComparison.Ordinal)) continue;
            string name = ns + "." + string.Join(".", type.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(TypeName));
            string kind = type is TypeDeclarationSyntax typed ? typed.Keyword.ValueText : "enum";
            result.Add(name + "|type\t" + type.Modifiers + " " + kind + " " + TypeName(type) + " " + type.BaseList?.WithoutTrivia().NormalizeWhitespace());
            if (type is EnumDeclarationSyntax enumeration)
            {
                foreach (EnumMemberDeclarationSyntax member in enumeration.Members)
                    result.Add(name + "|enum:" + member.Identifier.ValueText + "\t" + member.Identifier.ValueText + member.EqualsValue?.WithoutTrivia().NormalizeWhitespace());
                continue;
            }
            if (type is not TypeDeclarationSyntax declaration) continue;
            foreach (MemberDeclarationSyntax member in declaration.Members)
            {
                if (member is BaseTypeDeclarationSyntax) continue;
                if (type is not InterfaceDeclarationSyntax && !member.Modifiers.Any(SyntaxKind.PublicKeyword) && !member.Modifiers.Any(SyntaxKind.ProtectedKeyword)) continue;
                switch (member)
                {
                    case MethodDeclarationSyntax method:
                        string methodName = method.Identifier.ValueText + method.TypeParameterList?.ToString();
                        result.Add(name + "|method:" + methodName + "(" + ParameterTypes(method.ParameterList) + ")\t" + method.ReturnType + " " + methodName + method.ParameterList.WithoutTrivia().NormalizeWhitespace());
                        break;
                    case ConstructorDeclarationSyntax constructor:
                        result.Add(name + "|constructor:(" + ParameterTypes(constructor.ParameterList) + ")\t" + constructor.Identifier + constructor.ParameterList.WithoutTrivia().NormalizeWhitespace());
                        break;
                    case PropertyDeclarationSyntax property:
                        result.Add(name + "|property:" + property.Identifier.ValueText + "\t" + property.Type + " " + property.Identifier.ValueText + " " + Accessors(property.AccessorList));
                        break;
                    case FieldDeclarationSyntax field:
                        foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                            result.Add(name + "|field:" + variable.Identifier.ValueText + "\t" + field.Declaration.Type + " " + variable.Identifier.ValueText);
                        break;
                    case EventFieldDeclarationSyntax events:
                        foreach (VariableDeclaratorSyntax variable in events.Declaration.Variables)
                            result.Add(name + "|event:" + variable.Identifier.ValueText + "\t" + events.Declaration.Type + " " + variable.Identifier.ValueText);
                        break;
                    case EventDeclarationSyntax eventDeclaration:
                        result.Add(name + "|event:" + eventDeclaration.Identifier.ValueText + "\t" + eventDeclaration.Type + " " + eventDeclaration.Identifier.ValueText);
                        break;
                    case IndexerDeclarationSyntax indexer:
                        result.Add(name + "|indexer:" + indexer.ParameterList.WithoutTrivia().NormalizeWhitespace() + "\t" + indexer.Type);
                        break;
                    case DelegateDeclarationSyntax callback:
                        result.Add(name + "|delegate:" + callback.Identifier.ValueText + "\t" + callback.WithoutTrivia().NormalizeWhitespace());
                        break;
                }
            }
        }
    }
    return result;
}

static bool IsExported(BaseTypeDeclarationSyntax type) => type.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>()
    .All(x => x.Modifiers.Any(SyntaxKind.PublicKeyword) || x.Modifiers.Any(SyntaxKind.ProtectedKeyword));

static string TypeName(BaseTypeDeclarationSyntax type) => type.Identifier.ValueText +
    (type is TypeDeclarationSyntax declaration ? declaration.TypeParameterList?.ToString() : null);

static string ParameterTypes(ParameterListSyntax list) => string.Join(",", list.Parameters.Select(x =>
    (x.Modifiers.Count > 0 ? x.Modifiers + " " : string.Empty) + x.Type?.WithoutTrivia().NormalizeWhitespace().ToString().Replace("?", string.Empty)));

static string Accessors(AccessorListSyntax? accessors) => accessors is null ? "get;" :
    string.Join(" ", accessors.Accessors.Select(x => x.Modifiers + " " + x.Keyword + ";"));
