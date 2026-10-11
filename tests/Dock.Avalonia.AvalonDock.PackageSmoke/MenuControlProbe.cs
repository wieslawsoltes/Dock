using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using DropDownButton = AvalonDock.Controls.DropDownButton;

namespace PackageConsumer;

internal static class MenuControlProbe
{
    internal static void Verify()
    {
        LayoutDocument document = new() { Title = "Packaged menu" };
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) } };
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        ConsumerWindow resources = new();
        foreach (string key in new[] { "MenuButton", "LegacyMenuButton", "MenuArea" })
        {
            Control control = ((IDataTemplate)resources.Resources[key]!).Build(item)!;
            control.DataContext = item;
            ContextMenu menu = control is DropDownButton button ? button.DropDownContextMenu! : ((DropDownControlArea)control).DropDownContextMenu!;
            Window window = new() { Content = control, Width = 400, Height = 300 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                for (int index = 0; index < 2; index++)
                {
                    document.Title = $"{key} {index}";
                    Click(control, control is DropDownButton ? MouseButton.Left : MouseButton.Right);
                    if (!menu.IsOpen || !ReferenceEquals(menu.DataContext, item) || !ReferenceEquals(menu.PlacementTarget, control))
                        throw new Exception("The packaged dropdown did not open with its item and placement target.");
                    if (control is DropDownButton openButton && openButton.IsChecked != true)
                        throw new Exception("The packaged button did not track its open menu.");
                    MenuItemEx entry = (MenuItemEx)menu.Items[0]!;
                    if (entry.Icon is not Border { Child: TextBlock icon } || icon.Text != document.Title ||
                        !ReferenceEquals(entry.Command, item.ActivateCommand))
                        throw new Exception("Compiled packaged icon/command bindings did not survive menu reopening.");
                    Click(entry);
                    if (!ReferenceEquals(manager.Layout.ActiveContent, document) || menu.IsOpen ||
                        menu.DataContext is not null || menu.PlacementTarget is not null || control.ContextMenu is not null)
                        throw new Exception("The packaged menu command did not activate its document and release invocation state.");
                    if (control is DropDownButton closedButton && closedButton.IsChecked != false)
                        throw new Exception("The packaged button remained checked after closing.");
                }
            }
            finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
        }
        VerifyOverrides(item, (IDataTemplate)resources.Resources["PackagedMenuIcon"]!);
    }

    private static void VerifyOverrides(LayoutItem item, IDataTemplate iconTemplate)
    {
        MenuProbe menu = new();
        menu.Items.Add("Generated");
        ButtonProbe button = new() { Content = "Actions", Width = 180, Height = 40, DropDownContextMenu = menu, DropDownContextMenuDataContext = item };
        Window window = new() { Content = button, Width = 400, Height = 300 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Click(button);
            if (button.MenuChanges != 1 || menu.Openings != 1 || menu.Containers != 1 || menu.ContainerFromIndex(0) is not IconProbe generated)
                throw new Exception("Packaged button and menu protected overrides were not invoked.");
            generated.DataContext = item;
            generated.IconTemplate = iconTemplate;
            object? icon = generated.Icon;
            generated.IconTemplateSelector = new DataTemplateSelector();
            if (generated.Templates != 1 || generated.Selectors != 1 || !ReferenceEquals(icon, generated.Icon))
                throw new Exception("Packaged icon overrides or null selector precedence were not preserved.");
            menu.Close();
            AreaProbe area = new()
            {
                Width = 180, Height = 40, Content = new Border { Background = Brushes.Transparent },
                DropDownContextMenu = menu, DropDownContextMenuDataContext = item
            };
            window.Content = area;
            Dispatcher.UIThread.RunJobs();
            Click(area, MouseButton.Right);
            if (area.Presses != 1 || area.Releases != 1 || menu.Openings != 2 || !menu.IsOpen)
                throw new Exception("Packaged area protected pointer hooks did not open the menu.");
            menu.Close();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static void Click(Control control, MouseButton button = MouseButton.Left)
    {
        TopLevel top = TopLevel.GetTopLevel(control)!;
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), top)!.Value;
        top.MouseMove(point);
        top.MouseDown(point, button);
        top.MouseUp(point, button);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class ButtonProbe : DropDownButton
    {
        public int MenuChanges { get; private set; }
        protected override void OnDropDownContextMenuChanged(AvaloniaPropertyChangedEventArgs e) { MenuChanges++; base.OnDropDownContextMenuChanged(e); }
    }
    private sealed class AreaProbe : DropDownControlArea
    {
        public int Presses { get; private set; }
        public int Releases { get; private set; }
        protected override void OnMouseRightButtonDown(PointerPressedEventArgs e) { Presses++; base.OnMouseRightButtonDown(e); }
        protected override void OnPreviewMouseRightButtonUp(PointerReleasedEventArgs e) { Releases++; base.OnPreviewMouseRightButtonUp(e); }
    }
    private sealed class MenuProbe : ContextMenuEx
    {
        public int Containers { get; private set; }
        public int Openings { get; private set; }
        protected override AvaloniaObject GetContainerForItemOverride() { Containers++; return new IconProbe(); }
        protected override void OnOpened(RoutedEventArgs e) { Openings++; base.OnOpened(e); }
    }
    private sealed class IconProbe : MenuItemEx
    {
        public int Templates { get; private set; }
        public int Selectors { get; private set; }
        protected override void OnIconTemplateChanged(AvaloniaPropertyChangedEventArgs e) { Templates++; base.OnIconTemplateChanged(e); }
        protected override void OnIconTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) { Selectors++; base.OnIconTemplateSelectorChanged(e); }
    }
}
