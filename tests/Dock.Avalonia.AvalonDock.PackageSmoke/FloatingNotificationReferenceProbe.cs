using System;
using System.IO;
using System.Linq;
using AvalonDock.ReferenceProbes;

namespace PackageConsumer;

internal static class FloatingNotificationReferenceProbe
{
    internal static void Verify()
    {
        string[] expected = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Assets", "WpfFloatingNotifications.tsv"));
        int count = 0;
        foreach (bool tools in new[] { false, true })
        foreach (string suppression in new[] { "none", "changing", "changed" })
        foreach (string operation in FloatingNotificationProbe.Operations(tools))
        {
            string id = $"{(tools ? "tool" : "document")}-{suppression}-{operation}\t";
            string[] observations = expected.Where(row => row.StartsWith(id, StringComparison.Ordinal)).ToArray();
            if (observations.Length != 2 || !observations.SequenceEqual(FloatingNotificationProbe.Observe(tools, suppression, operation)))
                throw new Exception($"Packaged floating notifications differ from WPF: {id.Trim()}.");
            count++;
        }
        Console.WriteLine($"Packaged floating notifications match {count} WPF scenarios.");
    }
}
