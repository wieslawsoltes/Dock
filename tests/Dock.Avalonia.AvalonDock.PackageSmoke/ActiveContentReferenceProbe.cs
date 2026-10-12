using System;
using System.IO;
using System.Linq;
using AvalonDock.ReferenceProbes;

namespace PackageConsumer;

internal static class ActiveContentReferenceProbe
{
    internal static void Verify()
    {
        string[] wpf = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Assets", "WpfActiveContent.tsv"));
        string[] avalonia = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Assets", "AvaloniaActiveContent.tsv"));
        int matches = 0, differences = 0;
        foreach (bool suppress in new[] { false, true })
        foreach (string operation in ActiveContentProbe.Operations())
        {
            bool frameworkDifference = operation is "binding-default" or "binding-oneway" or "binding-clear" or "style-remove";
            string id = (suppress ? "override-" : "base-") + operation + "\t";
            string[] expected = (frameworkDifference ? avalonia : wpf)
                .Where(row => row.StartsWith(id, StringComparison.Ordinal)).ToArray();
            if (expected.Length < 2 || !expected.SequenceEqual(ActiveContentProbe.Observe(suppress, operation)))
                throw new Exception($"Packaged active content differs from its documented contract: {id.Trim()}.");
            if (frameworkDifference) differences++;
            else matches++;
        }
        Console.WriteLine($"Packaged active content matches {matches} WPF scenarios; {differences} cases retain documented Avalonia binding/style semantics.");
    }
}
