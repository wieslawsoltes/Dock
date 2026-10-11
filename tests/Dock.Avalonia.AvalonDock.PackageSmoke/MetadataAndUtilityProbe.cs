using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Converters;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Labels = AvalonDock.Properties.Resources;

namespace PackageConsumer;

internal static class MetadataAndUtilityProbe
{
    internal static void Verify()
    {
        ConsumerWindow resources = new();
        var converter = (UriSourceToBitmapImageConverter)resources.Resources["UriImageConverter"]!;
        Image image = (Image)converter.Convert(new Uri("avares://Dock.Avalonia.AvalonDock.PackageSmoke/Assets/converter.png"),
            typeof(Image), null, CultureInfo.InvariantCulture);
        using Bitmap bitmap = (Bitmap)image.Source!;
        if (bitmap.PixelSize.Width != 1 || bitmap.PixelSize.Height != 1)
            throw new Exception("The packaged URI converter did not load the consumer's compiled asset.");
        CultureInfo? previous = Labels.Culture;
        try
        {
            Labels.Culture = CultureInfo.GetCultureInfo("fr-CA");
            var header = (AnchorableContextMenuAutoHideHeaderConverter)resources.Resources["AutoHideHeaderConverter"]!;
            if (!Equals(header.Convert(false, typeof(string), null, CultureInfo.InvariantCulture), Labels.Anchorable_AutoHide) ||
                !Equals(header.Convert(true, typeof(string), null, CultureInfo.InvariantCulture), Labels.Window_Restore) ||
                Labels.ResourceManager.GetString(nameof(Labels.Document_Close), Labels.Culture) != Labels.Document_Close)
                throw new Exception("The packaged converters did not resolve the public resource strings.");
        }
        finally { Labels.Culture = previous; }
        foreach (string key in new[] { "Resizer", "LegacyResizer" })
        {
            var splitter = (LayoutGridResizerControl)((IDataTemplate)resources.Resources[key]!).Build(null)!;
            if (splitter.PreviewOpacity != 0.7 || !Equals(splitter.PreviewBackground, Brushes.Blue))
                throw new Exception("The packaged resizer did not compile in both XML namespaces.");
        }

        LayoutDocument document = new() { Title = "Document", Content = "Editor" };
        LayoutAnchorable tool = new() { Title = "Tool", IconSource = bitmap, ToolTip = "Tool hint", IsEnabled = false };
        DockingManager manager = new()
        {
            Layout = new LayoutRoot
            {
                RootPanel = new LayoutPanel
                {
                    Children = { new LayoutAnchorablePane { Children = { tool, new LayoutAnchorable { Title = "Other" } } }, new LayoutDocumentPane(document) }
                }
            }
        };
        Window window = new() { Width = 700, Height = 500, Content = manager };
        try
        {
            window.Show(); Update();
            ToolTabStripItem tab = manager.GetVisualDescendants().OfType<ToolTabStripItem>()
                .Single(item => ReferenceEquals(item.DataContext, tool.DockModel));
            if (tab.IsEnabled || !Equals(ToolTip.GetTip(tab), "Tool hint") ||
                !tab.GetVisualDescendants().OfType<Image>().Any(icon => ReferenceEquals(icon.Source, bitmap)))
                throw new Exception("The packaged native tool tab did not show its public metadata.");
            tool.IsEnabled = true; tool.ToolTip = "Updated hint";
            Update();
            if (!tab.IsEnabled || !Equals(ToolTip.GetTip(tab), "Updated hint"))
                throw new Exception("The packaged native tool tab did not update its public metadata.");
            if (!manager.GetVisualDescendants().OfType<LayoutGridResizerControl>().Any())
                throw new Exception("The packaged layout did not use the public native-backed resizer.");
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            var children = manager.LogicalChildrenPublic;
            bool found = false;
            while (children.MoveNext()) found |= ReferenceEquals(children.Current, item);
            if (!found) throw new Exception("The public logical enumerator did not expose the current layout item.");
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }
}
