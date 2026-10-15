using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Layout;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class GroupXmlProbe
{
    internal static void Verify()
    {
        LayoutAnchorablePane pane = new();
        List<string> changes = new();
        pane.PropertyChanged += (_, e) => changes.Add(e.PropertyName!);
        Read(pane, "<Pane Name='Restored' DockMinWidth='130' DockMinHeight='140' FloatingWidth='500' FloatingHeight='400' IsMaximized='true'></Pane>");
        if (changes.Count != 0 || pane.Name != "Restored" || pane.DockMinWidth != 130 || pane.DockMinHeight != 140 ||
            pane.FloatingWidth != 500 || pane.FloatingHeight != 400 || !pane.IsMaximized)
            throw new Exception("Packaged group XML did not restore silent backing metadata.");

        pane.DockModel.Name = "NativeStyleName";
        pane.DockModel.ApplyStyling();
        pane.Name = "Changed";
        if (changes.Count != 1 || changes[0] != "Name" || pane.DockModel.Name != "NativeStyleName")
            throw new Exception("The packaged pane name did not retain independent, observable layout metadata.");

        LayoutPanel panel = new();
        Read(panel, "<Panel Orientation='vertical' DockMinWidth='-10' />");
        if (panel.Orientation != Orientation.Vertical || panel.DockMinWidth != -10 || panel.IsVisible)
            throw new Exception("The packaged XML did not restore orientation, stored minimum, or empty-group visibility.");
        bool rejected = false;
        try { Read(panel, "<Panel DockWidth='147' DockHeight='invalid' />"); }
        catch (FormatException) { rejected = true; }
        if (!rejected || panel.DockWidth != new GridLength(147))
            throw new Exception("A later XML attribute error discarded an earlier packaged group value.");
        panel.DockMinWidth = 100;
        if (panel.DockModel.MinWidth != 100)
            throw new Exception("Public sizing no longer updated native Dock after XML restoration.");
    }

    private static void Read<T>(LayoutGroup<T> group, string xml) where T : class, ILayoutElement
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        reader.MoveToContent();
        group.ReadXml(reader);
    }
}
