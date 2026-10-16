using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock.ReferenceProbes;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingNotificationReferenceTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (bool tools in new[] { false, true })
        foreach (string suppression in new[] { "none", "changing", "changed" })
        foreach (string operation in FloatingNotificationProbe.Operations(tools))
            yield return new object[] { tools, suppression, operation };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Floating_Model_Notifications_And_Callback_State_Match_WPF(bool tools, string suppression, string operation)
    {
        string id = $"{(tools ? "tool" : "document")}-{suppression}-{operation}\t";
        string[] expected = File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Assets", "WpfFloatingNotifications.tsv"))
            .Where(row => row.StartsWith(id, StringComparison.Ordinal)).ToArray();
        Assert.Equal(2, expected.Length);
        Assert.Equal(expected, FloatingNotificationProbe.Observe(tools, suppression, operation));
    }
}
