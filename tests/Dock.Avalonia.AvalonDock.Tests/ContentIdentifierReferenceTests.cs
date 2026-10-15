using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock.ReferenceProbes;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContentIdentifierReferenceTests(ITestOutputHelper output)
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (bool tools in new[] { false, true })
        foreach (string operation in ContentIdentifierProbe.Operations())
            yield return new object[] { tools, operation };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Content_Identifiers_Notifications_And_Bindings_Match_WPF(bool tools, string operation)
    {
        string id = (tools ? "tool-" : "document-") + operation + "\t";
        string[] actual = ContentIdentifierProbe.Observe(tools, operation);
        foreach (string row in actual) output.WriteLine(row);
        string[] expected = File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Assets", "WpfContentIdentifiers.tsv"))
            .Where(row => row.StartsWith(id, StringComparison.Ordinal)).ToArray();
        Assert.True(expected.Length >= 2, "The WPF reference must cover every scenario.");
        Assert.Equal(expected, actual);
    }
}
