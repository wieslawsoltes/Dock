using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock.ReferenceProbes;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContentActivationHookReferenceTests(ITestOutputHelper output)
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (bool tools in new[] { false, true })
        foreach (bool suppress in new[] { false, true })
        foreach (string operation in ContentActivationHookProbe.Operations())
            yield return new object[] { tools, suppress, operation };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Activation_Hooks_And_Notifications_Match_WPF(bool tools, bool suppress, string operation)
    {
        string id = (tools ? "tool-" : "document-") + (suppress ? "override-" : "base-") + operation + "\t";
        string[] actual = ContentActivationHookProbe.Observe(tools, suppress, operation);
        foreach (string row in actual) output.WriteLine(row);
        string[] expected = File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Assets", "WpfContentActivationHooks.tsv"))
            .Where(row => row.StartsWith(id, StringComparison.Ordinal)).ToArray();
        Assert.Equal(2, expected.Length);
        Assert.Equal(expected, actual);
    }
}
