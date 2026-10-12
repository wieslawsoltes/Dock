using System;
using System.IO;
using System.Linq;
using AvalonDock.ReferenceProbes;

namespace PackageConsumer;

internal static class ContentStateReferenceProbe
{
    internal static void Verify()
    {
        string[] identifiers = Read("WpfContentIdentifiers.tsv");
        string[] hooks = Read("WpfContentActivationHooks.tsv");
        int identifierCases = 0, hookCases = 0;
        foreach (bool tools in new[] { false, true })
        {
            string kind = tools ? "tool-" : "document-";
            foreach (string operation in ContentIdentifierProbe.Operations())
            {
                Compare(identifiers, kind + operation, ContentIdentifierProbe.Observe(tools, operation));
                identifierCases++;
            }
            foreach (bool suppress in new[] { false, true })
            foreach (string operation in ContentActivationHookProbe.Operations())
            {
                Compare(hooks, kind + (suppress ? "override-" : "base-") + operation,
                    ContentActivationHookProbe.Observe(tools, suppress, operation));
                hookCases++;
            }
        }
        Console.WriteLine($"Packaged content matches {identifierCases} WPF identifier scenarios and {hookCases} activation-hook scenarios.");
    }

    private static string[] Read(string file) => File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Assets", file));

    private static void Compare(string[] reference, string id, string[] actual)
    {
        string[] expected = reference.Where(row => row.StartsWith(id + "\t", StringComparison.Ordinal)).ToArray();
        if (expected.Length < 2 || !expected.SequenceEqual(actual))
            throw new Exception($"Packaged content state differs from WPF: {id}.");
    }
}
