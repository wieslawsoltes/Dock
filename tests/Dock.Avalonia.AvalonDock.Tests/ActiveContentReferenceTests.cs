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
            if (!UsesAvaloniaBindingLifetime(operation)) yield return new object[] { suppress, operation };
    }

    public static IEnumerable<object[]> FrameworkCases()
    {
        foreach (bool suppress in new[] { false, true })
        foreach (string operation in ActiveContentProbe.Operations())
            if (UsesAvaloniaBindingLifetime(operation)) yield return new object[] { suppress, operation };
    }

    private static bool UsesAvaloniaBindingLifetime(string operation) =>
        operation is "binding-default" or "binding-oneway" or "binding-clear" or "style-remove";

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

    [AvaloniaTheory]
    [MemberData(nameof(FrameworkCases))]
    public void Binding_And_Style_Lifetimes_Follow_Documented_Avalonia_Semantics(bool suppress, string operation)
    {
        string[] wpf = Read("WpfActiveContent.tsv", suppress, operation);
        string[] expected = Read("AvaloniaActiveContent.tsv", suppress, operation);
        string[] actual = ActiveContentProbe.Observe(suppress, operation);
        foreach (string row in actual) output.WriteLine(row);
        Assert.True(expected.Length >= 4);
        Assert.Equal(wpf.Length, expected.Length);
        Assert.Equal(expected, actual);
        Assert.False(wpf.SequenceEqual(actual), "This documented framework difference needs review if WPF behavior changes.");
        int sharedStages = operation == "binding-clear" ? 2 : operation == "style-remove" ? 3 : 4;
        Assert.Equal(wpf.Take(sharedStages), actual.Take(sharedStages));
    }

    private static string[] Read(string file, bool suppress, string operation)
    {
        string id = (suppress ? "override-" : "base-") + operation + "\t";
        return File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Assets", file))
            .Where(row => row.StartsWith(id, StringComparison.Ordinal)).ToArray();
    }
}
