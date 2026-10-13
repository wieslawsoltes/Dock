using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Converters;
using AvalonDock.Layout;
using Dock.Controls.ProportionalStackPanel;
using Xunit;
using Labels = AvalonDock.Properties.Resources;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.AvalonDock.Tests;

public class CompatibilitySurfaceBatchTests
{
    [AvaloniaFact]
    public void Public_Resource_Properties_Resolve_From_The_Packaged_Resource_Manager()
    {
        CultureInfo? previous = Labels.Culture;
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        try
        {
            Labels.Culture = CultureInfo.GetCultureInfo("fr-CA");
            (string Key, string Text)[] values =
            [
                (nameof(Labels.Anchorable_AutoHide), Labels.Anchorable_AutoHide),
                (nameof(Labels.Anchorable_BtnAutoHide_Hint), Labels.Anchorable_BtnAutoHide_Hint),
                (nameof(Labels.Anchorable_BtnClose_Hint), Labels.Anchorable_BtnClose_Hint),
                (nameof(Labels.Anchorable_CxMenu_Hint), Labels.Anchorable_CxMenu_Hint),
                (nameof(Labels.Anchorable_Dock), Labels.Anchorable_Dock),
                (nameof(Labels.Anchorable_DockAsDocument), Labels.Anchorable_DockAsDocument),
                (nameof(Labels.Anchorable_Float), Labels.Anchorable_Float),
                (nameof(Labels.Anchorable_Hide), Labels.Anchorable_Hide),
                (nameof(Labels.Document_BtnPinned_Hint), Labels.Document_BtnPinned_Hint),
                (nameof(Labels.Document_Close), Labels.Document_Close),
                (nameof(Labels.Document_CloseAll), Labels.Document_CloseAll),
                (nameof(Labels.Document_CloseAllButThis), Labels.Document_CloseAllButThis),
                (nameof(Labels.Document_CxMenu_Hint), Labels.Document_CxMenu_Hint),
                (nameof(Labels.Document_DockAsDocument), Labels.Document_DockAsDocument),
                (nameof(Labels.Document_Float), Labels.Document_Float),
                (nameof(Labels.Document_MoveToNextTabGroup), Labels.Document_MoveToNextTabGroup),
                (nameof(Labels.Document_MoveToPreviousTabGroup), Labels.Document_MoveToPreviousTabGroup),
                (nameof(Labels.Document_NewHorizontalTabGroup), Labels.Document_NewHorizontalTabGroup),
                (nameof(Labels.Document_NewVerticalTabGroup), Labels.Document_NewVerticalTabGroup),
                (nameof(Labels.Window_Maximize), Labels.Window_Maximize),
                (nameof(Labels.Window_Restore), Labels.Window_Restore)
            ];
            foreach ((string key, string text) in values)
            {
                Assert.False(string.IsNullOrWhiteSpace(text));
                Assert.NotEqual(key, text);
                Assert.Equal(text, Labels.ResourceManager.GetString(key, Labels.Culture));
            }
            Assert.Same(uiCulture, CultureInfo.CurrentUICulture);
            Assert.Null(Labels.ResourceManager.GetString("UnknownKey", Labels.Culture));
            Labels.Culture = null;
            Assert.Equal(Labels.ResourceManager.GetString(nameof(Labels.Document_Close), uiCulture), Labels.Document_Close);
        }
        finally { Labels.Culture = previous; }
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    [InlineData("not a boolean")]
    public void AutoHide_Header_Converter_Uses_Public_Resources_And_Has_No_Reverse_Conversion(object? value)
    {
        SizingFixture resources = new();
        AnchorableContextMenuAutoHideHeaderConverter converter = Assert.IsType<AnchorableContextMenuAutoHideHeaderConverter>(resources.Resources["AutoHideHeaderConverter"]);
        Assert.Equal(value is true ? Labels.Window_Restore : Labels.Anchorable_AutoHide,
            converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture));
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(value, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Uri_Image_Converter_Loads_Absolute_And_Relative_Compiled_Assets(bool relative)
    {
        SizingFixture resources = new();
        UriSourceToBitmapImageConverter converter = Assert.IsType<UriSourceToBitmapImageConverter>(resources.Resources["UriImageConverter"]);
        Uri baseUri = new("avares://Dock.Avalonia.AvalonDock.Tests/Assets/");
        Uri uri = relative ? new Uri("converter.png", UriKind.Relative) : new Uri(baseUri, "converter.png");
        Image image = Assert.IsType<Image>(converter.Convert(uri, typeof(object), relative ? baseUri : null, CultureInfo.InvariantCulture));
        using Bitmap bitmap = Assert.IsType<Bitmap>(image.Source);
        Assert.Equal(new PixelSize(1, 1), bitmap.PixelSize);
        Assert.Same(BindingOperations.DoNothing, converter.Convert(null, typeof(Image), null, CultureInfo.InvariantCulture));
        Assert.Throws<InvalidCastException>(() => converter.Convert("not a URI", typeof(Image), null, CultureInfo.InvariantCulture));
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(image, typeof(Uri), null, CultureInfo.InvariantCulture));
        Assert.Throws<NotSupportedException>(() => converter.Convert(new Uri("ftp://example.invalid/image.png"), typeof(Image), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaFact]
    public void Uri_Image_Converter_Loads_File_Uris_And_Closes_The_File_Stream()
    {
        string path = Path.Combine(Path.GetTempPath(), "dock-avalondock-image-" + Guid.NewGuid().ToString("N") + ".png");
        try
        {
            using (Stream source = global::Avalonia.Platform.AssetLoader.Open(new Uri("avares://Dock.Avalonia.AvalonDock.Tests/Assets/converter.png")))
            using (FileStream target = File.Create(path)) source.CopyTo(target);
            Image image = Assert.IsType<Image>(new UriSourceToBitmapImageConverter().Convert(new Uri(path), typeof(Image), null, CultureInfo.InvariantCulture));
            using Bitmap bitmap = Assert.IsType<Bitmap>(image.Source);
            Assert.Equal(new PixelSize(1, 1), bitmap.PixelSize);
            using FileStream exclusive = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally { File.Delete(path); }
    }

    [AvaloniaFact]
    public void Public_Logical_Children_Enumerator_Exposes_Lazy_Item_Views()
    {
        LayoutDocument document = new() { Title = "First" };
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) } };
        LayoutItem old = manager.GetLayoutItemFromModel(document)!;
        Assert.DoesNotContain(old, Children(manager.LogicalChildrenPublic));
        Control view = old.View;
        Assert.Contains(view, Children(manager.LogicalChildrenPublic));
        manager.Layout = new();
        Assert.DoesNotContain(view, Children(manager.LogicalChildrenPublic));

        static List<object?> Children(IEnumerator enumerator)
        {
            List<object?> result = new();
            while (enumerator.MoveNext()) result.Add(enumerator.Current);
            return result;
        }
    }

    [AvaloniaFact]
    public void Window_State_Overrides_Observe_Updated_Maximization_Through_The_Base_Hook()
    {
        StateWindow window = new(new LayoutRoot());
        try
        {
            window.Show();
            window.States.Clear();
            window.WindowState = WindowState.Maximized;
            Assert.Equal((WindowState.Maximized, true), window.States[^1]);
            window.WindowState = WindowState.Minimized;
            Assert.Equal((WindowState.Minimized, false), window.States[^1]);
            window.WindowState = WindowState.Normal;
            Assert.Equal((WindowState.Normal, false), window.States[^1]);
            int calls = window.States.Count;
            window.WindowState = WindowState.Normal;
            Assert.Equal(calls, window.States.Count);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData("Resizer")]
    [InlineData("LegacyResizer")]
    public void Public_Resizers_Compile_In_Both_Namespaces_And_Configure_The_Native_Preview(string key)
    {
        SizingFixture resources = new();
        LayoutGridResizerControl defaults = new();
        Assert.Same(Brushes.Black, defaults.BackgroundWhileDragging);
        Assert.Equal(0.5, defaults.OpacityWhileDragging);
        LayoutGridResizerControl splitter = Assert.IsType<LayoutGridResizerControl>(((IDataTemplate)resources.Resources[key]!).Build(null));
        Assert.Equal(Brushes.Blue, splitter.PreviewBackground);
        Assert.Equal(0.7, splitter.PreviewOpacity);
        ProportionalStackPanelSplitter native = new();
        Assert.Null(native.PreviewBackground);
        Assert.Equal(1, native.PreviewOpacity);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Public_Resizers_Use_Native_Input_And_Update_An_Open_Preview(bool vertical)
    {
        LayoutDocumentPane first = new(new LayoutDocument { Content = "First" });
        LayoutDocumentPane second = new(new LayoutDocument { Content = "Second" });
        if (vertical) first.DockHeight = new GridLength(200); else first.DockWidth = new GridLength(200);
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel { Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal, Children = { first, second } } } };
        Window window = new() { Width = 700, Height = 600, Content = manager };
        try
        {
            window.Show(); Update();
            LayoutGridResizerControl splitter = Assert.Single(manager.GetVisualDescendants().OfType<LayoutGridResizerControl>(), control => control.IsEffectivelyVisible);
            splitter.PreviewResize = true;
            Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), window)!.Value;
            Point end = start + (vertical ? new Vector(0, 24) : new Vector(24, 0));
            window.MouseDown(start, MouseButton.Left);
            window.MouseMove(end, RawInputModifiers.LeftMouseButton);
            Update();
            Control preview = Assert.Single(AdornerLayer.GetAdornerLayer(splitter)!.Children,
                control => AdornerLayer.GetAdornedElement(control) is IProportionalSplitterHost);
            Assert.Equal(0.5, preview.Opacity);
            Assert.Equal(new GridLength(200), vertical ? first.DockHeight : first.DockWidth);
            splitter.OpacityWhileDragging = 0.25;
            splitter.BackgroundWhileDragging = Brushes.Blue;
            Assert.Equal(0.25, preview.Opacity);
            Assert.Same(Brushes.Blue, splitter.PreviewBackground);
            window.MouseUp(end, MouseButton.Left);
            Update();
            Assert.Equal(224, (vertical ? first.DockHeight : first.DockWidth).Value, 3);
            Assert.DoesNotContain(preview, window.GetVisualDescendants());
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }

    private sealed class StateWindow(ILayoutElement model) : LayoutFloatingWindowControl(model)
    {
        public override ILayoutElement Model { get; } = model;
        internal List<(WindowState, bool)> States { get; } = new();
        protected override void OnStateChanged(EventArgs e) { base.OnStateChanged(e); States.Add((WindowState, IsMaximized)); }
    }
}
