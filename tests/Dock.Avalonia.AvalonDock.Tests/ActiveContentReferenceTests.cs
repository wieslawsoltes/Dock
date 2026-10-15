using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock.ReferenceProbes;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ActiveContentReferenceTests(ITestOutputHelper output)
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (bool suppress in new[] { false, true })
        foreach (string operation in ActiveContentProbe.Operations())
            yield return new object[] { suppress, operation };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Manager_Values_Bindings_And_Callbacks_Match_WPF(bool suppress, string operation)
    {
        string[] expected = Read("WpfActiveContent.tsv", suppress, operation);
        Assert.True(expected.Length >= 2, "The WPF reference must cover every scenario.");
        string[] actual = ActiveContentProbe.Observe(suppress, operation);
        foreach (string row in actual) output.WriteLine(row);
        Assert.Equal(expected, actual);
    }

    private static string[] Read(string file, bool suppress, string operation)
    {
        string id = (suppress ? "override-" : "base-") + operation + "\t";
        return File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Assets", file))
            .Where(row => row.StartsWith(id, StringComparison.Ordinal)).ToArray();
    }
}
