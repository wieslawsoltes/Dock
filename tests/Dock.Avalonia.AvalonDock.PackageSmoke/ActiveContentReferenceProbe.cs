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
        int matches = 0;
        foreach (bool suppress in new[] { false, true })
        foreach (string operation in ActiveContentProbe.Operations())
        {
            string id = (suppress ? "override-" : "base-") + operation + "\t";
            string[] expected = wpf
                .Where(row => row.StartsWith(id, StringComparison.Ordinal)).ToArray();
            if (expected.Length < 2 || !expected.SequenceEqual(ActiveContentProbe.Observe(suppress, operation)))
                throw new Exception($"Packaged active content differs from its documented contract: {id.Trim()}.");
            matches++;
        }
        Console.WriteLine($"Packaged active content matches all {matches} WPF scenarios, including binding and style lifetimes.");
    }
}
