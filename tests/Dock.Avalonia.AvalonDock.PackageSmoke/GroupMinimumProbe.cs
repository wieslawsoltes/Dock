using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class GroupMinimumProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        foreach (bool width in new[] { false, true })
        {
            string property = width ? nameof(LayoutPanel.DockMinWidth) : nameof(LayoutPanel.DockMinHeight);
            Check("public notification name", () =>
            {
                LayoutPanel panel = new();
                List<string> events = Observe(panel);
                Write(panel, 40);
                if (string.Join(" ", events) != "before:" + property + " after:" + property)
                    throw new Exception("The public assignment leaked a native property name.");
            });
            Check("native notification name", () =>
            {
                LayoutPanel panel = new();
                List<string> events = Observe(panel);
                if (width) panel.DockModel.MinWidth = 40;
                else panel.DockModel.MinHeight = 40;
                if (string.Join(" ", events) != "after:" + property)
                    throw new Exception("Native changes did not project exactly one compatibility notification.");
            });
            Check("repeated NaN", () =>
            {
                LayoutPanel panel = new();
                int before = 0, after = 0;
                panel.PropertyChanging += (_, e) => { if (e.PropertyName == property) before++; };
                panel.PropertyChanged += (_, e) => { if (e.PropertyName == property) after++; };
                Write(panel, double.NaN);
                Write(panel, double.NaN);
                if (before != 2 || after != 2) throw new Exception("A repeated NaN lost its notifications.");
            });
            Check("restored negative equality", () =>
            {
                LayoutPanel panel = new();
                using XmlReader reader = XmlReader.Create(new StringReader("<Panel " + property + "='-12' />"));
                reader.MoveToContent();
                panel.ReadXml(reader);
                List<string> events = Observe(panel);
                Write(panel, -12);
                if (events.Count != 0) throw new Exception("An unchanged restored minimum raised a notification.");
            });
            Check("argument error", () =>
            {
                LayoutPanel panel = new();
                Exception? actual = null;
                try { Write(panel, -1); }
                catch (Exception error) { actual = error; }
                if (actual is not ArgumentException { ParamName: null } ||
                    actual.Message != "Invalid value, must be a positive number or equal to zero")
                    throw new Exception("The minimum validation error did not retain its argument contract.");
            });
            Check("reentrant same value", () =>
            {
                LayoutPanel panel = new();
                bool nested = false;
                int after = 0;
                panel.PropertyChanging += (_, e) =>
                {
                    if (e.PropertyName != property || nested) return;
                    nested = true;
                    Write(panel, 80);
                };
                panel.PropertyChanged += (_, e) => { if (e.PropertyName == property) after++; };
                Write(panel, 80);
                if (after != 2) throw new Exception("A callback setting the requested value suppressed the outer notification.");
            });

            void Write(LayoutPanel panel, double value)
            {
                if (width) panel.DockMinWidth = value;
                else panel.DockMinHeight = value;
            }

            void Check(string name, Action action)
            {
                try { action(); }
                catch (Exception error) { failures.Add(property + " " + name + ": " + error.Message); }
            }
        }
        if (failures.Count != 0) throw new Exception("Packaged minimum-metadata failures: " + string.Join("; ", failures));
    }

    private static List<string> Observe(LayoutPanel panel)
    {
        List<string> events = new();
        panel.PropertyChanging += (_, e) => events.Add("before:" + e.PropertyName);
        panel.PropertyChanged += (_, e) => events.Add("after:" + e.PropertyName);
        return events;
    }
}
