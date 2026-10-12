using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Converters;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Xunit;
using Labels = AvalonDock.Properties.Resources;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LocalizationTests
{
    public static IEnumerable<object[]> Cultures()
    {
        yield return new object[] { "cs-CZ", "Zavřít", "Maximalizovat", "Obnovit" };
        yield return new object[] { "de", "Schließen", "Maximieren", "Wiederherstellen" };
        yield return new object[] { "es", "Cerrar", "Maximizar", "Restaurar" };
        yield return new object[] { "fr", "Fermer", "Agrandir", "Rétablir" };
        yield return new object[] { "hu", "Bezárás", "Teljes méret", "Visszaállítás" };
        yield return new object[] { "it", "Chiudi", "Ingrandisci", "Ripristina" };
        yield return new object[] { "ja-JP", "閉じる", "最大化", "元に戻す" };
        yield return new object[] { "nl-BE", "Sluiten", "Maximaliseren", "Herstellen" };
        yield return new object[] { "pt-BR", "Fechar", "Maximizar", "Restaurar" };
        yield return new object[] { "ro", "Închidere", "Maximizare", "Restaurare" };
        yield return new object[] { "ru", "Закрыть", "Развернуть", "Восстановить" };
        yield return new object[] { "sv", "Stäng", "Maximera", "Återställ" };
        yield return new object[] { "zh-Hans", "关闭", "最大化", "还原" };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cultures))]
    public void Every_Satellite_Has_All_Public_Keys_And_Compiled_Xaml_Uses_The_Selected_Culture(
        string name, string close, string maximize, string restore)
    {
        CultureInfo? previous = Labels.Culture;
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(name);
            Labels.Culture = culture;
            ResourceSet set = Assert.IsAssignableFrom<ResourceSet>(Labels.ResourceManager.GetResourceSet(culture, true, false));
            int count = 0;
            foreach (DictionaryEntry entry in set)
            {
                string key = Assert.IsType<string>(entry.Key);
                string value = Assert.IsType<string>(entry.Value);
                Assert.False(string.IsNullOrWhiteSpace(value));
                Assert.NotEqual(Labels.ResourceManager.GetString(key, CultureInfo.InvariantCulture), value);
                Assert.Equal(value, Labels.ResourceManager.GetString(key, culture));
                count++;
            }
            Assert.Equal(21, count);
            Assert.Equal(close, Labels.Document_Close);
            Assert.Equal(maximize, Labels.Window_Maximize);
            Assert.Equal(restore, Labels.Window_Restore);
            LocalizedResourcesFixture fixture = new();
            Assert.Equal(close, fixture.FindControl<TextBlock>("CloseLabel")!.Text);
            foreach (string key in new[] { "DefaultConverter", "LegacyConverter" })
            {
                AnchorableContextMenuAutoHideHeaderConverter converter = Assert.IsType<AnchorableContextMenuAutoHideHeaderConverter>(fixture.Resources[key]);
                Assert.Equal(Labels.Anchorable_AutoHide, converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture));
                Assert.Equal(restore, converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture));
            }
            Assert.Same(uiCulture, CultureInfo.CurrentUICulture);
        }
        finally { Labels.Culture = previous; }
    }

    [AvaloniaTheory]
    [InlineData("fr-CA", "Fermer")]
    [InlineData("de-AT", "Schließen")]
    [InlineData("es-MX", "Cerrar")]
    [InlineData("it-CH", "Chiudi")]
    [InlineData("sv-FI", "Stäng")]
    [InlineData("ru-UA", "Закрыть")]
    [InlineData("ro-MD", "Închidere")]
    [InlineData("zh-CN", "关闭")]
    [InlineData("pt-PT", "Close")]
    [InlineData("ja", "Close")]
    [InlineData("nl-NL", "Close")]
    [InlineData("pl-PL", "Close")]
    public void Parent_And_English_Fallback_Use_The_Resource_Manager_Culture_Chain(string name, string expected)
    {
        CultureInfo? previous = Labels.Culture;
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        try
        {
            Labels.Culture = null;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
            Assert.Equal(expected, Labels.Document_Close);
            Assert.Equal(expected, Labels.ResourceManager.GetString(nameof(Labels.Document_Close)));
            Assert.Null(Labels.ResourceManager.GetString("UnknownKey"));
        }
        finally { Labels.Culture = previous; CultureInfo.CurrentUICulture = uiCulture; }
    }

    [AvaloniaFact]
    public void Explicit_Culture_Changes_Only_Typed_Lookups_And_New_Static_Xaml_Values()
    {
        CultureInfo? previous = Labels.Culture;
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-AT");
            Labels.Culture = CultureInfo.GetCultureInfo("fr-CA");
            LocalizedResourcesFixture french = new();
            Assert.Equal("Fermer", Labels.Document_Close);
            Assert.Equal("Schließen", Labels.ResourceManager.GetString(nameof(Labels.Document_Close)));
            Labels.Culture = CultureInfo.GetCultureInfo("es-MX");
            Assert.Equal("Cerrar", Labels.Document_Close);
            Assert.Equal("Fermer", french.FindControl<TextBlock>("CloseLabel")!.Text);
            Assert.Equal("Cerrar", new LocalizedResourcesFixture().FindControl<TextBlock>("CloseLabel")!.Text);
            Labels.Culture = null;
            Assert.Equal("Schließen", Labels.Document_Close);
        }
        finally { Labels.Culture = previous; CultureInfo.CurrentUICulture = uiCulture; }
    }

    [AvaloniaFact]
    public void Released_Satellites_Reload_And_Preserve_The_Culture_Override()
    {
        CultureInfo? previous = Labels.Culture;
        try
        {
            Labels.Culture = CultureInfo.GetCultureInfo("zh-Hans");
            Assert.Equal("关闭", Labels.Document_Close);
            Labels.ResourceManager.ReleaseAllResources();
            Assert.Equal("关闭", Labels.Document_Close);
            Assert.Equal("Fermer", Labels.ResourceManager.GetString(nameof(Labels.Document_Close), CultureInfo.GetCultureInfo("fr")));
            Assert.Equal("Close", Labels.ResourceManager.GetString(nameof(Labels.Document_Close), CultureInfo.InvariantCulture));
        }
        finally { Labels.Culture = previous; }
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cultures))]
    public void Rendered_Menus_And_Floating_Buttons_Use_Localized_Labels_And_Keep_Dynamic_Overrides(
        string name, string close, string maximize, string restore)
    {
        CultureInfo? previous = Labels.Culture;
        Labels.Culture = CultureInfo.GetCultureInfo(name);
        LayoutDocument document = new() { Title = "Document", Content = "Document" };
        LayoutAnchorable tool = new() { Title = "Tool", Content = "Tool" };
        DockingManager manager = new() { Layout = new() { RootPanel = new()
            { Children = { new LayoutDocumentPane(document), new LayoutAnchorablePane(tool) } } } };
        Window window = new() { Width = 900, Height = 600 };
        window.Styles.Add(new AvalonDockTheme());
        window.Content = manager;
        try
        {
            window.Show(); Update();
            ContextMenu documents = manager.DocumentContextMenu!;
            Control tab = manager.GetVisualDescendants().OfType<LayoutDocumentTabItem>()
                .Single(control => ReferenceEquals(control.Model, document));
            Click(tab, MouseButton.Right);
            Assert.True(documents.IsOpen);
            MenuItem[] documentItems = documents.Items.OfType<MenuItem>().ToArray();
            Assert.Equal(new[] { close, Labels.Document_CloseAllButThis, Labels.Document_CloseAll, Labels.Document_Float,
                    Labels.Document_DockAsDocument, Labels.Document_NewHorizontalTabGroup, Labels.Document_NewVerticalTabGroup,
                    Labels.Document_MoveToNextTabGroup, Labels.Document_MoveToPreviousTabGroup },
                documentItems.Select(item => item.Header));
            manager.Resources["ToolChromeControlCloseString"] = "Application close";
            Update();
            Assert.Equal("Application close", documentItems[0].FindResource("ToolChromeControlCloseString"));
            Assert.Equal("Application close", documentItems[0].Header);
            manager.Resources.Remove("ToolChromeControlCloseString");
            Update(); Assert.Equal(close, documentItems[0].Header);
            documents.Close();
            int closedNotifications = 0;
            documents.ResourcesChanged += CountClosedNotification;
            manager.Resources["AvalonDockHideString"] = "Application hide";
            Update();
            Assert.Equal(0, closedNotifications);
            documents.ResourcesChanged -= CountClosedNotification;

            ContextMenu tools = manager.AnchorableContextMenu!;
            Click(manager.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_MenuButton"), MouseButton.Left);
            Assert.True(tools.IsOpen);
            Assert.Equal(new[] { Labels.Anchorable_Float, Labels.Anchorable_Dock, Labels.Anchorable_DockAsDocument,
                Labels.Anchorable_AutoHide, close, "Application hide" }, tools.Items.OfType<MenuItem>().Select(item => item.Header));
            manager.Resources.Remove("AvalonDockHideString");
            Update();
            Assert.Equal(Labels.Anchorable_Hide, ((MenuItem)tools.Items[5]!).Header);
            window.Resources["ToolChromeControlCloseString"] = "Window close";
            Update();
            Assert.Equal("Window close", ((MenuItem)tools.Items[4]!).Header);
            window.Resources.Remove("ToolChromeControlCloseString");
            Update();
            Assert.Equal(close, ((MenuItem)tools.Items[4]!).Header);
            tools.Close();

            document.Float(); tool.Float(); Update();
            foreach (LayoutFloatingWindowControl host in manager.FloatingWindows.ToArray())
            {
                bool isDocument = host.Model is LayoutDocumentFloatingWindow;
                Button maximizeButton = host.GetVisualDescendants().OfType<Button>().Single(button => isDocument
                    ? button.Classes.Contains("avalondock-document-maximize") : button.Name == "PART_MaximizeRestoreButton");
                Button closeButton = host.GetVisualDescendants().OfType<Button>().Single(button => isDocument
                    ? button.Classes.Contains("avalondock-document-window-close") : button.Name == "PART_CloseButton");
                Assert.Equal(close, ToolTip.GetTip(closeButton));
                Assert.Equal(maximize, ToolTip.GetTip(maximizeButton));
                Assert.Equal(maximize, AutomationProperties.GetName(maximizeButton));
                host.WindowState = WindowState.Maximized; Update();
                Assert.Equal(restore, ToolTip.GetTip(maximizeButton));
                Assert.Equal(restore, AutomationProperties.GetName(maximizeButton));
                host.WindowState = WindowState.Normal; Update();
                Assert.Equal(maximize, ToolTip.GetTip(maximizeButton));
            }

            void CountClosedNotification(object? sender, ResourcesChangedEventArgs args) => closedNotifications++;
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear(); window.Close(); Dispatcher.UIThread.RunJobs(); Labels.Culture = previous;
        }
        void Click(Control control, MouseButton button)
        {
            Point point = window.PointToClient(control.PointToScreen(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2)));
            window.MouseMove(point); window.MouseDown(point, button); window.MouseUp(point, button); Update();
        }
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); foreach (LayoutFloatingWindowControl host in manager.FloatingWindows) host.UpdateLayout(); }
    }
}
